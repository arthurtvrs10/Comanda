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
        IconeDropbox.Content = IconesVarthex.Criar(IconeVarthex.Dropbox, 36);
        IconeDados.Content = IconesVarthex.Criar(IconeVarthex.Backup, 24);
        IconeFotos.Content = IconesVarthex.Criar(IconeVarthex.Fotos, 24);
        ViewModel.SolicitouReinicio += ViewModel_SolicitouReinicio;
        ViewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(BackupViewModel.ModoRestauracao)) AtualizarRecuperacao(); };
        AtualizarRecuperacao();
    }

    private void AtualizarRecuperacao()
    {
        CorpoBackup.Children.Remove(HistoricoBackup);
        if (ViewModel.ModoRestauracao) CorpoBackup.Children.Insert(0, HistoricoBackup);
        else CorpoBackup.Children.Add(HistoricoBackup);
        HistoricoBackup.IsExpanded = ViewModel.ModoRestauracao;
        HistoricoBackup.Margin = new Thickness(0, 0, 0, ViewModel.ModoRestauracao ? 20 : 0);
    }

    private async void CriarBackup_Click(object sender, RoutedEventArgs e) => await ViewModel.CriarBackupAsync();

    private void ConfigurarPasta_Click(object sender, RoutedEventArgs e)
    {
        var destino = (string)((Button)sender).Tag;
        var nome = destino == "drive" ? "Google Drive" : destino == "dropbox" ? "Dropbox" : "backups locais";
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
