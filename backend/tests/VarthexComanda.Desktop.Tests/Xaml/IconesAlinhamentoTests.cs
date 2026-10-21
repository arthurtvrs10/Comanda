using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class IconesAlinhamentoTests
{
    private static readonly Type Tipo=typeof(MainWindow).Assembly.GetType("VarthexComanda.Desktop.IconeVarthex")!;
    private static readonly MethodInfo Criar=typeof(MainWindow).Assembly.GetType("VarthexComanda.Desktop.IconesVarthex")!.GetMethod("Criar")!;
    private static FrameworkElement Icone(object tipo,double tamanho)=>(FrameworkElement)Criar.Invoke(null,new[]{tipo,(object)tamanho})!;

    [Fact]
    public void TodosOsDesenhosFicamCentralizadosSemCortesEmTamanhosEEscalasDoWindows()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var catalogo=new UniformGrid { Columns=7,Width=770 };
            foreach(var tipo in Enum.GetValues(Tipo))
            {
                foreach(var tamanho in new[]{16d,20d,30d,32d})
                foreach(var escala in new[]{1d,1.25,1.5,2d})
                {
                    var icone=Icone(tipo,tamanho);
                    icone.Measure(new Size(tamanho,tamanho));icone.Arrange(new Rect(0,0,tamanho,tamanho));icone.UpdateLayout();
                    var pixels=(int)Math.Ceiling(tamanho*escala);
                    var bitmap=new RenderTargetBitmap(pixels,pixels,96*escala,96*escala,PixelFormats.Pbgra32);bitmap.Render(icone);
                    var dados=new byte[pixels*pixels*4];bitmap.CopyPixels(dados,pixels*4,0);
                    int esquerda=pixels,topo=pixels,direita=-1,baseIcone=-1;
                    for(var y=0;y<pixels;y++)for(var x=0;x<pixels;x++)
                    {
                        if(dados[(y*pixels+x)*4+3]<=10)continue;
                        esquerda=Math.Min(esquerda,x);direita=Math.Max(direita,x);topo=Math.Min(topo,y);baseIcone=Math.Max(baseIcone,y);
                    }
                    var contexto=$"{tipo}, {tamanho} DIP, escala {escala}";
                    Assert.True(direita>=esquerda&&baseIcone>=topo,"Ícone vazio: "+contexto);
                    Assert.True(esquerda>=1&&topo>=1&&direita<=pixels-2&&baseIcone<=pixels-2,"Traço encostando no recorte: "+contexto);
                    Assert.True(Math.Abs((esquerda+direita)/2d-(pixels-1)/2d)<=1,"Deslocamento horizontal: "+contexto);
                    Assert.True(Math.Abs((topo+baseIcone)/2d-(pixels-1)/2d)<=1,"Deslocamento vertical: "+contexto);
                }
                var celula=new Grid { Width=110,Height=90,Background=Brushes.White };
                var iconeCatalogo=Icone(tipo,32);iconeCatalogo.VerticalAlignment=VerticalAlignment.Top;iconeCatalogo.Margin=new Thickness(0,14,0,0);celula.Children.Add(iconeCatalogo);
                celula.Children.Add(new TextBlock { Text=tipo.ToString(),FontSize=11,Foreground=Brushes.Black,TextAlignment=TextAlignment.Center,VerticalAlignment=VerticalAlignment.Bottom,Margin=new Thickness(0,0,0,10) });
                catalogo.Children.Add(new Border { Child=celula,BorderBrush=Brushes.LightGray,BorderThickness=new Thickness(1) });
            }
            var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
            if(!string.IsNullOrWhiteSpace(pasta))
            {
                Directory.CreateDirectory(pasta);catalogo.Measure(new Size(770,1000));catalogo.Arrange(new Rect(0,0,770,catalogo.DesiredSize.Height));catalogo.UpdateLayout();
                var bitmap=new RenderTargetBitmap(1155,(int)(catalogo.ActualHeight*1.5),144,144,PixelFormats.Pbgra32);bitmap.Render(catalogo);
                var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,"icones-catalogo.png"));encoder.Save(arquivo);
            }
        });
    }

    [Fact]
    public void RibbonMantemIconesNoMesmoEixoERotulosInteiros()
    {
        var pasta=Path.Combine(Path.GetTempPath(),"icones-ribbon-"+Guid.NewGuid());
        try
        {
            ThreadingHelper.EmSta(()=>
            {
                CarregamentoDeXamlTests.GarantirApp();
                var janela=CarregamentoDeXamlTests.CriarMainWindow(new VarthexComanda.Infrastructure.Storage.AppPaths(pasta));
                var categorias=(WrapPanel)janela.FindName("CategoriasRibbon");
                var comandos=(FrameworkElement)janela.FindName("GruposRibbon");var raiz=(FrameworkElement)janela.Content;
                foreach(var categoria in categorias.Children.OfType<Button>())
                {
                    categoria.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    raiz.Measure(new Size(1280,780));raiz.Arrange(new Rect(0,0,1280,780));raiz.UpdateLayout();
                    double? eixo=null;
                    foreach(var botao in AlinhamentoInterfaceTests.Filhos(comandos).OfType<Button>())
                    {
                        var imagem=AlinhamentoInterfaceTests.Filhos(botao).OfType<Image>().Single();
                        var centro=imagem.TranslatePoint(new Point(imagem.ActualWidth/2,imagem.ActualHeight/2),botao);
                        Assert.InRange(Math.Abs(centro.X-botao.ActualWidth/2),0,0.5);
                        var centroNaFaixa=imagem.TranslatePoint(new Point(imagem.ActualWidth/2,imagem.ActualHeight/2),comandos);
                        if(eixo is null)eixo=centroNaFaixa.Y;else Assert.InRange(Math.Abs(centroNaFaixa.Y-eixo.Value),0,0.5);
                        foreach(var elemento in new FrameworkElement[]{imagem,AlinhamentoInterfaceTests.Filhos(botao).OfType<TextBlock>().Single()})
                        {
                            var pos=elemento.TranslatePoint(new Point(),botao);
                            Assert.True(pos.X>=0&&pos.Y>=0);
                            Assert.True(pos.X+elemento.ActualWidth<=botao.ActualWidth+0.5);
                            Assert.True(pos.Y+elemento.ActualHeight<=botao.ActualHeight+0.5);
                            Assert.True(elemento.DesiredSize.Height<=elemento.ActualHeight+0.5);
                        }
                    }
                }
            });
        }
        finally { if(Directory.Exists(pasta))Directory.Delete(pasta,true); }
    }
}
