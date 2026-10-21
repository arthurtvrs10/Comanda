using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Backup;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Tests.Backup;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Desktop.Tests.Backup;
using VarthexComanda.Infrastructure.Backup;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class BackupLayoutTests
{
    [Fact]
    public void PastasPersistemSeparadamenteDesconectamSemApagarArquivosERecuperacaoLeDisco()
    {
        var raiz = Path.Combine(Path.GetTempPath(), "varthex-pastas-test-" + Guid.NewGuid());
        try
        {
            var paths = new AppPaths(raiz); paths.EnsureCreated();
            var drive = Directory.CreateDirectory(Path.Combine(raiz, "Drive")).FullName;
            var dropbox = Directory.CreateDirectory(Path.Combine(raiz, "Dropbox")).FullName;
            var local = Directory.CreateDirectory(Path.Combine(raiz, "Local")).FullName;
            var vm = Criar(paths);
            vm.ConfigurarPasta("local", local); vm.ConfigurarPasta("drive", drive); vm.ConfigurarPasta("dropbox", dropbox);
            var reaberto = Criar(paths);
            Assert.Equal(local, reaberto.PastaLocal); Assert.Equal(drive, reaberto.PastaGoogleDrive); Assert.Equal(dropbox, reaberto.PastaDropbox);
            var destino = Directory.CreateDirectory(PreferenciasBackup.DestinoSincronizado(drive)).FullName;
            var arquivo = Path.Combine(destino, "varthex-comanda-2026-10-21-120000.db");
            File.WriteAllText(arquivo, "backup");
            reaberto.ModoRestauracao = true; reaberto.AtualizarLista();
            Assert.Equal(destino, Assert.Single(reaberto.Backups).Destino);
            reaberto.ModoRestauracao = false; reaberto.ConfigurarPasta("drive", null);
            Assert.True(File.Exists(arquivo)); Assert.False(reaberto.GoogleDriveConfigurado); Assert.True(reaberto.DropboxConfigurado);
            reaberto.ConfigurarPasta("drive", Path.Combine(raiz, "ausente"));
            Assert.True(reaberto.MensagemErro); Assert.False(reaberto.GoogleDriveConfigurado);
        }
        finally { if (Directory.Exists(raiz)) Directory.Delete(raiz, true); }
    }

    [Fact]
    public void TelaDeBackupMantemAcoesAcessiveisNosDoisTemasETamanhos()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach (var escuro in new[] { false, true })
            foreach (var largura in new[] { 760d, 1100d })
            {
                TemaInterface.Aplicar(escuro, false);
                var vm = Criar();
                var view = new BackupView(vm);
                Renderizar(view, largura, largura == 760 ? 440 : 630, $"backup-{(escuro ? "escuro" : "claro")}-{largura}");
                var botoes = AlinhamentoInterfaceTests.Filhos(view).OfType<Button>().ToArray();
                var criar = Assert.Single(botoes, b => Equals(b.Content, "Fazer backup agora"));
                Assert.True(criar.ActualWidth > 100); Assert.True(criar.IsEnabled);
                Assert.InRange(criar.TranslatePoint(new Point(0, criar.ActualHeight), view).Y, 0, view.ActualHeight);
                var rolagem = (ScrollViewer)view.FindName("RolagemBackup");
                Assert.True(rolagem.ViewportWidth > 690);
                Assert.InRange(rolagem.ScrollableWidth, 0, 10);
                vm.ModoRestauracao = true;
                ((Expander)view.FindName("HistoricoBackup")).IsExpanded = true;
                vm.Mensagem = "Nenhum backup encontrado. Use Restaurar dados para selecionar um arquivo.";
                Renderizar(view, largura, 440, $"backup-recuperacao-{(escuro ? "escuro" : "claro")}-{largura}");
                Assert.False(criar.IsEnabled);
                var restaurarRodape = Assert.Single(botoes, b => Equals(b.Content, "Restaurar dados"));
                Assert.True(restaurarRodape.IsEnabled); Assert.Equal(Visibility.Visible, restaurarRodape.Visibility);
                Assert.InRange(restaurarRodape.TranslatePoint(new Point(0, restaurarRodape.ActualHeight), view).Y, 0, view.ActualHeight);
                Assert.Contains(botoes, b => b.IsEnabled && b.Content is StackPanel p && p.Children.OfType<TextBlock>().Any(t => t.Text == "Restaurar dados"));
            }
            TemaInterface.Aplicar(false, false);
        });
    }

    private static BackupViewModel Criar(AppPaths? paths = null)
    {
        var servico = new FakeBackupService();
        return new(new CriarBackupManual(servico), new RestaurarBackup(servico),
            new ListarBackupsRecentes(new FakeBackupRegistroRepository()), new FakeConfirmadorDeBackup(), paths: paths, fotos: new FotosFake());
    }

    private sealed class FotosFake : IBackupFotosService
    {
        public Resultado<FotosRestauradas> RestaurarFotos(string arquivoZip) => Resultado<FotosRestauradas>.Ok(new(1));
    }

    private static void Renderizar(FrameworkElement view, double largura, double altura, string nome)
    {
        view.Measure(new Size(largura, altura)); view.Arrange(new Rect(0, 0, largura, altura)); view.UpdateLayout();
        var pasta = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
        if (string.IsNullOrWhiteSpace(pasta)) return;
        Directory.CreateDirectory(pasta);
        var imagem = new RenderTargetBitmap((int)largura, (int)altura, 96, 96, PixelFormats.Pbgra32); imagem.Render(view);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(imagem));
        using var arquivo = File.Create(Path.Combine(pasta, nome + ".png")); encoder.Save(arquivo);
    }
}
