using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class AtendimentoView : UserControl
{
    public AtendimentoViewModel ViewModel { get; }
    public Action? ImpressaoSolicitada { get; set; }
    public Func<IReadOnlyList<string>>? OrigensCliente { get; set; }
    private void OrigemCliente_Click(object sender,RoutedEventArgs e)
    {
        if(ViewModel.ComandaAtual is null)return;
        var janela=new Window { Title="Origem do cliente",Owner=_janelaComanda??Window.GetWindow(this),Width=420,SizeToContent=SizeToContent.Height,WindowStartupLocation=WindowStartupLocation.CenterOwner,ResizeMode=ResizeMode.NoResize };
        var painel=new StackPanel { Margin=new Thickness(24) };janela.Content=painel;
        var lista=new ComboBox { ItemsSource=(OrigensCliente?.Invoke()??new[]{"Indefinido"}).Append(ViewModel.ComandaAtual.OrigemCliente??"Indefinido").Distinct().ToArray(),SelectedItem=ViewModel.ComandaAtual.OrigemCliente??"Indefinido",MinHeight=46 };
        painel.Children.Add(new Label { Content="Como o cliente conheceu o estabelecimento?",Target=lista });painel.Children.Add(lista);
        var erro=new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,8) };painel.Children.Add(erro);
        var salvar=new Button { Content="Salvar",IsDefault=true };salvar.Click+=(_,_)=>{if(ViewModel.SalvarDadosComanda("origem",lista.SelectedItem as string??"Indefinido"))janela.DialogResult=true;else erro.Text=ViewModel.Mensagem;};painel.Children.Add(salvar);
        painel.Children.Add(new Button { Content="Cancelar",IsCancel=true,Margin=new Thickness(0,8,0,0) });janela.ShowDialog();
    }
    private Window? _janelaComanda;
    private bool _abrindoJanelaComanda;

    private void AgendarJanelaComanda()
    {
        if (ViewModel.ComandaAtual is null)
        {
            _janelaComanda?.Close();
            return;
        }
        if (_janelaComanda is not null)
        {
            _janelaComanda.Title = TituloPedido();
            AtualizarDadosLaterais();
            return;
        }
        if (_abrindoJanelaComanda || !IsLoaded || !IsVisible) return;
        _abrindoJanelaComanda = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            _abrindoJanelaComanda = false;
            if (ViewModel.ComandaAtual is null || _janelaComanda is not null || !IsVisible) return;
            var owner = Window.GetWindow(this);
            if (owner?.IsVisible != true) return;
            AbrirJanelaComanda(owner);
        }));
    }

    private void AbrirJanelaComanda(Window owner)
    {
        var painelOriginal = (Panel)PainelEdicao.Parent;
        var indice = painelOriginal.Children.IndexOf(PainelEdicao);
        var bindingGrade = System.Windows.Data.BindingOperations.GetBindingBase(PainelGrade, VisibilityProperty)!;
        painelOriginal.Children.Remove(PainelEdicao);
        var janela = new Window
        {
            Title = TituloPedido(),
            Owner = owner, WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Width = Math.Min(900, SystemParameters.WorkArea.Width), Height = Math.Min(740, SystemParameters.WorkArea.Height),
            MinWidth = 800, MinHeight = 580, ResizeMode = ResizeMode.CanResize,
            ShowInTaskbar = false, DataContext = ViewModel,
            Content = new Border { Child = PainelEdicao, Padding = new Thickness(8) }
        };
        janela.SetResourceReference(Window.BackgroundProperty, "AjudaCanvas");
        ((Border)janela.Content).SetResourceReference(Border.BackgroundProperty, "AjudaCanvas");
        AtualizarDadosLaterais();
        _janelaComanda = janela;
        janela.InputBindings.Add(new KeyBinding(ViewModel.VerTotalCommand, Key.F4, ModifierKeys.None));
        PainelGrade.Visibility = Visibility.Visible;
        janela.PreviewKeyDown += (_, e) => OnPreviewKeyDown(e);
        janela.ContentRendered += (_, _) => CampoBusca.Focus();
        janela.Closing += (_, e) => { if (!SalvarObservacaoPendente()) { e.Cancel=true; ObservacaoPedido.Focus(); } };
        try { janela.ShowDialog(); }
        finally
        {
            _janelaComanda = null;
            ((Border)janela.Content).Child = null;
            janela.Content = null;
            painelOriginal.Children.Insert(indice, PainelEdicao);
            System.Windows.Data.BindingOperations.SetBinding(PainelGrade, VisibilityProperty, bindingGrade);
            if (ViewModel.ComandaAtual is not null) ViewModel.FecharEdicaoCommand.Execute(null);
            ViewModel.AtualizarComandasAbertas();
        }
    }

    private void OpcoesGrade_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu { PlacementTarget = (Button)sender, Placement = PlacementMode.Custom, MinWidth = 240, Padding = new Thickness(4) };
        menu.CustomPopupPlacementCallback = (tamanhoMenu, tamanhoBotao, deslocamento) =>
        {
            var botao = (Button)sender;
            var janela = Window.GetWindow(botao);
            if (janela is null)
                return new[] { new CustomPopupPlacement(new Point(tamanhoBotao.Width - tamanhoMenu.Width, tamanhoBotao.Height), PopupPrimaryAxis.Vertical) };
            var posicao = botao.TranslatePoint(new Point(0,0), janela);
            var x = Math.Max(-posicao.X, Math.Min(tamanhoBotao.Width - tamanhoMenu.Width, janela.ActualWidth - posicao.X - tamanhoMenu.Width));
            var y = posicao.Y + tamanhoBotao.Height + tamanhoMenu.Height <= janela.ActualHeight
                ? tamanhoBotao.Height : -tamanhoMenu.Height;
            return new[] { new CustomPopupPlacement(new Point(x,y), PopupPrimaryAxis.None) };
        };
        MenuItem Opcao(string nome, string desenho)
        {
            var icone = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(desenho), Width = 21, Height = 21,
                Stretch = System.Windows.Media.Stretch.Uniform, StrokeThickness = 1.8,
                StrokeStartLineCap = System.Windows.Media.PenLineCap.Round,
                StrokeEndLineCap = System.Windows.Media.PenLineCap.Round,
                StrokeLineJoin = System.Windows.Media.PenLineJoin.Round,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            icone.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty, "Ink");
            var quadro = new Grid { Width = 28, Height = 28, Margin = new Thickness(0,0,12,0) };
            quadro.Children.Add(icone);
            var cabecalho = new StackPanel { Orientation = Orientation.Horizontal };
            cabecalho.Children.Add(quadro);
            cabecalho.Children.Add(new TextBlock { Text = nome, VerticalAlignment = VerticalAlignment.Center });
            var opcao = new MenuItem { Header = cabecalho, FontSize = 16, MinHeight = 48, Padding = new Thickness(6,8,14,8) };
            System.Windows.Automation.AutomationProperties.SetName(opcao, nome);
            return opcao;
        }
        var atualizar = Opcao("Atualizar", "M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8 M21 3v5h-5 M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16 M8 16H3v5");
        atualizar.Click += (_, _) =>
        {
            try { ViewModel.AtualizarComandasAbertas(); }
            catch { JanelaAviso.Mostrar("Comandas", "Não foi possível atualizar. Tente novamente.", TipoAviso.Erro); }
        };
        menu.Items.Add(atualizar);
        var juntar = Opcao("Juntar pedidos", "M8 6l4-4 4 4 M12 2v10.3a4 4 0 0 1-1.172 2.872L4 22 M20 22l-5-5");
        juntar.Click += (_, _) => JuntarPedidos();
        menu.Items.Add(juntar);
        menu.IsOpen = true;
    }

    private void JuntarPedidos()
    {
        try { ViewModel.AtualizarComandasAbertas(); }
        catch { JanelaAviso.Mostrar("Comandas", "Não foi possível consultar as comandas. Tente novamente.", TipoAviso.Erro); return; }
        if (ViewModel.ComandasAbertas.Count < 2)
        {
            JanelaAviso.Mostrar("Juntar pedidos", "Abra pelo menos duas comandas para juntar os pedidos.", TipoAviso.Informacao);
            return;
        }
        var janela = new Window { Title="Juntar pedidos", Owner=Window.GetWindow(this), WindowStartupLocation=WindowStartupLocation.CenterOwner,
            Width=460, SizeToContent=SizeToContent.Height, ResizeMode=ResizeMode.NoResize, ShowInTaskbar=false };
        var painel = new StackPanel { Margin=new Thickness(24) };
        janela.Content = painel;
        painel.Children.Add(new TextBlock { Text="Todos os itens serão transferidos para o destino. A comanda de origem será liberada.", TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,0,0,16) });
        var opcoes = ViewModel.ComandasAbertas.Select(c=>new { c.Id, Nome=$"Comanda {c.Numero:00} · {CentavosParaMoedaConverter.Formatar(c.TotalCentavos)}" }).ToList();
        var origem = new ComboBox { ItemsSource=opcoes, DisplayMemberPath="Nome", SelectedValuePath="Id", MinHeight=46 };
        var destino = new ComboBox { ItemsSource=opcoes, DisplayMemberPath="Nome", SelectedValuePath="Id", MinHeight=46 };
        painel.Children.Add(new Label { Content="Comanda de origem", Target=origem, Padding=new Thickness(0,0,0,6) });
        painel.Children.Add(origem);
        painel.Children.Add(new Label { Content="Comanda de destino", Target=destino, Padding=new Thickness(0,16,0,6) });
        painel.Children.Add(destino);
        var erro = new TextBlock { TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,12,0,12) };
        erro.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");
        painel.Children.Add(erro);
        var confirmar = new Button { Content="Juntar pedidos", IsDefault=true };
        confirmar.Click += (_,_) =>
        {
            if (origem.SelectedValue is not int origemId || destino.SelectedValue is not int destinoId)
            { erro.Text="Selecione a origem e o destino."; return; }
            if (origemId==destinoId) { erro.Text="Escolha duas comandas diferentes."; return; }
            if (!new JanelaConfirmador().Confirmar("Confirmar transferência?", $"Transferir os itens de {origem.Text} para {destino.Text}? A origem será liberada.")) return;
            confirmar.IsEnabled=false;
            if (ViewModel.JuntarComandas(origemId,destinoId)) janela.DialogResult=true;
            else { erro.Text=ViewModel.Mensagem; confirmar.IsEnabled=true; }
        };
        painel.Children.Add(confirmar);
        painel.Children.Add(new Button { Content="Cancelar", IsCancel=true, Margin=new Thickness(0,8,0,0) });
        janela.ShowDialog();
    }

    private void EditarDados_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ComandaAtual is null || sender is not Button { Tag: string campo }) return;
        new EditarDadosComandaView(ViewModel, campo) { Owner = _janelaComanda ?? Window.GetWindow(this) }.ShowDialog();
    }

    private void AlterarPrioridade_Click(object sender, RoutedEventArgs e)
    {
        var botao = (Button)sender;
        var menu = new ContextMenu { PlacementTarget = botao, Placement = PlacementMode.Top };
        foreach (var prioridade in Enum.GetValues<PrioridadeComanda>())
        {
            var cor = prioridade switch
            {
                PrioridadeComanda.Alta => "#925600",
                PrioridadeComanda.Urgente => "#B71C1C",
                _ => "#28653F"
            };
            var cabecalho = new StackPanel { Orientation = Orientation.Horizontal };
            cabecalho.Children.Add(new System.Windows.Shapes.Ellipse
            {
                Width = 10, Height = 10, Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(cor)!
            });
            cabecalho.Children.Add(new TextBlock { Text = prioridade.ToString(), VerticalAlignment = VerticalAlignment.Center });
            var item = new MenuItem { Header = cabecalho, IsCheckable = true,
                IsChecked = ViewModel.ComandaAtual?.Prioridade == prioridade,
                Command = ViewModel.AlterarPrioridadeCommand, CommandParameter = prioridade, Padding = new Thickness(16,10,16,10) };
            System.Windows.Automation.AutomationProperties.SetName(item, prioridade.ToString());
            menu.Items.Add(item);
        }
        botao.ContextMenu = menu;
        menu.IsOpen = true;
    }

    private void AnotarPedido_Click(object sender, RoutedEventArgs e)
    {
        CampoBusca.Focus();
        CampoBusca.SelectAll();
    }

    public AtendimentoView(AtendimentoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        viewModel.PrepararSaidaPedido = SalvarObservacaoPendente;
        AtualizarDadosLaterais();
        viewModel.Itens.CollectionChanged += (_, _) => viewModel.NotificarItensPedido();
        var cronometro = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        cronometro.Tick += (_, _) => ViewModel.AtualizarCronometros();
        Loaded += (_, _) => { ViewModel.AtualizarCronometros(); cronometro.Start(); AgendarJanelaComanda(); };
        Unloaded += (_, _) => cronometro.Stop();
        DataObject.AddPastingHandler(CampoNumeroComanda, CampoNumero_Colar);
        // as linhas sao recriadas a cada alteracao; garante que o teclado continue chegando (F4/Esc)
        viewModel.Itens.CollectionChanged += (_, _) => RestaurarFocoSePerdido();
        viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(AtendimentoViewModel.ComandaAtual)) AgendarJanelaComanda(); };
    }

    // ---- foco inicial: so depois que o elemento esta de fato visivel (focar antes falha em silencio) ----

    private void PainelGrade_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => CampoNumeroComanda.Focus()));
        }
    }

    private void PainelEdicao_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            // a busca substitui o antigo foco em PainelComanda; o F4 segue valendo porque ela esta dentro deste UserControl
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => CampoBusca.Focus()));
        }
    }

    // ---- numero da comanda: somente digitos ----

    private void CampoNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !SomenteDigitos(e.Text);
    }

    private void CampoNumero_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // o espaco nao passa por PreviewTextInput
        if (e.Key == Key.Space)
        {
            e.Handled = true;
        }
    }

    private void CampoNumero_Colar(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetData(DataFormats.UnicodeText) is not string texto || !SomenteDigitos(texto))
        {
            e.CancelCommand();
        }
    }

    public static bool SomenteDigitos(string? texto) =>
        !string.IsNullOrEmpty(texto) && texto.All(char.IsAsciiDigit);

    // ---- teclado do atendimento ----

    public bool DigitarNumeroNaGrade(Key tecla, ModifierKeys modificadores, DependencyObject? origem)
    {
        if (ViewModel.ComandaAtual is not null || modificadores != ModifierKeys.None) return false;
        if (origem is TextBoxBase or PasswordBox or ComboBox
            || Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox) return false;
        string? digito = tecla is >= Key.D0 and <= Key.D9 ? ((int)tecla - (int)Key.D0).ToString()
            : tecla is >= Key.NumPad0 and <= Key.NumPad9 ? ((int)tecla - (int)Key.NumPad0).ToString() : null;
        if (digito is not null) ViewModel.NovoNumero += digito;
        else if (tecla == Key.Back)
        {
            if (ViewModel.NovoNumero.Length > 0) ViewModel.NovoNumero = ViewModel.NovoNumero[..^1];
        }
        else if (tecla == Key.Enter && ViewModel.NovoNumero.Length > 0)
        {
            ViewModel.AbrirCommand.Execute(null);
            return true;
        }
        else return false;
        CampoNumeroComanda.Focus();
        CampoNumeroComanda.CaretIndex = CampoNumeroComanda.Text.Length;
        return true;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Handled)
        {
            return;
        }

        var modificadores = e.KeyboardDevice.Modifiers;
        if (DigitarNumeroNaGrade(e.Key, modificadores, e.OriginalSource as DependencyObject))
        { e.Handled = true; return; }
        var ctrl = modificadores.HasFlag(ModifierKeys.Control);

        if (ctrl && !modificadores.HasFlag(ModifierKeys.Alt))
        {
            // somente Ctrl puro: Ctrl+Shift+N/F nao sao atalhos nossos
            if (modificadores != ModifierKeys.Control)
            {
                return;
            }

            if (e.Key == Key.N && ViewModel.ComandaAtual is null)
            {
                CampoNumeroComanda.Focus();
                CampoNumeroComanda.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.F && ViewModel.ComandaAtual is not null)
            {
                CampoBusca.Focus();
                CampoBusca.SelectAll();
                e.Handled = true;
            }
            return;
        }

        if (ViewModel.ComandaAtual is null || modificadores.HasFlag(ModifierKeys.Alt))
        {
            return;
        }

        var emBusca = ReferenceEquals(e.OriginalSource, CampoBusca);

        if (e.Key == Key.Enter && emBusca)
        {
            // Enter mantido pressionado repete o KeyDown: so a primeira pressao adiciona
            if (!e.IsRepeat)
            {
                ViewModel.AdicionarPrimeiroDaBuscaCommand.Execute(null);
                CampoBusca.SelectAll();
            }
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            if (PainelMenu.Visibility == Visibility.Visible)
            {
                PainelMenu.Visibility = Visibility.Collapsed;
                BotaoProdutosPedido.Focus();
            }
            else if (emBusca && !string.IsNullOrEmpty(CampoBusca.Text))
            {
                ViewModel.TextoBuscaCatalogo = string.Empty;
            }
            else
            {
                if (!SalvarObservacaoPendente()) { e.Handled = true; return; }
                ViewModel.FecharEdicaoCommand.Execute(null);
            }
            e.Handled = true;
            return;
        }

        // teclas de item nunca disparam com o foco numa caixa de texto (+ e - sao texto)
        if (EstaEmCampoDeTexto(e))
        {
            return;
        }

        // Teclas de item: nunca no menu (cards/categorias mantem o foco proprio) nem com modificadores.
        // O "+" da linha principal e digitado com Shift em varios layouts, entao OemPlus tolera Shift.
        var semModificador = modificadores == ModifierKeys.None;
        var teclaMaisComShift = e.Key == Key.OemPlus && modificadores == ModifierKeys.Shift;
        if (EstaNoMenu(e) || !(semModificador || teclaMaisComShift))
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Up or Key.Down when semModificador:
                (e.Key == Key.Up ? ViewModel.SelecionarItemAnteriorCommand : ViewModel.SelecionarProximoItemCommand).Execute(null);
                e.Handled = true;
                break;
            case Key.OemPlus or Key.Add when semModificador || teclaMaisComShift:
                ViewModel.AumentarSelecionadoCommand.Execute(null);
                e.Handled = true;
                RestaurarFocoSePerdido();
                break;
            case Key.OemMinus or Key.Subtract when semModificador:
                ViewModel.DiminuirSelecionadoCommand.Execute(null);
                e.Handled = true;
                RestaurarFocoSePerdido();
                break;
            case Key.Delete when semModificador:
                ViewModel.RemoverSelecionadoCommand.Execute(null);
                e.Handled = true;
                RestaurarFocoSePerdido();
                break;
        }
    }

    private static bool EstaEmCampoDeTexto(KeyEventArgs e) =>
        e.OriginalSource is TextBoxBase || Keyboard.FocusedElement is TextBoxBase;

    private bool EstaNoMenu(KeyEventArgs e) =>
        e.OriginalSource is DependencyObject origem && PainelMenu.IsAncestorOf(origem);

    // As linhas sao recriadas a cada alteracao; se o foco estava num botao da linha ele some junto e o
    // teclado (inclusive o F4) deixaria de chegar aqui. Recolhe o foco no painel da comanda.
    private bool _restauracaoDeFocoPendente;

    private void RestaurarFocoSePerdido()
    {
        if (_restauracaoDeFocoPendente)
        {
            return;
        }

        _restauracaoDeFocoPendente = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            _restauracaoDeFocoPendente = false;

            // so quando o foco ficou "solto" (nulo ou na propria janela): nunca rouba foco de outra janela
            // (ex.: dialogo de encerramento) nem de um controle que o usuario escolheu
            var foco = Keyboard.FocusedElement;
            var perdido = foco is null || foco is Window;
            if (ViewModel.ComandaAtual is not null && perdido && !IsKeyboardFocusWithin
                && Window.GetWindow(this)?.IsActive == true)
            {
                PainelComanda.Focus();
            }
        }));
    }

    // ---- selecao da linha (por Tab nos botoes da linha ou por clique) ----

    private void LinhaItem_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        SelecionarLinha(sender);

    private void LinhaItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        SelecionarLinha(sender);

    private void SelecionarLinha(object linha)
    {
        if (linha is FrameworkElement { DataContext: ItemComanda item })
        {
            ViewModel.ItemSelecionadoId = item.Id;
        }
    }
}
