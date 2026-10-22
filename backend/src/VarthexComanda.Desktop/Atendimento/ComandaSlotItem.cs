using CommunityToolkit.Mvvm.ComponentModel;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public class ComandaSlotItem : ObservableObject
{
    public required int Numero { get; init; }
    public required bool Aberta { get; init; }
    public bool Bloqueada { get; init; }
    public bool EmFechamento => Aberta && Bloqueada;
    public string SituacaoTexto => EmFechamento ? "Em fechamento" : PrioridadeTexto;
    public int? ComandaId { get; init; }
    public string? NomeCliente { get; init; }
    public int? NumeroMesa { get; init; }
    public string MesaTexto => Aberta ? NumeroMesa is int mesa ? $"Mesa {mesa}" : "Balcão" : string.Empty;
    public required string TotalFormatado { get; init; }
    public DateTime? AbertaEm { get; init; }
    public PrioridadeComanda Prioridade { get; init; }
    public string PrioridadeTexto => Aberta ? AtendimentoViewModel.NomePrioridade(Prioridade) : "Livre";
    public string CorPrioridade => !Aberta ? "#777777" : Prioridade switch
    {
        PrioridadeComanda.Alta => "#F2CB59",
        PrioridadeComanda.Urgente => "#B6322C",
        _ => "#2D8050"
    };
    private string _tempoFormatado = string.Empty;
    public required string TempoFormatado { get => _tempoFormatado; set => SetProperty(ref _tempoFormatado, value); }

    public static string FormatarTempo(DateTime abertura, DateTime agora)
    {
        var tempo = agora > abertura ? agora - abertura : TimeSpan.Zero;
        return $"{(long)tempo.TotalHours:00}:{tempo.Minutes:00}:{tempo.Seconds:00}";
    }
}
