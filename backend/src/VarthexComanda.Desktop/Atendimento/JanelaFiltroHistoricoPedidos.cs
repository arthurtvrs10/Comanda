using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Atendimento;

public sealed class JanelaFiltroHistoricoPedidos : Window
{
    private readonly FiltroHistoricoPedidos _anterior;
    private readonly DateTime _hoje;
    private readonly Dictionary<StatusFiltroPedido, RadioButton> _status = [];
    private readonly ComboBox _data;
    private readonly DatePicker _de, _ate;
    private readonly TextBox _horaDe, _horaAte, _cliente, _colaborador, _codigos;
    private readonly CheckBox _cancelados;
    private readonly TextBlock _mensagem;
    public FiltroHistoricoPedidos? Resultado { get; private set; }

    public JanelaFiltroHistoricoPedidos(FiltroHistoricoPedidos anterior, DateTime hoje)
    {
        _anterior = anterior; _hoje = hoje.Date;
        Title = "Histórico de pedidos - Pesquisar"; Width = 500; Height = 635; MinWidth = 440; MinHeight = 480;
        UiHistorico.ConfigurarJanela(this);
        var raiz = new DockPanel { Margin = new Thickness(16, 14, 16, 12) }; Content = raiz;
        var rodape = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        var voltar = UiHistorico.Botao("Voltar"); voltar.IsCancel = true; rodape.Children.Add(voltar);
        var pesquisar = UiHistorico.Botao("Pesquisar", UiHistorico.Busca); pesquisar.IsDefault = true;
        pesquisar.Click += (_, _) => { if (Pesquisar() && IsVisible) DialogResult = true; }; rodape.Children.Add(pesquisar);
        DockPanel.SetDock(rodape, Dock.Bottom); raiz.Children.Add(rodape);
        var painel = new StackPanel { Margin = new Thickness(12) };
        var borda = new Border { BorderThickness = new Thickness(1), Child = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = painel } };
        borda.SetResourceReference(Border.BorderBrushProperty, "AjudaLine"); raiz.Children.Add(borda);
        painel.Children.Add(UiHistorico.Titulo("Status do pedido"));
        var radios = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2, Margin = new Thickness(0, 2, 0, 16) };
        foreach (var (valor, texto) in new[] { (StatusFiltroPedido.Todos, "Todos os pedidos"), (StatusFiltroPedido.FinalizadosEPagos, "Finalizados e pagos"), (StatusFiltroPedido.Abertos, "Abertos"), (StatusFiltroPedido.Finalizados, "Finalizados") })
        {
            var radio = new RadioButton { Content = texto, GroupName = "StatusPedido", IsChecked = anterior.Status == valor, MinHeight = 28, FontSize = 12 };
            _status.Add(valor, radio); radios.Children.Add(radio);
        }
        painel.Children.Add(radios); painel.Children.Add(UiHistorico.Titulo("Outros filtros"));
        _data = new ComboBox { ItemsSource = new[] { "Data de abertura", "Data de fechamento" }, SelectedIndex = (int)anterior.Data, MinHeight = 28, FontSize = 12, Margin = new Thickness(0, 3, 0, 7) };
        painel.Children.Add(UiHistorico.Rotulo("Data para o filtro", _data)); painel.Children.Add(_data);
        var atalhos = new System.Windows.Controls.Primitives.UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 10) };
        foreach (var texto in new[] { "Hoje", "Mês atual", "Últimos 7 dias", "Ontem", "Mês anterior", "Últimos 30 dias" })
        {
            var atalho = UiHistorico.Botao(texto); atalho.MinWidth = 0; atalho.MinHeight = 27; atalho.Padding = new Thickness(3);
            atalho.BorderThickness = new Thickness(0); atalho.Background = Brushes.Transparent;
            atalho.Click += (_, _) => AplicarPeriodo(texto); atalhos.Children.Add(atalho);
        }
        painel.Children.Add(atalhos);
        _de = new DatePicker { SelectedDate = anterior.De.Date, MinHeight = 28 };
        _ate = new DatePicker { SelectedDate = anterior.Ate.Date, MinHeight = 28 };
        _horaDe = UiHistorico.Campo(anterior.De.ToString("HH:mm")); _horaDe.Width = 48; _horaDe.MaxLength = 5;
        _horaAte = UiHistorico.Campo(anterior.Ate.ToString("HH:mm")); _horaAte.Width = 48; _horaAte.MaxLength = 5;
        var datas = new Grid { Margin = new Thickness(0, 0, 0, 14) }; datas.ColumnDefinitions.Add(new()); datas.ColumnDefinitions.Add(new());
        void Data(string nome, DatePicker dia, TextBox hora, int coluna)
        {
            var grupo = new StackPanel { Margin = new Thickness(0, 0, coluna == 0 ? 6 : 0, 0) };
            grupo.Children.Add(UiHistorico.Rotulo(nome, dia));
            var linha = new DockPanel(); DockPanel.SetDock(hora, Dock.Right); linha.Children.Add(hora); linha.Children.Add(dia); grupo.Children.Add(linha);
            System.Windows.Automation.AutomationProperties.SetName(hora, nome + " - horário (HH:mm)");
            Grid.SetColumn(grupo, coluna); datas.Children.Add(grupo);
        }
        Data("De", _de, _horaDe, 0); Data("Até", _ate, _horaAte, 1); painel.Children.Add(datas);
        painel.Children.Add(UiHistorico.Titulo("Outros filtros"));
        _cliente = UiHistorico.Campo(anterior.Cliente); _cliente.MaxLength = 80;
        _colaborador = UiHistorico.Campo(anterior.Colaborador); _colaborador.MaxLength = 120;
        var nomes = new Grid(); nomes.ColumnDefinitions.Add(new()); nomes.ColumnDefinitions.Add(new());
        foreach (var (rotulo, campo, coluna) in new[] { ("Nome do cliente", _cliente, 0), ("Nome do colaborador", _colaborador, 1) })
        {
            var grupo = new StackPanel { Margin = new Thickness(0, 0, coluna == 0 ? 6 : 0, 0) };
            grupo.Children.Add(UiHistorico.Rotulo(rotulo, campo)); grupo.Children.Add(campo); Grid.SetColumn(grupo, coluna); nomes.Children.Add(grupo);
        }
        painel.Children.Add(nomes);
        _codigos = UiHistorico.Campo(anterior.Codigos); _codigos.MaxLength = 12000;
        painel.Children.Add(UiHistorico.Rotulo("Código do pedido (ex.: 111, 222)", _codigos)); painel.Children.Add(_codigos);
        _cancelados = new CheckBox { Content = "Exibir pedidos cancelados", IsChecked = anterior.IncluirCancelados, FontSize = 12, Margin = new Thickness(0, 6, 0, 0) }; painel.Children.Add(_cancelados);
        _mensagem = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
        _mensagem.SetResourceReference(TextBlock.ForegroundProperty, "TextB71C1C"); painel.Children.Add(_mensagem);
    }

    public void AplicarPeriodo(string periodo)
    {
        var mes = new DateTime(_hoje.Year, _hoje.Month, 1);
        var (de, ate) = periodo switch
        {
            "Ontem" => (_hoje.AddDays(-1), _hoje.AddDays(-1)),
            "Mês atual" => (mes, mes.AddMonths(1).AddDays(-1)),
            "Mês anterior" => (mes.AddMonths(-1), mes.AddDays(-1)),
            "Últimos 7 dias" => (_hoje.AddDays(-6), _hoje),
            "Últimos 30 dias" => (_hoje.AddDays(-29), _hoje), _ => (_hoje, _hoje)
        };
        _de.SelectedDate = de; _ate.SelectedDate = ate; _horaDe.Text = "00:00"; _horaAte.Text = "23:59";
    }

    public bool Pesquisar()
    {
        if (_de.SelectedDate is not DateTime de || _ate.SelectedDate is not DateTime ate
            || !TimeSpan.TryParseExact(_horaDe.Text, @"hh\:mm", CultureInfo.InvariantCulture, out var horaDe)
            || !TimeSpan.TryParseExact(_horaAte.Text, @"hh\:mm", CultureInfo.InvariantCulture, out var horaAte))
        { _mensagem.Text = "Preencha as datas e os horários no formato HH:mm."; return false; }
        var filtro = _anterior with { De = de.Date + horaDe, Ate = ate.Date + horaAte, Status = _status.Single(p => p.Value.IsChecked == true).Key,
            Data = (DataFiltroPedido)_data.SelectedIndex, Cliente = _cliente.Text.Trim(), Colaborador = _colaborador.Text.Trim(), Codigos = _codigos.Text.Trim(), IncluirCancelados = _cancelados.IsChecked == true };
        try { filtro.Validar(); Resultado = filtro; _mensagem.Text = ""; return true; }
        catch (ArgumentException ex) { _mensagem.Text = ex.Message; return false; }
    }
}

internal static class UiHistorico
{
    internal static void ConfigurarJanela(Window janela)
    {
        DialogoCompacto.Aplicar(janela);
        janela.SetResourceReference(Control.BackgroundProperty, "AjudaCanvas");
        janela.SetResourceReference(Control.ForegroundProperty, "AjudaInk");
        foreach (var tipo in new[] { typeof(TextBox), typeof(ComboBox), typeof(DatePicker), typeof(GroupBox) })
        {
            var anterior = janela.TryFindResource(tipo) as Style;
            var estilo = new Style(tipo, anterior);
            estilo.Setters.Add(new Setter(Control.BackgroundProperty, new DynamicResourceExtension("AjudaSurface")));
            estilo.Setters.Add(new Setter(Control.ForegroundProperty, new DynamicResourceExtension("AjudaInk")));
            estilo.Setters.Add(new Setter(Control.BorderBrushProperty, new DynamicResourceExtension("AjudaLine")));
            janela.Resources[tipo] = estilo;
        }
    }
    internal const string Busca = "M11,3 A8,8 0 1 0 11,19 A8,8 0 1 0 11,3 M17,17 L22,22";
    internal const string Atualizar = "M20,9 A8,8 0 0 0 5,6 L3,9 M3,9 V3 M3,9 H9 M4,15 A8,8 0 0 0 19,18 L21,15 M21,15 V21 M21,15 H15";
    internal const string Colunas = "M2,3 H22 V21 H2 Z M8,3 V21 M16,3 V21 M2,8 H22";
    internal const string Excel = "M6,2 H17 L22,7 V22 H6 Z M17,2 V7 H22 M2,8 H13 V19 H2 Z M5,11 L10,16 M10,11 L5,16";
    internal const string Imprimir = "M6,8 V2 H18 V8 M6,17 H2 V8 H22 V17 H18 M6,13 H18 V22 H6 Z M18,10 H19";
    internal static Button Botao(string texto, string? desenho = null)
    {
        var conteudo = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        if (desenho is not null)
        {
            var icone = new System.Windows.Shapes.Path { Data = Geometry.Parse(desenho), StrokeThickness = 1.4, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round, Width = 22, Height = 22, Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            icone.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty, "RibbonSubtitle"); conteudo.Children.Add(icone);
        }
        conteudo.Children.Add(new TextBlock { Text = texto, VerticalAlignment = VerticalAlignment.Center });
        var botao = new Button { Content = conteudo, MinHeight = 36, MinWidth = 90, Padding = new Thickness(12, 5, 12, 5), FontSize = 12, Margin = new Thickness(0, 0, 6, 0) };
        System.Windows.Automation.AutomationProperties.SetName(botao, texto); return botao;
    }
    internal static TextBox Campo(string texto = "") => new() { Text = texto, MinHeight = 28, FontSize = 12, Padding = new Thickness(5, 3, 5, 3), Margin = new Thickness(0, 3, 0, 8) };
    internal static Label Rotulo(string texto, Control campo)
    {
        System.Windows.Automation.AutomationProperties.SetName(campo, texto);
        return new Label { Content = texto, Target = campo, Padding = new Thickness(0), FontSize = 12 };
    }
    internal static TextBlock Titulo(string texto) => new() { Text = texto, FontWeight = FontWeights.SemiBold, FontSize = 12, Margin = new Thickness(0, 4, 0, 8) };
}
