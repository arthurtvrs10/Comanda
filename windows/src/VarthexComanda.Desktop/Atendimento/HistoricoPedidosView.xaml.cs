using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class HistoricoPedidosView : UserControl
{
    public HistoricoPedidosViewModel ViewModel { get; }
    public Action<Comanda>? AbrirComandaSolicitado { get; set; }
    private readonly Button _filtro, _exportar, _imprimir;
    private readonly Dictionary<string, DataGridColumn> _colunas = [];
    private bool _inicializando = true;

    public HistoricoPedidosView(HistoricoPedidosViewModel modelo)
    {
        InitializeComponent(); ViewModel = modelo; DataContext = modelo;
        _filtro = UiHistorico.Botao("", UiHistorico.Busca); _filtro.MaxWidth = 510; _filtro.Margin = new Thickness(0, 0, 8, 5);
        _filtro.ToolTip = "Pesquisar por status, período, cliente, colaborador e código (Ctrl+F)";
        System.Windows.Automation.AutomationProperties.SetName(_filtro, "Pesquisar histórico de pedidos");
        _filtro.Click += (_, _) => AbrirPesquisa(); PainelAcoes.Children.Add(_filtro);
        void Acao(string nome, string desenho, Action executar, out Button botao)
        {
            botao = UiHistorico.Botao(nome, desenho); botao.Margin = new Thickness(0, 0, 6, 5); botao.Click += (_, _) => executar(); PainelAcoes.Children.Add(botao);
        }
        Acao("Atualizar", UiHistorico.Atualizar, modelo.Atualizar, out _);
        Acao("Mais colunas", UiHistorico.Colunas, AbrirColunas, out _);
        Acao("Exportar Excel", UiHistorico.Excel, Exportar, out _exportar);
        Acao("Imprimir", UiHistorico.Imprimir, Imprimir, out _imprimir);
        foreach (var coluna in modelo.Colunas)
        {
            var vinculo = new Binding(coluna.Chave);
            if (coluna.Chave is "DataAbertura" or "DataFechamento") vinculo.StringFormat = "dd/MM/yyyy HH:mm";
            if (coluna.Chave == "Total") vinculo.StringFormat = "N2";
            var numero = coluna.Chave is "Codigo" or "Total" or "TotalItens" or "NumeroComanda" or "Mesa" or "Pessoas";
            var tabelaColuna = new DataGridTextColumn { Header = coluna.Titulo, Binding = vinculo, SortMemberPath = coluna.Chave,
                MinWidth = coluna.Chave.StartsWith("Data") ? 130 : coluna.Chave == "Cliente" ? 110 : 65,
                ElementStyle = (Style)FindResource(numero ? "NumeroTabela" : "TextoTabela") };
            _colunas.Add(coluna.Chave, tabelaColuna); TabelaPedidos.Columns.Add(tabelaColuna);
        }
        var botaoAbrir = new FrameworkElementFactory(typeof(Button));
        botaoAbrir.SetValue(ContentControl.ContentProperty, "Abrir"); botaoAbrir.SetValue(Control.MinHeightProperty, 28d);
        botaoAbrir.SetValue(Control.FontSizeProperty, 12d); botaoAbrir.SetValue(Control.PaddingProperty, new Thickness(5, 0, 5, 0));
        botaoAbrir.SetValue(Control.BorderThicknessProperty, new Thickness(0)); botaoAbrir.SetValue(Control.BackgroundProperty, Brushes.Transparent);
        botaoAbrir.SetBinding(System.Windows.Automation.AutomationProperties.NameProperty, new Binding("CodigoFormatado") { StringFormat = "Abrir pedido {0}" });
        botaoAbrir.AddHandler(Button.ClickEvent, new RoutedEventHandler((s, e) => { if (((FrameworkElement)s).DataContext is PedidoHistoricoResumo pedido) AbrirPedido(pedido); e.Handled = true; }));
        TabelaPedidos.Columns.Add(new DataGridTemplateColumn { Header = "Abrir", Width = 65, MinWidth = 65, CanUserSort = false, CanUserResize = false, CellTemplate = new DataTemplate { VisualTree = botaoAbrir } });
        AutoAjustarColunas.IsChecked = modelo.Preferencias.AutoAjustar;
        AplicarColunas(); AtualizarAcoes(); _inicializando = false;
        modelo.PropertyChanged += (_, e) => { if (e.PropertyName is nameof(modelo.ResumoFiltro) or nameof(modelo.Quantidade)) AtualizarAcoes(); };
    }

    private void AtualizarAcoes()
    {
        var texto = ((StackPanel)_filtro.Content).Children.OfType<TextBlock>().Single();
        texto.Text = ViewModel.ResumoFiltro; texto.MaxWidth = 430; texto.TextTrimming = TextTrimming.CharacterEllipsis;
        _filtro.ToolTip = ViewModel.ResumoFiltro + "\nClique para pesquisar (Ctrl+F).";
        _exportar.IsEnabled = _imprimir.IsEnabled = ViewModel.Quantidade > 0;
    }

    public void AbrirPesquisa()
    {
        var pesquisa = new JanelaFiltroHistoricoPedidos(ViewModel.Filtro, FusoBrasilia.ParaLocal(ViewModel.Relogio.UtcNow)) { Owner = Window.GetWindow(this) };
        if (pesquisa.ShowDialog() == true && pesquisa.Resultado is not null) ViewModel.Pesquisar(pesquisa.Resultado);
    }
    private void AbrirColunas()
    {
        var janela = new JanelaColunasPedidos(ViewModel) { Owner = Window.GetWindow(this) };
        if (janela.ShowDialog() != true) return;
        ViewModel.SalvarPreferencias(janela.ColunasSelecionadas, janela.GruposSelecionados, AutoAjustarColunas.IsChecked == true); AplicarColunas();
    }
    public void AplicarColunas()
    {
        foreach (var coluna in ViewModel.Colunas)
        {
            var tabelaColuna = _colunas[coluna.Chave]; tabelaColuna.Visibility = coluna.Visivel ? Visibility.Visible : Visibility.Collapsed;
            tabelaColuna.Width = new DataGridLength(coluna.Largura, ViewModel.Preferencias.AutoAjustar ? DataGridLengthUnitType.Star : DataGridLengthUnitType.Pixel);
        }
        var vista = CollectionViewSource.GetDefaultView(ViewModel.Pedidos);
        using (vista.DeferRefresh())
        {
            vista.GroupDescriptions.Clear();
            foreach (var grupo in ViewModel.Preferencias.AgruparPor) vista.GroupDescriptions.Add(new PropertyGroupDescription(grupo));
        }
    }
    private void AutoAjustar_Changed(object sender, RoutedEventArgs e)
    {
        if (_inicializando || ViewModel is null || _colunas.Count == 0) return;
        ViewModel.SalvarPreferencias(ViewModel.Colunas.Where(c => c.Visivel).Select(c => c.Chave), ViewModel.Preferencias.AgruparPor, AutoAjustarColunas.IsChecked == true); AplicarColunas();
    }
    private IReadOnlyList<ColunaHistoricoPedido> ColunasVisiveis() => ViewModel.Colunas.Where(c => c.Visivel).OrderBy(c => _colunas[c.Chave].DisplayIndex).ToArray();
    private PedidoHistoricoResumo[] PedidosOrdenados() => TabelaPedidos.Items.OfType<PedidoHistoricoResumo>().ToArray();

    private void Exportar()
    {
        if (ViewModel.Quantidade == 0) return;
        var dialogo = new SaveFileDialog { Title = "Exportar histórico de pedidos", FileName = "Pedidos-" + ViewModel.Filtro.De.ToString("yyyy-MM-dd") + ".xlsx", DefaultExt = ".xlsx", Filter = "Planilha Excel (*.xlsx)|*.xlsx", OverwritePrompt = true };
        if (dialogo.ShowDialog(Window.GetWindow(this)) != true) return;
        var temporario = dialogo.FileName + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var arquivo = File.Create(temporario)) RelatorioHistoricoPedidos.ExportarExcel(arquivo, ColunasVisiveis(), PedidosOrdenados());
            File.Move(temporario, dialogo.FileName, true); ViewModel.Mensagem = "Planilha exportada com sucesso.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or System.Xml.XmlException)
        { ViewModel.Mensagem = "Não foi possível exportar. Confira a pasta e feche a planilha se estiver em uso."; }
        finally { try { if (File.Exists(temporario)) File.Delete(temporario); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }
    private void Imprimir()
    {
        if (ViewModel.Quantidade == 0) return;
        var documento = RelatorioHistoricoPedidos.CriarDocumento(ViewModel.ResumoFiltro, ColunasVisiveis(), PedidosOrdenados());
        var janela = new Window { Title = "Imprimir histórico de pedidos", Width = 1000, Height = 680, MinWidth = 600, MinHeight = 400, Owner = Window.GetWindow(this) };
        UiHistorico.ConfigurarJanela(janela);
        var raiz = new DockPanel(); var imprimir = UiHistorico.Botao("Selecionar impressora e imprimir", UiHistorico.Imprimir);
        imprimir.HorizontalAlignment = HorizontalAlignment.Right; imprimir.Margin = new Thickness(10);
        DockPanel.SetDock(imprimir, Dock.Bottom); raiz.Children.Add(imprimir);
        documento.PageWidth = 1122; documento.PageHeight = 793;
        raiz.Children.Add(new FlowDocumentPageViewer { Document = documento }); janela.Content = raiz;
        imprimir.Click += (_, _) =>
        {
            try
            {
                var dialogo = new PrintDialog(); dialogo.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                if (dialogo.ShowDialog() != true) return;
                documento.PageWidth = dialogo.PrintableAreaWidth; documento.PageHeight = dialogo.PrintableAreaHeight;
                documento.ColumnWidth = Math.Max(1, dialogo.PrintableAreaWidth - 48);
                dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator, "Histórico de pedidos"); janela.Close();
            }
            catch (Exception ex) when (ex is System.Printing.PrintSystemException or InvalidOperationException or IOException)
            { JanelaAviso.Mostrar("Impressão", "Não foi possível imprimir. Confira a impressora e tente novamente.", TipoAviso.Erro); }
        };
        janela.ShowDialog();
    }

    public void AbrirPedido(PedidoHistoricoResumo pedido)
    {
        var detalhe = ViewModel.BuscarDetalhes(pedido); if (detalhe is null) return;
        var janela = new Window { Title = "Pedido " + pedido.CodigoFormatado, Width = 850, Height = 540, MinWidth = 650, MinHeight = 400, Owner = Window.GetWindow(this) };
        UiHistorico.ConfigurarJanela(janela);
        var raiz = new DockPanel { Margin = new Thickness(16) }; janela.Content = raiz;
        var rodape = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var voltar = UiHistorico.Botao("Voltar"); voltar.IsCancel = true; rodape.Children.Add(voltar);
        if (detalhe.Comanda.Status == StatusComanda.Aberta && AbrirComandaSolicitado is not null)
        {
            var abrir = UiHistorico.Botao("Abrir comanda"); abrir.Click += (_, _) => { janela.Close(); AbrirComandaSolicitado(detalhe.Comanda); }; rodape.Children.Add(abrir);
        }
        DockPanel.SetDock(rodape, Dock.Bottom); raiz.Children.Add(rodape);
        var cabecalho = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        cabecalho.Children.Add(new TextBlock { Text = $"{pedido.CodigoFormatado} · Comanda {pedido.NumeroComanda:00} · {pedido.Status}", FontWeight = FontWeights.SemiBold, FontSize = 16 });
        cabecalho.Children.Add(new TextBlock { Text = $"Abertura: {pedido.DataAbertura:dd/MM/yyyy HH:mm} · Criado por: {pedido.CriadoPor}", Margin = new Thickness(0, 5, 0, 5), TextWrapping = TextWrapping.Wrap });
        cabecalho.Children.Add(new TextBlock { Text = $"Cliente: {(pedido.Cliente.Length == 0 ? "Não informado" : pedido.Cliente)} · {pedido.Tipo} {(pedido.Mesa?.ToString() ?? "")} · Pagamento: {pedido.Pagamento}", TextWrapping = TextWrapping.Wrap });
        cabecalho.Children.Add(new TextBlock { Text = "Total: " + CentavosParaMoedaConverter.Formatar(detalhe.Comanda.TotalCentavos), FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) });
        DockPanel.SetDock(cabecalho, Dock.Top); raiz.Children.Add(cabecalho);
        var tabela = new DataGrid { AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false, HeadersVisibility = DataGridHeadersVisibility.Column, ItemsSource = detalhe.Itens, RowHeight = 34 };
        foreach (var (nome, campo, largura) in new[] { ("Produto", "NomeProduto", 220d), ("Quantidade", "QuantidadeFormatada", 85d), ("Medida", "Medida", 65d), ("Observação", "Observacao", 230d) })
            tabela.Columns.Add(new DataGridTextColumn { Header = nome, Binding = new Binding(campo), Width = largura, ElementStyle = (Style)FindResource("TextoTabela") });
        tabela.Columns.Add(new DataGridTextColumn { Header = "Subtotal", Binding = new Binding("SubtotalCentavos") { Converter = new CentavosParaMoedaConverter() }, Width = 100, ElementStyle = (Style)FindResource("NumeroTabela") });
        raiz.Children.Add(tabela); janela.ShowDialog();
    }
    private void Tabela_DoubleClick(object sender, MouseButtonEventArgs e)
    { if (e.OriginalSource is DependencyObject origem && ItensVisuais.ObterPai<DataGridRow>(origem) is not null && ViewModel.Selecionado is { } pedido) AbrirPedido(pedido); }
    private void Tabela_KeyDown(object sender, KeyEventArgs e)
    { if (e.Key == Key.Enter && ViewModel.Selecionado is { } pedido) { AbrirPedido(pedido); e.Handled = true; } }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.F5) { ViewModel.Atualizar(); e.Handled = true; }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F) { AbrirPesquisa(); e.Handled = true; }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E) { Exportar(); e.Handled = true; }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P) { Imprimir(); e.Handled = true; }
    }
}

internal static class ItensVisuais
{
    internal static T? ObterPai<T>(DependencyObject? objeto) where T : DependencyObject
    {
        while (objeto is not null) { if (objeto is T encontrado) return encontrado; objeto = objeto is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(objeto) : LogicalTreeHelper.GetParent(objeto); }
        return null;
    }
}
