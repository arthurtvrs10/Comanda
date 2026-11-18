using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Xaml;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Configuracao;
public class ImagemEstabelecimentoTests
{
    [Fact] public void ImagemSalvaComDadosPreservaLayoutEUsaMesmaImagemNaConta()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var pasta=Path.Combine(Path.GetTempPath(),"vcomanda-imagem-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(pasta);
            try
            {
                var original=new DrawingVisual();using(var dc=original.RenderOpen())dc.DrawRectangle(Brushes.DarkBlue,null,new Rect(0,0,1200,600));
                var bitmap=new RenderTargetBitmap(1200,600,96,96,PixelFormats.Pbgra32);bitmap.Render(original);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));var arquivo=Path.Combine(pasta,"foto.png");using(var stream=File.Create(arquivo))png.Save(stream);
                var imagem=JanelaConfiguracoesSistema.PrepararImagemEstabelecimento(arquivo);
                var repo=new FakeConfiguracaoRepository();var service=new ConfiguracoesSistemaService(repo);service.LayoutTicket.Salvar(new(){PapelMm=58,NomeCliente=false});
                Assert.Throws<ArgumentException>(()=>service.Salvar(new(),"",20,DateTime.UtcNow,imagem));Assert.Empty(service.LayoutTicket.Carregar()!.LogoBase64);
                service.Salvar(new(),"Restaurante",20,DateTime.UtcNow,imagem);var layout=service.LayoutTicket.Carregar()!;Assert.Equal(imagem,layout.LogoBase64);Assert.True(layout.ImprimirLogo);Assert.Equal(58,layout.PapelMm);Assert.False(layout.NomeCliente);
                var doc=CupomTermico.Conta(new(),"Restaurante",new Comanda {Id=1,Numero=1,TotalCentavos=0,Status=StatusComanda.Aberta,AbertaEm=DateTime.UtcNow},[],58,layout:layout);
                var foto=(Image)doc.Blocks.OfType<BlockUIContainer>().First().Child;Assert.Equal(600,((BitmapSource)foto.Source).PixelWidth);Assert.Equal(300,((BitmapSource)foto.Source).PixelHeight);
                var janela=new JanelaConfiguracoesSistema(service,new(){NomeEstabelecimento="",QuantidadeMaximaComandas=20},()=>0,()=>[],()=>"",_=>{},()=>{},()=>null);
                var raiz=(FrameworkElement)janela.Content;raiz.Measure(new Size(1180,700));raiz.Arrange(new Rect(0,0,1180,700));raiz.UpdateLayout();Assert.False(janela.TemAlteracoes);
                var filhos=AlinhamentoInterfaceTests.Filhos(raiz).ToArray();
                var nome=filhos.OfType<TextBox>().Single(t=>System.Windows.Automation.AutomationProperties.GetName(t)=="Nome do estabelecimento");
                Assert.True(System.Windows.Automation.AutomationProperties.GetIsRequiredForForm(nome));
                filhos.OfType<Button>().Single(b=>Equals(b.Content,"Salvar alterações")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Contains(filhos.OfType<TextBlock>(),t=>t.Text=="Preencha o nome do estabelecimento para salvar as alterações."&&t.Visibility==Visibility.Visible);
                Assert.Equal(imagem,service.LayoutTicket.Carregar()!.LogoBase64);
                Assert.Contains(filhos.OfType<TextBlock>().SelectMany(t=>t.Inlines.OfType<Run>()),r=>r.Text==" *"&&Equals(r.Foreground,Brushes.Firebrick));
                nome.Text="Restaurante";
                filhos.OfType<Button>().Single(b=>Equals(b.Content,"Remover foto")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.True(janela.TemAlteracoes);Assert.Equal(imagem,service.LayoutTicket.Carregar()!.LogoBase64);
                filhos.OfType<Button>().Single(b=>Equals(b.Content,"Salvar alterações")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Empty(service.LayoutTicket.Carregar()!.LogoBase64);Assert.False(service.LayoutTicket.Carregar()!.ImprimirLogo);Assert.False(janela.TemAlteracoes);
                var invalido=Path.Combine(pasta,"invalido.png");File.WriteAllText(invalido,"não é imagem");Assert.ThrowsAny<Exception>(()=>JanelaConfiguracoesSistema.PrepararImagemEstabelecimento(invalido));
            }
            finally{Directory.Delete(pasta,true);}
        });
    }
}




