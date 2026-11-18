namespace VarthexComanda.Domain;

public class Produto
{
    public required int Id { get; set; }
    public int? CategoriaId { get; set; }
    public required string Nome { get; set; }
    public required long PrecoCentavos { get; set; }
    public required bool Ativo { get; set; }
    public required DateTime CriadoEm { get; set; }
    public required DateTime AtualizadoEm { get; set; }
    public string? FotoArquivo { get; set; }
    public long CustoCentavos { get; set; }
    public string? Descricao { get; set; }
    public string? CodigoPersonalizado { get; set; }
    public UnidadeVenda UnidadeVenda { get; set; }
    public bool ControlarEstoque { get; set; }
    public long QuantidadeEstoque { get; set; }
    public long VersaoEstoque { get; set; }
    public int MinimoComplemento { get; set; }
    public int? MaximoComplemento { get; set; }
    public string EstoqueDescricao => ControlarEstoque ? QuantidadeEstoque.ToString("N0",System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) + " UN" : "Sem controle";
    public bool EhComplemento { get; set; }
    public Guid? OrigemComplementoId { get; set; }
    public string ComplementosJson { get; set; } = "[]";
    public List<GrupoComplementos> GruposComplementos { get => ComplementosProduto.Ler(ComplementosJson); set => ComplementosJson = ComplementosProduto.Gravar(value); }
    public string Medida => UnidadeVenda.Simbolo();
}
