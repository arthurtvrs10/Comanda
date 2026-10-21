using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using Xunit;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class ConfiguracaoAcessoXamlTests
{
    [Fact]
    public void AlterarSenhaTemTresCamposCompactosNosDoisTemas()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach (var escuro in new[] { false, true })
            {
                TemaInterface.Aplicar(escuro,false);
                var pasta=Path.Combine(Path.GetTempPath(),"acesso-ui-"+Guid.NewGuid());
                var janela=new JanelaAlterarAcesso(new AcessoGestor(Path.Combine(pasta,"gestor")),new AcessoAtendimento(Path.Combine(pasta,"pin")),false);
                Assert.Equal(350,janela.Width);
                var conteudo=(FrameworkElement)janela.Content;
                conteudo.Measure(new Size(350,400));conteudo.Arrange(new Rect(0,0,350,conteudo.DesiredSize.Height));conteudo.UpdateLayout();
                var campos=Filhos(conteudo).OfType<PasswordBox>().ToList();
                Assert.Equal(3,campos.Count);
                Assert.All(campos,c=>Assert.Equal(28,c.MinHeight));
                Assert.Single(Filhos(conteudo).OfType<Button>(),b=>Equals(b.Content,"Salvar"));
                var destino=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                if(!string.IsNullOrWhiteSpace(destino))
                {
                    Directory.CreateDirectory(destino);
                    var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)conteudo.ActualWidth,(int)conteudo.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(conteudo);
                    var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    using var arquivo=File.Create(Path.Combine(destino,$"alterar-senha-{(escuro?"escuro":"claro")}.png"));encoder.Save(arquivo);
                }
            }
            TemaInterface.Aplicar(false,false);
        });
    }

    [Fact]
    public void ComprovanteIncluiCodigoESenhaSemAlterarNumeroDaComanda()
    {
        var texto=ComprovanteConta.MontarTexto(new PreferenciasSistema(),"Estabelecimento",7,[],114,true);
        Assert.Contains("Comanda 07",texto);
        Assert.Contains("Pedido: 114",texto);
        Assert.EndsWith("SENHA DE ATENDIMENTO: 114",texto);
        var padrao=ComprovanteConta.MontarTexto(new PreferenciasSistema(),"Estabelecimento",7,[]);
        Assert.DoesNotContain("Pedido:",padrao);
        Assert.DoesNotContain("SENHA DE ATENDIMENTO",padrao);
    }

    private static IEnumerable<DependencyObject> Filhos(DependencyObject raiz)
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(raiz);i++)
        {
            var filho=VisualTreeHelper.GetChild(raiz,i);yield return filho;
            foreach(var neto in Filhos(filho))yield return neto;
        }
    }
}
