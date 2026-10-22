using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Atendimento;
using Xunit;

namespace VarthexComanda.Application.Tests.Configuracao;

public class NumeracaoComandasTests
{
    [Fact]
    public void InicioDaFaixaPersisteSemRenumerarHistoricoNemAlocarOutroCodigo()
    {
        var repo=new FakeConfiguracaoRepository();repo.Definir("comandas.quantidade_maxima","20",DateTime.UtcNow);
        repo.Definir(NumeracaoPedidosService.ChaveUltimo,"114",DateTime.UtcNow);
        var service=new NumeracaoComandasService(repo);service.Salvar(new(){Ativa=true,Inicio=100},DateTime.UtcNow);
        var atual=new ObterConfiguracao(repo).Executar();Assert.Equal(100,atual.NumeroInicialComandas);Assert.Equal(20,atual.QuantidadeMaximaComandas);
        var comandas=new FakeComandaRepository();var abrir=new AbrirComanda(comandas,new VarthexComanda.Application.Tests.Catalogo.FakeClock(),new ObterConfiguracao(repo));
        Assert.False(abrir.Executar(99).Sucesso);Assert.True(abrir.Executar(100).Sucesso);Assert.True(abrir.Executar(119).Sucesso);Assert.False(abrir.Executar(120).Sucesso);
        Assert.Equal("114",repo.ObterValor(NumeracaoPedidosService.ChaveUltimo));Assert.Equal("false",repo.ObterValor(NumeracaoPedidosService.ChaveAtiva));
        service.Salvar(new(){Ativa=false,Inicio=100},DateTime.UtcNow);Assert.Equal(1,new ObterConfiguracao(repo).Executar().NumeroInicialComandas);
    }
    [Fact]
    public void QuantidadeSalvaJuntoDaFaixaEValidaAntesDePersistir()
    {
        var repo=new FakeConfiguracaoRepository();var service=new NumeracaoComandasService(repo);
        service.Salvar(new(){Ativa=true,Inicio=100},DateTime.UtcNow,50);
        Assert.Equal(50,service.Quantidade);
        Assert.Throws<ArgumentException>(()=>service.Salvar(new(){Ativa=true,Inicio=200},DateTime.UtcNow,0));
        Assert.Equal(100,service.Carregar().Inicio);Assert.Equal(50,service.Quantidade);
        service.Salvar(new(){Ativa=false},DateTime.UtcNow,null);Assert.Null(service.Quantidade);
    }
    [Fact]
    public void InicioAntigoPreenchidoNaJanelaEInterpretadoComoInicioDaGrade()
    {
        var repo=new FakeConfiguracaoRepository();repo.Definir(NumeracaoPedidosService.ChaveAtiva,"true",DateTime.UtcNow);repo.Definir(NumeracaoPedidosService.ChaveInicio,"100",DateTime.UtcNow);
        Assert.Equal(100,new ObterConfiguracao(repo).Executar().NumeroInicialComandas);
    }
    [Fact]
    public void FaixaQueUltrapassaLimiteNaoSalva()
    {
        var repo=new FakeConfiguracaoRepository();Assert.Throws<ArgumentException>(()=>new NumeracaoComandasService(repo).Salvar(new(){Ativa=true,Inicio=999_999_999},DateTime.UtcNow));Assert.Null(repo.ObterValor(NumeracaoComandasService.ChaveInicio));
    }
}
