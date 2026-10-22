using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using VarthexComanda.Application.Backup;

namespace VarthexComanda.Desktop.Backup;

public partial class ConectarGoogleDriveView : Window
{
    private readonly IGoogleDriveConnection _connection;
    private readonly TimeProvider _relogio;
    private long? _abriu;
    private readonly DispatcherTimer _timer = new() { Interval=TimeSpan.FromMilliseconds(100) };
    private CancellationTokenSource? _cancelamento;
    private bool _ocupado;
    private bool _concluiu;
    public bool PodeAdiar => _abriu is long inicio && _relogio.GetElapsedTime(inicio) >= TimeSpan.FromSeconds(5);
    public ConectarGoogleDriveView(IGoogleDriveConnection connection, TimeProvider? relogio = null)
    {
        InitializeComponent();_connection=connection;_relogio=relogio ?? TimeProvider.System;
        IconeDrive.Content=IconesVarthex.Criar(IconeVarthex.GoogleDrive,64);
        Importar.Visibility=connection.Configurada?Visibility.Collapsed:Visibility.Visible;
        if(!connection.Configurada)Mensagem.Text="A conexão ainda precisa ser configurada pelo responsável pelo sistema.";
        Loaded+=(_,_)=> { _abriu ??= _relogio.GetTimestamp();_timer.Start();Conectar.Focus(); };
        _timer.Tick+=(_,_)=>AtualizarContagem();
        Closed+=(_,_)=> { _timer.Stop();_cancelamento?.Cancel(); };
    }
    public void AtualizarContagem()
    {
        var segundos=_abriu is long inicio?(int)_relogio.GetElapsedTime(inicio).TotalSeconds:0;
        Depois.Content=_ocupado?"Cancelar conexão":PodeAdiar?"Configurar depois":$"Configurar depois ({Math.Max(1,5-segundos)})";
        Depois.IsEnabled=PodeAdiar;
        if(PodeAdiar&&!_ocupado)_timer.Stop();
    }
    private async void Conectar_Click(object sender,RoutedEventArgs e)
    {
        if(_ocupado)return;
        _cancelamento=new CancellationTokenSource();
        _ocupado=true;Conectar.IsEnabled=false;Importar.IsEnabled=false;
        Mensagem.Text="Conclua a autorização no navegador do Google. Estamos aguardando…";AtualizarContagem();
        try { await _connection.ConectarAsync(_cancelamento.Token);_ocupado=false;_concluiu=true;DialogResult=true; }
        catch(OperationCanceledException){Mensagem.Text="Conexão cancelada ou tempo esgotado. Você pode tentar novamente.";}
        catch(Exception ex) when(ex is InvalidOperationException or System.IO.IOException or UnauthorizedAccessException or System.Net.Http.HttpRequestException or System.Text.Json.JsonException or System.Net.Sockets.SocketException or System.ComponentModel.Win32Exception or System.Security.Cryptography.CryptographicException)
        { Mensagem.Text=ex is InvalidOperationException?ex.Message:"Não foi possível conectar. Confira sua conexão e tente novamente."; }
        finally { _cancelamento.Dispose();_cancelamento=null;_ocupado=false;Conectar.IsEnabled=true;Importar.IsEnabled=true;AtualizarContagem(); }
    }
    private void Importar_Click(object sender,RoutedEventArgs e)
    {
        var arquivo=new OpenFileDialog { Title="Selecionar credenciais OAuth do Google (aplicativo para computador)",Filter="Credenciais Google (*.json)|*.json" };
        if(arquivo.ShowDialog(this)!=true)return;
        try { _connection.ImportarCredenciais(arquivo.FileName);Importar.Visibility=Visibility.Collapsed;Mensagem.Text="Configuração pronta. Clique em Conectar com o Google."; }
        catch { Mensagem.Text="Não foi possível importar. Selecione o JSON de um cliente OAuth do tipo Aplicativo para computador."; }
    }
    private void Depois_Click(object sender,RoutedEventArgs e)
    {
        if(!PodeAdiar)return;
        if(_ocupado){_cancelamento?.Cancel();return;}
        Close();
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        if(!_concluiu&&(!PodeAdiar||_ocupado))e.Cancel=true;
        base.OnClosing(e);
    }
}
