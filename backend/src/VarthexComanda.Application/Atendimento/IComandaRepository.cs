using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public interface IComandaRepository
{
    long ObterMaiorCodigoPedido() => 0;
    IReadOnlyList<Comanda> ListarAbertas();
    ComandaComItens? BuscarComItens(int comandaId);
    Comanda AbrirComanda(int numero, DateTime agora);
    Comanda DefinirNomeCliente(int comandaId, string? nome);
    Comanda DefinirMesa(int comandaId, int? numeroMesa);
    Comanda DefinirOrigemCliente(int comandaId, string? origem);
    ComandaComItens JuntarComandas(int origemId, int destinoId, DateTime agora);
    Comanda DefinirPrioridade(int comandaId, PrioridadeComanda prioridade);
    Comanda MoverPara(int comandaId, int numero);
    Comanda DefinirQuantidadePessoas(int comandaId, int quantidade);
    ComandaComItens AdicionarItem(int comandaId, Produto produto, int quantidade, DateTime agora, string? observacao = null);
    ComandaComItens AdicionarItem(int comandaId, Produto produto, int quantidade, DateTime agora, string? observacao, string? operador)
        => AdicionarItem(comandaId, produto, quantidade, agora, observacao);
    Comanda DefinirBloqueio(int comandaId, bool bloqueada) => throw new NotSupportedException();
    Comanda DefinirAtendimento(int comandaId, string? operador, string? responsavel, string? observacao) => throw new NotSupportedException();
    ComandaComItens TransferirItens(int origemId, int destinoId, IReadOnlyCollection<int> itens, bool copiar, DateTime agora) => throw new NotSupportedException();
    ComandaComItens Recalcular(int comandaId) => throw new NotSupportedException();
    void RegistrarImpressao(int comandaId, IReadOnlyDictionary<int, int> quantidades, bool cozinha) => throw new NotSupportedException();
    ComandaComItens AlterarQuantidade(int itemId, int quantidade, DateTime agora);
    ComandaComItens RemoverItem(int itemId, DateTime agora);
    Comanda CancelarComanda(int comandaId, DateTime agora);
    Venda EncerrarComanda(int comandaId, DateTime agora, FormaPagamento? formaPagamento = null);
}
