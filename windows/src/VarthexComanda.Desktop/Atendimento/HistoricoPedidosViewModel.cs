using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Atendimento;

namespace VarthexComanda.Desktop.Atendimento;

public partial class ColunaHistoricoPedido(string chave, string titulo, string grupo, double largura, bool inicial, Func<PedidoHistoricoResumo, object?> valor) : ObservableObject
{
    public string Chave { get; } = chave;
    public string Titulo { get; } = titulo;
    public string Grupo { get; } = grupo;
    public double Largura { get; } = largura;
    public Func<PedidoHistoricoResumo, object?> Valor { get; } = valor;
    [ObservableProperty] private bool visivel = inicial;
}

public sealed class PreferenciasHistoricoPedidos
{
    public string[]? Colunas { get; set; }
    public string[] AgruparPor { get; set; } = [];
    public bool AutoAjustar { get; set; } = true;
}

public partial class HistoricoPedidosViewModel : ObservableObject
{
    private readonly ConsultarHistoricoPedidos _consulta;
    private readonly ILogger? _logger;
    private readonly string? _arquivoPreferencias;
    public IClock Relogio { get; }
    public ObservableCollection<PedidoHistoricoResumo> Pedidos { get; } = [];
    public List<ColunaHistoricoPedido> Colunas { get; } =
    [
        new("Codigo", "Código", "Outras informações", 86, true, p => p.Codigo),
        new("DataAbertura", "Data abertura", "Outras informações", 150, true, p => p.DataAbertura),
        new("Status", "Status", "Outras informações", 125, true, p => p.Status),
        new("CriadoPor", "Criado por", "Outras informações", 130, true, p => p.CriadoPor),
        new("Cliente", "Cliente", "Dados do cliente", 150, true, p => p.Cliente),
        new("Tipo", "Tipo", "Outras informações", 85, true, p => p.Tipo),
        new("DataFechamento", "Data fechamento", "Outras informações", 150, true, p => p.DataFechamento),
        new("Total", "Total", "Totais do pedido", 105, true, p => p.Total),
        new("TotalItens", "Tot. itens", "Totais do pedido", 85, false, p => p.TotalItens),
        new("Pagamento", "Pagamento", "Pagamento", 110, false, p => p.Pagamento),
        new("NumeroComanda", "Comanda", "Outras informações", 85, false, p => p.NumeroComanda),
        new("Mesa", "Mesa", "Outras informações", 70, false, p => p.Mesa),
        new("Responsavel", "Responsável", "Outras informações", 130, false, p => p.Responsavel),
        new("Origem", "Origem", "Outras informações", 120, false, p => p.Origem),
        new("Pessoas", "Pessoas", "Outras informações", 75, false, p => p.Pessoas),
        new("Observacao", "Observação", "Outras informações", 200, false, p => p.Observacao)
    ];
    public PreferenciasHistoricoPedidos Preferencias { get; private set; } = new();
    public FiltroHistoricoPedidos Filtro { get; private set; }
    public string ResumoFiltro => Filtro.Resumo;
    public int Quantidade => Pedidos.Count;
    public long TotalCentavos => Pedidos.Sum(p => p.TotalCentavos);
    public string ResumoTotais => $"{Quantidade} pedido{(Quantidade == 1 ? "" : "s")} · Total: {CentavosParaMoedaConverter.Formatar(TotalCentavos)}";
    [ObservableProperty] private string mensagem = "";
    [ObservableProperty] private PedidoHistoricoResumo? selecionado;

    public HistoricoPedidosViewModel(ConsultarHistoricoPedidos consulta, IClock relogio, ILogger? logger = null, string? arquivoPreferencias = null)
    {
        _consulta = consulta; Relogio = relogio; _logger = logger; _arquivoPreferencias = arquivoPreferencias;
        var hoje = FusoBrasilia.ParaLocal(relogio.UtcNow).Date;
        Filtro = new() { De = hoje, Ate = hoje.AddDays(1).AddMinutes(-1) };
        try
        {
            if (arquivoPreferencias is not null && File.Exists(arquivoPreferencias))
                Preferencias = JsonSerializer.Deserialize<PreferenciasHistoricoPedidos>(File.ReadAllText(arquivoPreferencias)) ?? new();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        { _logger?.Warning(ex, "Falha ao ler preferências do histórico de pedidos"); }
        if (Preferencias.Colunas is not null)
            foreach (var coluna in Colunas) coluna.Visivel = coluna.Chave == "Codigo" || Preferencias.Colunas.Contains(coluna.Chave);
    }

    public bool Pesquisar(FiltroHistoricoPedidos filtro)
    {
        try
        {
            var resultado = _consulta.Executar(filtro);
            Filtro = filtro; Selecionado = null; Pedidos.Clear();
            foreach (var pedido in resultado) Pedidos.Add(pedido);
            Mensagem = resultado.Count == 0 ? "Nenhum pedido encontrado para os filtros selecionados." : "";
            OnPropertyChanged(nameof(ResumoFiltro)); OnPropertyChanged(nameof(Quantidade));
            OnPropertyChanged(nameof(TotalCentavos)); OnPropertyChanged(nameof(ResumoTotais));
            return true;
        }
        catch (ArgumentException ex) { Mensagem = ex.Message; return false; }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha ao pesquisar histórico de pedidos");
            Mensagem = "Não foi possível carregar os pedidos. Tente atualizar a consulta.";
            return false;
        }
    }

    public void Atualizar() => Pesquisar(Filtro);
    public ComandaComItens? BuscarDetalhes(PedidoHistoricoResumo pedido)
    {
        try
        {
            var detalhe = _consulta.BuscarDetalhes(pedido.Comanda.Id);
            Mensagem = detalhe is null ? "O pedido não foi encontrado. Atualize a consulta." : "";
            return detalhe;
        }
        catch (Exception ex) { _logger?.Error(ex, "Falha ao abrir pedido histórico"); Mensagem = "Não foi possível abrir o pedido. Tente novamente."; return null; }
    }

    public void SalvarPreferencias(IEnumerable<string> colunas, IEnumerable<string> grupos, bool autoAjustar)
    {
        var chaves = colunas.Append("Codigo").Distinct().ToArray();
        foreach (var coluna in Colunas) coluna.Visivel = chaves.Contains(coluna.Chave);
        Preferencias = new() { Colunas = chaves, AgruparPor = grupos.Where(g => g is "CriadoPor" or "Origem" or "Tipo" or "Pagamento").Distinct().ToArray(), AutoAjustar = autoAjustar };
        if (_arquivoPreferencias is null) return;
        var temporario = _arquivoPreferencias + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_arquivoPreferencias))!);
            File.WriteAllText(temporario, JsonSerializer.Serialize(Preferencias)); File.Move(temporario, _arquivoPreferencias, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { _logger?.Warning(ex, "Falha ao salvar preferências do histórico"); Mensagem = "As colunas foram aplicadas, mas não foi possível salvar a preferência neste computador."; }
    }
}
