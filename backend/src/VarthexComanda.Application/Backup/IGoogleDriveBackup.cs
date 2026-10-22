namespace VarthexComanda.Application.Backup;

public interface IGoogleDriveConnection
{
    bool Configurada { get; }
    bool Conectada { get; }
    void ImportarCredenciais(string arquivo);
    Task ConectarAsync(CancellationToken cancellationToken = default);
    Task DesconectarAsync(CancellationToken cancellationToken = default);
    Task<string> ObterAccessTokenAsync(CancellationToken cancellationToken = default);
}

public interface IGoogleDriveBackup
{
    bool Conectado { get; }
    Task EnviarAsync(string banco, string? fotos, IProgress<EtapaBackup>? progresso = null, CancellationToken cancellationToken = default);
}
