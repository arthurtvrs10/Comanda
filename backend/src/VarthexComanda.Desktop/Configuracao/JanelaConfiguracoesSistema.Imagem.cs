using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace VarthexComanda.Desktop.Configuracao;

public sealed partial class JanelaConfiguracoesSistema
{
    internal static string PrepararImagemEstabelecimento(string caminho)
    {
        var arquivo=new FileInfo(caminho);
        if(!arquivo.Exists)throw new ArgumentException("A imagem não foi encontrada. Selecione o arquivo novamente.");
        if(arquivo.Length>10*1024*1024)throw new ArgumentException("Escolha uma imagem de até 10 MB.");
        if(!new[]{".png",".jpg",".jpeg",".bmp"}.Contains(arquivo.Extension.ToLowerInvariant()))throw new ArgumentException("Selecione uma imagem JPG, PNG ou BMP.");
        using var stream=arquivo.OpenRead();
        var decoder=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);
        var original=decoder.Frames[0];
        if(original.PixelWidth>4000||original.PixelHeight>4000)throw new ArgumentException("Escolha uma imagem de até 4000 × 4000 pixels.");
        BitmapSource imagem=original;var escala=Math.Min(1,600d/Math.Max(original.PixelWidth,original.PixelHeight));
        if(escala<1)imagem=new TransformedBitmap(original,new ScaleTransform(escala,escala));
        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(imagem));using var destino=new MemoryStream();png.Save(destino);
        if(destino.Length>2*1024*1024)throw new ArgumentException("A imagem ficou muito grande. Escolha uma versão menor.");
        return Convert.ToBase64String(destino.ToArray());
    }
    private static BitmapImage LerImagemEstabelecimento(string base64)
    {
        using var stream=new MemoryStream(Convert.FromBase64String(base64));var imagem=new BitmapImage();imagem.BeginInit();imagem.CacheOption=BitmapCacheOption.OnLoad;imagem.StreamSource=stream;imagem.EndInit();imagem.Freeze();return imagem;
    }
    private void ImagemEstabelecimento()
    {
        var linha=new DockPanel {Margin=new Thickness(0,0,12,20)};
        var previa=new Border {Width=144,Height=144,BorderThickness=new Thickness(1),Margin=new Thickness(0,0,16,0),AllowDrop=true,Padding=new Thickness(8)};
        previa.SetResourceReference(Border.BackgroundProperty,"Canvas");previa.SetResourceReference(Border.BorderBrushProperty,"Line");DockPanel.SetDock(previa,Dock.Left);linha.Children.Add(previa);
        var opcoes=new StackPanel {VerticalAlignment=VerticalAlignment.Center};linha.Children.Add(opcoes);
        opcoes.Children.Add(new RotuloAjuda {Texto="Imagem do estabelecimento",FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,8)});
        var descricao=Descricao("A mesma imagem aparece no ticket da comanda. Selecione um arquivo ou arraste para o quadro.");descricao.Margin=new Thickness(0,0,0,8);opcoes.Children.Add(descricao);
        var botoes=new WrapPanel();opcoes.Children.Add(botoes);
        var selecionar=new Button {Content="Selecionar imagem…",MinHeight=32,Margin=new Thickness(0,0,8,6)};botoes.Children.Add(selecionar);
        var remover=new Button {Content="Remover foto",MinHeight=32,Margin=new Thickness(0,0,0,6)};botoes.Children.Add(remover);
        var dica=Descricao("Sugestão: 600 × 600 pixels. JPG, PNG ou BMP · até 10 MB.");dica.Margin=new Thickness(0);opcoes.Children.Add(dica);
        System.Windows.Automation.AutomationProperties.SetName(previa,"Prévia da imagem do estabelecimento");
        void AtualizarPrevia()
        {
            remover.IsEnabled=_imagemEstabelecimento.Length>0;
            if(_imagemEstabelecimento.Length==0)
            {
                var vazio=new StackPanel {VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center};
                vazio.Children.Add(IconesVarthex.Criar(IconeVarthex.Fotos,44));
                var texto=Descricao("Arraste uma imagem");texto.TextAlignment=TextAlignment.Center;texto.Margin=new Thickness(0,10,0,0);vazio.Children.Add(texto);previa.Child=vazio;return;
            }
            try{previa.Child=new Image {Source=LerImagemEstabelecimento(_imagemEstabelecimento),Stretch=Stretch.Uniform};}
            catch{previa.Child=Descricao("Não foi possível abrir a imagem. Selecione outra.");}
        }
        void Carregar(string caminho)
        {
            try{var imagem=PrepararImagemEstabelecimento(caminho);_imagemEstabelecimento=imagem;_mensagem.Text="Imagem selecionada. Salve as alterações para aplicá-la ao ticket.";AtualizarPrevia();AtualizarEstado();}
            catch(Exception ex) when(ex is IOException or ArgumentException or NotSupportedException or System.IO.FileFormatException or UnauthorizedAccessException){_mensagem.Text="Não foi possível usar a imagem. "+ex.Message;}
        }
        selecionar.Click+=(_,_)=>{var arquivo=new OpenFileDialog {Filter="Imagens JPG, PNG ou BMP|*.jpg;*.jpeg;*.png;*.bmp",Title="Imagem do estabelecimento"};if(arquivo.ShowDialog(this)==true)Carregar(arquivo.FileName);};
        remover.Click+=(_,_)=>{_imagemEstabelecimento="";_mensagem.Text="Salve as alterações para remover a imagem do estabelecimento e do ticket.";AtualizarPrevia();AtualizarEstado();};
        previa.DragOver+=(_,e)=>{e.Effects=e.Data.GetDataPresent(DataFormats.FileDrop)?DragDropEffects.Copy:DragDropEffects.None;e.Handled=true;};
        previa.Drop+=(_,e)=>{if(e.Data.GetData(DataFormats.FileDrop) is string[] arquivos&&arquivos.Length==1)Carregar(arquivos[0]);else _mensagem.Text="Arraste uma única imagem para o quadro.";e.Handled=true;};
        AtualizarPrevia();_form.Children.Add(linha);
    }
}
