using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop;

internal static class AjudaSenhaRecuperacao
{
    internal static FrameworkElement CriarRotulo(string texto,Control campo)
    {
        const string explicacao="Use esta senha em ‘Esqueci minha senha’ para criar uma nova senha de acesso, sem internet. Ela deve ser diferente da senha de acesso. Guarde-a em um local seguro e não compartilhe com a equipe.";
        var linha=new DockPanel { LastChildFill=true,Margin=new Thickness(0,12,0,4) };
        var ajuda=new TextBlock { Text="❓",FontSize=14,Focusable=true,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(6,0,0,0),Cursor=System.Windows.Input.Cursors.Help };
        ajuda.ToolTip=new ToolTip { Content=new TextBlock { Text=explicacao,TextWrapping=TextWrapping.Wrap,MaxWidth=300 } };
        ToolTipService.SetInitialShowDelay(ajuda,200);ToolTipService.SetShowDuration(ajuda,20000);
        ajuda.GotKeyboardFocus+=(_,_)=>((ToolTip)ajuda.ToolTip).IsOpen=true;
        ajuda.LostKeyboardFocus+=(_,_)=>((ToolTip)ajuda.ToolTip).IsOpen=false;
        System.Windows.Automation.AutomationProperties.SetName(ajuda,"Para que serve a senha de recuperação");
        System.Windows.Automation.AutomationProperties.SetHelpText(ajuda,explicacao);
        var rotulo=new Label { Content=texto,Target=campo,Padding=new Thickness(0),VerticalContentAlignment=VerticalAlignment.Center };
        var conteudo=new StackPanel { Orientation=Orientation.Horizontal };conteudo.Children.Add(rotulo);conteudo.Children.Add(ajuda);linha.Children.Add(conteudo);
        return linha;
    }
}
