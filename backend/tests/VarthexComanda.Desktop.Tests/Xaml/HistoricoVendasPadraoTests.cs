using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using VarthexComanda.Desktop.Atendimento;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class HistoricoVendasPadraoTests
{
    [Fact]
    public void TabelaUnicaMantemFiltrosResumoDetalhesETemasEmDuasLarguras()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try
            {
                foreach(var escuro in new[]{false,true})
                foreach(var largura in new[]{800d,1180d})
                {
                    TemaInterface.Aplicar(escuro);
                    var vm=CarregamentoDeXamlTests.CriarHistorico();var view=new HistoricoView(vm);
                    var tabela=(DataGrid)view.FindName("TabelaVendas");
                    Assert.Equal(6,tabela.Columns.Count);
                    view.Measure(new Size(largura,600));view.Arrange(new Rect(0,0,largura,600));view.UpdateLayout();
                    view.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ApplicationIdle);view.UpdateLayout();
                    Assert.Single(tabela.Items);
                    Assert.True(tabela.ActualHeight>300);Assert.True(tabela.ActualWidth>largura-30);
                    Assert.Contains("Total do dia",((TextBlock)view.FindName("ResumoDia")).Text);
                    tabela.SelectedItem=vm.Vendas.Single();
                    var detalhes=view.CriarDetalhesVenda();Assert.NotNull(detalhes);
                    Assert.Contains("Comanda 10",detalhes.Title);detalhes.Close();
                    var destino=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                    if(!string.IsNullOrWhiteSpace(destino))
                    {
                        Directory.CreateDirectory(destino);var bitmap=new RenderTargetBitmap((int)largura,600,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
                        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(destino,$"historico-vendas-{(escuro?"escuro":"claro")}-{largura}.png"));png.Save(arquivo);
                    }
                    var total=vm.TotalDiaCentavos;vm.TextoBuscaNumero="999";
                    Assert.Empty(tabela.Items);Assert.Equal(total,vm.TotalDiaCentavos);Assert.Null(vm.VendaSelecionada);
                    Assert.All(AlinhamentoInterfaceTests.Filhos(view).OfType<Button>().Where(b=>System.Windows.Automation.AutomationProperties.GetName(b) is "Exportar Excel" or "Imprimir" or "Detalhes"),b=>Assert.False(b.IsEnabled));
                }
            }
            finally { TemaInterface.Aplicar(false); }
        });
    }

    [Fact]
    public void ExportacaoEImpressaoSeguemColunasVisiveisEFiltro()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();TemaInterface.Aplicar(false);
            var vm=CarregamentoDeXamlTests.CriarHistorico();var view=new HistoricoView(vm);
            var tabela=(DataGrid)view.FindName("TabelaVendas");
            view.Measure(new Size(1180,600));view.Arrange(new Rect(0,0,1180,600));view.UpdateLayout();
            tabela.Columns[2].Visibility=Visibility.Collapsed;tabela.Columns[3].DisplayIndex=0;
            using var memoria=new MemoryStream();view.ExportarPara(memoria);memoria.Position=0;
            using(var zip=new ZipArchive(memoria,ZipArchiveMode.Read,true))
            {
                using var stream=zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();var xml=XDocument.Load(stream);
                XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                var linhas=xml.Descendants(ns+"row").ToArray();Assert.Equal(2,linhas.Length);
                Assert.Equal("Total",linhas[0].Descendants(ns+"t").First().Value);
                Assert.DoesNotContain(linhas[0].Descendants(ns+"t"),t=>t.Value=="Cliente");
                Assert.Equal("10",linhas[1].Descendants(ns+"v").First().Value);
            }
            var documento=view.CriarDocumento();Assert.Contains("Histórico de vendas",new TextRange(documento.ContentStart,documento.ContentEnd).Text);
            Assert.Equal(2,documento.Blocks.OfType<Table>().Single().RowGroups[0].Rows.Count);
            vm.TextoBuscaNumero="999";using var vazia=new MemoryStream();view.ExportarPara(vazia);vazia.Position=0;
            using var vazio=new ZipArchive(vazia,ZipArchiveMode.Read);using var sheet=vazio.GetEntry("xl/worksheets/sheet1.xml")!.Open();
            Assert.Single(XDocument.Load(sheet).Descendants(XName.Get("row","http://schemas.openxmlformats.org/spreadsheetml/2006/main")));
        });
    }
}
