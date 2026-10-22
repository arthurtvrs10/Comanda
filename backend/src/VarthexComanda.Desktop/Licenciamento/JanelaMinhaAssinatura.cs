using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VarthexComanda.Application.Licenciamento;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Licenciamento;

public sealed class JanelaMinhaAssinatura : Window
{
    private readonly TextBox _dados;
    public JanelaMinhaAssinatura(Func<DadosLicenca> consultar,TimeProvider? relogio=null)
    {
        Title="Minha assinatura";Width=510;Height=410;MinWidth=460;MinHeight=380;ResizeMode=ResizeMode.CanResize;
        DialogoCompacto.Aplicar(this);
        var raiz=new Grid { Margin=new Thickness(12) };raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");Content=raiz;
        raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});raiz.RowDefinitions.Add(new());raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});
        var cabecalho=new StackPanel { Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,14) };
        var chave=new System.Windows.Shapes.Path { Data=Geometry.Parse("M14,8 A6,6 0 1 1 2,8 A6,6 0 1 1 14,8 M12,12 L23,23 M16,16 L19,13 M20,20 L23,17"),Width=38,Height=38,StrokeThickness=2,Stretch=Stretch.Uniform,Margin=new Thickness(0,0,16,0),VerticalAlignment=VerticalAlignment.Center };chave.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty,"Accent");cabecalho.Children.Add(chave);
        cabecalho.Children.Add(new TextBlock { Text="Minha assinatura",FontSize=22,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center });raiz.Children.Add(cabecalho);
        _dados=new TextBox { IsReadOnly=true,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,VerticalContentAlignment=VerticalAlignment.Top,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Padding=new Thickness(14),FontSize=12 };
        Grid.SetRow(_dados,1);raiz.Children.Add(_dados);
        var aviso=new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,6,0,0) };Grid.SetRow(aviso,2);raiz.Children.Add(aviso);
        var rodape=new DockPanel { LastChildFill=false,Margin=new Thickness(0,10,0,0) };
        var copiar=new Button { Content="Copiar informações",Padding=new Thickness(8,7,8,7) };copiar.Click+=(_,_)=>{try{Clipboard.SetText(_dados.Text);aviso.Text="Informações copiadas.";}catch{aviso.Text="Selecione as informações e copie com Ctrl+C.";}};DockPanel.SetDock(copiar,Dock.Left);rodape.Children.Add(copiar);
        var trocar=new Button { Content="Trocar assinatura",MinWidth=180,Padding=new Thickness(16,7,16,7) };trocar.Click+=(_,_)=>DialogResult=true;DockPanel.SetDock(trocar,Dock.Right);rodape.Children.Add(trocar);Grid.SetRow(rodape,3);raiz.Children.Add(rodape);
        try { _dados.Text=MontarInformacoes(consultar(),(relogio??TimeProvider.System).GetUtcNow()); }
        catch(Exception ex) { _dados.Text="Comanda\n\n"+(ex is InvalidOperationException?ex.Message:"Não foi possível consultar a assinatura. Abra a ativação para verificar a licença."); }
        PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape){Close();e.Handled=true;}};
    }
    public static string MontarInformacoes(DadosLicenca dados,DateTimeOffset agora)
    {
        var validade=dados.Vencimento is null?"Sem vencimento (vitalícia)":$"{dados.Vencimento.Value.ToOffset(TimeSpan.FromHours(-3)):dd/MM/yyyy} ({Math.Max(0,(int)Math.Ceiling((dados.Vencimento.Value-agora).TotalDays))} dia(s) restante(s))";
        return $"Comanda · Versão {VersaoDoAplicativo.Atual}\n"+
            (dados.Vencimento is null?"Licença vitalícia":"Licença por prazo")+
            $"\n\nIdentificação da licença: {dados.Id}\nNome: {dados.Cliente}\nValidade: {validade}\n\nRecursos do sistema:\nMesas/comandas; produtos e categorias; cozinha; painel de senhas; colaboradores; impressão não fiscal; backup e restauração.";
    }
}
