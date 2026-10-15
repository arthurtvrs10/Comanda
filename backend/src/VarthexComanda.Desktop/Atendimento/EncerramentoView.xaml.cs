using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class EncerramentoView : Window
{
    private void Forma_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is EncerramentoViewModel vm && sender is RadioButton { CommandParameter: FormaPagamento forma })
            vm.SelecionarFormaCommand.Execute(forma);
    }

    private void Pagamento_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.None || DataContext is not EncerramentoViewModel vm) return;
        RadioButton? opcao = e.Key switch
        {
            Key.D1 or Key.NumPad1 => FormaDinheiro,
            Key.D2 or Key.NumPad2 => FormaPix,
            Key.D3 or Key.NumPad3 => FormaDebito,
            Key.D4 or Key.NumPad4 => FormaCredito,
            _ => null
        };
        if (opcao is not null)
        {
            e.Handled = true;
            if (e.IsRepeat || vm.Processando) return;
            opcao.IsChecked = true;
            opcao.Focus();
        }
        else if (e.Key == Key.Space)
        {
            e.Handled = true;
            if (e.IsRepeat || vm.Processando || !vm.FormaSelecionada.HasValue) return;
            vm.PagamentoRecebido = !vm.PagamentoRecebido;
            CaixaPagamentoRecebido.Focus();
        }
    }

    public EncerramentoView(EncerramentoViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Concluido += (sender, sucesso) =>
        {
            DialogResult = sucesso;
            Close();
        };
    }
}
