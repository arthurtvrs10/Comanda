using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaAlterarAcesso : Window
{
    public JanelaAlterarAcesso(AcessoGestor gestor, AcessoAtendimento atendimento, bool pin,
        Action<string,string>? alterarSenha = null, string? usuario = null)
    {
        Title = pin ? "Alterar PIN da equipe" : "Alterar senha";
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
            painel.Children.Add(new Label { Content=titulo, Target=campo, Padding=new Thickness(0,0,0,3) });
            System.Windows.Automation.AutomationProperties.SetName(campo,titulo);
            painel.Children.Add(campo);
            return campo;
        }
        var atual = Campo(pin ? "Senha atual do gestor" : "Senha atual",128);
        var nova = Campo(pin ? "Novo PIN" : "Nova senha",pin?8:128);
        painel.Children.Add(new TextBlock { Text=pin?"De 4 a 8 números.":"Use pelo menos 8 caracteres.", Margin=new Thickness(0,0,0,9) });
        var confirmar = Campo(pin ? "Confirmar novo PIN" : "Confirmar nova senha",128);
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
                    else gestor.AlterarSenha(atual.Password,nova.Password);
                }
                DialogResult=true;
            }
            catch (Exception ex) { erro.Text=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível salvar a credencial. Tente novamente."; }
        };
        PreviewKeyDown += (_,e) => { if(e.Key==System.Windows.Input.Key.Escape){Close();e.Handled=true;} };
        Closed += (_,_) => { atual.Clear();nova.Clear();confirmar.Clear(); };
        Loaded += (_,_) => atual.Focus();
    }
}
