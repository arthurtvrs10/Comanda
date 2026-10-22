using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class FundoSistemaTests
{
    [Fact]
    public void ImagemPersonalizadaTemPrecedenciaEPodeRetornarAoPadrao()
    {
        ThreadingHelper.EmSta(() =>
        {
            var p=new PreferenciasSistema { CorFundo="#F3F5F2" };
            Assert.Equal(PreferenciasSistema.CorFundoPadrao,FundoSistema.Cor(p));
            using var stream=new MemoryStream();
            var encoder=new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(1,1,96,96,PixelFormats.Bgra32,null,new byte[]{0,0,0,255},4)));
            encoder.Save(stream);
            p.ImagemFundo=Convert.ToBase64String(stream.ToArray());p.PosicaoImagem="Repetir";
            var personalizado=FundoSistema.CriarPincel(p);
            Assert.IsType<BitmapImage>(personalizado.ImageSource);Assert.Equal(TileMode.Tile,personalizado.TileMode);
            Assert.Equal("#F3F5F2",FundoSistema.Cor(p));
            p.ImagemFundo="";
            Assert.Same(FundoSistema.ImagemPadrao,FundoSistema.CriarPincel(p).ImageSource);
            Assert.Equal(PreferenciasSistema.CorFundoPadrao,FundoSistema.Cor(p));
            p.CorFundo="#CCCCCC";Assert.Equal("#CCCCCC",FundoSistema.Cor(p));
            var capturas=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
            if(!string.IsNullOrWhiteSpace(capturas))
            {
                CarregamentoDeXamlTests.GarantirApp();
                using var banco=new VarthexComanda.Infrastructure.Tests.Homologacao.BancoDeHomologacao();
                var janela=CarregamentoDeXamlTests.CriarMainWindow(banco.Paths);
                try
                {
                    Directory.CreateDirectory(capturas);
                    foreach(var escuro in new[]{false,true})
                    {
                        TemaInterface.Aplicar(escuro,false);
                        var raiz=(System.Windows.FrameworkElement)janela.Content;
                        raiz.Measure(new System.Windows.Size(1280,720));raiz.Arrange(new System.Windows.Rect(0,0,1280,720));raiz.UpdateLayout();
                        var bitmap=new RenderTargetBitmap(1280,720,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);
                        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));
                        using var arquivo=File.Create(Path.Combine(capturas,$"fundo-padrao-{(escuro?"escuro":"claro")}.png"));png.Save(arquivo);
                    }
                }
                finally { janela.Close();TemaInterface.Aplicar(false,false); }
            }
        });
    }
}
