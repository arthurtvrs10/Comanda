using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VarthexComanda.Desktop.Atendimento;

public sealed class EditarDadosComandaView : Window
{
    public EditarDadosComandaView(AtendimentoViewModel vm, string campo)
    {
        Title = campo switch { "mesa" => "Definir mesa", "mover" => "Mover para", "pessoas" => "Alterar quantidade de pessoas", _ => "Adicionar ou alterar pessoa" };
        Width = 420; SizeToContent = SizeToContent.Height; ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = TemaInterface.Pincel("#F2F2F0");
        var painel = new StackPanel { Margin = new Thickness(24) };
        Content = painel;
        var rotulo = campo switch { "mesa" => "Mesa (1 a 999). Vazio para atendimento no balcão.", "mover" => "Número da comanda de destino (deve estar livre)", "pessoas" => "Quantidade de pessoas na mesa", _ => "Nome do cliente (opcional)" };
        painel.Children.Add(new TextBlock { Text = rotulo, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,8) });
        var entrada = new TextBox { MaxLength = campo == "cliente" ? 80 : 9, Text = campo switch
        { "mesa" => vm.ComandaAtual!.NumeroMesa?.ToString() ?? "", "pessoas" => vm.ComandaAtual!.QuantidadePessoas.ToString(), "cliente" => vm.ComandaAtual!.NomeCliente ?? "", _ => "" } };
        System.Windows.Automation.AutomationProperties.SetName(entrada, rotulo);
        painel.Children.Add(entrada);
        var erro = new TextBlock { Foreground = TemaInterface.Pincel("#B22222"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,8) };
        painel.Children.Add(erro);
        var salvar = new Button { Content = campo == "mover" ? "Mover comanda" : "Salvar", IsDefault = true };
        salvar.Click += (_, _) => { if (vm.SalvarDadosComanda(campo, entrada.Text)) DialogResult = true; else erro.Text = vm.Mensagem; };
        painel.Children.Add(salvar);
        painel.Children.Add(new Button { Content = "Cancelar", IsCancel = true, Margin = new Thickness(0,8,0,0) });
        Loaded += (_, _) => { entrada.Focus(); entrada.SelectAll(); };
    }
}
