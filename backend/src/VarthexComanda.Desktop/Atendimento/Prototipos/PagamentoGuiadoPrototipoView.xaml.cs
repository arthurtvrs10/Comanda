using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VarthexComanda.Desktop.Atendimento.Prototipos;

public partial class PagamentoGuiadoPrototipoView : UserControl
{
    public event Action? VoltarSolicitado;
    public PagamentoGuiadoPrototipoView(PagamentoGuiadoPrototipoViewModel viewModel) { InitializeComponent();DataContext=viewModel; }
    private void Voltar_Click(object sender,RoutedEventArgs e)=>VoltarSolicitado?.Invoke();
    private void Acao_MouseDown(object sender,MouseButtonEventArgs e) { if(e.ClickCount>1)e.Handled=true; }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if(e.IsRepeat&&e.Key is Key.Enter or Key.Space)e.Handled=true;
    }
}
