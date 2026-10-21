using Serilog;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Domain;

namespace VarthexComanda.Application.Backup;

public class CriarBackupManual
{
    private readonly IBackupService _backupService;
    private readonly ILogger? _logger;

    public CriarBackupManual(IBackupService backupService, ILogger? logger = null)
    {
        _backupService = backupService;
        _logger = logger;
    }

    public Resultado<BackupRegistro> Executar(string? pastaExterna, IProgress<EtapaBackup>? progresso = null)
    {
        var resultado = _backupService.CriarBackupGerenciado(progresso);
        if (pastaExterna is not null && resultado.Sucesso)
        {
            try
            {
                progresso?.Report(new(DateTime.UtcNow, "Criando cópia na pasta externa…"));
                var resultadoExterno = _backupService.CriarBackupExterno(pastaExterna);
                if (!resultadoExterno.Sucesso)
                {
                    resultado.Valor!.Mensagem = "Backup criado na pasta do aplicativo, mas a cópia na pasta externa falhou: " + string.Join(" ", resultadoExterno.Erros);
                }
                else
                {
                    var copia = resultadoExterno.Valor!;
                    progresso?.Report(new(DateTime.UtcNow, "Backup gravado na pasta externa:\n" + Path.Combine(copia.Destino, copia.Arquivo)));
                    if (!string.IsNullOrEmpty(copia.Mensagem))
                    {
                        resultado.Valor!.Mensagem = string.Join(" ", new[] { resultado.Valor!.Mensagem, copia.Mensagem }.Where(m => !string.IsNullOrWhiteSpace(m)));
                        progresso?.Report(new(DateTime.UtcNow, copia.Mensagem, true));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Falha em {Operacao}", "CriarBackupExterno");
                // falha na cópia externa não invalida o backup gerenciado, que já teve sucesso,
                // mas o operador precisa ser avisado de que a cópia externa não foi feita
                resultado.Valor!.Mensagem = "Backup criado na pasta do aplicativo, mas a cópia na pasta externa falhou: " + ex.Message;
            }
        }

        return resultado;
    }
}
