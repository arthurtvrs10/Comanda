using System.IO.Compression;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class HistoricoPedidosXamlTests
{
    private static readonly DateTime Dia = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    private static HistoricoPedidosViewModel Modelo(BancoDeHomologacao banco)
    {
        var repo = new EfComandaRepository(banco.Fabrica);
        var produto = banco.CriarProduto("Suco natural", 1234);
        var aberta = repo.AbrirComanda(10, Dia); repo.AdicionarItem(aberta.Id, produto, 2, Dia);
        repo.DefinirAtendimento(aberta.Id, "Ana", "João", "Entregar os itens juntos."); repo.DefinirNomeCliente(aberta.Id, "Érica");
        var fechada = repo.AbrirComanda(11, Dia.AddMinutes(1)); repo.AdicionarItem(fechada.Id, produto, 1, Dia);
        repo.EncerrarComanda(fechada.Id, Dia.AddMinutes(15), FormaPagamento.Pix);
        var cancelada = repo.AbrirComanda(12, Dia.AddMinutes(2)); repo.CancelarComanda(cancelada.Id, Dia.AddMinutes(3));
        var modelo = new HistoricoPedidosViewModel(new(new EfHistoricoPedidosRepository(banco.Fabrica)), new RelogioFixo(Dia), arquivoPreferencias: Path.Combine(banco.Raiz, "historico-pedidos.json"));
        modelo.Atualizar(); return modelo;
    }

    [Fact]
    public void ConsultaInvalidaPreservaResultadosEPreferenciasPersistemSemAlterarDados()
    {
        using var banco = new BancoDeHomologacao(); var vm = Modelo(banco);
        Assert.Equal(2, vm.Quantidade); Assert.Equal(3702, vm.TotalCentavos);
        Assert.False(vm.Pesquisar(vm.Filtro with { Codigos = "inválido" })); Assert.Equal(2, vm.Quantidade);
        vm.SalvarPreferencias(["Cliente", "Total"], ["CriadoPor", "inexistente"], false);
        var reaberto = new HistoricoPedidosViewModel(new(new EfHistoricoPedidosRepository(banco.Fabrica)), new RelogioFixo(Dia), arquivoPreferencias: Path.Combine(banco.Raiz, "historico-pedidos.json"));
        Assert.Equal(new[] { "Codigo", "Cliente", "Total" }, reaberto.Colunas.Where(c => c.Visivel).Select(c => c.Chave));
        Assert.Equal(new[] { "CriadoPor" }, reaberto.Preferencias.AgruparPor); Assert.False(reaberto.Preferencias.AutoAjustar);
        reaberto.Atualizar(); Assert.Equal(3702, reaberto.TotalCentavos);
        Assert.Equal(2, reaberto.BuscarDetalhes(reaberto.Pedidos.Single(p => p.NumeroComanda == 10))!.Itens.Single().Quantidade);
    }

    [Fact]
    public void ExcelPreservaNumerosDatasUnicodeETextosQueParecemFormula()
    {
        ThreadingHelper.EmSta(() =>
        {
            using var banco = new BancoDeHomologacao(); var vm = Modelo(banco); var pedido = vm.Pedidos.First();
            pedido.Comanda.NomeCliente = "=HYPERLINK(\"teste\") 🧾";
            var colunas = vm.Colunas.Where(c => c.Chave is "Codigo" or "Cliente" or "Total" or "DataAbertura").ToArray();
            using var memoria = new MemoryStream(); RelatorioHistoricoPedidos.ExportarExcel(memoria, colunas, [pedido]);
            var capturas = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
            if (!string.IsNullOrWhiteSpace(capturas)) { Directory.CreateDirectory(capturas); File.WriteAllBytes(Path.Combine(capturas, "historico-exportado.xlsx"), memoria.ToArray()); }
            memoria.Position = 0; using var zip = new ZipArchive(memoria, ZipArchiveMode.Read);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            using var arquivo = zip.GetEntry("xl/worksheets/sheet1.xml")!.Open(); var xml = XDocument.Load(arquivo);
            Assert.Empty(xml.Descendants(ns + "f")); Assert.Contains(pedido.Cliente, xml.Descendants(ns + "t").Select(t => t.Value));
            Assert.Contains("12.34", xml.Descendants(ns + "v").Select(v => v.Value));
            Assert.Contains("22/10/2026 09:01", xml.Descendants(ns + "t").Select(t => t.Value));
            Assert.NotNull(zip.GetEntry("xl/styles.xml")); Assert.NotNull(zip.GetEntry("_rels/.rels"));
            var documento = RelatorioHistoricoPedidos.CriarDocumento(vm.ResumoFiltro, colunas, [pedido]);
            Assert.Single(documento.Blocks.OfType<Table>()); Assert.Equal(2, documento.Blocks.OfType<Table>().Single().RowGroups.Single().Rows.Count);
        });
    }

    [Fact]
    public void PesquisaColunasEAgrupamentoFuncionamNosDoisTemasETamanhos()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp(); using var banco = new BancoDeHomologacao(); var vm = Modelo(banco);
            try
            {
                foreach (var escuro in new[] { false, true })
                {
                    TemaInterface.Aplicar(escuro, false);
                    var view = new HistoricoPedidosView(vm); var tabela = (DataGrid)view.FindName("TabelaPedidos");
                    foreach (var largura in new[] { 1180d, 800d })
                    {
                        view.Measure(new Size(largura, 520)); view.Arrange(new Rect(0, 0, largura, 520)); view.UpdateLayout();
                        view.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle); view.UpdateLayout();
                        Assert.True(tabela.ActualHeight > 250); Assert.Equal(vm.Pedidos.Count, tabela.Items.Count);
                        Capturar(view, $"historico-{escuro}-{largura}");
                    }
                    var filtro = new JanelaFiltroHistoricoPedidos(vm.Filtro, new DateTime(2026, 10, 22));
                    filtro.Show(); filtro.UpdateLayout(); Capturar(filtro, $"historico-pesquisar-{escuro}");
                    filtro.AplicarPeriodo("Últimos 7 dias"); Assert.True(filtro.Pesquisar());
                    Assert.Equal(new DateTime(2026, 10, 16), filtro.Resultado!.De); Assert.Equal(new DateTime(2026, 10, 22, 23, 59, 0), filtro.Resultado.Ate);
                    filtro.Close();
                    var colunas = new JanelaColunasPedidos(vm); colunas.Show(); colunas.UpdateLayout(); Capturar(colunas, $"historico-colunas-{escuro}"); colunas.Close();
                    vm.SalvarPreferencias(["Codigo", "Cliente", "Total", "Status"], ["CriadoPor"], true); view.AplicarColunas(); view.UpdateLayout();
                    Assert.Single(CollectionViewSource.GetDefaultView(vm.Pedidos).GroupDescriptions);
                    Assert.Equal(4, tabela.Columns.Count(c => c.Visibility == Visibility.Visible && c.CanUserSort));
                    Capturar(view, $"historico-agrupado-{escuro}");
                    vm.Pesquisar(vm.Filtro with { Cliente = "Sem correspondência" }); view.UpdateLayout();
                    Assert.Empty(tabela.Items); Assert.Contains("Nenhum pedido", vm.Mensagem); Capturar(view, $"historico-vazio-{escuro}");
                    vm.Pesquisar(vm.Filtro with { Cliente = "" });
                    vm.SalvarPreferencias(vm.Colunas.Where(c => c.Chave is "Codigo" or "DataAbertura" or "Status" or "CriadoPor" or "Cliente" or "Tipo" or "DataFechamento" or "Total").Select(c => c.Chave), [], true);
                }
            }
            finally { TemaInterface.Aplicar(false, false); }
        });
    }

    [Fact]
    public void RibbonAbreJanelaInternaPropriaESeguePermissoesDeAtendimento()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp(); using var banco = new BancoDeHomologacao();
            var historico = new HistoricoPedidosView(Modelo(banco));
            var janela = CarregamentoDeXamlTests.CriarMainWindow(banco.Paths, historico);
            try
            {
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                Assert.DoesNotContain(AlinhamentoInterfaceTests.Filhos((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>(), b => System.Windows.Automation.AutomationProperties.GetName(b) == "Histórico de pedidos");
                ((WrapPanel)janela.FindName("CategoriasRibbon")).Children.OfType<Button>().Single(b=>Equals(b.Content,"Financeiro")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.DoesNotContain(AlinhamentoInterfaceTests.Filhos((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>(), b => System.Windows.Automation.AutomationProperties.GetName(b) == "Histórico de pedidos");
                ((WrapPanel)janela.FindName("CategoriasRibbon")).Children.OfType<Button>().Single(b=>Equals(b.Content,"Relatórios")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Button Botao() => AlinhamentoInterfaceTests.Filhos((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>().Single(b => System.Windows.Automation.AutomationProperties.GetName(b) == "Histórico de pedidos");
                var area = (AreaTrabalhoInterna)janela.FindName("ConteudoPrincipal");
                typeof(MainWindow).GetField("_bloqueado",flags)!.SetValue(janela,true);
                typeof(MainWindow).GetMethod("IrParaHistoricoPedidos", flags)!.Invoke(janela, null);
                Assert.Empty(area.Janelas);
                typeof(MainWindow).GetField("_bloqueado", flags)!.SetValue(janela, false);
                typeof(MainWindow).GetMethod("AtualizarPerfil", flags)!.Invoke(janela, null);
                Assert.True(Botao().IsEnabled); Botao().RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Same(historico, area.JanelaAtiva!.Conteudo);
                Assert.Equal("Janela interna de Histórico de pedidos", System.Windows.Automation.AutomationProperties.GetName(area.JanelaAtiva));
                typeof(MainWindow).GetField("_cozinhaAtiva", flags)!.SetValue(janela, true);
                typeof(MainWindow).GetMethod("AtualizarPerfil", flags)!.Invoke(janela, null);
                var repo = new EfComandaRepository(banco.Fabrica); repo.AbrirComanda(99, Dia);
                typeof(MainWindow).GetMethod("IrParaHistoricoPedidos", flags)!.Invoke(janela, null);
                Assert.Equal(2, historico.ViewModel.Quantidade);
            }
            finally { janela.Close(); }
        });
    }

    private static void Capturar(FrameworkElement elemento, string nome)
    {
        var pasta = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE"); if (string.IsNullOrWhiteSpace(pasta)) return;
        elemento.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle); elemento.UpdateLayout();
        Directory.CreateDirectory(pasta);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(elemento.ActualWidth), (int)Math.Ceiling(elemento.ActualHeight), 96, 96, PixelFormats.Pbgra32); bitmap.Render(elemento);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using var arquivo = File.Create(Path.Combine(pasta, nome + ".png")); png.Save(arquivo);
    }
}
