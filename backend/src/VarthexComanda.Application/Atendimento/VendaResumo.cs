using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public class VendaResumo
{
    public string FormaPagamentoDescricao => Venda.FormaPagamento switch
    {
        FormaPagamento.Dinheiro => "Dinheiro", FormaPagamento.Pix => "Pix",
        FormaPagamento.Debito => "Débito", FormaPagamento.Credito => "Crédito", _ => "Não informado"
    };
    public required Venda Venda { get; init; }
    public required int NumeroComanda { get; init; }
    public string? NomeCliente { get; init; }
}
