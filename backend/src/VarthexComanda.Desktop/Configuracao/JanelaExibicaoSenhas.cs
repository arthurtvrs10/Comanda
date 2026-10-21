using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaExibicaoSenhas : Window
{
    private readonly EstadoPainelSenhas _estado;
    private readonly Grid _raiz;
    private readonly TextBlock _numero;
    private readonly TextBlock _legenda;
    private readonly StackPanel _historico;
    private readonly Border _coluna;
    private PreferenciasPainelSenhas _preferencias;
    private MonitorPainel? _monitor;
    public JanelaExibicaoSenhas(EstadoPainelSenhas estado,PreferenciasPainelSenhas preferencias)
    {
        _estado=estado;_preferencias=preferencias;
        Title="Varthex · Painel de senhas";Width=1100;Height=700;MinWidth=500;MinHeight=300;ShowActivated=false;
        _raiz=new Grid { Margin=new Thickness(0) };Content=_raiz;
        _raiz.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        _raiz.ColumnDefinitions.Add(new(){Width=new GridLength(0.28,GridUnitType.Star)});
        var principal=new Grid { Margin=new Thickness(30) };
        principal.RowDefinitions.Add(new(){Height=GridLength.Auto});principal.RowDefinitions.Add(new());principal.RowDefinitions.Add(new(){Height=GridLength.Auto});_raiz.Children.Add(principal);
        _legenda=new TextBlock { FontSize=36,FontWeight=FontWeights.SemiBold,TextAlignment=TextAlignment.Center,Margin=new Thickness(0,12,0,12) };principal.Children.Add(_legenda);
        _numero=new TextBlock { FontSize=240,FontWeight=FontWeights.Bold,FontFamily=new FontFamily("Segoe UI"),Text="—" };
        var destaque=new Viewbox { Child=_numero,Margin=new Thickness(0,10,0,10),Stretch=Stretch.Uniform };Grid.SetRow(destaque,1);principal.Children.Add(destaque);
        var marca=new TextBlock { Text="VARTHEX COMANDA",FontSize=18,TextAlignment=TextAlignment.Center,Margin=new Thickness(0,12,0,6) };Grid.SetRow(marca,2);principal.Children.Add(marca);
        var anteriores=new DockPanel { Margin=new Thickness(20,28,20,28) };
        var titulo=new TextBlock { Text="ANTERIORES",FontSize=20,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,24),TextAlignment=TextAlignment.Center };DockPanel.SetDock(titulo,Dock.Top);anteriores.Children.Add(titulo);
        _historico=new StackPanel();anteriores.Children.Add(new Viewbox { Child=_historico,Stretch=Stretch.Uniform,StretchDirection=StretchDirection.DownOnly,VerticalAlignment=VerticalAlignment.Top });
        _coluna=new Border { Child=anteriores,BorderThickness=new Thickness(1,0,0,0),BorderBrush=new SolidColorBrush(Color.FromArgb(80,0,0,0)),Background=new SolidColorBrush(Color.FromArgb(28,255,255,255)) };Grid.SetColumn(_coluna,1);_raiz.Children.Add(_coluna);
        _estado.Alterado+=Atualizar;
        Closed+=(_,_)=>_estado.Alterado-=Atualizar;
        PreviewKeyDown+=(_,e)=>{if(e.Key is Key.Escape or Key.F11){if(_monitor is not null)ConfigurarMonitor(_monitor,e.Key==Key.F11&&WindowStyle!=WindowStyle.None);e.Handled=true;}};
        Aplicar(preferencias);
    }
    public void Aplicar(PreferenciasPainelSenhas preferencias)
    {
        PainelSenhasService.Validar(preferencias);_preferencias=preferencias;
        _raiz.Background=Fundo(preferencias);
        _raiz.SetValue(TextElement.ForegroundProperty,(Brush)new BrushConverter().ConvertFromString(preferencias.CorNumeros)!);
        Atualizar();
    }
    public static Brush Fundo(PreferenciasPainelSenhas preferencias)=>new LinearGradientBrush((Color)ColorConverter.ConvertFromString(preferencias.CorInicial),(Color)ColorConverter.ConvertFromString(preferencias.CorFinal),90);
    public void ConfigurarMonitor(MonitorPainel monitor,bool telaCheia)
    {
        _monitor=monitor;MonitoresPainel.Posicionar(this,monitor,telaCheia);
    }
    private void Atualizar()
    {
        _numero.Text=_estado.Atual??"—";
        _legenda.Text=_estado.Atual is null?"AGUARDE A CHAMADA":"SENHA";
        _historico.Children.Clear();
        foreach(var senha in _estado.Anteriores)
            _historico.Children.Add(new Viewbox { Child=new TextBlock { Text=senha,FontSize=72,FontWeight=FontWeights.SemiBold },Height=75,Margin=new Thickness(4,0,4,18),Stretch=Stretch.Uniform });
        if(_estado.Anteriores.Count==0)_historico.Children.Add(new TextBlock { Text="Nenhuma chamada anterior",FontSize=16,TextWrapping=TextWrapping.Wrap,TextAlignment=TextAlignment.Center });
    }
}
