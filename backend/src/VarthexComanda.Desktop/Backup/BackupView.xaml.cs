using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace VarthexComanda.Desktop.Backup;

public partial class BackupView : UserControl
{
    public BackupViewModel ViewModel { get; }

    public BackupView(BackupViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        IconePasta.Content = IconesVarthex.Criar(IconeVarthex.Pasta, 26);
        IconeDrive.Content = IconesVarthex.Criar(IconeVarthex.GoogleDrive, 36);
        IconeOneDrive.Content = IconesVarthex.Criar(IconeVarthex.OneDrive, 36);
        IconeDados.Content = IconesVarthex.Criar(IconeVarthex.Backup, 24);
        IconeFotos.Content = IconesVarthex.Criar(IconeVarthex.Fotos, 24);
        IconeAlertaRestauracao.Content = IconesVarthex.Criar(IconeVarthex.Alerta, 24);
        ViewModel.SolicitouReinicio += ViewModel_SolicitouReinicio;
    }

    public Task CriarBackupComProgressoAsync(string? pastaExterna = null)
    {
        if (!ViewModel.PodeCriarBackup) return Task.CompletedTask;
        new JanelaProgressoBackup(ViewModel, pastaExterna) { Owner=Window.GetWindow(this) }.ShowDialog();
        return Task.CompletedTask;
    }

    private async void CriarBackup_Click(object sender, RoutedEventArgs e) => await CriarBackupComProgressoAsync();
    public void AbrirConexaoGoogle()
    {
        if (!ViewModel.PodeCriarBackup || ViewModel.GoogleDrive is not { } conexao) return;
        new ConectarGoogleDriveView(conexao) { Owner=Window.GetWindow(this) }.ShowDialog();
        ViewModel.AtualizarGoogleDrive();
    }
    private void ConectarGoogle_Click(object sender,RoutedEventArgs e) => AbrirConexaoGoogle();
    private async void DesconectarGoogle_Click(object sender,RoutedEventArgs e)
    {
        if (!ViewModel.PodeCriarBackup || ViewModel.GoogleDrive is not { } conexao) return;
        if (!new Atendimento.JanelaConfirmador().Confirmar("Desconectar Google Drive", "Os próximos backups continuarão salvos neste computador. As cópias já enviadas ao Google serão preservadas. Deseja desconectar?")) return;
        ViewModel.EmAndamento=true;
        using var limite=new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try { await conexao.DesconectarAsync(limite.Token);ViewModel.EmAndamento=false;ViewModel.ConfigurarPasta("drive",null);ViewModel.Mensagem="Conta Google desconectada."; }
        catch { ViewModel.Mensagem="Não foi possível desconectar. Verifique a internet e tente novamente."; }
        finally { ViewModel.EmAndamento=false;ViewModel.AtualizarGoogleDrive(); }
    }
    private void ConfigurarPasta_Click(object sender, RoutedEventArgs e)
    {
        var destino = (string)((Button)sender).Tag;
        var nome = destino == "drive" ? "Google Drive" : destino == "onedrive" ? "OneDrive" : "backups locais";
        var dialogo = new OpenFolderDialog { Title = "Escolha a pasta de " + nome };
        if (dialogo.ShowDialog(Window.GetWindow(this)) == true)
        {
            ViewModel.ConfigurarPasta(destino, dialogo.FolderName);
        }
    }

    private void DesconectarPasta_Click(object sender, RoutedEventArgs e) =>
        ViewModel.ConfigurarPasta((string)((Button)sender).Tag, null);

    private async void RestaurarFotos_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog { Title = "Selecionar backup de fotos do Varthex", Filter = "Backup de fotos (*.fotos.zip)|*.fotos.zip" };
        if (dialogo.ShowDialog(Window.GetWindow(this)) == true)
            await ViewModel.RestaurarFotosAsync(dialogo.FileName);
    }

    private void SelecionarArquivo_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Selecionar arquivo de backup",
            Filter = "Banco de dados (*.db)|*.db|Todos os arquivos (*.*)|*.*"
        };
        if (dialogo.ShowDialog(Window.GetWindow(this)) == true)
        {
            ViewModel.RestaurarArquivoExterno(dialogo.FileName);
        }
    }

    private void ViewModel_SolicitouReinicio(object? sender, EventArgs e)
    {
        JanelaAviso.Mostrar(
            "Varthex Comanda",
            "Backup restaurado com sucesso. O Varthex Comanda vai reiniciar agora.",
            TipoAviso.Informacao);

        ((App)System.Windows.Application.Current).ReiniciarAplicativo();
    }
}
