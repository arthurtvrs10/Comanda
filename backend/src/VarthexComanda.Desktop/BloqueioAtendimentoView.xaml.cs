using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VarthexComanda.Desktop;

public partial class BloqueioAtendimentoView : UserControl
{
    private readonly AcessoAtendimento _acesso;
    private string _pin = "";
    private string? _primeiroPin;
    private bool _configurando;
    public event Action? Desbloqueado;
    public event Action? ConfiguracaoSolicitada;

    public BloqueioAtendimentoView(AcessoAtendimento acesso)
    {
        InitializeComponent();
        _acesso = acesso;
        Reiniciar();
        Loaded += (_, _) => Focus();
    }
    public void Reiniciar()
    {
        _pin = ""; _primeiroPin = null; _configurando = false;
        Titulo.Text = "Atendimento bloqueado";
        Instrucao.Text = _acesso.Configurado ? "Digite o PIN para iniciar um atendimento." : "Peça ao gestor para configurar o PIN do estabelecimento.";
        BotaoEntrar.Content = "Desbloquear";
        BotaoEntrar.IsEnabled = _acesso.Configurado;
        BotaoConfigurar.Content = "Configurar PIN · somente gestor";
        Mensagem.Text = "";
        AtualizarVisor();
    }
    public void IniciarConfiguracao()
    {
        Reiniciar(); _configurando = true;
        Titulo.Text = "Definir PIN do atendimento";
        Instrucao.Text = "Escolha de 4 a 8 números para a equipe.";
        BotaoEntrar.Content = "Continuar";
        BotaoEntrar.IsEnabled = true;
        BotaoConfigurar.Content = "Cancelar configuração";
        Focus();
    }
    private void AtualizarVisor() => Visor.Text = _pin.Length == 0 ? "PIN" : new string('●', _pin.Length);
    private void Digito_Click(object sender, RoutedEventArgs e) => Adicionar(((Button)sender).Content.ToString()!);
    private void Adicionar(string numero) { if (_pin.Length < 8) _pin += numero; AtualizarVisor(); }
    private void Limpar_Click(object sender, RoutedEventArgs e) { _pin = ""; AtualizarVisor(); }
    private void Apagar_Click(object sender, RoutedEventArgs e) { if (_pin.Length > 0) _pin = _pin[..^1]; AtualizarVisor(); }
    private void Configurar_Click(object sender, RoutedEventArgs e)
    {
        if (_configurando) Reiniciar(); else ConfiguracaoSolicitada?.Invoke();
    }
    private void Entrar_Click(object sender, RoutedEventArgs e) => Confirmar();
    private void Confirmar()
    {
        try
        {
            if (_configurando)
            {
                if (!AcessoAtendimento.PinValido(_pin)) { Mensagem.Text = "Use de 4 a 8 números."; return; }
                if (_primeiroPin is null)
                {
                    _primeiroPin = _pin; _pin = ""; AtualizarVisor();
                    Instrucao.Text = "Digite o mesmo PIN novamente para confirmar.";
                    BotaoEntrar.Content = "Salvar PIN"; Mensagem.Text = "";
                }
                else if (_primeiroPin != _pin)
                { _pin = ""; AtualizarVisor(); Mensagem.Text = "Os PINs não coincidem. Repita o primeiro PIN."; }
                else { _acesso.DefinirPin(_pin); Reiniciar(); Mensagem.Text = "PIN salvo. Digite-o para desbloquear."; }
            }
            else if (_acesso.Configurado)
            {
                var sucesso = _acesso.Entrar(_pin, out var mensagem);
                _pin = ""; AtualizarVisor(); Mensagem.Text = mensagem;
                if (sucesso) Desbloqueado?.Invoke();
            }
        }
        catch (Exception)
        {
            _acesso.Bloquear(); _pin = ""; AtualizarVisor();
            Mensagem.Text = "Não foi possível liberar o atendimento. Peça ao gestor para verificar ou redefinir o PIN.";
        }
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (Keyboard.Modifiers == ModifierKeys.None && e.Key >= Key.D0 && e.Key <= Key.D9) { Adicionar(((int)e.Key - (int)Key.D0).ToString()); e.Handled = true; }
        else if (Keyboard.Modifiers == ModifierKeys.None && e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) { Adicionar(((int)e.Key - (int)Key.NumPad0).ToString()); e.Handled = true; }
        else if (e.Key == Key.Enter) { Confirmar(); e.Handled = true; }
        else if (e.Key == Key.Back) { Apagar_Click(this, e); e.Handled = true; }
        else if (e.Key is Key.Escape or Key.Delete) { Limpar_Click(this, e); e.Handled = true; }
    }
}
