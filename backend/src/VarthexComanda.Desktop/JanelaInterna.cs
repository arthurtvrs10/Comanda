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
    private readonly Grid _barra;
    private readonly ViewportInterno _viewport;
    private readonly Thumb _arrastar;
    private readonly TranslateTransform _posicao=new();
    private CacheMode? _cacheAntesDoArraste;
    private bool _arrastando;
    private bool _posicionada;
    private double _x,_y;
    private double _larguraNormal=1180,_alturaNormal=600,_deslocamentoInicial;
    private readonly List<Thumb> _redimensionadores=new();
    public UIElement Conteudo { get; }
    public bool Maximizada { get; private set; }
    public event Action<JanelaInterna>? FecharSolicitado;

    public JanelaInterna(string titulo,UIElement conteudo,double larguraMinimaConteudo=640,double alturaMinimaConteudo=320)
    {
        Conteudo=conteudo;ClipToBounds=true;
        System.Windows.Automation.AutomationProperties.SetName(this,"Janela interna de "+titulo);
        _moldura=new Border { Name="MolduraInterna",HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top,BorderThickness=new Thickness(1),RenderTransform=_posicao };
        _moldura.SetResourceReference(Border.BackgroundProperty,"Canvas");_moldura.SetResourceReference(Border.BorderBrushProperty,"Line");Children.Add(_moldura);
        var superficie=new Grid();_moldura.Child=superficie;
        var layout=new DockPanel();superficie.Children.Add(layout);
        var barra=new Grid { Height=30 };_barra=barra;barra.SetResourceReference(Panel.BackgroundProperty,"RibbonHover");DockPanel.SetDock(barra,Dock.Top);layout.Children.Add(barra);
        barra.Resources["HoverOverlay"]=new SolidColorBrush(Color.FromArgb(38,255,255,255));
        var arrastar=new Thumb { Cursor=Cursors.SizeAll,Margin=new Thickness(0,0,84,0),Template=(ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Thumb'><Border Background='Transparent'/></ControlTemplate>") };_arrastar=arrastar;
        System.Windows.Automation.AutomationProperties.SetName(arrastar,"Mover janela de "+titulo);
        arrastar.DragStarted+=(_,_)=>IniciarArraste();
        arrastar.DragDelta+=(_,e)=>Mover(e.HorizontalChange,e.VerticalChange);
        arrastar.DragCompleted+=(_,_)=>FinalizarArraste();
        arrastar.MouseDoubleClick+=(_,e)=>{AlternarMaximizar();e.Handled=true;};barra.Children.Add(arrastar);
        barra.Children.Add(new TextBlock { Text=titulo,Foreground=Brushes.White,FontSize=12,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(10,0,90,0),IsHitTestVisible=false,TextTrimming=TextTrimming.CharacterEllipsis });
        var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };barra.Children.Add(acoes);
        _maximizar=BotaoTitulo("Maximizar "+titulo,"M1,1 H11 V11 H1 Z");_maximizar.Click+=(_,_)=>AlternarMaximizar();acoes.Children.Add(_maximizar);
        var fechar=BotaoTitulo("Fechar "+titulo,"M1,1 L11,11 M11,1 L1,11");fechar.Click+=(_,_)=>FecharSolicitado?.Invoke(this);acoes.Children.Add(fechar);
        _viewport=new ViewportInterno(conteudo,larguraMinimaConteudo,alturaMinimaConteudo);layout.Children.Add(_viewport);
        foreach(var (nome,horizontal,vertical,cursor) in new (string,int,int,Cursor)[]
        {
            ("esquerda",-1,0,Cursors.SizeWE),("direita",1,0,Cursors.SizeWE),
            ("superior",0,-1,Cursors.SizeNS),("inferior",0,1,Cursors.SizeNS),
            ("superior esquerda",-1,-1,Cursors.SizeNWSE),("superior direita",1,-1,Cursors.SizeNESW),
            ("inferior esquerda",-1,1,Cursors.SizeNESW),("inferior direita",1,1,Cursors.SizeNWSE)
        })
        {
            var alca=new Thumb { Cursor=cursor,HorizontalAlignment=horizontal<0?HorizontalAlignment.Left:horizontal>0?HorizontalAlignment.Right:HorizontalAlignment.Stretch,
                VerticalAlignment=vertical<0?VerticalAlignment.Top:vertical>0?VerticalAlignment.Bottom:VerticalAlignment.Stretch,
                Margin=horizontal==0?new Thickness(6,0,6,0):vertical==0?new Thickness(0,6,0,6):new Thickness(0),
                Template=(ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Thumb'><Border Background='Transparent'/></ControlTemplate>") };
            if(horizontal!=0)alca.Width=6;
            if(vertical!=0)alca.Height=6;
            System.Windows.Automation.AutomationProperties.SetName(alca,"Redimensionar "+nome);
            alca.DragDelta+=(_,e)=>Redimensionar(e.HorizontalChange,e.VerticalChange,horizontal,vertical);
            _redimensionadores.Add(alca);superficie.Children.Add(alca);
        }
        SizeChanged+=(_,_)=>AjustarLimites();Loaded+=(_,_)=>AjustarLimites();
        Unloaded+=(_,_)=>CancelarArraste();
        PreviewKeyDown+=(_,e)=>
        {
            if(e.KeyboardDevice.Modifiers!=ModifierKeys.Control)return;
            if(e.Key==Key.F4){FecharSolicitado?.Invoke(this);e.Handled=true;}
            else if(e.Key==Key.F10){AlternarMaximizar();e.Handled=true;}
        };
    }
    internal void DeslocarAbertura(double deslocamento)
    {
        if(!_posicionada)_deslocamentoInicial=deslocamento;
    }
    public void DefinirMinimoConteudo(double largura,double altura)=>_viewport.DefinirMinimo(largura,altura);
    private void IniciarArraste()
    {
        if(Maximizada||_arrastando)return;
        _arrastando=true;_cacheAntesDoArraste=_moldura.CacheMode;
        // O transform evita medir novamente; o cache evita redesenhar tabelas,
        // fotos e cartões a cada posição. Existe somente durante o gesto.
        var cache=new BitmapCache { EnableClearType=true,SnapsToDevicePixels=true,RenderAtScale=VisualTreeHelper.GetDpi(this).DpiScaleX };
        cache.Freeze();_moldura.CacheMode=cache;
    }
    private void FinalizarArraste()
    {
        if(!_arrastando)return;
        _moldura.CacheMode=_cacheAntesDoArraste;_cacheAntesDoArraste=null;_arrastando=false;
    }
    internal void CancelarArraste()
    {
        _arrastar.CancelDrag();FinalizarArraste();
    }
    internal void DefinirAtiva(bool ativa)
    {
        _barra.SetResourceReference(Panel.BackgroundProperty,ativa?"RibbonHover":"RibbonBar");
        System.Windows.Automation.AutomationProperties.SetItemStatus(this,ativa?"Janela ativa":"Janela em segundo plano");
    }
    private void Mover(double dx,double dy)
    {
        if(Maximizada||ActualWidth<=0||ActualHeight<=0||!double.IsFinite(dx)||!double.IsFinite(dy))return;
        _x=Math.Clamp(_x+dx,0,Math.Max(0,ActualWidth-_moldura.Width));
        _y=Math.Clamp(_y+dy,0,Math.Max(0,ActualHeight-_moldura.Height));
        _posicionada=true;
        // Thumb calcula o próximo delta na posição visual atual. Margin aguardava
        // outro layout e também voltava a medir a área inteira a cada movimento.
        _posicao.X=_x;_posicao.Y=_y;
    }
    private void Redimensionar(double dx,double dy,int horizontal,int vertical)
    {
        if(Maximizada||ActualWidth<=0||ActualHeight<=0||!double.IsFinite(dx)||!double.IsFinite(dy))return;
        var largura=_moldura.Width;var altura=_moldura.Height;
        var minimaLargura=Math.Min(640,ActualWidth);var minimaAltura=Math.Min(360,ActualHeight);
        if(horizontal<0)
        {
            var direita=_x+largura;_x=Math.Clamp(_x+dx,0,Math.Max(0,direita-minimaLargura));largura=direita-_x;
        }
        else if(horizontal>0)largura=Math.Clamp(largura+dx,Math.Min(minimaLargura,ActualWidth-_x),ActualWidth-_x);
        if(vertical<0)
        {
            var baseJanela=_y+altura;_y=Math.Clamp(_y+dy,0,Math.Max(0,baseJanela-minimaAltura));altura=baseJanela-_y;
        }
        else if(vertical>0)altura=Math.Clamp(altura+dy,Math.Min(minimaAltura,ActualHeight-_y),ActualHeight-_y);
        _larguraNormal=largura;_alturaNormal=altura;_posicionada=true;AjustarLimites();
    }
    private static Button BotaoTitulo(string nome,string geometria)
    {
        var botao=new Button { Width=42,Height=30,MinHeight=0,MinWidth=0,Padding=new Thickness(0),BorderThickness=new Thickness(0),Background=Brushes.Transparent,Foreground=Brushes.White,ToolTip=nome,
            Content=new Path { Data=Geometry.Parse(geometria),Width=10,Height=10,Stretch=Stretch.Uniform,Stroke=Brushes.White,StrokeThickness=1.2 } };
        System.Windows.Automation.AutomationProperties.SetName(botao,nome);return botao;
    }
    public void AlternarMaximizar()
    {
        CancelarArraste();
        Maximizada=!Maximizada;
        var nome=Maximizada?"Restaurar janela":"Maximizar janela";
        System.Windows.Automation.AutomationProperties.SetName(_maximizar,nome);_maximizar.ToolTip=nome;
        ((Path)_maximizar.Content).Data=Geometry.Parse(Maximizada?"M1,4 H8 V11 H1 Z M4,4 V1 H11 V8 H8":"M1,1 H11 V11 H1 Z");
        AjustarLimites();
    }
    private void AjustarLimites()
    {
        if(ActualWidth<=0||ActualHeight<=0)return;
        if(Maximizada)
        {
            _moldura.Width=ActualWidth;
            _moldura.Height=ActualHeight;
            _posicao.X=0;_posicao.Y=0;
            foreach(var alca in _redimensionadores)alca.Visibility=Visibility.Collapsed;
            return;
        }
        foreach(var alca in _redimensionadores)alca.Visibility=Visibility.Visible;
        var largura=Math.Max(0,Math.Min(_larguraNormal,ActualWidth));
        var altura=Math.Max(0,Math.Min(_alturaNormal,ActualHeight));
        if(!_posicionada){_x=(ActualWidth-largura)/2+_deslocamentoInicial;_y=(ActualHeight-altura)/2+_deslocamentoInicial;_posicionada=true;}
        _x=Math.Clamp(_x,0,Math.Max(0,ActualWidth-largura));_y=Math.Clamp(_y,0,Math.Max(0,ActualHeight-altura));
        _moldura.Width=largura;_moldura.Height=altura;
        _posicao.X=_x;_posicao.Y=_y;
    }

    // Mede a tela em um espaço finito: mantém listas virtualizadas e oferece
    // rolagem quando o usuário reduz a moldura além do espaço útil do formulário.
    private sealed class ViewportInterno : ScrollViewer
    {
        private readonly ContentControl _apresentador;
        private double _larguraMinima,_alturaMinima;
        public ViewportInterno(UIElement conteudo,double larguraMinima,double alturaMinima)
        {
            Name="ViewportJanelaInterna";
            _larguraMinima=larguraMinima;_alturaMinima=alturaMinima;
            HorizontalScrollBarVisibility=ScrollBarVisibility.Auto;VerticalScrollBarVisibility=ScrollBarVisibility.Auto;
            HorizontalContentAlignment=HorizontalAlignment.Stretch;VerticalContentAlignment=VerticalAlignment.Stretch;
            _apresentador=new ContentControl { Content=conteudo,HorizontalContentAlignment=HorizontalAlignment.Stretch,VerticalContentAlignment=VerticalAlignment.Stretch };
            Content=_apresentador;
        }
        public void DefinirMinimo(double largura,double altura)
        {
            _larguraMinima=Math.Max(0,largura);_alturaMinima=Math.Max(0,altura);InvalidateMeasure();
        }
        protected override Size MeasureOverride(Size constraint)
        {
            var largura=double.IsInfinity(constraint.Width)?_larguraMinima:constraint.Width;
            var altura=double.IsInfinity(constraint.Height)?_alturaMinima:constraint.Height;
            bool rolarH=false,rolarV=false;
            for(var i=0;i<3;i++)
            {
                var disponivelW=Math.Max(0,largura-(rolarV?SystemParameters.VerticalScrollBarWidth:0));
                var disponivelH=Math.Max(0,altura-(rolarH?SystemParameters.HorizontalScrollBarHeight:0));
                rolarH=disponivelW<_larguraMinima;rolarV=disponivelH<_alturaMinima;
            }
            _apresentador.Width=Math.Max(_larguraMinima,largura-(rolarV?SystemParameters.VerticalScrollBarWidth:0));
            _apresentador.Height=Math.Max(_alturaMinima,altura-(rolarH?SystemParameters.HorizontalScrollBarHeight:0));
            return base.MeasureOverride(constraint);
        }
    }
}
