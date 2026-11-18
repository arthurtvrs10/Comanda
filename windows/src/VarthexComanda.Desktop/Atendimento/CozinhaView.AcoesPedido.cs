using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop.Atendimento;
public partial class CozinhaView
{
    private async void AvancarPedido_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:PedidoKds pedido})return;
        var indice=pedido.Estado switch {"Pendente"=>2,"Em preparo"=>3,"Pronto"=>4,_=>-1};
        if(indice<0)return;
        await ViewModel.MoverAsync(pedido,ViewModel.Colunas[indice]);
    }
    public Action<int>? CancelarPedido { get;set; }
    public Action<int>? ImprimirPedidoKds { get;set; }
    private void CancelarPedido_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:PedidoKds pedido}||ViewModel.Autorizar?.Invoke()==false)return;
        Executar(()=>{CancelarPedido?.Invoke(pedido.Id);ViewModel.Atualizar();});
    }
    private void ImprimirPedido_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:PedidoKds pedido}||ViewModel.Autorizar?.Invoke()==false)return;
        Executar(()=>ImprimirPedidoKds?.Invoke(pedido.Id));
    }
}
