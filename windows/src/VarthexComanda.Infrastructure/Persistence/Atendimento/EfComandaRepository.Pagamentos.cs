using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Atendimento;

public partial class EfComandaRepository
{
    public Comanda RegistrarPagamento(int comandaId,PagamentoRegistrado pagamento)
    {
        using var contexto=_fabricaContexto.CreateDbContext();
        var comanda=contexto.Comandas.SingleOrDefault(c=>c.Id==comandaId)??throw new InvalidOperationException("Comanda não encontrada.");
        if(comanda.Status!=StatusComanda.Aberta)throw new ComandaNaoAbertaException();
        if(pagamento.ItensIds.Length>0)
        {
            var itens=contexto.ItensComanda.Where(i=>i.ComandaId==comandaId&&pagamento.ItensIds.Contains(i.Id)).ToList();
            if(itens.Count!=pagamento.ItensIds.Length||itens.Sum(i=>i.SubtotalCentavos)!=pagamento.ValorCentavos)
                throw new ArgumentException("O valor deve corresponder aos itens selecionados.");
        }
        comanda.PagamentosJson=PagamentosConta.Adicionar(comanda.PagamentosJson,pagamento,comanda.TotalCentavos);
        contexto.SaveChanges();return comanda;
    }
    public Comanda RemoverRegistroPagamento(int comandaId,Guid id)
    {
        using var contexto=_fabricaContexto.CreateDbContext();
        var comanda=contexto.Comandas.SingleOrDefault(c=>c.Id==comandaId)??throw new InvalidOperationException("Comanda não encontrada.");
        if(comanda.Status!=StatusComanda.Aberta)throw new ComandaNaoAbertaException();
        comanda.PagamentosJson=PagamentosConta.Remover(comanda.PagamentosJson,id);
        contexto.SaveChanges();return comanda;
    }
}

