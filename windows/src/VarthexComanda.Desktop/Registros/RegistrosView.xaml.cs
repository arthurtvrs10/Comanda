using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VarthexComanda.Desktop.Registros;

public partial class RegistrosView : UserControl
{
    public RegistrosViewModel ViewModel { get; }
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) };
    public RegistrosView(RegistrosViewModel viewModel)
    {
        InitializeComponent(); ViewModel = viewModel; DataContext = viewModel;
        _timer.Tick += (_, _) => Atualizar();
        Loaded += (_, _) => { if (IsVisible) { Atualizar(); _timer.Start(); } };
        Unloaded += (_, _) => _timer.Stop();
        IsVisibleChanged += (_, _) => { if (IsVisible && IsLoaded) { Atualizar(); _timer.Start(); } else _timer.Stop(); };
    }
    private void Atualizar() { if (ViewModel.AtualizarCommand.CanExecute(null)) ViewModel.AtualizarCommand.Execute(null); }
}
