namespace VarthexComanda.Application.Configuracao;

public class ConfiguracaoEstabelecimento
{
    public required string NomeEstabelecimento { get; init; }
    public int NumeroInicialComandas { get; init; } = 1;
    public int? QuantidadeMaximaComandas { get; init; }
    public string? PastaBackupExterna { get; init; }
}
