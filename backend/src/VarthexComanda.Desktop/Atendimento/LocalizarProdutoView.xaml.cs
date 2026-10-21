using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VarthexComanda.Desktop.Atendimento;

public partial class LocalizarProdutoView : Window
{
    public LocalizarProdutoViewModel ViewModel { get; }
    public LocalizarProdutoView(AtendimentoViewModel pedido)
    {
        InitializeComponent(); ViewModel = new(pedido); DataContext = ViewModel;
        Loaded += (_, _) => BuscaProduto.Focus();
        Closed += (_, _) => ViewModel.Dispose();
    }
    private void Adicionar_Click(object sender, RoutedEventArgs e) => ViewModel.AdicionarCommand.Execute(((Button)sender).Tag);
    private void Personalizar_Click(object sender, RoutedEventArgs e) => Personalizar(((Button)sender).Tag as ProdutoLocalizado);
    private void Personalizar(ProdutoLocalizado? produto)
    {
        if (produto is null) { ViewModel.Mensagem = "Selecione um produto."; return; }
        ViewModel.Selecionado = produto;
        new PersonalizarItemView(ViewModel, produto) { Owner = this }.ShowDialog();
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if(e.Key==Key.F && Keyboard.Modifiers==ModifierKeys.Control){BuscaProduto.Focus();BuscaProduto.SelectAll();e.Handled=true;}
        else if(e.Key==Key.F11 && Keyboard.Modifiers==ModifierKeys.None){if(!e.IsRepeat)ViewModel.AdicionarCommand.Execute(null);e.Handled=true;}
        else if(e.Key==Key.Enter && e.OriginalSource is not Button && Keyboard.Modifiers==ModifierKeys.None){if(!e.IsRepeat)Personalizar(ViewModel.Selecionado);e.Handled=true;}
    }
}
