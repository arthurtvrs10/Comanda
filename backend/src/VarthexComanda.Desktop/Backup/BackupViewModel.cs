using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using VarthexComanda.Application.Backup;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Storage;
using VarthexComanda.Infrastructure.Backup;

namespace VarthexComanda.Desktop.Backup;

public partial class BackupViewModel : ObservableObject
{
    private readonly CriarBackupManual _criarBackupManual;
    private readonly RestaurarBackup _restaurarBackup;
    private readonly ListarBackupsRecentes _listarBackupsRecentes;
    private readonly IConfirmador _confirmador;
    private readonly ILogger? _logger;
    private readonly AppPaths? _paths;
    private readonly PreferenciasBackup? _preferencias;
    private readonly IBackupFotosService? _fotos;
    private PastasBackup _pastas = new();

    public event EventHandler? SolicitouReinicio;

    public BackupViewModel(
        CriarBackupManual criarBackupManual,
        RestaurarBackup restaurarBackup,
        ListarBackupsRecentes listarBackupsRecentes,
        IConfirmador confirmador,
        ILogger? logger = null,
        AppPaths? paths = null,
        IBackupFotosService? fotos = null)
    {
        _criarBackupManual = criarBackupManual;
        _restaurarBackup = restaurarBackup;
        _listarBackupsRecentes = listarBackupsRecentes;
        _confirmador = confirmador;
        _logger = logger;
        _paths = paths;
        _fotos = fotos;
        _preferencias = paths is null ? null : new PreferenciasBackup(paths);
        CarregarPastas();

        Backups = new ObservableCollection<BackupRegistro>();
        AtualizarLista();
    }

    public ObservableCollection<BackupRegistro> Backups { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestaurarCommand))]
    private BackupRegistro? backupSelecionado;

    [ObservableProperty]
    private string mensagem = string.Empty;
    [ObservableProperty]
    private bool mensagemErro;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CriarBackupCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestaurarCommand))]
    [NotifyPropertyChangedFor(nameof(PodeCriarBackup))]
    [NotifyPropertyChangedFor(nameof(PodeInteragir))]
    [NotifyPropertyChangedFor(nameof(PodeRestaurarFotos))]
    private bool emAndamento;

    public bool PodeInteragir => !EmAndamento;
    public bool PodeRestaurarFotos => !EmAndamento && _fotos is not null;
    public string PastaLocal => _pastas.Local ?? _paths?.BackupsDirectory ?? "Pasta padrão do Varthex";
    public string PastaGoogleDrive => _pastas.GoogleDrive ?? "Nenhuma pasta configurada";
    public string PastaDropbox => _pastas.Dropbox ?? "Nenhuma pasta configurada";
    public bool GoogleDriveConfigurado => !string.IsNullOrWhiteSpace(_pastas.GoogleDrive);
    public bool DropboxConfigurado => !string.IsNullOrWhiteSpace(_pastas.Dropbox);
    public string StatusGoogleDrive => StatusPasta(_pastas.GoogleDrive);
    public string StatusDropbox => StatusPasta(_pastas.Dropbox);
    public string ResumoPastas => "Backups automáticos ao abrir o sistema (uma vez ao dia) e ao encerrar. As configurações são salvas neste computador.";

    private static string StatusPasta(string? pasta)
    {
        if (string.IsNullOrWhiteSpace(pasta)) return "Selecione uma pasta sincronizada pelo aplicativo.";
        if (!Directory.Exists(pasta)) return "Pasta indisponível. Abra o aplicativo ou conecte a unidade.";
        var destino = PreferenciasBackup.DestinoSincronizado(pasta);
        try
        {
            var arquivo = Directory.Exists(destino) ? Directory.GetFiles(destino, "varthex-comanda-*.db").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault() : null;
            return arquivo is null ? "Configurado. A próxima cópia será salva em Varthex\\Backups." :
                $"Última cópia na pasta: {File.GetLastWriteTime(arquivo):dd/MM/yyyy HH:mm}.";
        }
        catch { return "Não foi possível consultar a pasta. Verifique o acesso."; }
    }

    private void CarregarPastas()
    {
        try { _pastas = _preferencias?.Ler() ?? new(); }
        catch { MensagemErro = true; Mensagem = "Não foi possível ler as pastas de backup. Configure as pastas novamente."; }
        foreach (var propriedade in new[] { nameof(PastaLocal), nameof(PastaGoogleDrive), nameof(PastaDropbox), nameof(GoogleDriveConfigurado), nameof(DropboxConfigurado), nameof(StatusGoogleDrive), nameof(StatusDropbox) })
            OnPropertyChanged(propriedade);
    }

    public void ConfigurarPasta(string destino, string? pasta)
    {
        if (!PodeCriarBackup || _preferencias is null) return;
        try
        {
            var validada = pasta is null ? null : PreferenciasBackup.ValidarPasta(pasta);
            var nova = destino switch
            {
                "local" => _pastas with { Local = validada },
                "drive" => _pastas with { GoogleDrive = validada },
                "dropbox" => _pastas with { Dropbox = validada },
                _ => throw new ArgumentException("Destino desconhecido.")
            };
            if (validada is not null && _paths is not null && string.Equals(validada, _paths.DataDirectory, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Escolha uma pasta diferente da pasta do banco de dados ativo.");
            _preferencias.Salvar(nova);
            CarregarPastas();
            MensagemErro = false;
            Mensagem = "Configuração salva. Os backups existentes continuam nas pastas anteriores.";
        }
        catch (Exception ex) { MensagemErro = true; Mensagem = "Não foi possível salvar a pasta: " + ex.Message; }
    }

    /// <summary>
    /// Modo de restauração (banco corrompido): não cria backups e a lista vem dos arquivos da
    /// pasta de backups, já que o registro de backups vive no banco corrompido.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CriarBackupCommand))]
    [NotifyPropertyChangedFor(nameof(PodeCriarBackup))]
    private bool modoRestauracao;

    public bool PodeCriarBackup => !ModoRestauracao && !EmAndamento;

    public void AtualizarLista()
    {
        Backups.Clear();
        if (ModoRestauracao)
        {
            ListarBackupsDoDisco();
            return;
        }

        try
        {
            foreach (var registro in _listarBackupsRecentes.Executar(20))
            {
                Backups.Add(registro);
            }
        }
        catch (Exception ex)
        {
            // o registro de backups vive no próprio banco; se ele estiver corrompido a lista
            // não carrega, mas a restauração por "Selecionar arquivo..." continua possível
            _logger?.Error(ex, "Falha em {Operacao}", "ListarBackups");
            Backups.Clear();
            Mensagem = "Não foi possível listar os backups registrados. Use \"Selecionar arquivo...\" para escolher um backup.";
        }
    }

    private void ListarBackupsDoDisco()
    {
        Mensagem = string.Empty;
        try
        {
            var pastas = new[] { _paths?.BackupsDirectory, _pastas.Local,
                _pastas.GoogleDrive is null ? null : PreferenciasBackup.DestinoSincronizado(_pastas.GoogleDrive),
                _pastas.Dropbox is null ? null : PreferenciasBackup.DestinoSincronizado(_pastas.Dropbox) };
            foreach (var pasta in pastas.Where(p => p is not null && Directory.Exists(p)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                // o nome carrega data e hora (yyyy-MM-dd-HHmmss), então a ordem decrescente do
                // nome é a mais recente primeiro; corrompido-*.db.bak nunca casa com este padrão
                var arquivos = Directory.GetFiles(pasta!, "varthex-comanda-*.db")
                    .Where(f => f.EndsWith(".db", StringComparison.OrdinalIgnoreCase)) // nunca o .db.fotos.zip
                    .OrderByDescending(Path.GetFileName, StringComparer.Ordinal);
                foreach (var caminho in arquivos)
                {
                    var checksum = caminho + ".sha256";
                    Backups.Add(new BackupRegistro
                    {
                        Id = 0,
                        Arquivo = Path.GetFileName(caminho),
                        Destino = pasta!,
                        CriadoEm = File.GetLastWriteTimeUtc(caminho),
                        Status = StatusBackup.Sucesso,
                        Checksum = File.Exists(checksum) ? File.ReadAllText(checksum).Trim() : null,
                        Mensagem = null
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "ListarBackupsDoDisco");
            Backups.Clear();
        }

        if (Backups.Count == 0)
        {
            Mensagem = "Nenhum backup encontrado na pasta de backups. Use \"Selecionar arquivo...\" para escolher um backup.";
        }
    }

    [RelayCommand(CanExecute = nameof(PodeCriarBackup))]
    private void CriarBackup(string? pastaExterna)
    {
        if (!PodeCriarBackup)
        {
            return;
        }

        try
        {
            var resultado = _criarBackupManual.Executar(pastaExterna);
            ExibirResultado(resultado);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "CriarBackup");
            MensagemErro = true;
            Mensagem = "Não foi possível criar o backup. Tente novamente.";
        }
    }

    private void ExibirResultado(VarthexComanda.Application.Catalogo.Resultado<BackupRegistro> resultado)
    {
        AtualizarLista();
        MensagemErro = !resultado.Sucesso || !string.IsNullOrEmpty(resultado.Valor?.Mensagem);
        Mensagem = resultado.Sucesso
            ? (string.IsNullOrEmpty(resultado.Valor?.Mensagem)
                ? $"Backup criado com sucesso. Pasta local: {resultado.Valor!.Destino}." +
                    (GoogleDriveConfigurado || DropboxConfigurado ? " Cópias gravadas nas pastas configuradas." : string.Empty)
                : resultado.Valor!.Mensagem!)
            : "Não foi possível criar o backup: " + string.Join(" ", resultado.Erros);
        CarregarPastas();
    }

    public async Task CriarBackupAsync()
    {
        if (!PodeCriarBackup) return;
        EmAndamento = true;
        MensagemErro = false;
        Mensagem = "Criando backup e copiando para as pastas configuradas… Aguarde.";
        try { ExibirResultado(await Task.Run(() => _criarBackupManual.Executar(null))); }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha ao criar backup");
            MensagemErro = true; Mensagem = "Não foi possível criar o backup. Verifique o espaço e o acesso às pastas e tente novamente.";
        }
        finally { EmAndamento = false; }
    }

    public async Task RestaurarFotosAsync(string arquivo)
    {
        if (EmAndamento || _fotos is null) return;
        if (!_confirmador.Confirmar("Restaurar fotos", "As fotos com o mesmo nome serão substituídas. Os dados dos pedidos e produtos serão preservados. Uma cópia das fotos atuais será guardada antes. Deseja continuar?")) return;
        EmAndamento = true;
        Mensagem = "Validando e restaurando fotos… Aguarde."; MensagemErro = false;
        try
        {
            var resultado = await Task.Run(() => _fotos.RestaurarFotos(arquivo));
            MensagemErro = !resultado.Sucesso;
            Mensagem = resultado.Sucesso ? $"{resultado.Valor!.Quantidade} foto(s) restaurada(s) com sucesso." : string.Join(" ", resultado.Erros);
        }
        catch { MensagemErro = true; Mensagem = "Não foi possível restaurar as fotos. Verifique o arquivo selecionado."; }
        finally { EmAndamento = false; }
    }

    [RelayCommand(CanExecute = nameof(PodeRestaurar))]
    private void Restaurar()
    {
        if (BackupSelecionado is null)
        {
            return;
        }

        var caminho = Path.Combine(BackupSelecionado.Destino, BackupSelecionado.Arquivo);
        RestaurarCaminho(caminho, BackupSelecionado.CriadoEm);
    }

    public void RestaurarArquivoExterno(string caminho)
    {
        if (EmAndamento) return;
        RestaurarCaminho(caminho, null);
    }

    private void RestaurarCaminho(string caminho, DateTime? criadoEm)
    {
        if (EmAndamento) return;
        var mensagemConfirmacao = criadoEm is not null
            ? $"Isso vai substituir todos os dados atuais pelo backup de {criadoEm:dd/MM/yyyy HH:mm}. Uma cópia de segurança da base atual será criada antes."
            : $"Isso vai substituir todos os dados atuais pelo arquivo {Path.GetFileName(caminho)}. Uma cópia de segurança da base atual será criada antes.";
        if (!_confirmador.Confirmar("Restaurar backup", mensagemConfirmacao))
        {
            return;
        }

        try
        {
            var resultado = _restaurarBackup.Executar(caminho);
            if (!resultado.Sucesso)
            {
                MensagemErro = true;
                Mensagem = string.Join(" ", resultado.Erros);
                return;
            }

            Mensagem = string.Empty;
            SolicitouReinicio?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "RestaurarBackup");
            MensagemErro = true;
            Mensagem = "Não foi possível restaurar o backup. Tente novamente.";
        }
    }

    private bool PodeRestaurar() => BackupSelecionado is not null && !EmAndamento;
}
