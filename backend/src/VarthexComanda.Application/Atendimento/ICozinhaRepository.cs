using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public sealed record AlteracaoPreparo(int ItemId, StatusPreparo Esperado, decimal QuantidadeEsperada, StatusPreparo Destino);

public interface ICozinhaRepository
{
    IReadOnlyList<ComandaComItens> ListarFila();
    void AlterarPreparo(int comandaId, IReadOnlyList<AlteracaoPreparo> alteracoes, DateTime agora);
}
