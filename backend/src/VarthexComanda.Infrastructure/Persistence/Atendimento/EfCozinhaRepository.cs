using Microsoft.EntityFrameworkCore;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Atendimento;

public sealed class EfCozinhaRepository(IDbContextFactory<VarthexComandaDbContext> fabrica) : ICozinhaRepository
{
    public IReadOnlyList<ComandaComItens> ListarFila()
    {
        using var contexto=fabrica.CreateDbContext();
        var itens=contexto.ItensComanda.AsNoTracking()
            .Where(i=>i.StatusPreparo!=StatusPreparo.Entregue && contexto.Comandas.Any(c=>c.Id==i.ComandaId && c.Status!=StatusComanda.Cancelada))
            .OrderBy(i=>i.CriadoEm).ThenBy(i=>i.Id).ToList();
        var ids=itens.Select(i=>i.ComandaId).Distinct().ToArray();
        var comandas=contexto.Comandas.AsNoTracking().Where(c=>ids.Contains(c.Id)).ToDictionary(c=>c.Id);
        return itens.GroupBy(i=>i.ComandaId).Select(g=>new ComandaComItens { Comanda=comandas[g.Key],Itens=g.ToList() })
            .OrderByDescending(c=>c.Comanda.Prioridade).ThenBy(c=>c.Itens.Min(i=>i.CriadoEm)).ToArray();
    }

    public void AlterarPreparo(int comandaId, IReadOnlyList<AlteracaoPreparo> alteracoes, DateTime agora)
    {
        if(alteracoes.Count==0 || alteracoes.Select(a=>a.ItemId).Distinct().Count()!=alteracoes.Count
            || alteracoes.Any(a=>!AtualizarPreparo.TransicaoPermitida(a.Esperado,a.Destino)))
            throw new InvalidOperationException("Escolha uma transição válida de preparo.");
        using var contexto=fabrica.CreateDbContext();
        using var transacao=contexto.Database.BeginTransaction();
        var comanda=contexto.Comandas.SingleOrDefault(c=>c.Id==comandaId);
        if(comanda is null || comanda.Status==StatusComanda.Cancelada)
            throw new InvalidOperationException("A comanda foi cancelada ou não está mais disponível.");
        var ids=alteracoes.Select(a=>a.ItemId).ToArray();
        var itens=contexto.ItensComanda.Where(i=>i.ComandaId==comandaId && ids.Contains(i.Id)).ToDictionary(i=>i.Id);
        if(itens.Count!=ids.Length || alteracoes.Any(a=>itens[a.ItemId].StatusPreparo!=a.Esperado || itens[a.ItemId].Quantidade!=a.QuantidadeEsperada))
            throw new InvalidOperationException("Os itens mudaram. Atualize a cozinha e confira o pedido antes de tentar novamente.");
        foreach(var a in alteracoes) { itens[a.ItemId].StatusPreparo=a.Destino;itens[a.ItemId].AtualizadoEm=agora; }
        contexto.SaveChanges();transacao.Commit();
    }
}
