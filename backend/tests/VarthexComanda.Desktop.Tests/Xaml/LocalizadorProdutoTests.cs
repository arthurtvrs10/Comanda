using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class LocalizadorProdutoTests
{
    [Fact]
    public void BuscaCategoriasEModosPreservamOLancamentoPersonalizado()
    {
        using var banco=new BancoDeHomologacao();
        banco.CriarProduto("Almoço executivo",4500,"Refeições");banco.CriarProduto("Suco de laranja",900,"Bebidas");
        var repo=new EfComandaRepository(banco.Fabrica);var comanda=repo.AbrirComanda(1,DateTime.UtcNow);
        var pedido=OperacoesComandaRegressaoTests.Criar(banco,repo);pedido.SelecionarComandaCommand.Execute(comanda);
        using var vm=new LocalizarProdutoViewModel(pedido);
        Assert.Equal(2,vm.Produtos.Count);
        vm.Categoria=pedido.Categorias.Single(c=>c.Nome=="Bebidas");
        Assert.Equal("Suco de laranja",vm.Produtos.Single().Nome);
        pedido.TextoBuscaCatalogo="suco";vm.TodasCommand.Execute(null);
        Assert.Equal("suco",pedido.TextoBuscaCatalogo);Assert.Single(vm.Produtos);
        vm.Fotos=true;Assert.False(vm.Lista);
        Assert.True(vm.Lancar(vm.Selecionado,3,"Sem açúcar"));
        var salvo=repo.BuscarComItens(comanda.Id)!.Itens.Single();
        Assert.Equal(3,salvo.Quantidade);Assert.Equal("Sem açúcar",salvo.Observacao);Assert.Equal(2700,salvo.SubtotalCentavos);
        vm.Lista=true;Assert.False(vm.Fotos);Assert.Single(vm.Produtos);
        pedido.TextoBuscaCatalogo="produto inexistente";Assert.True(vm.SemProdutos);Assert.Null(vm.Selecionado);
        Assert.False(vm.Lancar(null,1,null));
        pedido.TextoBuscaCatalogo="";vm.AtualizarCommand.Execute(null);Assert.Equal(2,vm.Produtos.Count);
    }

    [Fact]
    public void PrioridadeColoreNumeroEStatusEUsaToggleComPedidoEmNegrito()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try
            {
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,false);
                    var (vm,_)=CarregamentoDeXamlTests.CriarAtendimento();vm.ComandaAtual!.CodigoPedido=102;
                    var view=new AtendimentoView(vm);
                    foreach(var prioridade in Enum.GetValues<PrioridadeComanda>())
                    {
                        vm.AlterarPrioridadeCommand.Execute(prioridade);
                        Capturar(view,800,548,$"prioridade-{escuro}-{prioridade}");
                        var status=(Button)view.FindName("BotaoPrioridadePedido");
                        var cor=((SolidColorBrush)status.Background).Color;
                        Assert.Equal(prioridade switch{PrioridadeComanda.Urgente=>Color.FromRgb(182,50,44),PrioridadeComanda.Alta=>Color.FromRgb(242,203,89),_=>Color.FromRgb(45,128,80)},cor);
                        Assert.Contains(AlinhamentoInterfaceTests.Filhos(view).OfType<TextBlock>(),t=>t.Text=="#102"&&t.FontWeight==FontWeights.Bold);
                        var toggle=(CheckBox)view.FindName("BloqueioPedido");toggle.ApplyTemplate();
                        Assert.NotNull(toggle.Template.FindName("Trilho",toggle));
                        if(prioridade==PrioridadeComanda.Normal)
                        {
                            var menu=view.CriarMenuPrioridade(status);menu.ApplyTemplate();menu.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
                            Capturar(menu,Math.Ceiling(menu.DesiredSize.Width),Math.Ceiling(menu.DesiredSize.Height),$"prioridade-menu-{escuro}");
                        }
                    }
                }
            }
            finally{TemaInterface.Aplicar(false,false);}
        });
    }

    [Fact]
    public void LocalizadorMostraListaFotosEVazioNosDoisTemasETamanhos()
    {
        using var banco=new BancoDeHomologacao();
        var produto=banco.CriarProduto("Produto de teste com foto",4500,"Refeições");
        banco.CriarProduto("Almoço completo com acompanhamentos e salada da casa",3200,"Refeições");banco.CriarProduto("Suco de laranja",900,"Bebidas");
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var arquivo=Path.Combine(banco.Raiz,"imagem-controlada.png");
            var diretorioAnterior=VarthexComanda.Desktop.Catalogo.FotoArquivoParaImagemConverter.DiretorioFotos;
            VarthexComanda.Desktop.Catalogo.FotoArquivoParaImagemConverter.DiretorioFotos=banco.Raiz;
            // Padrão sintético de teste: prova carregamento da foto sem usar imagens do usuário.
            var visual=new DrawingVisual();using(var desenho=visual.RenderOpen()) {desenho.DrawRectangle(Brushes.SlateGray,null,new Rect(0,0,160,100));desenho.DrawRectangle(Brushes.LightSteelBlue,null,new Rect(12,12,62,76));desenho.DrawRectangle(Brushes.LightGray,null,new Rect(86,12,62,76));}
            var bitmap=new RenderTargetBitmap(160,100,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var stream=File.Create(arquivo))png.Save(stream);
            using(var contexto=banco.Fabrica.CreateDbContext()){contexto.Produtos.Single(p=>p.Id==produto.Id).FotoArquivo=arquivo;contexto.SaveChanges();}
            try
            {
                foreach(var escuro in new[]{false,true})
                foreach(var largura in new[]{740d,1100d})
                {
                    TemaInterface.Aplicar(escuro,false);
                    var repo=new EfComandaRepository(banco.Fabrica);var pedido=OperacoesComandaRegressaoTests.Criar(banco,repo);
                    var janela=new LocalizarProdutoView(pedido){Width=largura,Height=largura==740?520:740};
                    try
                    {
                        janela.Show();
                        janela.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.Render);
                        TemaInterface.Aplicar(escuro,false);
                        foreach(var fotos in new[]{false,true})
                        {
                            janela.ViewModel.Fotos=fotos;janela.UpdateLayout();
                            Capturar(janela,janela.ActualWidth,janela.ActualHeight,$"localizador-{escuro}-{largura}-{(fotos?"fotos":"lista")}");
                            Assert.Equal(fotos?Visibility.Collapsed:Visibility.Visible,((DataGrid)janela.FindName("TabelaLocalizador")).Visibility);
                            Assert.Equal(3,janela.ViewModel.Produtos.Count);
                            if(!fotos && largura==1100)Assert.True(((DataGrid)janela.FindName("TabelaLocalizador")).Columns[2].ActualWidth>400);
                            if(fotos)Assert.Contains(AlinhamentoInterfaceTests.Filhos(janela).OfType<Image>(),i=>i.Source is not null);
                        }
                        if(largura==740)
                        {
                            pedido.TextoBuscaCatalogo="não existe";janela.UpdateLayout();
                            Capturar(janela,janela.ActualWidth,janela.ActualHeight,$"localizador-{escuro}-vazio");
                        }
                    }
                    finally{janela.Close();}
                }
            }
            finally{VarthexComanda.Desktop.Catalogo.FotoArquivoParaImagemConverter.DiretorioFotos=diretorioAnterior;TemaInterface.Aplicar(false,false);}
        });
    }

    private static void Capturar(FrameworkElement view,double largura,double altura,string nome)
    {
        if(view is not Window){view.Measure(new Size(largura,altura));view.Arrange(new Rect(0,0,largura,altura));}view.UpdateLayout();
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrEmpty(pasta))return;
        Directory.CreateDirectory(pasta);var imagem=new RenderTargetBitmap((int)largura,(int)altura,96,96,PixelFormats.Pbgra32);imagem.Render(view);
        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(imagem));using var destino=File.Create(Path.Combine(pasta,nome+".png"));png.Save(destino);
    }
}
