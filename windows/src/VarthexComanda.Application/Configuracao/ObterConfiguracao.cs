namespace VarthexComanda.Application.Configuracao;

public class ObterConfiguracao
{
    private readonly IConfiguracaoRepository _configuracoes;

    public ObterConfiguracao(IConfiguracaoRepository configuracoes)
    {
        _configuracoes = configuracoes;
    }

    public ConfiguracaoEstabelecimento Executar()
    {
        var nome = _configuracoes.ObterValor("estabelecimento.nome") ?? string.Empty;

        var quantidadeTexto = _configuracoes.ObterValor("comandas.quantidade_maxima");
        int? quantidade = int.TryParse(quantidadeTexto, out var quantidadeValor) ? quantidadeValor : null;
        // Preferências antigas maiores não impedem a abertura do sistema.
        // Pedidos existentes continuam consultáveis; a nova grade tem até 500 posições.
        if (quantidade > NumeracaoComandasService.LimiteQuantidade) quantidade = NumeracaoComandasService.LimiteQuantidade;

        var inicio = new NumeracaoComandasService(_configuracoes).Carregar().PrimeiroNumero;
        NumeracaoComandasService.ValidarFaixa(inicio, quantidade ?? 20);
        var pastaExterna = _configuracoes.ObterValor("backup.pasta_externa");
        if (string.IsNullOrEmpty(pastaExterna))
        {
            pastaExterna = null;
        }

        return new ConfiguracaoEstabelecimento
        {
            NomeEstabelecimento = nome,
            QuantidadeMaximaComandas = quantidade,
            NumeroInicialComandas = inicio,
            PastaBackupExterna = pastaExterna
        };
    }
}
