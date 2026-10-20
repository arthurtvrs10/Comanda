using CommunityToolkit.Mvvm.ComponentModel;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public class ComandaSlotItem : ObservableObject
{
    public required int Numero { get; init; }
    public required bool Aberta { get; init; }
    public int? ComandaId { get; init; }
    public string? NomeCliente { get; init; }
    public int? NumeroMesa { get; init; }
    public string MesaTexto => Aberta ? NumeroMesa is int mesa ? $"Mesa {mesa}" : "Balcão" : string.Empty;
    public required string TotalFormatado { get; init; }
    public DateTime? AbertaEm { get; init; }
    public PrioridadeComanda Prioridade { get; init; }
    public string PrioridadeTexto => Aberta ? Prioridade.ToString() : "Livre";
    public string CorPrioridade => !Aberta ? "#777777" : Prioridade switch
    {
        PrioridadeComanda.Alta => "#925600",
        PrioridadeComanda.Urgente => "#B71C1C",
        _ => "#28653F"
    };
    private string _tempoFormatado = string.Empty;
    public required string TempoFormatado { get => _tempoFormatado; set => SetProperty(ref _tempoFormatado, value); }

    public static string FormatarTempo(DateTime abertura, DateTime agora)
    {
        var tempo = agora > abertura ? agora - abertura : TimeSpan.Zero;
        return $"{(long)tempo.TotalHours:00}:{tempo.Minutes:00}:{tempo.Seconds:00}";
    }
}
