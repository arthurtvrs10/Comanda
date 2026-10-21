using System.Printing;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public sealed record ImpressoraWindows(string Nome, string Caminho, bool Compartilhada)
{
    public override string ToString() => Nome;
}

public sealed class JanelaImpressoras : Window
{
    private const string Caixa = "Impressora do Caixa (Este PC)";
    private readonly ConfiguracoesImpressaoService _service;
    private readonly PreferenciasImpressao _preferencias;
    private readonly Func<IReadOnlyList<ImpressoraWindows>> _consultar;
    private readonly Action<PreferenciaImpressora> _testar;
    private readonly Action? _salvo;
    private readonly ListBox _locais = new() { MinWidth = 190 };
    private readonly StackPanel _form = new() { Margin = new Thickness(18, 12, 12, 12) };
    private readonly TextBlock _mensagem = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private readonly Button _teste = new() { Content = "Testar impressora", MinWidth = 132, Margin = new Thickness(0, 0, 8, 0) };
    private IReadOnlyList<ImpressoraWindows> _impressoras = [];
    private string _snapshot;
    private bool _mostrando;
    public bool TemAlteracoes => _snapshot != JsonSerializer.Serialize(_preferencias);

    public JanelaImpressoras(ConfiguracoesImpressaoService service, Action? salvo = null,
        Func<IReadOnlyList<ImpressoraWindows>>? consultar = null, Action<PreferenciaImpressora>? testar = null)
    {
        _service = service; _preferencias = service.Carregar(); _consultar = consultar ?? ImpressaoWindows.Consultar; _testar = testar ?? ImpressaoWindows.Testar; _salvo = salvo;
        _snapshot = JsonSerializer.Serialize(_preferencias);
        Title = "Impressoras"; Width = Math.Min(1000, SystemParameters.WorkArea.Width); Height = Math.Min(620, SystemParameters.WorkArea.Height);
        MinWidth = 760; MinHeight = 470; FontSize = 12; ShowInTaskbar = false; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SetResourceReference(BackgroundProperty, "Canvas"); SetResourceReference(ForegroundProperty, "Ink");
        foreach (var tipo in new[] { typeof(Button), typeof(ComboBox), typeof(TextBox), typeof(RadioButton) })
        {
            var estilo = new Style(tipo, System.Windows.Application.Current?.TryFindResource(tipo) as Style);
            estilo.Setters.Add(new Setter(Control.FontSizeProperty, 12.0));
            estilo.Setters.Add(new Setter(MinHeightProperty, tipo == typeof(RadioButton) ? 26.0 : 28.0));
            estilo.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 4, 8, 4))); Resources[tipo] = estilo;
        }
        var raiz = new Grid { Margin = new Thickness(10) }; raiz.SetResourceReference(Panel.BackgroundProperty, "Canvas");
        raiz.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) }); raiz.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var area = new Grid(); area.ColumnDefinitions.Add(new() { Width = new GridLength(235) }); area.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        var lateral = new DockPanel(); var legenda = new TextBlock { Text = "Locais", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) }; DockPanel.SetDock(legenda, Dock.Top); lateral.Children.Add(legenda);
        lateral.Children.Add(_locais); area.Children.Add(lateral);
        foreach (var nome in new[] { Caixa }.Concat(_service.CarregarLocais().Select(x => x.Nome)))
        {
            var item = new ListBoxItem { Tag = nome, Padding = new Thickness(8), MinHeight = 44, HorizontalContentAlignment = HorizontalAlignment.Stretch };
            var linha = new DockPanel(); var icone = new Path { Data = Geometry.Parse("M3,8 L3,2 13,2 13,8 M3,12 L1,12 1,6 15,6 15,12 13,12 M3,10 L13,10 13,15 3,15 Z M11,8 L13,8"), Width = 18, Height = 18, Stretch = Stretch.Uniform, StrokeThickness = 1.2, Margin = new Thickness(0, 0, 10, 0) };
            icone.SetResourceReference(Shape.StrokeProperty, "Ink"); DockPanel.SetDock(icone, Dock.Left); linha.Children.Add(icone);
            linha.Children.Add(new TextBlock { Text = nome, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }); item.Content = linha;
            System.Windows.Automation.AutomationProperties.SetName(item, nome); _locais.Items.Add(item);
        }
        var scroll = new ScrollViewer { Content = _form, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }; Grid.SetColumn(scroll, 1); area.Children.Add(scroll);
        var moldura = new Border { Child = area, Padding = new Thickness(10), BorderThickness = new Thickness(1) }; moldura.SetResourceReference(Border.BorderBrushProperty, "Line"); raiz.Children.Add(moldura);
        var rodape = new DockPanel { Margin = new Thickness(0, 10, 0, 0) }; var botoes = new StackPanel { Orientation = Orientation.Horizontal };
        _teste.Click += (_, _) => Executar(Testar); botoes.Children.Add(_teste);
        var salvar = new Button { Content = "Salvar", MinWidth = 94, IsDefault = true }; salvar.Click += (_, _) => Executar(Salvar); botoes.Children.Add(salvar);
        DockPanel.SetDock(botoes, Dock.Right); rodape.Children.Add(botoes); rodape.Children.Add(_mensagem); Grid.SetRow(rodape, 1); raiz.Children.Add(rodape); Content = raiz;
        _locais.SelectionChanged += (_, _) => Mostrar(); _locais.SelectedIndex = 0;
        Loaded += (_, _) => AtualizarImpressoras();
        Closing += (_, e) => { if (TemAlteracoes && !new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?", "As alterações de impressora ainda não foram salvas. Deseja descartá-las e fechar?")) e.Cancel = true; };
    }
    private string LocalAtual => (_locais.SelectedItem as ListBoxItem)?.Tag as string ?? Caixa;
    private PreferenciaImpressora Atual => LocalAtual == Caixa ? _preferencias.Caixa : _preferencias.Cozinhas[LocalAtual];
    private void Executar(Action acao)
    {
        try { acao(); }
        catch (Exception ex) { _mensagem.Text = ex is ArgumentException or InvalidOperationException ? ex.Message : "Não foi possível acessar a impressora. Verifique a conexão, o nome e as permissões do Windows."; }
    }
    private void AtualizarImpressoras()
    {
        Executar(() => { _impressoras = _consultar(); _mensagem.Text = ""; Mostrar(); });
    }
    private void Mostrar()
    {
        _mostrando = true; _form.Children.Clear();
        var saida = Atual;
        _form.Children.Add(new TextBlock { Text = LocalAtual, FontSize = 17, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 14), TextWrapping = TextWrapping.Wrap });
        var modos = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        var opcoes = new List<(ModoImpressora Modo, string Texto)> { (ModoImpressora.Desativada, "Não utilizo"), (ModoImpressora.Instalada, "Impressoras instaladas"), (ModoImpressora.Compartilhada, "Compartilhadas") };
        if (LocalAtual != Caixa) opcoes.Add((ModoImpressora.MesmaDoCaixa, "Mesma do caixa"));
        foreach (var (modo, texto) in opcoes)
        {
            var radio = new RadioButton { Content = texto, IsChecked = saida.Modo == modo, GroupName = "SaidaImpressora", Margin = new Thickness(0, 0, 18, 0) };
            radio.Checked += (_, _) => { if (_mostrando) return; saida.Modo = modo; Mostrar(); }; modos.Children.Add(radio);
        }
        _form.Children.Add(modos);
        if (saida.Modo == ModoImpressora.Desativada)
        {
            _form.Children.Add(new TextBlock { Text = LocalAtual == Caixa ? "Impressão desabilitada." : "Somente monitor de preparo.", FontSize = 19, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10, 115, 10, 16), TextWrapping = TextWrapping.Wrap });
        }
        else if (saida.Modo == ModoImpressora.MesmaDoCaixa)
        {
            Texto(_preferencias.Caixa.Habilitada ? "Os cupons deste local usarão a impressora configurada para o caixa." : "A impressão do caixa está desabilitada. Configure-a para imprimir neste local.");
        }
        else
        {
            Texto(saida.Modo == ModoImpressora.Instalada ? "Selecione uma impressora instalada neste computador." : "Selecione uma conexão do Windows ou informe o caminho do compartilhamento.");
            var nomes = _impressoras.Where(x => x.Compartilhada == (saida.Modo == ModoImpressora.Compartilhada)).ToList();
            var seletor = new ComboBox { ItemsSource = nomes, DisplayMemberPath = nameof(ImpressoraWindows.Nome), SelectedItem = nomes.FirstOrDefault(x => x.Caminho.Equals(saida.Caminho, StringComparison.OrdinalIgnoreCase)), MaxWidth = 540, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 10) };
            System.Windows.Automation.AutomationProperties.SetName(seletor, "Impressora");
            seletor.SelectionChanged += (_, _) => { if (!_mostrando && seletor.SelectedItem is ImpressoraWindows selecionada) { saida.Caminho = selecionada.Caminho; Mostrar(); } }; _form.Children.Add(seletor);
            if (saida.Modo == ModoImpressora.Compartilhada)
            {
                _form.Children.Add(new Label { Content = @"Caminho (\\computador\impressora)", Padding = new Thickness(0, 0, 0, 3) });
                var caminho = new TextBox { Text = saida.Caminho, MaxLength = 260, Margin = new Thickness(0, 0, 0, 10) };
                System.Windows.Automation.AutomationProperties.SetName(caminho, "Caminho da impressora compartilhada"); caminho.TextChanged += (_, _) => saida.Caminho = caminho.Text.Trim(); _form.Children.Add(caminho);
            }
            if (nomes.Count == 0) Texto("Nenhuma impressora deste tipo encontrada no Windows.");
            if (!string.IsNullOrEmpty(saida.Caminho)) Texto("Selecionada: " + saida.Caminho);
            var atualizar = new Button { Content = "Atualizar impressoras", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 8, 0, 8) }; atualizar.Click += (_, _) => AtualizarImpressoras(); _form.Children.Add(atualizar);
            Texto("Salvar aplica a configuração. Para conferir a saída em papel, clique em Testar impressora.");
        }
        _teste.IsEnabled = saida.Habilitada && (saida.Modo != ModoImpressora.MesmaDoCaixa || _preferencias.Caixa.Habilitada);
        _mostrando = false;
    }
    private void Texto(string texto) => _form.Children.Add(new TextBlock { Text = texto, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) });
    private void Testar()
    {
        var configurada = Atual.Modo == ModoImpressora.MesmaDoCaixa ? _preferencias.Caixa : Atual;
        ConfiguracoesImpressaoService.Validar(configurada, false);
        if (!configurada.Habilitada) throw new InvalidOperationException("A impressão está desabilitada.");
        _testar(configurada); _mensagem.Text = "Teste enviado à fila de impressão do Windows.";
    }
    private void Salvar()
    {
        _service.Salvar(_preferencias, DateTime.UtcNow); _snapshot = JsonSerializer.Serialize(_preferencias); _salvo?.Invoke(); _mensagem.Text = "Configuração de impressoras salva.";
    }
}

public static class ImpressaoWindows
{
    public static IReadOnlyList<ImpressoraWindows> Consultar()
    {
        using var servidor = new LocalPrintServer();
        using var filas = servidor.GetPrintQueues([EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections]);
        var resultado = new List<ImpressoraWindows>();
        foreach (var fila in filas)
        {
            using (fila)
            {
                var remota = !fila.HostingPrintServer.Name.TrimStart('\\').Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);
                resultado.Add(new(fila.Name, fila.FullName, remota));
            }
        }
        return resultado.DistinctBy(x => x.Caminho, StringComparer.OrdinalIgnoreCase).OrderBy(x => x.Nome).ToArray();
    }
    public static void Testar(PreferenciaImpressora impressora) => ImprimirTexto(impressora,
        $"VARTHEX COMANDA\nTESTE DE IMPRESSÃO\n{DateTime.Now:dd/MM/yyyy HH:mm:ss}\n\nImpressora: {impressora.Caminho}\n\nSe você leu este cupom, a saída está funcionando.", "Teste de impressão Varthex");
    public static void ImprimirTexto(PreferenciaImpressora impressora, string texto, string titulo)
    {
        ConfiguracoesImpressaoService.Validar(impressora, false);
        if (!impressora.Habilitada) throw new InvalidOperationException("A impressão do caixa está desabilitada.");
        var caminho = impressora.Caminho;
        var partes = caminho.StartsWith(@"\\") ? caminho[2..].Split('\\', StringSplitOptions.RemoveEmptyEntries) : [];
        using PrintServer servidor = partes.Length == 2 ? new PrintServer(@"\\" + partes[0]) : new LocalPrintServer();
        using var fila = servidor.GetPrintQueue(partes.Length == 2 ? partes[1] : caminho);
        var dialogo = new PrintDialog { PrintQueue = fila, PrintTicket = fila.DefaultPrintTicket };
        var documento = new FlowDocument(new Paragraph(new Run(texto))) { FontFamily = new FontFamily("Consolas"), FontSize = 11, PagePadding = new Thickness(12), Foreground = Brushes.Black, Background = Brushes.White };
        documento.PageWidth = dialogo.PrintableAreaWidth; documento.PageHeight = dialogo.PrintableAreaHeight; documento.ColumnWidth = Math.Max(1, dialogo.PrintableAreaWidth - 24);
        dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator, titulo);
    }
}
