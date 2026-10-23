using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Xunit.Abstractions;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class MovimentoWorkspaceTests(ITestOutputHelper saida)
{
    [Fact]
    public void ArrastarNaoRefazMedicaoDaAreaANemDeOutrasJanelas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var raiz=new PainelMedido();var area=new AreaTrabalhoInterna();raiz.Children.Add(area);
            var conteudo=new PainelMedido();conteudo.Children.Add(new TextBox{Text="Rascunho preservado"});
            var mesas=new JanelaInterna("Mesas / comandas",conteudo);var produtos=new JanelaInterna("Produtos",new Grid());
            area.Abrir(produtos);area.Abrir(mesas);
            raiz.Measure(new Size(1920,1000));raiz.Arrange(new Rect(0,0,1920,1000));raiz.UpdateLayout();
            var medidasRaiz=raiz.Medidas;var medidasConteudo=conteudo.Medidas;var relogio=Stopwatch.StartNew();
            var invalidacoes=0;
            for(var i=0;i<240;i++)
            {
                AreaTrabalhoInternaTests.Mover(mesas,i%2==0?1:-1,i%2==0?1:-1);
                if(!raiz.IsMeasureValid)invalidacoes++;
                raiz.UpdateLayout();
            }
            relogio.Stop();
            saida.WriteLine($"240 movimentos: {invalidacoes} invalidações, {raiz.Medidas-medidasRaiz} medições da área, {conteudo.Medidas-medidasConteudo} do conteúdo, {relogio.Elapsed.TotalMilliseconds:F2} ms.");
            Assert.Equal(0,invalidacoes);Assert.Equal(medidasRaiz,raiz.Medidas);Assert.Equal(medidasConteudo,conteudo.Medidas);
            Assert.Equal("Rascunho preservado",((TextBox)conteudo.Children[0]).Text);Assert.Equal(2,area.Janelas.Count);Assert.Same(mesas,area.JanelaAtiva);
        });
    }
    [Fact]
    public void MovimentoSegueImediatamenteOMouseEPreservaCliquesSemAguardarLayout()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var area=new AreaTrabalhoInterna();var janela=new JanelaInterna("Mesas / comandas",new TextBox{Text="Texto em edição"});
            area.Abrir(janela);area.Measure(new Size(1920,1000));area.Arrange(new Rect(0,0,1920,1000));area.UpdateLayout();
            var antes=AreaTrabalhoInternaTests.Posicao(janela);var tamanho=AreaTrabalhoInternaTests.Moldura(janela).RenderSize;
            for(var i=0;i<50;i++)
            {
                AreaTrabalhoInternaTests.Mover(janela,1,-1);
                Assert.Equal(new Point(antes.X+i+1,antes.Y-i-1),AreaTrabalhoInternaTests.Posicao(janela));
            }
            Assert.Equal(tamanho,AreaTrabalhoInternaTests.Moldura(janela).RenderSize);
            var ponto=AreaTrabalhoInternaTests.Moldura(janela).TranslatePoint(new Point(20,15),area);
            var alvo=System.Windows.Media.VisualTreeHelper.HitTest(area,ponto)?.VisualHit;
            while(alvo is not null&&alvo is not JanelaInterna)alvo=System.Windows.Media.VisualTreeHelper.GetParent(alvo);
            Assert.Same(janela,alvo);
            janela.AlternarMaximizar();area.UpdateLayout();var maximizada=AreaTrabalhoInternaTests.Posicao(janela);
            AreaTrabalhoInternaTests.Mover(janela,200,100);Assert.Equal(maximizada,AreaTrabalhoInternaTests.Posicao(janela));
            janela.AlternarMaximizar();area.UpdateLayout();Assert.Equal(new Point(antes.X+50,antes.Y-50),AreaTrabalhoInternaTests.Posicao(janela));
        });
    }
    private sealed class PainelMedido : Grid
    {
        public int Medidas { get;private set; }
        protected override Size MeasureOverride(Size constraint){Medidas++;return base.MeasureOverride(constraint);}
    }
}
