using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Xaml;

namespace VarthexComanda.Desktop.Tests.Configuracao;

public sealed class JanelaNumeracaoPedidosTests
{
    [Fact]
    public void ModalCompactoPreservaPreferenciasValidaInicioERenderizaDoisTemas()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var repo = new FakeConfiguracaoRepository();
            repo.Definir(NumeracaoPedidosService.ChaveUltimo, "114", DateTime.UtcNow);
            var service = new NumeracaoComandasService(repo);
            service.Salvar(new() { Ativa = true, Inicio = 100, UsarComoSenhaAtendimento = true }, DateTime.UtcNow);
            var janela = new JanelaNumeracaoPedidos(service);
            var raiz = (FrameworkElement)janela.Content;
            var diretorio = new DirectoryInfo(AppContext.BaseDirectory);
            while (diretorio.Parent is not null && !File.Exists(Path.Combine(diretorio.FullName, "backend", "VarthexComanda.slnx"))) diretorio = diretorio.Parent;
            var pasta = Path.Combine(diretorio.FullName, "backend", "artifacts", "numeracao", "previas");
            Directory.CreateDirectory(pasta);
            foreach (var escuro in new[] { false, true })
            {
                TemaInterface.Aplicar(escuro);
                foreach (var largura in new[] { 560d, 470d })
                {
                    raiz.Measure(new Size(largura, 440));
                    raiz.Arrange(new Rect(0, 0, largura, 440));
                    raiz.UpdateLayout();
                    var bitmap = new RenderTargetBitmap((int)largura, 440, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(raiz);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var stream = File.Create(Path.Combine(pasta, $"numeracao-{(escuro ? "escuro" : "claro")}-{largura}.png"));
                    encoder.Save(stream);
                }
            }
            var inicio = Descendentes(raiz).OfType<TextBox>().Single(t=>System.Windows.Automation.AutomationProperties.GetName(t)=="Começa com");
            inicio.Text = "0";
            var salvar = Descendentes(raiz).OfType<Button>().Single(b => Equals(b.Content, "Salvar"));
            salvar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Contains(Descendentes(raiz).OfType<TextBlock>(), t => t.Text.Contains("Informe um número inicial"));
            Assert.Equal(100, service.Carregar().Inicio);
            inicio.Text = "200";
            salvar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(janela.TemAlteracoes);
            Assert.Equal(200, service.Carregar().Inicio);
            Assert.Equal("114",repo.ObterValor(NumeracaoPedidosService.ChaveUltimo));
            TemaInterface.Aplicar(false);
        });
    }

    private static IEnumerable<DependencyObject> Descendentes(DependencyObject pai)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(pai); i++)
        {
            var filho = VisualTreeHelper.GetChild(pai, i);
            yield return filho;
            foreach (var descendente in Descendentes(filho)) yield return descendente;
        }
    }
}
