using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using QRCoder;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Rede;

public partial class JanelaServidor : Window
{
    public ServidorViewModel ViewModel { get; }
    private string? _linkQr;
    public JanelaServidor(ServidorLocal servidor):this(new ServidorViewModel(servidor)){}
    public JanelaServidor(ServidorViewModel modelo)
    {
        InitializeComponent();ViewModel=modelo;DataContext=modelo;
        var timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};
        timer.Tick+=(_,_)=>ViewModel.Atualizar();
        modelo.PropertyChanged+=AoAtualizar;
        Loaded+=(_,_)=>{ViewModel.Atualizar(true);AtualizarQr();timer.Start();};
        Closing+=(_,e)=>e.Cancel=ViewModel.Ocupada;
        Closed+=(_,_)=>{timer.Stop();modelo.PropertyChanged-=AoAtualizar;};
    }
    private void AoAtualizar(object? sender,PropertyChangedEventArgs e)
    {if(e.PropertyName is nameof(ViewModel.LinkAcesso) or nameof(ViewModel.EnderecoSelecionado))AtualizarQr();}
    private void AtualizarQr()
    {
        var link=ViewModel.LinkAcesso;if(link==_linkQr)return;_linkQr=link;
        Qr.Source=null;QrVazio.Visibility=Visibility.Visible;if(link is null)return;
        using var dados=QRCodeGenerator.GenerateQrCode(link,QRCodeGenerator.ECCLevel.M);
        using var codigo=new PngByteQRCode(dados);using var stream=new MemoryStream(codigo.GetGraphic(6));
        var imagem=new BitmapImage();imagem.BeginInit();imagem.CacheOption=BitmapCacheOption.OnLoad;imagem.StreamSource=stream;imagem.EndInit();imagem.Freeze();
        Qr.Source=imagem;QrVazio.Visibility=Visibility.Collapsed;
    }
    private void Paineis_SizeChanged(object sender,SizeChangedEventArgs e)
    {
        if(Informacoes is null||Conectar is null)return;
        var empilhar=e.NewSize.Width<960;
        Paineis.ColumnDefinitions[1].Width=empilhar?new GridLength(0):new GridLength(1,GridUnitType.Star);
        Grid.SetColumn(Conectar,empilhar?0:1);Grid.SetRow(Conectar,empilhar?1:0);
        Informacoes.Margin=empilhar?new Thickness(0,0,0,22):new Thickness(0,0,20,0);
    }
    private void Copiar_Click(object sender,RoutedEventArgs e)
    {
        if(ViewModel.LinkAcesso is not string link)return;
        try{Clipboard.SetText(link);ViewModel.Mensagem="Link copiado. Compartilhe somente com a equipe.";}
        catch{ViewModel.Mensagem="Não foi possível copiar. Use o QR Code para conectar.";}
    }
    private void ComoHabilitar_Click(object sender,RoutedEventArgs e) => new JanelaGuiaServidor { Owner=this }.ShowDialog();
    private void Fechar_Click(object sender,RoutedEventArgs e)=>Close();
    private void QrMesas_Click(object sender,RoutedEventArgs e)
    {
        try{ViewModel.AbrirQrMesas(this);}catch(Exception ex){ViewModel.Mensagem=ex.Message;}
    }
}
