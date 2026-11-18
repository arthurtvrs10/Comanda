using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VarthexComanda.Desktop.Backup;

public partial class ProgressoBackupView : UserControl
{
    public BackupViewModel ViewModel { get; }
    public ProgressoBackupView(BackupViewModel vm)
    {
        InitializeComponent(); ViewModel=vm;DataContext=vm;
        Loaded+=(_,_)=>{vm.Progresso.CollectionChanged+=AtualizarRolagem;vm.PropertyChanged+=AtualizarEstado;AtualizarEstado(this,new(nameof(vm.EmAndamento)));};
        Unloaded+=(_,_)=>{vm.Progresso.CollectionChanged-=AtualizarRolagem;vm.PropertyChanged-=AtualizarEstado;PontosProgresso.BeginAnimation(OpacityProperty,null);};
    }
    private void AtualizarRolagem(object? sender,NotifyCollectionChangedEventArgs e)=>Dispatcher.BeginInvoke(new Action(()=>RolagemProgresso.ScrollToEnd()));
    private void AtualizarEstado(object? sender,PropertyChangedEventArgs e)
    {
        if(e.PropertyName!=nameof(BackupViewModel.EmAndamento))return;
        PontosProgresso.BeginAnimation(OpacityProperty,ViewModel.EmAndamento&&SystemParameters.ClientAreaAnimation
            ?new DoubleAnimation(0.35,1,TimeSpan.FromSeconds(0.65)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever}:null);
    }
    private void Fechar_Click(object sender,RoutedEventArgs e){if(!ViewModel.EmAndamento)Window.GetWindow(this)?.Close();}
}

public sealed class JanelaProgressoBackup : Window
{
    public ProgressoBackupView Progresso { get; }
    public Task Operacao { get; private set; } = Task.CompletedTask;
    private bool _iniciou;
    public JanelaProgressoBackup(BackupViewModel vm,string? pastaExterna=null)
    {
        Title="Progresso do backup";Width=660;Height=510;MinWidth=460;MinHeight=340;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;ShowInTaskbar=false;
        SetResourceReference(BackgroundProperty,"AjudaCanvas");
        Progresso=new(vm);Content=Progresso;
        Closing+=(_,e)=>{if(!_iniciou||vm.EmAndamento)e.Cancel=true;};
        Loaded+=(_,_)=>{if(_iniciou)return;_iniciou=true;Operacao=vm.CriarBackupAsync(pastaExterna);};
        Closed+=(_,_)=>vm.VoltarConfiguracao();
    }
}
