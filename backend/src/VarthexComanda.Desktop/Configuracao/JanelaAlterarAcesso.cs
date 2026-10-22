using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaAlterarAcesso : Window
{
    public JanelaAlterarAcesso(AcessoGestor gestor, AcessoAtendimento atendimento, bool pin,
        Action<string,string>? alterarSenha = null, string? usuario = null)
    {
        var primeiroCadastro=!pin&&alterarSenha is null&&!gestor.Configurado;
        Title = primeiroCadastro?"Definir senha do gestor":pin ? "Alterar PIN da equipe" : "Alterar senha";
        Width = 350;
        SizeToContent = SizeToContent.Height;
        MaxHeight = SystemParameters.WorkArea.Height;
        ResizeMode = ResizeMode.NoResize;
        DialogoCompacto.Aplicar(this);
        var painel = new StackPanel { Margin = new Thickness(20,16,20,12) };
        var conteudo = new ScrollViewer { Content=painel, VerticalScrollBarVisibility=ScrollBarVisibility.Auto };
        conteudo.SetResourceReference(Control.BackgroundProperty,"Canvas");
        Content = conteudo;
        if (!string.IsNullOrWhiteSpace(usuario))
            painel.Children.Add(new TextBlock { Text=usuario, FontWeight=FontWeights.SemiBold, Margin=new Thickness(0,0,0,12) });
        PasswordBox Campo(string titulo, int maximo)
        {
            var campo = new PasswordBox { MaxLength=maximo, Margin=new Thickness(0,0,0,9) };
            if(titulo.StartsWith("Senha de recuperação"))painel.Children.Add(AjudaSenhaRecuperacao.CriarRotulo("Senha de recuperação",campo));
            else painel.Children.Add(new Label { Content=titulo, Target=campo, Padding=new Thickness(0,0,0,3) });
            System.Windows.Automation.AutomationProperties.SetName(campo,titulo);
            painel.Children.Add(campo);
            return campo;
        }
        var atual = primeiroCadastro?new PasswordBox():Campo(pin ? "Senha atual do gestor" : "Senha atual",128);
        var nova = Campo(pin ? "Novo PIN" : "Nova senha",pin?8:128);
        painel.Children.Add(new TextBlock { Text=pin?"De 4 a 8 números.":"Use pelo menos 4 caracteres.", Margin=new Thickness(0,0,0,9) });
        var confirmar = Campo(pin ? "Confirmar novo PIN" : "Confirmar nova senha",128);
        var recuperacao=primeiroCadastro?Campo("Senha de recuperação (diferente da senha de acesso)",128):new PasswordBox();
        var confirmarRecuperacao=primeiroCadastro?Campo("Confirmar senha de recuperação",128):new PasswordBox();
        var erro = new TextBlock { TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,0,0,6) };
        erro.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");
        painel.Children.Add(erro);
        var salvar = new Button { Content="Salvar",IsDefault=true,HorizontalAlignment=HorizontalAlignment.Right,MinWidth=92,Margin=new Thickness(0,8,0,0) };
        painel.Children.Add(salvar);
        salvar.Click += (_,_) =>
        {
            try
            {
                if (nova.Password != confirmar.Password) throw new InvalidOperationException("A confirmação não corresponde à nova senha.");
                if (alterarSenha is not null && !pin) alterarSenha(atual.Password,nova.Password);
                else
                {
                    if (!gestor.Autenticado) throw new InvalidOperationException("Entre como gestor para alterar o acesso.");
                    if (pin)
                    {
                        if (!gestor.ConferirSenha(atual.Password)) throw new InvalidOperationException("Senha atual incorreta.");
                        atendimento.DefinirPin(nova.Password);
                    }
                    else if(primeiroCadastro)
                    {
                        if(recuperacao.Password!=confirmarRecuperacao.Password)throw new InvalidOperationException("As senhas de recuperação não coincidem.");
                        gestor.CadastrarComSenhaRecuperacao(nova.Password,recuperacao.Password);
                    }
                    else gestor.AlterarSenha(atual.Password,nova.Password);
                }
                DialogResult=true;
            }
            catch (Exception ex) { erro.Text=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível salvar a credencial. Tente novamente."; }
        };
        PreviewKeyDown += (_,e) => { if(e.Key==System.Windows.Input.Key.Escape){Close();e.Handled=true;} };
        Closed += (_,_) => { atual.Clear();nova.Clear();confirmar.Clear();recuperacao.Clear();confirmarRecuperacao.Clear(); };
        Loaded += (_,_) => {if(primeiroCadastro)nova.Focus();else atual.Focus();};
    }
}
