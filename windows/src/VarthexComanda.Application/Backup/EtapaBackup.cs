namespace VarthexComanda.Application.Backup;

/// <summary>Evento de uma etapa realmente executada, sem estimativa artificial de percentual.</summary>
public sealed record EtapaBackup(DateTime OcorridoEm, string Mensagem, bool Aviso = false);
