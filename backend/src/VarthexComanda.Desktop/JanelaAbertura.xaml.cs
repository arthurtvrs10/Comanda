using System.Diagnostics;
using System.Windows;

namespace VarthexComanda.Desktop;

/// <summary>Apresentação transitória durante a preparação do sistema, sem substituir a autenticação.</summary>
public partial class JanelaAbertura : Window
{
    private readonly TimeSpan _tempoMinimo;
    private readonly CancellationTokenSource _fechamento = new();
    private long _inicio = Stopwatch.GetTimestamp();

    public JanelaAbertura() : this(SystemParameters.WorkArea, TimeSpan.FromSeconds(5)) { }

    internal JanelaAbertura(Rect areaUtil, TimeSpan tempoMinimo)
    {
        InitializeComponent();
        var escala = Math.Clamp(Math.Min((areaUtil.Width - 32) / 760, (areaUtil.Height - 32) / 490), 0.2, 1);
        Width = 760 * escala; Height = 490 * escala;
        _tempoMinimo = tempoMinimo;
        VersaoAbertura.Text = VersaoDoAplicativo.Atual;
        ContentRendered += (_, _) => _inicio = Stopwatch.GetTimestamp();
        Closed += (_, _) => { AtividadeAbertura.IsIndeterminate = false; _fechamento.Cancel(); };
    }

    public void AtualizarEtapa(string etapa) => EtapaAbertura.Text = etapa;

    public async Task AguardarExibicaoMinimaAsync()
    {
        while (!_fechamento.IsCancellationRequested)
        {
            var restante = _tempoMinimo - Stopwatch.GetElapsedTime(_inicio);
            if (restante <= TimeSpan.Zero) return;
            // O temporizador pode arredondar milissegundos; o relógio monotônico confirma o mínimo.
            var espera = TimeSpan.FromMilliseconds(Math.Ceiling(restante.TotalMilliseconds));
            try { await Task.Delay(espera, _fechamento.Token).ConfigureAwait(false); }
            catch (OperationCanceledException) when (_fechamento.IsCancellationRequested) { return; }
        }
    }
}
