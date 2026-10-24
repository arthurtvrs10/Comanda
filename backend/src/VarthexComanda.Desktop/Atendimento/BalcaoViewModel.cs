using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class BalcaoViewModel : ObservableObject
{
    private readonly AtendimentoViewModel _atendimento;
    public ListCollectionView Pedidos { get; }
    public event Action<Comanda,bool>? AbrirPedidoSolicitado;
    [ObservableProperty] private string busca="";
    [ObservableProperty] private string mensagem="";
    [ObservableProperty] private Comanda? selecionado;
    public bool Vazio=>Pedidos.IsEmpty;
    public string Contagem=>$"{Pedidos.Count} pedido{(Pedidos.Count==1?"":"s")} no balcão";

    public BalcaoViewModel(AtendimentoViewModel atendimento)
    {
        _atendimento=atendimento;
        Pedidos=new ListCollectionView(atendimento.ComandasAbertas) { Filter=Filtrar };
        Pedidos.SortDescriptions.Add(new(nameof(Comanda.AbertaEm),ListSortDirection.Descending));
        ((INotifyCollectionChanged)Pedidos).CollectionChanged+=(_,_)=>{OnPropertyChanged(nameof(Vazio));OnPropertyChanged(nameof(Contagem));};
    }
    private bool Filtrar(object valor)=>valor is Comanda { NumeroMesa:null } c &&
        (string.IsNullOrWhiteSpace(Busca) || $"{c.Numero} {c.CodigoPedido??c.Id} {c.NomeCliente}".Contains(Busca.Trim().TrimStart('#'),StringComparison.CurrentCultureIgnoreCase));
    partial void OnBuscaChanged(string value)=>Pedidos.Refresh();
    [RelayCommand] private void NovoPedido()
    {
        var pedido=_atendimento.CriarPedidoBalcao();Mensagem=_atendimento.Mensagem;
        if(pedido is not null)AbrirPedidoSolicitado?.Invoke(pedido,true);
    }
    [RelayCommand] private void AbrirPedido(Comanda? pedido)
    {
        if(pedido is null)return;
        AbrirPedidoSolicitado?.Invoke(pedido,false);
    }
    [RelayCommand] private void Atualizar()
    {
        try{_atendimento.AtualizarComandasAbertas();Pedidos.Refresh();Mensagem="";}
        catch{Mensagem="Não foi possível atualizar os pedidos. Tente novamente.";}
    }
}
