using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Licenciamento;
using VarthexComanda.Desktop.Tests.Xaml;

namespace VarthexComanda.Desktop.Tests;

public class QrAtivacaoTests
{
    [Fact]
    public void ProtocoloVersionadoNormalizaIdentificador()
    {
        Assert.Equal("varthex://ativacao?v=1&maquina=" + new string('A',64), QrAtivacao.Conteudo(new string('a',64)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("pc")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\n")]
    public void RejeitaIdentificadorInvalido(string value) => Assert.Throws<ArgumentException>(() => QrAtivacao.Conteudo(value));

    [Fact]
    public void GeraImagemEJanelaDeAtivacaoComEntradaManual()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var machine = new string('A',64);
            var png = QrAtivacao.Imagem(machine);
            Assert.Equal(new byte[]{137,80,78,71,13,10,26,10},png.Take(8));
            var service = new ServicoLicenca("unused", "unused", machine);
            var window = new JanelaLicenca(service,"Sem licença válida. Solicite a ativação ao responsável.");
            var panel = (StackPanel)((ScrollViewer)window.Content).Content;
            Assert.Single(panel.Children.OfType<Image>());
            Assert.Contains(panel.Children.OfType<TextBox>(),t=>t.IsReadOnly && t.Text==machine);
            var folder = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder,"ativacao-qr.png"),png);
                window.Show(); window.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);
                bitmap.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var file = File.Create(Path.Combine(folder,"ativacao-janela.png")); encoder.Save(file);
                window.Close();
            }
        });
    }
}
