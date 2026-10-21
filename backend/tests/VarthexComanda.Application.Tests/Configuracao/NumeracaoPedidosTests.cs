using VarthexComanda.Application.Configuracao;
using Xunit;

namespace VarthexComanda.Application.Tests.Configuracao;

public sealed class NumeracaoPedidosTests
{
    [Fact]
    public void PreferenciaPersisteSemAlterarContadorOuNumeroDaComanda()
    {
        var repo = new FakeConfiguracaoRepository();
        repo.Definir(NumeracaoPedidosService.ChaveUltimo, "114", DateTime.UtcNow);
        var service = new NumeracaoPedidosService(repo);
        service.Salvar(new() { Ativa = true, Inicio = 100, UsarComoSenhaAtendimento = true }, DateTime.UtcNow);
        var recarregada = new NumeracaoPedidosService(repo).Carregar();
        Assert.True(recarregada.Ativa);
        Assert.Equal(100, recarregada.Inicio);
        Assert.True(recarregada.UsarComoSenhaAtendimento);
        Assert.Equal(114, recarregada.UltimoCriado);
        Assert.Null(repo.ObterValor("comandas.quantidade_maxima"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1_000_000_000)]
    public void InicioInvalidoNaoGrava(long inicio)
    {
        var repo = new FakeConfiguracaoRepository();
        Assert.Throws<ArgumentException>(() => new NumeracaoPedidosService(repo).Salvar(new() { Ativa = true, Inicio = inicio }, DateTime.UtcNow));
        Assert.Null(repo.ObterValor(NumeracaoPedidosService.ChaveAtiva));
    }

    [Fact]
    public void ReinicioPreservaMaiorCodigoHistorico()
    {
        var repo = new FakeConfiguracaoRepository();
        repo.Definir(NumeracaoPedidosService.ChaveUltimo, "800", DateTime.UtcNow);
        var service = new NumeracaoPedidosService(repo, () => 114);
        service.Reiniciar(new() { Ativa = true, Inicio = 100 }, DateTime.UtcNow);
        Assert.Equal(114, service.Carregar().UltimoCriado);
        Assert.Equal("114", repo.ObterValor(NumeracaoPedidosService.ChaveUltimo));
    }
}
