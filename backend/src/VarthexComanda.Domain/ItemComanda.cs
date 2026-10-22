namespace VarthexComanda.Domain;

public class ItemComanda
{
    public required int Id { get; set; }
    public required int ComandaId { get; set; }
    public required int ProdutoId { get; set; }
    public required string NomeProduto { get; set; }
    public required long PrecoUnitarioCentavos { get; set; }
    public required decimal Quantidade { get; set; }
    public UnidadeVenda UnidadeVenda { get; set; }
    public string QuantidadeFormatada => MedidasVenda.FormatarQuantidade(Quantidade, UnidadeVenda);
    public string Medida => UnidadeVenda.Simbolo();
    public required long SubtotalCentavos { get; set; }
    public string? Observacao { get; set; }
    public string? LancadoPor { get; set; }
    public decimal QuantidadeCozinha { get; set; }
    public decimal QuantidadeFicha { get; set; }
    public required DateTime CriadoEm { get; set; }
    public required DateTime AtualizadoEm { get; set; }
}
