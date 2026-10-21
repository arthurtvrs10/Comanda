using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace VarthexComanda.Desktop;

/// <summary>Mantém a maximização das janelas filhas dentro do proprietário, com margem.</summary>
internal static class LimitesJanelaModal
{
    private static readonly ConditionalWeakTable<Window, object> Registradas = new();

    public static void Aplicar(Window janela)
    {
        if (janela.Owner is null || janela.ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip) ||
            Registradas.TryGetValue(janela, out _)) return;
        var fonte = HwndSource.FromHwnd(new WindowInteropHelper(janela).Handle);
        if (fonte is null) return;
        Registradas.Add(janela, new object());
        fonte.AddHook(Gancho);
        janela.Closed += (_, _) => fonte.RemoveHook(Gancho);

        IntPtr Gancho(IntPtr hwnd, int mensagem, IntPtr wParam, IntPtr lParam, ref bool tratado)
        {
            if (mensagem != 0x0024 || janela.Owner is null) return IntPtr.Zero;
            var dono = new WindowInteropHelper(janela.Owner).Handle;
            if (!GetWindowRect(dono, out var area)) return IntPtr.Zero;
            if(janela.Owner is MainWindow principal && principal.FindName("ConteudoPrincipal") is FrameworkElement trabalho && trabalho.ActualWidth>0 && trabalho.ActualHeight>0)
            {
                var inicio=trabalho.PointToScreen(new Point());
                var fim=trabalho.PointToScreen(new Point(trabalho.ActualWidth,trabalho.ActualHeight));
                area=new Retangulo { Esquerda=(int)Math.Round(inicio.X),Topo=(int)Math.Round(inicio.Y),Direita=(int)Math.Round(fim.X),Base=(int)Math.Round(fim.Y) };
            }
            var monitor = MonitorFromWindow(dono, 2);
            var info = new InformacaoMonitor { Tamanho = Marshal.SizeOf<InformacaoMonitor>() };
            if (!GetMonitorInfo(monitor, ref info)) return IntPtr.Zero;
            area.Esquerda = Math.Max(area.Esquerda, info.Trabalho.Esquerda);
            area.Direita = Math.Min(area.Direita, info.Trabalho.Direita);
            area.Topo = Math.Max(area.Topo, info.Trabalho.Topo);
            area.Base = Math.Min(area.Base, info.Trabalho.Base);
            var largura = area.Direita - area.Esquerda;
            var altura = area.Base - area.Topo;
            if (largura <= 0 || altura <= 0) return IntPtr.Zero;
            var limites = Marshal.PtrToStructure<Limites>(lParam);
            limites.PosicaoMaxima = new Ponto(area.Esquerda + (area.Direita - area.Esquerda - largura) / 2 - info.Area.Esquerda,
                area.Topo + (area.Base - area.Topo - altura) / 2 - info.Area.Topo);
            limites.TamanhoMaximo = new Ponto(largura, altura);
            limites.RastreioMaximo = new Ponto(largura, altura);
            limites.RastreioMinimo.X = Math.Min(limites.RastreioMinimo.X, largura);
            limites.RastreioMinimo.Y = Math.Min(limites.RastreioMinimo.Y, altura);
            Marshal.StructureToPtr(limites, lParam, false);
            tratado = true;
            return IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)] private struct Ponto(int x, int y) { public int X = x, Y = y; }
    [StructLayout(LayoutKind.Sequential)] private struct Retangulo { public int Esquerda, Topo, Direita, Base; }
    [StructLayout(LayoutKind.Sequential)] private struct InformacaoMonitor { public int Tamanho; public Retangulo Area, Trabalho; public uint Flags; }
    [StructLayout(LayoutKind.Sequential)] private struct Limites { public Ponto Reservado, TamanhoMaximo, PosicaoMaxima, RastreioMinimo, RastreioMaximo; }
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(IntPtr janela, out Retangulo area);
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr janela, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetMonitorInfo(IntPtr monitor, ref InformacaoMonitor informacao);
}
