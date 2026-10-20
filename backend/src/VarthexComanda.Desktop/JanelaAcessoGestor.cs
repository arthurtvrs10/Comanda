using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VarthexComanda.Desktop;

public sealed class JanelaAcessoGestor : Window
{
    public JanelaAcessoGestor(AcessoGestor acesso)
    {
        var cadastro = !acesso.Configurado;
        Title = cadastro ? "Definir senha do gestor" : "Entrar como gestor";
        Width = 420; SizeToContent = SizeToContent.Height; ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var painel = new StackPanel { Margin = new Thickness(24) };
        Content = painel;
        painel.Children.Add(new TextBlock { Text = cadastro
            ? "O responsável pelo estabelecimento deve definir a senha do gestor. Use pelo menos 8 caracteres."
            : "Digite a senha para acessar produtos, backup, configurações e licença.", TextWrapping = TextWrapping.Wrap });
        painel.Children.Add(new TextBlock { Text = "Senha", Margin = new Thickness(0,16,0,4) });
        var senha = new PasswordBox { Padding = new Thickness(8), MaxLength = 128 };
        System.Windows.Automation.AutomationProperties.SetName(senha, "Senha do gestor");
        painel.Children.Add(senha);
        var confirmacao = new PasswordBox { Padding = new Thickness(8), MaxLength = 128 };
        System.Windows.Automation.AutomationProperties.SetName(confirmacao, "Confirmar senha");
        if (cadastro)
        {
            painel.Children.Add(new TextBlock { Text = "Confirmar senha", Margin = new Thickness(0,12,0,4) });
            painel.Children.Add(confirmacao);
        }
        var erro = new TextBlock { Foreground = TemaInterface.Pincel("#B22222"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,12,0,8) };
        painel.Children.Add(erro);
        var entrar = new Button { Content = cadastro ? "Definir senha e entrar" : "Entrar", IsDefault = true, Padding = new Thickness(12,8,12,8) };
        entrar.Click += (_, _) =>
        {
            try
            {
                if (cadastro)
                {
                    if (senha.Password != confirmacao.Password) { erro.Text = "As senhas não coincidem."; return; }
                    acesso.Cadastrar(senha.Password);
                }
                else if (!acesso.Entrar(senha.Password)) { erro.Text = "Senha incorreta."; senha.Clear(); senha.Focus(); return; }
                DialogResult = true;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { erro.Text = ex.Message; }
            catch (Exception) { erro.Text = "Não foi possível acessar a senha do gestor. Verifique a pasta de dados."; }
        };
        painel.Children.Add(entrar);
        painel.Children.Add(new Button { Content = "Cancelar", IsCancel = true, Margin = new Thickness(0,8,0,0), Padding = new Thickness(12,8,12,8) });
        Loaded += (_, _) => senha.Focus();
    }
}
