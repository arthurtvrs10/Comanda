using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Application.Abstractions;
using VarthexComanda.Infrastructure.Logging;

namespace VarthexComanda.Desktop.Registros;

public partial class RegistrosViewModel(RegistroEventos registro, ConsultarLogs logs) : ObservableObject
{
    private IReadOnlyList<RegistroLog> _logs = [];
    private IReadOnlyList<EventoSistema> _eventos = [];
    public ObservableCollection<RegistroLog> Logs { get; } = [];
    public ObservableCollection<EventoSistema> Eventos { get; } = [];
    public string[] Niveis { get; } = ["Todos os níveis", "INF", "WRN", "ERR", "FTL"];
    public string[] Origens { get; } = ["Todas as origens", "Atendimento", "Cozinha", "Servidor"];
    public string[] Estados { get; } = ["Todos os estados", "Em curso", "Aguardando", "Processando", "Concluído", "Recusado", "Falha", "Cancelado"];
    [ObservableProperty] private string busca = "";
    [ObservableProperty] private string nivel = "Todos os níveis";
    [ObservableProperty] private string origem = "Todas as origens";
    [ObservableProperty] private string estado = "Todos os estados";
    [ObservableProperty] private string mensagem = "";
    [ObservableProperty] private RegistroLog? logSelecionado;
    public string Resumo => $"{_eventos.Count(e => e.Estado == EstadoEvento.Aguardando)} aguardando · {_eventos.Count(e => e.Estado == EstadoEvento.Processando)} processando · {_eventos.Count(e => e.Estado == EstadoEvento.Falha)} falha(s)";
    public string Contagem => $"{Logs.Count} registro(s) · {Eventos.Count} evento(s)";
    public bool LogsVazios => Logs.Count == 0;
    public bool EventosVazios => Eventos.Count == 0;
    public string AvisoLimite => registro.Omitidos == 0 ? "Até 500 eventos da sessão. O histórico é renovado ao fechar o sistema." : $"{registro.Omitidos} evento(s) não exibido(s) por excesso de operações simultâneas. Consulte também os logs.";
    partial void OnBuscaChanged(string value) => Filtrar();
    partial void OnNivelChanged(string value) => Filtrar();
    partial void OnOrigemChanged(string value) => Filtrar();
    partial void OnEstadoChanged(string value) => Filtrar();
    [RelayCommand]
    private async Task AtualizarAsync()
    {
        try { _logs = await Task.Run(logs.Executar); Mensagem = ""; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { Mensagem = "Não foi possível ler os logs. Tente atualizar novamente. A fila de eventos continua disponível."; }
        _eventos = registro.Consultar(); Filtrar();
        OnPropertyChanged(nameof(Resumo)); OnPropertyChanged(nameof(AvisoLimite));
    }
    private void Filtrar()
    {
        var busca = Busca.Trim();
        var selecionado = LogSelecionado;
        Substituir(Logs, _logs.Where(l => (Nivel == Niveis[0] || l.Nivel == Nivel) && (l.Mensagem.Contains(busca, StringComparison.OrdinalIgnoreCase) || l.Data.Contains(busca))));
        LogSelecionado = Logs.FirstOrDefault(l => l == selecionado) ?? Logs.FirstOrDefault();
        Substituir(Eventos, _eventos.Where(e => (Origem == Origens[0] || e.Origem.ToString() == Origem)
            && (Estado == Estados[0] || (Estado == "Em curso" ? e.EmCurso : e.Situacao == Estado))
            && (e.Operacao.Contains(busca, StringComparison.OrdinalIgnoreCase) || e.Id.ToString().Contains(busca))));
        OnPropertyChanged(nameof(Contagem)); OnPropertyChanged(nameof(LogsVazios)); OnPropertyChanged(nameof(EventosVazios));
    }
    private static void Substituir<T>(ObservableCollection<T> destino, IEnumerable<T> fonte)
    {
        var itens = fonte.ToArray(); if (destino.SequenceEqual(itens)) return;
        destino.Clear(); foreach (var item in itens) destino.Add(item);
    }
}
