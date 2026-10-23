using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Servidor.Tests;

public class ServidorLocalTests
{
    [Fact]
    public async Task ExigeChaveNoCabecalhoERevogaAoReiniciar()
    {
        await using var e=await Ensaio.Criar();
        Assert.Equal($"http://127.0.0.1:{e.Servidor.Porta}",Assert.Single(e.Servidor.Enderecos));
        using var anonimo=new HttpClient { BaseAddress=e.Cliente.BaseAddress };
        Assert.Equal(HttpStatusCode.Unauthorized,(await anonimo.GetAsync("/api/comandas")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await anonimo.GetAsync("/api/comandas?chave="+e.Chave)).StatusCode);
        Assert.Contains("Atendimento pelo celular",await anonimo.GetStringAsync("/"));
        var antiga=e.Chave;
        await e.Servidor.PararAsync();Assert.False(e.Servidor.Ativo);Assert.Empty(e.Servidor.Enderecos);
        await e.Servidor.IniciarAsync(0,true);e.Conectar();Assert.NotEqual(antiga,e.Chave);
        e.Cliente.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",antiga);
        Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.GetAsync("/api/comandas")).StatusCode);
    }
    [Fact]
    public async Task ComandaEItemPersistemUmaVezAoRepetirOuConcorrer()
    {
        await using var e=await Ensaio.Criar();
        var identificacao=Guid.NewGuid();
        var a=await e.Enviar("/api/comandas",new {numero=10},identificacao);Assert.Equal(HttpStatusCode.Created,a.StatusCode);
        var id=(await a.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.Created,(await e.Enviar("/api/comandas",new {numero=10},identificacao)).StatusCode);
        Assert.Single(e.Repositorio.ListarAbertas());
        Assert.Equal(HttpStatusCode.Conflict,(await e.Enviar("/api/comandas",new {numero=11},identificacao)).StatusCode);
        var item=new {produtoId=e.ProdutoId,quantidade=2,observacao="Sem cebola <script>",operador="Ana"};var chave=Guid.NewGuid();
        var respostas=await Task.WhenAll(Enumerable.Range(0,8).Select(_=>e.Enviar($"/api/comandas/{id}/itens",item,chave)));
        Assert.All(respostas,r=>Assert.Equal(HttpStatusCode.OK,r.StatusCode));
        var estado=e.Repositorio.BuscarComItens(id)!;Assert.Equal(1500,estado.Comanda.TotalCentavos);Assert.Equal(2,estado.Itens.Sum(i=>i.Quantidade));
        Assert.All(estado.Itens,i=>{Assert.Equal("Ana",i.LancadoPor);Assert.Equal("Sem cebola <script>",i.Observacao);});
        var leitura=await e.Cliente.GetFromJsonAsync<JsonElement>($"/api/comandas/{id}");
        Assert.Equal(1500,leitura.GetProperty("totalCentavos").GetInt64());Assert.False(leitura.TryGetProperty("pagamentosJson",out _));
    }
    [Fact]
    public async Task PreservaFracoesSnapshotsEGuardaDeFechamento()
    {
        await using var e=await Ensaio.Criar();var c=e.Repositorio.AbrirComanda(12,DateTime.UtcNow);
        Assert.Equal(HttpStatusCode.Conflict,(await e.Enviar($"/api/comandas/{c.Id}/itens",new {produtoId=e.ProdutoId,quantidade=0.5,operador="Ana"})).StatusCode);
        var produtos=new EfProdutoRepository(e.Banco.Fabrica);var p=produtos.BuscarPorId(e.ProdutoId)!;p.UnidadeVenda=UnidadeVenda.Quilo;produtos.Salvar(p);
        Assert.Equal(HttpStatusCode.OK,(await e.Enviar($"/api/comandas/{c.Id}/itens",new {produtoId=e.ProdutoId,quantidade=0.125,operador="Ana"})).StatusCode);
        var item=Assert.Single(e.Repositorio.BuscarComItens(c.Id)!.Itens);Assert.Equal(94,item.SubtotalCentavos);
        p.PrecoCentavos=900;produtos.Salvar(p);Assert.Equal(750,e.Repositorio.BuscarComItens(c.Id)!.Itens.Single().PrecoUnitarioCentavos);
        e.Repositorio.DefinirBloqueio(c.Id,true);
        Assert.Equal(HttpStatusCode.Conflict,(await e.Enviar($"/api/comandas/{c.Id}/itens",new {produtoId=e.ProdutoId,quantidade=1,operador="Ana"})).StatusCode);
        e.Repositorio.DefinirBloqueio(c.Id,false);e.Repositorio.CancelarComanda(c.Id,DateTime.UtcNow);
        Assert.Equal(HttpStatusCode.NotFound,(await e.Cliente.GetAsync($"/api/comandas/{c.Id}")).StatusCode);
    }
    [Fact]
    public async Task RejeitaOrigemExternaJsonInvalidoEOperadorVazio()
    {
        await using var e=await Ensaio.Criar();
        using var requisicao=new HttpRequestMessage(HttpMethod.Post,"/api/comandas") { Content=JsonContent.Create(new {numero=10}) };
        requisicao.Headers.Add("Origin","http://site-externo.invalid");requisicao.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());
        Assert.Equal(HttpStatusCode.Forbidden,(await e.Cliente.SendAsync(requisicao)).StatusCode);
        using var invalida=new HttpRequestMessage(HttpMethod.Post,"/api/comandas"){Content=new StringContent("{",Encoding.UTF8,"application/json")};
        invalida.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());Assert.Equal(HttpStatusCode.BadRequest,(await e.Cliente.SendAsync(invalida)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await e.Cliente.PostAsJsonAsync("/api/comandas",new {numero=10})).StatusCode);
        var c=e.Repositorio.AbrirComanda(12,DateTime.UtcNow);
        Assert.Equal(HttpStatusCode.BadRequest,(await e.Enviar($"/api/comandas/{c.Id}/itens",new {produtoId=e.ProdutoId,quantidade=1,operador=""})).StatusCode);
        Assert.Empty(e.Repositorio.BuscarComItens(c.Id)!.Itens);
    }
    [Fact]
    public async Task LicencaOuRestauracaoImpedemNovasOperacoes()
    {
        await using var e=await Ensaio.Criar();e.Permitido=false;
        Assert.Equal(HttpStatusCode.Forbidden,(await e.Enviar("/api/comandas",new {numero=10})).StatusCode);
        Assert.Empty(e.Repositorio.ListarAbertas());
        await e.Servidor.PararAsync();await Assert.ThrowsAsync<InvalidOperationException>(()=>e.Servidor.IniciarAsync(0,true));
        e.Permitido=true;e.Servidor.InicioPermitido=()=>false;await Assert.ThrowsAsync<InvalidOperationException>(()=>e.Servidor.IniciarAsync(0,true));
    }
    [Fact]
    public async Task FotoDoCatalogoExigeSessaoSemExporNomeOuCaminhoLocal()
    {
        await using var e=await Ensaio.Criar();
        var bytes=Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jX1sAAAAASUVORK5CYII=");
        var produtos=new EfProdutoRepository(e.Banco.Fabrica);var p=produtos.BuscarPorId(e.ProdutoId)!;
        p.FotoArquivo="produto-privado.png";produtos.Salvar(p);
        await File.WriteAllBytesAsync(Path.Combine(e.Banco.Paths.FotosDirectory,p.FotoArquivo),bytes);
        using var anonimo=new HttpClient{BaseAddress=e.Cliente.BaseAddress};
        Assert.Equal(HttpStatusCode.Unauthorized,(await anonimo.GetAsync($"/api/produtos/{p.Id}/foto")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await anonimo.GetAsync($"/api/produtos/{p.Id}/foto?chave={e.Chave}")).StatusCode);
        var catalogo=await e.Cliente.GetStringAsync("/api/catalogo");
        Assert.True(JsonDocument.Parse(catalogo).RootElement.GetProperty("produtos")[0].GetProperty("temFoto").GetBoolean());
        Assert.DoesNotContain(p.FotoArquivo,catalogo);Assert.DoesNotContain(e.Banco.Raiz,catalogo);
        using var resposta=await e.Cliente.GetAsync($"/api/produtos/{p.Id}/foto");
        Assert.Equal(HttpStatusCode.OK,resposta.StatusCode);Assert.Equal("image/png",resposta.Content.Headers.ContentType!.MediaType);
        Assert.Equal(bytes,await resposta.Content.ReadAsByteArrayAsync());
        Assert.True(resposta.Headers.CacheControl!.NoStore);Assert.Contains("blob:",resposta.Headers.GetValues("Content-Security-Policy").Single());
        File.Delete(Path.Combine(e.Banco.Paths.FotosDirectory,p.FotoArquivo));
        Assert.Equal(HttpStatusCode.NotFound,(await e.Cliente.GetAsync($"/api/produtos/{p.Id}/foto")).StatusCode);
    }
    [Theory]
    [InlineData("../fora.png")]
    [InlineData("..\\fora.png")]
    [InlineData("C:\\fora.png")]
    [InlineData("imagem.svg")]
    [InlineData("imagem.png:segredo")]
    public async Task FotoNaoPublicaCaminhosArbitrariosOuFormatosAtivos(string nome)
    {
        await using var e=await Ensaio.Criar();var produtos=new EfProdutoRepository(e.Banco.Fabrica);var p=produtos.BuscarPorId(e.ProdutoId)!;
        p.FotoArquivo=nome;produtos.Salvar(p);
        Assert.False(FotosProdutos.NomePermitido(nome));
        Assert.Equal(HttpStatusCode.NotFound,(await e.Cliente.GetAsync($"/api/produtos/{p.Id}/foto")).StatusCode);
    }
    [Fact]
    public async Task FotoRecusaArquivoInvalidoGrandeEProdutoOuCategoriaInativos()
    {
        await using var e=await Ensaio.Criar();var produtos=new EfProdutoRepository(e.Banco.Fabrica);var p=produtos.BuscarPorId(e.ProdutoId)!;
        p.FotoArquivo="foto.jpg";produtos.Salvar(p);var caminho=Path.Combine(e.Banco.Paths.FotosDirectory,p.FotoArquivo);
        await File.WriteAllTextAsync(caminho,"<svg onload='executar()'/>");
        Assert.Equal(HttpStatusCode.NotFound,(await e.Cliente.GetAsync($"/api/produtos/{p.Id}/foto")).StatusCode);
        using(var arquivo=File.Open(caminho,FileMode.Create)){arquivo.SetLength(10L*1024*1024+1);}
        Assert.Equal(HttpStatusCode.NotFound,(await e.Cliente.GetAsync($"/api/produtos/{p.Id}/foto")).StatusCode);
        await File.WriteAllBytesAsync(caminho,[0xFF,0xD8,0xFF,0]);
        p.Ativo=false;produtos.Salvar(p);
        Assert.Equal(HttpStatusCode.NotFound,(await e.Cliente.GetAsync($"/api/produtos/{p.Id}/foto")).StatusCode);
        p.Ativo=true;produtos.Salvar(p);var categorias=new EfCategoriaRepository(e.Banco.Fabrica);var categoria=categorias.BuscarPorId(p.CategoriaId)!;categoria.Ativo=false;categorias.Salvar(categoria);
        Assert.Equal(HttpStatusCode.NotFound,(await e.Cliente.GetAsync($"/api/produtos/{p.Id}/foto")).StatusCode);
    }
    private sealed class Ensaio : IAsyncDisposable
    {
        public BancoDeHomologacao Banco { get; }=new();
        public EfComandaRepository Repositorio { get; }
        public ServidorLocal Servidor { get; }
        public HttpClient Cliente { get; private set; }=new();
        public string Chave { get; private set; }="";
        public int ProdutoId { get; }
        public bool Permitido { get; set; }=true;
        private Ensaio()
        {
            ProdutoId=Banco.CriarProduto("Hambúrguer",750).Id;Repositorio=new(Banco.Fabrica);
            var produtos=new EfProdutoRepository(Banco.Fabrica);var config=new ObterConfiguracao(new EfConfiguracaoRepository(Banco.Fabrica));var clock=new RelogioFixo();
            var atendimento=new AtendimentoRemoto(Repositorio,produtos,new EfCategoriaRepository(Banco.Fabrica),new AbrirComanda(Repositorio,clock,config),new AdicionarItem(Repositorio,produtos,clock),config,new FotosProdutos(Banco.Paths.FotosDirectory));
            Servidor=new(atendimento,()=>Permitido);
        }
        public static async Task<Ensaio> Criar(){var e=new Ensaio();await e.Servidor.IniciarAsync(0,true);e.Conectar();return e;}
        public void Conectar()
        {
            Cliente.Dispose();Cliente=new HttpClient {BaseAddress=new Uri($"http://127.0.0.1:{Servidor.Porta}")};
            Chave=Servidor.LinkAcesso(Cliente.BaseAddress.ToString().TrimEnd('/')).Split("#chave=")[1];
            Cliente.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",Chave);
        }
        public Task<HttpResponseMessage> Enviar(string caminho,object corpo,Guid? chave=null)
        {
            var r=new HttpRequestMessage(HttpMethod.Post,caminho){Content=JsonContent.Create(corpo)};
            r.Headers.Add("Idempotency-Key",(chave??Guid.NewGuid()).ToString());return Cliente.SendAsync(r);
        }
        public async ValueTask DisposeAsync(){Cliente.Dispose();await Servidor.DisposeAsync();Banco.Dispose();}
    }
}
