using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Catalogo;

namespace VarthexComanda.Application.Atendimento;

public class AdicionarItem
{
    private readonly IComandaRepository _comandas;
    private readonly IProdutoRepository _produtos;
    private readonly IClock _relogio;
    private readonly RegistroEventos? _eventos;

    public AdicionarItem(IComandaRepository comandas, IProdutoRepository produtos, IClock relogio, RegistroEventos? eventos = null)
    {
        _comandas = comandas;
        _produtos = produtos;
        _relogio = relogio;
        _eventos = eventos;
    }

    public Resultado<ComandaComItens> Executar(int comandaId, int produtoId, decimal quantidade, string? observacao = null, string? operador = null, IReadOnlyList<Guid>? complementos = null) => _eventos is null
        ? ExecutarCore(comandaId, produtoId, quantidade, observacao, operador, complementos)
        : _eventos.Executar(OrigemEvento.Atendimento, AcaoEvento.AdicionarItem, () => ExecutarCore(comandaId, produtoId, quantidade, observacao, operador, complementos));

    private Resultado<ComandaComItens> ExecutarCore(int comandaId, int produtoId, decimal quantidade, string? observacao = null, string? operador = null, IReadOnlyList<Guid>? complementos = null)
    {
        observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        if (observacao?.Length > 300)
            return Resultado<ComandaComItens>.Falha("A observação deve ter no máximo 300 caracteres.");
        if (quantidade <= 0)
        {
            return Resultado<ComandaComItens>.Falha("A quantidade deve ser maior que zero.");
        }

        var produto = _produtos.BuscarPorId(produtoId);
        if (produto is null || !produto.Ativo || produto.EhComplemento)
        {
            return Resultado<ComandaComItens>.Falha("Produto indisponível.");
        }
        if (VarthexComanda.Domain.MedidasVenda.ValidarQuantidade(quantidade,produto.UnidadeVenda) is string erro)
            return Resultado<ComandaComItens>.Falha(erro);

        try
        {
            VarthexComanda.Domain.ComplementosProduto.VerificarEstoque(produto, complementos ?? [], quantidade);
            return Resultado<ComandaComItens>.Ok(_comandas.AdicionarItem(comandaId, produto, quantidade, _relogio.UtcNow, observacao, operador, complementos ?? []));
        }
        catch (ComandaNaoAbertaException ex)
        {
            return Resultado<ComandaComItens>.Falha(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Resultado<ComandaComItens>.Falha(ex.Message);
        }
    }
}
