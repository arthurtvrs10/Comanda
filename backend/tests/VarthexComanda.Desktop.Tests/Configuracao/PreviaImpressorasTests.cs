using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Xaml;
namespace VarthexComanda.Desktop.Tests.Configuracao;
public class PreviaImpressorasTests
{
    [Fact] public void PreviaFicaVisivelNaJanelaPadraoEAcessivelNaJanelaMenor()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try{foreach(var escuro in new[]{false,true})foreach(var pequeno in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);var janela=new JanelaImpressoras(new(new FakeConfiguracaoRepository()),consultar:()=>[],testar:_=>{});
                var raiz=(FrameworkElement)janela.Content;var largura=pequeno?740d:980d;var altura=pequeno?430d:580d;
                void Arranjar(){raiz.Measure(new Size(largura,altura));raiz.Arrange(new Rect(0,0,largura,altura));raiz.UpdateLayout();}
                Arranjar();var lista=AlinhamentoInterfaceTests.Filhos(raiz).OfType<ListBox>().Single();lista.SelectedItem=lista.Items.OfType<ListBoxItem>().Single(i=>Equals(i.Tag,"Layout do ticket"));Arranjar();Arranjar();
                var previa=AlinhamentoInterfaceTests.Filhos(raiz).OfType<FlowDocumentScrollViewer>().Single();Assert.NotNull(previa.Document);Assert.NotEmpty(previa.Document!.Blocks);
                var rolagem=AlinhamentoInterfaceTests.Filhos(raiz).OfType<ScrollViewer>().Single(s=>s.Content is Grid);
                if(pequeno){Assert.True(rolagem.ScrollableHeight>0);rolagem.ScrollToBottom();Arranjar();}
                var posicao=previa.TransformToAncestor(raiz).TransformBounds(new Rect(previa.RenderSize));var visivel=Rect.Intersect(posicao,new Rect(0,0,largura,altura-45));Assert.False(visivel.IsEmpty);Assert.True(visivel.Height>100);Assert.True(visivel.Width>200);
                var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(pasta is not null){Directory.CreateDirectory(pasta);var imagem=new RenderTargetBitmap((int)largura,(int)altura,96,96,PixelFormats.Pbgra32);imagem.Render(raiz);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(imagem));using var saida=File.Create(Path.Combine(pasta,$"impressoras-previa-{(escuro?"escuro":"claro")}-{(pequeno?"menor":"padrao")}.png"));png.Save(saida);}
            }}finally{TemaInterface.Aplicar(false,false);}
        });
    }
}
