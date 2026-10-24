using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Atendimento;
using VarthexComanda.Application.Tests.Catalogo;

namespace VarthexComanda.Application.Tests.Configuracao;

public class LimiteComandasTests
{
    [Theory]
    [InlineData(0)] [InlineData(501)] [InlineData(9999)]
    public void TodasAsConfiguracoesRecusamQuantidadeForaDaFaixaSemSalvar(int quantidade)
    {
        var repo = new FakeConfiguracaoRepository(); var clock = new FakeClock();
        new NumeracaoComandasService(repo).Salvar(new() { Ativa = true, Inicio = 100 }, clock.UtcNow, 500);
        Assert.Throws<ArgumentException>(() => new NumeracaoComandasService(repo).Salvar(new() { Ativa = true, Inicio = 200 }, clock.UtcNow, quantidade));
        Assert.Throws<ArgumentException>(() => new ConfiguracoesSistemaService(repo).Salvar(new(), "Teste", quantidade, clock.UtcNow));
        Assert.False(new SalvarConfiguracao(repo, clock).Executar(new() { NomeEstabelecimento = "Teste", QuantidadeMaximaComandas = quantidade }).Sucesso);
        Assert.Equal("500", repo.ObterValor("comandas.quantidade_maxima"));
        Assert.Equal(100, new ObterConfiguracao(repo).Executar().NumeroInicialComandas);
    }
    [Fact]
    public void LimiteEQuantidadeNaoNumeroPreservaComandaAntigaForaDaFaixa()
    {
        var repo = new FakeConfiguracaoRepository(); var comandas = new FakeComandaRepository(); var clock = new FakeClock();
        var antiga = comandas.AbrirComanda(800, clock.UtcNow);
        new NumeracaoComandasService(repo).Salvar(new() { Ativa = true, Inicio = 100 }, clock.UtcNow, 500);
        var abrir = new AbrirComanda(comandas, clock, new ObterConfiguracao(repo));
        Assert.True(abrir.Executar(599).Sucesso); Assert.False(abrir.Executar(600).Sucesso);
        Assert.NotNull(comandas.BuscarComItens(antiga.Id));
        Assert.Equal(100, abrir.ExecutarBalcao().Valor!.Numero);
        repo.Definir("comandas.quantidade_maxima", "9999", clock.UtcNow);
        Assert.Equal(500, new ObterConfiguracao(repo).Executar().QuantidadeMaximaComandas);
        Assert.False(abrir.Executar(600).Sucesso); Assert.Equal(800, comandas.BuscarComItens(antiga.Id)!.Comanda.Numero);
    }
}
