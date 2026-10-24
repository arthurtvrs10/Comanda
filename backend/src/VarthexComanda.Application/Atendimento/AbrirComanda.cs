using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public class AbrirComanda
{
    private readonly IComandaRepository _comandas;
    private readonly IClock _relogio;
    private readonly ObterConfiguracao _obterConfiguracao;

    public AbrirComanda(IComandaRepository comandas, IClock relogio, ObterConfiguracao obterConfiguracao)
    {
        _comandas = comandas;
        _relogio = relogio;
        _obterConfiguracao = obterConfiguracao;
    }

    public Resultado<Comanda> Executar(int numero)
    {
        if (numero <= 0)
        {
            return Resultado<Comanda>.Falha("Informe um número de comanda válido.");
        }

        var configuracao = _obterConfiguracao.Executar();
        if(numero < configuracao.NumeroInicialComandas)
            return Resultado<Comanda>.Falha($"O número da comanda deve ser no mínimo {configuracao.NumeroInicialComandas}.");
        var maximo = Math.Min(999_999_999L, (long)configuracao.NumeroInicialComandas + (configuracao.QuantidadeMaximaComandas ?? NumeracaoComandasService.LimiteQuantidade) - 1);
        if (numero > maximo)
        {
            return Resultado<Comanda>.Falha($"O número da comanda deve ser no máximo {maximo}.");
        }

        try
        {
            return Resultado<Comanda>.Ok(_comandas.AbrirComanda(numero, _relogio.UtcNow));
        }
        catch (NumeroComandaOcupadoException ex)
        {
            return Resultado<Comanda>.Falha(ex.Message);
        }
    }

    /// <summary>Pedido sem mesa, usando o primeiro número livre da faixa configurada.</summary>
    public Resultado<Comanda> ExecutarBalcao()
    {
        var configuracao = _obterConfiguracao.Executar();
        var ocupados = _comandas.ListarAbertas().Select(c => c.Numero).ToHashSet();
        long numero = configuracao.NumeroInicialComandas;
        var limite = Math.Min(999_999_999L, numero + (configuracao.QuantidadeMaximaComandas ?? NumeracaoComandasService.LimiteQuantidade) - 1);
        while (numero <= limite && ocupados.Contains((int)numero)) numero++;
        if (numero > limite)
            return Resultado<Comanda>.Falha("Não há número de comanda livre. Encerre um pedido ou ajuste a quantidade de comandas nas configurações.");
        // A gravação mantém a restrição de número único. Uma disputa com outro
        // atendimento falha sem selecionar nem alterar o pedido de outra pessoa.
        return Executar((int)numero);
    }
}
