using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Reflection;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;
using Xunit.Abstractions;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class MovimentoWorkspaceTests(ITestOutputHelper saida)
{
    [Theory]
    [InlineData("_atendimentoView")]
    [InlineData("_produtosView")]
    [InlineData("_historicoPedidosView")]
    [InlineData("_historicoView")]
    [InlineData("_cozinhaView")]
    [InlineData("_balcaoView")]
    [InlineData("_backupView")]
    [InlineData("_configuracaoView")]
    [InlineData("_registrosView")]
    public void TelasReaisMovemComConteudoEmCacheEContinuamUtilizaveisAoSoltar(string campo)
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            using var banco=new BancoDeHomologacao();
            var paths=new VarthexComanda.Infrastructure.Storage.AppPaths(banco.Raiz);
            var relogio=new VarthexComanda.Application.Tests.Catalogo.FakeClock();
            var repositorio=new EfComandaRepository(banco.Fabrica);
            var produto=banco.CriarProduto("Suco",500);var comanda=repositorio.AbrirComanda(10,relogio.UtcNow);
            repositorio.AdicionarItem(comanda.Id,produto,1,relogio.UtcNow);
            var pedidos=new HistoricoPedidosView(new HistoricoPedidosViewModel(
                new(new EfHistoricoPedidosRepository(banco.Fabrica)),relogio));
            var principal=CarregamentoDeXamlTests.CriarMainWindow(paths,pedidos);
            var view=(FrameworkElement)typeof(MainWindow).GetField(campo,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(principal)!;
            if(view is CozinhaView cozinha)cozinha.Atualizar();
            if(view is HistoricoPedidosView historico)historico.ViewModel.Atualizar();
            if(view is HistoricoView vendas)vendas.ViewModel.AtualizarVendas();
            var janela=(JanelaInterna)typeof(MainWindow).GetMethod("ObterJanela",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(principal,[view])!;
            var raiz=new PainelMedido();var area=new AreaTrabalhoInterna();raiz.Children.Add(area);
            var outra=new JanelaInterna("Outra janela",new TextBox{Text="Pedido preservado"});
            area.Abrir(janela);area.Abrir(outra);area.Ativar(janela);
            raiz.Measure(new Size(1920,1000));raiz.Arrange(new Rect(0,0,1920,1000));raiz.UpdateLayout();
            if(view is CozinhaView kds)Assert.NotEmpty(kds.ViewModel.Pedidos);
            else if(view is AtendimentoView atendimento)Assert.NotEmpty(atendimento.ViewModel.Slots);
            else if(campo is "_produtosView" or "_historicoPedidosView" or "_historicoView")
                Assert.Contains(AlinhamentoInterfaceTests.Filhos(view).OfType<DataGrid>(),t=>t.Items.Count>0);
            Assert.NotNull(view.DataContext);
            var barra=AlinhamentoInterfaceTests.Filhos(janela).OfType<Thumb>().Single(t=>System.Windows.Automation.AutomationProperties.GetName(t).StartsWith("Mover janela"));
            var moldura=AreaTrabalhoInternaTests.Moldura(janela);
            var medidas=raiz.Medidas;var contexto=view.DataContext;var posicao=AreaTrabalhoInternaTests.Posicao(janela);var habilitada=view.IsEnabled;
            foreach(var cancelar in new[]{false,true})
            {
                barra.RaiseEvent(new DragStartedEventArgs(20,15){RoutedEvent=Thumb.DragStartedEvent});
                Assert.IsType<BitmapCache>(moldura.CacheMode);
                Assert.True(JanelaInterna.GetEmArraste(view));
                if(view is AtendimentoView tela)
                {
                    var notificacoes=0;
                    System.ComponentModel.PropertyChangedEventHandler observar=(_,e)=>{if(e.PropertyName==nameof(AtendimentoViewModel.TempoComandaAtual))notificacoes++;};
                    tela.ViewModel.PropertyChanged+=observar;
                    typeof(AtendimentoView).GetMethod("AtualizarCronometrosSeDisponivel",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(tela,null);
                    tela.ViewModel.PropertyChanged-=observar;
                    Assert.Equal(0,notificacoes);
                }
                var tempo=Stopwatch.StartNew();
                var transform=(TranslateTransform)moldura.RenderTransform;var alteracoes=0;
                EventHandler observarMovimento=(_,_)=>alteracoes++;
                transform.Changed+=observarMovimento;
                for(var i=0;i<2000;i++)
                {
                    AreaTrabalhoInternaTests.Mover(janela,i%2==0?20:-20,i%2==0?20:-20);
                    raiz.UpdateLayout();
                }
                Assert.Equal(0,alteracoes);
                AreaTrabalhoInternaTests.Mover(janela,40,40);
                typeof(JanelaInterna).GetMethod("RenderizarMovimento",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(janela,[null,EventArgs.Empty]);
                Assert.Equal(2,alteracoes);Assert.Equal(new Point(posicao.X+40,posicao.Y+40),AreaTrabalhoInternaTests.Posicao(janela));
                AreaTrabalhoInternaTests.Mover(janela,-40,-40);
                saida.WriteLine($"{campo}: 2002 movimentos em {tempo.Elapsed.TotalMilliseconds:F2} ms; {raiz.Medidas-medidas} novas medições.");
                barra.RaiseEvent(new DragCompletedEventArgs(0,0,cancelar){RoutedEvent=Thumb.DragCompletedEvent});
                transform.Changed-=observarMovimento;
                Assert.Null(moldura.CacheMode);Assert.Equal(medidas,raiz.Medidas);
                Assert.False(JanelaInterna.GetEmArraste(view));
                if(view is AtendimentoView retomada)
                {
                    var notificacoes=0;
                    System.ComponentModel.PropertyChangedEventHandler observar=(_,e)=>{if(e.PropertyName==nameof(AtendimentoViewModel.TempoComandaAtual))notificacoes++;};
                    retomada.ViewModel.PropertyChanged+=observar;
                    typeof(AtendimentoView).GetMethod("AtualizarCronometrosSeDisponivel",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(retomada,null);
                    retomada.ViewModel.PropertyChanged-=observar;
                    Assert.Equal(1,notificacoes);
                }
                Assert.Equal(posicao,AreaTrabalhoInternaTests.Posicao(janela));
                Assert.Same(contexto,view.DataContext);Assert.Equal(habilitada,view.IsEnabled);
                area.Ativar(outra);area.Ativar(janela);Assert.Same(janela,area.JanelaAtiva);
            }
            janela.AlternarMaximizar();raiz.UpdateLayout();janela.AlternarMaximizar();raiz.UpdateLayout();
            Assert.Equal(posicao,AreaTrabalhoInternaTests.Posicao(janela));
            area.Fechar(janela);Assert.Same(outra,area.JanelaAtiva);Assert.Null(moldura.CacheMode);
            area.Limpar();principal.Content=null;
        });
    }

    [Fact]
    public void CacheTemporarioTerminaAoMaximizarFecharOuDescarregarEAtivarDeNovoNaoRefazRecursos()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var area=new AreaTrabalhoInterna();var janela=new JanelaInterna("Produtos",new TextBox{Text="Edição preservada"});
            area.Abrir(janela);area.Measure(new Size(1600,900));area.Arrange(new Rect(0,0,1600,900));area.UpdateLayout();
            var moldura=AreaTrabalhoInternaTests.Moldura(janela);
            var barra=AlinhamentoInterfaceTests.Filhos(janela).OfType<Thumb>().Single(t=>System.Windows.Automation.AutomationProperties.GetName(t).StartsWith("Mover janela"));
            var titulo=AlinhamentoInterfaceTests.Filhos(moldura).OfType<Grid>().Single(g=>g.Height==30);
            var recurso=titulo.ReadLocalValue(Panel.BackgroundProperty);
            for(var i=0;i<100;i++)area.Ativar(janela);
            Assert.Same(recurso,titulo.ReadLocalValue(Panel.BackgroundProperty));
            var original=new BitmapCache();moldura.CacheMode=original;
            void Iniciar()=>barra.RaiseEvent(new DragStartedEventArgs(20,15){RoutedEvent=Thumb.DragStartedEvent});
            Iniciar();Assert.NotSame(original,moldura.CacheMode);
            janela.AlternarMaximizar();Assert.Same(original,moldura.CacheMode);
            Iniciar();Assert.Same(original,moldura.CacheMode);
            janela.AlternarMaximizar();Iniciar();janela.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
            Assert.Same(original,moldura.CacheMode);
            Iniciar();area.Fechar(janela);Assert.Same(original,moldura.CacheMode);
            Assert.Equal("Edição preservada",((TextBox)janela.Conteudo).Text);
        });
    }

    [Fact]
    public void ArrastarNaoRefazMedicaoDaAreaANemDeOutrasJanelas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var raiz=new PainelMedido();var area=new AreaTrabalhoInterna();raiz.Children.Add(area);
            var conteudo=new PainelMedido();conteudo.Children.Add(new TextBox{Text="Rascunho preservado"});
            var mesas=new JanelaInterna("Mesas / comandas",conteudo);var produtos=new JanelaInterna("Produtos",new Grid());
            area.Abrir(produtos);area.Abrir(mesas);
            raiz.Measure(new Size(1920,1000));raiz.Arrange(new Rect(0,0,1920,1000));raiz.UpdateLayout();
            var medidasRaiz=raiz.Medidas;var medidasConteudo=conteudo.Medidas;var relogio=Stopwatch.StartNew();
            var invalidacoes=0;
            for(var i=0;i<240;i++)
            {
                AreaTrabalhoInternaTests.Mover(mesas,i%2==0?1:-1,i%2==0?1:-1);
                if(!raiz.IsMeasureValid)invalidacoes++;
                raiz.UpdateLayout();
            }
            relogio.Stop();
            saida.WriteLine($"240 movimentos: {invalidacoes} invalidações, {raiz.Medidas-medidasRaiz} medições da área, {conteudo.Medidas-medidasConteudo} do conteúdo, {relogio.Elapsed.TotalMilliseconds:F2} ms.");
            Assert.Equal(0,invalidacoes);Assert.Equal(medidasRaiz,raiz.Medidas);Assert.Equal(medidasConteudo,conteudo.Medidas);
            Assert.Equal("Rascunho preservado",((TextBox)conteudo.Children[0]).Text);Assert.Equal(2,area.Janelas.Count);Assert.Same(mesas,area.JanelaAtiva);
        });
    }
    [Fact]
    public void MovimentoSegueImediatamenteOMouseEPreservaCliquesSemAguardarLayout()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var area=new AreaTrabalhoInterna();var janela=new JanelaInterna("Mesas / comandas",new TextBox{Text="Texto em edição"});
            area.Abrir(janela);area.Measure(new Size(1920,1000));area.Arrange(new Rect(0,0,1920,1000));area.UpdateLayout();
            var antes=AreaTrabalhoInternaTests.Posicao(janela);var tamanho=AreaTrabalhoInternaTests.Moldura(janela).RenderSize;
            for(var i=0;i<50;i++)
            {
                AreaTrabalhoInternaTests.Mover(janela,1,-1);
                Assert.Equal(new Point(antes.X+i+1,antes.Y-i-1),AreaTrabalhoInternaTests.Posicao(janela));
            }
            Assert.Equal(tamanho,AreaTrabalhoInternaTests.Moldura(janela).RenderSize);
            var ponto=AreaTrabalhoInternaTests.Moldura(janela).TranslatePoint(new Point(20,15),area);
            var alvo=System.Windows.Media.VisualTreeHelper.HitTest(area,ponto)?.VisualHit;
            while(alvo is not null&&alvo is not JanelaInterna)alvo=System.Windows.Media.VisualTreeHelper.GetParent(alvo);
            Assert.Same(janela,alvo);
            janela.AlternarMaximizar();area.UpdateLayout();var maximizada=AreaTrabalhoInternaTests.Posicao(janela);
            AreaTrabalhoInternaTests.Mover(janela,200,100);Assert.Equal(maximizada,AreaTrabalhoInternaTests.Posicao(janela));
            janela.AlternarMaximizar();area.UpdateLayout();Assert.Equal(new Point(antes.X+50,antes.Y-50),AreaTrabalhoInternaTests.Posicao(janela));
        });
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RajadaDeMovimentosAplicaSomenteUltimaPosicaoPorQuadroESoltaSemPerderDestino(bool cancelar)
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var area=new AreaTrabalhoInterna();var conteudo=new PainelMedido();
            var janela=new JanelaInterna("Mesas / comandas",conteudo);area.Abrir(janela);
            area.Measure(new Size(1920,1000));area.Arrange(new Rect(0,0,1920,1000));area.UpdateLayout();
            var barra=AlinhamentoInterfaceTests.Filhos(janela).OfType<Thumb>().Single(t=>System.Windows.Automation.AutomationProperties.GetName(t).StartsWith("Mover janela"));
            var transform=(TranslateTransform)AreaTrabalhoInternaTests.Moldura(janela).RenderTransform;
            var antes=AreaTrabalhoInternaTests.Posicao(janela);var medidas=conteudo.Medidas;var alteracoes=0;
            transform.Changed+=(_,_)=>alteracoes++;
            barra.RaiseEvent(new DragStartedEventArgs(0,0){RoutedEvent=Thumb.DragStartedEvent});
            var tempo=Stopwatch.StartNew();
            for(var i=0;i<2000;i++)AreaTrabalhoInternaTests.Mover(janela,i%2==0?40:-40,i%2==0?20:-20);
            AreaTrabalhoInternaTests.Mover(janela,100,50);
            Assert.Equal(0,alteracoes);Assert.Equal(antes,AreaTrabalhoInternaTests.Posicao(janela));
            typeof(JanelaInterna).GetMethod("RenderizarMovimento",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(janela,[null,EventArgs.Empty]);
            Assert.Equal(2,alteracoes);Assert.Equal(new Point(antes.X+100,antes.Y+50),AreaTrabalhoInternaTests.Posicao(janela));
            AreaTrabalhoInternaTests.Mover(janela,10,10);
            barra.RaiseEvent(new DragCompletedEventArgs(0,0,cancelar){RoutedEvent=Thumb.DragCompletedEvent});
            Assert.Equal(new Point(antes.X+110,antes.Y+60),AreaTrabalhoInternaTests.Posicao(janela));
            Assert.False(JanelaInterna.GetEmArraste(conteudo));Assert.Null(AreaTrabalhoInternaTests.Moldura(janela).CacheMode);
            area.UpdateLayout();Assert.Equal(medidas,conteudo.Medidas);
            saida.WriteLine($"2001 eventos agrupados em um quadro: {tempo.Elapsed.TotalMilliseconds:F2} ms; 2 alterações do transform; nenhuma nova medição.");
            area.Limpar();
        });
    }
    private sealed class PainelMedido : Grid
    {
        public int Medidas { get;private set; }
        protected override Size MeasureOverride(Size constraint){Medidas++;return base.MeasureOverride(constraint);}
    }
}
