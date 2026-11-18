using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop;

public sealed class JanelaRecuperacaoGestor : Window
{
    public JanelaRecuperacaoGestor(AcessoGestor acesso,bool recuperar,Window owner)
    {
        Preparar(this,recuperar?"Recuperar senha do gestor":"Senha de recuperação",owner);
        var painel=new StackPanel { Margin=new Thickness(24) };Content=new ScrollViewer { Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto };
        painel.Children.Add(Texto(recuperar?"Informe sua senha de recuperação e escolha uma nova senha de acesso do gestor.":"Confirme a senha atual do gestor e defina uma senha de recuperação diferente. Ela será usada apenas se você esquecer a senha de acesso."));
        var codigo=new PasswordBox { MaxLength=128,MinHeight=38 };
        var atual=new PasswordBox { MaxLength=128,MinHeight=38 };
        Campo(painel,recuperar?"Senha de recuperação":"Senha atual do gestor",recuperar?codigo:atual);
        var nova=new PasswordBox { MaxLength=128,MinHeight=38 };var confirmar=new PasswordBox { MaxLength=128,MinHeight=38 };
        Campo(painel,recuperar?"Nova senha (mínimo 4 caracteres)":"Nova senha de recuperação (mínimo 4 caracteres)",nova);Campo(painel,"Confirmar nova senha",confirmar);
        var erro=Texto("");erro.SetResourceReference(TextBlock.ForegroundProperty,"TextA12822");painel.Children.Add(erro);
        var aplicar=new Button { Content=recuperar?"Redefinir senha":"Salvar senha de recuperação",IsDefault=true,Margin=new Thickness(0,16,0,8) };
        aplicar.Click+=(_,_)=>
        {
            try
            {
                if(nova.Password!=confirmar.Password){erro.Text="As senhas não coincidem.";return;}
                if(recuperar)acesso.RecuperarSenha(codigo.Password,nova.Password);else acesso.DefinirSenhaRecuperacao(atual.Password,nova.Password);
                codigo.Clear();atual.Clear();nova.Clear();confirmar.Clear();
                DialogResult=true;
            }
            catch(Exception ex) when(ex is ArgumentException or InvalidOperationException){erro.Text=ex.Message;}
            catch{erro.Text="Não foi possível atualizar o acesso. Verifique a pasta de dados e tente novamente.";}
        };painel.Children.Add(aplicar);painel.Children.Add(new Button { Content="Cancelar",IsCancel=true });
    }
    private static void Campo(Panel painel,string titulo,Control campo)
    {
        if(titulo.Contains("senha de recuperação",StringComparison.OrdinalIgnoreCase))painel.Children.Add(AjudaSenhaRecuperacao.CriarRotulo(titulo,campo));
        else painel.Children.Add(new Label { Content=titulo,Target=campo,Padding=new Thickness(0,16,0,6) });
        System.Windows.Automation.AutomationProperties.SetName(campo,titulo);painel.Children.Add(campo);
    }
    private static TextBlock Texto(string texto)=>new(){Text=texto,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,4,0,4)};
    private static void Preparar(Window janela,string titulo,Window owner)
    {
        janela.Title=titulo;janela.Owner=owner;janela.Width=480;janela.SizeToContent=SizeToContent.Height;janela.ResizeMode=ResizeMode.NoResize;
        janela.WindowStartupLocation=WindowStartupLocation.CenterOwner;janela.ShowInTaskbar=false;
        janela.SetResourceReference(BackgroundProperty,"Surface");janela.SetResourceReference(ForegroundProperty,"Ink");janela.FontSize=13;janela.MaxHeight=SystemParameters.WorkArea.Height;Configuracao.DialogoCompacto.Aplicar(janela);
    }
}
