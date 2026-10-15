using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VarthexComanda.Desktop;

public partial class InicioView : UserControl
{
    public event Action<string>? AreaSolicitada;
    public InicioView()
    {
        InitializeComponent();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += (_, _) => AtualizarHorario();
        Loaded += (_, _) => { AtualizarHorario(); timer.Start(); };
        Unloaded += (_, _) => timer.Stop();
        SizeChanged += (_, _) => Layout.MinHeight = Math.Max(560, ActualHeight - 48);
        AtualizarHorario();
    }
    private void AtualizarHorario()
    {
        var agora = DateTime.Now;
        Horario.Text = agora.ToString("HH:mm");
        DataHoje.Text = agora.ToString("dddd, dd 'de' MMMM", CultureInfo.GetCultureInfo("pt-BR"));
    }
    private void Escolher_Click(object sender, RoutedEventArgs e) => AreaSolicitada?.Invoke((string)((Button)sender).Tag);
    public void MostrarAviso(string mensagem) => Aviso.Text = mensagem;
}
