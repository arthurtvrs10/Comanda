using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Desktop.Tests.Backup;

public class GoogleDriveConnectionTests
{
    [Fact]
    public void PkceCorrespondeAoVetorRfc7636() => Assert.Equal("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM",GoogleDriveConnection.DesafioPkce("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"));
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RenovaTokenProtegidoOuExigeReconexaoQuandoRevogado(bool revogado)
    {
        var raiz=Path.Combine(Path.GetTempPath(),"varthex-oauth-test-"+Guid.NewGuid());Directory.CreateDirectory(raiz);
        try
        {
            var paths=new AppPaths(raiz);var handler=new TokenHandler(revogado);using var http=new HttpClient(handler);
            var drive=new GoogleDriveConnection(paths,http);
            Assert.False(drive.Configurada);Assert.False(drive.Conectada);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>drive.ObterAccessTokenAsync());
            var origem=Path.Combine(raiz,"cliente-teste.json");
            await File.WriteAllTextAsync(origem,"{\"installed\":{\"client_id\":\"teste.apps.googleusercontent.com\",\"client_secret\":\"valor-de-teste\"}}");
            drive.ImportarCredenciais(origem);Assert.True(drive.Configurada);
            var arquivo=Path.Combine(raiz,"google-drive-token.bin");
            var json=JsonSerializer.SerializeToUtf8Bytes(new { AccessToken="expirado",RefreshToken="renovacao-de-teste",Expira=DateTimeOffset.UtcNow.AddHours(-1) });
            await File.WriteAllBytesAsync(arquivo,ProtectedData.Protect(json,null,DataProtectionScope.CurrentUser));
            Assert.True(drive.Conectada);
            if(revogado)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(()=>drive.ObterAccessTokenAsync());Assert.False(drive.Conectada);Assert.False(File.Exists(arquivo));
            }
            else
            {
                Assert.Equal("novo-acesso-de-teste",await drive.ObterAccessTokenAsync());
                Assert.Equal("novo-acesso-de-teste",await drive.ObterAccessTokenAsync());Assert.Equal(1,handler.Renovacoes);
                Assert.DoesNotContain("novo-acesso-de-teste",Encoding.UTF8.GetString(await File.ReadAllBytesAsync(arquivo)));
                await drive.DesconectarAsync();Assert.False(drive.Conectada);Assert.True(handler.Revogou);
            }
        }
        finally { Directory.Delete(raiz,true); }
    }
    private sealed class TokenHandler(bool revogado):HttpMessageHandler
    {
        public int Renovacoes;public bool Revogou;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct)
        {
            Assert.Equal("oauth2.googleapis.com",r.RequestUri!.Host);
            if(r.RequestUri.AbsolutePath=="/revoke"){Revogou=true;return new(HttpStatusCode.OK){Content=new StringContent("")};}
            Assert.Contains("grant_type=refresh_token",await r.Content!.ReadAsStringAsync(ct));Renovacoes++;
            return new(revogado?HttpStatusCode.BadRequest:HttpStatusCode.OK){Content=new StringContent(revogado?"{\"error\":\"invalid_grant\"}":"{\"access_token\":\"novo-acesso-de-teste\",\"expires_in\":3600}")};
        }
    }
}
