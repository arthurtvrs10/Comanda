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
        var janela = new Window { Title="Adicionar produto",Owner=this,Width=420,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false };
        var painel = new StackPanel { Margin=new Thickness(20) };janela.Content=painel;
        painel.Children.Add(new TextBlock { Text=produto.Nome,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12) });
        var quantidade = new TextBox { Text="1",MaxLength=4 };painel.Children.Add(new Label { Content="Quantidade",Target=quantidade });painel.Children.Add(quantidade);
        var observacao = new TextBox { Text=ViewModel.Pedido.ObservacaoNovoItem,MaxLength=300,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,Height=76 };painel.Children.Add(new Label { Content="Observação do produto",Target=observacao });painel.Children.Add(observacao);
        var erro = new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,8) };painel.Children.Add(erro);
        var adicionar = new Button { Content="Adicionar ao pedido",IsDefault=true };painel.Children.Add(adicionar);
        adicionar.Click+=(_,_)=>
        {
            if(!int.TryParse(quantidade.Text,out var numero)||numero is <1 or >999){erro.Text="Informe uma quantidade entre 1 e 999.";return;}
            if(ViewModel.Lancar(produto,numero,observacao.Text))janela.DialogResult=true;else erro.Text=ViewModel.Mensagem;
        };
        painel.Children.Add(new Button { Content="Cancelar",IsCancel=true,Margin=new Thickness(0,8,0,0) });janela.ShowDialog();
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if(e.Key==Key.F && Keyboard.Modifiers==ModifierKeys.Control){BuscaProduto.Focus();BuscaProduto.SelectAll();e.Handled=true;}
        else if(e.Key==Key.F11 && Keyboard.Modifiers==ModifierKeys.None){if(!e.IsRepeat)ViewModel.AdicionarCommand.Execute(null);e.Handled=true;}
        else if(e.Key==Key.Enter && e.OriginalSource is not Button && Keyboard.Modifiers==ModifierKeys.None){if(!e.IsRepeat)Personalizar(ViewModel.Selecionado);e.Handled=true;}
    }
}
