using System.Net;
using System.Text;
using System.Text.Json;
using VarthexComanda.Application.Backup;
using VarthexComanda.Infrastructure.Backup;
using Xunit;

namespace VarthexComanda.Infrastructure.Tests.Backup;

public class GoogleDriveBackupTests
{
    [Fact]
    public void FalhaRemotaPreservaBackupLocalComChecksumEAviso()
    {
        using var banco=new VarthexComanda.Infrastructure.Tests.Homologacao.BancoDeHomologacao();
        banco.CriarProduto("Produto de teste",500);
        var servico=new EfBackupService(banco.Paths,new EfBackupRegistroRepository(banco.Fabrica),new VarthexComanda.Infrastructure.Time.SystemClock(),googleDrive:new DriveIndisponivel());
        var resultado=servico.CriarBackupGerenciado();
        Assert.True(resultado.Sucesso);Assert.Contains("backup local foi preservado",resultado.Valor!.Mensagem);
        var arquivo=Path.Combine(resultado.Valor.Destino,resultado.Valor.Arquivo);
        Assert.True(File.Exists(arquivo));Assert.True(File.Exists(arquivo+".sha256"));
    }
    private sealed class DriveIndisponivel:IGoogleDriveBackup
    {
        public bool Conectado=>true;
        public Task EnviarAsync(string banco,string? fotos,IProgress<EtapaBackup>? progresso=null,CancellationToken cancellationToken=default)=>Task.FromException(new IOException("Rede de teste indisponível"));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PublicaComoCompletoSomenteAposTodosArquivos(bool falhar)
    {
        var pasta=Path.Combine(Path.GetTempPath(),"varthex-drive-test-"+Guid.NewGuid());Directory.CreateDirectory(pasta);
        try
        {
            var banco=Path.Combine(pasta,"teste.db");await File.WriteAllTextAsync(banco,"dados");await File.WriteAllTextAsync(banco+".sha256","hash");
            var fotos=banco+".fotos.zip";await File.WriteAllTextAsync(fotos,"fotos");
            var handler=new DriveHandler(falhar);using var http=new HttpClient(handler);
            var servico=new GoogleDriveBackup(new Conexao(),http);
            if(falhar)await Assert.ThrowsAsync<IOException>(()=>servico.EnviarAsync(banco,fotos));
            else await servico.EnviarAsync(banco,fotos);
            Assert.Equal(!falhar,handler.Completo);Assert.Equal(falhar?1:3,handler.Enviados);
            Assert.True(File.Exists(banco));Assert.True(File.Exists(fotos));
        }
        finally { Directory.Delete(pasta,true); }
    }
    private sealed class Conexao : IGoogleDriveConnection
    {
        public bool Configurada=>true;public bool Conectada=>true;
        public void ImportarCredenciais(string arquivo)=>throw new NotSupportedException();
        public Task ConectarAsync(CancellationToken ct=default)=>Task.CompletedTask;
        public Task DesconectarAsync(CancellationToken ct=default)=>Task.CompletedTask;
        public Task<string> ObterAccessTokenAsync(CancellationToken ct=default)=>Task.FromResult("token-de-teste");
    }
    private sealed class DriveHandler(bool falhar) : HttpMessageHandler
    {
        public bool Completo;public int Enviados;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Assert.Equal("Bearer",request.Headers.Authorization?.Scheme);
            Assert.Equal("token-de-teste",request.Headers.Authorization?.Parameter);
            if(request.Method==HttpMethod.Get)return Json("{\"files\":[]}");
            if(request.Method==HttpMethod.Put)
            {
                Enviados++;Assert.NotEmpty(await request.Content!.ReadAsByteArrayAsync(ct));
                return Json("{}",falhar?HttpStatusCode.ServiceUnavailable:HttpStatusCode.OK);
            }
            using var data=JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            if(request.Method==HttpMethod.Patch){Completo=true;Assert.Equal("completo",data.RootElement.GetProperty("appProperties").GetProperty("varthexBackup").GetString());return Json("{}");}
            if(request.RequestUri!.AbsolutePath.Contains("/upload/"))
            {
                Assert.Equal("lote",data.RootElement.GetProperty("parents")[0].GetString());
                var result=Json("{}");result.Headers.Location=new Uri("https://www.googleapis.com/upload/session-test");return result;
            }
            var raiz=data.RootElement.TryGetProperty("appProperties",out _);
            if(!raiz)Assert.EndsWith("(incompleto)",data.RootElement.GetProperty("name").GetString());
            return Json(raiz?"{\"id\":\"raiz\"}":"{\"id\":\"lote\"}");
        }
        private static HttpResponseMessage Json(string texto,HttpStatusCode status=HttpStatusCode.OK)=>new(status){Content=new StringContent(texto,Encoding.UTF8,"application/json")};
    }
}
