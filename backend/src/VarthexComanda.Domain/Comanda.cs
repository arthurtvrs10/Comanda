namespace VarthexComanda.Domain;

public class Comanda
{
    public required int Id { get; set; }
    public required int Numero { get; set; }
    public long? CodigoPedido { get; set; }
    public required StatusComanda Status { get; set; }
    public required DateTime AbertaEm { get; set; }
    public DateTime? FechadaEm { get; set; }
    public required long TotalCentavos { get; set; }
    public string? Observacao { get; set; }
    public bool Bloqueada { get; set; }
    public string? CriadoPor { get; set; }
    public string? Responsavel { get; set; }
    public string? NomeCliente { get; set; }
    public int? NumeroMesa { get; set; }
    public string? OrigemCliente { get; set; }
    public PrioridadeComanda Prioridade { get; set; }
    public int QuantidadePessoas { get; set; } = 1;
}
