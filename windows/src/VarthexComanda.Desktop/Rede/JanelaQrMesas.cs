using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QRCoder;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Rede;

public sealed class JanelaQrMesas : Window
{
    private readonly ServidorLocal _servidor;
    private readonly string _endereco;
    private readonly ListBox _mesas=new(){MinWidth=130,Margin=new Thickness(0,0,18,0)};
    private readonly TextBox _quantidade=new(){Width=90,MaxLength=3,MinHeight=36,VerticalContentAlignment=VerticalAlignment.Center};
    private readonly Image _qr=new(){Width=240,Height=240,Stretch=Stretch.Uniform};
    private readonly TextBlock _titulo=new(){FontSize=24,FontWeight=FontWeights.SemiBold,HorizontalAlignment=HorizontalAlignment.Center};
    private readonly TextBlock _erro=new(){TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,0)};
    private readonly TextBox _link=new(){IsReadOnly=true,TextWrapping=TextWrapping.Wrap,MinHeight=54,Margin=new Thickness(0,16,0,0)};
    public JanelaQrMesas(ServidorLocal servidor,string endereco)
    {
        _servidor=servidor;_endereco=endereco;
        Title="QR Codes das mesas · Chamar garçom";Width=760;Height=760;MinWidth=650;MinHeight=600;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        SetResourceReference(BackgroundProperty,"Canvas");SetResourceReference(ForegroundProperty,"Ink");
        var raiz=new DockPanel {Margin=new Thickness(20)};
        raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");raiz.SetResourceReference(TextElement.ForegroundProperty,"Ink");
        var topo=new StackPanel();DockPanel.SetDock(topo,Dock.Top);raiz.Children.Add(topo);
        topo.Children.Add(new TextBlock {Text="Um QR Code para cada mesa",FontSize=22,FontWeight=FontWeights.SemiBold});
        topo.Children.Add(new TextBlock {Text="O cliente abre somente Chamar garçom, sem comanda ou cadastro.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,16)});
        var linha=new StackPanel {Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,16)};
        var campo=new StackPanel();var rotulo=new TextBlock();rotulo.Inlines.Add(new Run("Quantidade de mesas"));rotulo.Inlines.Add(new Run(" *"){Foreground=Brushes.Firebrick});campo.Children.Add(rotulo);_quantidade.Text=servidor.QuantidadeMesas.ToString();campo.Children.Add(_quantidade);linha.Children.Add(campo);
        System.Windows.Automation.AutomationProperties.SetName(_quantidade,"Quantidade de mesas");System.Windows.Automation.AutomationProperties.SetIsRequiredForForm(_quantidade,true);
        var aplicar=new Button {Content="Aplicar quantidade",MinHeight=36,Margin=new Thickness(12,0,0,0),VerticalAlignment=VerticalAlignment.Bottom};aplicar.Click+=(_,_)=>Executar(Aplicar);linha.Children.Add(aplicar);topo.Children.Add(linha);
        var baixo=new StackPanel();DockPanel.SetDock(baixo,Dock.Bottom);raiz.Children.Add(baixo);
        baixo.Children.Add(new TextBlock {Text="Use o Wi-Fi do estabelecimento. Os códigos continuam válidos ao reiniciar; se o IP ou a porta mudar, gere novas etiquetas.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,8)});
        var acoes=new WrapPanel();var imprimir=new Button {Content="Imprimir mesas",MinHeight=38,Margin=new Thickness(0,0,8,0)};imprimir.Click+=(_,_)=>Executar(Imprimir);acoes.Children.Add(imprimir);
        var salvar=new Button {Content="Salvar todos em PNG",MinHeight=38,Margin=new Thickness(0,0,8,0)};salvar.Click+=async(_,_)=>
        {
            var pasta=new OpenFolderDialog {Title="Escolha onde salvar os QR Codes das mesas"};if(pasta.ShowDialog(this)!=true)return;
            var quantidade=_servidor.QuantidadeMesas;var destino=pasta.FolderName;
            salvar.IsEnabled=false;aplicar.IsEnabled=false;imprimir.IsEnabled=false;_erro.Text="Salvando QR Codes…";
            try{await Task.Run(()=>{for(var mesa=1;mesa<=quantidade;mesa++)File.WriteAllBytes(Path.Combine(destino,$"mesa-{mesa:000}-chamar-garcom.png"),GerarPng(_servidor.LinkMesa(_endereco,mesa)));});_erro.Text=$"{quantidade} QR Codes salvos.";}catch(Exception ex){_erro.Text=ex.Message;}finally{salvar.IsEnabled=true;aplicar.IsEnabled=true;imprimir.IsEnabled=true;}
        };acoes.Children.Add(salvar);baixo.Children.Add(acoes);baixo.Children.Add(_erro);
        var corpo=new Grid();corpo.ColumnDefinitions.Add(new(){Width=new GridLength(150)});corpo.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});raiz.Children.Add(new ScrollViewer {Content=corpo,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled});corpo.Children.Add(_mesas);
        var previa=new StackPanel {Margin=new Thickness(12),VerticalAlignment=VerticalAlignment.Center};previa.Children.Add(_titulo);var papel=new Border {Background=Brushes.White,Padding=new Thickness(12),Child=_qr,HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,12,0,0)};previa.Children.Add(papel);previa.Children.Add(new TextBlock {Text="Chamar garçom",FontSize=16,HorizontalAlignment=HorizontalAlignment.Center,Margin=new Thickness(0,8,0,0)});previa.Children.Add(_link);Grid.SetColumn(previa,1);corpo.Children.Add(previa);Content=raiz;
        RenderOptions.SetBitmapScalingMode(_qr,BitmapScalingMode.NearestNeighbor);_mesas.SelectionChanged+=(_,_)=>Executar(AtualizarPrevia);Carregar();
    }
    internal static byte[] GerarPng(string link){using var dados=QRCodeGenerator.GenerateQrCode(link,QRCodeGenerator.ECCLevel.M);using var qr=new PngByteQRCode(dados);return qr.GetGraphic(8);}
    internal static BitmapImage Imagem(string link){using var stream=new MemoryStream(GerarPng(link));var imagem=new BitmapImage();imagem.BeginInit();imagem.CacheOption=BitmapCacheOption.OnLoad;imagem.StreamSource=stream;imagem.EndInit();imagem.Freeze();return imagem;}
    private void Executar(Action acao){try{acao();}catch(Exception ex){_erro.Text=ex.Message;}}
    private void Aplicar(){if(!int.TryParse(_quantidade.Text,out var n)||n is <1 or >999)throw new ArgumentException("Informe de 1 a 999 mesas.");_servidor.DefinirQuantidadeMesas(n);Carregar();_erro.Text=$"{n} mesas configuradas. Selecione uma mesa para conferir o QR Code.";}
    private void Carregar(){_mesas.ItemsSource=Enumerable.Range(1,_servidor.QuantidadeMesas).ToArray();_mesas.SelectedIndex=0;}
    private void AtualizarPrevia(){if(_mesas.SelectedItem is not int mesa)return;var link=_servidor.LinkMesa(_endereco,mesa);_titulo.Text=$"Mesa {mesa:00}";_link.Text=link;_qr.Source=Imagem(link);System.Windows.Automation.AutomationProperties.SetName(_qr,$"QR Code para chamar garçom na mesa {mesa}");}
    private void Imprimir()
    {
        var dialogo=new PrintDialog();if(dialogo.ShowDialog()!=true)return;
        var doc=CriarDocumento(dialogo.PrintableAreaWidth,dialogo.PrintableAreaHeight);
        dialogo.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator,"QR Codes das mesas · Chamar garçom");
    }
    internal FlowDocument CriarDocumento(double largura,double altura)
    {
        var doc=new FlowDocument {FontFamily=new FontFamily("Segoe UI"),FontSize=14,PagePadding=new Thickness(24),ColumnWidth=Math.Max(200,largura-48),Foreground=Brushes.Black,Background=Brushes.White,PageWidth=largura,PageHeight=altura};
        var tabela=new Table {CellSpacing=12};tabela.Columns.Add(new());tabela.Columns.Add(new());var grupo=new TableRowGroup();tabela.RowGroups.Add(grupo);doc.Blocks.Add(tabela);
        for(var mesa=1;mesa<=_servidor.QuantidadeMesas;mesa+=2)
        {
            var linha=new TableRow();grupo.Rows.Add(linha);
            for(var coluna=0;coluna<2&&mesa+coluna<=_servidor.QuantidadeMesas;coluna++)
            {
                var numero=mesa+coluna;var celula=new TableCell {BorderBrush=Brushes.LightGray,BorderThickness=new Thickness(1),Padding=new Thickness(10)};
                celula.Blocks.Add(new Paragraph(new Run($"MESA {numero:00}")){TextAlignment=TextAlignment.Center,FontSize=22,FontWeight=FontWeights.Bold});
                celula.Blocks.Add(new BlockUIContainer(new Image {Source=Imagem(_servidor.LinkMesa(_endereco,numero)),Width=160,Height=160,Stretch=Stretch.Uniform}));
                celula.Blocks.Add(new Paragraph(new Run("Chamar garçom\nLeia com a câmera do celular\nConecte ao Wi-Fi do estabelecimento")){TextAlignment=TextAlignment.Center,FontSize=12});linha.Cells.Add(celula);
            }
        }
        return doc;
    }
}
