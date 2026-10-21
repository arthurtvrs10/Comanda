using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class AreaTrabalhoInternaTests
{
    [Fact]
    public void JanelasCoexistemRecebemCliquesSaoOrganizadasEFechamIndependentemente()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var area=new AreaTrabalhoInterna();
            var campo=new TextBox { Text="Em edição" };
            var mesas=new JanelaInterna("Mesas / comandas",campo);
            var produtos=new JanelaInterna("Produtos",new TextBox());
            void Layout(){area.Measure(new Size(1920,1000));area.Arrange(new Rect(0,0,1920,1000));area.UpdateLayout();}
            area.Abrir(mesas);Layout();area.Abrir(produtos);Layout();
            Assert.Equal(2,area.Janelas.Count);Assert.Same(produtos,area.JanelaAtiva);
            Assert.True(Panel.GetZIndex(produtos)>Panel.GetZIndex(mesas));
            // A área transparente da janela superior não bloqueia a barra da outra.
            var molduraMesas=Moldura(mesas);
            var ponto=molduraMesas.TranslatePoint(new Point(20,15),area);
            var alvo=VisualTreeHelper.HitTest(area,ponto)?.VisualHit;
            while(alvo is not null && alvo is not JanelaInterna)alvo=VisualTreeHelper.GetParent(alvo);
            Assert.Same(mesas,alvo);
            mesas.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice,0,MouseButton.Left){RoutedEvent=UIElement.PreviewMouseDownEvent});
            Assert.Same(mesas,area.JanelaAtiva);Assert.True(Panel.GetZIndex(mesas)>Panel.GetZIndex(produtos));
            Redimensionar(mesas,"inferior direita",-480,-160);Layout();
            Mover(mesas,-300,-160);Layout();
            Redimensionar(produtos,"inferior direita",-420,-120);Layout();
            Mover(produtos,580,0);Layout();
            Assert.True(Moldura(mesas).Margin.Left+Moldura(mesas).ActualWidth<Moldura(produtos).Margin.Left);
            var tamanho=new Size(molduraMesas.ActualWidth,molduraMesas.ActualHeight);var posicao=molduraMesas.Margin;
            area.Abrir(mesas);Assert.Equal(2,area.Janelas.Count);
            mesas.AlternarMaximizar();Layout();
            Assert.Equal(area.RenderSize,molduraMesas.RenderSize);Assert.Equal(new Thickness(0),molduraMesas.Margin);
            mesas.AlternarMaximizar();Layout();Assert.Equal(tamanho,molduraMesas.RenderSize);Assert.Equal(posicao,molduraMesas.Margin);
            Botao(mesas,"Fechar Mesas / comandas").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Single(area.Janelas);Assert.Same(produtos,area.JanelaAtiva);
            area.Abrir(mesas);Layout();Assert.Equal("Em edição",campo.Text);Assert.Equal(posicao,molduraMesas.Margin);
            area.Limpar();Assert.Empty(area.Janelas);Assert.Null(area.JanelaAtiva);
            // Reabrir depois da limpeza não duplica os manipuladores de fechamento.
            area.Abrir(produtos);Botao(produtos,"Fechar Produtos").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Empty(area.Janelas);
        });
    }

    [Fact]
    public void TodasAsBordasRedimensionamSemEscaparEAJanelaSeAdaptaAoDiminuirArea()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach(var nome in new[]{"esquerda","direita","superior","inferior","superior esquerda","superior direita","inferior esquerda","inferior direita"})
            {
                var area=new AreaTrabalhoInterna();var janela=new JanelaInterna("Teste",new Grid());area.Abrir(janela);
                void Layout(double w,double h){area.Measure(new Size(w,h));area.Arrange(new Rect(0,0,w,h));area.UpdateLayout();}
                Layout(1920,1000);var moldura=Moldura(janela);var antes=moldura.RenderSize;
                Redimensionar(janela,nome,nome.Contains("esquerda")?100:-100,nome.Contains("superior")?100:-100);Layout(1920,1000);
                Assert.NotEqual(antes,moldura.RenderSize);
                Mover(janela,10000,10000);Layout(800,440);
                Assert.InRange(moldura.Margin.Left,0,800-moldura.ActualWidth);
                Assert.InRange(moldura.Margin.Top,0,440-moldura.ActualHeight);
                janela.AlternarMaximizar();Layout(800,440);Assert.Equal(new Size(800,440),moldura.RenderSize);
                Assert.All(Filhos(janela).OfType<Thumb>().Where(t=>Nome(t).StartsWith("Redimensionar")),t=>Assert.Equal(Visibility.Collapsed,t.Visibility));
                janela.AlternarMaximizar();Layout(800,440);
                Assert.All(Filhos(janela).OfType<Thumb>().Where(t=>Nome(t).StartsWith("Redimensionar")),t=>Assert.Equal(Visibility.Visible,t.Visibility));
            }
        });
    }

    internal static Border Moldura(JanelaInterna janela)=>Filhos(janela).OfType<Border>().Single(b=>b.Name=="MolduraInterna");
    internal static void Mover(JanelaInterna janela,double dx,double dy)=>Filhos(janela).OfType<Thumb>().Single(t=>Nome(t).StartsWith("Mover janela")).RaiseEvent(new DragDeltaEventArgs(dx,dy){RoutedEvent=Thumb.DragDeltaEvent});
    internal static void Redimensionar(JanelaInterna janela,string lado,double dx,double dy)=>Filhos(janela).OfType<Thumb>().Single(t=>Nome(t)=="Redimensionar "+lado).RaiseEvent(new DragDeltaEventArgs(dx,dy){RoutedEvent=Thumb.DragDeltaEvent});
    private static Button Botao(JanelaInterna janela,string nome)=>Filhos(janela).OfType<Button>().Single(b=>Nome(b)==nome);
    private static string Nome(DependencyObject objeto)=>System.Windows.Automation.AutomationProperties.GetName(objeto);
    private static IEnumerable<DependencyObject> Filhos(DependencyObject objeto)=>AlinhamentoInterfaceTests.Filhos(objeto);
}
