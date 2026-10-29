using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Xaml;

namespace VarthexComanda.Desktop.Tests.Configuracao;
public class DadosEstabelecimentoTests
{
    [Theory]
    [InlineData("61900000900","BR","(61) 9 0000-0900")]
    [InlineData("6130000900","BR","(61) 3000-0900")]
    [InlineData("12345678901","US","12345678901")]
    public void TelefoneRespeitaPaisEFixoOuCelular(string numero,string pais,string esperado)
        =>Assert.Equal(esperado,JanelaConfiguracoesSistema.FormatarTelefoneNacional(numero,pais));

    [Fact] public void DadosFormatamSalvamEAbremSemAlterarCadastroExistente()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var repo=new FakeConfiguracaoRepository();var service=new ConfiguracoesSistemaService(repo);
            service.Salvar(new(){Documento="123.456.789-09",TipoDocumento="CPF",Telefone="+55 (61) 9 0000-0900",Cep="70000-000"},"Restaurante",20,DateTime.UtcNow);
            var janela=new JanelaConfiguracoesSistema(service,new(){NomeEstabelecimento="Restaurante",QuantidadeMaximaComandas=20},()=>0,()=>[],()=>"",_=>{},()=>{},()=>null);
            var raiz=(FrameworkElement)janela.Content;
            void Arranjar(double largura){raiz.Measure(new Size(largura,700));raiz.Arrange(new Rect(0,0,largura,700));raiz.UpdateLayout();}
            Arranjar(1180);
            var filhos=AlinhamentoInterfaceTests.Filhos(raiz).ToArray();
            TextBox Campo(string nome)=>filhos.OfType<TextBox>().Single(t=>AutomationProperties.GetName(t)==nome);
            ComboBox Seletor(string nome)=>filhos.OfType<ComboBox>().Single(t=>AutomationProperties.GetName(t)==nome);
            Assert.False(janela.TemAlteracoes);Assert.Equal("CPF",Seletor("Tipo de documento").SelectedItem);
            Assert.Equal("(61) 9 0000-0900",Campo("Telefone principal").Text);
            Assert.Equal(12,filhos.OfType<Button>().Count(b=>AutomationProperties.GetName(b).StartsWith("Ajuda:")));
            Assert.Equal(248,Seletor("País e código do telefone").Items.Count);
            Campo("Telefone principal").Text="6130000900";
            Campo("CEP").Text="71000123";
            Seletor("Tipo de documento").SelectedItem="CNPJ";Campo("CPF / CNPJ").Text="12345678000190";
            Assert.Equal("12.345.678/0001-90",Campo("CPF / CNPJ").Text);
            Assert.Equal("71000-123",Campo("CEP").Text);
            filhos.OfType<Button>().Single(b=>Equals(b.Content,"Salvar alterações")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var prefs=service.Carregar();Assert.Equal("+55 (61) 3000-0900",prefs.Telefone);Assert.Equal("CNPJ",prefs.TipoDocumento);Assert.Equal("BR",prefs.PaisTelefone);Assert.Equal("71000-123",prefs.Cep);Assert.False(janela.TemAlteracoes);
            var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
            try{foreach(var escuro in new[]{false,true})foreach(var largura in new[]{1180d,800d})
            {
                TemaInterface.Aplicar(escuro,false);Arranjar(largura);
                if(pasta is null)continue;Directory.CreateDirectory(pasta);
                var bitmap=new RenderTargetBitmap((int)largura,700,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);
                var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var saida=File.Create(Path.Combine(pasta,$"dados-{(escuro?"escuro":"claro")}-{largura}.png"));png.Save(saida);
            }}finally{TemaInterface.Aplicar(false,false);}
        });
    }
}
