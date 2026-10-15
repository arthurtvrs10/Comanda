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
    private bool _restauracao;
    private bool _bloqueado = true;
    private bool _cozinhaAtiva;
    private string _areaPendente = "garcom";
    private readonly InicioView _inicioView;
    private readonly CozinhaView _cozinhaView;
    private readonly AcessoAtendimento _acessoAtendimento;
    private readonly BloqueioAtendimentoView _bloqueioView;
    private readonly AtendimentoView _atendimentoView;
    private readonly ProdutosView _produtosView;
    private readonly HistoricoView _historicoView;
    private readonly BackupView _backupView;
    private readonly ConfiguracaoView _configuracaoView;

    public MainWindow(AtendimentoView atendimentoView, ProdutosView produtosView, HistoricoView historicoView, BackupView backupView, ConfiguracaoView configuracaoView, VarthexComanda.Infrastructure.Storage.AppPaths? paths = null)
    {
        InitializeComponent();
        _acesso = new AcessoGestor(System.IO.Path.Combine((paths ?? new VarthexComanda.Infrastructure.Storage.AppPaths()).Root, "gestor.credencial"));
        _acessoAtendimento = new AcessoAtendimento(System.IO.Path.Combine((paths ?? new VarthexComanda.Infrastructure.Storage.AppPaths()).Root, "atendimento.credencial"));
        _bloqueioView = new BloqueioAtendimentoView(_acessoAtendimento);
        CamadaBloqueio.Content = _bloqueioView;
        _bloqueioView.Desbloqueado += DesbloquearAtendimento;
        _bloqueioView.ConfiguracaoSolicitada += ConfigurarPin;
        _bloqueioView.VoltarSolicitado += MostrarInicio;
        _inicioView = new InicioView();
        MenuInicial.Content = _inicioView;
        _inicioView.AreaSolicitada += EscolherArea;
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
        ConteudoPrincipal.Content = null;
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

    private void IrParaProdutos() { if (!_bloqueado && _acesso.Autenticado && !_restauracao) MostrarConteudo(_produtosView); }

    private void IrParaHistorico()
    {
        if (_bloqueado || _cozinhaAtiva || _restauracao) return;
        _historicoView.ViewModel.AtualizarVendas();
        MostrarConteudo(_historicoView);
    }

    private void IrParaBackup()
    {
        if (_bloqueado || !_acesso.Autenticado) return;
        _backupView.ViewModel.AtualizarLista();
        MostrarConteudo(_backupView);
    }

    private void IrParaConfiguracoes()
    {
        if (_bloqueado || !_acesso.Autenticado || _restauracao) return;
        _configuracaoView.ViewModel.Carregar();
        MostrarConteudo(_configuracaoView);
    }

    private void AlternarPerfil_Click(object sender, RoutedEventArgs e)
    {
        if (_bloqueado || _cozinhaAtiva) return;
        if (_acesso.Autenticado)
        {
            _acesso.Sair();
            ConteudoPrincipal.Content = _restauracao ? null : _atendimentoView;
        }
        else if (new JanelaAcessoGestor(_acesso) { Owner = this }.ShowDialog() == true && _restauracao)
            IrParaBackup();
        AtualizarPerfil();
    }

    private void Bloquear_Click(object sender, RoutedEventArgs e)
    {
        if (_atendimentoView.ViewModel.ComandaAtual is not null)
            _atendimentoView.ViewModel.FecharEdicaoCommand.Execute(null);
        else BloquearAtendimento();
    }

    private void BloquearAtendimento()
    {
        _bloqueado = true;
        _acesso.Sair();
        _acessoAtendimento.Bloquear();
        ConteudoPrincipal.Content = null;
        _cozinhaAtiva = false;
        AreaSistema.IsEnabled = false;
        AreaSistema.Visibility = Visibility.Collapsed;
        MenuInicial.Visibility = Visibility.Visible;
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
            ConteudoPrincipal.Content = _restauracao ? null : _cozinhaAtiva ? _cozinhaView : _atendimentoView;
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
        BloquearAtendimento();
        CamadaBloqueio.Visibility = Visibility.Collapsed;
        MenuInicial.IsEnabled = true;
        _inicioView.MostrarAviso(_restauracao ? "Banco indisponível. Entre como gestor para restaurar um backup." : "");
    }

    private void MenuPrincipal_Click(object sender, RoutedEventArgs e)
    {
        if (_atendimentoView.ViewModel.ComandaAtual is not null)
            _atendimentoView.ViewModel.FecharEdicaoCommand.Execute(null);
        MostrarInicio();
    }

    private void EscolherArea(string area)
    {
        if (MenuInicial.Visibility != Visibility.Visible || !MenuInicial.IsEnabled) return;
        _inicioView.MostrarAviso("");
        if (area is "garcom" or "cozinha")
        {
            if (_restauracao) { _inicioView.MostrarAviso("Entre como gestor para restaurar o banco antes de iniciar a operação."); return; }
            _areaPendente = area;
            BloquearAtendimento();
            return;
        }
        if (_restauracao && area is not ("gestor" or "backup"))
        { _inicioView.MostrarAviso("Somente a restauração de backup está disponível."); return; }
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
                case "licenca": IrParaProdutos(); ((App)System.Windows.Application.Current).RenovarLicenca(); break;
                default: IrParaProdutos(); break;
            }
        }
        catch
        {
            MostrarInicio();
            _inicioView.MostrarAviso("Não foi possível abrir essa área. Tente novamente.");
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Handled || e.IsRepeat || Keyboard.Modifiers != ModifierKeys.None || MenuInicial.Visibility != Visibility.Visible || !MenuInicial.IsEnabled) return;
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
        foreach (var botao in new[] { BotaoProdutos, BotaoBackup, BotaoConfiguracoes, BotaoLicenca })
        {
            botao.Visibility = _acesso.Autenticado ? Visibility.Visible : Visibility.Collapsed;
            botao.IsEnabled = !_bloqueado && _acesso.Autenticado && (!_restauracao || botao == BotaoBackup);
        }
        BotaoAtendimento.IsEnabled = !_bloqueado && !_cozinhaAtiva && !_restauracao;
        BotaoHistorico.IsEnabled = !_bloqueado && !_cozinhaAtiva && !_restauracao;
        BotaoPerfil.IsEnabled = !_bloqueado && !_cozinhaAtiva;
        BotaoAtendimento.Visibility = _cozinhaAtiva ? Visibility.Collapsed : Visibility.Visible;
        BotaoHistorico.Visibility = _cozinhaAtiva ? Visibility.Collapsed : Visibility.Visible;
        BotaoPerfil.Visibility = _cozinhaAtiva ? Visibility.Collapsed : Visibility.Visible;
        RotuloPerfil.Text = _cozinhaAtiva ? "Cozinha · consulta" : _acesso.Autenticado ? "Menu gestor" : "Menu garçom";
        BotaoPerfil.Content = _acesso.Autenticado ? "Voltar ao garçom" : "Entrar como gestor";
        CommandManager.InvalidateRequerySuggested();
    }

    private void MostrarConteudo(UIElement view)
    {
        if (_bloqueado) return;
        ConteudoPrincipal.Content = view;
        // Foco no primeiro controle util da tela; adiado ate a nova tela ser carregada.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            try
            {
                ConteudoPrincipal.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
            catch (Exception)
            {
                // Foco inicial e apenas conveniencia: nunca deve derrubar a troca de tela.
            }
        }));
    }
}
