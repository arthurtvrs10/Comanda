using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Atendimento;

public class CozinhaViewModelTests
{
    [Fact]
    public void ResumoDoPreparoEObservacaoDoPedidoAcompanhamMudancasSemPerderCartao()
    {
        var a=Item(1);var b=Item(2);var pedido=Pedido(1,1,a,b);pedido.Comanda.Observacao="Sem talheres";
        var vm=new CozinhaViewModel(()=>[pedido]);vm.Atualizar();var cartao=Assert.Single(vm.Pedidos);
        Assert.Equal("Pendente",cartao.Estado);Assert.True(cartao.TemObservacao);Assert.Equal("Sem talheres",cartao.Observacao);
        a.StatusPreparo=StatusPreparo.Pronto;vm.Atualizar();Assert.Same(cartao,vm.Pedidos.Single());Assert.Equal("Em preparo",cartao.Estado);
        b.StatusPreparo=StatusPreparo.Pronto;pedido.Comanda.Observacao=" ";vm.Atualizar();Assert.Equal("Pronto",cartao.Estado);Assert.False(cartao.TemObservacao);
    }
    internal static readonly DateTime Agora=new(2026,10,23,19,0,0,DateTimeKind.Utc);
    internal static ItemComanda Item(int id,string nome="Lanche",StatusPreparo estado=StatusPreparo.Pendente,int produto=1,int minutos=5)=>new()
    {
        Id=id,ComandaId=1,ProdutoId=produto,NomeProduto=nome,PrecoUnitarioCentavos=1000,Quantidade=1,SubtotalCentavos=1000,
        StatusPreparo=estado,CriadoEm=Agora.AddMinutes(-minutos),AtualizadoEm=Agora.AddMinutes(-minutos)
    };
    internal static ComandaComItens Pedido(int id,int? mesa,params ItemComanda[] itens)
    {
        foreach(var item in itens)item.ComandaId=id;
        return new(){Comanda=new(){Id=id,Numero=100+id,NumeroMesa=mesa,CodigoPedido=200+id,Status=StatusComanda.Aberta,AbertaEm=Agora.AddHours(-1),TotalCentavos=itens.Sum(i=>i.SubtotalCentavos)},Itens=itens};
    }
    [Fact]
    public async Task AtualizacaoPreservaCartoesFiltraLocalEAcaoAfetaSomenteItensExibidos()
    {
        var a=Item(1,produto:1);var b=Item(2,produto:2);var dados=new[]{Pedido(1,2,a,b)};
        IReadOnlyList<AlteracaoPreparo>? enviadas=null;
        var vm=new CozinhaViewModel(()=>dados,(_,alteracoes)=>{enviadas=alteracoes;foreach(var alt in alteracoes)dados[0].Itens.Single(i=>i.Id==alt.ItemId).StatusPreparo=alt.Destino;return Resultado<ConfirmacaoPreparo>.Ok(new(1));})
        {Agora=()=>Agora,ConsultarLocais=()=>[new(){Nome="Cozinha",Produtos=[1]},new(){Nome="Bar",Produtos=[2]}]};
        vm.Atualizar();var cartao=vm.Pedidos.Single();vm.Atualizar();Assert.Same(cartao,vm.Pedidos.Single());
        vm.Local="Cozinha";Assert.Equal(1,vm.Pedidos.Single().Itens.Single().Id);
        await vm.MarcarProntosAsync(cartao);Assert.Equal(1,Assert.Single(enviadas!).ItemId);Assert.Equal(StatusPreparo.Pendente,b.StatusPreparo);
        vm.Estado="Prontos";await vm.EntregarAsync(vm.Pedidos.Single());Assert.Equal(StatusPreparo.Entregue,a.StatusPreparo);Assert.True(vm.Vazio);
        vm.Local="Bar";vm.Estado="Todos os itens";Assert.Equal(2,vm.Pedidos.Single().Itens.Single().Id);
    }
    [Fact]
    public void SomNaoTocaNaCargaInicialFiltrosOuRepeticaoESomDesligadoRespeitado()
    {
        var itens=new List<ItemComanda>{Item(1)};var pedido=new ComandaComItens{Comanda=Pedido(1,null).Comanda,Itens=itens};var toques=0;
        var vm=new CozinhaViewModel(()=>[pedido]){Agora=()=>Agora,NotificarNovoItem=()=>toques++};
        vm.Atualizar();Assert.Equal(0,toques);itens.Add(Item(2));vm.Atualizar();Assert.Equal(1,toques);
        vm.Estado="Prontos";vm.Atualizar();vm.Estado="Todos os itens";Assert.Equal(1,toques);
        vm.SomAtivo=false;itens.Add(Item(3));vm.Atualizar();vm.SomAtivo=true;vm.Atualizar();Assert.Equal(1,toques);
        Assert.False(vm.PodeAlterar);
    }
    [Fact]
    public async Task FalhaMantemUltimaFilaDesabilitaAcoesEAtualizacaoRecupera()
    {
        var falhar=false;var pedido=Pedido(1,1,Item(1));
        var vm=new CozinhaViewModel(()=>falhar?throw new IOException():[pedido],(_,_)=>Resultado<ConfirmacaoPreparo>.Ok(new(1))){Agora=()=>Agora};
        await vm.AtualizarAsync();var cartao=vm.Pedidos.Single();falhar=true;await vm.AtualizarAsync();
        Assert.Same(cartao,vm.Pedidos.Single());Assert.True(vm.Falha);Assert.False(vm.PodeAlterar);Assert.Contains("mantidos",vm.Mensagem);
        falhar=false;await vm.AtualizarAsync();Assert.False(vm.Falha);Assert.True(vm.PodeAlterar);
    }
    [Fact]
    public async Task ConflitoNaoApresentaSucessoNemPermiteRepetirAcaoSemConferir()
    {
        var vm=new CozinhaViewModel(()=>[Pedido(1,1,Item(1))],(_,_)=>Resultado<ConfirmacaoPreparo>.Falha("Os itens mudaram.")){Agora=()=>Agora};
        vm.Atualizar();await vm.AvancarAsync(vm.Pedidos.Single().Itens.Single());
        Assert.Equal("Os itens mudaram.",vm.Mensagem);Assert.True(vm.Falha);Assert.False(vm.PodeAlterar);
        Assert.Equal(StatusPreparo.Pendente,vm.Pedidos.Single().Itens.Single().Status);
    }
    [Fact]
    public void TempoNuncaNegativoPrioridadeEEsperaSinalizamSemDependerSoDaCor()
    {
        var normal=Pedido(1,1,Item(1,minutos:-2));var alerta=Pedido(2,2,Item(2,minutos:12));var atraso=Pedido(3,3,Item(3,minutos:22));
        var vm=new CozinhaViewModel(()=>[normal,alerta,atraso]){Agora=()=>Agora};vm.Atualizar();
        Assert.Equal("0 min",vm.Pedidos[0].Tempo);Assert.Equal("normal",vm.Pedidos[0].Nivel);Assert.Equal("atencao",vm.Pedidos[1].Nivel);Assert.Equal("atraso",vm.Pedidos[2].Nivel);
        normal.Comanda.Prioridade=PrioridadeComanda.Urgente;vm.Atualizar();Assert.Equal("atraso",vm.Pedidos[0].Nivel);
    }
}
