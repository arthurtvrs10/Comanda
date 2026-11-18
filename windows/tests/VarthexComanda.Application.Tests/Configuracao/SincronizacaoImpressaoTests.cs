using VarthexComanda.Application.Configuracao;
namespace VarthexComanda.Application.Tests.Configuracao;
public class SincronizacaoImpressaoTests
{
    [Fact] public void AlteracoesEmGeralEImpressorasUsamMesmasPreferencias()
    {
        var r=new FakeConfiguracaoRepository();var geral=new ConfiguracoesSistemaService(r);var p=geral.Carregar();p.SolicitarSenha=true;p.Cidade="Brasília";geral.Salvar(p,"Restaurante",20,DateTime.UtcNow);
        var layouts=new LayoutTicketService(r);layouts.Salvar(new(){PapelMm=58,ObservacoesItem=false});
        Assert.Null(new ConfiguracoesImpressaoService(r).ObterCaixaConfigurado());Assert.False(geral.Carregar().ExibirObservacoesCupom);Assert.Equal(58,new ConfiguracoesImpressaoService(r).Carregar().Caixa.LarguraPapelMm);
        p=geral.Carregar();p.ExibirObservacoesCupom=true;p.RodapeCupom="Volte sempre";geral.Salvar(p,"Restaurante",20,DateTime.UtcNow);
        Assert.True(layouts.Carregar()!.ObservacoesItem);Assert.Equal("Volte sempre",layouts.Preferencias.RodapeCupom);
        var imp=new ConfiguracoesImpressaoService(r);var saidas=imp.Carregar();saidas.Caixa.LarguraPapelMm=80;var cupom=layouts.Preferencias;cupom.CabecalhoCupom="Conta da mesa";cupom.ExibirObservacoesCupom=false;
        imp.Salvar(saidas,DateTime.UtcNow,layouts.Carregar(),cupom);
        Assert.Equal(80,layouts.Carregar()!.PapelMm);Assert.False(layouts.Carregar()!.ObservacoesItem);Assert.Equal("Conta da mesa",geral.Carregar().CabecalhoCupom);Assert.True(geral.Carregar().SolicitarSenha);Assert.Equal("Brasília",geral.Carregar().Cidade);
    }
    [Fact] public void LayoutInvalidoNaoSalvaParteDasConfiguracoes()
    {
        var r=new FakeConfiguracaoRepository();var geral=new ConfiguracoesSistemaService(r);geral.Salvar(new(),"Original",20,DateTime.UtcNow);
        Assert.Throws<ArgumentException>(()=>geral.Salvar(new(){RodapeCupom="Alterado"},"Novo",20,DateTime.UtcNow,layoutTicket:new(){PapelMm=70}));
        Assert.Equal("Original",r.ObterValor("estabelecimento.nome"));Assert.NotEqual("Alterado",geral.Carregar().RodapeCupom);Assert.Null(new LayoutTicketService(r).Carregar());
    }
}
