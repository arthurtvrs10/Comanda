using Microsoft.EntityFrameworkCore;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Atendimento;

public sealed class EfHistoricoPedidosRepository(IDbContextFactory<VarthexComandaDbContext> fabrica) : IHistoricoPedidosRepository
{
    public IReadOnlyList<PedidoHistoricoResumo> Pesquisar(FiltroHistoricoPedidos filtro)
    {
        filtro.Validar();
        using var db = fabrica.CreateDbContext();
        var inicio = FusoBrasilia.ParaUtc(new DateTime(filtro.De.Year, filtro.De.Month, filtro.De.Day, filtro.De.Hour, filtro.De.Minute, 0));
        var fim = FusoBrasilia.ParaUtc(new DateTime(filtro.Ate.Year, filtro.Ate.Month, filtro.Ate.Day, filtro.Ate.Hour, filtro.Ate.Minute, 0).AddMinutes(1));
        var comandas = db.Comandas.AsNoTracking().AsQueryable();
        comandas = filtro.Data == DataFiltroPedido.Abertura
            ? comandas.Where(c => c.AbertaEm >= inicio && c.AbertaEm < fim)
            : comandas.Where(c => c.FechadaEm >= inicio && c.FechadaEm < fim);
        if (!filtro.IncluirCancelados) comandas = comandas.Where(c => c.Status != StatusComanda.Cancelada);
        if (filtro.Status == StatusFiltroPedido.Abertos) comandas = comandas.Where(c => c.Status == StatusComanda.Aberta);
        if (filtro.Status is StatusFiltroPedido.Finalizados or StatusFiltroPedido.FinalizadosEPagos) comandas = comandas.Where(c => c.Status == StatusComanda.Fechada);
        if (filtro.Status == StatusFiltroPedido.FinalizadosEPagos)
            comandas = comandas.Where(c => db.Vendas.Any(v => v.ComandaId == c.Id && v.Status == StatusVenda.Concluida));
        var codigos = filtro.LerCodigos();
        if (codigos.Length > 0) comandas = comandas.Where(c => codigos.Contains(c.CodigoPedido ?? c.Id));
        var resultados = (from c in comandas
                join v in db.Vendas.AsNoTracking().Where(v => v.Status == StatusVenda.Concluida) on c.Id equals v.ComandaId into vendas
                from v in vendas.DefaultIfEmpty()
                orderby c.AbertaEm descending, c.Id descending
                select new PedidoHistoricoResumo { Comanda = c, Venda = v, TotalItens = db.ItensComanda.Count(i => i.ComandaId == c.Id) }).ToList();
        // OrdinalIgnoreCase também cobre letras acentuadas; o UPPER nativo do SQLite só cobre ASCII.
        return resultados.Where(p => p.Cliente.Contains(filtro.Cliente.Trim(), StringComparison.OrdinalIgnoreCase)
            && (p.CriadoPor.Contains(filtro.Colaborador.Trim(), StringComparison.OrdinalIgnoreCase)
                || p.Responsavel.Contains(filtro.Colaborador.Trim(), StringComparison.OrdinalIgnoreCase))).ToList();
    }

    public ComandaComItens? BuscarDetalhes(int comandaId) => new EfComandaRepository(fabrica).BuscarComItens(comandaId);
}
