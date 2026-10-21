using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Ajuda;

namespace VarthexComanda.Desktop.Tests.Xaml;
public class AjudaXamlTests
{
    [Fact]
    public void SuporteAbreJuntoAoIconeComTamanhoReduzidoEReduzParaCaberNaTela()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();TemaInterface.Aplicar(false,false);
            var icone=new Button { Width=54,Height=54,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Bottom,Margin=new Thickness(16) };
            var area=new Grid();area.Children.Add(icone);
            var sistema=new Window { Left=80,Top=60,Width=1050,Height=700,Opacity=0,ShowInTaskbar=false,Content=area };
            var ajuda=new JanelaSuporte(ancora:icone) { Opacity=0 };
            try
            {
                sistema.Show();ajuda.Owner=sistema;ajuda.Show();
                ajuda.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                void Conferir()
                {
                    sistema.UpdateLayout();ajuda.UpdateLayout();
                    var canto=icone.PointToScreen(new Point(icone.ActualWidth,0));var dpi=VisualTreeHelper.GetDpi(icone);
                    Assert.InRange(Math.Abs((ajuda.Left+ajuda.ActualWidth)*dpi.DpiScaleX-canto.X),0,2);
                    Assert.InRange(Math.Abs((ajuda.Top+ajuda.ActualHeight)*dpi.DpiScaleY-canto.Y+4*dpi.DpiScaleY),0,2);
                    Assert.InRange(Math.Abs(ajuda.ActualHeight-Math.Min(640,canto.Y/dpi.DpiScaleY-sistema.Top-12)),0,2);
                    Assert.InRange(Math.Abs(ajuda.ActualWidth-360),0,2);
                    Assert.Equal(ResizeMode.NoResize,ajuda.ResizeMode);
                }
                Conferir();sistema.Left=140;sistema.Top=90;sistema.Width=900;sistema.Height=620;Conferir();
                sistema.Top=40;sistema.Height=820;Conferir();Assert.InRange(ajuda.ActualHeight,638,642);
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,false);
                    Renderizar((FrameworkElement)ajuda.Content,344,601,$"suporte-modal-icone-{escuro}");
                }
                sistema.Width=300;sistema.Height=440;sistema.UpdateLayout();ajuda.UpdateLayout();
                Assert.True(ajuda.ActualHeight<360);Assert.True(ajuda.ActualWidth<320);
                Assert.True(ajuda.Left>=sistema.Left+7);
                Assert.True(ajuda.Left+ajuda.ActualWidth<=sistema.Left+sistema.ActualWidth-7);
                var topoIcone=icone.PointToScreen(new Point());var escala=VisualTreeHelper.GetDpi(icone).DpiScaleY;
                Assert.True((ajuda.Top+ajuda.ActualHeight)*escala<=topoIcone.Y-3*escala);
                ajuda.Close();sistema.Left=160;sistema.Width=920;
            }
            finally { ajuda.Close();sistema.Close();TemaInterface.Aplicar(false,false); }
        });
    }
    [Fact]
    public void DiagnosticoApresentaResultadosDaVerificacaoSemBloquearAInterface()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var janela=new JanelaDiagnosticoServicos(()=>new VerificacaoServico[] {
                new("Banco de dados","Funcionando","Conexão e integridade verificadas."),
                new("Backups locais","Falha","A pasta não foi encontrada."),
                new("Impressão do Windows","Atenção","Nenhuma fila encontrada."),
                new("Serviços online","Não utilizado","A operação é local.") });
            var tarefa=janela.Executar();var frame=new System.Windows.Threading.DispatcherFrame();
            tarefa.ContinueWith(_=>janela.Dispatcher.BeginInvoke(()=>frame.Continue=false));
            System.Windows.Threading.Dispatcher.PushFrame(frame);tarefa.GetAwaiter().GetResult();
            var raiz=(FrameworkElement)janela.Content;
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);Renderizar(raiz,870,520,$"diagnostico-{escuro}");
                var lista=AlinhamentoInterfaceTests.Filhos(raiz).OfType<ListView>().Single();Assert.Equal(4,lista.Items.Count);
                Assert.Contains(AlinhamentoInterfaceTests.Filhos(raiz).OfType<TextBlock>(),t=>t.Text.Contains("1 falha(s)"));
            }
            janela.Close();TemaInterface.Aplicar(false,false);
        });
    }
    [Fact]
    public void SuporteEFeedbackSoAbremDestinoAoClicarETreinamentoPercorreEtapas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();TemaInterface.Aplicar(false,false);var links=new List<string>();
            var suporte=new JanelaSuporte(links.Add);var raiz=(FrameworkElement)suporte.Content;Renderizar(raiz,470,810,"suporte");Assert.Empty(links);
            Botoes(raiz).Single(b=>Equals(b.Content,"Abrir WhatsApp")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.StartsWith("https://wa.me/5561985532004",Assert.Single(links));
            Botoes(raiz).Single(b=>Equals(b.Content,"Baixar RustDesk")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Equal(CanaisAjuda.RustDesk,links.Last());
            Assert.False(Botoes(raiz).Single(b=>Equals(b.Content,"Em preparação")).IsEnabled);
            var feedback=new JanelaFeedback(links.Add);var formulario=(FrameworkElement)feedback.Content;Renderizar(formulario,560,400,"feedback");var enviar=Botoes(formulario).Single(b=>Equals(b.Content,"Enviar pelo WhatsApp"));var total=links.Count;enviar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Equal(total,links.Count);
            var texto=AlinhamentoInterfaceTests.Filhos(formulario).OfType<TextBox>().Single();texto.Text="Sugestão: mostrar meus produtos favoritos.";enviar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Contains(texto.Text,Uri.UnescapeDataString(links.Last()));
            var categorias=new List<string>();var tutorial=new JanelaTreinamento(categorias.Add);var guia=(FrameworkElement)tutorial.Content;
            for(int i=0;i<JanelaTreinamento.Etapas.Count-1;i++){Renderizar(guia,540,310,"treinamento");Botoes(guia).Single(b=>Equals(b.Content,"Próximo")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));}
            Assert.Equal(JanelaTreinamento.Etapas.Count-1,tutorial.EtapaAtual);Assert.Equal(JanelaTreinamento.Etapas.Select(e=>e.Categoria),categorias);Assert.Contains(Botoes(guia),b=>Equals(b.Content,"Concluir"));
            foreach(var escuro in new[]{false,true}){TemaInterface.Aplicar(escuro,false);Renderizar(raiz,470,810,$"suporte-{escuro}");Renderizar(raiz,380,450,$"suporte-compacto-{escuro}");Renderizar(formulario,560,400,$"feedback-{escuro}");Renderizar(guia,540,310,$"treinamento-{escuro}");}
            suporte.Close();feedback.Close();tutorial.Close();TemaInterface.Aplicar(false,false);
        });
    }
    [Fact]
    public void AjudaAbreFluxosNoCliquePermiteOcultarAtalhoEUsaLogoEmFundosNeutros()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();TemaInterface.Aplicar(false,false);
            var feedback=0;var treinamento=0;var ocultou=false;Window? ownerRecebido=null;
            var janela=new JanelaSuporte(_=>throw new InvalidOperationException(),
                owner=>{ownerRecebido=owner;feedback++;},
                _=>treinamento++,()=>ocultou=true);
            var raiz=(FrameworkElement)janela.Content;Renderizar(raiz,470,810,"ajuda-canais");
            foreach(var nome in new[]{"BotaoWhatsapp","BotaoFecharOcultar"})
                Assert.Same(System.Windows.Application.Current.FindResource("FocoVisivel"),((Button)janela.FindName(nome)).FocusVisualStyle);
            Assert.Equal(0,feedback);Assert.Equal(0,treinamento);Assert.False(ocultou);
            ((Button)janela.FindName("BotaoFeedback")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            ((Button)janela.FindName("BotaoTreinamento")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(1,feedback);Assert.Equal(1,treinamento);Assert.Same(janela,ownerRecebido);
            ((Button)janela.FindName("BotaoWhatsapp")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var mensagem=(TextBlock)janela.FindName("Mensagem");Assert.Equal(Visibility.Visible,mensagem.Visibility);Assert.Contains(CanaisAjuda.Telefone,mensagem.Text);
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);Renderizar(raiz,470,810,"ajuda-canais");
                var logo=AlinhamentoInterfaceTests.Filhos(raiz).OfType<Image>().Single(i=>i.Source is BitmapSource);Assert.True(((BitmapSource)logo.Source).PixelWidth>0);
                foreach(var chave in new[]{"AjudaCanvas","AjudaSurface","AjudaSoft","AjudaInk","AjudaMuted"})
                {
                    var cor=((SolidColorBrush)System.Windows.Application.Current.FindResource(chave)).Color;
                    Assert.Equal(cor.R,cor.G);Assert.Equal(cor.G,cor.B);
                }
            }
            ((Button)janela.FindName("BotaoFecharOcultar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.True(ocultou);
            TemaInterface.Aplicar(false,false);
        });
    }
    private static IEnumerable<Button> Botoes(FrameworkElement e)=>AlinhamentoInterfaceTests.Filhos(e).OfType<Button>();
    private static void Renderizar(FrameworkElement e,int w,int h,string nome)
    {
        e.Measure(new Size(w,h));e.Arrange(new Rect(0,0,w,h));e.UpdateLayout();var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrWhiteSpace(pasta))return;Directory.CreateDirectory(pasta);
        var bitmap=new RenderTargetBitmap(w,h,96,96,PixelFormats.Pbgra32);bitmap.Render(e);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
}
