using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento.Prototipos;

public partial class PagamentoFluxoPrototipoView : UserControl
{
    public event Action? VoltarSolicitado;
    public PagamentoFluxoPrototipoView(PagamentoFluxoPrototipoViewModel viewModel)
    { InitializeComponent();DataContext=viewModel; }
    private void Voltar_Click(object sender,RoutedEventArgs e)=>VoltarSolicitado?.Invoke();
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if(e.IsRepeat||DataContext is not PagamentoFluxoPrototipoViewModel vm)return;
        if(e.Key==Key.Escape){VoltarSolicitado?.Invoke();e.Handled=true;return;}
        if(Keyboard.Modifiers!=ModifierKeys.Alt)return;
        var forma=e.SystemKey switch {Key.D1=>FormaPagamento.Dinheiro,Key.D2=>FormaPagamento.Pix,Key.D3=>FormaPagamento.Debito,Key.D4=>FormaPagamento.Credito,_=>(FormaPagamento?)null};
        if(forma is {} valor){vm.SelecionarFormaCommand.Execute(valor);ValorPagamento.Focus();ValorPagamento.SelectAll();e.Handled=true;}
    }
}
