using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using QRCoder;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Rede;

public partial class JanelaServidor : Window
{
    private readonly ServidorLocal _servidor;
    private bool _ocupada;
    public JanelaServidor(ServidorLocal servidor)
    {
        InitializeComponent(); _servidor=servidor; Atualizar();
        Closing+=(_,e)=>e.Cancel=_ocupada;
    }
    private void Atualizar()
    {
        Estado.Text=_servidor.Ativo?"Servidor em funcionamento":"Servidor parado";
        Alternar.Content=_servidor.Ativo?"Parar servidor":"Iniciar servidor";
        Porta.IsEnabled=!_servidor.Ativo && !_ocupada;
        if(_servidor.Ativo)Porta.Text=_servidor.Porta.ToString();
        Enderecos.ItemsSource=_servidor.Enderecos;
        Enderecos.SelectedIndex=_servidor.Enderecos.Count>0?0:-1;
        Copiar.IsEnabled=_servidor.Ativo; Enderecos.IsEnabled=_servidor.Ativo;
        AtualizarQr();
    }
    private async void Alternar_Click(object sender,RoutedEventArgs e)
    {
        if(_ocupada)return;
        if(!_servidor.Ativo && (!int.TryParse(Porta.Text,out var valor) || valor is <1024 or >65535))
        { Mensagem.Text="Informe uma porta entre 1024 e 65535.";return; }
        _ocupada=true; Alternar.IsEnabled=false; Porta.IsEnabled=false;
        Mensagem.Text=_servidor.Ativo?"Parando o servidor…":"Iniciando o servidor…";
        try
        {
            if(_servidor.Ativo)await _servidor.PararAsync();else await _servidor.IniciarAsync(int.Parse(Porta.Text));
            Mensagem.Text=_servidor.Ativo?"Pronto. Leia o QR Code no celular. Fechar esta janela mantém o servidor ligado.":"Servidor parado. Os links anteriores não permitem novos lançamentos.";
        }
        catch { Mensagem.Text="Não foi possível alterar o servidor. Confira a licença e se a porta está livre; tente outra porta se necessário."; }
        finally { _ocupada=false; Alternar.IsEnabled=true; Atualizar(); }
    }
    private void Endereco_Changed(object sender,System.Windows.Controls.SelectionChangedEventArgs e)=>AtualizarQr();
    private void AtualizarQr()
    {
        if(Qr is null)return;
        Qr.Source=null; QrVazio.Visibility=Visibility.Visible;
        if(!_servidor.Ativo || Enderecos.SelectedItem is not string endereco)return;
        using var dados=QRCodeGenerator.GenerateQrCode(_servidor.LinkAcesso(endereco),QRCodeGenerator.ECCLevel.M);
        using var codigo=new PngByteQRCode(dados); using var stream=new MemoryStream(codigo.GetGraphic(6));
        var imagem=new BitmapImage();imagem.BeginInit();imagem.CacheOption=BitmapCacheOption.OnLoad;imagem.StreamSource=stream;imagem.EndInit();imagem.Freeze();
        Qr.Source=imagem;QrVazio.Visibility=Visibility.Collapsed;
    }
    private void Copiar_Click(object sender,RoutedEventArgs e)
    {
        if(Enderecos.SelectedItem is not string endereco)return;
        try { Clipboard.SetText(_servidor.LinkAcesso(endereco));Mensagem.Text="Link copiado. Compartilhe somente com a equipe."; }
        catch { Mensagem.Text="Não foi possível copiar. Use o QR Code para conectar."; }
    }
    private void Fechar_Click(object sender,RoutedEventArgs e)=>Close();
}
