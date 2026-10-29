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
public class LayoutTicketTests
{
    [Fact] public void LayoutPersisteSeparadoSemAlterarPreferenciasDaImpressora()
    {
        var repo=new FakeConfiguracaoRepository();repo.Definir(ConfiguracoesImpressaoService.Chave,"original",DateTime.UtcNow);var servico=new LayoutTicketService(repo);
        Assert.Null(servico.Carregar());servico.Salvar(new(){NomeCliente=false,PapelMm=58});Assert.False(servico.Carregar()!.NomeCliente);Assert.Equal(58,servico.Carregar()!.PapelMm);Assert.Equal("original",repo.ObterValor(ConfiguracoesImpressaoService.Chave));
        Assert.Throws<ArgumentException>(()=>servico.Salvar(new(){ImprimirLogo=true}));Assert.Throws<ArgumentException>(()=>servico.Salvar(new(){PapelMm=72}));
    }
    [Fact] public void ContaExistenteRespeitaElementosEValoresNosDoisPapeis()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var c=new Comanda {Id=1,Numero=12,NumeroMesa=3,NomeCliente="Cliente teste",CodigoPedido=104,Status=StatusComanda.Aberta,AbertaEm=new DateTime(2026,10,29,18,0,0),TotalCentavos=500,Observacao="Pedido teste"};
            var item=new ItemComanda {Id=1,ComandaId=1,ProdutoId=1,NomeProduto="Produto teste",PrecoUnitarioCentavos=500,SubtotalCentavos=500,Quantidade=1,CriadoEm=c.AbertaEm,AtualizadoEm=c.AbertaEm,Observacao="Sem cebola"};
            foreach(var mm in new[]{58,80})
            {
                var layout=new LayoutTicket {NumeroMesa=false,NomeCliente=false,DataHora=false,ObservacoesItem=false,ObservacoesPedido=false,CodigoPedido=false,NomeEstabelecimento=false};
                var d=CupomTermico.Conta(new(),"Loja teste",c,[item],mm,layout:layout);var texto=new TextRange(d.ContentStart,d.ContentEnd).Text;
                Assert.Contains("Produto teste",texto);Assert.DoesNotContain("Cliente teste",texto);Assert.DoesNotContain("Sem cebola",texto);Assert.DoesNotContain("Loja teste",texto);Assert.DoesNotContain("MESA",texto);
                d=CupomTermico.Conta(new(),"Loja teste",c,[item],mm,layout:new());texto=new TextRange(d.ContentStart,d.ContentEnd).Text;
                Assert.Contains("Cliente teste",texto);Assert.Contains("Sem cebola",texto);Assert.Contains("COMANDA 12",texto);Assert.Contains("TOTAL: R$ 5,00",texto);Assert.Contains("CONFERÊNCIA DA CONTA",texto);Assert.Equal(CupomTermico.Largura(mm),d.PageWidth);
            }
        });
    }
    [Fact] public void TelaOfereceElementosEPreviaNosDoisTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try{foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);var janela=new JanelaLayoutTicket(new(new FakeConfiguracaoRepository()));var raiz=(FrameworkElement)janela.Content;
                raiz.Measure(new Size(1000,700));raiz.Arrange(new Rect(0,0,1000,700));raiz.UpdateLayout();
                Assert.Equal(8,AlinhamentoInterfaceTests.Filhos(raiz).OfType<CheckBox>().Count());Assert.Single(AlinhamentoInterfaceTests.Filhos(raiz).OfType<FlowDocumentScrollViewer>());
                var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(pasta is not null){Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap(1000,700,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var saida=File.Create(Path.Combine(pasta,$"ticket-layout-{(escuro?"escuro":"claro")}.png"));png.Save(saida);}
            }}finally{TemaInterface.Aplicar(false,false);}
        });
    }
}
