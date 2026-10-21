using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VarthexComanda.Desktop;

/// <summary>Área de trabalho com janelas independentes e ordem de sobreposição.</summary>
public sealed class AreaTrabalhoInterna : Grid
{
    public JanelaInterna? JanelaAtiva { get; private set; }
    public IReadOnlyList<JanelaInterna> Janelas => Children.OfType<JanelaInterna>().ToArray();
    public event Action? JanelaAtivaAlterada;

    public AreaTrabalhoInterna()
    {
        ClipToBounds = true;
        System.Windows.Automation.AutomationProperties.SetName(this, "Área de trabalho do sistema");
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.F6 || !Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) return;
            var janelas = Janelas;
            if (janelas.Count == 0) return;
            var indice = janelas.ToList().IndexOf(JanelaAtiva!);
            var passo = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1;
            var proxima = janelas[(indice + passo + janelas.Count) % janelas.Count];
            Ativar(proxima);
            proxima.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            e.Handled = true;
        };
    }

    public void Abrir(JanelaInterna janela)
    {
        if (!Children.Contains(janela))
        {
            janela.DeslocarAbertura((Children.Count % 6) * 24);
            Children.Add(janela);
            janela.PreviewMouseDown += AoSelecionar;
            janela.GotKeyboardFocus += AoFocar;
            janela.FecharSolicitado += AoFechar;
        }
        Ativar(janela);
    }

    public void Ativar(JanelaInterna janela)
    {
        if (!Children.Contains(janela)) return;
        // Renumera para não acumular índices indefinidamente na sessão.
        var ordem = Janelas.OrderBy(GetZIndex).Where(j => j != janela).Append(janela).ToArray();
        for (var i = 0; i < ordem.Length; i++) { SetZIndex(ordem[i], i); ordem[i].DefinirAtiva(ordem[i]==janela); }
        if (ReferenceEquals(JanelaAtiva, janela)) return;
        JanelaAtiva = janela;
        JanelaAtivaAlterada?.Invoke();
    }

    public void Fechar(JanelaInterna janela)
    {
        if (!Children.Contains(janela)) return;
        janela.PreviewMouseDown -= AoSelecionar;
        janela.GotKeyboardFocus -= AoFocar;
        janela.FecharSolicitado -= AoFechar;
        Children.Remove(janela);
        if (!ReferenceEquals(JanelaAtiva, janela)) return;
        JanelaAtiva = Janelas.OrderByDescending(GetZIndex).FirstOrDefault();
        janela.DefinirAtiva(false);
        JanelaAtiva?.DefinirAtiva(true);
        JanelaAtivaAlterada?.Invoke();
        JanelaAtiva?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
    }

    public void Limpar()
    {
        foreach (var janela in Janelas) Fechar(janela);
    }

    private void AoSelecionar(object sender, MouseButtonEventArgs e) => Ativar((JanelaInterna)sender);
    private void AoFocar(object sender, KeyboardFocusChangedEventArgs e) => Ativar((JanelaInterna)sender);
    private void AoFechar(JanelaInterna janela) => Fechar(janela);
}
