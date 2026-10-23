using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public class VendaResumo
{
    public string FormaPagamentoDescricao => PagamentosConta.Descrever(Venda.PagamentosJson,Venda.FormaPagamento);
    public required Venda Venda { get; init; }
    public required int NumeroComanda { get; init; }
    public string? NomeCliente { get; init; }
}
