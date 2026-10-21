using System.Text.Json;

namespace VarthexComanda.Application.Configuracao;

public enum ModoImpressora { Desativada, Instalada, Compartilhada, MesmaDoCaixa }

public sealed class PreferenciaImpressora
{
    public ModoImpressora Modo { get; set; }
    public string Caminho { get; set; } = "";
    public bool Habilitada => Modo != ModoImpressora.Desativada;
}

public sealed class PreferenciasImpressao
{
    public PreferenciaImpressora Caixa { get; set; } = new();
    public Dictionary<string, PreferenciaImpressora> Cozinhas { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Preferências de saída não fiscal. A ausência de configuração preserva a seleção manual anterior.</summary>
public sealed class ConfiguracoesImpressaoService(IConfiguracaoRepository repositorio)
{
    public const string Chave = "impressoras.preferencias.v1";
    public PreferenciasImpressao Carregar()
    {
        var json = repositorio.ObterValor(Chave);
        var p = string.IsNullOrWhiteSpace(json) ? new PreferenciasImpressao() : JsonSerializer.Deserialize<PreferenciasImpressao>(json) ?? new();
        p.Cozinhas = new(p.Cozinhas ?? [], StringComparer.OrdinalIgnoreCase);
        foreach (var local in CarregarLocais())
        {
            var mesmaDoCaixa = local.Impressora is "Mesma do caixa" or "Selecionar ao imprimir";
            if (!p.Cozinhas.TryGetValue(local.Nome, out var anterior) || anterior.Habilitada != local.Imprimir ||
                (local.Imprimir && (mesmaDoCaixa ? anterior.Modo != ModoImpressora.MesmaDoCaixa : !anterior.Caminho.Equals(local.Impressora, StringComparison.OrdinalIgnoreCase))))
                p.Cozinhas[local.Nome] = new()
                {
                    Modo = !local.Imprimir ? ModoImpressora.Desativada : mesmaDoCaixa ? ModoImpressora.MesmaDoCaixa : local.Impressora.StartsWith(@"\\") ? ModoImpressora.Compartilhada : ModoImpressora.Instalada,
                    Caminho = local.Imprimir ? local.Impressora : ""
                };
        }
        return p;
    }
    public IReadOnlyList<LocalCozinha> CarregarLocais() => new ConfiguracoesSistemaService(repositorio).Carregar().Cozinhas;
    public PreferenciaImpressora? ObterCaixaConfigurado() => string.IsNullOrWhiteSpace(repositorio.ObterValor(Chave)) ? null : Carregar().Caixa;
    public void Salvar(PreferenciasImpressao p, DateTime agora)
    {
        Validar(p.Caixa, false);
        var sistema = new ConfiguracoesSistemaService(repositorio).Carregar();
        foreach (var local in sistema.Cozinhas)
        {
            if (!p.Cozinhas.TryGetValue(local.Nome, out var saida)) continue;
            Validar(saida, true);
            local.Imprimir = saida.Habilitada;
            local.Impressora = saida.Modo == ModoImpressora.MesmaDoCaixa ? "Mesma do caixa" : saida.Caminho;
        }
        ConfiguracoesSistemaService.Validar(sistema);
        repositorio.DefinirValores(new Dictionary<string, string>
        {
            [Chave] = JsonSerializer.Serialize(p),
            [ConfiguracoesSistemaService.Chave] = JsonSerializer.Serialize(sistema)
        }, agora);
    }
    public static void Validar(PreferenciaImpressora p, bool cozinha)
    {
        if (!Enum.IsDefined(p.Modo) || (!cozinha && p.Modo == ModoImpressora.MesmaDoCaixa)) throw new ArgumentException("Selecione uma saída válida para a impressora.");
        if (p.Modo is ModoImpressora.Desativada or ModoImpressora.MesmaDoCaixa) return;
        if (string.IsNullOrWhiteSpace(p.Caminho) || p.Caminho.Length > 260 || p.Caminho.Any(char.IsControl)) throw new ArgumentException("Selecione uma impressora ou informe o compartilhamento do Windows.");
        if (p.Modo == ModoImpressora.Compartilhada && (!p.Caminho.StartsWith(@"\\") || p.Caminho[2..].Split('\\', StringSplitOptions.RemoveEmptyEntries).Length != 2)) throw new ArgumentException(@"Informe uma impressora compartilhada no formato \\computador\impressora.");
    }
}
