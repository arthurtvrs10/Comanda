using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Servidor.Tests;
public class QrMesasTests
{
    private sealed class Ensaio:IAsyncDisposable
    {
        public BancoDeHomologacao Banco=new();
        public EfConfiguracaoRepository Config;
        public ServidorLocal Servidor;
        public HttpClient Cliente=new();
        public Ensaio()
        {
            Config=new(Banco.Fabrica);var repo=new EfComandaRepository(Banco.Fabrica);var produtos=new EfProdutoRepository(Banco.Fabrica);var obter=new ObterConfiguracao(Config);var clock=new RelogioFixo();
            Servidor=new(new(repo,produtos,new EfCategoriaRepository(Banco.Fabrica),new(repo,clock,obter),new(repo,produtos,clock),obter,configuracoes:Config));
        }
        public async Task Iniciar(){await Servidor.IniciarAsync(0,true);Cliente.Dispose();Cliente=new(){BaseAddress=new Uri($"http://127.0.0.1:{Servidor.Porta}")};}
        public string Token(int mesa)=>Servidor.LinkMesa(Cliente.BaseAddress!.ToString().TrimEnd('/'),mesa).Split("&chave=")[1];
        public void Mesa(int mesa,string? token=null){Cliente.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token??Token(mesa));Cliente.DefaultRequestHeaders.Remove("X-Mesa");Cliente.DefaultRequestHeaders.Add("X-Mesa",mesa.ToString());}
        public async ValueTask DisposeAsync(){Cliente.Dispose();await Servidor.DisposeAsync();Banco.Dispose();}
    }
    [Fact] public async Task QrDaMesaChamaSemComandaENaoPermiteAcessarCatalogoOuPedidos()
    {
        await using var e=new Ensaio();await e.Iniciar();e.Mesa(12);
        Assert.Empty(new EfComandaRepository(e.Banco.Fabrica).ListarAbertas());
        Assert.Equal(HttpStatusCode.OK,(await e.Cliente.GetAsync("/mesa")).StatusCode);
        var id=Guid.NewGuid();Assert.Equal(HttpStatusCode.OK,(await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada("Água, por favor",id))).StatusCode);
        var chamada=Assert.Single(new ChamadasMesasService(e.Config).Listar());Assert.Equal(12,chamada.Mesa);Assert.Equal("Água, por favor",chamada.Solicitacao);
        Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.GetAsync("/api/catalogo")).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.GetAsync("/api/comandas")).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.PostAsJsonAsync("/api/comandas",new AberturaRemota(1))).StatusCode);
        await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada("Outro texto",Guid.NewGuid()));Assert.Single(new ChamadasMesasService(e.Config).Listar());
        new ChamadasMesasService(e.Config).Atender(chamada.Id);await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada("Água, por favor",id));Assert.Empty(new ChamadasMesasService(e.Config).Listar());
    }
    [Fact] public async Task TokensSaoPorMesaPersistemNoReinicioERespeitamQuantidade()
    {
        await using var e=new Ensaio();await e.Iniciar();e.Servidor.DefinirQuantidadeMesas(12);var token=e.Token(12);Assert.NotEqual(e.Token(1),token);e.Mesa(1,token);Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.GetAsync("/api/mesa-publica")).StatusCode);
        e.Mesa(12,token);Assert.Equal(HttpStatusCode.OK,(await e.Cliente.GetAsync("/api/mesa-publica")).StatusCode);await e.Servidor.PararAsync();await e.Iniciar();Assert.Equal(token,e.Token(12));e.Mesa(12,token);Assert.Equal(HttpStatusCode.OK,(await e.Cliente.GetAsync("/api/mesa-publica")).StatusCode);
        e.Servidor.DefinirQuantidadeMesas(5);Assert.Equal(5,new QrMesasService(new EfConfiguracaoRepository(e.Banco.Fabrica)).Quantidade);Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.GetAsync("/api/mesa-publica")).StatusCode);Assert.Throws<ArgumentException>(()=>e.Token(6));
    }
    [Fact] public async Task MesaRecusaSolicitacoesInvalidasOrigemExternaEExcessoDeTentativas()
    {
        await using var e=new Ensaio();await e.Iniciar();e.Mesa(1);
        Assert.Equal(HttpStatusCode.BadRequest,(await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada(new string('a',121),Guid.NewGuid()))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada("Teste",Guid.Empty))).StatusCode);
        e.Cliente.DefaultRequestHeaders.Add("Origin","http://outro-site.invalid");Assert.Equal(HttpStatusCode.Forbidden,(await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada("Teste",Guid.NewGuid()))).StatusCode);e.Cliente.DefaultRequestHeaders.Remove("Origin");
        for(var i=0;i<4;i++)Assert.Equal(HttpStatusCode.OK,(await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada(null,Guid.NewGuid()))).StatusCode);
        Assert.Equal((HttpStatusCode)429,(await e.Cliente.PostAsJsonAsync("/api/mesa-publica",new PedidoChamada(null,Guid.NewGuid()))).StatusCode);Assert.Single(new ChamadasMesasService(e.Config).Listar());
    }
    [Fact] public async Task SemQrOuChaveDaEquipeNaoAutorizaMesa()
    {
        await using var e=new Ensaio();await e.Iniciar();Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.GetAsync("/api/mesa-publica")).StatusCode);
        var equipe=e.Servidor.LinkAcesso(e.Cliente.BaseAddress!.ToString().TrimEnd('/')).Split("#chave=")[1];e.Mesa(1,equipe);Assert.Equal(HttpStatusCode.Unauthorized,(await e.Cliente.GetAsync("/api/mesa-publica")).StatusCode);
    }
}
