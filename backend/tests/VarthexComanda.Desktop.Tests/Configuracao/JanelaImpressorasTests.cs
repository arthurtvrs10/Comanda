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

public class JanelaImpressorasTests
{
    [Fact]
    public void ConfiguracaoSomenteImprimeQuandoUsuarioClicaNoTesteEPersisteSelecao()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp(); var repo = new FakeConfiguracaoRepository(); var service = new ConfiguracoesImpressaoService(repo); var testes = 0;
            var janela = new JanelaImpressoras(service, consultar: () => [new("Cupom USB (exemplo de teste)", "Cupom USB", false), new("Cozinha (exemplo de teste)", @"\\servidor\cozinha", true)], testar: _ => testes++);
            janela.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            var raiz = (FrameworkElement)janela.Content;
            Descendentes(raiz).OfType<RadioButton>().Single(b => Equals(b.Content, "Impressoras instaladas")).IsChecked = true;
            Descendentes(raiz).OfType<ComboBox>().Single().SelectedIndex = 0;
            Assert.True(janela.TemAlteracoes); Assert.Equal(0, testes);
            Descendentes(raiz).OfType<Button>().Single(b => Equals(b.Content, "Salvar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(janela.TemAlteracoes); Assert.Equal(0, testes); Assert.Equal("Cupom USB", service.ObterCaixaConfigurado()!.Caminho);
            Descendentes(raiz).OfType<Button>().Single(b => Equals(b.Content, "Testar impressora")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Assert.Equal(1, testes);
            foreach (var escuro in new[] { false, true })
            {
                TemaInterface.Aplicar(escuro);
                foreach (var largura in new[] { 1000d, 760d })
                {
                    raiz.Measure(new Size(largura, 590)); raiz.Arrange(new Rect(0, 0, largura, 590)); raiz.UpdateLayout();
                    var bitmap = new RenderTargetBitmap((int)largura, 590, 96, 96, PixelFormats.Pbgra32); bitmap.Render(raiz);
                    var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    var pasta = Path.Combine(Path.GetTempPath(), "varthex-previas-impressoras"); Directory.CreateDirectory(pasta);
                    using var stream = File.Create(Path.Combine(pasta, $"impressoras-{(escuro ? "escuro" : "claro")}-{largura}.png")); encoder.Save(stream);
                }
            }
            TemaInterface.Aplicar(false);
        });
    }
    private static IEnumerable<DependencyObject> Descendentes(DependencyObject pai)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(pai); i++) { var filho = VisualTreeHelper.GetChild(pai, i); yield return filho; foreach (var descendente in Descendentes(filho)) yield return descendente; }
        if (pai is FrameworkElement e) foreach (var filho in LogicalTreeHelper.GetChildren(e).OfType<DependencyObject>()) if (VisualTreeHelper.GetChildrenCount(pai) == 0) { yield return filho; foreach (var descendente in Descendentes(filho)) yield return descendente; }
    }
}
