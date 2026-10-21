using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace VarthexComanda.Desktop.Ajuda;

public partial class JanelaSuporte : Window
{
    private readonly Action<string> _abrir;
    private readonly Action<Window> _feedback;
    private readonly Action<Window> _treinamento;
    private readonly Action? _ocultarBotao;
    private readonly FrameworkElement? _ancora;
    private Window? _janelaPrincipal;
    private bool _posicionando;

    public JanelaSuporte(Action<string>? abrir = null, Action<Window>? feedback = null,
        Action<Window>? treinamento = null, Action? ocultarBotao = null, FrameworkElement? ancora = null)
    {
        InitializeComponent();
        _abrir = abrir ?? CanaisAjuda.Abrir;
        _feedback = feedback ?? (owner => new JanelaFeedback(_abrir) { Owner = owner }.ShowDialog());
        _treinamento = treinamento ?? (owner => new JanelaTreinamento { Owner = owner }.ShowDialog());
        _ocultarBotao = ocultarBotao;
        _ancora = ancora;
        Height = Math.Min(Height, Math.Max(MinHeight, SystemParameters.WorkArea.Height - 32));
        IconeWhatsapp.Content = IconesVarthex.Criar(IconeVarthex.Contato, 20);
        IconeCentral.Content = IconesVarthex.Criar(IconeVarthex.CentralAjuda, 20);
        IconeRemoto.Content = IconesVarthex.Criar(IconeVarthex.SuporteRemoto, 20);
        IconeIdeias.Content = IconesVarthex.Criar(IconeVarthex.Feedback, 20);
        IconeTutorial.Content = IconesVarthex.Criar(IconeVarthex.Treinamento, 20);
        Loaded += FixarDireita;
        Closed += (_, _) =>
        {
            if (_janelaPrincipal is null) return;
            _janelaPrincipal.LocationChanged -= AcompanharJanela;
            _janelaPrincipal.SizeChanged -= AcompanharTamanho;
            _janelaPrincipal.StateChanged -= AcompanharJanela;
            if (_ancora is not null) _ancora.LayoutUpdated -= AcompanharJanela;
            _janelaPrincipal = null;
        };
    }

    private void FixarDireita(object sender, RoutedEventArgs e)
    {
        if (_janelaPrincipal is not null || Owner is null) return;
        _janelaPrincipal = Owner;
        _janelaPrincipal.LocationChanged += AcompanharJanela;
        _janelaPrincipal.SizeChanged += AcompanharTamanho;
        _janelaPrincipal.StateChanged += AcompanharJanela;
        if (_ancora is not null) _ancora.LayoutUpdated += AcompanharJanela;
        HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(ImpedirMover);
        PosicionarDireita();
    }

    private void AcompanharJanela(object? sender, EventArgs e) => PosicionarDireita();
    private void AcompanharTamanho(object sender, SizeChangedEventArgs e) => PosicionarDireita();
    private void PosicionarDireita()
    {
        if (_posicionando || _janelaPrincipal is null || _janelaPrincipal.WindowState == WindowState.Minimized) return;
        var dono = new WindowInteropHelper(_janelaPrincipal).Handle;
        if (!GetWindowRect(dono, out var area)) return;
        var monitor = MonitorFromWindow(dono, 2);
        var info = new InformacaoMonitor { Tamanho = Marshal.SizeOf<InformacaoMonitor>() };
        if (GetMonitorInfo(monitor, ref info))
        {
            area.Esquerda = Math.Max(area.Esquerda, info.Trabalho.Esquerda);
            area.Direita = Math.Min(area.Direita, info.Trabalho.Direita);
            area.Topo = Math.Max(area.Topo, info.Trabalho.Topo);
            area.Base = Math.Min(area.Base, info.Trabalho.Base);
        }
        if (area.Direita <= area.Esquerda || area.Base <= area.Topo) return;
        var dpi = VisualTreeHelper.GetDpi(this);
        var margemX = (int)Math.Round(8 * dpi.DpiScaleX);
        var margemY = (int)Math.Round(8 * dpi.DpiScaleY);
        var intervalo = (int)Math.Round(4 * dpi.DpiScaleY);
        var canto = new Point(area.Direita - 2 * margemX, area.Base - 2 * margemY);
        var baseIcone = canto.Y;
        if (_ancora is { IsVisible: true, ActualWidth: > 0 } && PresentationSource.FromVisual(_ancora) is not null)
        {
            canto = _ancora.PointToScreen(new Point(_ancora.ActualWidth, 0));
            baseIcone = _ancora.PointToScreen(new Point(0, _ancora.ActualHeight)).Y;
        }
        var largura = Math.Min((int)Math.Round(360 * dpi.DpiScaleX), Math.Max(1, area.Direita - area.Esquerda - 2 * margemX));
        var acima = Math.Max(1, (int)Math.Round(canto.Y) - intervalo - area.Topo - margemY);
        var abaixo = Math.Max(1, area.Base - margemY - (int)Math.Round(baseIcone) - intervalo);
        var abrirAbaixo = acima < MinHeight * dpi.DpiScaleY && abaixo > acima;
        var altura = Math.Min((int)Math.Round(640 * dpi.DpiScaleY), abrirAbaixo ? abaixo : acima);
        var topo = abrirAbaixo ? (int)Math.Round(baseIcone) + intervalo : (int)Math.Round(canto.Y) - intervalo - altura;
        var esquerda = Math.Clamp((int)Math.Round(canto.X) - largura, area.Esquerda + margemX, area.Direita - margemX - largura);
        var janela = new WindowInteropHelper(this).Handle;
        if (GetWindowRect(janela, out var atual) && atual.Esquerda == esquerda && atual.Topo == topo &&
            atual.Direita - atual.Esquerda == largura && atual.Base - atual.Topo == altura) return;
        _posicionando = true;
        try
        {
            MinWidth = Math.Min(320, largura / dpi.DpiScaleX);
            MinHeight = Math.Min(360, altura / dpi.DpiScaleY);
            SetWindowPos(janela, IntPtr.Zero, esquerda,
                topo, largura, altura, 0x0010 | 0x0004);
        }
        finally { _posicionando = false; }
    }

    private static IntPtr ImpedirMover(IntPtr hwnd, int mensagem, IntPtr comando, IntPtr dados, ref bool tratado)
    {
        // Mantém a janela ancorada, inclusive ao tentar arrastar a barra de título.
        if (mensagem == 0x0112 && (comando.ToInt64() & 0xFFF0) == 0xF010) tratado = true;
        return IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Retangulo { public int Esquerda, Topo, Direita, Base; }
    [StructLayout(LayoutKind.Sequential)]
    private struct InformacaoMonitor { public int Tamanho; public Retangulo Area, Trabalho; public uint Flags; }
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr janela, out Retangulo retangulo);
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr janela, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfoW")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref InformacaoMonitor informacao);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr janela, IntPtr anterior, int x, int y, int largura, int altura, uint flags);

    private void Executar(Action acao, string erro)
    {
        try { acao(); Mensagem.Visibility = Visibility.Collapsed; }
        catch { Mensagem.Text = erro; Mensagem.Visibility = Visibility.Visible; }
    }

    private void Whatsapp_Click(object sender, RoutedEventArgs e) => Executar(
        () => _abrir(CanaisAjuda.Whatsapp("Olá! Preciso de suporte com o Varthex Comanda.")),
        "Não foi possível abrir o WhatsApp. Confira o navegador padrão e a conexão. Suporte: " + CanaisAjuda.Telefone);
    private void RustDesk_Click(object sender, RoutedEventArgs e) => Executar(
        () => _abrir(CanaisAjuda.RustDesk),
        "Não foi possível abrir o site do RustDesk. Confira o navegador padrão e a conexão.");
    private void Feedback_Click(object sender, RoutedEventArgs e) => Executar(
        () => _feedback(this), "Não foi possível abrir o feedback. Tente novamente.");
    private void Treinamento_Click(object sender, RoutedEventArgs e) => Executar(
        () => _treinamento(this), "Não foi possível abrir o treinamento. Tente novamente.");
    private void FecharOcultar_Click(object sender, RoutedEventArgs e)
    {
        _ocultarBotao?.Invoke();
        Close();
    }
}
