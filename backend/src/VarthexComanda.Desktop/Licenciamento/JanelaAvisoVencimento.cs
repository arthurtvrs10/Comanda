using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VarthexComanda.Application.Licenciamento;
using VarthexComanda.Desktop.Ajuda;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Licenciamento;

public sealed class JanelaAvisoVencimento : Window
{
    public static bool DeveAvisar(DadosLicenca dados,DateTimeOffset agora)=>dados.Vencimento is { } fim&&fim>agora&&fim<=agora.AddDays(7);
    public static int DiasRestantes(DadosLicenca dados,DateTimeOffset agora)=>dados.Vencimento is { } fim?Math.Max(0,(int)Math.Ceiling((fim-agora).TotalDays)):0;
    public JanelaAvisoVencimento(DadosLicenca dados,Action<string>? abrir=null,TimeProvider? relogio=null)
    {
        abrir??=CanaisAjuda.Abrir;
        var dias=DiasRestantes(dados,(relogio??TimeProvider.System).GetUtcNow());
        Title="Renovação da assinatura";Width=540;MinWidth=450;SizeToContent=SizeToContent.Height;MaxHeight=650;ResizeMode=ResizeMode.NoResize;
        DialogoCompacto.Aplicar(this);
        var raiz=new StackPanel { Margin=new Thickness(26) };
        var rolagem=new ScrollViewer { Content=raiz,VerticalScrollBarVisibility=ScrollBarVisibility.Auto };rolagem.SetResourceReference(Control.BackgroundProperty,"Canvas");Content=rolagem;
        var cabecalho=new StackPanel { Orientation=Orientation.Horizontal };
        var urgencia=new Border { Background=new SolidColorBrush(Color.FromRgb(160,24,24)),Padding=new Thickness(18),Margin=new Thickness(0,0,0,18),Child=cabecalho };
        var icone=new System.Windows.Shapes.Path { Data=Geometry.Parse("M12,1 A11,11 0 1 1 12,23 A11,11 0 1 1 12,1 M12,5 L12,12 17,15"),Width=34,Height=34,Stretch=Stretch.Uniform,StrokeThickness=1.8,Margin=new Thickness(0,0,14,0) };
        icone.Stroke=Brushes.White;cabecalho.Children.Add(icone);
        cabecalho.Children.Add(new TextBlock { Text=dias==1?"Sua assinatura vence em 1 dia":$"Sua assinatura vence em {dias} dias",Foreground=Brushes.White,FontSize=22,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center,TextWrapping=TextWrapping.Wrap,MaxWidth=340 });raiz.Children.Add(urgencia);
        raiz.Children.Add(new TextBlock { Text=$"Validade: {dados.Vencimento?.ToOffset(TimeSpan.FromHours(-3)):dd/MM/yyyy HH:mm}",Margin=new Thickness(0,0,0,18) });
        raiz.Children.Add(new TextBlock { Text="Para continuar usando o Comanda, entre em contato com o suporte pelo WhatsApp e solicite a renovação.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,16),FontSize=14 });
        raiz.Children.Add(new TextBlock { Text="Após a renovação, você receberá uma nova chave para ativar no sistema. Você pode inserir a chave em Configurações → Geral → Consultar ativação.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,18) });
        var contato=new Button { Content="Falar sobre a renovação no WhatsApp",MinHeight=44,Padding=new Thickness(12,7,12,7),Style=TryFindResource("BotaoPrimario") as Style,Background=new SolidColorBrush(Color.FromRgb(19,122,63)),Foreground=Brushes.White,BorderBrush=new SolidColorBrush(Color.FromRgb(19,122,63)) };
        var conteudoContato=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center };
        conteudoContato.Children.Add(new System.Windows.Shapes.Path { Data=Geometry.Parse("M12,1 A10,10 0 1 1 4,18 L1,23 3,16 A10,10 0 0 1 12,1 M8,6 C6,6 6,9 8,12 C10,15 14,18 17,16 L18,14 14,12 13,14 C11,13 10,12 9,10 L10,9 8,6 Z"),Width=22,Height=22,Stretch=Stretch.Uniform,Stroke=Brushes.White,StrokeThickness=1.5,Margin=new Thickness(0,0,10,0),VerticalAlignment=VerticalAlignment.Center });
        conteudoContato.Children.Add(new TextBlock { Text="Renovar pelo WhatsApp",VerticalAlignment=VerticalAlignment.Center });contato.Content=conteudoContato;
        System.Windows.Automation.AutomationProperties.SetName(contato,"Renovar pelo WhatsApp");
        var estado=new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,0) };
        System.Windows.Automation.AutomationProperties.SetLiveSetting(estado,System.Windows.Automation.AutomationLiveSetting.Polite);
        contato.Click+=(_,_)=>
        {
            try { abrir(CanaisAjuda.Whatsapp($"Olá! Quero renovar minha assinatura do Comanda. Cliente: {dados.Cliente}. Licença: {dados.Id}. Vencimento: {dados.Vencimento?.ToOffset(TimeSpan.FromHours(-3)):dd/MM/yyyy}."));estado.Text="Revise a mensagem no WhatsApp e envie para o suporte."; }
            catch { estado.Text="Não foi possível abrir o WhatsApp. Acesse Ajuda → Suporte para entrar em contato."; }
        };
        raiz.Children.Add(contato);
        raiz.Children.Add(estado);
        var depois=new Button { Content="Continuar no sistema",IsCancel=true,MinHeight=36,Margin=new Thickness(0,16,0,0) };depois.Click+=(_,_)=>Close();raiz.Children.Add(depois);
    }
}

