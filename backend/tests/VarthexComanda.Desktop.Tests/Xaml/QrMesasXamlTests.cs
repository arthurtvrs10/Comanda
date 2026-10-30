using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Rede;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Tests.Xaml;
public class QrMesasXamlTests
{
    [Fact] public void MesasGeramCodigosDistintosPreviaEImpressosSemComandas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();using var banco=new BancoDeHomologacao();var config=new EfConfiguracaoRepository(banco.Fabrica);var repo=new EfComandaRepository(banco.Fabrica);var produtos=new EfProdutoRepository(banco.Fabrica);var clock=new RelogioFixo();var obter=new ObterConfiguracao(config);
            using var servidor=new ServidorLocal(new(repo,produtos,new EfCategoriaRepository(banco.Fabrica),new(repo,clock,obter),new(repo,produtos,clock),obter,configuracoes:config));servidor.IniciarAsync(0,true).GetAwaiter().GetResult();servidor.DefinirQuantidadeMesas(3);var endereco=$"http://127.0.0.1:{servidor.Porta}";
            try{foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);var janela=new JanelaQrMesas(servidor,endereco);var raiz=(FrameworkElement)janela.Content;raiz.Measure(new Size(720,720));raiz.Arrange(new Rect(0,0,720,720));raiz.UpdateLayout();
                var filhos=AlinhamentoInterfaceTests.Filhos(raiz).ToArray();var lista=Assert.Single(filhos.OfType<ListBox>());Assert.Equal(3,lista.Items.Count);lista.SelectedIndex=2;
                var imagem=Assert.Single(filhos.OfType<Image>());Assert.NotNull(imagem.Source);Assert.Contains(filhos.OfType<TextBlock>(),t=>t.Text=="Mesa 03");
                raiz.UpdateLayout();System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ContextIdle);raiz.UpdateLayout();
                var png1=JanelaQrMesas.GerarPng(servidor.LinkMesa(endereco,1));var png2=JanelaQrMesas.GerarPng(servidor.LinkMesa(endereco,2));Assert.Equal(new byte[]{137,80,78,71},png1.Take(4));Assert.False(png1.SequenceEqual(png2));
                var documento=janela.CriarDocumento(794,1123);var tabela=Assert.Single(documento.Blocks.OfType<Table>());Assert.Equal(3,tabela.RowGroups[0].Rows.Sum(r=>r.Cells.Count));Assert.Contains("MESA 03",new TextRange(documento.ContentStart,documento.ContentEnd).Text);Assert.Empty(repo.ListarAbertas());
                var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(pasta is not null){Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap(720,720,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,$"qr-mesas-{(escuro?"escuro":"claro")}.png"));encoder.Save(arquivo);}
                janela.Close();
            }}finally{TemaInterface.Aplicar(false,false);}
        });
    }
}
