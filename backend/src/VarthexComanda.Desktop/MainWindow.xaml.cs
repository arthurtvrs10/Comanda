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
        BloquearAtendimento();
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
        AtualizarPerfil();
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
        if (_bloqueado || _restauracao) return;
        _atendimentoView.ViewModel.AtualizarCategorias();
        _atendimentoView.ViewModel.AtualizarComandasAbertas();
        MostrarConteudo(_atendimentoView);
    }

    private void IrParaProdutos() { if (!_bloqueado && _acesso.Autenticado && !_restauracao) MostrarConteudo(_produtosView); }

    private void IrParaHistorico()
    {
        if (_bloqueado || _restauracao) return;
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
        if (_bloqueado) return;
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
        AreaSistema.IsEnabled = false;
        CamadaBloqueio.Visibility = Visibility.Visible;
        _bloqueioView.Reiniciar();
        AtualizarPerfil();
        Dispatcher.BeginInvoke(new Action(() => { if (_bloqueado) _bloqueioView.Focus(); }));
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
            CamadaBloqueio.Visibility = Visibility.Collapsed;
            ConteudoPrincipal.Content = _restauracao ? null : _atendimentoView;
            AtualizarPerfil();
        }
        catch
        {
            BloquearAtendimento();
            throw;
        }
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
        BotaoAtendimento.IsEnabled = !_bloqueado && !_restauracao;
        BotaoHistorico.IsEnabled = !_bloqueado && !_restauracao;
        BotaoPerfil.IsEnabled = !_bloqueado;
        RotuloPerfil.Text = _acesso.Autenticado ? "Menu gestor" : "Menu garçom";
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
