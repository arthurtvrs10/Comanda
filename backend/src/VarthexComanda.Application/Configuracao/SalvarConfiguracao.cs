using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Catalogo;

namespace VarthexComanda.Application.Configuracao;

public class SalvarConfiguracao
{
    private readonly IConfiguracaoRepository _configuracoes;
    private readonly IClock _relogio;

    private readonly Atendimento.IComandaRepository? _comandas;
    public SalvarConfiguracao(IConfiguracaoRepository configuracoes, IClock relogio, Atendimento.IComandaRepository? comandas = null)
    {
        _configuracoes = configuracoes;
        _relogio = relogio;
        _comandas = comandas;
    }

    public Resultado<ConfiguracaoEstabelecimento> Executar(ConfiguracaoEstabelecimento configuracao)
    {
        if (string.IsNullOrWhiteSpace(configuracao.NomeEstabelecimento))
        {
            return Resultado<ConfiguracaoEstabelecimento>.Falha("Informe o nome do estabelecimento.");
        }

        if (configuracao.QuantidadeMaximaComandas is int quantidade && quantidade <= 0)
        {
            return Resultado<ConfiguracaoEstabelecimento>.Falha("A quantidade de comandas deve ser maior que zero.");
        }

        if (configuracao.QuantidadeMaximaComandas is int limite && _comandas is not null)
        {
            var maior = _comandas.ListarAbertas().Select(c => c.Numero).DefaultIfEmpty(0).Max();
            if (maior > limite)
                return Resultado<ConfiguracaoEstabelecimento>.Falha($"A comanda {maior} está aberta. Encerre ou transfira o atendimento antes de reduzir para {limite}.");
        }
        var agora = _relogio.UtcNow;
        _configuracoes.Definir("estabelecimento.nome", configuracao.NomeEstabelecimento, agora);
        _configuracoes.Definir("comandas.quantidade_maxima", configuracao.QuantidadeMaximaComandas?.ToString() ?? string.Empty, agora);
        _configuracoes.Definir("backup.pasta_externa", configuracao.PastaBackupExterna ?? string.Empty, agora);

        return Resultado<ConfiguracaoEstabelecimento>.Ok(configuracao);
    }
}
