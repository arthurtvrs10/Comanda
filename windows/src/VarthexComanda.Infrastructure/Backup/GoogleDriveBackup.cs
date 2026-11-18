using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using VarthexComanda.Application.Backup;

namespace VarthexComanda.Infrastructure.Backup;

/// <summary>Uploads local snapshots. A folder is marked complete only after all companions arrive.</summary>
public sealed class GoogleDriveBackup(IGoogleDriveConnection connection, HttpClient http) : IGoogleDriveBackup
{
    public bool Conectado => connection.Conectada;
    public async Task EnviarAsync(string banco, string? fotos, IProgress<EtapaBackup>? progresso = null, CancellationToken cancellationToken = default)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limite.CancelAfter(TimeSpan.FromMinutes(10));
        var ct = limite.Token;
        var token = await connection.ObterAccessTokenAsync(ct).ConfigureAwait(false);
        async Task<JsonDocument> Api(HttpMethod method, string url, object? body = null)
        {
            using var pedido = new HttpRequestMessage(method, url);
            pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (body is not null) pedido.Content = JsonContent.Create(body);
            using var resposta = await http.SendAsync(pedido, ct).ConfigureAwait(false);
            ExigirSucesso(resposta);
            return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(ct).ConfigureAwait(false));
        }
        const string files = "https://www.googleapis.com/drive/v3/files";
        var query = Uri.EscapeDataString("trashed = false and mimeType = 'application/vnd.google-apps.folder' and appProperties has { key='varthex' and value='backups' }");
        using var pastas = await Api(HttpMethod.Get, files + "?spaces=drive&fields=files(id)&q=" + query).ConfigureAwait(false);
        var existentes = pastas.RootElement.GetProperty("files");
        string raiz;
        if (existentes.GetArrayLength() > 0) raiz = existentes[0].GetProperty("id").GetString()!;
        else
        {
            using var pasta = await Api(HttpMethod.Post, files, new { name = "Varthex Backups", mimeType = "application/vnd.google-apps.folder", appProperties = new { varthex = "backups" } }).ConfigureAwait(false);
            raiz = pasta.RootElement.GetProperty("id").GetString()!;
        }
        var nome = Path.GetFileNameWithoutExtension(banco);
        using var lote = await Api(HttpMethod.Post, files, new { name = nome + " (incompleto)", mimeType = "application/vnd.google-apps.folder", parents = new[] { raiz } }).ConfigureAwait(false);
        var destino = lote.RootElement.GetProperty("id").GetString()!;
        var arquivos = new List<string> { banco, banco + ".sha256" };
        if (fotos is not null) arquivos.Add(fotos);
        foreach (var caminho in arquivos)
        {
            ct.ThrowIfCancellationRequested();
            progresso?.Report(new(DateTime.UtcNow, "Enviando para o Google Drive: " + Path.GetFileName(caminho)));
            using var dados = File.OpenRead(caminho);
            using var inicio = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/upload/drive/v3/files?uploadType=resumable");
            inicio.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            inicio.Headers.TryAddWithoutValidation("X-Upload-Content-Type", "application/octet-stream");
            inicio.Headers.TryAddWithoutValidation("X-Upload-Content-Length", dados.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
            inicio.Content = JsonContent.Create(new { name = Path.GetFileName(caminho), parents = new[] { destino } });
            using var sessao = await http.SendAsync(inicio, ct).ConfigureAwait(false);
            ExigirSucesso(sessao);
            var uri = sessao.Headers.Location;
            if (uri is null || !uri.IsAbsoluteUri || uri.Scheme != "https" || uri.Host != "www.googleapis.com")
                throw new IOException("O Google Drive não retornou um endereço de envio válido.");
            using var envio = new HttpRequestMessage(HttpMethod.Put, uri);
            envio.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            envio.Content = new StreamContent(dados);
            envio.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            envio.Content.Headers.ContentLength = dados.Length;
            using var concluido = await http.SendAsync(envio, ct).ConfigureAwait(false);
            ExigirSucesso(concluido);
        }
        using var pronto = await Api(HttpMethod.Patch, files + "/" + Uri.EscapeDataString(destino), new { name = nome, appProperties = new { varthexBackup = "completo" } }).ConfigureAwait(false);
        progresso?.Report(new(DateTime.UtcNow, "Backup enviado ao Google Drive com sucesso. Pasta: Varthex Backups / " + nome));
    }
    private static void ExigirSucesso(HttpResponseMessage resposta)
    {
        if (resposta.IsSuccessStatusCode) return;
        throw new IOException(resposta.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden
            ? "O Google Drive recusou o acesso. Reconecte sua conta e verifique as permissões."
            : "O Google Drive não concluiu o envio. Verifique a conexão e o espaço disponível e tente novamente.");
    }
}
