using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class BalcaoXamlTests
{
    [Fact]
    public void NovoPedidoReutilizaProdutosObservacoesPagamentoEFiltroNaoIncluiMesas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (atendimento,existente)=CarregamentoDeXamlTests.CriarAtendimento();
            atendimento.SalvarDadosComanda("mesa","4");atendimento.FecharEdicaoCommand.Execute(null);
            var modelo=new BalcaoViewModel(atendimento);Assert.True(modelo.Vazio);
            Comanda? aberto=null;bool novo=false;
            modelo.AbrirPedidoSolicitado+=(pedido,criado)=>{aberto=pedido;novo=criado;atendimento.PrepararPedidoBalcao(pedido.Id,criado);};
            modelo.NovoPedidoCommand.Execute(null);
            Assert.NotNull(aberto);Assert.True(novo);Assert.Null(aberto.NumeroMesa);Assert.NotEqual(existente.Id,aberto.Id);
            Assert.Single(modelo.Pedidos.Cast<Comanda>());
            Assert.True(atendimento.AdicionarProduto(atendimento.ProdutosCatalogo.First(),2,"Sem gelo"));
            Assert.Equal(1000,atendimento.ComandaAtual!.TotalCentavos);Assert.Equal("Sem gelo",atendimento.Itens.Single().Observacao);
            atendimento.NomeCliente="Érica";atendimento.SalvarNomeClienteCommand.Execute(null);
            modelo.Busca="érica";Assert.Single(modelo.Pedidos.Cast<Comanda>());
            modelo.Busca=$"#{aberto.Id}";Assert.Single(modelo.Pedidos.Cast<Comanda>());
            modelo.Busca="Não existe";Assert.True(modelo.Vazio);modelo.Busca="";
            modelo.AbrirPedidoCommand.Execute(modelo.Pedidos.Cast<Comanda>().Single());Assert.False(novo);
            atendimento.VerTotalCommand.Execute(null);
            Assert.Null(atendimento.ComandaAtual);Assert.True(modelo.Vazio);
            Assert.Equal(4,atendimento.ComandasAbertas.Single().NumeroMesa);
        });
    }

    [Fact]
    public void TelaNoWorkspaceTemDadosRolagemTemasEEstadoVazio()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();using var banco=new BancoDeHomologacao();
            var principal=CarregamentoDeXamlTests.CriarMainWindow(banco.Paths);
            var view=(BalcaoView)typeof(MainWindow).GetField("_balcaoView",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(principal)!;
            var atendimento=(AtendimentoView)typeof(MainWindow).GetField("_atendimentoView",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(principal)!;
            atendimento.ViewModel.FecharEdicaoCommand.Execute(null);
            for(var i=20;i<55;i++)atendimento.ViewModel.ComandasAbertas.Add(new(){Id=i,Numero=i,CodigoPedido=100+i,Status=StatusComanda.Aberta,AbertaEm=DateTime.UtcNow.AddMinutes(-i),TotalCentavos=i*100,NomeCliente=i==20?"Nome de cliente longo para conferir truncamento sem deslocar as ações":"Cliente "+i,Bloqueada=i==21});
            try
            {
                foreach(var escuro in new[]{false,true})
                foreach(var tamanho in new[]{new Size(1280,780),new Size(800,600)})
                {
                    TemaInterface.Aplicar(escuro,false);
                    // A view isolada é criada depois do tema para a captura não
                    // reutilizar desenhos sem uma PresentationSource conectada.
                    var previa=new BalcaoView(view.ViewModel);Medir(previa,tamanho);
                    Assert.Equal(PaletaComanda.Cor("Canvas",escuro),((SolidColorBrush)previa.Background).Color);
                    var tabela=(DataGrid)previa.FindName("TabelaPedidos");Assert.True(tabela.Items.Count>20);
                    if(tamanho.Width==1280)Assert.True(tabela.Columns[2].ActualWidth>400);
                    Assert.Contains(AlinhamentoInterfaceTests.Filhos(tabela).OfType<ScrollViewer>(),s=>s.ScrollableHeight>0);
                    Assert.Contains(AlinhamentoInterfaceTests.Filhos(previa).OfType<TextBlock>(),t=>t.Text=="#120");
                    Capturar(previa,$"balcao-{(escuro?"escuro":"claro")}-{tamanho.Width}");
                }
                var interna=(JanelaInterna)typeof(MainWindow).GetMethod("ObterJanela",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(principal,[view])!;
                var area=new AreaTrabalhoInterna();area.Abrir(interna);Medir(area,new Size(1600,900));
                Assert.Same(view,area.JanelaAtiva!.Conteudo);var antes=AreaTrabalhoInternaTests.Posicao(interna);
                AreaTrabalhoInternaTests.Mover(interna,30,-20);Assert.NotEqual(antes,AreaTrabalhoInternaTests.Posicao(interna));
                interna.AlternarMaximizar();area.UpdateLayout();Assert.Equal(area.ActualWidth,AreaTrabalhoInternaTests.Moldura(interna).ActualWidth);
                area.Fechar(interna);
                view.ViewModel.Busca="Sem resultados";var vazio=new BalcaoView(view.ViewModel);Medir(vazio,new Size(800,600));Assert.True(view.ViewModel.Vazio);Capturar(vazio,"balcao-vazio");
            }
            finally{principal.Close();TemaInterface.Aplicar(false,false);}
        });
    }

    [Fact]
    public void NovoPedidoAbreEditorRealMesmoSemAbrirJanelaDeMesas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            TemaInterface.Aplicar(false,false);
            var (atendimento,_)=CarregamentoDeXamlTests.CriarAtendimento();atendimento.FecharEdicaoCommand.Execute(null);
            var editor=new AtendimentoView(atendimento);var modelo=new BalcaoViewModel(atendimento);
            var owner=new Window{Content=new BalcaoView(modelo),Width=1100,Height=700,Opacity=0,ShowInTaskbar=false};
            Exception? falha=null;var visitou=false;
            modelo.AbrirPedidoSolicitado+=(pedido,novo)=>editor.AbrirPedidoBalcao(pedido,novo,owner);
            try
            {
                owner.Show();
                owner.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle,new Action(()=>
                {
                    var modal=owner.OwnedWindows.OfType<Window>().SingleOrDefault();
                    try
                    {
                        Assert.NotNull(modal);Assert.Contains("Pedido #",modal.Title);Assert.False(editor.IsLoaded);
                        Assert.True(atendimento.AdicionarProduto(atendimento.ProdutosCatalogo.First(),1,"Sem gelo"));
                        modal.UpdateLayout();
                        Assert.Contains(AlinhamentoInterfaceTests.Filhos(modal).OfType<TextBlock>(),t=>new System.Windows.Documents.TextRange(t.ContentStart,t.ContentEnd).Text.Contains("Refrigerante"));
                        Capturar((FrameworkElement)modal.Content,"balcao-editor");
                        Assert.Equal(500,atendimento.ComandaAtual!.TotalCentavos);visitou=true;
                    }
                    catch(Exception ex){falha=ex;}
                    finally{modal?.Close();}
                }));
                modelo.NovoPedidoCommand.Execute(null);
                Assert.Null(falha);Assert.True(visitou);Assert.Null(atendimento.ComandaAtual);
                Assert.Equal(2,modelo.Pedidos.Count);
            }
            finally{foreach(Window modal in owner.OwnedWindows)modal.Close();owner.Close();}
        });
    }
    private static void Medir(FrameworkElement view,Size tamanho)
    {
        view.Measure(tamanho);view.Arrange(new Rect(new Point(),tamanho));view.UpdateLayout();
        view.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ContextIdle);view.UpdateLayout();
    }
    private static void Capturar(FrameworkElement view,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrEmpty(pasta))return;Directory.CreateDirectory(pasta);
        var bitmap=new RenderTargetBitmap((int)view.ActualWidth,(int)view.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
}
