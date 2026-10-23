using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public sealed class ConsultarFilaCozinha(ICozinhaRepository repositorio)
{
    public IReadOnlyList<ComandaComItens> Executar() => repositorio.ListarFila();
}

public sealed record ConfirmacaoPreparo(int ComandaId);

public sealed class AtualizarPreparo(ICozinhaRepository repositorio, IClock relogio)
{
    public Resultado<ConfirmacaoPreparo> Executar(int comandaId, IReadOnlyList<AlteracaoPreparo> alteracoes)
    {
        if (alteracoes.Count == 0 || alteracoes.Select(a => a.ItemId).Distinct().Count() != alteracoes.Count)
            return Resultado<ConfirmacaoPreparo>.Falha("Selecione os itens exibidos que deseja atualizar.");
        if (alteracoes.Any(a => !TransicaoPermitida(a.Esperado,a.Destino) || a.QuantidadeEsperada <= 0))
            return Resultado<ConfirmacaoPreparo>.Falha("Inicie o preparo ou marque o item como pronto antes de entregar.");
        try
        {
            repositorio.AlterarPreparo(comandaId,alteracoes,relogio.UtcNow);
            return Resultado<ConfirmacaoPreparo>.Ok(new(comandaId));
        }
        catch (InvalidOperationException ex) { return Resultado<ConfirmacaoPreparo>.Falha(ex.Message); }
    }

    public static bool TransicaoPermitida(StatusPreparo origem, StatusPreparo destino) => (origem,destino) is
        (StatusPreparo.Pendente,StatusPreparo.EmPreparo) or (StatusPreparo.Pendente,StatusPreparo.Pronto)
        or (StatusPreparo.EmPreparo,StatusPreparo.Pronto) or (StatusPreparo.Pronto,StatusPreparo.Entregue);
}
