using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop;

public sealed class JanelaAcessoGestor : Window
{
    public JanelaAcessoGestor(AcessoGestor acesso, bool ativarProtecao = false)
    {
        var cadastro = ativarProtecao || !acesso.Configurado;
        Title = cadastro ? "Criar senha de acesso" : "Confirmar senha de acesso";
        Width = 420; SizeToContent = SizeToContent.Height; ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Configuracao.DialogoCompacto.Aplicar(this);MaxHeight=SystemParameters.WorkArea.Height;
        var painel = new StackPanel { Margin = new Thickness(24) };
        Content = new ScrollViewer { Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto };
        painel.Children.Add(new TextBlock { Text = cadastro
            ? "O responsável pelo estabelecimento deve definir a senha do gestor. Use pelo menos 4 caracteres."
            : "Confirme sua senha para continuar.", TextWrapping = TextWrapping.Wrap });
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
        var recuperacao=new PasswordBox { MaxLength=128,Padding=new Thickness(8) };
        var confirmarRecuperacao=new PasswordBox { MaxLength=128,Padding=new Thickness(8) };
        if(cadastro)
        {
            painel.Children.Add(AjudaSenhaRecuperacao.CriarRotulo("Senha de recuperação",recuperacao));painel.Children.Add(recuperacao);
            painel.Children.Add(new TextBlock { Text="Confirmar senha de recuperação",Margin=new Thickness(0,12,0,4) });painel.Children.Add(confirmarRecuperacao);
        }
        var erro = new TextBlock { Foreground = TemaInterface.Pincel("#B22222"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,12,0,8) };
        painel.Children.Add(erro);
        var entrar = new Button { Content = cadastro ? "Criar senha" : "Confirmar", IsDefault = true, Padding = new Thickness(12,8,12,8) };
        entrar.Click += (_, _) =>
        {
            try
            {
                if (cadastro)
                {
                    if (senha.Password != confirmacao.Password) { erro.Text = "As senhas não coincidem."; return; }
                    if(recuperacao.Password!=confirmarRecuperacao.Password){erro.Text="As senhas de recuperação não coincidem.";return;}
                    if(ativarProtecao)acesso.DefinirSenhasNaAtivacao(senha.Password,recuperacao.Password);
                    else acesso.CadastrarComSenhaRecuperacao(senha.Password,recuperacao.Password);
                }
                else if (!acesso.Entrar(senha.Password)) { erro.Text = "Senha incorreta."; senha.Clear(); senha.Focus(); return; }
                DialogResult = true;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { erro.Text = ex.Message; }
            catch (Exception) { erro.Text = "Não foi possível acessar a senha do gestor. Verifique a pasta de dados."; }
        };
        painel.Children.Add(entrar);
        if(!cadastro && !ativarProtecao)
        {
            var recuperar=new Button { Content="Esqueci minha senha",Margin=new Thickness(0,8,0,0) };
            recuperar.Click+=(_,_)=>
            {
                if(!acesso.RecuperacaoConfigurada){erro.Text="Não há senha de recuperação configurada para este gestor.";return;}
                if(new JanelaRecuperacaoGestor(acesso,true,this).ShowDialog()==true){senha.Clear();erro.Text="Senha redefinida. Entre com a nova senha.";}
            };painel.Children.Add(recuperar);
        }
        painel.Children.Add(new Button { Content = "Voltar", IsCancel = true, Margin = new Thickness(0,8,0,0), Padding = new Thickness(12,8,12,8) });
        Loaded += (_, _) => senha.Focus();
    }
}
