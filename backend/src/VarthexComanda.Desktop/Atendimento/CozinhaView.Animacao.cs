using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VarthexComanda.Desktop.Atendimento;
public partial class CozinhaView
{
    private PreviaArraste? _previa;
    private AdornerLayer? _camadaPrevia;
    private Border? _origemVisual;
    private void IniciarPrevia(Border cartao)
    {
        if(!SystemParameters.ClientAreaAnimation)return;
        _origemVisual=cartao;_camadaPrevia=AdornerLayer.GetAdornerLayer(this);
        if(_camadaPrevia is not null){_previa=new PreviaArraste(this,cartao);_previa.Posicao=_inicioArraste;_camadaPrevia.Add(_previa);}
        cartao.BeginAnimation(OpacityProperty,new DoubleAnimation(0.35,TimeSpan.FromMilliseconds(120)));
    }
    private void EncerrarPrevia()
    {
        if(_previa is not null)_camadaPrevia?.Remove(_previa);
        _origemVisual?.BeginAnimation(OpacityProperty,null);_origemVisual=null;_previa=null;_camadaPrevia=null;
    }
    private void AnimarEntrada(Border coluna,int pedidoId)
    {
        if(!SystemParameters.ClientAreaAnimation)return;
        coluna.UpdateLayout();
        foreach(var cartao in Descendentes(coluna).OfType<Border>().Where(b=>b.DataContext is PedidoKds p&&p.Id==pedidoId&&b.Style==FindResource("MolduraPedido")))
        {
            var movimento=new ScaleTransform();cartao.RenderTransform=movimento;cartao.RenderTransformOrigin=new Point(0.5,0.5);
            var duracao=TimeSpan.FromMilliseconds(220);var curva=new CubicEase {EasingMode=EasingMode.EaseOut};
            movimento.BeginAnimation(ScaleTransform.ScaleXProperty,new DoubleAnimation(0.97,1,duracao){EasingFunction=curva});
            movimento.BeginAnimation(ScaleTransform.ScaleYProperty,new DoubleAnimation(0.97,1,duracao){EasingFunction=curva});
            cartao.BeginAnimation(OpacityProperty,new DoubleAnimation(0.4,1,duracao){EasingFunction=curva});
        }
    }
    private static IEnumerable<DependencyObject> Descendentes(DependencyObject raiz)
    {
        for(var i=0;i<VisualTreeHelper.GetChildrenCount(raiz);i++){var filho=VisualTreeHelper.GetChild(raiz,i);yield return filho;foreach(var neto in Descendentes(filho))yield return neto;}
    }
    private sealed class PreviaArraste : Adorner
    {
        private readonly Brush _imagem;
        private readonly Size _tamanho;
        public Point Posicao {get;set;}
        public PreviaArraste(UIElement superficie,Border cartao):base(superficie)
        {
            IsHitTestVisible=false;Opacity=0.85;
            var disponivel=superficie.RenderSize;
            var escala=Math.Min(1,Math.Min(Math.Max(1,disponivel.Width-24)/Math.Max(1,cartao.ActualWidth),Math.Max(1,disponivel.Height-24)/Math.Max(1,cartao.ActualHeight)));
            _tamanho=new Size(cartao.ActualWidth*escala,cartao.ActualHeight*escala);
            _imagem=new VisualBrush(cartao){Stretch=Stretch.Uniform,AlignmentY=AlignmentY.Top,ViewboxUnits=BrushMappingMode.Absolute,Viewbox=new Rect(0,0,cartao.ActualWidth,cartao.ActualHeight)};
        }
        protected override void OnRender(DrawingContext dc)
        {
            var limite=AdornedElement.RenderSize;
            var x=Math.Clamp(Posicao.X+14,12,Math.Max(12,limite.Width-_tamanho.Width-12));
            var y=Math.Clamp(Posicao.Y+12,12,Math.Max(12,limite.Height-_tamanho.Height-12));
            dc.DrawRectangle(_imagem,new Pen(Brushes.Gray,1),new Rect(new Point(x,y),_tamanho));
        }
    }
}
