using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaAlterarAcesso : Window
{
    public JanelaAlterarAcesso(AcessoGestor gestor,AcessoAtendimento atendimento,bool pin)
    {
        Title=pin?"Alterar PIN do atendimento":"Alterar senha do gestor";
        Width=460;SizeToContent=SizeToContent.Height;MaxHeight=SystemParameters.WorkArea.Height;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;ResizeMode=ResizeMode.NoResize;
        var painel=new StackPanel { Margin=new Thickness(24) };
        Content=new ScrollViewer { Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto };
        painel.Children.Add(new TextBlock { Text=Title,FontSize=22,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,20) });
        PasswordBox Campo(string titulo){var campo=new PasswordBox { MinHeight=46,MaxLength=128,Margin=new Thickness(0,0,0,16) };painel.Children.Add(new Label { Content=titulo,Target=campo });painel.Children.Add(campo);return campo;}
        var atual=Campo("Senha atual do gestor");var nova=Campo(pin?"Novo PIN (4 a 8 números)":"Nova senha (mínimo 8 caracteres)");var confirmar=Campo("Confirmar novo valor");
        var erro=new TextBlock { TextWrapping=TextWrapping.Wrap,Foreground=Brushes.Firebrick };painel.Children.Add(erro);
        var acoes=new WrapPanel { HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,20,0,0) };
        var cancelar=new Button { Content="Cancelar",IsCancel=true,Margin=new Thickness(0,0,8,0) };acoes.Children.Add(cancelar);
        var salvar=new Button { Content="Salvar alteração",IsDefault=true,Background=new SolidColorBrush(Color.FromRgb(36,86,64)),Foreground=Brushes.White };acoes.Children.Add(salvar);painel.Children.Add(acoes);
        salvar.Click+=(_,_)=>{
            try {
                if(!gestor.Autenticado)throw new InvalidOperationException("Entre como gestor para alterar o acesso.");
                if(nova.Password!=confirmar.Password)throw new InvalidOperationException("A confirmação não corresponde ao novo valor.");
                if(pin){if(!gestor.ConferirSenha(atual.Password))throw new InvalidOperationException("Senha atual incorreta.");atendimento.DefinirPin(nova.Password);}
                else gestor.AlterarSenha(atual.Password,nova.Password);
                DialogResult=true;
            }catch(Exception ex){erro.Text=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível salvar a credencial. Tente novamente.";}
        };
        Closed+=(_,_)=>{atual.Clear();nova.Clear();confirmar.Clear();};Loaded+=(_,_)=>atual.Focus();
    }
}
