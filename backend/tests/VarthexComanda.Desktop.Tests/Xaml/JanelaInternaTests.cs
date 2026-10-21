using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class JanelaInternaTests
{
    [Fact]
    public void JanelaPermaneceDentroDaAreaMaximizaRestauraMoveEFechaSemFecharSistema()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var produto=new VarthexComanda.Desktop.Catalogo.ProdutosView(CarregamentoDeXamlTests.CriarProdutos());
            var interna=new JanelaInterna("Produtos",produto);
            var area=new Grid();area.SetResourceReference(Panel.BackgroundProperty,"Canvas");area.Children.Add(interna);
            var moldura=(Border)AlinhamentoInterfaceTests.Filhos(interna).Single(c=>c is Border b&&b.Name=="MolduraInterna");
            bool fechou=false;interna.FecharSolicitado+=_=>{fechou=true;area.Children.Remove(interna);};
            void Organizar(double w,double h){area.Measure(new Size(w,h));area.Arrange(new Rect(0,0,w,h));area.UpdateLayout();}
            void ConferirLimites()
            {
                var pos=moldura.TranslatePoint(new Point(),area);Assert.True(pos.X>=0&&pos.Y>=0);
                Assert.True(pos.X+moldura.ActualWidth<=area.ActualWidth+1);Assert.True(pos.Y+moldura.ActualHeight<=area.ActualHeight+1);
            }
            foreach(var escuro in new[]{false,true})
            foreach(var tamanho in new[]{new Size(1920,1080),new Size(1440,800),new Size(800,440)})
            {
                TemaInterface.Aplicar(escuro,false);Organizar(tamanho.Width,tamanho.Height);ConferirLimites();
                Assert.True(moldura.ActualWidth<=area.ActualWidth);Assert.True(moldura.ActualHeight<=area.ActualHeight);
                var mover=AlinhamentoInterfaceTests.Filhos(interna).OfType<Thumb>().Single(t=>System.Windows.Automation.AutomationProperties.GetName(t)=="Mover janela de Produtos");
                mover.RaiseEvent(new DragDeltaEventArgs(90,-40){RoutedEvent=Thumb.DragDeltaEvent});Organizar(tamanho.Width,tamanho.Height);ConferirLimites();
                var posicao=moldura.Margin;
                interna.AlternarMaximizar();Organizar(tamanho.Width,tamanho.Height);Assert.True(interna.Maximizada);ConferirLimites();
                Assert.Equal(area.ActualWidth,moldura.ActualWidth);
                Assert.Equal(area.ActualHeight,moldura.ActualHeight);
                Assert.Equal(0,moldura.Margin.Left);Assert.Equal(0,moldura.Margin.Top);
                Assert.Equal(area.ActualWidth,moldura.Margin.Left+moldura.ActualWidth);
                Assert.Equal(area.ActualHeight,moldura.Margin.Top+moldura.ActualHeight);
                var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                if(!string.IsNullOrWhiteSpace(pasta))
                {
                    Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap((int)tamanho.Width,(int)tamanho.Height,96,96,PixelFormats.Pbgra32);bitmap.Render(area);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,$"produtos-maximizado-{(escuro?"escuro":"claro")}-{tamanho.Width}.png"));encoder.Save(arquivo);
                }
                interna.AlternarMaximizar();Organizar(tamanho.Width,tamanho.Height);Assert.False(interna.Maximizada);Assert.Equal(posicao,moldura.Margin);
            }
            var fechar=AlinhamentoInterfaceTests.Filhos(interna).OfType<Button>().Single(b=>System.Windows.Automation.AutomationProperties.GetName(b)=="Fechar Produtos");
            fechar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.True(fechou);Assert.Empty(area.Children.Cast<UIElement>());
            area.Children.Add(interna);Organizar(800,440);Assert.Same(produto,interna.Conteudo);ConferirLimites();TemaInterface.Aplicar(false,false);
        });
    }
}
