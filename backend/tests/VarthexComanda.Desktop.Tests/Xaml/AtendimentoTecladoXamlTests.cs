using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using VarthexComanda.Desktop.Atendimento;
using Xunit;

namespace VarthexComanda.Desktop.Tests.Xaml;

/// <summary>
/// Atendimento por teclado (RNF18 / CT17) no nivel da view: controles existem e estao ligados, ordem de Tab,
/// destaque da linha selecionada e tratamento das teclas. Sem exibir janela (foco real de teclado nao e
/// testavel aqui; o foco inicial e conferido na verificacao por UI Automation).
/// </summary>
[Trait("Requisito", "RNF18")]
[Trait("Caso", "CT17")]
public class AtendimentoTecladoXamlTests
{
    [Fact]
    public void NumeroDigitadoDestacaSomenteAComandaCorrespondente()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (vm, _) = CarregamentoDeXamlTests.CriarAtendimento();
            vm.FecharEdicaoCommand.Execute(null);
            var view = new AtendimentoView(vm);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var botoes=Descendentes(view).OfType<Button>().Where(b=>b.CommandParameter is ComandaSlotItem).ToArray();
            vm.NovoNumero="01";
            view.UpdateLayout();
            var marcado=Assert.Single(botoes.Where(b=>b.BorderThickness==new Thickness(4)));
            Assert.Equal(1,((ComandaSlotItem)marcado.CommandParameter).Numero);
            vm.NovoNumero="10";
            view.UpdateLayout();
            marcado=Assert.Single(botoes.Where(b=>b.BorderThickness==new Thickness(4)));
            Assert.Equal(10,((ComandaSlotItem)marcado.CommandParameter).Numero);
            vm.NovoNumero="";
            view.UpdateLayout();
            Assert.DoesNotContain(botoes,b=>b.BorderThickness==new Thickness(4));
        });
    }
    [Fact]
    public void GradeAceitaNumerosBackspaceEEnterSemSelecionarCampo()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (vm, _) = CarregamentoDeXamlTests.CriarAtendimento();
            vm.FecharEdicaoCommand.Execute(null);
            var view = new AtendimentoView(vm);
            Assert.True(view.DigitarNumeroNaGrade(Key.D1, ModifierKeys.None, new Button()));
            Assert.True(view.DigitarNumeroNaGrade(Key.NumPad2, ModifierKeys.None, new Button()));
            Assert.Equal("12", vm.NovoNumero);
            Assert.True(view.DigitarNumeroNaGrade(Key.Back, ModifierKeys.None, new Button()));
            Assert.Equal("1", vm.NovoNumero);
            Assert.False(view.DigitarNumeroNaGrade(Key.D3, ModifierKeys.None, new TextBox()));
            Assert.False(view.DigitarNumeroNaGrade(Key.D3, ModifierKeys.Control, new Button()));
            Assert.True(view.DigitarNumeroNaGrade(Key.Enter, ModifierKeys.None, new Button()));
            Assert.Equal(1, vm.ComandaAtual!.Numero);
            Assert.False(view.DigitarNumeroNaGrade(Key.D3, ModifierKeys.None, new Button()));
        });
    }
    private static IEnumerable<DependencyObject> Descendentes(DependencyObject raiz)
    {
        var quantidade = VisualTreeHelper.GetChildrenCount(raiz);
        for (var i = 0; i < quantidade; i++)
        {
            var filho = VisualTreeHelper.GetChild(raiz, i);
            yield return filho;
            foreach (var neto in Descendentes(filho))
            {
                yield return neto;
            }
        }
    }

    // Aproximacao da ordem de Tab do WPF: percurso em profundidade, irmaos ordenados por TabIndex
    // (estavel: empate = ordem do documento), ignorando subarvores nao visiveis e elementos que nao sao tab stop.
    private static IEnumerable<UIElement> OrdemDeTab(DependencyObject raiz)
    {
        var filhos = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(raiz))
            .Select(i => VisualTreeHelper.GetChild(raiz, i))
            .OrderBy(f => f is UIElement u ? KeyboardNavigation.GetTabIndex(u) : int.MaxValue);

        foreach (var filho in filhos)
        {
            if (filho is UIElement { Visibility: not Visibility.Visible })
            {
                continue;
            }

            if (filho is UIElement elemento && elemento.Focusable && KeyboardNavigation.GetIsTabStop(elemento))
            {
                yield return elemento;
            }

            foreach (var neto in OrdemDeTab(filho))
            {
                yield return neto;
            }
        }
    }

    private static int PosicaoDoBotao(IReadOnlyList<UIElement> ordem, string conteudo)
    {
        var indice = ordem.ToList().FindIndex(e => e is Button { Content: string texto } && texto == conteudo);
        Assert.True(indice >= 0, $"Botao \"{conteudo}\" nao esta na ordem de Tab.");
        return indice;
    }

    [Fact]
    public void AtendimentoView_ExpoeCampoDeNumeroEDeBusca()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);

            var numero = Assert.IsType<TextBox>(view.FindName("CampoNumeroComanda"));
            var busca = Assert.IsType<TextBox>(view.FindName("CampoBusca"));

            viewModel.NovoNumero = "12";
            Assert.Equal("12", numero.Text);
            viewModel.TextoBuscaCatalogo = "refri";
            Assert.Equal("refri", busca.Text);

            // Enter no campo de numero executa AbrirCommand (o KeyBinding herda o DataContext)
            var enter = Assert.Single(numero.InputBindings.OfType<KeyBinding>(), k => k.Key == Key.Enter);
            Assert.Same(viewModel.AbrirCommand, enter.Command);
        });
    }

    [Fact]
    public void AtendimentoView_OrdemDeTab_BuscaProdutosItensEPagamentoComCatalogoSobDemanda()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (vm, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(vm);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var ordem = OrdemDeTab(view).ToList();
            Assert.Contains((UIElement)view.FindName("CampoBusca"), ordem);
            Assert.Contains((UIElement)view.FindName("BotaoProdutosPedido"), ordem);
            Assert.Contains(ordem, e => e is Button b && b.Command == vm.VerTotalCommand);
            Assert.DoesNotContain(ordem, e => e is Grid { Name: "PainelComanda" });
            var catalogo = (FrameworkElement)view.FindName("PainelMenu");
            Assert.Equal(Visibility.Collapsed, catalogo.Visibility);
            ((Button)view.FindName("BotaoProdutosPedido")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            view.UpdateLayout();
            Assert.Equal(Visibility.Visible, catalogo.Visibility);
            ordem = OrdemDeTab(view).ToList();
            Assert.True(PosicaoDoBotao(ordem, "Todas") < PosicaoDoBotao(ordem, "Bebidas"));
            Assert.Contains(ordem, e => e is Button b && b.Command == vm.AdicionarProdutoAoItemCommand);
        });
    }
    [Fact]
    public void AtendimentoView_ItemSelecionado_TemLinhaComFundoDestacado()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            TemaInterface.Aplicar(false,false);
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            var destaque = Color.FromRgb(0xFB, 0xE3, 0xC2);

            viewModel.ItemSelecionadoId = null;
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            Assert.DoesNotContain(Descendentes(view).OfType<Border>(), b => b.Background is SolidColorBrush s && s.Color == destaque);

            viewModel.ItemSelecionadoId = viewModel.Itens[0].Id;
            view.UpdateLayout();
            Assert.Single(Descendentes(view).OfType<Border>(), b => b.Background is SolidColorBrush s && s.Color == destaque);
        });
    }

    [Fact]
    public void ItemSelecionadoParaCorConverter_DestacaSoALinhaSelecionada()
    {
        ThreadingHelper.EmSta(()=>
        {
        CarregamentoDeXamlTests.GarantirApp();TemaInterface.Aplicar(false,false);
        var conversor = new ItemSelecionadoParaCorConverter();

        var fundoSelecionada = Assert.IsAssignableFrom<SolidColorBrush>(conversor.Convert(new object[] { 5, 5 }, typeof(Brush), null!, CultureInfo.InvariantCulture));
        var marcaSelecionada = Assert.IsAssignableFrom<SolidColorBrush>(conversor.Convert(new object[] { 5, 5 }, typeof(Brush), "Marca", CultureInfo.InvariantCulture));
        var outra = conversor.Convert(new object[] { 5, 6 }, typeof(Brush), null!, CultureInfo.InvariantCulture);
        var semSelecao = conversor.Convert(new object[] { 5, null! }, typeof(Brush), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Color.FromRgb(0xFB, 0xE3, 0xC2), fundoSelecionada.Color);
        Assert.Equal(Color.FromRgb(0xD9, 0x82, 0x2B), marcaSelecionada.Color);
        Assert.Same(Brushes.Transparent, outra);
        Assert.Same(Brushes.Transparent, semSelecao);
        });
    }

    [Theory]
    [InlineData("7", true)]
    [InlineData("0123456789", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("a", false)]
    [InlineData("1a", false)]
    [InlineData("-1", false)]
    [InlineData("1 2", false)]
    [InlineData("٣", false)] // digito arabe-indico: char.IsDigit aceitaria, IsAsciiDigit nao
    public void SomenteDigitos_AceitaApenasDigitosAscii(string? texto, bool esperado)
    {
        Assert.Equal(esperado, AtendimentoView.SomenteDigitos(texto));
    }

    // ---- teclas: eventos roteados reais (PreviewKeyDown) contra a view, sem exibir janela ----

    private static bool Pressionar(AtendimentoView view, Key tecla, IInputElement origem)
    {
        using var fonte = new HwndSource(new HwndSourceParameters("teste-teclado"));
        var evento = new KeyEventArgs(Keyboard.PrimaryDevice, fonte, Environment.TickCount, tecla)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent,
            Source = origem
        };
        view.RaiseEvent(evento);
        return evento.Handled;
    }

    [Fact]
    public void Teclas_SetasESinais_OperamNoItemSelecionadoForaDeCampoDeTexto()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var item = viewModel.Itens[0]; // quantidade 2
            viewModel.ItemSelecionadoId = null;

            Assert.True(Pressionar(view, Key.Down, view));
            Assert.Equal(item.Id, viewModel.ItemSelecionadoId);

            Assert.True(Pressionar(view, Key.Add, view));
            Assert.Equal(3, viewModel.Itens[0].Quantidade);
            Assert.True(Pressionar(view, Key.OemPlus, view));
            Assert.Equal(4, viewModel.Itens[0].Quantidade);
            Assert.True(Pressionar(view, Key.Subtract, view));
            Assert.True(Pressionar(view, Key.OemMinus, view));
            Assert.Equal(2, viewModel.Itens[0].Quantidade);

            Assert.True(Pressionar(view, Key.Delete, view)); // FakeConfirmador confirma
            Assert.Empty(viewModel.Itens);
            Assert.Null(viewModel.ItemSelecionadoId);
        });
    }

    [Theory]
    [InlineData(Key.Add)]
    [InlineData(Key.OemPlus)]
    [InlineData(Key.Subtract)]
    [InlineData(Key.OemMinus)]
    [InlineData(Key.Delete)]
    [InlineData(Key.Down)]
    [InlineData(Key.Up)]
    public void Teclas_ComFocoNaBusca_NaoAgemSobreOsItens(Key tecla)
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var busca = (TextBox)view.FindName("CampoBusca");
            viewModel.ItemSelecionadoId = viewModel.Itens[0].Id;

            var tratada = Pressionar(view, tecla, busca);

            Assert.False(tratada);
            Assert.Single(viewModel.Itens);
            Assert.Equal(2, viewModel.Itens[0].Quantidade);
            Assert.Equal(viewModel.Itens[0].Id, viewModel.ItemSelecionadoId);
        });
    }

    [Theory]
    [InlineData(Key.Add)]
    [InlineData(Key.OemPlus)]
    [InlineData(Key.Subtract)]
    [InlineData(Key.OemMinus)]
    [InlineData(Key.Delete)]
    [InlineData(Key.Down)]
    [InlineData(Key.Up)]
    public void Teclas_ComFocoNoMenu_NaoAgemSobreOsItens(Key tecla)
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var menu = (DependencyObject)view.FindName("PainelMenu");
            var botaoDoMenu = Descendentes(menu).OfType<Button>().First();
            viewModel.ItemSelecionadoId = viewModel.Itens[0].Id;

            var tratada = Pressionar(view, tecla, botaoDoMenu);

            Assert.False(tratada);
            Assert.Single(viewModel.Itens);
            Assert.Equal(2, viewModel.Itens[0].Quantidade);
            Assert.Equal(viewModel.Itens[0].Id, viewModel.ItemSelecionadoId);
        });
    }

    [Fact]
    public void AtendimentoView_SemComandaSelecionada_InstanciaOsSlotsComFocoVisivel()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            viewModel.FecharEdicaoCommand.Execute(null);
            var view = new AtendimentoView(viewModel);

            CarregamentoDeXamlTests.MedirEOrganizar(view);

            var grade = (DependencyObject)view.FindName("PainelGrade");
            var slots = Descendentes(grade).OfType<Button>()
                .Where(b => ReferenceEquals(b.Command, viewModel.AbrirOuSelecionarSlotCommand))
                .ToList();
            Assert.NotEmpty(slots);
            var foco = System.Windows.Application.Current.FindResource("FocoVisivel");
            Assert.All(slots, b => Assert.Same(foco, b.FocusVisualStyle));
        });
    }

    [Fact]
    public void Enter_NaBusca_AdicionaOPrimeiroEMantemOTexto()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var busca = (TextBox)view.FindName("CampoBusca");
            viewModel.TextoBuscaCatalogo = "refri";

            Assert.True(Pressionar(view, Key.Enter, busca));

            Assert.Equal(3, viewModel.Itens.Single().Quantidade);
            Assert.Equal("refri", busca.Text);
            Assert.Equal(0, busca.SelectionStart);
            Assert.Equal(5, busca.SelectionLength);
        });
    }

    [Fact]
    public void Enter_NaBuscaVazia_NaoAdicionaNada()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var busca = (TextBox)view.FindName("CampoBusca");
            viewModel.TextoBuscaCatalogo = string.Empty;

            Pressionar(view, Key.Enter, busca);

            Assert.Equal(2, viewModel.Itens.Single().Quantidade);
        });
    }

    [Fact]
    public void Enter_NaBuscaSemResultado_MostraMensagem()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var busca = (TextBox)view.FindName("CampoBusca");
            viewModel.TextoBuscaCatalogo = "zzz";

            Pressionar(view, Key.Enter, busca);

            Assert.Equal("Nenhum produto encontrado.", viewModel.Mensagem);
        });
    }

    [Fact]
    public void Esc_ComTextoNaBusca_LimpaSemSair_ESemTextoVolta()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            var busca = (TextBox)view.FindName("CampoBusca");
            viewModel.TextoBuscaCatalogo = "refri";

            Assert.True(Pressionar(view, Key.Escape, busca));
            Assert.Equal(string.Empty, viewModel.TextoBuscaCatalogo);
            Assert.NotNull(viewModel.ComandaAtual);

            Assert.True(Pressionar(view, Key.Escape, busca));
            Assert.Null(viewModel.ComandaAtual);
        });
    }

    [Fact]
    public void Esc_ForaDaBusca_VoltaMesmoComTextoNaBusca()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (viewModel, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(viewModel);
            CarregamentoDeXamlTests.MedirEOrganizar(view);
            viewModel.TextoBuscaCatalogo = "refri";

            Assert.True(Pressionar(view, Key.Escape, view));

            Assert.Null(viewModel.ComandaAtual);
        });
    }
}
