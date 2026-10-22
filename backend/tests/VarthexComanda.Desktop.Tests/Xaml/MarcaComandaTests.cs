using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class MarcaComandaTests
{
    [Fact]
    public void EntradaCarregaMarcaVetorialEAlternaComOTema()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var pasta = Path.Combine(Path.GetTempPath(), "comanda-marca-" + Guid.NewGuid());
            var janela = new JanelaLogin(new AcessoGestor(Path.Combine(pasta,"gestor")), new AcessoAtendimento(Path.Combine(pasta,"pin")));
            try
            {
                foreach (var escuro in new[] { false, true })
                {
                    TemaInterface.Aplicar(escuro);
                    var conteudo = (StackPanel)janela.Content;
                    var logo = Assert.Single(conteudo.Children.OfType<Image>());
                    Assert.Same(escuro ? MarcaComanda.LogoEscuro : MarcaComanda.LogoClaro, logo.Source);
                    Assert.IsType<DrawingImage>(logo.Source);
                    Assert.True(logo.Source.Width > logo.Source.Height);
                    conteudo.Measure(new Size(372, 700));
                    var altura = (int)Math.Ceiling(conteudo.DesiredSize.Height);
                    conteudo.Arrange(new Rect(0,0,372,altura));conteudo.UpdateLayout();
                    Assert.True(logo.ActualWidth > 0);
                    var destino = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                    if (!string.IsNullOrWhiteSpace(destino))
                    {
                        Directory.CreateDirectory(destino);
                        var bitmap = new RenderTargetBitmap(420,altura+48,96,96,PixelFormats.Pbgra32);
                        var desenho = new DrawingVisual();
                        using(var dc=desenho.RenderOpen())
                        {
                            dc.DrawRectangle((Brush)System.Windows.Application.Current.Resources["Surface"],null,new Rect(0,0,420,altura+48));
                            dc.PushTransform(new TranslateTransform(24,24));
                            dc.DrawRectangle(new VisualBrush(conteudo),null,new Rect(0,0,372,altura));
                        }
                        bitmap.Render(desenho);
                        var encoder = new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using var arquivo = File.Create(Path.Combine(destino,$"comanda-entrada-{(escuro?"escuro":"claro")}.png"));encoder.Save(arquivo);
                    }
                }
            }
            finally { janela.Close();TemaInterface.Aplicar(false); }
        });
    }
}
