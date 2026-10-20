using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public interface IComandaRepository
{
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
    ComandaComItens AlterarQuantidade(int itemId, int quantidade, DateTime agora);
    ComandaComItens RemoverItem(int itemId, DateTime agora);
    Comanda CancelarComanda(int comandaId, DateTime agora);
    Venda EncerrarComanda(int comandaId, DateTime agora, FormaPagamento? formaPagamento = null);
}
