using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class CupomTermicoTests
{
    private static Comanda Comanda()=>new() { Id=1,Numero=12,CodigoPedido=1042,NumeroMesa=5,Status=StatusComanda.Aberta,AbertaEm=DateTime.UtcNow,TotalCentavos=5000,Observacao="Enviar juntos" };
    private static ItemComanda Item(int id=1)=>new() { Id=id,ComandaId=1,ProdutoId=1,NomeProduto="Hambúrguer da casa com nome extenso para conferir quebra de linha",Quantidade=2,PrecoUnitarioCentavos=2500,SubtotalCentavos=5000,Observacao="Sem cebola",CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow };
    private static string Texto(FlowDocument d)=>new TextRange(d.ContentStart,d.ContentEnd).Text;
    [Fact]
    public void ContaUsaCamposOpcionaisCompactosAgrupaECalculaTotalSemAlterarDados()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var prefs=new PreferenciasSistema { Documento="00.000.000/0000-00",Endereco="Rua Exemplo",Numero="123",Bairro="Centro",Cidade="Cidade",Uf="SP",RazaoSocial="NAO IMPRIMIR RAZAO",Telefone="NAO IMPRIMIR TELEFONE" };
            foreach(var mm in new[]{58,80})
            {
                var doc=CupomTermico.Conta(prefs,"Lanchonete Exemplo",Comanda(),[Item(),Item(2)],mm,12);var texto=Texto(doc);
                Assert.Contains(prefs.Documento,texto);Assert.DoesNotContain("CNPJ:",texto);Assert.DoesNotContain("NAO IMPRIMIR",texto);
                Assert.Contains("Rua Exemplo, 123 · Centro · Cidade/SP",texto);Assert.Contains("R$ 100,00",texto);Assert.Contains("SENHA DE ATENDIMENTO: 12",texto);
                Assert.Equal(1,texto.Split("Hambúrguer da casa").Length-1);Assert.Contains("Sem cebola",texto);
                Assert.Equal(CupomTermico.Largura(mm),doc.PageWidth);Assert.Contains("SISTEMA DE GESTÃO",texto);
                Assert.All(doc.Blocks.OfType<Table>(),t=>Assert.True(t.Columns[0].Width.Value>100));
                Capturar(doc,$"conta-{mm}");
            }
            prefs.ExibirDocumentoCupom=false;prefs.ExibirEnderecoCupom=false;prefs.ExibirObservacoesCupom=false;
            var simples=Texto(CupomTermico.Conta(prefs,"",Comanda(),[Item()],58));
            Assert.DoesNotContain(prefs.Documento,simples);Assert.DoesNotContain("Rua Exemplo",simples);Assert.DoesNotContain("Sem cebola",simples);
            var reduzido=CupomTermico.Conta(prefs,"Loja",Comanda(),[Item()],80);
            CupomTermico.AjustarLargura(reduzido,180);
            Assert.Equal(180,reduzido.PageWidth);Assert.All(reduzido.Blocks.OfType<Table>(),t=>Assert.InRange(t.Columns.Sum(c=>c.Width.Value),1,164.01));
        });
    }
    [Fact]
    public void CozinhaFiltraQuantidadesSemPrecosEIdentificaReimpressaoEFichas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var p=new PreferenciasSistema();var item=Item();var ignorado=Item(2);ignorado.NomeProduto="NAO ENVIADO";
            var quantidades=new Dictionary<int,decimal>{{1,1}};
            foreach(var mm in new[]{58,80})
            {
                var d=CupomTermico.Producao(p,"Loja",Comanda(),[item,ignorado],quantidades,false,mm);var texto=Texto(d);
                Assert.DoesNotContain("R$",texto);Assert.DoesNotContain("NAO ENVIADO",texto);Assert.Contains("SEM CEBOLA",texto);Assert.Contains("Enviar juntos",texto);Assert.Contains("1 UN",texto);
                Capturar(d,$"cozinha-{mm}");
                var re=CupomTermico.Producao(p,"Loja",Comanda(),[item],quantidades,true,mm);Assert.Contains("REIMPRESSÃO",Texto(re));Capturar(re,$"reimpressao-{mm}");
                var fichas=CupomTermico.Fichas(p,"Loja",Comanda(),[item],new Dictionary<int,decimal>{{1,2}},mm);
                Assert.Equal(2,Texto(fichas).Split("FICHA DE CONSUMAÇÃO").Length-1);Assert.DoesNotContain("R$",Texto(fichas));Capturar(fichas,$"fichas-{mm}");
                Assert.Equal(0,item.QuantidadeCozinha);Assert.Equal(0,item.QuantidadeFicha);
                Capturar(CupomTermico.Texto("TESTE DE IMPRESSÃO\nAcentos: pão, café, porção.\n1234567890",mm),$"teste-{mm}");
            }
        });
    }
    [Fact]
    public void LarguraLegadaContinua80EValidacaoRejeitaOutrosTamanhos()
    {
        Assert.Equal(80,System.Text.Json.JsonSerializer.Deserialize<PreferenciaImpressora>("{}")!.LarguraPapelMm);
        Assert.Throws<ArgumentException>(()=>CupomTermico.Largura(60));
        Assert.Throws<ArgumentException>(()=>ConfiguracoesImpressaoService.Validar(new PreferenciaImpressora { LarguraPapelMm=60 },false));
    }
    private static void Capturar(FlowDocument d,string nome)
    {
        var destino=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrWhiteSpace(destino))return;
        Directory.CreateDirectory(destino);
        var viewer=new FlowDocumentScrollViewer { Document=d,IsToolBarVisible=false,Width=d.PageWidth+20,Height=1100,Background=Brushes.White,VerticalScrollBarVisibility=ScrollBarVisibility.Hidden };
        viewer.Measure(new Size(viewer.Width,1100));viewer.Arrange(new Rect(0,0,viewer.Width,1100));viewer.UpdateLayout();
        viewer.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ApplicationIdle);viewer.UpdateLayout();
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(viewer.Width*2),2200,192,192,PixelFormats.Pbgra32);bitmap.Render(viewer);
        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(destino,$"cupom-{nome}.png"));png.Save(arquivo);
    }
}
