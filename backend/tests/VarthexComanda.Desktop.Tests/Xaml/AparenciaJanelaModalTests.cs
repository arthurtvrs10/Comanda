using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Ajuda;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class AparenciaJanelaModalTests
{
    [Fact]
    public void CabecalhoPadraoPreservaConteudoTituloDinamicoEModosDeRedimensionamento()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach (var modo in new[] { ResizeMode.NoResize, ResizeMode.CanResize })
            {
                var conteudo = new TextBox { Text = "Conteúdo preservado" };
                var janela = new Window { Title = "Novo produto", Width = 600, Height = 400, Content = conteudo, ResizeMode = modo, Opacity = 0, ShowInTaskbar = false };
                try
                {
                    janela.Show(); janela.UpdateLayout();
                    Assert.Equal(WindowStyle.None, janela.WindowStyle);
                    Assert.Equal(30, WindowChrome.GetWindowChrome(janela).CaptionHeight);
                    Assert.Same(conteudo, janela.Content);
                    var barra = (Grid)janela.Template.FindName("BarraTituloModal", janela);
                    var titulo = (TextBlock)janela.Template.FindName("TituloModal", janela);
                    var maximizar = (Button)janela.Template.FindName("MaximizarModal", janela);
                    Assert.Equal(30, barra.ActualHeight); Assert.Equal("Novo produto", titulo.Text);
                    Assert.Empty(AlinhamentoInterfaceTests.Filhos(barra).OfType<Image>());
                    Assert.Equal(modo == ResizeMode.CanResize ? Visibility.Visible : Visibility.Collapsed, maximizar.Visibility);
                    janela.Title = "Editar produto"; janela.UpdateLayout(); Assert.Equal("Editar produto", titulo.Text);
                    var fechar = (Button)janela.Template.FindName("FecharModal", janela);
                    Assert.True(fechar.Command.CanExecute(null));
                    Assert.InRange(fechar.TranslatePoint(new Point(0, 0), barra).Y, 0, 1);
                    Assert.InRange(fechar.TranslatePoint(new Point(fechar.ActualWidth, 0), barra).X, barra.ActualWidth - 1, barra.ActualWidth);
                    var bloquear = true;
                    janela.Closing += (_, e) => e.Cancel = bloquear;
                    SystemCommands.CloseWindowCommand.Execute(null, janela); Assert.True(janela.IsVisible);
                    bloquear = false; SystemCommands.CloseWindowCommand.Execute(null, janela); Assert.False(janela.IsVisible);
                }
                finally { janela.Close(); }
            }
        });
    }

    [Fact]
    public void CapturasIncluemBarraInteiraEmSenhaERedimensionavelNosDoisTemas()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach (var escuro in new[] { false, true })
            {
                TemaInterface.Aplicar(escuro, false);
                var senha = new JanelaAlterarAcesso(new AcessoGestor("inexistente-teste-senha"), new AcessoAtendimento("inexistente-teste-pin"), false) { Opacity = 0, ShowInTaskbar = false };
                var diagnostico = new JanelaDiagnosticoServicos(() => Array.Empty<VerificacaoServico>()) { Opacity = 0, ShowInTaskbar = false };
                try
                {
                    senha.Show(); diagnostico.Show();
                    Salvar(senha, $"modal-senha-{(escuro ? "escuro" : "claro")}");
                    Salvar(diagnostico, $"modal-redimensionavel-{(escuro ? "escuro" : "claro")}");
                }
                finally { senha.Close(); diagnostico.Close(); }
            }
            TemaInterface.Aplicar(false, false);
        });
    }

    private static void Salvar(Window janela, string nome)
    {
        janela.UpdateLayout();
        var raiz = (FrameworkElement)VisualTreeHelper.GetChild(janela, 0);
        var pasta = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE"); if (string.IsNullOrWhiteSpace(pasta)) return;
        Directory.CreateDirectory(pasta);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(raiz.ActualWidth), (int)Math.Ceiling(raiz.ActualHeight), 96, 96, PixelFormats.Pbgra32); bitmap.Render(raiz);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var arquivo = File.Create(Path.Combine(pasta, nome + ".png")); encoder.Save(arquivo);
    }
}
