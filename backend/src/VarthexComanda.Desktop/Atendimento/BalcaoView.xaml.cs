using System.Windows.Controls;
using System.Windows.Input;

namespace VarthexComanda.Desktop.Atendimento;

public partial class BalcaoView : UserControl
{
    public BalcaoViewModel ViewModel { get; }
    public BalcaoView(BalcaoViewModel modelo){InitializeComponent();ViewModel=modelo;DataContext=modelo;}
    private void Tabela_MouseDoubleClick(object sender,MouseButtonEventArgs e)
    {
        if(ItemsControl.ContainerFromElement(TabelaPedidos,e.OriginalSource as System.Windows.DependencyObject) is DataGridRow)
            ViewModel.AbrirPedidoCommand.Execute(ViewModel.Selecionado);
    }
    private void Tabela_PreviewKeyDown(object sender,KeyEventArgs e)
    {
        if(e.Key!=Key.Enter)return;
        ViewModel.AbrirPedidoCommand.Execute(ViewModel.Selecionado);e.Handled=true;
    }
}
