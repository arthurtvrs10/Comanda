using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Licenciamento;
using VarthexComanda.Desktop.Licenciamento;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class AvisosInicioTests
{
    [Fact]
    public void ConviteSoAposLoginEUmaVezPorAbertura()
    {
        var aviso=new AvisosInicioSessao();
        Assert.False(aviso.TentarAgendar(false,true,false,false));
        Assert.False(aviso.TentarAgendar(true,false,true,false));
        Assert.False(aviso.TentarAgendar(true,true,false,true));
        Assert.True(aviso.TentarAgendar(true,true,false,false));
        Assert.False(aviso.TentarAgendar(true,false,true,false));
        Assert.False(aviso.TentarAgendar(true,true,false,false));
        Assert.True(new AvisosInicioSessao().TentarAgendar(true,true,false,false));
    }
    [Fact]
    public void AvisoConsideraLimiteDeSeteDiasEVitalicia()
    {
        var agora=DateTimeOffset.UtcNow;
        var dados=new DadosLicenca(1,"teste","pc","Restaurante",agora.AddDays(-20),agora.AddDays(3));
        Assert.True(JanelaAvisoVencimento.DeveAvisar(dados,agora));
        Assert.Equal(3,JanelaAvisoVencimento.DiasRestantes(dados,agora));
        Assert.True(JanelaAvisoVencimento.DeveAvisar(dados with {Vencimento=agora.AddDays(7)},agora));
        Assert.False(JanelaAvisoVencimento.DeveAvisar(dados with {Vencimento=agora.AddDays(7).AddSeconds(1)},agora));
        Assert.False(JanelaAvisoVencimento.DeveAvisar(dados with {Vencimento=null},agora));
        Assert.False(JanelaAvisoVencimento.DeveAvisar(dados with {Vencimento=agora},agora));
    }
    [Fact]
    public void AvisoAbreRascunhoWhatsappSemEnviarMensagem()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            string? url=null;
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);
                var janela=new JanelaAvisoVencimento(new(1,"lic-teste","pc","Lanchonete",DateTimeOffset.UtcNow.AddDays(-10),DateTimeOffset.UtcNow.AddDays(3)),u=>url=u);
                try
                {
                    var raiz=(FrameworkElement)janela.Content;raiz.Measure(new Size(540,600));raiz.Arrange(new Rect(0,0,540,600));raiz.UpdateLayout();
                    var painel=(Panel)((ScrollViewer)raiz).Content;
                    var contato=painel.Children.OfType<Button>().First();contato.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.StartsWith("https://wa.me/5561985532004?text=",url);Assert.Contains("lic-teste",Uri.UnescapeDataString(url!));
                    var destino=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                    if(destino is not null)
                    {
                        System.IO.Directory.CreateDirectory(destino);var bitmap=new RenderTargetBitmap(540,600,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);
                        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=System.IO.File.Create(System.IO.Path.Combine(destino,$"aviso-vencimento-{escuro}.png"));png.Save(arquivo);
                    }
                }
                finally {janela.Close();}
            }
            TemaInterface.Aplicar(false,false);
        });
    }
}
