using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VarthexComanda.Desktop.RecuperacaoEmail;

namespace VarthexComanda.Desktop.Tests;

public sealed class RecuperacaoEmailTests
{
    private sealed class Fixture : IDisposable
    {
        public string Folder {get;}=Path.Combine(Path.GetTempPath(),"recuperacao-email-"+Guid.NewGuid());
        public string Credencial=>Path.Combine(Folder,"gestor.credencial");
        public string Estado=>Credencial+".email";
        public AcessoGestor Gestor {get;}
        public RSA Rsa {get;}=RSA.Create(2048);
        public ConfiguracaoRecuperacaoEmail Config=>new("https://recuperacao.example.com",Rsa.ExportSubjectPublicKeyInfoPem());
        public Fixture(){Gestor=new(Credencial);Gestor.CadastrarComSenhaRecuperacao("antiga","offline");}
        public string Grant(string install="install",string challenge="challenge",string nonce="nonce",string? digest=null,long? issued=null,long? expires=null)
        {
            var now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Sign(new {aud="vcomanda-gestor-reset",installationId=install,challengeId=challenge,nonce,credentialHash=digest??Gestor.ImpressaoCredencial,iat=issued??now,exp=expires??now+600,jti=Guid.NewGuid()});
        }
        public string Sign(object claims)
        {
            static string B(byte[] bytes)=>Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
            var payload="v1."+B(JsonSerializer.SerializeToUtf8Bytes(claims));
            return payload+"."+B(Rsa.SignData(Encoding.UTF8.GetBytes(payload),HashAlgorithmName.SHA256,RSASignaturePadding.Pss));
        }
        public void Dispose(){Rsa.Dispose();Directory.Delete(Folder,true);}
    }
    private sealed class Handler(Func<HttpRequestMessage,Task<HttpResponseMessage>> action):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)=>action(request);}
    private static HttpResponseMessage Reply(object body,HttpStatusCode status=HttpStatusCode.OK)=>new(status){Content=JsonContent.Create(body)};

    [Fact] public void AutorizacaoValidaRedefineLocalmenteMantemOfflineEImpedeReutilizacao()
    {
        using var f=new Fixture();var token=f.Grant();var before=File.ReadAllBytes(f.Credencial);
        f.Gestor.RecuperarSenhaPorEmail(token,"nova",f.Config.ChavePublicaPem,"install","challenge","nonce");
        Assert.False(f.Gestor.Autenticado);Assert.False(f.Gestor.Entrar("antiga"));Assert.True(f.Gestor.Entrar("nova"));
        Assert.Equal(before[64..],File.ReadAllBytes(f.Credencial)[64..]);
        Assert.Throws<InvalidOperationException>(()=>f.Gestor.RecuperarSenhaPorEmail(token,"outra",f.Config.ChavePublicaPem,"install","challenge","nonce"));
        f.Gestor.RecuperarSenha("offline","final");Assert.True(f.Gestor.Entrar("final"));
    }
    [Fact] public void AssinaturaGeradaPeloWebCryptoDoWorkerEValidadaPeloDotNet()
    {
        using var f=new Fixture();using var stream=typeof(RecuperacaoEmailTests).Assembly.GetManifestResourceStream("VarthexComanda.Desktop.Tests.Fixtures.recuperacao-webcrypto.json")!;
        using var doc=JsonDocument.Parse(stream);var v=doc.RootElement;var c=v.GetProperty("claims");
        File.WriteAllBytes(f.Credencial,Convert.FromBase64String(v.GetProperty("credential").GetString()!));
        f.Gestor.ValidarAutorizacaoEmail(v.GetProperty("grant").GetString()!,v.GetProperty("publicKey").GetString()!,c.GetProperty("installationId").GetString()!,c.GetProperty("challengeId").GetString()!,c.GetProperty("nonce").GetString()!,DateTimeOffset.FromUnixTimeSeconds(c.GetProperty("iat").GetInt64()+1));
    }
    [Theory]
    [InlineData("installation")][InlineData("challenge")][InlineData("nonce")][InlineData("credential")][InlineData("expired")][InlineData("future")][InlineData("long")][InlineData("tampered")][InlineData("other-key")]
    public void RejeitaAutorizacaoForaDoContextoSemAlterarCredencial(string scenario)
    {
        using var f=new Fixture();var before=File.ReadAllBytes(f.Credencial);var now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var token=f.Grant(scenario=="installation"?"other":"install",scenario=="challenge"?"other":"challenge",scenario=="nonce"?"other":"nonce",scenario=="credential"?"wrong":null,
            scenario=="expired"?now-700:scenario=="future"?now+60:null,scenario=="expired"?now-100:scenario=="long"?now+601:null);
        if(scenario=="tampered")token=token[..^5]+"AAAAA";
        using var other=RSA.Create(2048);var key=scenario=="other-key"?other.ExportSubjectPublicKeyInfoPem():f.Config.ChavePublicaPem;
        Assert.Throws<InvalidOperationException>(()=>f.Gestor.RecuperarSenhaPorEmail(token,"nova",key,"install","challenge","nonce"));
        Assert.Equal(before,File.ReadAllBytes(f.Credencial));
    }
    [Theory][InlineData("x")][InlineData("offline")]
    public void NovaSenhaInvalidaNaoConsomeAutorizacao(string password)
    {
        using var f=new Fixture();var t=f.Grant();
        Assert.Throws<ArgumentException>(()=>f.Gestor.RecuperarSenhaPorEmail(t,password,f.Config.ChavePublicaPem,"install","challenge","nonce"));
        f.Gestor.RecuperarSenhaPorEmail(t,"nova",f.Config.ChavePublicaPem,"install","challenge","nonce");Assert.True(f.Gestor.Entrar("nova"));
    }
    [Fact] public async Task CadastroExigeSenhaAtualNaoEnviaSenhaEPersisteVinculoProtegido()
    {
        using var f=new Fixture();var calls=0;
        using var http=new HttpClient(new Handler(async request=>
        {
            calls++;var text=await request.Content!.ReadAsStringAsync();Assert.DoesNotContain("antiga",text);Assert.DoesNotContain("offline",text);
            Assert.Equal("Bearer",request.Headers.Authorization!.Scheme);
            return request.RequestUri!.AbsolutePath.EndsWith("confirm")?Reply(new{verified=true,maskedEmail="ge***@example.com"}):Reply(new{challengeId=new string('a',32),maskedEmail="ge***@example.com",expiresIn=600,retryAfter=60});
        }));
        var client=new ClienteRecuperacaoEmail(f.Gestor,f.Estado,f.Config,http);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.SolicitarCadastroAsync("errada","gestor@example.com"));Assert.Equal(0,calls);
        f.Gestor.Sair();await Assert.ThrowsAsync<InvalidOperationException>(()=>client.SolicitarCadastroAsync("antiga","gestor@example.com"));Assert.Equal(0,calls);
        f.Gestor.Entrar("antiga");await client.SolicitarCadastroAsync("antiga","gestor@example.com");Assert.False(client.Verificado);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.SolicitarCadastroAsync("antiga","outro@example.com"));
        await client.ConfirmarCadastroAsync("123456");Assert.True(client.Verificado);
        var encrypted=File.ReadAllBytes(f.Estado);Assert.DoesNotContain("ge***",Encoding.UTF8.GetString(encrypted));
        var restored=new ClienteRecuperacaoEmail(f.Gestor,f.Estado,f.Config,http);Assert.Equal(client.EmailMascarado,restored.EmailMascarado);
        Assert.Equal(2,calls);
    }
    [Fact] public async Task ClienteIntegraCadastroCodigoEAplicacaoDaSenhaSemEnviaLa()
    {
        using var f=new Fixture();string install="",nonce="",digest="",challenge=new string('a',32);var bodies=new List<string>();
        using var http=new HttpClient(new Handler(async r=>
        {
            var text=await r.Content!.ReadAsStringAsync();bodies.Add(text);using var doc=JsonDocument.Parse(text);var b=doc.RootElement;
            install=b.GetProperty("installationId").GetString()!;
            if(r.RequestUri!.AbsolutePath.EndsWith("request")){nonce=b.GetProperty("nonce").GetString()!;digest=b.GetProperty("credentialHash").GetString()!;return Reply(new {challengeId=challenge,maskedEmail="ge***@example.com",expiresIn=600,retryAfter=60});}
            if(r.RequestUri.AbsolutePath.Contains("enroll"))return Reply(new{verified=true,maskedEmail="ge***@example.com"});
            return Reply(new{grant=f.Grant(install,challenge,nonce,digest)});
        }));
        var client=new ClienteRecuperacaoEmail(f.Gestor,f.Estado,f.Config,http);
        await client.SolicitarCadastroAsync("antiga","gestor@example.com");await client.ConfirmarCadastroAsync("123456");
        f.Gestor.Sair();await client.SolicitarRecuperacaoAsync();
        var grant=await client.ConfirmarCodigoAsync("123456");client.RedefinirSenha(grant,"senha-nova");
        Assert.True(f.Gestor.Entrar("senha-nova"));Assert.DoesNotContain(bodies,b=>b.Contains("senha-nova"));
        Assert.Throws<InvalidOperationException>(()=>client.RedefinirSenha(grant,"outra"));
    }
    [Fact] public async Task AlteracaoDaCredencialInvalidaCadastroPendente()
    {
        using var f=new Fixture();using var http=new HttpClient(new Handler(_=>Task.FromResult(Reply(new{challengeId=new string('a',32),maskedEmail="ge***@example.com",expiresIn=600,retryAfter=60}))));
        var client=new ClienteRecuperacaoEmail(f.Gestor,f.Estado,f.Config,http);
        await client.SolicitarCadastroAsync("antiga","gestor@example.com");f.Gestor.AlterarSenha("antiga","nova");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.ConfirmarCadastroAsync("123456"));Assert.False(client.Verificado);
    }
    [Fact] public async Task FalhaDeConexaoMantemCredencialERecuperacaoOffline()
    {
        using var f=new Fixture();var before=File.ReadAllBytes(f.Credencial);
        using var http=new HttpClient(new Handler(_=>throw new HttpRequestException("detalhe interno")));
        var client=new ClienteRecuperacaoEmail(f.Gestor,f.Estado,f.Config,http);
        var e=await Assert.ThrowsAsync<InvalidOperationException>(()=>client.SolicitarCadastroAsync("antiga","gestor@example.com"));
        Assert.Contains("internet",e.Message);Assert.DoesNotContain("detalhe interno",e.Message);Assert.Equal(before,File.ReadAllBytes(f.Credencial));Assert.True(f.Gestor.RecuperacaoConfigurada);
    }
    [Theory][InlineData("http://example.com")][InlineData("https://usuario:senha@example.com")][InlineData("https://example.com?token=x")]
    public void ConfiguracaoExigeHttpsSemCredencialNaUrl(string url)
    {
        using var f=new Fixture();Assert.Throws<ArgumentException>(()=>new ConfiguracaoRecuperacaoEmail(url,f.Config.ChavePublicaPem).Validar());
    }
    [Theory][InlineData("corrompido")][InlineData("nulo")][InlineData("grande")]
    public void VinculoDanificadoRetornaOrientacaoSemAlterarSenha(string caso)
    {
        using var f=new Fixture();var before=File.ReadAllBytes(f.Credencial);
        var bytes=caso switch {"nulo"=>ProtectedData.Protect(Encoding.UTF8.GetBytes("null"),null,DataProtectionScope.CurrentUser),"grande"=>new byte[17000],_=>new byte[64]};
        File.WriteAllBytes(f.Estado,bytes);
        Assert.Throws<InvalidOperationException>(()=>new ClienteRecuperacaoEmail(f.Gestor,f.Estado,f.Config));
        Assert.Equal(before,File.ReadAllBytes(f.Credencial));Assert.True(f.Gestor.RecuperacaoConfigurada);
    }
}
