using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class JanelaAberturaTests
{
    [Fact]
    public void AberturaPreservaArteMarcaVersaoEEnquadramentoSemCabecalhoDeModal()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach (var (largura, altura) in new[] { (1366d, 768d), (640d, 400d) })
            {
                var janela = new JanelaAbertura(new Rect(0, 0, largura, altura), TimeSpan.Zero);
                try
                {
                    janela.Show(); janela.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle); janela.UpdateLayout();
                    Assert.Equal(WindowStyle.None, janela.WindowStyle); Assert.Equal(ResizeMode.NoResize, janela.ResizeMode);
                    Assert.Null(WindowChrome.GetWindowChrome(janela)); Assert.False(janela.ShowInTaskbar);
                    Assert.Equal(VersaoDoAplicativo.Atual, ((System.Windows.Controls.TextBlock)janela.FindName("VersaoAbertura")).Text);
                    var logo = (System.Windows.Controls.Image)janela.FindName("LogoAbertura"); Assert.True(logo.ActualWidth > 0);
                    Assert.InRange(janela.Width, 1, largura - 32); Assert.InRange(janela.Height, 1, altura - 32);
                    Assert.InRange(Math.Abs(janela.Width - janela.Height * 760 / 490), 0, 1);
                    janela.AtualizarEtapa("Preparando o atendimento…"); janela.UpdateLayout();
                    Assert.Equal("Preparando o atendimento…", ((System.Windows.Controls.TextBlock)janela.FindName("EtapaAbertura")).Text);
                    Capturar(janela, $"abertura-{largura}");
                }
                finally { janela.Close(); }
            }
        });
    }

    [Fact]
    public void TempoMinimoEContadoUmaVezEFechamentoInterrompeAEspera()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var janela = new JanelaAbertura(new Rect(0, 0, 1366, 768), TimeSpan.FromMilliseconds(150));
            var desdeRender = new Stopwatch(); janela.ContentRendered += (_, _) => desdeRender.Restart();
            try
            {
                janela.Show(); janela.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                janela.AguardarExibicaoMinimaAsync().GetAwaiter().GetResult();
                Assert.True(desdeRender.ElapsedMilliseconds >= 140); Assert.True(janela.AguardarExibicaoMinimaAsync().IsCompleted);
            }
            finally { janela.Close(); }
            var longa = new JanelaAbertura(new Rect(0, 0, 1366, 768), TimeSpan.FromSeconds(10));
            longa.Show(); var espera = longa.AguardarExibicaoMinimaAsync(); longa.Close();
            Assert.True(espera.Wait(TimeSpan.FromSeconds(1)));
            Assert.False(((System.Windows.Controls.ProgressBar)longa.FindName("AtividadeAbertura")).IsIndeterminate);
        });
    }

    [Fact]
    public void FechamentoNormalDaAberturaNaoCancelaInicializacaoNemTrocaJanelaPrincipal()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp(); var app = (App)System.Windows.Application.Current;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var campo = typeof(App).GetField("_abertura", flags)!;
            var mostrar = typeof(App).GetMethod("MostrarAberturaAsync", flags)!;
            var fechar = typeof(App).GetMethod("FecharAbertura", flags)!;
            var anterior = app.MainWindow; var principal = new Window(); app.MainWindow = principal;
            try
            {
                var exibicao = (Task)mostrar.Invoke(app, null)!;
                var frame = new System.Windows.Threading.DispatcherFrame();
                exibicao.ContinueWith(_ => app.Dispatcher.BeginInvoke(new Action(() => frame.Continue = false)));
                System.Windows.Threading.Dispatcher.PushFrame(frame); exibicao.GetAwaiter().GetResult();
                var abertura = Assert.IsType<JanelaAbertura>(campo.GetValue(app)); Assert.True(abertura.IsVisible);
                fechar.Invoke(app, null);
                Assert.Null(campo.GetValue(app)); Assert.False(abertura.IsVisible); Assert.Same(principal, app.MainWindow);
                Assert.False((bool)typeof(App).GetField("_cancelouInicializacao", flags)!.GetValue(app)!);
            }
            finally { fechar.Invoke(app, null); app.MainWindow = anterior; principal.Close(); }
        });
    }

    private static void Capturar(Window janela, string nome)
    {
        var pasta = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE"); if (string.IsNullOrWhiteSpace(pasta)) return;
        Directory.CreateDirectory(pasta);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(janela.ActualWidth), (int)Math.Ceiling(janela.ActualHeight), 96, 96, PixelFormats.Pbgra32); bitmap.Render(janela);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using var arquivo = File.Create(Path.Combine(pasta, nome + ".png")); png.Save(arquivo);
    }
}
