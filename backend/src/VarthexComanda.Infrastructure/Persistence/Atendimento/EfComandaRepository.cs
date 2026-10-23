using Microsoft.EntityFrameworkCore;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Atendimento;

public partial class EfComandaRepository : IComandaRepository
{
    private readonly IDbContextFactory<VarthexComandaDbContext> _fabricaContexto;

    public EfComandaRepository(IDbContextFactory<VarthexComandaDbContext> fabricaContexto)
    {
        _fabricaContexto = fabricaContexto;
    }

    public long ObterMaiorCodigoPedido()
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        return contexto.Comandas.Max(c => c.CodigoPedido) ?? 0;
    }

    public IReadOnlyList<Comanda> ListarAbertas()
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        return contexto.Comandas
            .Where(c => c.Status == StatusComanda.Aberta)
            .OrderBy(c => c.Numero)
            .ToList();
    }

    public ComandaComItens? BuscarComItens(int comandaId)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId);
        if (comanda is null)
        {
            return null;
        }

        var itens = contexto.ItensComanda
            .Where(i => i.ComandaId == comandaId)
            .OrderBy(i => i.Id)
            .ToList();
        return new ComandaComItens { Comanda = comanda, Itens = itens };
    }

    public Comanda AbrirComanda(int numero, DateTime agora)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        using var transacao = contexto.Database.BeginTransaction();
        var configuracoes = contexto.Configuracoes.Where(c => c.Chave == NumeracaoPedidosService.ChaveAtiva
            || c.Chave == NumeracaoPedidosService.ChaveInicio || c.Chave == NumeracaoPedidosService.ChaveUltimo).ToDictionary(c => c.Chave);
        long? codigoPedido = null;
        if (configuracoes.GetValueOrDefault(NumeracaoPedidosService.ChaveAtiva)?.Valor == "true")
        {
            var inicio = NumeracaoPedidosService.LerNumero(configuracoes.GetValueOrDefault(NumeracaoPedidosService.ChaveInicio)?.Valor, 100);
            if (inicio < 1) throw new InvalidOperationException("O número inicial do pedido deve ser maior que zero.");
            var ultimo = NumeracaoPedidosService.LerNumero(configuracoes.GetValueOrDefault(NumeracaoPedidosService.ChaveUltimo)?.Valor, 0);
            var maior = contexto.Comandas.Max(c => c.CodigoPedido) ?? 0;
            codigoPedido = Math.Max(inicio, Math.Max(ultimo, maior) + 1);
            if (codigoPedido > NumeracaoPedidosService.LimiteNumero)
                throw new InvalidOperationException("O limite da numeração de pedidos foi atingido. Desative a numeração personalizada para continuar.");
            if (!configuracoes.TryGetValue(NumeracaoPedidosService.ChaveUltimo, out var contador))
            {
                contador = new VarthexComanda.Domain.Configuracao { Chave = NumeracaoPedidosService.ChaveUltimo, Valor = "0", AtualizadoEm = agora };
                contexto.Configuracoes.Add(contador);
            }
            contador.Valor = codigoPedido.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            contador.AtualizadoEm = agora;
        }
        var comanda = new Comanda
        {
            Id = 0,
            Numero = numero,
            CodigoPedido = codigoPedido,
            Status = StatusComanda.Aberta,
            AbertaEm = agora,
            FechadaEm = null,
            TotalCentavos = 0
        };
        contexto.Comandas.Add(comanda);
        try
        {
            contexto.SaveChanges();
            transacao.Commit();
        }
        catch (DbUpdateException)
        {
            throw new NumeroComandaOcupadoException();
        }
        return comanda;
    }

    public Comanda DefinirNomeCliente(int comandaId, string? nome)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta) throw new ComandaNaoAbertaException();
        comanda.NomeCliente = nome;
        contexto.SaveChanges();
        return comanda;
    }

    public Comanda DefinirPrioridade(int comandaId, PrioridadeComanda prioridade)
    {
        if (!Enum.IsDefined(prioridade)) throw new ArgumentOutOfRangeException(nameof(prioridade));
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta) throw new ComandaNaoAbertaException();
        comanda.Prioridade = prioridade;
        contexto.SaveChanges();
        return comanda;
    }

    public Comanda MoverPara(int comandaId, int numero)
    {
        if (numero <= 0) throw new ArgumentOutOfRangeException(nameof(numero));
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta) throw new ComandaNaoAbertaException();
        if (contexto.Comandas.Any(c => c.Numero == numero && c.Status == StatusComanda.Aberta && c.Id != comandaId))
            throw new NumeroComandaOcupadoException();
        comanda.Numero = numero;
        try { contexto.SaveChanges(); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteExtendedErrorCode: 2067 })
        { throw new NumeroComandaOcupadoException(); }
        return comanda;
    }

    public Comanda DefinirQuantidadePessoas(int comandaId, int quantidade)
    {
        if (quantidade is < 1 or > 999) throw new ArgumentOutOfRangeException(nameof(quantidade));
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta) throw new ComandaNaoAbertaException();
        comanda.QuantidadePessoas = quantidade;
        contexto.SaveChanges();
        return comanda;
    }

    public Comanda DefinirMesa(int comandaId, int? numeroMesa)
    {
        if (numeroMesa is < 1 or > 999) throw new ArgumentOutOfRangeException(nameof(numeroMesa));
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta) throw new ComandaNaoAbertaException();
        comanda.NumeroMesa = numeroMesa;
        contexto.SaveChanges();
        return comanda;
    }

    public Comanda DefinirOrigemCliente(int comandaId,string? origem)
    {
        if(origem?.Length>80)throw new ArgumentException("A origem deve ter até 80 caracteres.");
        using var contexto=_fabricaContexto.CreateDbContext();
        var comanda=contexto.Comandas.SingleOrDefault(c=>c.Id==comandaId)??throw new InvalidOperationException("Comanda não encontrada.");
        if(comanda.Status!=StatusComanda.Aberta)throw new ComandaNaoAbertaException();
        comanda.OrigemCliente=string.IsNullOrWhiteSpace(origem)?null:origem.Trim();contexto.SaveChanges();return comanda;
    }

    public ComandaComItens JuntarComandas(int origemId, int destinoId, DateTime agora)
    {
        if (origemId == destinoId) throw new InvalidOperationException("Escolha duas comandas diferentes.");
        using var contexto = _fabricaContexto.CreateDbContext();
        using var transacao = contexto.Database.BeginTransaction();
        var origem = contexto.Comandas.SingleOrDefault(c => c.Id == origemId)
            ?? throw new InvalidOperationException("Comanda de origem não encontrada.");
        var destino = contexto.Comandas.SingleOrDefault(c => c.Id == destinoId)
            ?? throw new InvalidOperationException("Comanda de destino não encontrada.");
        if (origem.Status != StatusComanda.Aberta || destino.Status != StatusComanda.Aberta)
            throw new ComandaNaoAbertaException();
        ExigirDesbloqueada(origem);
        ExigirDesbloqueada(destino);
        var itensOrigem = contexto.ItensComanda.Where(i => i.ComandaId == origemId).ToList();
        if (itensOrigem.Count == 0) throw new InvalidOperationException("A comanda de origem não tem itens para transferir.");
        var itensDestino = contexto.ItensComanda.Where(i => i.ComandaId == destinoId).ToList();
        foreach (var item in itensOrigem)
        {
            item.ComandaId = destinoId;
            item.AtualizadoEm = agora;
            itensDestino.Add(item);
        }
        destino.TotalCentavos = checked(itensDestino.Sum(i => i.SubtotalCentavos));
        origem.TotalCentavos = 0;
        origem.Status = StatusComanda.Cancelada;
        origem.FechadaEm = agora;
        var registro = $"Itens transferidos para a comanda {destino.Numero} (ID {destino.Id}) em {agora:O}.";
        origem.Observacao = string.IsNullOrWhiteSpace(origem.Observacao) ? registro : origem.Observacao + "\n" + registro;
        contexto.SaveChanges();
        transacao.Commit();
        return new ComandaComItens { Comanda = destino, Itens = itensDestino.OrderBy(i => i.Id).ToList() };
    }

    public ComandaComItens AdicionarItem(int comandaId, Produto produto, decimal quantidade, DateTime agora, string? observacao = null)
        => AdicionarItem(comandaId, produto, quantidade, agora, observacao, null);

    public ComandaComItens AdicionarItem(int comandaId, Produto produto, decimal quantidade, DateTime agora, string? observacao, string? operador)
    {
        if (MedidasVenda.ValidarQuantidade(quantidade,produto.UnidadeVenda) is string erro) throw new InvalidOperationException(erro);
        if (MedidasVenda.CalcularSubtotal(produto.PrecoCentavos,quantidade) <= 0) throw new InvalidOperationException("A quantidade deve resultar em pelo menos R$ 0,01.");
        observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        if (observacao?.Length > 300) throw new ArgumentException("A observação deve ter no máximo 300 caracteres.", nameof(observacao));
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta)
        {
            throw new ComandaNaoAbertaException();
        }

        ExigirDesbloqueada(comanda);
        var itens = contexto.ItensComanda.Where(i => i.ComandaId == comandaId).OrderBy(i => i.Id).ToList();
        var itemExistente = itens.FirstOrDefault(i => i.ProdutoId == produto.Id && i.UnidadeVenda == produto.UnidadeVenda && i.PrecoUnitarioCentavos == produto.PrecoCentavos && i.Observacao == observacao && i.LancadoPor == operador);

        if (itemExistente is not null)
        {
            itemExistente.Quantidade += quantidade;
            itemExistente.SubtotalCentavos = VarthexComanda.Domain.MedidasVenda.CalcularSubtotal(itemExistente.PrecoUnitarioCentavos, itemExistente.Quantidade);
            itemExistente.AtualizadoEm = agora;
        }
        else
        {
            var novoItem = new ItemComanda
            {
                Id = 0,
                ComandaId = comandaId,
                ProdutoId = produto.Id,
                UnidadeVenda = produto.UnidadeVenda,
                NomeProduto = produto.Nome,
                Observacao = observacao,
                LancadoPor = operador,
                PrecoUnitarioCentavos = produto.PrecoCentavos,
                Quantidade = quantidade,
                SubtotalCentavos = VarthexComanda.Domain.MedidasVenda.CalcularSubtotal(produto.PrecoCentavos, quantidade),
                CriadoEm = agora,
                AtualizadoEm = agora
            };
            contexto.ItensComanda.Add(novoItem);
            itens.Add(novoItem);
        }

        comanda.TotalCentavos = itens.Sum(i => i.SubtotalCentavos);
        contexto.SaveChanges();

        return new ComandaComItens { Comanda = comanda, Itens = itens, ItemAlteradoId = itemExistente?.Id ?? itens.Last().Id };
    }

    public ComandaComItens AlterarQuantidade(int itemId, decimal quantidade, DateTime agora)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var item = contexto.ItensComanda.SingleOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Item não encontrado.");
        if (MedidasVenda.ValidarQuantidade(quantidade,item.UnidadeVenda) is string erro) throw new InvalidOperationException(erro);
        if (MedidasVenda.CalcularSubtotal(item.PrecoUnitarioCentavos,quantidade) <= 0) throw new InvalidOperationException("A quantidade deve resultar em pelo menos R$ 0,01.");
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == item.ComandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta)
        {
            throw new ComandaNaoAbertaException();
        }

        ExigirDesbloqueada(comanda);
        item.QuantidadeCozinha = Math.Min(item.QuantidadeCozinha, quantidade);
        item.QuantidadeFicha = Math.Min(item.QuantidadeFicha, quantidade);
        item.Quantidade = quantidade;
        item.SubtotalCentavos = VarthexComanda.Domain.MedidasVenda.CalcularSubtotal(item.PrecoUnitarioCentavos, quantidade);
        item.AtualizadoEm = agora;

        var itens = contexto.ItensComanda.Where(i => i.ComandaId == comanda.Id).OrderBy(i => i.Id).ToList();
        comanda.TotalCentavos = itens.Sum(i => i.SubtotalCentavos);
        contexto.SaveChanges();

        return new ComandaComItens { Comanda = comanda, Itens = itens };
    }

    public ComandaComItens RemoverItem(int itemId, DateTime agora)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var item = contexto.ItensComanda.SingleOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Item não encontrado.");
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == item.ComandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta)
        {
            throw new ComandaNaoAbertaException();
        }

        ExigirDesbloqueada(comanda);
        contexto.ItensComanda.Remove(item);

        var itensRestantes = contexto.ItensComanda
            .Where(i => i.ComandaId == comanda.Id && i.Id != itemId)
            .OrderBy(i => i.Id)
            .ToList();
        comanda.TotalCentavos = itensRestantes.Sum(i => i.SubtotalCentavos);
        contexto.SaveChanges();

        return new ComandaComItens { Comanda = comanda, Itens = itensRestantes };
    }

    public Comanda CancelarComanda(int comandaId, DateTime agora)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta)
        {
            throw new ComandaNaoAbertaException();
        }

        if(PagamentosConta.Total(comanda.PagamentosJson)>0)throw new InvalidOperationException("Remova os registros de recebimento antes de cancelar a comanda.");
        comanda.Status = StatusComanda.Cancelada;
        comanda.FechadaEm = agora;
        contexto.SaveChanges();
        return comanda;
    }

    public Venda EncerrarComanda(int comandaId, DateTime agora, FormaPagamento? formaPagamento = null)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var comanda = contexto.Comandas.SingleOrDefault(c => c.Id == comandaId)
            ?? throw new InvalidOperationException("Comanda não encontrada.");
        if (comanda.Status != StatusComanda.Aberta)
        {
            throw new ComandaNaoAbertaException();
        }

        var pagamentos=PagamentosConta.Ler(comanda.PagamentosJson).Where(p=>!p.Cancelado).ToArray();
        if(pagamentos.Length>0&&pagamentos.Sum(p=>p.ValorCentavos)!=comanda.TotalCentavos)throw new InvalidOperationException("Conclua o recebimento do total antes de encerrar.");
        if(pagamentos.Length>0)formaPagamento=pagamentos.Select(p=>p.Forma).Distinct().Count()==1?pagamentos[0].Forma:null;
        var proximoNumero = (contexto.Vendas.Max(v => (int?)v.Numero) ?? 0) + 1;
        var venda = new Venda
        {
            Id = 0,
            ComandaId = comanda.Id,
            FormaPagamento = formaPagamento,
            PagamentosJson=comanda.PagamentosJson,
            Numero = proximoNumero,
            TotalCentavos = comanda.TotalCentavos,
            FinalizadaEm = agora,
            Status = StatusVenda.Concluida
        };
        contexto.Vendas.Add(venda);

        comanda.Status = StatusComanda.Fechada;
        comanda.FechadaEm = agora;

        contexto.SaveChanges();
        return venda;
    }
}
