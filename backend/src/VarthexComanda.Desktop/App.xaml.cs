using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using VarthexComanda.Desktop.Licenciamento;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Backup;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Desktop.Catalogo;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Infrastructure.Backup;
using VarthexComanda.Infrastructure.Concurrency;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Logging;
using VarthexComanda.Infrastructure.Persistence;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Storage;
using VarthexComanda.Infrastructure.Time;

namespace VarthexComanda.Desktop;

public partial class App : System.Windows.Application
{
    public App()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, e) =>
        {
            if (sender is Window janela && ReferenceEquals(e.OriginalSource, janela))
            {
                AparenciaJanelaModal.Aplicar(janela);
                LimitesJanelaModal.Aplicar(janela);
            }
        }));
    }
    private SingleInstanceGuard? _guard;
    private ILogger? _logger;
    private ServiceProvider? _serviceProvider;
    private bool _startupConcluido;
    private ServicoLicenca? _licenca;
    private DispatcherTimer? _timerLicenca;
    private bool _ativando;
    private Task _backupInicial = Task.CompletedTask;
    private JanelaAbertura? _abertura;
    private bool _cancelouInicializacao;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        TemaInterface.Inicializar(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VarthexComanda"));

        _guard = new SingleInstanceGuard("VarthexComanda.SingleInstance");
        if (!_guard.TryAcquire())
        {
            JanelaAviso.Mostrar(
                "Comanda",
                "O Comanda já está aberto neste computador.",
                TipoAviso.Informacao);
            Shutdown();
            return;
        }

        try
        {
            await MostrarAberturaAsync();
            if (!_cancelouInicializacao) await InicializarSistemaAsync(e);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha na inicialização do sistema");
            FecharAbertura();
            JanelaAviso.Mostrar("Inicialização", "Não foi possível iniciar o sistema. Confira as permissões da pasta de dados e tente novamente.", TipoAviso.Erro);
            Shutdown();
        }
        finally { FecharAbertura(); }
    }

    private async Task InicializarSistemaAsync(StartupEventArgs e)
    {
        try
        {
            _licenca = new ServicoLicenca();
            if (!GarantirLicenca()) { Shutdown(); return; }
        }
        catch (Exception ex)
        {
            FecharAbertura();
            JanelaAviso.Mostrar("Comanda", "Não foi possível verificar a licença. " + ex.Message, TipoAviso.Erro);
            Shutdown();
            return;
        }

        await MostrarAberturaAsync();
        if (_cancelouInicializacao) return;
        _abertura?.AtualizarEtapa("Preparando os dados…");
        var demonstracao = ModoExecucao.Demonstracao(e.Args, AppContext.BaseDirectory);
        var raizDados = ModoExecucao.RaizDados(demonstracao,
            Environment.GetEnvironmentVariable("VARTHEX_COMANDA_DADOS"),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        var paths = new AppPaths(string.IsNullOrWhiteSpace(raizDados) ? null : raizDados);
        paths.EnsureCreated();
        VarthexComanda.Desktop.Catalogo.FotoArquivoParaImagemConverter.DiretorioFotos = paths.FotosDirectory;

        // RF26: o log mascara o perfil do usuario, a pasta de dados fora do perfil (%DADOS%) e a pasta de
        // backup externo (registrada pelo EfBackupService quando a usa)
        var pastasMascaradas = new PastasMascaradas();
        pastasMascaradas.AdicionarDados(paths.Root);
        _logger = LoggingConfigurator.CreateLogger(paths.LogsDirectory, pastas: pastasMascaradas);
        _logger.Information("Iniciando Comanda {Versao}", VersaoDoAplicativo.Atual);

        DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.Error(args.Exception, "Erro nao tratado na interface");
            FecharAbertura();
            JanelaAviso.Mostrar(
                "Comanda",
                "Ocorreu um erro inesperado no Comanda. Consulte os logs.",
                TipoAviso.Erro);
            args.Handled = true;
        };

        var services = new ServiceCollection();
        services.AddSingleton(paths);
        services.AddSingleton(pastasMascaradas);
        services.AddSingleton(_logger);
        services.AddSingleton<IClock, SystemClock>();
        services.AddDbContextFactory<VarthexComandaDbContext>(options =>
            options.UseSqlite($"Data Source={paths.DatabasePath};Foreign Keys=True"));
        services.AddTransient<ICategoriaRepository, EfCategoriaRepository>();
        services.AddTransient<IProdutoRepository, EfProdutoRepository>();
        services.AddTransient<CadastrarCategoria>();
        services.AddTransient<AlterarCategoria>();
        services.AddTransient<ListarCategoriasAtivas>();
        services.AddTransient<CadastrarProduto>();
        services.AddTransient<AlterarProduto>();
        services.AddTransient<DesativarProduto>();
        services.AddTransient<PesquisarProdutos>();
        services.AddSingleton<IFotoStorage>(sp => new ArquivoFotoStorage(
            sp.GetRequiredService<AppPaths>(),
            sp.GetRequiredService<ILogger>()));
        services.AddTransient<DefinirFotoProduto>();
        services.AddTransient<RemoverFotoProduto>();
        services.AddTransient<IComandaRepository, EfComandaRepository>();
        services.AddTransient<AbrirComanda>();
        services.AddTransient<AdicionarItem>();
        services.AddSingleton(new VarthexComanda.Servidor.FotosProdutos(paths.FotosDirectory));
        services.AddSingleton<VarthexComanda.Servidor.AtendimentoRemoto>();
        services.AddSingleton<VarthexComanda.Servidor.ServidorLocal>(sp => new(sp.GetRequiredService<VarthexComanda.Servidor.AtendimentoRemoto>(),
            () => { try { _licenca!.Verificar(); return true; } catch { return false; } }));
        services.AddTransient<AlterarQuantidade>();
        services.AddTransient<RemoverItem>();
        services.AddTransient<CancelarComanda>();
        services.AddTransient<IConfirmador, JanelaConfirmador>();
        services.AddTransient<EncerrarComanda>();
        services.AddTransient<EncerramentoViewModel>();
        services.AddTransient<Func<EncerramentoViewModel>>(sp => () => sp.GetRequiredService<EncerramentoViewModel>());
        services.AddTransient<IEncerramentoDialog, EncerramentoDialog>();
        services.AddTransient<IVendaRepository, EfVendaRepository>();
        services.AddTransient<ListarVendasPorData>();
        services.AddTransient<BuscarItensDaVenda>();
        services.AddTransient<HistoricoViewModel>();
        services.AddTransient<HistoricoView>();
        services.AddTransient<IHistoricoPedidosRepository, EfHistoricoPedidosRepository>();
        services.AddTransient<ConsultarHistoricoPedidos>();
        services.AddTransient<HistoricoPedidosViewModel>(sp => new HistoricoPedidosViewModel(
            sp.GetRequiredService<ConsultarHistoricoPedidos>(), sp.GetRequiredService<IClock>(),
            sp.GetRequiredService<ILogger>(), Path.Combine(sp.GetRequiredService<AppPaths>().Root, "historico-pedidos.json")));
        services.AddTransient<HistoricoPedidosView>();
        services.AddTransient<IBackupRegistroRepository, EfBackupRegistroRepository>();
        services.AddSingleton<IGoogleDriveConnection>(sp => new GoogleDriveConnection(sp.GetRequiredService<AppPaths>(), new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler { AllowAutoRedirect=false }) { Timeout=TimeSpan.FromMinutes(3) }));
        services.AddSingleton<IGoogleDriveBackup>(sp => new GoogleDriveBackup(sp.GetRequiredService<IGoogleDriveConnection>(), new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler { AllowAutoRedirect=false }) { Timeout=TimeSpan.FromMinutes(10) }));
        services.AddTransient<IBackupService, EfBackupService>();
        services.AddTransient<IBackupFotosService, EfBackupService>();
        services.AddTransient<CriarBackupAutomatico>();
        services.AddTransient<CriarBackupManual>();
        services.AddTransient<ValidarBackup>();
        services.AddTransient<RestaurarBackup>();
        services.AddTransient<ListarBackupsRecentes>();
        services.AddTransient<BackupViewModel>();
        services.AddTransient<BackupView>();
        services.AddTransient<IConfiguracaoRepository, EfConfiguracaoRepository>();
        services.AddTransient<ObterConfiguracao>();
        services.AddTransient<SalvarConfiguracao>();
        services.AddTransient<ConfiguracaoViewModel>();
        services.AddTransient<ConfiguracaoView>();
        services.AddTransient<AtendimentoViewModel>();
        services.AddTransient<AtendimentoView>();
        services.AddTransient<ProdutosViewModel>();
        services.AddTransient<ProdutosView>();
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var bancoCorrompido = false;
        var janelaPrincipalExibida = false;
        try
        {
            var factory = _serviceProvider.GetRequiredService<IDbContextFactory<VarthexComandaDbContext>>();
            using var dbContext = factory.CreateDbContext();

            // A integridade é verificada ANTES de migrar: nunca aplicamos migrações (escritas) num
            // arquivo corrompido, e um arquivo que nem é SQLite também cai aqui (o PRAGMA lança
            // SQLITE_NOTADB/SQLITE_CORRUPT), em vez de virar "falha ao preparar o banco".
            var linhas = dbContext.Database
                .SqlQueryRaw<string>("PRAGMA integrity_check")
                .AsEnumerable()
                .ToList();
            if (linhas.Count != 1 || linhas[0] != "ok")
            {
                // uma base muito corrompida pode devolver milhares de linhas: registra so as 5 primeiras e o total
                _logger.Error("PRAGMA integrity_check retornou {Total} linha(s); primeiras: {Linhas}", linhas.Count, string.Join("; ", linhas.Take(5)));
                bancoCorrompido = true;
            }
            else
            {
                // instalação nova (nenhuma migração aplicada) não tem dados a preservar: sem backup preventivo
                if (MigracaoDoBanco.ExigeBackupPreventivo(dbContext.Database, out var pendentes))
                {
                    var backupService = _serviceProvider.GetRequiredService<IBackupService>();
                    var resultadoPreventivo = backupService.CriarBackupGerenciado();
                    if (resultadoPreventivo.Sucesso)
                    {
                        _logger.Information("Backup preventivo criado antes de aplicar {Quantidade} migração(ões) pendente(s)", pendentes);
                    }
                    else
                    {
                        _logger.Warning("Backup preventivo antes da migração falhou: {Mensagem}", string.Join(" ", resultadoPreventivo.Erros));
                    }
                }

                dbContext.Database.Migrate();

                if (demonstracao)
                    CatalogoDemonstracao.Inicializar(dbContext, paths, Path.Combine(AppContext.BaseDirectory, "demo"));

                _logger.Information("Banco pronto");

                var abertas = _serviceProvider.GetRequiredService<IComandaRepository>().ListarAbertas().Count;
                _logger.Information("Comandas abertas recuperadas: {Quantidade}", abertas);

                RegistrarFotosSemProduto(paths);

                _abertura?.AtualizarEtapa("Preparando o atendimento…");
                var janela = _serviceProvider.GetRequiredService<MainWindow>();
                if (_abertura is { } abertura) await abertura.AguardarExibicaoMinimaAsync();
                if (_cancelouInicializacao) { janela.Close(); return; }
                MainWindow = janela;
                janela.Show();
                FecharAbertura();
                janelaPrincipalExibida = true;
                _startupConcluido = true;
                var backupInicial = _serviceProvider.GetRequiredService<CriarBackupAutomatico>();
                _backupInicial = Task.Run(() => backupInicial.Executar());
            }
        }
        // só entra em modo de restauração se a corrupção apareceu antes de a MainWindow existir na tela;
        // depois disso resolver uma segunda MainWindow seria errado (cai no tratamento genérico)
        catch (Exception ex) when (!janelaPrincipalExibida && CorrupcaoDeBanco.EhErroDeCorrupcao(ex))
        {
            _logger.Error(ex, "Banco de dados corrompido ou inválido");
            bancoCorrompido = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Falha ao preparar o banco de dados");
            FecharAbertura();
            JanelaAviso.Mostrar(
                "Comanda",
                "Não foi possível preparar o banco de dados do Comanda. Consulte os logs.",
                TipoAviso.Erro);
            Shutdown();
            return;
        }

        if (bancoCorrompido)
        {
            FecharAbertura();
            AbrirModoRestauracao();
        }
        MainWindow = Windows.OfType<MainWindow>().FirstOrDefault();
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        _timerLicenca = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromSeconds(15) };
        _timerLicenca.Tick += (_, _) => { if (!GarantirLicenca()) Shutdown(); };
        _timerLicenca.Start();
    }

    private async Task MostrarAberturaAsync()
    {
        if (_abertura is not null || _cancelouInicializacao) return;
        var abertura = new JanelaAbertura(); _abertura = abertura;
        abertura.Closed += (_, _) =>
        {
            if (!ReferenceEquals(_abertura, abertura)) return;
            _abertura = null; _cancelouInicializacao = true; Shutdown();
        };
        abertura.Show();
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
    }

    private void FecharAbertura()
    {
        var abertura = _abertura; _abertura = null;
        abertura?.Close();
    }

    public void RenovarLicenca()
    {
        if (_licenca is null || _ativando) return;
        _ativando = true;
        try
        {
            string mensagem;
            try { var atual = _licenca.Verificar(); mensagem = atual.Vencimento is null ? "Licença vitalícia ativa." : $"Licença válida até {atual.Vencimento.Value.ToOffset(TimeSpan.FromHours(-3)):dd/MM/yyyy HH:mm}."; }
            catch (Exception ex) { mensagem = ex.Message; }
            new JanelaLicenca(_licenca, mensagem) { Owner = MainWindow }.ShowDialog();
        }
        finally { _ativando = false; }
        if (!GarantirLicenca()) Shutdown();
    }

    private bool GarantirLicenca()
    {
        if (_ativando) return true;
        try { _licenca!.Verificar(); return true; }
        catch (Exception ex)
        {
            _ativando = true;
            try
            {
                FecharAbertura();
                var janela = new JanelaLicenca(_licenca!, ex is InvalidOperationException ? ex.Message : "Não foi possível ler ou salvar a licença. Verifique as permissões locais.");
                if (MainWindow is MainWindow principal && principal.IsVisible) janela.Owner = principal;
                return janela.ShowDialog() == true;
            }
            finally { _ativando = false; }
        }
    }

    // Só informativo: conta fotos em fotos\ que nenhum produto referencia. Nunca apaga nada (uma
    // restauração de banco antigo poderia deixar fotos válidas "sem produto") e nunca derruba a inicialização.
    private void RegistrarFotosSemProduto(AppPaths paths)
    {
        try
        {
            if (!Directory.Exists(paths.FotosDirectory))
            {
                return;
            }

            var referenciadas = _serviceProvider!.GetRequiredService<IProdutoRepository>()
                .ListarNomesDeFotos()
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var semProduto = Directory.GetFiles(paths.FotosDirectory)
                .Count(f => ArquivoFotoStorage.ExtensaoPermitida(f) && !referenciadas.Contains(Path.GetFileName(f)));
            _logger!.Information("Fotos sem produto: {Quantidade}", semProduto);
        }
        catch (Exception ex)
        {
            _logger?.Warning(ex, "Falha em {Operacao}", "ContarFotosSemProduto");
        }
    }

    // Banco corrompido: abre a janela só com a aba Backup habilitada e a faixa de aviso.
    // Nada de backup automático ao sair (_startupConcluido continua false) e nada de Shutdown:
    // a restauração é sempre uma ação explícita do operador (BackupViewModel confirma e reinicia).
    private void AbrirModoRestauracao()
    {
        try
        {
            _startupConcluido = false;
            var janela = _serviceProvider!.GetRequiredService<MainWindow>();
            janela.EntrarModoRestauracao();
            janela.Show();
        }
        catch (Exception ex)
        {
            _logger!.Error(ex, "Falha em {Operacao}", "AbrirModoRestauracao");
            JanelaAviso.Mostrar(
                "Comanda",
                "O banco de dados está corrompido e não foi possível abrir a tela de restauração. Consulte os logs.",
                TipoAviso.Erro);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        FecharAbertura();
        _timerLicenca?.Stop();
        if (_startupConcluido)
        {
            _backupInicial.GetAwaiter().GetResult();
            _serviceProvider?.GetRequiredService<CriarBackupAutomatico>().Executar(incondicional: true);
        }
        _logger?.Information("Encerrando Comanda");
        _serviceProvider?.Dispose();
        (_logger as IDisposable)?.Dispose();
        _guard?.Dispose();
        base.OnExit(e);
    }

    public void ReiniciarAplicativo()
    {
        _guard?.Release();
        _guard?.Dispose();
        _serviceProvider?.Dispose();

        // o logger so e descartado depois da tentativa, para registrar uma falha ao reiniciar
        try
        {
            var caminhoExecutavel = Environment.ProcessPath
                ?? throw new InvalidOperationException("Não foi possível determinar o caminho do executável.");
            Process.Start(caminhoExecutavel);
        }
        catch (Exception ex)
        {
            _logger?.Warning(ex, "Falha em {Operacao}", "ReiniciarAplicativo");
            JanelaAviso.Mostrar(
                "Comanda",
                "A restauração foi concluída, mas não foi possível reiniciar automaticamente. Feche e abra o Comanda manualmente.",
                TipoAviso.Aviso);
        }
        finally
        {
            (_logger as IDisposable)?.Dispose();
        }

        Environment.Exit(0);
    }
}
