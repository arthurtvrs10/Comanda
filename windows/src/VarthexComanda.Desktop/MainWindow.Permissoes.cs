using VarthexComanda.Desktop.Configuracao;
using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private bool PermiteColaborador(string id)
    {
        try { return !_colaboradores.SessaoIndividualIniciada || _colaboradores.Pode(id); }
        catch { return false; }
    }

    private bool AutorizarOperacao(string id)
    {
        if (_bloqueado || _restauracao || !PermiteColaborador("atendimento")) return false;
        if (PermiteColaborador(id)) return true;
        if (_colaboradores.ConferirSessao() is null) return false;
        return new JanelaAutorizacaoColaborador(_colaboradores, id) { Owner = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w=>w.IsActive) ?? this }.ShowDialog() == true;
    }

    private string? PermissaoDaTela(UIElement view) => view == _atendimentoView || view == _balcaoView ? "atendimento" :
        view == _produtosView || view == _complementosView ? "produtos" : view == _historicoView ? "historico-vendas" :
        view == _historicoPedidosView ? "historico-pedidos" : view == _backupView ? "backup" :
        view == _configuracaoView ? "configuracoes" : view == _registrosView ? "logs" : view == _cozinhaView ? "cozinha" : null;

    private void ProtegerTela(FrameworkElement view, string id)
    {
        // Revalida inclusive janelas já abertas e permissões alteradas em outra instância.
        view.PreviewMouseDown += (_, e) => { if (_bloqueado || !PermiteColaborador(id)) e.Handled = true; };
        view.PreviewKeyDown += (_, e) => { if (!PermiteColaborador(id)) e.Handled = true; };
    }
}

public sealed class JanelaAutorizacaoColaborador : Window
{
    public JanelaAutorizacaoColaborador(ServicoColaboradores servico, string id)
    {
        Title = "Autorizar operação"; Width = 460; SizeToContent = SizeToContent.Height; ResizeMode = ResizeMode.NoResize;
        DialogoCompacto.Aplicar(this);
        var painel = new StackPanel { Margin = new Thickness(20) }; Content = painel;
        painel.Children.Add(new TextBlock { Text = PermissoesColaborador.Todas.Single(p=>p.Id==id).Nome, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,12) });
        painel.Children.Add(new TextBlock { Text = "Peça a uma pessoa habilitada para informar seu login e sua senha mestra. A autorização vale somente para esta operação.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,12) });
        var login = new TextBox { MaxLength = 60 }; painel.Children.Add(new Label { Content = "Login de quem autoriza", Target = login }); painel.Children.Add(login);
        var senha = new PasswordBox { MaxLength = 128 }; painel.Children.Add(new Label { Content = "Senha", Target = senha }); painel.Children.Add(senha);
        var erro = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,8) }; painel.Children.Add(erro);
        var botoes = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var cancelar = new Button { Content = "Cancelar", IsCancel = true, MinWidth = 96, Margin = new Thickness(0,0,8,0) }; botoes.Children.Add(cancelar);
        var autorizar = new Button { Content = "Autorizar", IsDefault = true, MinWidth = 96 }; botoes.Children.Add(autorizar); painel.Children.Add(botoes);
        autorizar.Click += (_,_) =>
        {
            try
            {
                if (servico.AutorizarComSenhaMestra(login.Text, senha.Password, id)) DialogResult = true;
                else erro.Text = "Credenciais inválidas, pessoa sem autorização ou limite de tentativas atingido. Confira os dados; após várias tentativas, aguarde 30 segundos.";
            }
            catch { erro.Text = "Não foi possível registrar a autorização. Tente novamente."; }
            finally { senha.Clear(); }
        };
        Closed += (_,_) => senha.Clear(); Loaded += (_,_) => login.Focus();
    }
}
