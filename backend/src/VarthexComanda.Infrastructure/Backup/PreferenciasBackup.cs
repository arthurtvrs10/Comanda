using System.Text.Json;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Infrastructure.Backup;

public sealed record PastasBackup(string? Local = null, string? GoogleDrive = null, string? Dropbox = null);

/// <summary>Fora do banco para continuar disponível durante a recuperação de uma base corrompida.</summary>
public sealed class PreferenciasBackup(AppPaths paths)
{
    private string Arquivo => Path.Combine(paths.Root, "pastas-backup.json");

    public PastasBackup Ler() => !File.Exists(Arquivo) ? new() :
        JsonSerializer.Deserialize<PastasBackup>(File.ReadAllText(Arquivo)) ?? new();

    public void Salvar(PastasBackup pastas)
    {
        Directory.CreateDirectory(paths.Root);
        var temporario = Arquivo + ".tmp";
        File.WriteAllText(temporario, JsonSerializer.Serialize(pastas));
        File.Move(temporario, Arquivo, overwrite: true);
    }

    public static string ValidarPasta(string pasta)
    {
        var completa = Path.GetFullPath(pasta);
        if (!Directory.Exists(completa))
            throw new IOException("A pasta não está disponível. Conecte a unidade ou abra o aplicativo de sincronização e tente novamente.");
        var teste = Path.Combine(completa, ".varthex-verificacao-" + Guid.NewGuid().ToString("N"));
        using (new FileStream(teste, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)) { }
        return completa;
    }

    public static string DestinoSincronizado(string raiz) => Path.Combine(raiz, "Varthex", "Backups");
}
