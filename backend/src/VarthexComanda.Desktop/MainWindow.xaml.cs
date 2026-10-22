using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Desktop.Catalogo;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop;

public partial class MainWindow : Window
{
    public static readonly RoutedCommand IrParaAtendimentoCommand = new();
    public static readonly RoutedCommand IrParaProdutosCommand = new();
    public static readonly RoutedCommand IrParaHistoricoCommand = new();
    public static readonly RoutedCommand IrParaBackupCommand = new();
    public static readonly RoutedCommand IrParaConfiguracoesCommand = new();

    private readonly AcessoGestor _acesso;
    private readonly ServicoColaboradores _colaboradores;
    private bool _restauracao;
    private bool _bloqueado = true;
    private bool _cozinhaAtiva;
    private string _areaPendente = "garcom";
    private readonly CozinhaView _cozinhaView;
    private readonly AcessoAtendimento _acessoAtendimento;
    private readonly BloqueioAtendimentoView _bloqueioView;
    private readonly AtendimentoView _atendimentoView;
    private readonly ProdutosView _produtosView;
    private readonly HistoricoView _historicoView;
    private readonly BackupView _backupView;
    private readonly ConfiguracaoView _configuracaoView;


    public MainWindow(AtendimentoView atendimentoView, ProdutosView produtosView, HistoricoView historicoView, BackupView backupView, ConfiguracaoView configuracaoView, VarthexComanda.Infrastructure.Storage.AppPaths? paths = null, VarthexComanda.Application.Configuracao.IConfiguracaoRepository? configuracoes = null)
    {
        InitializeComponent();
        Title = $"Varthex Comanda · Versão {VersaoDoAplicativo.Atual}";
        _acesso = new AcessoGestor(System.IO.Path.Combine((paths ?? new VarthexComanda.Infrastructure.Storage.AppPaths()).Root, "gestor.credencial"));
        _colaboradores = new ServicoColaboradores(System.IO.Path.Combine((paths ?? new VarthexComanda.Infrastructure.Storage.AppPaths()).Root, "colaboradores.json"));
        _acessoAtendimento = new AcessoAtendimento(System.IO.Path.Combine((paths ?? new VarthexComanda.Infrastructure.Storage.AppPaths()).Root, "atendimento.credencial"));
        _acessoAtendimento.GarantirPinInicial();
        _bloqueioView = new BloqueioAtendimentoView(_acessoAtendimento);
        CamadaBloqueio.Content = _bloqueioView;
        _bloqueioView.Desbloqueado += DesbloquearAtendimento;
        _bloqueioView.ConfiguracaoSolicitada += ConfigurarPin;
        _bloqueioView.VoltarSolicitado += MostrarInicio;
        _cozinhaView = new CozinhaView(atendimentoView.ViewModel.ConsultarCozinha);
        RegistrarAtalho(IrParaAtendimentoCommand, BotaoAtendimento, IrParaAtendimento);
        RegistrarAtalho(IrParaProdutosCommand, BotaoProdutos, IrParaProdutos);
        RegistrarAtalho(IrParaHistoricoCommand, BotaoHistorico, IrParaHistorico);
        RegistrarAtalho(IrParaBackupCommand, BotaoBackup, IrParaBackup);
        RegistrarAtalho(IrParaConfiguracoesCommand, BotaoConfiguracoes, IrParaConfiguracoes);
        _atendimentoView = atendimentoView;
        _produtosView = produtosView;
        _historicoView = historicoView;
        _backupView = backupView;
        _configuracaoView = configuracaoView;
        ConteudoPrincipal.JanelaAtivaAlterada += AtualizarAbaAtiva;
        _pathsSistema=paths ?? new VarthexComanda.Infrastructure.Storage.AppPaths();
        _repositorioSistema=configuracoes;
        _atendimentoView.ImpressaoSolicitada=ImprimirConta;
        _atendimentoView.ImprimirPedido=ImprimirPedido;
        _atendimentoView.ViewModel.ObterOperador=()=>_colaboradores.SessaoAtual?.Nome ?? Environment.UserName;
        _atendimentoView.ViewModel.ObterResponsaveis=()=>_colaboradores.Listar().Where(c=>c.Ativo).Select(c=>c.Nome).Append(_colaboradores.SessaoAtual?.Nome ?? Environment.UserName).Distinct().Order().ToArray();
        _atendimentoView.OrigensCliente=()=>LerPreferencias().OrigensCliente;
        _cozinhaView.ConsultarLocais=()=>LerPreferencias().Cozinhas;
        AplicarFundoSistema();
        InicializarRibbon((paths ?? new VarthexComanda.Infrastructure.Storage.AppPaths()).Root);
        _configuracaoView.AcaoSolicitada += AcaoConfiguracao;
        _configuracaoView.ConsultarBackup = () => {
            _backupView.ViewModel.AtualizarLista();
            var ultimo = _backupView.ViewModel.Backups.OrderByDescending(b => b.CriadoEm).FirstOrDefault();
            return ultimo is null ? "Nenhum backup registrado." : $"{ultimo.CriadoEm.ToLocalTime():dd/MM/yyyy HH:mm} · {ultimo.Status}";
        };
        _configuracaoView.ConsultarLicenca = () => {
            try { var atual = new Licenciamento.ServicoLicenca().Verificar(); return atual.Vencimento is null ? "Licença vitalícia ativa · Sem vencimento." : $"Licença ativa até {atual.Vencimento.Value.ToOffset(TimeSpan.FromHours(-3)):dd/MM/yyyy HH:mm}."; }
            catch { return "Não foi possível confirmar a licença. Abra a ativação para verificar."; }
        };
        Closing += (_, e) => { if (_backupView.ViewModel.EmAndamento || (!_bloqueado && !PodeSairDasConfiguracoes())) e.Cancel=true; };
        Closed += (_,_) => _painelSenhas?.Dispose();
        _atendimentoView.ViewModel.AtendimentoConcluido += BloquearAtendimento;
        MostrarInicio();
    }

    /// <summary>
    /// Modo de restauração (banco corrompido): só a aba Backup fica disponível, com a faixa de
    /// aviso no topo. As demais abas ficam desabilitadas para que nada leia ou escreva no banco.
    /// </summary>
    public void EntrarModoRestauracao()
    {
        _restauracao = true;
        BotaoAtendimento.IsEnabled = false;
        BotaoProdutos.IsEnabled = false;
        BotaoHistorico.IsEnabled = false;
        BotaoConfiguracoes.IsEnabled = false;
        FaixaRestauracao.Visibility = Visibility.Visible;

        _backupView.ViewModel.ModoRestauracao = true;
        ConteudoPrincipal.Limpar();
        MostrarInicio();
    }

    // Atalhos Ctrl+1..5 seguem o estado do botao correspondente: no modo de restauracao as abas
    // desabilitadas nao respondem ao teclado (CanExecute = false).
    private void RegistrarAtalho(RoutedCommand comando, Button botao, Action acao)
    {
        CommandBindings.Add(new CommandBinding(
            comando,
            (_, _) => { if (!_bloqueado && botao.IsEnabled) acao(); },
            (_, e) => { e.CanExecute = !_bloqueado && botao.IsEnabled; e.Handled = true; }));
    }

    private void MostrarCozinha_Click(object sender, RoutedEventArgs e)
    {
        if (_bloqueado || !_cozinhaAtiva || _restauracao) return;
        _cozinhaView.Atualizar();
        MostrarConteudo(_cozinhaView);
    }

    private void AtualizarAbaAtiva()
    {
        foreach (var (botao, conteudo) in new (Button, object)[]
        {
            (BotaoAtendimento, _atendimentoView), (BotaoProdutos, _produtosView),
            (BotaoHistorico, _historicoView), (BotaoBackup, _backupView),
            (BotaoConfiguracoes, _configuracaoView), (BotaoCozinha, _cozinhaView)
        })
        {
            var ativa = !_bloqueado && ReferenceEquals(ConteudoPrincipal.JanelaAtiva?.Conteudo, conteudo);
            botao.Tag = ativa ? "ativa" : null;
            System.Windows.Automation.AutomationProperties.SetItemStatus(botao, ativa ? "Tela atual" : "");
        }
    }

    private void MostrarAtendimento_Click(object sender, RoutedEventArgs e) => IrParaAtendimento();

    private void MostrarProdutos_Click(object sender, RoutedEventArgs e) => IrParaProdutos();

    private void MostrarHistorico_Click(object sender, RoutedEventArgs e) => IrParaHistorico();

    private void MostrarBackup_Click(object sender, RoutedEventArgs e) => IrParaBackup();

    private void MostrarConfiguracao_Click(object sender, RoutedEventArgs e) => IrParaConfiguracoes();
    private void MostrarLicenca_Click(object sender, RoutedEventArgs e) { if (!_bloqueado && _acesso.Autenticado && !_restauracao) ((App)System.Windows.Application.Current).RenovarLicenca(); }

    private void IrParaAtendimento()
    {
        if (_bloqueado || _cozinhaAtiva || _restauracao) return;
        _atendimentoView.ViewModel.AtualizarCategorias();
        _atendimentoView.ViewModel.AtualizarComandasAbertas();
        MostrarConteudo(_atendimentoView);
    }

    private readonly Dictionary<UIElement,JanelaInterna> _janelasInternas=new();
    private JanelaInterna ObterJanela(UIElement view)
    {
        if(_janelasInternas.TryGetValue(view,out var existente))return existente;
        var titulo=view==_atendimentoView?"Mesas / comandas":view==_produtosView?"Produtos":view==_historicoView?"Histórico de vendas":view==_cozinhaView?"Cozinha":view==_backupView?"Backup e restauração":"Configurações";
        var janela=new JanelaInterna(titulo,view,view==_atendimentoView?760:720,view==_atendimentoView?540:360);
        if(view==_atendimentoView)
        {
            void AjustarMinimoAtendimento()=>janela.DefinirMinimoConteudo(_atendimentoView.ViewModel.ComandaAtual is null?0:760,_atendimentoView.ViewModel.ComandaAtual is null?0:540);
            AjustarMinimoAtendimento();
            _atendimentoView.ViewModel.PropertyChanged+=(_,e)=>
            {
                if(e.PropertyName==nameof(AtendimentoViewModel.ComandaAtual))AjustarMinimoAtendimento();
            };
        }
        janela.FecharSolicitado += _ =>
        {
            Dispatcher.BeginInvoke(new Action(()=>
            {
                if(ConteudoPrincipal.JanelaAtiva is null)
                    CategoriasRibbon.Children.OfType<Button>().FirstOrDefault(b=>Equals(b.Content,_categoriaRibbon))?.Focus();
            }));
        };
        _janelasInternas.Add(view,janela);return janela;
    }
    private void IrParaProdutos()
    {
        if(_bloqueado||!_acesso.Autenticado||_restauracao)return;
        MostrarConteudo(_produtosView);
    }

    private void IrParaHistorico()
    {
        if (_bloqueado || _cozinhaAtiva || _restauracao) return;
        _historicoView.ViewModel.AtualizarVendas();
        MostrarConteudo(_historicoView);
    }

    private void IrParaBackup()
    {
        if (_bloqueado || !_acesso.Autenticado) return;
        if (!_restauracao) { AcaoConfiguracao("backups"); return; }
        _backupView.ViewModel.AtualizarLista();
        MostrarConteudo(_backupView);
    }

    private void IrParaConfiguracoes()
    {
        if (_bloqueado || !_acesso.Autenticado || _restauracao) return;
        AbrirSistemaConfiguracoes();
    }

    private void AlternarPerfil_Click(object sender, RoutedEventArgs e)
    {
        if (_bloqueado || _cozinhaAtiva) return;
        if (!PodeSairDasConfiguracoes()) return;
        if (_acesso.Autenticado)
        {
            _acesso.Sair();
            ConteudoPrincipal.Limpar();
        }
        else if (new JanelaAcessoGestor(_acesso) { Owner = this }.ShowDialog() == true && _restauracao)
            IrParaBackup();
        AtualizarPerfil();
    }

    private void Bloquear_Click(object sender, RoutedEventArgs e)
    {
        if (!PodeSairDasConfiguracoes()) return;
        if (_atendimentoView.ViewModel.ComandaAtual is not null)
            _atendimentoView.ViewModel.FecharEdicaoCommand.Execute(null);
        else BloquearAtendimento();
    }

    private void BloquearAtendimento()
    {
        _painelSenhas?.FecharChamador();
        _bloqueado = true;
        _acesso.Sair();
        _acessoAtendimento.Bloquear();
        _colaboradores.Sair();
        ConteudoPrincipal.Limpar();
        _cozinhaAtiva = false;
        AreaSistema.IsEnabled = false;
        AreaSistema.Visibility = Visibility.Visible;
        MenuInicial.Visibility = Visibility.Collapsed;
        MenuInicial.IsEnabled = false;
        CamadaBloqueio.Visibility = Visibility.Visible;
        _bloqueioView.Reiniciar();
        AtualizarPerfil();
        Dispatcher.BeginInvoke(new Action(() => { if (_bloqueado && CamadaBloqueio.Visibility == Visibility.Visible) _bloqueioView.Focus(); }));
    }

    private void DesbloquearAtendimento()
    {
        if (!_acessoAtendimento.Autenticado) return;
        // Mantém a barreira ativa se o carregamento dos dados falhar.
        try
        {
            if (!_restauracao)
            {
                _atendimentoView.ViewModel.AtualizarCategorias();
                _atendimentoView.ViewModel.AtualizarComandasAbertas();
            }
            _bloqueado = false;
            AreaSistema.IsEnabled = true;
            AreaSistema.Visibility = Visibility.Visible;
            CamadaBloqueio.Visibility = Visibility.Collapsed;
            MenuInicial.Visibility = Visibility.Collapsed;
            _cozinhaAtiva = _areaPendente == "cozinha";
            if (_cozinhaAtiva) _cozinhaView.Atualizar();
            if(!_restauracao)MostrarConteudo(_cozinhaAtiva?_cozinhaView:_atendimentoView);
            AtualizarPerfil();
        }
        catch
        {
            BloquearAtendimento();
            throw;
        }
    }

    private void MostrarInicio()
    {
        _areaPendente = "garcom";
        BloquearAtendimento();
        CamadaBloqueio.Visibility = Visibility.Collapsed;
        AreaSistema.IsEnabled = true;
        MenuInicial.Visibility = Visibility.Collapsed;
        SelecionarCategoriaRibbon("Principal");
    }

    private void MenuPrincipal_Click(object sender, RoutedEventArgs e)
    {
        if (!PodeSairDasConfiguracoes()) return;
        if (_atendimentoView.ViewModel.ComandaAtual is not null)
            _atendimentoView.ViewModel.FecharEdicaoCommand.Execute(null);
        MostrarInicio();
    }

    private void EscolherArea(string area)
    {
        if (!PodeSairDasConfiguracoes()) return;
        if (area is "garcom" or "cozinha")
        {
            if (_restauracao) { JanelaAviso.Mostrar("Recuperação", "Entre como gestor para restaurar o banco antes de iniciar a operação.", TipoAviso.Informacao); return; }
            _areaPendente = area;
            BloquearAtendimento();
            return;
        }
        if (_restauracao && area is not ("gestor" or "backup"))
        { JanelaAviso.Mostrar("Recuperação", "Somente a restauração de backup está disponível.", TipoAviso.Informacao); return; }
        if (new JanelaAcessoGestor(_acesso) { Owner = this }.ShowDialog() != true) return;
        _bloqueado = false;
        _cozinhaAtiva = false;
        MenuInicial.Visibility = Visibility.Collapsed;
        CamadaBloqueio.Visibility = Visibility.Collapsed;
        AreaSistema.Visibility = Visibility.Visible;
        AreaSistema.IsEnabled = true;
        AtualizarPerfil();
        try
        {
            switch (_restauracao ? "backup" : area)
            {
                case "backup": IrParaBackup(); break;
                case "historico": IrParaHistorico(); break;
                case "configuracoes": IrParaConfiguracoes(); break;
                case "licenca": IrParaConfiguracoes(); _configuracaoView.AbrirSecao(4); break;
                default: IrParaProdutos(); break;
            }
        }
        catch
        {
            MostrarInicio();
            JanelaAviso.Mostrar("Varthex Comanda", "Não foi possível abrir essa área. Tente novamente.", TipoAviso.Erro);
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (!e.Handled && !_bloqueado && ReferenceEquals(ConteudoPrincipal.JanelaAtiva?.Conteudo, _atendimentoView)
            && _atendimentoView.DigitarNumeroNaGrade(e.Key, e.KeyboardDevice.Modifiers, e.OriginalSource as DependencyObject))
        { e.Handled = true; return; }
        if (e.Key == Key.K && Keyboard.Modifiers == ModifierKeys.Control)
        { AbrirPesquisa(); e.Handled = true; return; }
        if (e.Handled || e.IsRepeat || Keyboard.Modifiers != ModifierKeys.None || !AreaSistema.IsEnabled || !_bloqueado) return;
        var area = e.Key switch { Key.F1 => "garcom", Key.F2 => "cozinha", Key.F3 => "gestor", _ => null };
        if (area is null) return;
        e.Handled = true;
        EscolherArea(area);
    }

    private void ConfigurarPin()
    {
        if (new JanelaAcessoGestor(_acesso) { Owner = this }.ShowDialog() == true)
            _bloqueioView.IniciarConfiguracao();
        _acesso.Sair();
        AtualizarPerfil();
    }

    private void AtualizarPerfil()
    {
        SugerirConexaoDriveAposLogin();
        if (_acesso.Autenticado && !_acesso.AutenticadoPorColaborador) _colaboradores.Sair();
        foreach (var botao in new[] { BotaoProdutos, BotaoBackup, BotaoConfiguracoes, BotaoLicenca })
        {
            botao.Visibility = _acesso.Autenticado ? Visibility.Visible : Visibility.Collapsed;
            if (botao == BotaoLicenca || (botao == BotaoBackup && !_restauracao)) botao.Visibility=Visibility.Collapsed;
            botao.IsEnabled = !_bloqueado && _acesso.Autenticado && (!_restauracao || botao == BotaoBackup);
        }
        BotaoAtendimento.IsEnabled = !_bloqueado && !_cozinhaAtiva && !_restauracao;
        BotaoHistorico.IsEnabled = !_bloqueado && !_cozinhaAtiva && !_restauracao;
        BotaoPerfil.IsEnabled = !_bloqueado && !_cozinhaAtiva;
        BotaoAtendimento.Visibility = _cozinhaAtiva ? Visibility.Collapsed : Visibility.Visible;
        BotaoHistorico.Visibility = _cozinhaAtiva ? Visibility.Collapsed : Visibility.Visible;
        BotaoPerfil.Visibility = _cozinhaAtiva ? Visibility.Collapsed : Visibility.Visible;
        BotaoCozinha.Visibility = _cozinhaAtiva ? Visibility.Visible : Visibility.Collapsed;
        BotaoCozinha.IsEnabled = !_bloqueado && _cozinhaAtiva && !_restauracao;
        RotuloPerfil.Text = _cozinhaAtiva ? "Cozinha · consulta" : _acesso.Autenticado ? "Gestor" : "Garçom";
        UsuarioRodape.Text = $"Usuário: {_colaboradores.SessaoAtual?.Nome ?? Environment.UserName} · {(_bloqueado ? "Acesso bloqueado" : RotuloPerfil.Text)}";
        LicencaRodape.Text = _configuracaoView.ConsultarLicenca?.Invoke() ?? "Licença: consultar ativação";
        LicencaRodape.ToolTip = LicencaRodape.Text;
        BotaoBackupRodape.IsEnabled = true;
        AtualizarAbaAtiva();
        BotaoPerfil.Content = _acesso.Autenticado ? "Voltar ao garçom" : "Entrar como gestor";
        CommandManager.InvalidateRequerySuggested();
        AtualizarRibbon();
    }

    private bool _gestorAnterior;
    private void SugerirConexaoDriveAposLogin()
    {
        var gestor = _acesso.Autenticado && !_bloqueado;
        var entrou = gestor && !_gestorAnterior;
        _gestorAnterior = gestor;
        if (!entrou || _restauracao || _backupView.ViewModel.GoogleDrive is not { } drive || drive.Conectada) return;
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
        {
            if (!_acesso.Autenticado || _bloqueado || drive.Conectada) return;
            new ConectarGoogleDriveView(drive) { Owner=this }.ShowDialog();
            _backupView.ViewModel.AtualizarGoogleDrive();
        }));
    }

    private void MostrarConteudo(UIElement view)
    {
        if (_bloqueado) return;
        var janela=ObterJanela(view);
        ConteudoPrincipal.Abrir(janela);
        AtualizarAbaAtiva();
        // Foco no primeiro controle util da tela; adiado ate a nova tela ser carregada.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            try
            {
                if(ReferenceEquals(ConteudoPrincipal.JanelaAtiva,janela))
                    janela.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
            catch (Exception)
            {
                // Foco inicial e apenas conveniencia: nunca deve derrubar a troca de tela.
            }
        }));
    }

    private bool PodeSairDasConfiguracoes() => !_configuracaoView.ViewModel.TemAlteracoes || _configuracaoView.ConfirmarSaida();

    private void AcaoConfiguracao(string acao)
    {
        if(_bloqueado || !_acesso.Autenticado || _restauracao) return;
        if(acao is "senha" or "pin")
        {
            AbrirAlteracaoAcesso(acao=="pin");
        }
        else if(acao=="licenca") { ((App)System.Windows.Application.Current).RenovarLicenca();_configuracaoView.AtualizarSecao(); }
        else if(acao is "backups" or "criar-backup")
        {
            _backupView.ViewModel.VoltarConfiguracao();
            var janela=new Window { Title="Backups e restauração",Owner=_configuracoesModal??(Window)this,Width=Math.Min(1100,SystemParameters.WorkArea.Width),Height=Math.Min(700,SystemParameters.WorkArea.Height),WindowStartupLocation=WindowStartupLocation.CenterOwner,Content=_backupView,ResizeMode=ResizeMode.CanResize };
            janela.MinWidth = 760; janela.MinHeight = 480;
            janela.Closing += (_, e) => { if (_backupView.ViewModel.EmAndamento) e.Cancel = true; };
            if (acao == "criar-backup")
            {
                var iniciou = false;
                janela.Loaded += async (_, _) =>
                {
                    if (iniciou) return;
                    iniciou = true;
                    await _backupView.CriarBackupComProgressoAsync(_configuracaoView.ViewModel.PastaBackupExterna);
                };
            }
            try { janela.ShowDialog(); } finally { janela.Content=null; }
            _configuracaoView.AtualizarSecao();
        }
    }
}
