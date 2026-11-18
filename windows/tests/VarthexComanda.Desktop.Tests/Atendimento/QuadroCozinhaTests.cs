using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Atendimento;

public class QuadroCozinhaTests
{
    [Fact] public async Task NovoLancamentoDaMesmaComandaEntraPendenteSemMoverItensEmPreparoOuProntos()
    {
        var preparo=CozinhaViewModelTests.Item(1,estado:StatusPreparo.EmPreparo);var pronto=CozinhaViewModelTests.Item(2,estado:StatusPreparo.Pronto);
        var lista=new List<ItemComanda>{preparo,pronto};var dados=CozinhaViewModelTests.Pedido(1,1);dados=new(){Comanda=dados.Comanda,Itens=lista};
        IReadOnlyList<Application.Atendimento.AlteracaoPreparo>? alteracoes=null;
        var vm=new CozinhaViewModel(()=>[dados],(_,a)=>{alteracoes=a;return Application.Catalogo.Resultado<Application.Atendimento.ConfirmacaoPreparo>.Ok(new(1));});
        vm.Atualizar();var antigo=Assert.Single(vm.Colunas[2].Pedidos);var finalizado=Assert.Single(vm.Colunas[3].Pedidos);
        lista.Add(CozinhaViewModelTests.Item(3));vm.Atualizar();var novo=Assert.Single(vm.Colunas[1].Pedidos);
        Assert.Same(antigo,Assert.Single(vm.Colunas[2].Pedidos));Assert.Same(finalizado,Assert.Single(vm.Colunas[3].Pedidos));Assert.Equal(3,Assert.Single(novo.Itens).Id);
        await vm.IniciarAsync(novo);Assert.Equal(3,Assert.Single(alteracoes!).ItemId);Assert.Equal(StatusPreparo.EmPreparo,preparo.StatusPreparo);Assert.Equal(StatusPreparo.Pronto,pronto.StatusPreparo);
    }
    [Fact] public void SomDeProntoTocaSomenteNaTransicaoConfirmadaSemRepetirOuTocarNaEntrega()
    {
        var a=CozinhaViewModelTests.Item(1);var b=CozinhaViewModelTests.Item(2);var dados=CozinhaViewModelTests.Pedido(1,1,a,b);var toques=0;
        var vm=new CozinhaViewModel(()=>[dados]){NotificarPedidoPronto=()=>toques++};vm.Atualizar();Assert.Equal(0,toques);
        a.StatusPreparo=StatusPreparo.Pronto;vm.Atualizar();Assert.Equal(0,toques);
        b.StatusPreparo=StatusPreparo.Pronto;vm.Atualizar();Assert.Equal(1,toques);vm.Atualizar();Assert.Equal(1,toques);
        a.StatusPreparo=b.StatusPreparo=StatusPreparo.Entregue;vm.Atualizar();Assert.Equal(1,toques);
        a.StatusPreparo=StatusPreparo.EmPreparo;vm.Atualizar();vm.SomAtivo=false;a.StatusPreparo=StatusPreparo.Pronto;vm.Atualizar();Assert.Equal(1,toques);vm.SomAtivo=true;vm.Atualizar();Assert.Equal(1,toques);
        var inicial=new CozinhaViewModel(()=>[dados]){NotificarPedidoPronto=()=>toques++};inicial.Atualizar();Assert.Equal(1,toques);
    }
    [Fact] public void LimpezaOcultaApenasSnapshotSemAlterarDadosOuNovasEntregas()
    {
        var item=CozinhaViewModelTests.Item(1,estado:StatusPreparo.Entregue);var dados=CozinhaViewModelTests.Pedido(1,1,item);
        var vm=new CozinhaViewModel(()=>[dados]){Agora=()=>CozinhaViewModelTests.Agora};vm.Atualizar();var snapshot=vm.CapturarEntregues();vm.LimparEntregues(snapshot);Assert.Empty(vm.Colunas[4].Pedidos);vm.Atualizar();Assert.Empty(vm.Colunas[4].Pedidos);Assert.Equal(StatusPreparo.Entregue,item.StatusPreparo);Assert.Equal(1000,dados.Comanda.TotalCentavos);
        item.AtualizadoEm=item.AtualizadoEm.AddSeconds(1);vm.Atualizar();Assert.Single(vm.Colunas[4].Pedidos);
        vm.SomAtivo=false;Assert.Contains("desativado",vm.SomDescricao);vm.SomAtivo=true;Assert.Contains("ativado",vm.SomDescricao);
    }
    [Fact] public async Task ArrastarPersisteTransicoesERecusaEntregaPrematuraRetornoEChamadas()
    {
        var item=CozinhaViewModelTests.Item(1);var dados=CozinhaViewModelTests.Pedido(1,1,item);var escritas=0;
        var vm=new CozinhaViewModel(()=>[dados],(_,alteracoes)=>{escritas++;foreach(var a in alteracoes){item.StatusPreparo=a.Destino;item.AtualizadoEm=CozinhaViewModelTests.Agora;}return Application.Catalogo.Resultado<Application.Atendimento.ConfirmacaoPreparo>.Ok(new(1));}){Agora=()=>CozinhaViewModelTests.Agora};
        vm.Atualizar();var pedido=vm.Pedidos[0];Assert.False(vm.PodeMover(pedido,vm.Colunas[4]));await vm.MoverAsync(pedido,vm.Colunas[4]);Assert.Equal(0,escritas);
        await vm.MoverAsync(pedido,vm.Colunas[2]);Assert.Equal(StatusPreparo.EmPreparo,item.StatusPreparo);Assert.False(vm.PodeMover(pedido,vm.Colunas[1]));Assert.False(vm.PodeMover(pedido,vm.Colunas[0]));
        await vm.MoverAsync(pedido,vm.Colunas[3]);Assert.Equal(StatusPreparo.Pronto,item.StatusPreparo);await vm.MoverAsync(pedido,vm.Colunas[4]);Assert.Equal(StatusPreparo.Entregue,item.StatusPreparo);Assert.Equal(3,escritas);Assert.False(vm.PodeMover(pedido,vm.Colunas[2]));
    }
    [Fact] public async Task TransicoesAtualizamColunasEEntregaParcialSomaSomenteItensEntregues()
    {
        var a=CozinhaViewModelTests.Item(1);var b=CozinhaViewModelTests.Item(2);var pedido=CozinhaViewModelTests.Pedido(1,2,a,b);
        var vm=new CozinhaViewModel(()=>[pedido],(_,alteracoes)=>{foreach(var alt in alteracoes){var item=pedido.Itens.Single(i=>i.Id==alt.ItemId);item.StatusPreparo=alt.Destino;item.AtualizadoEm=CozinhaViewModelTests.Agora;}return Application.Catalogo.Resultado<Application.Atendimento.ConfirmacaoPreparo>.Ok(new(1));}){Agora=()=>CozinhaViewModelTests.Agora};
        vm.Atualizar();Assert.Single(vm.Colunas[1].Pedidos);Assert.Equal("15:00 · Brasília",vm.Pedidos[0].Abertura);
        await vm.IniciarAsync(vm.Pedidos[0]);Assert.Empty(vm.Colunas[1].Pedidos);Assert.Single(vm.Colunas[2].Pedidos);
        a.StatusPreparo=StatusPreparo.Entregue;a.AtualizadoEm=CozinhaViewModelTests.Agora;vm.Atualizar();Assert.Equal(1000,Assert.Single(vm.Colunas[4].Pedidos).TotalCentavos);Assert.Single(vm.Colunas[2].Pedidos);
        await vm.MarcarProntosAsync(vm.Pedidos[0]);Assert.Single(vm.Colunas[3].Pedidos);await vm.EntregarAsync(vm.Pedidos[0]);Assert.Equal(2000,Assert.Single(vm.Colunas[4].Pedidos).TotalCentavos);Assert.Empty(vm.Pedidos);
    }
    [Fact] public void ChamadasNaoDuplicamMesaEAtendimentoNaoAlteraPedidos()
    {
        var vm=new CozinhaViewModel(()=>[]){Agora=()=>CozinhaViewModelTests.Agora};vm.RegistrarChamada(3,"Água");Assert.Equal(1,vm.Colunas[0].Quantidade);Assert.Throws<InvalidOperationException>(()=>vm.RegistrarChamada(3,"Conta"));vm.AtenderChamada(vm.Colunas[0].Chamadas[0]);Assert.Equal(0,vm.Colunas[0].Quantidade);Assert.Empty(vm.Pedidos);
    }
    [Fact] public void TituloPodeSerEditadoRestauradoEValidado()
    {
        var vm=new CozinhaViewModel(()=>[]);var c=vm.Colunas[2];vm.Renomear(c,"Na cozinha");Assert.Equal("Na cozinha",c.Titulo);vm.Renomear(c,c.Padrao);Assert.Equal("Preparando",c.Titulo);Assert.Throws<ArgumentException>(()=>vm.Renomear(c," "));
    }
}
