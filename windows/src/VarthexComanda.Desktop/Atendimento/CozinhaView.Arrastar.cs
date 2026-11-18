using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VarthexComanda.Desktop.Atendimento;

public partial class CozinhaView
{
    private Point _inicioArraste;
    private PedidoKds? _pedidoArraste;
    private bool _arrastando;
    private const string FormatoPedido="VComanda.Kds.Pedido";
    private void Cartao_MouseDown(object sender,MouseButtonEventArgs e)
    {
        _pedidoArraste=null;
        if(e.ChangedButton!=MouseButton.Left||sender is not Border {DataContext:PedidoKds pedido}||!ViewModel.PodeAlterar||pedido.Entregue)return;
        // Comandos e rolagens mantêm seu comportamento de clique.
        for(var atual=e.OriginalSource as DependencyObject;atual is not null&&atual!=sender;atual=atual is ContentElement conteudo?ContentOperations.GetParent(conteudo)??(conteudo as FrameworkContentElement)?.Parent:VisualTreeHelper.GetParent(atual))
            if(atual is System.Windows.Controls.Primitives.ButtonBase
                or System.Windows.Controls.Primitives.ScrollBar
                or System.Windows.Controls.Primitives.Thumb
                or System.Windows.Controls.Primitives.TextBoxBase
                or System.Windows.Controls.Primitives.Selector
                or System.Windows.Documents.Hyperlink)return;
        _inicioArraste=e.GetPosition(this);_pedidoArraste=pedido;
    }
    private void Cartao_MouseMove(object sender,MouseEventArgs e)
    {
        if(sender is Border cartaoHover)
        {
            var clicavel=false;
            for(var atual=e.OriginalSource as DependencyObject;atual is not null&&atual!=sender;atual=atual is ContentElement conteudo?ContentOperations.GetParent(conteudo)??(conteudo as FrameworkContentElement)?.Parent:VisualTreeHelper.GetParent(atual))
                if(atual is System.Windows.Controls.Primitives.ButtonBase or System.Windows.Controls.Primitives.ScrollBar or System.Windows.Controls.Primitives.Thumb or System.Windows.Controls.Primitives.TextBoxBase or System.Windows.Controls.Primitives.Selector or System.Windows.Documents.Hyperlink){clicavel=true;break;}
            cartaoHover.Cursor=!clicavel&&ViewModel.PodeAlterar&&cartaoHover.DataContext is PedidoKds {Entregue:false}?Cursors.Hand:Cursors.Arrow;
        }
        if(_arrastando||_pedidoArraste is null||e.LeftButton!=MouseButtonState.Pressed)return;
        var ponto=e.GetPosition(this);
        if(Math.Abs(ponto.X-_inicioArraste.X)<SystemParameters.MinimumHorizontalDragDistance&&Math.Abs(ponto.Y-_inicioArraste.Y)<SystemParameters.MinimumVerticalDragDistance)return;
        var pedido=_pedidoArraste;_pedidoArraste=null;_arrastando=true;
        try{if(sender is Border cartao)IniciarPrevia(cartao);DragDrop.DoDragDrop((DependencyObject)sender,new DataObject(FormatoPedido,pedido),DragDropEffects.Move);}
        finally{EncerrarPrevia();_arrastando=false;}
    }
    private void Coluna_DragOver(object sender,DragEventArgs e)
    {
        if(_previa is not null){_previa.Posicao=e.GetPosition(this);_previa.InvalidateVisual();}
        e.Effects=sender is Border {DataContext:ColunaCozinha coluna}&&e.Data.GetData(FormatoPedido) is PedidoKds pedido&&ViewModel.PodeMover(pedido,coluna)?DragDropEffects.Move:DragDropEffects.None;
        if(sender is Border borda)borda.SetResourceReference(Border.BorderBrushProperty,e.Effects==DragDropEffects.Move?"Accent":"AjudaLine");
        e.Handled=true;
    }
    private void Coluna_DragLeave(object sender,DragEventArgs e){if(sender is Border borda)borda.SetResourceReference(Border.BorderBrushProperty,"AjudaLine");}
    private async void Coluna_Drop(object sender,DragEventArgs e)
    {
        if(sender is not Border {DataContext:ColunaCozinha coluna} borda)return;
        borda.SetResourceReference(Border.BorderBrushProperty,"AjudaLine");e.Handled=true;
        if(e.Data.GetData(FormatoPedido) is PedidoKds pedido&&ViewModel.PodeMover(pedido,coluna))
        {
            await ViewModel.MoverAsync(pedido,coluna);
            if(!ViewModel.Falha&&coluna.Pedidos.Any(p=>p.Id==pedido.Id))
                await Dispatcher.InvokeAsync(()=>AnimarEntrada(borda,pedido.Id),System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }
}
