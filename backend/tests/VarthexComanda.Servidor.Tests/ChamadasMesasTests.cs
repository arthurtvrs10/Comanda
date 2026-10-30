using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Servidor.Tests;
public class ChamadasMesasTests
{
    [Fact]
    public async Task CelularAutenticadoPersisteChamadaSemDuplicarEReenvioAposAtendimentoNaoRecria()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var config=new EfConfiguracaoRepository(banco.Fabrica);var produtos=new EfProdutoRepository(banco.Fabrica);var clock=new RelogioFixo();var obter=new ObterConfiguracao(config);
        var remoto=new AtendimentoRemoto(repo,produtos,new EfCategoriaRepository(banco.Fabrica),new(repo,clock,obter),new(repo,produtos,clock),obter,configuracoes:config);
        await using var servidor=new ServidorLocal(remoto);await servidor.IniciarAsync(0,true);
        using var cliente=new HttpClient {BaseAddress=new Uri($"http://127.0.0.1:{servidor.Porta}")};
        var c=repo.AbrirComanda(1,clock.UtcNow);repo.DefinirMesa(c.Id,12);var rota=$"/api/comandas/{c.Id}/chamadas";var id=Guid.NewGuid();
        Assert.Equal(HttpStatusCode.Unauthorized,(await cliente.PostAsJsonAsync(rota,new PedidoChamada("Guardanapos",id))).StatusCode);
        cliente.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",servidor.LinkAcesso(cliente.BaseAddress.ToString().TrimEnd('/')).Split("#chave=")[1]);
        var respostas=await Task.WhenAll(Enumerable.Range(0,8).Select(_=>cliente.PostAsJsonAsync(rota,new PedidoChamada("Guardanapos",id))));
        Assert.All(respostas,r=>Assert.Equal(HttpStatusCode.OK,r.StatusCode));var service=new ChamadasMesasService(config);var chamada=Assert.Single(service.Listar());Assert.Equal(12,chamada.Mesa);Assert.Equal("Guardanapos",chamada.Solicitacao);
        await cliente.PostAsJsonAsync(rota,new PedidoChamada("Outra solicitação",Guid.NewGuid()));Assert.Single(service.Listar());
        service.Atender(chamada.Id);Assert.Empty(service.Listar());
        Assert.Equal(HttpStatusCode.OK,(await cliente.PostAsJsonAsync(rota,new PedidoChamada("Guardanapos",id))).StatusCode);Assert.Empty(service.Listar());
        await servidor.PararAsync();await servidor.IniciarAsync(0,true);
        Assert.Empty(new ChamadasMesasService(new EfConfiguracaoRepository(banco.Fabrica)).Listar());
        using var reiniciado=new HttpClient {BaseAddress=new Uri($"http://127.0.0.1:{servidor.Porta}")};
        reiniciado.DefaultRequestHeaders.Authorization=cliente.DefaultRequestHeaders.Authorization;
        Assert.Equal(HttpStatusCode.Unauthorized,(await reiniciado.PostAsJsonAsync(rota,new PedidoChamada("Teste",Guid.NewGuid()))).StatusCode);
    }
    [Fact]
    public void ValidacaoMesaConcorrenciaELeituraEmNovaInstanciaPreservamFila()
    {
        using var banco=new BancoDeHomologacao();var config=new EfConfiguracaoRepository(banco.Fabrica);var s=new ChamadasMesasService(config);
        Assert.Throws<ArgumentException>(()=>s.Registrar(1,0,"Teste",Guid.NewGuid()));Assert.Throws<ArgumentException>(()=>s.Registrar(1,1,new string('x',121),Guid.NewGuid()));Assert.Empty(s.Listar());
        Parallel.For(1,21,i=>new ChamadasMesasService(new EfConfiguracaoRepository(banco.Fabrica)).Registrar(i,i,"Atendimento",Guid.NewGuid()));
        Assert.Equal(20,s.Listar().Count);s.Atender(s.Listar()[0].Id);Assert.Equal(19,new ChamadasMesasService(new EfConfiguracaoRepository(banco.Fabrica)).Listar().Count);
    }
    [Fact]
    public void ComandaSemMesaOuEncerradaNaoPodeChamar()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var config=new EfConfiguracaoRepository(banco.Fabrica);var produtos=new EfProdutoRepository(banco.Fabrica);var clock=new RelogioFixo();var obter=new ObterConfiguracao(config);
        var remoto=new AtendimentoRemoto(repo,produtos,new EfCategoriaRepository(banco.Fabrica),new(repo,clock,obter),new(repo,produtos,clock),obter,configuracoes:config);var c=repo.AbrirComanda(1,clock.UtcNow);
        Assert.Equal(409,remoto.ChamarAtendente(c.Id,"Teste",Guid.NewGuid()).Status);Assert.Equal(404,remoto.ChamarAtendente(999,"Teste",Guid.NewGuid()).Status);repo.CancelarComanda(c.Id,clock.UtcNow);Assert.Equal(404,remoto.ChamarAtendente(c.Id,"Teste",Guid.NewGuid()).Status);
    }
}
