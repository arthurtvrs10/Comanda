using Microsoft.EntityFrameworkCore;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Atendimento;

public partial class EfComandaRepository
{
    private static void ExigirDesbloqueada(Comanda comanda)
    {
        if (comanda.Bloqueada) throw new InvalidOperationException("Pedido bloqueado. Desbloqueie para alterar os itens.");
    }

    private static Comanda Aberta(VarthexComandaDbContext contexto, int id)
    {
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == id)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta) throw new ComandaNaoAbertaException();
        return comanda;
    }

    public Comanda DefinirBloqueio(int comandaId, bool bloqueada)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = Aberta(contexto, comandaId);
        comanda.Bloqueada = bloqueada;
        contexto.SaveChanges();
        return comanda;
    }

    public Comanda DefinirAtendimento(int comandaId, string? operador, string? responsavel, string? observacao)
    {
        if (operador?.Length > 120 || responsavel?.Length > 120 || observacao?.Length > 1000)
            throw new ArgumentException("Use até 120 caracteres no responsável e até 1.000 na observação.");
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = Aberta(contexto, comandaId);
        ExigirDesbloqueada(comanda);
        comanda.CriadoPor ??= string.IsNullOrWhiteSpace(operador) ? null : operador.Trim();
        comanda.Responsavel = string.IsNullOrWhiteSpace(responsavel) ? null : responsavel.Trim();
        comanda.Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        contexto.SaveChanges();
        return comanda;
    }

    public ComandaComItens Recalcular(int comandaId)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = Aberta(contexto, comandaId);
        var itens = contexto.ItensComanda.Where(i => i.ComandaId == comandaId).OrderBy(i => i.Id).ToList();
        foreach (var item in itens) item.SubtotalCentavos = VarthexComanda.Domain.MedidasVenda.CalcularSubtotal(item.PrecoUnitarioCentavos, item.Quantidade);
        comanda.TotalCentavos = checked(itens.Sum(i => i.SubtotalCentavos));
        contexto.SaveChanges();
        return new() { Comanda = comanda, Itens = itens };
    }

    public ComandaComItens TransferirItens(int origemId, int destinoId, IReadOnlyCollection<int> itens, bool copiar, DateTime agora)
    {
        if (origemId == destinoId) throw new InvalidOperationException("Escolha outra comanda para o destino.");
        if (itens.Count == 0) throw new InvalidOperationException("Selecione pelo menos um item.");
        using var contexto = _fabricaContexto.CreateDbContext();
        using var transacao = contexto.Database.BeginTransaction();
        var origem = Aberta(contexto, origemId);
        var destino = Aberta(contexto, destinoId);
        ExigirDesbloqueada(origem);
        ExigirDesbloqueada(destino);
        var ids = itens.Distinct().ToArray();
        var selecionados = contexto.ItensComanda.Where(i => i.ComandaId == origemId && ids.Contains(i.Id)).ToList();
        if (selecionados.Count != ids.Length) throw new InvalidOperationException("Os itens mudaram. Atualize a comanda e tente novamente.");
        foreach (var item in selecionados)
        {
            if (copiar)
                contexto.ItensComanda.Add(new ItemComanda
                {
                    Id = 0, ComandaId = destinoId, ProdutoId = item.ProdutoId, NomeProduto = item.NomeProduto,
                    UnidadeVenda = item.UnidadeVenda,
                    PrecoUnitarioCentavos = item.PrecoUnitarioCentavos, Quantidade = item.Quantidade,
                    SubtotalCentavos = item.SubtotalCentavos, Observacao = item.Observacao,
                    LancadoPor = item.LancadoPor, CriadoEm = agora, AtualizadoEm = agora
                });
            else { item.ComandaId = destinoId; item.AtualizadoEm = agora; }
        }
        contexto.SaveChanges();
        origem.TotalCentavos = contexto.ItensComanda.Where(i => i.ComandaId == origemId).Sum(i => i.SubtotalCentavos);
        destino.TotalCentavos = contexto.ItensComanda.Where(i => i.ComandaId == destinoId).Sum(i => i.SubtotalCentavos);
        contexto.SaveChanges();
        transacao.Commit();
        return new() { Comanda = origem, Itens = contexto.ItensComanda.Where(i => i.ComandaId == origemId).OrderBy(i => i.Id).ToList() };
    }

    // Só é chamado depois de a saída aceitar a impressão; cancelar a prévia não consome os itens pendentes.
    public void RegistrarImpressao(int comandaId, IReadOnlyDictionary<int, decimal> quantidades, bool cozinha)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        Aberta(contexto, comandaId);
        var ids = quantidades.Keys.ToArray();
        var itens = contexto.ItensComanda.Where(i => i.ComandaId == comandaId && ids.Contains(i.Id)).ToList();
        if (itens.Count != ids.Length) throw new InvalidOperationException("Os itens mudaram. Confira a comanda antes de imprimir novamente.");
        foreach (var item in itens)
        {
            var quantidade = quantidades[item.Id];
            if (quantidade < 0) throw new ArgumentOutOfRangeException(nameof(quantidades));
            if (cozinha) item.QuantidadeCozinha = Math.Min(item.Quantidade, checked(item.QuantidadeCozinha + quantidade));
            else item.QuantidadeFicha = Math.Min(item.Quantidade, checked(item.QuantidadeFicha + quantidade));
        }
        contexto.SaveChanges();
    }
}
