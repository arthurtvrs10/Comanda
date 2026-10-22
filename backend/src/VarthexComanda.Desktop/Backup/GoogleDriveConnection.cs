using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VarthexComanda.Application.Backup;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Desktop.Backup;

public sealed class GoogleDriveConnection(AppPaths paths, HttpClient http) : IGoogleDriveConnection
{
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private string Credenciais => Path.Combine(paths.Root, "google-oauth-client.json");
    private string Tokens => Path.Combine(paths.Root, "google-drive-token.bin");
    public bool Configurada { get { try { LerCliente(); return true; } catch { return false; } } }
    public bool Conectada { get { try { return !string.IsNullOrWhiteSpace(LerToken()?.RefreshToken); } catch { return false; } } }
    private sealed record Cliente(string Id, string Secret);
    private sealed record Token(string AccessToken, string RefreshToken, DateTimeOffset Expira);
    private Cliente LerCliente(string? arquivo = null)
    {
        using var json = JsonDocument.Parse(File.ReadAllText(arquivo ?? Credenciais));
        if (!json.RootElement.TryGetProperty("installed", out var app)) throw new InvalidDataException("Use as credenciais do tipo Aplicativo para computador.");
        var id = app.GetProperty("client_id").GetString();
        if (string.IsNullOrWhiteSpace(id) || !id.EndsWith(".apps.googleusercontent.com", StringComparison.Ordinal))
            throw new InvalidDataException("O arquivo não contém um cliente Google válido.");
        return new(id, app.TryGetProperty("client_secret", out var s) ? s.GetString() ?? "" : "");
    }
    public void ImportarCredenciais(string arquivo)
    {
        if (!_mutex.Wait(0)) throw new InvalidOperationException("Aguarde a conexão em andamento.");
        try
        {
            LerCliente(arquivo);
            if (File.Exists(Tokens)) throw new InvalidOperationException("Desconecte a conta antes de alterar as credenciais.");
            Directory.CreateDirectory(paths.Root);
            File.Copy(arquivo, Credenciais, true);
        }
        finally { _mutex.Release(); }
    }
    private Token? LerToken() => !File.Exists(Tokens) ? null : JsonSerializer.Deserialize<Token>(ProtectedData.Unprotect(File.ReadAllBytes(Tokens), null, DataProtectionScope.CurrentUser));
    private void Salvar(Token token)
    {
        Directory.CreateDirectory(paths.Root);
        var temporario = Tokens + ".tmp";
        try
        {
            File.WriteAllBytes(temporario, ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(token), null, DataProtectionScope.CurrentUser));
            File.Move(temporario, Tokens, true);
        }
        finally { if (File.Exists(temporario)) File.Delete(temporario); }
    }
    public static string DesafioPkce(string verifier) => Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
    public async Task ConectarAsync(CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            Cliente cliente;
            try { cliente = LerCliente(); }
            catch { throw new InvalidOperationException("A conexão com o Google ainda não foi configurada nesta instalação. Solicite a configuração ao responsável pelo sistema."); }
            using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            limite.CancelAfter(TimeSpan.FromMinutes(3));
            var ct = limite.Token;
            var verifier = Base64Url(RandomNumberGenerator.GetBytes(48));
            var state = Base64Url(RandomNumberGenerator.GetBytes(32));
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var redirect = "http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port + "/oauth2callback";
            var parametros = new Dictionary<string,string> { ["client_id"]=cliente.Id,["redirect_uri"]=redirect,["response_type"]="code",["scope"]="https://www.googleapis.com/auth/drive.file",["access_type"]="offline",["prompt"]="consent",["state"]=state,["code_challenge"]=DesafioPkce(verifier),["code_challenge_method"]="S256" };
            var url = "https://accounts.google.com/o/oauth2/v2/auth?" + string.Join("&",parametros.Select(p=>Uri.EscapeDataString(p.Key)+"="+Uri.EscapeDataString(p.Value)));
            Process.Start(new ProcessStartInfo(url) { UseShellExecute=true });
            using var clienteLocal = await listener.AcceptTcpClientAsync(ct);
            await using var stream = clienteLocal.GetStream();
            using var leitor = new StreamReader(stream, Encoding.ASCII, false, 1024, true);
            var linha = new StringBuilder();
            var caractere = new char[1];
            while (linha.Length < 8192)
            {
                if (await leitor.ReadAsync(caractere.AsMemory(), ct) == 0) break;
                if (caractere[0] == '\n') break;
                linha.Append(caractere[0]);
            }
            var partes = linha.ToString().TrimEnd('\r').Split(' ');
            if (partes.Length != 3 || partes[0] != "GET" || !Uri.TryCreate(redirect[..redirect.LastIndexOf('/')] + partes[1], UriKind.Absolute, out var callback) || callback.AbsolutePath != "/oauth2callback")
                throw new InvalidOperationException("Resposta de autorização inválida. Tente conectar novamente.");
            var query = callback.Query.TrimStart('?').Split('&').Select(p=>p.Split('=',2)).Where(p=>p.Length==2).ToDictionary(p=>Uri.UnescapeDataString(p[0]),p=>Uri.UnescapeDataString(p[1]));
            var valido = query.GetValueOrDefault("state") == state && query.TryGetValue("code", out var codigo) && !query.ContainsKey("error");
            var corpo = Encoding.UTF8.GetBytes(valido ? "Autorizacao recebida. Volte ao Varthex para concluir a conexao." : "Autorizacao nao concluida. Volte ao Varthex e tente novamente.");
            var cabecalho = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=utf-8\r\nCache-Control: no-store\r\nConnection: close\r\nContent-Length: " + corpo.Length + "\r\n\r\n");
            await stream.WriteAsync(cabecalho,ct);await stream.WriteAsync(corpo,ct);
            if (!valido) throw new InvalidOperationException("Autorização cancelada ou inválida. Nenhuma conta foi conectada.");
            var campos = new Dictionary<string,string> { ["client_id"]=cliente.Id,["client_secret"]=cliente.Secret,["code"]=query["code"],["code_verifier"]=verifier,["redirect_uri"]=redirect,["grant_type"]="authorization_code" };
            Salvar(await TrocarToken(campos, null, ct));
        }
        finally { _mutex.Release(); }
    }
    public async Task<string> ObterAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Token? token;
            try { token=LerToken(); } catch { throw new InvalidOperationException("Reconecte sua conta Google neste usuário do Windows."); }
            if (token is null) throw new InvalidOperationException("Conecte sua conta Google antes de enviar backups.");
            if (token.Expira > DateTimeOffset.UtcNow.AddMinutes(1)) return token.AccessToken;
            var cliente=LerCliente();
            var novo=await TrocarToken(new() { ["client_id"]=cliente.Id,["client_secret"]=cliente.Secret,["refresh_token"]=token.RefreshToken,["grant_type"]="refresh_token" },token.RefreshToken,cancellationToken).ConfigureAwait(false);
            Salvar(novo);return novo.AccessToken;
        }
        finally { _mutex.Release(); }
    }
    private async Task<Token> TrocarToken(Dictionary<string,string> campos, string? refresh, CancellationToken ct)
    {
        using var resposta=await http.PostAsync("https://oauth2.googleapis.com/token",new FormUrlEncodedContent(campos),ct).ConfigureAwait(false);
        if (!resposta.IsSuccessStatusCode)
        {
            if (refresh is not null && resposta.StatusCode == HttpStatusCode.BadRequest)
            {
                using var erro=JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(ct).ConfigureAwait(false));
                if (erro.RootElement.TryGetProperty("error",out var motivo) && motivo.GetString()=="invalid_grant" && File.Exists(Tokens)) File.Delete(Tokens);
            }
            throw new InvalidOperationException("O Google não autorizou a conexão. Verifique a configuração e conecte novamente.");
        }
        using var json=JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(ct).ConfigureAwait(false));var raiz=json.RootElement;
        var acesso=raiz.GetProperty("access_token").GetString();
        var renovacao=raiz.TryGetProperty("refresh_token",out var r)?r.GetString():refresh;
        if(string.IsNullOrWhiteSpace(acesso)||string.IsNullOrWhiteSpace(renovacao))throw new InvalidOperationException("O Google não permitiu o backup automático. Conecte novamente e autorize o acesso.");
        if (raiz.TryGetProperty("scope",out var escopo) && !(escopo.GetString() ?? "").Split(' ').Contains("https://www.googleapis.com/auth/drive.file"))
            throw new InvalidOperationException("Autorize o acesso aos arquivos do Varthex para salvar os backups.");
        return new(acesso,renovacao,DateTimeOffset.UtcNow.AddSeconds(raiz.GetProperty("expires_in").GetInt32()));
    }
    public async Task DesconectarAsync(CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            var token=LerToken();
            if(token is not null)
            {
                using var resposta=await http.PostAsync("https://oauth2.googleapis.com/revoke",new FormUrlEncodedContent(new Dictionary<string,string> { ["token"]=token.RefreshToken }),cancellationToken);
                if(!resposta.IsSuccessStatusCode)throw new IOException("Não foi possível revogar o acesso no Google. Verifique a conexão e tente novamente.");
            }
            if(File.Exists(Tokens))File.Delete(Tokens);
        }
        finally { _mutex.Release(); }
    }
}
