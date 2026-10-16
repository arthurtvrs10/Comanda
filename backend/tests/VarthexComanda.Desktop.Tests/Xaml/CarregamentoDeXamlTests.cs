using System.Windows;
using System.Windows.Controls;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Backup;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Atendimento;
using VarthexComanda.Application.Tests.Backup;
using VarthexComanda.Application.Tests.Catalogo;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Desktop.Catalogo;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Atendimento;
using VarthexComanda.Desktop.Tests.Backup;
using VarthexComanda.Domain;
using Xunit;
using WpfApp = System.Windows.Application;

namespace VarthexComanda.Desktop.Tests.Xaml;

/// <summary>
/// Rede de seguranca contra XAML invalido: a UI nao pode ser aberta na verificacao, e um recurso
/// ausente ({StaticResource}), estilo ou converter inexistente so estouraria em runtime. Estes testes
/// constroem cada view com ViewModels reais sobre dobles em memoria e fazem Measure/Arrange.
/// (RNF18: o foco visivel vem de App.xaml, carregado aqui como o app o carrega.)
///
/// Carga do App.xaml: em processo de teste nao ha instancia de App. <c>new App()</c> mais
/// <c>InitializeComponent()</c> (gerado pelo compilador de XAML, publico) carrega o BAML real de
/// App.xaml na propria instancia e a torna <c>Application.Current</c> - sem Run(), portanto sem
/// OnStartup (nem banco, nem single instance, nem log). Exatamente um App por processo.
/// </summary>
public class CarregamentoDeXamlTests
{
    private static readonly Size Tela = new(1200, 700);
    private static readonly object TravaApp = new();
    private static App? _app;

    // Sempre chamado de dentro de EmSta (o App fica preso a thread STA dedicada)
    internal static void GarantirApp()
    {
        lock (TravaApp)
        {
            if (_app is not null)
            {
                return;
            }

            _app = WpfApp.Current as App ?? new App();
            _app.InitializeComponent();
            _app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
    }

    internal static void MedirEOrganizar(FrameworkElement elemento)
    {
        elemento.Measure(Tela);
        elemento.Arrange(new Rect(new Point(0, 0), Tela));
        elemento.UpdateLayout();
        Assert.True(elemento.ActualWidth > 0, $"{elemento.GetType().Name} ficou sem largura apos o layout.");
        Assert.True(elemento.ActualHeight > 0, $"{elemento.GetType().Name} ficou sem altura apos o layout.");
    }

    // ---- dobles cheios o bastante para os DataTemplates serem instanciados ----

    internal static (AtendimentoViewModel ViewModel, Comanda Comanda) CriarAtendimento()
    {
        var relogio = new FakeClock();
        var comandas = new FakeComandaRepository();
        var categorias = new FakeCategoriaRepository();
        var produtos = new FakeProdutoRepository();
        var configuracoes = new FakeConfiguracaoRepository();

        var categoria = categorias.Salvar(new Categoria { Id = 0, Nome = "Bebidas", Ativo = true, CriadoEm = relogio.UtcNow, AtualizadoEm = relogio.UtcNow });
        var produto = produtos.Salvar(new Produto { Id = 0, CategoriaId = categoria.Id, Nome = "Refrigerante", PrecoCentavos = 500, Ativo = true, CriadoEm = relogio.UtcNow, AtualizadoEm = relogio.UtcNow });
        var comanda = comandas.AbrirComanda(10, relogio.UtcNow);
        comandas.AdicionarItem(comanda.Id, produto, 2, relogio.UtcNow);

        var viewModel = new AtendimentoViewModel(
            new AbrirComanda(comandas, relogio, new ObterConfiguracao(configuracoes)),
            new AdicionarItem(comandas, produtos, relogio),
            new AlterarQuantidade(comandas, relogio),
            new RemoverItem(comandas, relogio),
            new CancelarComanda(comandas, relogio),
            comandas,
            new ListarCategoriasAtivas(categorias),
            new PesquisarProdutos(produtos),
            new FakeConfirmador(),
            new FakeEncerramentoDialog(comandas, relogio),
            new ObterConfiguracao(configuracoes),
            relogio);

        // preenche a comanda em edicao (painel de itens) alem de slots, categorias e catalogo
        viewModel.SelecionarComandaCommand.Execute(comanda);
        return (viewModel, comanda);
    }

    internal static ProdutosViewModel CriarProdutos()
    {
        var relogio = new FakeClock();
        var categorias = new FakeCategoriaRepository();
        var produtos = new FakeProdutoRepository();
        var fotos = new FakeFotoStorage();

        var categoria = categorias.Salvar(new Categoria { Id = 0, Nome = "Bebidas", Ativo = true, CriadoEm = relogio.UtcNow, AtualizadoEm = relogio.UtcNow });
        produtos.Salvar(new Produto { Id = 0, CategoriaId = categoria.Id, Nome = "Refrigerante", PrecoCentavos = 500, Ativo = true, CriadoEm = relogio.UtcNow, AtualizadoEm = relogio.UtcNow });

        return new ProdutosViewModel(
            new ListarCategoriasAtivas(categorias),
            new CadastrarCategoria(categorias, relogio),
            new PesquisarProdutos(produtos),
            new CadastrarProduto(produtos, categorias, relogio),
            new AlterarProduto(produtos, categorias, relogio),
            new DesativarProduto(produtos, relogio),
            new DefinirFotoProduto(produtos, fotos, relogio),
            new RemoverFotoProduto(produtos, fotos, relogio));
    }

    internal static HistoricoViewModel CriarHistorico()
    {
        var relogio = new FakeClock();
        var vendas = new FakeVendaRepository();
        vendas.AdicionarVenda(
            new Venda { Id = 1, ComandaId = 1, Numero = 1, TotalCentavos = 1000, FinalizadaEm = relogio.UtcNow, Status = StatusVenda.Concluida },
            10,
            new List<ItemComanda>());
        return new HistoricoViewModel(new ListarVendasPorData(vendas), new BuscarItensDaVenda(vendas), relogio);
    }

    private static BackupViewModel CriarBackup()
    {
        var backupService = new FakeBackupService();
        var registros = new FakeBackupRegistroRepository();
        registros.Registrar(new BackupRegistro
        {
            Id = 0,
            Arquivo = "varthex-comanda-2026-09-17-080000.db",
            Destino = "C:\\backups",
            CriadoEm = new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc),
            Status = StatusBackup.Sucesso,
            Checksum = "xyz",
            Mensagem = null
        });
        return new BackupViewModel(
            new CriarBackupManual(backupService),
            new RestaurarBackup(backupService),
            new ListarBackupsRecentes(registros),
            new FakeConfirmadorDeBackup());
    }

    internal static ConfiguracaoViewModel CriarConfiguracao()
    {
        var repositorio = new FakeConfiguracaoRepository();
        return new ConfiguracaoViewModel(new ObterConfiguracao(repositorio), new SalvarConfiguracao(repositorio, new FakeClock()));
    }

    private static MainWindow CriarMainWindow(VarthexComanda.Infrastructure.Storage.AppPaths? paths = null)
    {
        var (atendimento, _) = CriarAtendimento();
        return new MainWindow(
            new AtendimentoView(atendimento),
            new ProdutosView(CriarProdutos()),
            new HistoricoView(CriarHistorico()),
            new BackupView(CriarBackup()),
            new ConfiguracaoView(CriarConfiguracao()), paths);
    }

    // ---- testes ----

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void AppXaml_CarregaEExpoeEstiloFocoVisivelResolvidoPeloButton()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();

            var estilo = Assert.IsType<Style>(WpfApp.Current.Resources["FocoVisivel"]);
            Assert.Equal(typeof(Control), estilo.TargetType);

            var botao = new Button();
            MedirEOrganizar(new Grid { Width = 200, Height = 100, Children = { botao } });
            Assert.Same(estilo, botao.FocusVisualStyle);

            // os estilos implicitos de toque de App.xaml continuam valendo junto do foco
            Assert.Equal(46, botao.MinHeight);
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void AtendimentoView_ComComandaAberta_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var (viewModel, _) = CriarAtendimento();

            var view = new AtendimentoView(viewModel);

            Assert.Same(viewModel, view.DataContext);
            Assert.NotNull(view.Content);
            MedirEOrganizar(view);
            SalvarPrevia(view, "comanda-painel");
            viewModel.FecharEdicaoCommand.Execute(null);
            MedirEOrganizar(view);
            SalvarPrevia(view, "comanda-grade");
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void ProdutosView_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var viewModel = CriarProdutos();

            var view = new ProdutosView(viewModel);

            Assert.Same(viewModel, view.DataContext);
            Assert.NotNull(view.Content);
            MedirEOrganizar(view);
            SalvarPrevia(view, "produtos-lista");
            view.Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogo = System.Windows.Application.Current.Windows.OfType<Window>()
                    .Single(w => w.Title == "Novo produto");
                var formulario = (FrameworkElement)dialogo.Content;
                formulario.UpdateLayout();
                SalvarPrevia(formulario, "produtos-modal");
                viewModel.NomeProduto = "Não deve persistir";
                viewModel.DefinirFoto("foto-descartada.png");
                dialogo.Close();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            ((Button)view.FindName("BotaoNovoProduto")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Empty(viewModel.NomeProduto);
            Assert.Null(viewModel.FotoPendenteCaminho);
            Assert.False(viewModel.AdiarFotoAteSalvar);
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void HistoricoView_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var viewModel = CriarHistorico();

            var view = new HistoricoView(viewModel);

            Assert.Same(viewModel, view.DataContext);
            Assert.NotNull(view.Content);
            MedirEOrganizar(view);
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void BackupView_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var viewModel = CriarBackup();

            var view = new BackupView(viewModel);

            Assert.Same(viewModel, view.DataContext);
            Assert.NotNull(view.Content);
            MedirEOrganizar(view);
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void ConfiguracaoView_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var viewModel = CriarConfiguracao();

            var view = new ConfiguracaoView(viewModel);

            Assert.Same(viewModel, view.DataContext);
            Assert.NotNull(view.Content);
            MedirEOrganizar(view);
            foreach(var largura in new[] {1200,780})
            {
                for(var secao=0;secao<5;secao++)
                {
                    view.AbrirSecao(secao);
                    view.Measure(new Size(largura,650));view.Arrange(new Rect(0,0,largura,650));view.UpdateLayout();
                    Assert.Equal(secao.ToString(), ((WrapPanel)view.FindName("Navegacao")).Children.OfType<RadioButton>().Single(b=>b.IsChecked==true).Tag);
                    SalvarPrevia(view,$"configuracao-{secao}-{largura}");
                }
            }
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void EncerramentoView_SemExibirJanela_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var relogio = new FakeClock();
            var comandas = new FakeComandaRepository();
            var produtos = new FakeProdutoRepository();
            var produto = produtos.Salvar(new Produto { Id = 0, CategoriaId = 1, Nome = "Refrigerante", PrecoCentavos = 500, Ativo = true, CriadoEm = relogio.UtcNow, AtualizadoEm = relogio.UtcNow });
            var comanda = comandas.AbrirComanda(10, relogio.UtcNow);
            comandas.AdicionarItem(comanda.Id, produto, 2, relogio.UtcNow);
            var viewModel = new EncerramentoViewModel(new EncerrarComanda(comandas, relogio), comandas);
            viewModel.Carregar(comanda.Id);

            // Window: nunca Show()/ShowDialog(); Measure/Arrange no conteudo
            var janela = new EncerramentoView(viewModel);

            Assert.Same(viewModel, janela.DataContext);
            var conteudo = Assert.IsAssignableFrom<FrameworkElement>(janela.Content);
            conteudo.Measure(new Size(580, 530));
            conteudo.Arrange(new Rect(0, 0, 580, 530));
            conteudo.UpdateLayout();
            SalvarPrevia(conteudo, "pagamento-sem-selecao");
            var pix = (RadioButton)janela.FindName("FormaPix");
            pix.IsChecked = true;
            viewModel.SelecionarFormaCommand.Execute(FormaPagamento.Pix);
            conteudo.Measure(new Size(580, 530));
            conteudo.Arrange(new Rect(0, 0, 580, 530));
            conteudo.UpdateLayout();
            SalvarPrevia(conteudo, "pagamento-pix");
            Assert.False(janela.IsVisible);
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void ConfirmacaoView_SemExibirJanela_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();

            var janela = new ConfirmacaoView("Cancelar comanda", "Deseja cancelar a comanda?");

            Assert.Equal("Cancelar comanda", janela.Title);
            var conteudo = Assert.IsAssignableFrom<FrameworkElement>(janela.Content);
            MedirEOrganizar(conteudo);
            Assert.False(janela.IsVisible);
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void MainWindow_ComAsCincoViews_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();

            var janela = CriarMainWindow();

            var conteudo = Assert.IsAssignableFrom<FrameworkElement>(janela.Content);
            Assert.NotNull(janela.FindName("ConteudoPrincipal"));
            MedirEOrganizar(conteudo);
            foreach (var nome in new[] { "BotaoProdutos", "BotaoBackup", "BotaoConfiguracoes", "BotaoLicenca" })
            {
                var botao = Assert.IsType<Button>(janela.FindName(nome));
                Assert.False(botao.IsEnabled);
                Assert.Equal(Visibility.Collapsed, botao.Visibility);
            }
            foreach (var comando in new[] { MainWindow.IrParaProdutosCommand, MainWindow.IrParaBackupCommand, MainWindow.IrParaConfiguracoesCommand })
                Assert.False(comando.CanExecute(null, janela));
            SalvarPrevia(conteudo, "menu-garcom");
            Assert.False(janela.IsVisible);
        });
    }

    [Fact]
    [Trait("Requisito", "RNF18")]
    public void MainWindow_EmModoRestauracao_CarregaEMedeSemExcecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var janela = CriarMainWindow();

            janela.EntrarModoRestauracao();

            var conteudo = Assert.IsAssignableFrom<FrameworkElement>(janela.Content);
            MedirEOrganizar(conteudo);
            Assert.False(janela.IsVisible);
        });
    }

    [Fact]
    public void PinBloqueiaMenusEDesbloqueiaPorToqueESaidaBloqueiaNovamente()
    {
        var raiz = Path.Combine(Path.GetTempPath(), "pin-ui-" + Guid.NewGuid());
        var arquivo = Path.Combine(raiz, "atendimento.credencial");
        try
        {
            new AcessoAtendimento(arquivo).DefinirPin("0123");
            ThreadingHelper.EmSta(() =>
            {
                GarantirApp();
                var janela = CriarMainWindow(new VarthexComanda.Infrastructure.Storage.AppPaths(raiz));
                var area = (DockPanel)janela.FindName("AreaSistema");
                var camada = (ContentControl)janela.FindName("CamadaBloqueio");
                var bloqueio = Assert.IsType<BloqueioAtendimentoView>(camada.Content);
                var conteudo = (ContentControl)janela.FindName("ConteudoPrincipal");
                var raizVisual = (FrameworkElement)janela.Content;
                MedirEOrganizar(raizVisual);
                var inicioHost = (ContentControl)janela.FindName("MenuInicial");
                Assert.Equal(Visibility.Visible, inicioHost.Visibility);
                Assert.Equal(Visibility.Collapsed, camada.Visibility);
                SalvarPrevia(raizVisual, "inicio-vcomanda-1200");
                raizVisual.Measure(new Size(800, 600));
                raizVisual.Arrange(new Rect(0,0,800,600));
                raizVisual.UpdateLayout();
                SalvarPrevia(raizVisual, "inicio-vcomanda-800");
                var inicio = Assert.IsType<InicioView>(inicioHost.Content);
                ((Button)inicio.FindName("BotaoGarcom")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                MedirEOrganizar(raizVisual);
                Assert.False(area.IsEnabled);
                Assert.Null(conteudo.Content);
                Assert.False(MainWindow.IrParaAtendimentoCommand.CanExecute(null, janela));
                Assert.False(MainWindow.IrParaHistoricoCommand.CanExecute(null, janela));
                SalvarPrevia(raizVisual, "bloqueio-pin-1200");
                raizVisual.Measure(new Size(800, 600));
                raizVisual.Arrange(new Rect(0,0,800,600));
                raizVisual.UpdateLayout();
                SalvarPrevia(raizVisual, "bloqueio-pin-800");
                var botoes = Descendentes(bloqueio).OfType<Button>().ToArray();
                foreach (var digito in "9999")
                    botoes.Single(b => Equals(b.Content, digito.ToString())).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                ((Button)bloqueio.FindName("BotaoEntrar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.False(area.IsEnabled);
                foreach (var digito in "0123")
                    botoes.Single(b => Equals(b.Content, digito.ToString())).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                ((Button)bloqueio.FindName("BotaoEntrar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.True(area.IsEnabled);
                Assert.Equal(Visibility.Collapsed, camada.Visibility);
                Assert.False(MainWindow.IrParaProdutosCommand.CanExecute(null, janela));
                var abaAtendimento = (Button)janela.FindName("BotaoAtendimento");
                var abaHistorico = (Button)janela.FindName("BotaoHistorico");
                Assert.Equal("ativa", abaAtendimento.Tag);
                MainWindow.IrParaHistoricoCommand.Execute(null, janela);
                Assert.Equal("ativa", abaHistorico.Tag);
                Assert.Null(abaAtendimento.Tag);
                abaAtendimento.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal("ativa", abaAtendimento.Tag);
                Assert.Null(abaHistorico.Tag);
                MedirEOrganizar(raizVisual);
                SalvarPrevia(raizVisual, "abas-garcom-1200");
                raizVisual.Measure(new Size(800, 600));
                raizVisual.Arrange(new Rect(0,0,800,600));
                raizVisual.UpdateLayout();
                SalvarPrevia(raizVisual, "abas-garcom-800");
                var acesso = (AcessoGestor)typeof(MainWindow).GetField("_acesso", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(janela)!;
                acesso.Cadastrar("teste1234");
                Assert.True(acesso.Entrar("teste1234"));
                typeof(MainWindow).GetMethod("AtualizarPerfil", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(janela, null);
                MainWindow.IrParaProdutosCommand.Execute(null, janela);
                Assert.Equal("ativa", ((Button)janela.FindName("BotaoProdutos")).Tag);
                MedirEOrganizar(raizVisual);
                SalvarPrevia(raizVisual, "abas-gestor-1200");
                raizVisual.Measure(new Size(800, 600));
                raizVisual.Arrange(new Rect(0,0,800,600));
                raizVisual.UpdateLayout();
                SalvarPrevia(raizVisual, "abas-gestor-800");
                ((Button)janela.FindName("BotaoPerfil")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal("ativa", abaAtendimento.Tag);
                Assert.Equal(Visibility.Collapsed, ((Button)janela.FindName("BotaoProdutos")).Visibility);
                var atendimento = Assert.IsType<AtendimentoView>(conteudo.Content);
                var id = atendimento.ViewModel.ComandaAtual!.Id;
                atendimento.ViewModel.FecharEdicaoCommand.Execute(null);
                Assert.False(area.IsEnabled);
                Assert.Equal(Visibility.Visible, camada.Visibility);
                Assert.Null(conteudo.Content);
                Assert.Contains(atendimento.ViewModel.ComandasAbertas, c => c.Id == id);
                Descendentes(bloqueio).OfType<Button>().Single(b => Equals(b.Content, "Voltar ao início")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.True(inicioHost.IsEnabled);
                Assert.False(area.IsEnabled);
                ((Button)inicio.FindName("BotaoCozinha")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                foreach (var digito in "0123")
                    botoes.Single(b => Equals(b.Content, digito.ToString())).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                ((Button)bloqueio.FindName("BotaoEntrar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var cozinha = Assert.IsType<CozinhaView>(conteudo.Content);
                Assert.Equal("ativa", ((Button)janela.FindName("BotaoCozinha")).Tag);
                Assert.Equal(Visibility.Collapsed, abaAtendimento.Visibility);
                Assert.False(MainWindow.IrParaAtendimentoCommand.CanExecute(null, janela));
                Assert.False(MainWindow.IrParaProdutosCommand.CanExecute(null, janela));
                Assert.False(MainWindow.IrParaHistoricoCommand.CanExecute(null, janela));
                Assert.Contains(atendimento.ViewModel.ComandasAbertas, c => c.Id == id);
                MedirEOrganizar(raizVisual);
                SalvarPrevia(raizVisual, "cozinha-consulta");
            });
        }
        finally { if (File.Exists(arquivo)) File.Delete(arquivo); var gestor = Path.Combine(raiz, "gestor.credencial"); if (File.Exists(gestor)) File.Delete(gestor); if (Directory.Exists(raiz)) Directory.Delete(raiz); }
    }

    private static IEnumerable<DependencyObject> Descendentes(DependencyObject raiz)
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(raiz); i++)
        {
            var filho = System.Windows.Media.VisualTreeHelper.GetChild(raiz, i);
            yield return filho;
            foreach (var neto in Descendentes(filho)) yield return neto;
        }
    }

    private static void SalvarPrevia(FrameworkElement elemento, string nome)
    {
        var pasta = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
        if (string.IsNullOrWhiteSpace(pasta)) return;
        Directory.CreateDirectory(pasta);
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(
            (int)elemento.ActualWidth, (int)elemento.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(elemento);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
        using var arquivo = File.Create(Path.Combine(pasta, nome + ".png"));
        encoder.Save(arquivo);
    }
}
