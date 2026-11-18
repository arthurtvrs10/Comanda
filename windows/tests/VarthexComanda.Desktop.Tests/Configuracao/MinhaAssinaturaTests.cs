using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Licenciamento;
using VarthexComanda.Desktop.Licenciamento;
using VarthexComanda.Desktop.Tests.Xaml;

namespace VarthexComanda.Desktop.Tests.Configuracao;

public class MinhaAssinaturaTests
{
    [Fact]
    public void InformacoesUsamDadosReaisDePrazoOuVitaliciaSemExporChave()
    {
        var agora=new DateTimeOffset(2026,10,20,15,0,0,TimeSpan.Zero);
        var dados=new DadosLicenca(1,"LIC-TESTE","COMPUTADOR-TESTE","Loja de teste",agora.AddDays(-1),agora.AddDays(6));
        var texto=JanelaMinhaAssinatura.MontarInformacoes(dados,agora);
        Assert.Contains("Loja de teste",texto);Assert.Contains("LIC-TESTE",texto);Assert.Contains("6 dia(s)",texto);Assert.DoesNotContain("COMPUTADOR-TESTE",texto);
        var vitalicia=JanelaMinhaAssinatura.MontarInformacoes(dados with { Vencimento=null },agora);Assert.Contains("Sem vencimento",vitalicia);Assert.DoesNotContain("dia(s)",vitalicia);
    }
    [Fact]
    public void ModalTemConsultaEAcaoDeTrocaNosDoisTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);
                var janela=new JanelaMinhaAssinatura(()=>new(1,"LIC-TESTE","COMPUTADOR-TESTE","Loja de teste",DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow.AddDays(6)));
                var raiz=(FrameworkElement)janela.Content;raiz.Measure(new Size(490,370));raiz.Arrange(new Rect(0,0,490,370));raiz.UpdateLayout();
                Assert.Equal("Minha assinatura",janela.Title);Assert.Equal(510,janela.Width);
                Assert.True(Filhos(raiz).OfType<TextBox>().Single().IsReadOnly);
                Assert.Single(Filhos(raiz).OfType<Button>(),b=>Equals(b.Content,"Trocar assinatura"));
                var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(!string.IsNullOrWhiteSpace(pasta)){Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap(490,370,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,$"minha-assinatura-{(escuro?"escuro":"claro")}.png"));encoder.Save(arquivo);}
                janela.Close();
            }
            TemaInterface.Aplicar(false,false);
        });
    }
    private static IEnumerable<DependencyObject> Filhos(DependencyObject raiz)
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(raiz);i++){var filho=VisualTreeHelper.GetChild(raiz,i);yield return filho;foreach(var neto in Filhos(filho))yield return neto;}
    }
}
