using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Application.Tests.Configuracao;

public class PreferenciasImpressaoTests
{
    [Fact]
    public void SemConfiguracaoMantemEscolhaManualAnterior()
    {
        var service = new ConfiguracoesImpressaoService(new FakeConfiguracaoRepository());
        Assert.Null(service.ObterCaixaConfigurado());
        Assert.Equal(ModoImpressora.Desativada, service.Carregar().Caixa.Modo);
        Assert.Single(service.Carregar().Cozinhas);
    }

    [Fact]
    public void SalvarRecarregaCaixaESincronizaSaidaDaCozinhaSemAlterarOutrosDados()
    {
        var repo = new FakeConfiguracaoRepository(); var sistema = new ConfiguracoesSistemaService(repo);
        var dados = sistema.Carregar(); dados.Cidade = "Recife"; dados.Cozinhas[0].Produtos = [1, 2]; sistema.Salvar(dados, "Loja", 15, DateTime.UtcNow);
        var service = new ConfiguracoesImpressaoService(repo); var p = service.Carregar();
        p.Caixa = new() { Modo = ModoImpressora.Instalada, Caminho = "Cupom USB", LarguraPapelMm=58 };
        p.Cozinhas["Cozinha principal"] = new() { Modo = ModoImpressora.Compartilhada, Caminho = @"\\cozinha\termica", LarguraPapelMm=58 };
        service.Salvar(p, DateTime.UtcNow);
        var salvo = new ConfiguracoesImpressaoService(repo).Carregar();
        Assert.Equal("Cupom USB", service.ObterCaixaConfigurado()!.Caminho);
        Assert.Equal(58,salvo.Caixa.LarguraPapelMm);Assert.Equal(58,salvo.Cozinhas["Cozinha principal"].LarguraPapelMm);
        Assert.Equal(@"\\cozinha\termica", salvo.Cozinhas["Cozinha principal"].Caminho);
        var local = sistema.Carregar().Cozinhas[0]; Assert.True(local.Imprimir); Assert.Equal(@"\\cozinha\termica", local.Impressora); Assert.Equal([1, 2], local.Produtos);
        Assert.Equal("Recife", sistema.Carregar().Cidade); Assert.Equal("15", repo.ObterValor("comandas.quantidade_maxima"));
    }

    [Fact]
    public void SaidaInvalidaNaoSobrescrevePreferenciaAnterior()
    {
        var repo = new FakeConfiguracaoRepository(); var service = new ConfiguracoesImpressaoService(repo); var p = service.Carregar();
        p.Caixa = new() { Modo = ModoImpressora.Compartilhada, Caminho = "impressora" };
        Assert.Throws<ArgumentException>(() => service.Salvar(p, DateTime.UtcNow));
        Assert.Null(repo.ObterValor(ConfiguracoesImpressaoService.Chave));
        Assert.Null(repo.ObterValor(ConfiguracoesSistemaService.Chave));
    }

    [Fact]
    public void CozinhaPodeUsarCaixaOuSomenteMonitor()
    {
        var repo = new FakeConfiguracaoRepository(); var service = new ConfiguracoesImpressaoService(repo); var p = service.Carregar();
        p.Cozinhas["Cozinha principal"].Modo = ModoImpressora.MesmaDoCaixa; service.Salvar(p, DateTime.UtcNow);
        var local = new ConfiguracoesSistemaService(repo).Carregar().Cozinhas[0]; Assert.True(local.Imprimir); Assert.Equal("Mesma do caixa", local.Impressora);
        p.Cozinhas["Cozinha principal"].Modo = ModoImpressora.Desativada; service.Salvar(p, DateTime.UtcNow);
        Assert.False(new ConfiguracoesSistemaService(repo).Carregar().Cozinhas[0].Imprimir);
    }

    [Fact]
    public void AlteracaoPeloEditorDeCozinhasPrevaleceSobrePreferenciaAnterior()
    {
        var repo = new FakeConfiguracaoRepository(); var service = new ConfiguracoesImpressaoService(repo); var p = service.Carregar();
        p.Cozinhas["Cozinha principal"] = new() { Modo = ModoImpressora.Instalada, Caminho = "Cupom USB" }; service.Salvar(p, DateTime.UtcNow);
        var sistema = new ConfiguracoesSistemaService(repo); var alterado = sistema.Carregar(); alterado.Cozinhas[0].Imprimir = false;
        sistema.Salvar(alterado, "Loja", 20, DateTime.UtcNow);
        Assert.Equal(ModoImpressora.Desativada, service.Carregar().Cozinhas["Cozinha principal"].Modo);
        alterado.Cozinhas[0].Imprimir = true; alterado.Cozinhas[0].Impressora = @"\\outro\cozinha"; sistema.Salvar(alterado, "Loja", 20, DateTime.UtcNow);
        Assert.Equal(@"\\outro\cozinha", service.Carregar().Cozinhas["Cozinha principal"].Caminho);
    }
}
