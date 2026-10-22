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

    internal static MainWindow CriarMainWindow(VarthexComanda.Infrastructure.Storage.AppPaths? paths = null)
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
    public void AjudaPermiteUsarRibbonSemBloquearSistemaEReutilizaJanelaAberta()
    {
        var pasta=Path.Combine(Path.GetTempPath(),"ajuda-livre-"+Guid.NewGuid());
        try
        {
            ThreadingHelper.EmSta(()=>
            {
                GarantirApp();
                var janela=CriarMainWindow(new VarthexComanda.Infrastructure.Storage.AppPaths(pasta));
                janela.Opacity=0;janela.ShowInTaskbar=false;
                try
                {
                    janela.Show();
                    var botaoAjuda=(Button)janela.FindName("BotaoAjudaFlutuante");
                    Assert.Equal("Abrir opções de ajuda",System.Windows.Automation.AutomationProperties.GetName(botaoAjuda));
                    var abrir=typeof(MainWindow).GetMethod("AbrirSuporte",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
                    abrir.Invoke(janela,null);
                    var ajuda=Assert.Single(janela.OwnedWindows.OfType<VarthexComanda.Desktop.Ajuda.JanelaSuporte>());
                    Assert.True(janela.IsEnabled);Assert.True(ajuda.IsVisible);
                    Assert.Equal("Fechar ajuda",System.Windows.Automation.AutomationProperties.GetName(botaoAjuda));
                    Assert.Equal("Fechar ajuda",botaoAjuda.ToolTip);
                    janela.UpdateLayout();
                    void CapturarBotao(string nome)
                    {
                        var previa=new System.Windows.Shapes.Rectangle { Width=54,Height=54,Fill=new System.Windows.Media.VisualBrush(botaoAjuda) };
                        previa.Measure(new Size(54,54));previa.Arrange(new Rect(0,0,54,54));
                        SalvarPrevia(previa,nome);
                    }
                    CapturarBotao("ajuda-botao-x");
                    var categorias=(WrapPanel)janela.FindName("CategoriasRibbon");
                    var produtos=categorias.Children.OfType<Button>().Single(b=>Equals(b.Content,"Produtos"));
                    Assert.True(produtos.IsEnabled);produtos.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.Equal("ativa",produtos.Tag);
                    abrir.Invoke(janela,null);
                    Assert.Same(ajuda,Assert.Single(janela.OwnedWindows.OfType<VarthexComanda.Desktop.Ajuda.JanelaSuporte>()));
                    botaoAjuda.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.Empty(janela.OwnedWindows.OfType<VarthexComanda.Desktop.Ajuda.JanelaSuporte>());
                    Assert.Equal("Abrir opções de ajuda",System.Windows.Automation.AutomationProperties.GetName(botaoAjuda));
                    janela.UpdateLayout();CapturarBotao("ajuda-botao-original");
                    abrir.Invoke(janela,null);
                    var reaberta=Assert.Single(janela.OwnedWindows.OfType<VarthexComanda.Desktop.Ajuda.JanelaSuporte>());
                    Assert.NotSame(ajuda,reaberta);Assert.True(janela.IsEnabled);reaberta.Close();
                    Assert.Equal("Abrir opções de ajuda",System.Windows.Automation.AutomationProperties.GetName(botaoAjuda));
                }
                finally { janela.Close(); }
            });
        }
        finally { if(Directory.Exists(pasta))Directory.Delete(pasta,true); }
    }

    [Fact]
    public void ConfiguracoesSistemaSecoesSalvamEmModalEUsamTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            GarantirApp();
            var repo=new VarthexComanda.Application.Tests.Configuracao.FakeConfiguracaoRepository();
            var service=new VarthexComanda.Application.Configuracao.ConfiguracoesSistemaService(repo);
            var atual=new VarthexComanda.Application.Configuracao.ConfiguracaoEstabelecimento { NomeEstabelecimento="Loja de teste",QuantidadeMaximaComandas=20 };
            var janela=new VarthexComanda.Desktop.Configuracao.JanelaConfiguracoesSistema(service,atual,()=>0,()=>Array.Empty<Produto>(),()=>"VARTHEX — diagnóstico de teste",_=>{},()=>{},()=>null);
            Assert.Equal(ResizeMode.CanResize,janela.ResizeMode);
            Assert.Equal(WindowStartupLocation.CenterOwner,janela.WindowStartupLocation);
            var raiz=(FrameworkElement)janela.Content;
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro);
                foreach(var largura in new[]{1180d,800d})
                {
                    foreach(var secao in VarthexComanda.Desktop.Configuracao.JanelaConfiguracoesSistema.Secoes)
                    {
                        raiz.Measure(new Size(largura,700));raiz.Arrange(new Rect(0,0,largura,700));raiz.UpdateLayout();
                        Descendentes(raiz).OfType<RadioButton>().Single(b=>Equals(b.Content,secao)).IsChecked=true;
                        raiz.Measure(new Size(largura,700));raiz.Arrange(new Rect(0,0,largura,700));raiz.UpdateLayout();
                        SalvarPrevia(raiz,$"sistema-{Array.IndexOf(VarthexComanda.Desktop.Configuracao.JanelaConfiguracoesSistema.Secoes,secao)}-{(escuro?"escuro":"claro")}-{largura}");
                    }
                }
            }
            Descendentes(raiz).OfType<RadioButton>().Single(b=>Equals(b.Content,"Geral")).IsChecked=true;
            raiz.Measure(new Size(1180,700));raiz.Arrange(new Rect(0,0,1180,700));raiz.UpdateLayout();
            Descendentes(raiz).OfType<TextBox>().Single(t=>System.Windows.Automation.AutomationProperties.GetName(t)=="Cidade").Text="Curitiba";
            Assert.True(janela.TemAlteracoes);
            Descendentes(raiz).OfType<Button>().Single(b=>Equals(b.Content,"Salvar alterações")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(janela.TemAlteracoes);Assert.Equal("Curitiba",service.Carregar().Cidade);
            TemaInterface.Aplicar(false);
        });
    }

    [Fact]
    public void ComandaAbreModalMaximizavelEFechaPreservandoConsumo()
    {
        ThreadingHelper.EmSta(()=>
        {
            GarantirApp();
            var (vm,comanda)=CriarAtendimento();
            var view=new AtendimentoView(vm);
            var owner=new Window { Width=900,Height=650,ShowInTaskbar=false };
            Exception? falha=null;
            var timer=new System.Windows.Threading.DispatcherTimer { Interval=TimeSpan.FromMilliseconds(100) };
            timer.Tick+=(_,_)=>
            {
                var modal=owner.OwnedWindows.OfType<Window>().FirstOrDefault();
                if(modal is null)return;
                timer.Stop();
                try
                {
                    Assert.Equal(ResizeMode.CanResize,modal.ResizeMode);
                    Assert.Same(owner,modal.Owner);
                    Assert.Contains($"{comanda.Numero:00}",modal.Title);
                    modal.WindowState=WindowState.Maximized;
                    Assert.Equal(WindowState.Maximized,modal.WindowState);
                    modal.WindowState=WindowState.Normal;
                    var conteudo=(FrameworkElement)modal.Content;
                    MedirEOrganizar(conteudo);
                    SalvarPrevia(conteudo,"comanda-modal");
                }
                catch(Exception ex){falha=ex;}
                finally{modal.Close();}
            };
            try
            {
                owner.Show();
                timer.Start();
                typeof(AtendimentoView).GetMethod("AbrirJanelaComanda",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.Invoke(view,new object[]{owner});
                if(falha is not null)throw falha;
                Assert.Null(vm.ComandaAtual);
                Assert.Contains(vm.ComandasAbertas,c=>c.Id==comanda.Id && c.TotalCentavos==1000);
                vm.SelecionarComandaCommand.Execute(comanda);
                Assert.Equal(2,vm.Itens.Single().Quantidade);
                var painel=(Grid)view.FindName("PainelEdicao");
                Assert.Contains(painel,Assert.IsAssignableFrom<Panel>(painel.Parent).Children.Cast<UIElement>());
            }
            finally{timer.Stop();owner.Close();}
        });
    }

    [Fact]
    public void LoginCentralUsaCredenciaisExistentesERestringeRecuperacaoAoGestor()
    {
        ThreadingHelper.EmSta(()=>
        {
            GarantirApp();
            var pasta=Path.Combine(Path.GetTempPath(),"login-ui-"+Guid.NewGuid());
            var gestor=new AcessoGestor(Path.Combine(pasta,"gestor.credencial"));
            var equipe=new AcessoAtendimento(Path.Combine(pasta,"atendimento.credencial"));
            foreach(var recuperacao in new[]{false,true})
            {
                var login=new JanelaLogin(gestor,equipe,recuperacao);
                Assert.Equal(WindowStartupLocation.CenterOwner,login.WindowStartupLocation);
                var conteudo=(FrameworkElement)login.Content;
                conteudo.Measure(new Size(420,600));conteudo.Arrange(new Rect(0,0,420,600));conteudo.UpdateLayout();
                var perfis=Descendentes(conteudo).OfType<ComboBox>().Single();
                Assert.Equal(recuperacao?1:3,perfis.Items.Count);
                Assert.Equal("gestor",perfis.SelectedValue);
                Assert.Single(Descendentes(conteudo).OfType<PasswordBox>());
                Assert.False(gestor.Autenticado);
                Assert.False(equipe.Autenticado);
                SalvarPrevia(conteudo,recuperacao?"login-recuperacao":"login-central");
            }
        });
    }

    [Fact]
    public void RibbonPreservaConteudoFiltrosPermissoesETemaEntreSessoes()
    {
        var pasta=Path.Combine(Path.GetTempPath(),"ribbon-"+Guid.NewGuid());
        try
        {
            ThreadingHelper.EmSta(()=>
            {
                GarantirApp();
                var janela=CriarMainWindow(new VarthexComanda.Infrastructure.Storage.AppPaths(pasta));
                var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
                var tipo=typeof(MainWindow);
                var acesso=(AcessoGestor)tipo.GetField("_acesso",flags)!.GetValue(janela)!;
                acesso.Cadastrar("senha-teste-123");
                tipo.GetField("_bloqueado",flags)!.SetValue(janela,false);
                ((FrameworkElement)janela.FindName("MenuInicial")).Visibility=Visibility.Collapsed;
                ((FrameworkElement)janela.FindName("CamadaBloqueio")).Visibility=Visibility.Collapsed;
                var area=(FrameworkElement)janela.FindName("AreaSistema");area.Visibility=Visibility.Visible;area.IsEnabled=true;
                tipo.GetMethod("AtualizarPerfil",flags)!.Invoke(janela,null);
                var conteudo=(AreaTrabalhoInterna)janela.FindName("ConteudoPrincipal");
                MainWindow.IrParaAtendimentoCommand.Execute(null,janela);
                var janelaAtendimento=Assert.IsType<JanelaInterna>(conteudo.JanelaAtiva);
                var atendimento=Assert.IsType<AtendimentoView>(janelaAtendimento.Conteudo);
                atendimento.ViewModel.ObservacaoNovoItem="Sem cebola";
                atendimento.ViewModel.TextoBuscaCatalogo="Refrigerante";
                var categorias=(WrapPanel)janela.FindName("CategoriasRibbon");
                Assert.Equal(new[]{"Principal","Produtos","Marketing","Financeiro","Apps","Configurações","Ajuda"},categorias.Children.OfType<Button>().Select(b=>(string)b.Content));
                void Categoria(string nome)=>categorias.Children.OfType<Button>().Single(b=>Equals(b.Content,nome)).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                void Comando(string nome)=>Descendentes((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>().Single(b=>System.Windows.Automation.AutomationProperties.GetName(b)==nome).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Categoria("Produtos");Assert.Same(janelaAtendimento,conteudo.JanelaAtiva);
                Comando("Produtos");var janelaProdutos=Assert.IsType<JanelaInterna>(conteudo.JanelaAtiva);var produtos=Assert.IsType<ProdutosView>(janelaProdutos.Conteudo);
                Assert.Equal(2,conteudo.Janelas.Count);Assert.Contains(janelaAtendimento,conteudo.Janelas);
                ((ProdutosViewModel)produtos.DataContext).TextoBusca="Refrigerante";
                Descendentes(janelaProdutos).OfType<Button>().Single(b=>System.Windows.Automation.AutomationProperties.GetName(b)=="Fechar Produtos").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Same(janelaAtendimento,conteudo.JanelaAtiva);Comando("Produtos");Assert.Same(janelaProdutos,conteudo.JanelaAtiva);
                Assert.Equal("Refrigerante",((ProdutosViewModel)produtos.DataContext).TextoBusca);
                Categoria("Marketing");Assert.Same(janelaProdutos,conteudo.JanelaAtiva);
                Assert.Empty(Descendentes((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>());
                Categoria("Principal");Comando("Mesas / comandas");
                Assert.Equal("Sem cebola",atendimento.ViewModel.ObservacaoNovoItem);
                Assert.Same(janelaAtendimento,conteudo.JanelaAtiva);
                Descendentes(janelaAtendimento).OfType<Button>().Single(b=>System.Windows.Automation.AutomationProperties.GetName(b)=="Fechar Mesas / comandas").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Same(janelaProdutos,conteudo.JanelaAtiva);
                Comando("Mesas / comandas");Assert.Same(janelaAtendimento,conteudo.JanelaAtiva);
                Assert.Equal("Sem cebola",atendimento.ViewModel.ObservacaoNovoItem);
                Categoria("Configurações");
                Assert.Equal(new[]{"Configurações do sistema","Numeração de pedidos","Impressoras","Colaboradores","Alterar senha","Backup e restauração","Minha assinatura"},Descendentes((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>().Select(b=>System.Windows.Automation.AutomationProperties.GetName(b)));
                var config=(ConfiguracaoView)tipo.GetField("_configuracaoView",flags)!.GetValue(janela)!;config.ViewModel.NomeEstabelecimento="Nome em edição";
                Categoria("Produtos");Comando("Produtos");Assert.Equal("Refrigerante",((ProdutosViewModel)produtos.DataContext).TextoBusca);
                Categoria("Configurações");Assert.Equal("Nome em edição",config.ViewModel.NomeEstabelecimento);
                config.ViewModel.DescartarCommand.Execute(null);
                Categoria("Produtos");Comando("Produtos");
                var pesquisa=(TextBox)janela.FindName("PesquisaGlobal");pesquisa.Text="backup";
                var resultados=(ListBox)janela.FindName("ResultadosPesquisa");Assert.NotEmpty(resultados.Items.Cast<object>());
                pesquisa.Clear();
                var raiz=(FrameworkElement)janela.Content;
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,true);
                    ((CheckBox)janela.FindName("ModoEscuro")).IsChecked=escuro;
                    foreach(var tamanho in new[]{new Size(1280,780),new Size(800,600)})
                    {
                        janela.Width=tamanho.Width;tipo.GetMethod("AtualizarRibbonCompacta",flags)!.Invoke(janela,null);
                        raiz.Measure(tamanho);raiz.Arrange(new Rect(new Point(0,0),tamanho));raiz.UpdateLayout();
                        SalvarPrevia(raiz,$"ribbon-produtos-{(escuro?"escuro":"claro")}-{tamanho.Width}");
                    }
                    janela.Width=1280;
                    raiz.Measure(new Size(1280,780));raiz.Arrange(new Rect(0,0,1280,780));raiz.UpdateLayout();
                    tipo.GetMethod("AtualizarRibbonCompacta",flags)!.Invoke(janela,null);
                    raiz.UpdateLayout();
                    Assert.Equal(Visibility.Visible,((FrameworkElement)janela.FindName("GruposRibbon")).Visibility);
                    SalvarPrevia(raiz,$"ribbon-expandida-{(escuro?"escuro":"claro")}");
                    janelaProdutos.AlternarMaximizar();raiz.UpdateLayout();
                    SalvarPrevia(raiz,$"ribbon-produtos-maximizado-{(escuro?"escuro":"claro")}");
                    janelaProdutos.AlternarMaximizar();raiz.UpdateLayout();
                    Categoria("Principal");Comando("Mesas / comandas");
                    raiz.Measure(new Size(1280,780));raiz.Arrange(new Rect(0,0,1280,780));raiz.UpdateLayout();
                    SalvarPrevia(raiz,$"ribbon-atendimento-{(escuro?"escuro":"claro")}");
                    var aviso=new ConfirmacaoView("Conferir comanda","Confira os itens antes de encerrar.");
                    var avisoConteudo=(FrameworkElement)aviso.Content;MedirEOrganizar(avisoConteudo);SalvarPrevia(avisoConteudo,$"ribbon-dialogo-{(escuro?"escuro":"claro")}");
                    Categoria("Configurações");
                    raiz.Measure(new Size(1280,780));raiz.Arrange(new Rect(0,0,1280,780));raiz.UpdateLayout();
                    SalvarPrevia(raiz,$"ribbon-configuracoes-{(escuro?"escuro":"claro")}");
                    Categoria("Ajuda");raiz.UpdateLayout();
                    Assert.Equal(new[]{"Ajuda"},Descendentes((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>().Select(b=>System.Windows.Automation.AutomationProperties.GetName(b)));
                    SalvarPrevia(raiz,$"ribbon-ajuda-{(escuro?"escuro":"claro")}");
                    // As duas telas continuam abertas e podem ocupar posições independentes.
                    Assert.Equal(2,conteudo.Janelas.Count);
                    raiz.Measure(new Size(1920,1080));raiz.Arrange(new Rect(0,0,1920,1080));raiz.UpdateLayout();
                    AreaTrabalhoInternaTests.Redimensionar(janelaAtendimento,"inferior direita",-400,-180);
                    AreaTrabalhoInternaTests.Mover(janelaAtendimento,-500,-300);
                    AreaTrabalhoInternaTests.Redimensionar(janelaProdutos,"inferior direita",-400,-180);
                    AreaTrabalhoInternaTests.Mover(janelaProdutos,600,220);
                    raiz.UpdateLayout();
                    SalvarPrevia(raiz,$"area-trabalho-organizada-{(escuro?"escuro":"claro")}");
                    conteudo.Ativar(janelaAtendimento);janelaAtendimento.AlternarMaximizar();raiz.UpdateLayout();
                    Assert.Equal(conteudo.RenderSize,AreaTrabalhoInternaTests.Moldura(janelaAtendimento).RenderSize);
                    SalvarPrevia(raiz,$"area-trabalho-maximizada-{(escuro?"escuro":"claro")}");
                    janelaAtendimento.AlternarMaximizar();
                    Categoria("Produtos");Comando("Produtos");
                    TemaInterface.Inicializar(pasta);Assert.Equal(escuro,TemaInterface.Escuro);
                }
                acesso.Sair();tipo.GetMethod("AtualizarPerfil",flags)!.Invoke(janela,null);
                pesquisa.Text="backup";Assert.NotEmpty(resultados.Items.Cast<object>());
                Categoria("Produtos");Assert.Single(Descendentes((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>());
                Assert.False(MainWindow.IrParaProdutosCommand.CanExecute(null,janela));
                TemaInterface.Aplicar(false);
            });
        }
        finally{if(Directory.Exists(pasta))Directory.Delete(pasta,true);}
    }

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
    public void AtendimentoComMesaEObservacaoRenderizaEmDuasResolucoes()
    {
        ThreadingHelper.EmSta(() =>
        {
            GarantirApp();
            var (vm, _) = CriarAtendimento();
            Assert.True(vm.SalvarDadosComanda("mesa", "12"));
            vm.ObservacaoNovoItem = "Sem gelo. Entregar junto com os lanches.";
            vm.AdicionarProdutoAoItemCommand.Execute(vm.ProdutosCatalogo.First());
            var view = new AtendimentoView(vm);
            foreach (var tamanho in new[] { new Size(1280, 780), new Size(800, 600) })
            {
                view.Measure(tamanho);
                view.Arrange(new Rect(new Point(0, 0), tamanho));
                view.UpdateLayout();
                Assert.Contains(Descendentes(view).OfType<TextBlock>(), t => t.Text == "Sem gelo. Entregar junto com os lanches.");
                Assert.Equal(12, vm.ComandaAtual!.NumeroMesa);
                SalvarPrevia(view, $"mesa-observacao-{tamanho.Width}");
            }
            var cozinha = new CozinhaView(vm.ConsultarCozinha);
            cozinha.Atualizar();
            MedirEOrganizar(cozinha);
            Assert.Contains(Descendentes(cozinha).OfType<TextBlock>(), t => t.Text == "Mesa 12");
            Assert.Contains(Descendentes(cozinha).OfType<TextBlock>(), t => t.Text == "Sem gelo. Entregar junto com os lanches.");
            SalvarPrevia(cozinha, "cozinha-mesa-observacao");
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
                    .Single(w => w.Title == "Novo produto" && ReferenceEquals(w.DataContext, viewModel));
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
                var conteudo = (AreaTrabalhoInterna)janela.FindName("ConteudoPrincipal");
                var raizVisual = (FrameworkElement)janela.Content;
                MedirEOrganizar(raizVisual);
                var inicioHost = (ContentControl)janela.FindName("MenuInicial");
                Assert.Equal(Visibility.Collapsed, inicioHost.Visibility);
                Assert.Equal(Visibility.Collapsed, camada.Visibility);
                Assert.True(area.IsEnabled);
                SalvarPrevia(raizVisual, "inicio-vcomanda-1200");
                raizVisual.Measure(new Size(800, 600));
                raizVisual.Arrange(new Rect(0,0,800,600));
                raizVisual.UpdateLayout();
                SalvarPrevia(raizVisual, "inicio-vcomanda-800");
                Descendentes((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>()
                    .Single(b=>System.Windows.Automation.AutomationProperties.GetName(b)=="Mesas / comandas")
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                MedirEOrganizar(raizVisual);
                Assert.False(area.IsEnabled);
                Assert.Null(conteudo.JanelaAtiva);Assert.Empty(conteudo.Janelas);
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
                Assert.Null(conteudo.JanelaAtiva);Assert.Empty(conteudo.Janelas);
                Assert.Null(abaAtendimento.Tag);
                MainWindow.IrParaAtendimentoCommand.Execute(null, janela);
                Assert.Equal(Visibility.Collapsed, ((Button)janela.FindName("BotaoProdutos")).Visibility);
                var atendimento = Assert.IsType<AtendimentoView>(Assert.IsType<JanelaInterna>(conteudo.JanelaAtiva).Conteudo);
                var id = atendimento.ViewModel.ComandaAtual!.Id;
                atendimento.ViewModel.FecharEdicaoCommand.Execute(null);
                Assert.False(area.IsEnabled);
                Assert.Equal(Visibility.Visible, camada.Visibility);
                Assert.Null(conteudo.JanelaAtiva);Assert.Empty(conteudo.Janelas);
                Assert.Contains(atendimento.ViewModel.ComandasAbertas, c => c.Id == id);
                Descendentes(bloqueio).OfType<Button>().Single(b => Equals(b.Content, "Voltar ao início")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(Visibility.Collapsed,inicioHost.Visibility);
                Assert.True(area.IsEnabled);
                typeof(MainWindow).GetMethod("EscolherArea",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.Invoke(janela,new object[]{"cozinha"});
                foreach (var digito in "0123")
                    botoes.Single(b => Equals(b.Content, digito.ToString())).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                ((Button)bloqueio.FindName("BotaoEntrar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var cozinha = Assert.IsType<CozinhaView>(Assert.IsType<JanelaInterna>(conteudo.JanelaAtiva).Conteudo);
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
