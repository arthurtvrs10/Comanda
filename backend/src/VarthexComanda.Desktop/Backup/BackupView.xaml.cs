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
        ViewModel.Progresso.CollectionChanged += (_, _) => Dispatcher.BeginInvoke(new Action(() => RolagemProgresso.ScrollToEnd()));
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(BackupViewModel.EmAndamento)) return;
            if (ViewModel.EmAndamento && SystemParameters.ClientAreaAnimation)
                PontosProgresso.BeginAnimation(OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.35, 1, TimeSpan.FromSeconds(0.65)) { AutoReverse = true, RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever });
            else PontosProgresso.BeginAnimation(OpacityProperty, null);
        };
    }

    private void VoltarProgresso_Click(object sender, RoutedEventArgs e) => ViewModel.VoltarConfiguracao();

    private async void CriarBackup_Click(object sender, RoutedEventArgs e) => await ViewModel.CriarBackupAsync();

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
