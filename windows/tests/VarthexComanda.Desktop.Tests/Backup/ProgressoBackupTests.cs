using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Backup;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Tests.Backup;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Desktop.Tests.Xaml;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Backup;

public class ProgressoBackupTests
{
    [Theory]
    [InlineData("sucesso", false)]
    [InlineData("sucesso", true)]
    [InlineData("aviso", false)]
    [InlineData("falha", false)]
    public async Task PaginaMostraEtapasReaisImpedeDuplicacaoEPermiteVoltarDepois(string estado, bool escuro)
    {
        using var servico = new BackupControlado(estado);
        BackupViewModel vm = null!; JanelaProgressoBackup janela = null!; Task tarefa = Task.CompletedTask;
        try
        {
            ThreadingHelper.EmSta(() =>
            {
                CarregamentoDeXamlTests.GarantirApp(); TemaInterface.Aplicar(escuro, false);
                vm = new(new CriarBackupManual(servico), new RestaurarBackup(servico),
                    new ListarBackupsRecentes(new FakeBackupRegistroRepository()), new FakeConfirmadorDeBackup());
                janela = new(vm) { ShowInTaskbar=false }; janela.Show();
            });
            Assert.True(await Task.Run(() => servico.Pronto.Wait(TimeSpan.FromSeconds(10))));
            ThreadingHelper.EmSta(() =>
            {
                TemaInterface.Aplicar(escuro, false);
                Assert.True(vm.ExibindoProgresso); Assert.True(vm.EmAndamento);
                tarefa = janela.Operacao;
                Assert.Equal(2, vm.Progresso.Count); Assert.False(vm.PodeInteragir);
                vm.VoltarConfiguracao(); Assert.True(vm.ExibindoProgresso);
                Assert.True(vm.CriarBackupAsync().IsCompletedSuccessfully); Assert.Equal(1, servico.Chamadas);
                janela.Close(); Assert.True(janela.IsVisible);
                Assert.True(((ProgressBar)janela.Progresso.FindName("BarraBackup")).IsIndeterminate);
                Renderizar(janela, $"progresso-{estado}-{escuro}-andamento", estado == "sucesso");
            });
            servico.Liberar.Set(); await tarefa.WaitAsync(TimeSpan.FromSeconds(10));
            ThreadingHelper.EmSta(() =>
            {
                TemaInterface.Aplicar(escuro, false);
                Assert.False(vm.EmAndamento); Assert.True(vm.ExibindoProgresso); Assert.Equal(estado, vm.EstadoProgresso);
                Assert.Contains(vm.Progresso, p => p.Texto.Contains("Etapa final executada"));
                Assert.Equal(estado != "sucesso", vm.MensagemErro);
                Assert.False(((ProgressBar)janela.Progresso.FindName("BarraBackup")).IsIndeterminate);
                Assert.Equal(estado=="falha"?0:100,vm.ValorBarraProgresso);
                Renderizar(janela, $"progresso-{estado}-{escuro}-concluido", true);
                janela.Width=460;janela.Height=340;janela.UpdateLayout();
                Renderizar(janela, $"progresso-{estado}-{escuro}-compacto", true);
                janela.Close(); Assert.False(vm.ExibindoProgresso);
            });
        }
        finally
        {
            servico.Liberar.Set();
            ThreadingHelper.EmSta(() => TemaInterface.Aplicar(false, false));
        }
    }

    private static void Renderizar(FrameworkElement view, string nome, bool salvar)
    {
        view.UpdateLayout();
        var pasta = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
        if (!salvar || string.IsNullOrWhiteSpace(pasta)) return;
        Directory.CreateDirectory(pasta);
        var bitmap = new RenderTargetBitmap((int)view.ActualWidth, (int)view.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(view);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var arquivo = File.Create(Path.Combine(pasta, nome + ".png")); encoder.Save(arquivo);
    }

    private sealed class BackupControlado(string estado) : IBackupService, IDisposable
    {
        public ManualResetEventSlim Pronto { get; } = new(false);
        public ManualResetEventSlim Liberar { get; } = new(false);
        public int Chamadas { get; private set; }
        public Resultado<BackupRegistro> CriarBackupGerenciado() => CriarBackupGerenciado(null);
        public Resultado<BackupRegistro> CriarBackupGerenciado(IProgress<EtapaBackup>? progresso)
        {
            Chamadas++;
            progresso?.Report(new(DateTime.UtcNow, "Preparando backup local…"));
            progresso?.Report(new(DateTime.UtcNow, "Verificando os dados e preparando as fotos…"));
            Pronto.Set(); if (!Liberar.Wait(TimeSpan.FromSeconds(15))) throw new TimeoutException();
            progresso?.Report(new(DateTime.UtcNow, "Etapa final executada."));
            return estado == "falha" ? Resultado<BackupRegistro>.Falha("Sem acesso à pasta de destino de teste. Verifique as permissões e tente novamente.") :
                Resultado<BackupRegistro>.Ok(new() { Id = 0, Arquivo = "backup-exemplo.db", Destino = @"C:\Backups de exemplo", CriadoEm = DateTime.UtcNow, Status = StatusBackup.Sucesso,
                    Mensagem = estado == "aviso" ? "Backup local preservado. A pasta do OneDrive está indisponível; verifique o aplicativo e tente novamente." : null });
        }
        public Resultado<BackupRegistro> CriarBackupExterno(string pasta) => throw new NotSupportedException();
        public RelatorioValidacao Validar(string caminho) => throw new NotSupportedException();
        public Resultado<BackupRegistro> RestaurarPara(string caminho) => throw new NotSupportedException();
        public void Dispose() { Liberar.Dispose(); Pronto.Dispose(); }
    }
}
