using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Xaml;
namespace VarthexComanda.Desktop.Tests.Configuracao;
public class EditorCupomIntegradoTests
{
    [Fact] public void AjustesDeCupomUsamEditorCompartilhadoESalvamSemNovaJanela()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var repo=new FakeConfiguracaoRepository();var service=new ConfiguracoesSistemaService(repo);service.Salvar(new(),"Restaurante",20,DateTime.UtcNow);
            try {foreach(var escuro in new[]{false,true})foreach(var largura in new[]{1180d,800d})
            {
                TemaInterface.Aplicar(escuro,false);var janela=new JanelaConfiguracoesSistema(service,new(){NomeEstabelecimento="Restaurante",QuantidadeMaximaComandas=20},()=>0,()=>[],()=>"",_=>{},()=>{},()=>null);
                var raiz=(FrameworkElement)janela.Content;
                void Arranjar(){raiz.Measure(new Size(largura,700));raiz.Arrange(new Rect(0,0,largura,700));raiz.UpdateLayout();}
                Arranjar();AlinhamentoInterfaceTests.Filhos(raiz).OfType<RadioButton>().Single(b=>Equals(b.Content,"Ajustes de cupom")).IsChecked=true;Arranjar();
                var filhos=AlinhamentoInterfaceTests.Filhos(raiz).ToArray();Assert.Single(filhos.OfType<FlowDocumentScrollViewer>());
                var mensagem=filhos.OfType<TextBox>().Single(c=>AutomationProperties.GetName(c)=="Mensagem do rodapé");mensagem.Text="Mensagem sincronizada";
                filhos.OfType<Button>().Single(b=>Equals(b.Content,"Salvar alterações")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Equal("Mensagem sincronizada",service.Carregar().RodapeCupom);Assert.False(janela.TemAlteracoes);
                var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(pasta is not null){Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap((int)largura,700,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var saida=File.Create(Path.Combine(pasta,$"cupom-integrado-{(escuro?"escuro":"claro")}-{largura}.png"));png.Save(saida);}
            }}finally{TemaInterface.Aplicar(false,false);}
        });
    }
}
