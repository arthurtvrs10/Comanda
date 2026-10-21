using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VarthexComanda.Desktop;

/// <summary>Janela filha limitada à área de trabalho, sem criar outra janela do Windows.</summary>
public sealed class JanelaInterna : Grid
{
    private readonly Border _moldura;
    private readonly Button _maximizar;
    private bool _posicionada;
    private double _x,_y;
    public UIElement Conteudo { get; }
    public bool Maximizada { get; private set; }
    public event Action? FecharSolicitado;

    public JanelaInterna(string titulo,UIElement conteudo)
    {
        Conteudo=conteudo;ClipToBounds=true;
        System.Windows.Automation.AutomationProperties.SetName(this,"Janela interna de "+titulo);
        _moldura=new Border { Name="MolduraInterna",HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top,BorderThickness=new Thickness(1) };
        _moldura.SetResourceReference(Border.BackgroundProperty,"Canvas");_moldura.SetResourceReference(Border.BorderBrushProperty,"Line");Children.Add(_moldura);
        var layout=new DockPanel();_moldura.Child=layout;
        var barra=new Grid { Height=30 };barra.SetResourceReference(Panel.BackgroundProperty,"RibbonHover");DockPanel.SetDock(barra,Dock.Top);layout.Children.Add(barra);
        var arrastar=new Thumb { Cursor=Cursors.SizeAll,Margin=new Thickness(0,0,84,0),Template=(ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Thumb'><Border Background='Transparent'/></ControlTemplate>") };
        System.Windows.Automation.AutomationProperties.SetName(arrastar,"Mover janela de "+titulo);
        arrastar.DragDelta+=(_,e)=>{if(Maximizada)return;_x+=e.HorizontalChange;_y+=e.VerticalChange;_posicionada=true;AjustarLimites();};
        arrastar.MouseDoubleClick+=(_,e)=>{AlternarMaximizar();e.Handled=true;};barra.Children.Add(arrastar);
        barra.Children.Add(new TextBlock { Text=titulo,Foreground=Brushes.White,FontSize=12,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(10,0,90,0),IsHitTestVisible=false,TextTrimming=TextTrimming.CharacterEllipsis });
        var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };barra.Children.Add(acoes);
        _maximizar=BotaoTitulo("Maximizar "+titulo,"M1,1 H11 V11 H1 Z");_maximizar.Click+=(_,_)=>AlternarMaximizar();acoes.Children.Add(_maximizar);
        var fechar=BotaoTitulo("Fechar "+titulo,"M1,1 L11,11 M11,1 L1,11");fechar.Click+=(_,_)=>FecharSolicitado?.Invoke();acoes.Children.Add(fechar);
        layout.Children.Add(new ContentControl { Content=conteudo,HorizontalContentAlignment=HorizontalAlignment.Stretch,VerticalContentAlignment=VerticalAlignment.Stretch });
        SizeChanged+=(_,_)=>AjustarLimites();Loaded+=(_,_)=>AjustarLimites();
        PreviewKeyDown+=(_,e)=>
        {
            if(e.KeyboardDevice.Modifiers!=ModifierKeys.Control)return;
            if(e.Key==Key.F4){FecharSolicitado?.Invoke();e.Handled=true;}
            else if(e.Key==Key.F10){AlternarMaximizar();e.Handled=true;}
        };
    }
    private static Button BotaoTitulo(string nome,string geometria)
    {
        var botao=new Button { Width=42,Height=30,MinHeight=0,MinWidth=0,Padding=new Thickness(0),BorderThickness=new Thickness(0),Background=Brushes.Transparent,Foreground=Brushes.White,ToolTip=nome,
            Content=new Path { Data=Geometry.Parse(geometria),Width=10,Height=10,Stretch=Stretch.Uniform,Stroke=Brushes.White,StrokeThickness=1.2 } };
        System.Windows.Automation.AutomationProperties.SetName(botao,nome);return botao;
    }
    public void AlternarMaximizar()
    {
        Maximizada=!Maximizada;
        var nome=Maximizada?"Restaurar janela":"Maximizar janela";
        System.Windows.Automation.AutomationProperties.SetName(_maximizar,nome);_maximizar.ToolTip=nome;
        ((Path)_maximizar.Content).Data=Geometry.Parse(Maximizada?"M1,4 H8 V11 H1 Z M4,4 V1 H11 V8 H8":"M1,1 H11 V11 H1 Z");
        AjustarLimites();
    }
    private void AjustarLimites()
    {
        if(ActualWidth<=0||ActualHeight<=0)return;
        if(Maximizada){_moldura.Width=ActualWidth;_moldura.Height=ActualHeight;_moldura.Margin=new Thickness(0);return;}
        var largura=Math.Max(0,Math.Min(1180,ActualWidth-32));
        var altura=Math.Max(0,Math.Min(600,ActualHeight-32));
        if(!_posicionada){_x=(ActualWidth-largura)/2;_y=(ActualHeight-altura)/2;}
        _x=Math.Clamp(_x,0,Math.Max(0,ActualWidth-largura));_y=Math.Clamp(_y,0,Math.Max(0,ActualHeight-altura));
        _moldura.Width=largura;_moldura.Height=altura;
        _moldura.Margin=new Thickness(_x,_y,0,0);
    }
}
