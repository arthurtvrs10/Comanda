using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Catalogo;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Domain;
using Xunit;

namespace VarthexComanda.Application.Tests.Atendimento;

public class AbrirComandaTests
{
    [Fact]
    public void BalcaoEscolhePrimeiroNumeroLivreSemMesaEPreservaPedidosExistentes()
    {
        var repo=new FakeComandaRepository();var relogio=new FakeClock();var config=new FakeConfiguracaoRepository();
        config.Definir(NumeracaoComandasService.ChaveAtiva,"true",relogio.UtcNow);
        config.Definir(NumeracaoComandasService.ChaveInicio,"100",relogio.UtcNow);
        config.Definir("comandas.quantidade_maxima","3",relogio.UtcNow);
        var caso=new AbrirComanda(repo,relogio,new ObterConfiguracao(config));
        var existente=caso.Executar(100).Valor!;repo.DefinirMesa(existente.Id,7);
        var pedido=caso.ExecutarBalcao();
        Assert.True(pedido.Sucesso);Assert.Equal(101,pedido.Valor!.Numero);Assert.Null(pedido.Valor.NumeroMesa);
        Assert.Equal(7,repo.BuscarComItens(existente.Id)!.Comanda.NumeroMesa);
        Assert.Equal(102,caso.ExecutarBalcao().Valor!.Numero);
        var lotado=caso.ExecutarBalcao();Assert.False(lotado.Sucesso);Assert.Equal(3,repo.ListarAbertas().Count);
        repo.CancelarComanda(pedido.Valor.Id,relogio.UtcNow);
        Assert.Equal(101,caso.ExecutarBalcao().Valor!.Numero);
    }

    [Fact]
    [Trait("Caso", "CT01")]
    public void Executar_NumeroValido_AbreComanda()
    {
        var caso = new AbrirComanda(new FakeComandaRepository(), new FakeClock(), new ObterConfiguracao(new FakeConfiguracaoRepository()));

        var resultado = caso.Executar(10);

        Assert.True(resultado.Sucesso);
        Assert.Equal(10, resultado.Valor!.Numero);
        Assert.Equal(StatusComanda.Aberta, resultado.Valor.Status);
    }

    [Fact]
    public void Executar_NumeroZeroOuNegativo_Falha()
    {
        var caso = new AbrirComanda(new FakeComandaRepository(), new FakeClock(), new ObterConfiguracao(new FakeConfiguracaoRepository()));

        var resultado = caso.Executar(0);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Informe um número de comanda válido.", resultado.Erros);
    }

    [Fact]
    [Trait("Caso", "CT02")]
    public void Executar_NumeroJaAberto_Falha()
    {
        var repositorio = new FakeComandaRepository();
        var caso = new AbrirComanda(repositorio, new FakeClock(), new ObterConfiguracao(new FakeConfiguracaoRepository()));
        caso.Executar(10);

        var resultado = caso.Executar(10);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Já existe uma comanda aberta com esse número.", resultado.Erros);
    }

    [Fact]
    public void Executar_SemConfiguracao_NumeroAltoContinuaFuncionando()
    {
        var caso = new AbrirComanda(new FakeComandaRepository(), new FakeClock(), new ObterConfiguracao(new FakeConfiguracaoRepository()));

        var resultado = caso.Executar(9999);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public void Executar_ComConfiguracao_NumeroAcimaDoLimiteFalha()
    {
        var configuracoes = new FakeConfiguracaoRepository();
        configuracoes.Definir("comandas.quantidade_maxima", "30", DateTime.UtcNow);
        var caso = new AbrirComanda(new FakeComandaRepository(), new FakeClock(), new ObterConfiguracao(configuracoes));

        var resultado = caso.Executar(31);

        Assert.False(resultado.Sucesso);
        Assert.Contains("O número da comanda deve ser no máximo 30.", resultado.Erros);
    }

    [Fact]
    public void Executar_ComConfiguracao_NumeroDentroDoLimitePassa()
    {
        var configuracoes = new FakeConfiguracaoRepository();
        configuracoes.Definir("comandas.quantidade_maxima", "30", DateTime.UtcNow);
        var caso = new AbrirComanda(new FakeComandaRepository(), new FakeClock(), new ObterConfiguracao(configuracoes));

        var resultado = caso.Executar(30);

        Assert.True(resultado.Sucesso);
    }
}
