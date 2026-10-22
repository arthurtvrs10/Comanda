using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public enum SaidaPedido { Conta, ContaBloquear, CozinhaNovos, CozinhaTodos, Fichas }

public partial class AtendimentoView
{
    public Func<SaidaPedido, bool>? ImprimirPedido { get; set; }
    private bool _atualizandoLateral;
    private int? _comandaLateralId;
    private string TituloPedido() => ViewModel.ComandaAtual is not { } c ? "Comanda" :
        $"{(c.NumeroMesa is int mesa ? $"Mesa: {mesa}" : $"Comanda: {c.Numero:00}")} ({ViewModel.IdentificacaoPedido})";

    private void AtualizarDadosLaterais()
    {
        if (ViewModel.ComandaAtual is not { } c) return;
        if (_comandaLateralId == c.Id && Equals(ResponsavelPedido.SelectedItem, c.Responsavel ?? "Não informado")) return;
        _atualizandoLateral = true;
        try
        {
            ResponsavelPedido.ItemsSource = (ViewModel.ObterResponsaveis?.Invoke() ?? [])
                .Append(c.Responsavel ?? "Não informado").Distinct().ToArray();
            ResponsavelPedido.SelectedItem = c.Responsavel ?? "Não informado";
            _comandaLateralId = c.Id;
        }
        finally { _atualizandoLateral = false; }
    }
    private void ResponsavelPedido_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_atualizandoLateral && ViewModel.ComandaAtual is not null && ResponsavelPedido.SelectedItem is string nome
            && !ViewModel.SalvarAtendimento(nome == "Não informado" ? null : nome, ObservacaoPedido.Text))
            AtualizarDadosLaterais();
    }
    private void ObservacaoPedido_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        SalvarObservacaoPendente();
    }
    private bool SalvarObservacaoPendente() => !ViewModel.SalvandoFechamento && (ViewModel.ComandaAtual is not { } c
        || c.Observacao == ObservacaoPedido.Text || (string.IsNullOrEmpty(c.Observacao) && ObservacaoPedido.Text.Length == 0)
        || ViewModel.SalvarAtendimento(c.Responsavel, ObservacaoPedido.Text));
    private async void BloqueioPedido_Click(object sender, RoutedEventArgs e)
    {
        if (!SalvarObservacaoPendente()) { BloqueioPedido.IsChecked=ViewModel.ComandaAtual?.Bloqueada; return; }
        try { await ViewModel.DefinirFechamentoAsync(BloqueioPedido.IsChecked == true); }
        finally { BloqueioPedido.IsChecked = ViewModel.ComandaAtual?.Bloqueada; }
    }
    private void ProdutosPedido_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.PodeAlterarPedido || !SalvarObservacaoPendente()) return;
        new LocalizarProdutoView(ViewModel) { Owner=_janelaComanda ?? Window.GetWindow(this) }.ShowDialog();
    }
    private void PrioridadePedido_Click(object sender, RoutedEventArgs e) => CriarMenuPrioridade((Button)sender).IsOpen = true;
    internal ContextMenu CriarMenuPrioridade(Button botao)
    {
        var menu = MenuPedido(botao);
        foreach(var prioridade in Enum.GetValues<PrioridadeComanda>())
        {
            var item=Acao(menu,AtendimentoViewModel.NomePrioridade(prioridade),()=>ViewModel.AlterarPrioridadeCommand.Execute(prioridade));
            item.IsCheckable=true;item.IsChecked=ViewModel.ComandaAtual?.Prioridade==prioridade;
            var cabecalho=new StackPanel { Orientation=Orientation.Horizontal };
            cabecalho.Children.Add(new System.Windows.Shapes.Ellipse { Width=10,Height=10,Margin=new Thickness(0,0,10,0),Fill=new SolidColorBrush((Color)ColorConverter.ConvertFromString(prioridade switch {PrioridadeComanda.Urgente=>"#B6322C",PrioridadeComanda.Alta=>"#F2CB59",_=>"#2D8050"})) });
            cabecalho.Children.Add(new TextBlock { Text=AtendimentoViewModel.NomePrioridade(prioridade)+(item.IsChecked?" (atual)":""),VerticalAlignment=VerticalAlignment.Center });
            item.Header=cabecalho;
        }
        return menu;
    }

    private ContextMenu MenuPedido(Button botao)
    {
        var menu = new ContextMenu { PlacementTarget = botao, Placement = PlacementMode.Top, MinWidth = 284, Padding = new Thickness(6), FontSize = 13 };
        menu.Style = (Style)FindResource("MenuPedidoContexto");
        menu.MaxHeight = Math.Max(240, SystemParameters.WorkArea.Height - 48);
        menu.SetResourceReference(BackgroundProperty, "AjudaCanvas");
        menu.SetResourceReference(ForegroundProperty, "AjudaInk");
        menu.SetResourceReference(BorderBrushProperty, "AjudaLine");
        menu.BorderThickness = new Thickness(1);
        return menu;
    }
    private MenuItem Acao(ItemsControl menu, string texto, Action executar, bool habilitada = true)
    {
        var item = new MenuItem { Header = texto, MinHeight = 42, Padding = new Thickness(14,8,14,8), IsEnabled = habilitada };
        item.Style = (Style)FindResource("MenuPedidoItem");
        System.Windows.Automation.AutomationProperties.SetName(item, texto);
        item.SetResourceReference(BackgroundProperty, "AjudaCanvas");
        item.SetResourceReference(ForegroundProperty, "AjudaInk");
        item.Click += (_, _) => executar();
        menu.Items.Add(item);
        return item;
    }
    internal ContextMenu CriarMenuMaisOpcoes(Button botao)
    {
        var menu = MenuPedido(botao);
        var pode = ViewModel.PodeAlterarPedido;
        Acao(menu, "Trocar para…", () => new EditarDadosComandaView(ViewModel, "mover") { Owner = _janelaComanda ?? Window.GetWindow(this) }.ShowDialog(), pode);
        Acao(menu, "Visualizar conta resumida", () => Imprimir(SaidaPedido.Conta));
        Acao(menu, "Transferir / copiar itens para outro pedido", TransferirCopiarItens, pode && ViewModel.Itens.Count > 0);
        var novos = ViewModel.Itens.Sum(i => i.UnidadeVenda == VarthexComanda.Domain.UnidadeVenda.Unidade ? Math.Max(0, i.Quantidade - i.QuantidadeFicha) : i.Quantidade > i.QuantidadeFicha ? 1 : 0);
        Acao(menu, $"Imprimir fichas de consumação ({novos} nova(s))", () => Imprimir(SaidaPedido.Fichas), novos > 0);
        Acao(menu, "Enviar para WhatsApp", CompartilharPedido, ViewModel.Itens.Count > 0);
        Acao(menu, "Recalcular pedido", () => ViewModel.RecalcularPedido());
        menu.Items.Add(new Separator());
        var dados = Acao(menu, "Dados do pedido", () => { });
        Acao(dados, "Definir mesa", () => new EditarDadosComandaView(ViewModel, "mesa") { Owner = _janelaComanda ?? Window.GetWindow(this) }.ShowDialog());
        Acao(dados, "Alterar quantidade de pessoas", () => new EditarDadosComandaView(ViewModel, "pessoas") { Owner = _janelaComanda ?? Window.GetWindow(this) }.ShowDialog());
        var prioridades = Acao(dados, "Prioridade", () => { });
        foreach (var prioridade in Enum.GetValues<PrioridadeComanda>())
            Acao(prioridades, AtendimentoViewModel.NomePrioridade(prioridade), () => ViewModel.AlterarPrioridadeCommand.Execute(prioridade));
        Acao(dados, "Origem do cliente", () => OrigemCliente_Click(botao, new RoutedEventArgs()));
        menu.Items.Add(new Separator());
        Acao(menu, "Excluir pedido…", () => ViewModel.CancelarComandaAtualCommand.Execute(null));
        menu.Items.Add(new Separator());
        Acao(menu, "Cancelar", () => menu.IsOpen = false);
        return menu;
    }
    private void MaisOpcoesPedido_Click(object sender, RoutedEventArgs e) => CriarMenuMaisOpcoes((Button)sender).IsOpen = true;

    internal ContextMenu CriarMenuImprimir(Button botao)
    {
        var menu = MenuPedido(botao);
        var temItens = ViewModel.Itens.Count > 0;
        Acao(menu, "Imprimir conta", () => Imprimir(SaidaPedido.Conta), temItens);
        Acao(menu, "Imprimir conta e iniciar fechamento", () => Imprimir(SaidaPedido.ContaBloquear), temItens);
        menu.Items.Add(new Separator());
        var novos = ViewModel.Itens.Sum(i => Math.Max(0, i.Quantidade - i.QuantidadeCozinha));
        Acao(menu, novos > 0 ? $"Imprimir na cozinha ({novos} novo(s))" : "Não há itens novos para imprimir na cozinha", () => Imprimir(SaidaPedido.CozinhaNovos), novos > 0);
        Acao(menu, "Reimprimir na cozinha", () => Imprimir(SaidaPedido.CozinhaTodos), temItens);
        menu.Items.Add(new Separator());
        Acao(menu, "Cancelar", () => menu.IsOpen = false);
        return menu;
    }
    private void ImprimirPedido_Click(object sender, RoutedEventArgs e) => CriarMenuImprimir((Button)sender).IsOpen = true;
    private void Imprimir(SaidaPedido saida)
    {
        try
        {
            if (ImprimirPedido is null) { ViewModel.Mensagem = "A impressão não está disponível nesta janela."; return; }
            if (ImprimirPedido(saida)) ViewModel.Mensagem = "Impressão enviada. Confira a saída na impressora.";
        }
        catch (InvalidOperationException ex) { ViewModel.Mensagem = ex.Message; }
        catch (Exception) { ViewModel.Mensagem = "Não foi possível concluir a impressão. Confira a impressora e tente novamente."; }
    }
    private void OpcoesItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ItemComanda item } botao) return;
        var menu = MenuPedido(botao);
        Acao(menu, "Aumentar quantidade", () => ViewModel.AumentarQuantidadeCommand.Execute(item), ViewModel.PodeAlterarPedido);
        Acao(menu, "Diminuir quantidade", () => ViewModel.DiminuirQuantidadeCommand.Execute(item), ViewModel.PodeAlterarPedido);
        Acao(menu, "Remover item…", () => ViewModel.RemoverCommand.Execute(item), ViewModel.PodeAlterarPedido);
        menu.Items.Add(new Separator());
        Acao(menu, "Cancelar", () => menu.IsOpen = false);
        menu.IsOpen = true;
    }

    private void TransferirCopiarItens()
    {
        ViewModel.AtualizarComandasAbertas();
        var destinos = ViewModel.ComandasAbertas.Where(c => c.Id != ViewModel.ComandaAtual?.Id && !c.Bloqueada).ToArray();
        var janela = new Window { Title = "Transferir ou copiar itens", Owner = _janelaComanda ?? Window.GetWindow(this), Width = 540, Height = 580, MinWidth = 460, MinHeight = 440, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var raiz = new DockPanel { Margin = new Thickness(20) };
        var topo = new StackPanel(); DockPanel.SetDock(topo, Dock.Top); raiz.Children.Add(topo);
        topo.Children.Add(new TextBlock { Text = "Comanda de destino", Margin = new Thickness(0,0,0,6) });
        var destino = new ComboBox { ItemsSource = destinos.Select(c => new { c.Id, Texto = $"Comanda {c.Numero:00} · {CentavosParaMoedaConverter.Formatar(c.TotalCentavos)}" }).ToArray(), DisplayMemberPath = "Texto", SelectedValuePath = "Id", MinHeight = 36 };
        System.Windows.Automation.AutomationProperties.SetName(destino, "Comanda de destino"); topo.Children.Add(destino);
        var aviso = new TextBlock { Text = destinos.Length == 0 ? "Abra outra comanda fora de fechamento antes de transferir ou copiar os itens." : "Selecione os itens. A transferência mantém os preços e libera os itens da origem; a cópia mantém os dois pedidos.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,12,0,12) }; topo.Children.Add(aviso);
        var rodape = new StackPanel(); DockPanel.SetDock(rodape, Dock.Bottom); raiz.Children.Add(rodape);
        var copiar = new CheckBox { Content = "Copiar itens e manter na origem", Margin = new Thickness(0,12,0,12) }; rodape.Children.Add(copiar);
        var erro = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,8) }; rodape.Children.Add(erro);
        var botoes = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        rodape.Children.Add(botoes); botoes.Children.Add(new Button { Content = "Cancelar", IsCancel = true, Margin = new Thickness(0,0,8,0) });
        var confirmar = new Button { Content = "Confirmar", IsDefault = true, IsEnabled = destinos.Length > 0 }; botoes.Children.Add(confirmar);
        var lista = new StackPanel(); var selecoes = new List<(CheckBox Caixa, int Id)>();
        foreach (var item in ViewModel.Itens)
        {
            var caixa = new CheckBox { Content = $"{item.QuantidadeFormatada} {item.Medida} × {item.NomeProduto} · {CentavosParaMoedaConverter.Formatar(item.SubtotalCentavos)}", IsChecked = true, Margin = new Thickness(0,6,0,6) };
            lista.Children.Add(caixa); selecoes.Add((caixa, item.Id));
        }
        raiz.Children.Add(new ScrollViewer { Content = lista, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }); janela.Content = raiz;
        confirmar.Click += (_, _) =>
        {
            if (destino.SelectedValue is not int destinoId) { erro.Text = "Selecione a comanda de destino."; return; }
            var ids = selecoes.Where(s => s.Caixa.IsChecked == true).Select(s => s.Id).ToArray();
            if (ids.Length == 0) { erro.Text = "Selecione pelo menos um item."; return; }
            if (!new JanelaConfirmador().Confirmar("Confirmar operação", $"{(copiar.IsChecked == true ? "Copiar" : "Transferir")} {ids.Length} item(ns) para {destino.Text}?")) return;
            if (ViewModel.TransferirItens(destinoId, ids, copiar.IsChecked == true)) janela.DialogResult = true;
            else erro.Text = ViewModel.Mensagem;
        };
        janela.ShowDialog();
    }

    private void CompartilharPedido()
    {
        var janela = new Window { Title = "Enviar conta para WhatsApp", Owner = _janelaComanda ?? Window.GetWindow(this), Width = 440, SizeToContent = SizeToContent.Height, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var painel = new StackPanel { Margin = new Thickness(20) }; janela.Content = painel;
        painel.Children.Add(new TextBlock { Text = "WhatsApp do cliente (DDD e número)", Margin = new Thickness(0,0,0,6) });
        var telefone = new TextBox { MaxLength = 24 }; painel.Children.Add(telefone);
        System.Windows.Automation.AutomationProperties.SetName(telefone, "WhatsApp do cliente");
        painel.Children.Add(new TextBlock { Text = "A conta será aberta como rascunho no WhatsApp. Revise e envie para o cliente.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,12,0,12) });
        var erro = new TextBlock { TextWrapping = TextWrapping.Wrap }; painel.Children.Add(erro);
        var abrir = new Button { Content = "Abrir WhatsApp", IsDefault = true, Margin = new Thickness(0,12,0,0) }; painel.Children.Add(abrir);
        painel.Children.Add(new Button { Content = "Cancelar", IsCancel = true, Margin = new Thickness(0,8,0,0) });
        abrir.Click += (_, _) =>
        {
            var numero = new string(telefone.Text.Where(char.IsAsciiDigit).ToArray());
            if (numero.Length is 10 or 11) numero = "55" + numero;
            if (numero.Length is < 12 or > 13 || !numero.StartsWith("55")) { erro.Text = "Informe um número brasileiro com DDD."; return; }
            var texto = $"{ViewModel.IdentificacaoPedido}\nComanda {ViewModel.ComandaAtual!.Numero:00}\n" + string.Join("\n", ViewModel.Itens.Select(i => $"{i.QuantidadeFormatada} {i.Medida} × {i.NomeProduto}: {CentavosParaMoedaConverter.Formatar(i.SubtotalCentavos)}")) + $"\nTotal: {ViewModel.TotalPedido}\nConferência da conta · Não fiscal";
            try { Process.Start(new ProcessStartInfo($"https://wa.me/{numero}?text={Uri.EscapeDataString(texto)}") { UseShellExecute = true }); janela.DialogResult = true; }
            catch { erro.Text = "Não foi possível abrir o WhatsApp. Confira o navegador e tente novamente."; }
        };
        janela.ShowDialog();
    }
}

public sealed class DetalheItemPedidoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ItemComanda i) return "";
        var cozinha = i.QuantidadeCozinha >= i.Quantidade ? "Cozinha: impresso" : "Cozinha: pendente";
        return $"Lançado por {i.LancadoPor ?? "não informado"} em {FusoBrasilia.ParaLocal(i.CriadoEm):dd/MM HH:mm} · {cozinha}";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
