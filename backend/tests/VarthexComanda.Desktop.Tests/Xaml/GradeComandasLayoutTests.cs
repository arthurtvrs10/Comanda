using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Atendimento;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class GradeComandasLayoutTests
{
    [Fact]
    public void GradeDistribuiCartoesAteALateralERolaAteAUltimaComanda()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (vm,_)=CarregamentoDeXamlTests.CriarAtendimento();vm.FecharEdicaoCommand.Execute(null);
            var view=new AtendimentoView(vm);
            var rolagem=(ScrollViewer)view.FindName("RolagemComandas");
            foreach(var escuro in new[]{false,true})
            foreach(var largura in new[]{462d,800d,1180d,1920d})
            {
                TemaInterface.Aplicar(escuro,false);
                view.Measure(new Size(largura,350));view.Arrange(new Rect(0,0,largura,350));view.UpdateLayout();
                rolagem.ScrollToTop();Atualizar(view);
                var cartoes=AlinhamentoInterfaceTests.Filhos(view).OfType<Button>().Where(b=>b.CommandParameter is ComandaSlotItem).ToArray();
                Assert.Equal(vm.Slots.Count,cartoes.Length);
                var topo=cartoes[0].TranslatePoint(new Point(),rolagem).Y;
                var primeiraLinha=cartoes.Where(b=>Math.Abs(b.TranslatePoint(new Point(),rolagem).Y-topo)<1).ToArray();
                Assert.True(primeiraLinha.Length>=3);
                // O último cartão da linha deve terminar junto à barra, com só seu espaço de 4 DIP.
                var ultimo=primeiraLinha[^1];
                var direita=ultimo.TranslatePoint(new Point(ultimo.ActualWidth,0),rolagem).X;
                Assert.InRange(rolagem.ViewportWidth-direita,3,5);
                Assert.Equal(ScrollBarVisibility.Visible,rolagem.VerticalScrollBarVisibility);
                Assert.Equal(0,rolagem.ScrollableWidth);
                Assert.True(rolagem.ScrollableHeight>0);
                Assert.Equal(Visibility.Collapsed,((TextBlock)view.FindName("MensagemGrade")).Visibility);
                SalvarPrevia(view,$"grade-comandas-{(escuro?"escuro":"claro")}-{largura}");
                rolagem.ScrollToBottom();Atualizar(view);
                Assert.Equal(rolagem.ScrollableHeight,rolagem.VerticalOffset);
                var fim=cartoes[^1].TranslatePoint(new Point(0,cartoes[^1].ActualHeight),rolagem).Y;
                Assert.InRange(fim,0,rolagem.ViewportHeight);
                Assert.Same(vm.AbrirOuSelecionarSlotCommand,cartoes[^1].Command);
                Assert.Equal(vm.Slots[^1],cartoes[^1].CommandParameter);
            }
            vm.Mensagem="Confira o número da comanda.";Atualizar(view);
            Assert.Equal(Visibility.Visible,((TextBlock)view.FindName("MensagemGrade")).Visibility);
            Assert.Equal(vm.Mensagem,((TextBlock)view.FindName("MensagemGrade")).Text);
            TemaInterface.Aplicar(false,false);
        });
    }

    [Fact]
    public void JanelaPequenaMantemARolagemNaGradeSemRolarATelaInteira()
    {
        var pasta=Path.Combine(Path.GetTempPath(),"grade-rolagem-"+Guid.NewGuid());
        try
        {
            ThreadingHelper.EmSta(()=>
            {
                CarregamentoDeXamlTests.GarantirApp();
                var janela=CarregamentoDeXamlTests.CriarMainWindow(new VarthexComanda.Infrastructure.Storage.AppPaths(pasta));
                var flags=BindingFlags.Instance|BindingFlags.NonPublic;
                var tipo=typeof(MainWindow);
                var view=(AtendimentoView)tipo.GetField("_atendimentoView",flags)!.GetValue(janela)!;
                view.ViewModel.FecharEdicaoCommand.Execute(null);
                tipo.GetField("_bloqueado",flags)!.SetValue(janela,false);
                ((FrameworkElement)janela.FindName("CamadaBloqueio")).Visibility=Visibility.Collapsed;
                ((FrameworkElement)janela.FindName("AreaSistema")).IsEnabled=true;
                tipo.GetMethod("AtualizarPerfil",flags)!.Invoke(janela,null);
                MainWindow.IrParaAtendimentoCommand.Execute(null,janela);
                var area=(AreaTrabalhoInterna)janela.FindName("ConteudoPrincipal");
                var interna=Assert.IsType<JanelaInterna>(area.JanelaAtiva);
                var raiz=(FrameworkElement)janela.Content;
                raiz.Measure(new Size(1280,780));raiz.Arrange(new Rect(0,0,1280,780));raiz.UpdateLayout();
                AreaTrabalhoInternaTests.Redimensionar(interna,"inferior direita",-10000,-10000);Atualizar(raiz);
                var externa=AlinhamentoInterfaceTests.Filhos(interna).OfType<ScrollViewer>().Single(s=>s.Name=="ViewportJanelaInterna");
                var grade=(ScrollViewer)view.FindName("RolagemComandas");
                Assert.Equal(0,externa.ScrollableWidth);Assert.Equal(0,externa.ScrollableHeight);
                Assert.True(grade.ScrollableHeight>0);Assert.True(grade.ActualHeight>0);
                Assert.True(grade.TranslatePoint(new Point(grade.ActualWidth,grade.ActualHeight),interna).Y<=AreaTrabalhoInternaTests.Moldura(interna).Margin.Top+AreaTrabalhoInternaTests.Moldura(interna).ActualHeight);
                SalvarPrevia(raiz,"grade-comandas-janela-pequena");
                // Ao editar, o formulário volta a ter rolagem externa para preservar seus campos.
                view.ViewModel.AbrirOuSelecionarSlotCommand.Execute(view.ViewModel.Slots.First());Atualizar(raiz);
                Assert.True(externa.ScrollableHeight>0);
                area.Limpar();
            });
        }
        finally { if(Directory.Exists(pasta))Directory.Delete(pasta,true); }
    }

    private static void Atualizar(FrameworkElement elemento)
    {
        elemento.UpdateLayout();
    }
    private static void SalvarPrevia(FrameworkElement elemento,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrWhiteSpace(pasta))return;
        Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap((int)elemento.ActualWidth,(int)elemento.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(elemento);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
}
