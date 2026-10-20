using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace VarthexComanda.Desktop.Catalogo;

public partial class ProdutosView : UserControl
{
    private Window? _dialogo;

    public ProdutosView(ProdutosViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (ModalProduto.Visibility == Visibility.Visible)
        {
            if (e.Key == Key.Escape) { FecharModal(); e.Handled = true; }
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)) e.Handled = true;
            return;
        }
        if (!e.Handled && e.Key == Key.F
            && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
            && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            CampoBuscaProduto.Focus();
            CampoBuscaProduto.SelectAll();
            e.Handled = true;
        }
    }

    private void NovoProduto_Click(object sender, RoutedEventArgs e)
    {
        ((ProdutosViewModel)DataContext).NovoCommand.Execute(null);
        AbrirModal();
    }

    public void AbrirNovoProduto() => NovoProduto_Click(this, new RoutedEventArgs());

    private void EditarProduto_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: VarthexComanda.Domain.Produto produto })
        {
            ((ProdutosViewModel)DataContext).ProdutoSelecionado = produto;
            AbrirModal();
        }
    }

    private void AbrirModal()
    {
        EncerrarNovaCategoria();
        var vm = (ProdutosViewModel)DataContext;
        vm.AdiarFotoAteSalvar = true;
        var formulario = (Border)ModalProduto.Children[0];
        ModalProduto.Children.Remove(formulario);
        formulario.Width = double.NaN;
        formulario.Margin = new Thickness(0);
        formulario.HorizontalAlignment = HorizontalAlignment.Stretch;
        formulario.VerticalAlignment = VerticalAlignment.Stretch;
        _dialogo = new Window
        {
            Title = vm.TemProdutoSelecionado ? "Editar produto" : "Novo produto",
            Owner = Window.GetWindow(this), WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Width = 620, Height = Math.Min(660, SystemParameters.WorkArea.Height - 40),
            MinWidth = 480, MinHeight = 420, ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false, Content = formulario, DataContext = vm,
            Background = TemaInterface.Pincel("#FFFFFF")
        };
        _dialogo.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                if (CampoNovaCategoria.Visibility == Visibility.Visible) EncerrarNovaCategoria();
                else _dialogo.Close();
                e.Handled = true;
            }
        };
        _dialogo.ContentRendered += (_, _) => CampoNomeProduto.Focus();
        try { _dialogo.ShowDialog(); }
        finally
        {
            _dialogo.Content = null;
            _dialogo = null;
            ModalProduto.Children.Add(formulario);
            vm.AdiarFotoAteSalvar = false;
            vm.NovoCommand.Execute(null);
            CampoBuscaProduto.Focus();
        }
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) => FecharModal();
    private void FecharModal() => _dialogo?.Close();

    private void Gravar_Click(object sender, RoutedEventArgs e)
    {
        // O comando do botão é executado depois de Click.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var vm = (ProdutosViewModel)DataContext;
            if (vm.ProdutoSelecionado is null && string.IsNullOrEmpty(vm.Mensagem)
                && string.IsNullOrEmpty(vm.NomeProduto)) FecharModal();
        }));
    }

    private void NovaCategoria_Click(object sender, RoutedEventArgs e)
    {
        if (CampoNovaCategoria.Visibility == Visibility.Visible) { ConfirmarNovaCategoria(); return; }
        ((ProdutosViewModel)DataContext).NovaCategoriaNome = string.Empty;
        SeletorCategoriaProduto.Visibility = Visibility.Collapsed;
        CampoNovaCategoria.Visibility = Visibility.Visible;
        BotaoNovaCategoria.ToolTip = "Confirmar nova categoria (Enter)";
        System.Windows.Automation.AutomationProperties.SetName(BotaoNovaCategoria, "Confirmar nova categoria");
        ((System.Windows.Shapes.Path)BotaoNovaCategoria.Content).Data = System.Windows.Media.Geometry.Parse("M0,8 L6,14 L16,2");
        CampoNovaCategoria.Focus();
    }

    private void CampoNovaCategoria_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { ConfirmarNovaCategoria(); e.Handled = true; }
    }

    private void ConfirmarNovaCategoria()
    {
        var vm = (ProdutosViewModel)DataContext;
        vm.AdicionarCategoriaCommand.Execute(null);
        if (string.IsNullOrEmpty(vm.Mensagem)) EncerrarNovaCategoria();
    }

    private void EncerrarNovaCategoria()
    {
        CampoNovaCategoria.Visibility = Visibility.Collapsed;
        SeletorCategoriaProduto.Visibility = Visibility.Visible;
        BotaoNovaCategoria.ToolTip = "Adicionar nova categoria";
        System.Windows.Automation.AutomationProperties.SetName(BotaoNovaCategoria, "Adicionar nova categoria");
        ((System.Windows.Shapes.Path)BotaoNovaCategoria.Content).Data = System.Windows.Media.Geometry.Parse("M8,0 V16 M0,8 H16");
        SeletorCategoriaProduto.Focus();
    }

    private void EscolherFoto_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Escolher foto do produto",
            Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (dialogo.ShowDialog() == true && DataContext is ProdutosViewModel viewModel)
        {
            viewModel.DefinirFoto(dialogo.FileName);
        }
    }
}
