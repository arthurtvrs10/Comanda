using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

namespace VarthexComanda.Desktop.RecuperacaoEmail;

public sealed class ClienteRecuperacaoEmail
{
    private readonly AcessoGestor _gestor;
    private readonly string _arquivo;
    private readonly ConfiguracaoRecuperacaoEmail _config;
    private readonly HttpClient _http;
    private Estado? _estado;
    private Pendente? _pendente;
    private static readonly HttpClient Http=new(new HttpClientHandler {AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(20)};
    internal sealed record Estado(string InstallationId,string Secret,string? MaskedEmail=null);
    internal sealed record Pendente(string ChallengeId,string Nonce,string CredentialHash,string Purpose,DateTimeOffset ReenviarApos);
    public sealed record Desafio(string ChallengeId,string MaskedEmail,int ExpiresIn,int RetryAfter);
    private sealed record Resposta(string? Grant=null,string? MaskedEmail=null,bool Verified=false);
    public string? EmailMascarado=>_estado?.MaskedEmail;
    public bool Verificado=>!string.IsNullOrEmpty(EmailMascarado);
    public TimeSpan EsperaReenvio=>_pendente is {} p&&p.ReenviarApos>DateTimeOffset.UtcNow?p.ReenviarApos-DateTimeOffset.UtcNow:TimeSpan.Zero;

    internal ClienteRecuperacaoEmail(AcessoGestor gestor,string arquivo,ConfiguracaoRecuperacaoEmail config,HttpClient? http=null)
    {
        config.Validar();_gestor=gestor;_arquivo=arquivo;_config=config;_http=http??Http;
        if(File.Exists(arquivo))
        {
            try
            {
                if(new FileInfo(arquivo).Length>16384)throw new JsonException();
                _estado=JsonSerializer.Deserialize<Estado>(ProtectedData.Unprotect(File.ReadAllBytes(arquivo),null,DataProtectionScope.CurrentUser));
            }
            catch(Exception ex) when(ex is CryptographicException or JsonException){throw new InvalidOperationException("O vínculo de recuperação pertence a outro usuário ou computador. Entre com a senha offline e configure o e-mail novamente com o suporte.");}
            catch(Exception ex) when(ex is IOException or UnauthorizedAccessException){throw new InvalidOperationException("Não foi possível ler o vínculo de recuperação. Verifique as permissões da pasta de dados ou use a recuperação offline.");}
            if(_estado is null||_estado.InstallationId?.Length!=32||_estado.Secret?.Length!=64)throw new InvalidOperationException("Vínculo de recuperação inválido. Contate o suporte.");
        }
    }
    public async Task<Desafio> SolicitarCadastroAsync(string senhaAtual,string email,CancellationToken ct=default)
    {
        if(!_gestor.Autenticado||_gestor.AutenticadoPorColaborador||!_gestor.ConferirSenha(senhaAtual))
            throw new InvalidOperationException("Confirme a senha atual do gestor para configurar o e-mail.");
        email=email.Trim();
        if(email.Length>254||!System.Net.Mail.MailAddress.TryCreate(email,out var endereco)||endereco.Address!=email||!System.Text.RegularExpressions.Regex.IsMatch(email,@"^[^\s@<>]+@[^\s@<>]+\.[^\s@<>]+$"))
            throw new ArgumentException("Informe um e-mail válido.");
        if(_estado is null){var novo=new Estado(Guid.NewGuid().ToString("N"),Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32)));SalvarEstado(novo);_estado=novo;}
        return await SolicitarAsync("enroll",email,ct);
    }
    public Task<Desafio> SolicitarRecuperacaoAsync(CancellationToken ct=default)
    {
        if(!Verificado)throw new InvalidOperationException("Não há e-mail verificado nesta instalação. Use sua senha de recuperação offline.");
        return SolicitarAsync("reset",null,ct);
    }
    private async Task<Desafio> SolicitarAsync(string purpose,string? email,CancellationToken ct)
    {
        if(EsperaReenvio>TimeSpan.Zero)throw new InvalidOperationException($"Aguarde {(int)Math.Ceiling(EsperaReenvio.TotalSeconds)} segundos antes de reenviar.");
        var nonce=Guid.NewGuid().ToString("N");var digest=_gestor.ImpressaoCredencial;
        // A requisição anterior deixa de ser utilizável também no cliente.
        _pendente=null;
        var resposta=await PostAsync<Desafio>($"v1/{purpose}/request",new {installationId=_estado!.InstallationId,email,nonce,credentialHash=digest},ct);
        _pendente=new(resposta.ChallengeId,nonce,digest,purpose,DateTimeOffset.UtcNow.AddSeconds(Math.Max(60,resposta.RetryAfter)));
        return resposta;
    }
    public async Task ConfirmarCadastroAsync(string codigo,CancellationToken ct=default)
    {
        var p=ExigirPendente("enroll",codigo);
        if(!_gestor.Autenticado||_gestor.AutenticadoPorColaborador)throw new InvalidOperationException("Entre como gestor para verificar o e-mail.");
        var resposta=await PostAsync<Resposta>("v1/enroll/confirm",new {installationId=_estado!.InstallationId,challengeId=p.ChallengeId,code=codigo},ct);
        if(!resposta.Verified||string.IsNullOrWhiteSpace(resposta.MaskedEmail))throw new InvalidOperationException("O serviço não confirmou o e-mail. Tente novamente.");
        var novo=_estado with {MaskedEmail=resposta.MaskedEmail};SalvarEstado(novo);_estado=novo;_pendente=null;
    }
    public async Task<string> ConfirmarCodigoAsync(string codigo,CancellationToken ct=default)
    {
        var p=ExigirPendente("reset",codigo);
        var resposta=await PostAsync<Resposta>("v1/reset/confirm",new {installationId=_estado!.InstallationId,challengeId=p.ChallengeId,code=codigo},ct);
        if(string.IsNullOrEmpty(resposta.Grant))throw new InvalidOperationException("Autorização de recuperação inválida.");
        _gestor.ValidarAutorizacaoEmail(resposta.Grant,_config.ChavePublicaPem,_estado.InstallationId,p.ChallengeId,p.Nonce);
        return resposta.Grant;
    }
    public void RedefinirSenha(string autorizacao,string novaSenha)
    {
        var p=_pendente??throw new InvalidOperationException("Solicite outro código.");
        if(p.Purpose!="reset")throw new InvalidOperationException("Autorização de recuperação inválida.");
        _gestor.RecuperarSenhaPorEmail(autorizacao,novaSenha,_config.ChavePublicaPem,_estado!.InstallationId,p.ChallengeId,p.Nonce);
        _pendente=null;
    }
    private Pendente ExigirPendente(string purpose,string code)
    {
        if(code.Length!=6||!code.All(char.IsAsciiDigit))throw new ArgumentException("Informe os seis números do código.");
        if(_pendente is not {} p||p.Purpose!=purpose||p.CredentialHash!=_gestor.ImpressaoCredencial)
            throw new InvalidOperationException("O acesso foi alterado. Solicite outro código.");
        return p;
    }
    private async Task<T> PostAsync<T>(string path,object body,CancellationToken ct)
    {
        try
        {
            using var request=new HttpRequestMessage(HttpMethod.Post,new Uri(new Uri(_config.ApiUrl.TrimEnd('/')+"/"),path));
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",_estado!.Secret);
            request.Content=JsonContent.Create(body);
            using var response=await _http.SendAsync(request,ct);
            if(!response.IsSuccessStatusCode)
            {
                var mensagem=(int)response.StatusCode switch {429=>"Limite de solicitações atingido. Aguarde e tente novamente; se persistir, use a recuperação offline.",400 when path.EndsWith("request")=>"Não foi possível solicitar o código. Confira o e-mail e o vínculo desta instalação nas configurações de Segurança.",400=>"Código inválido, expirado ou com tentativas esgotadas. Confira os números ou solicite outro código.",401=>"Não foi possível reconhecer esta instalação. Use a recuperação offline e contate o suporte.",_=>"Recuperação por e-mail indisponível. Tente novamente ou use sua senha de recuperação offline."};
                throw new InvalidOperationException(mensagem);
            }
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken:ct)??throw new InvalidOperationException("Resposta inválida do serviço de recuperação.");
        }
        catch(HttpRequestException){throw new InvalidOperationException("Não foi possível conectar. Verifique sua internet e tente novamente.");}
        catch(TaskCanceledException) when(!ct.IsCancellationRequested){throw new InvalidOperationException("O serviço demorou para responder. Tente novamente.");}
        catch(JsonException){throw new InvalidOperationException("Resposta inválida do serviço de recuperação.");}
    }
    private void SalvarEstado(Estado estado)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_arquivo)!);
        var temp=_arquivo+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{File.WriteAllBytes(temp,ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(estado),null,DataProtectionScope.CurrentUser));File.Move(temp,_arquivo,true);}
        finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
