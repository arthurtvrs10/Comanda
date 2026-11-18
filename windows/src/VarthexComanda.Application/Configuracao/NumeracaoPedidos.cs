using System.Globalization;

namespace VarthexComanda.Application.Configuracao;

public sealed class ConfiguracaoNumeracaoPedidos
{
    public bool Ativa { get; set; }
    public long Inicio { get; set; } = 100;
    public bool UsarComoSenhaAtendimento { get; set; }
    public long UltimoCriado { get; set; }
}

public sealed class NumeracaoPedidosService
{
    public const string ChaveAtiva = "pedidos.numeracao.ativa";
    public const string ChaveInicio = "pedidos.numeracao.inicio";
    public const string ChaveUltimo = "pedidos.numeracao.ultimo";
    public const string ChaveSenhaAtendimento = "pedidos.numeracao.senha-atendimento";
    public const long LimiteNumero = 999_999_999;
    private readonly IConfiguracaoRepository _repositorio;
    private readonly Func<long> _maiorCodigoExistente;

    public NumeracaoPedidosService(IConfiguracaoRepository repositorio, Func<long>? maiorCodigoExistente = null)
    {
        _repositorio = repositorio;
        _maiorCodigoExistente = maiorCodigoExistente ?? (() => 0);
    }

    public ConfiguracaoNumeracaoPedidos Carregar() => new()
    {
        Ativa = _repositorio.ObterValor(ChaveAtiva) == "true",
        Inicio = LerNumero(_repositorio.ObterValor(ChaveInicio), 100),
        UltimoCriado = Math.Max(_maiorCodigoExistente(), LerNumero(_repositorio.ObterValor(ChaveUltimo), 0)),
        UsarComoSenhaAtendimento = _repositorio.ObterValor(ChaveSenhaAtendimento) == "true"
    };

    public void Salvar(ConfiguracaoNumeracaoPedidos configuracao, DateTime agora)
    {
        Validar(configuracao);
        _repositorio.DefinirValores(Valores(configuracao), agora);
    }

    public void Reiniciar(ConfiguracaoNumeracaoPedidos configuracao, DateTime agora)
    {
        Validar(configuracao);
        var valores = Valores(configuracao);
        // Números já atribuídos são históricos. A alocação também consulta o maior código
        // dentro da transação, protegendo uma abertura simultânea ao reset.
        valores[ChaveUltimo] = _maiorCodigoExistente().ToString(CultureInfo.InvariantCulture);
        _repositorio.DefinirValores(valores, agora);
    }

    public static long LerNumero(string? texto, long padrao)
    {
        if (texto is null) return padrao;
        if (!long.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) || numero < 0 || numero > LimiteNumero)
            throw new InvalidOperationException("A numeração de pedidos está inválida. Revise as configurações antes de abrir uma comanda.");
        return numero;
    }

    private static void Validar(ConfiguracaoNumeracaoPedidos configuracao)
    {
        if (configuracao.Inicio is < 1 or > LimiteNumero)
            throw new ArgumentException($"O número inicial deve estar entre 1 e {LimiteNumero:N0}.");
    }

    private static Dictionary<string, string> Valores(ConfiguracaoNumeracaoPedidos configuracao) => new()
    {
        [ChaveAtiva] = configuracao.Ativa ? "true" : "false",
        [ChaveInicio] = configuracao.Inicio.ToString(CultureInfo.InvariantCulture),
        [ChaveSenhaAtendimento] = configuracao.UsarComoSenhaAtendimento ? "true" : "false"
    };
}
