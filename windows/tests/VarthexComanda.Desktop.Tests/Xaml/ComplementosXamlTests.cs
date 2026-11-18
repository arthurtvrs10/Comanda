using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Catalogo;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class ComplementosXamlTests
{
    [Fact]
    public void CatalogoComplementosReutilizaListaEFormularioDeProdutosComVinculoSeparado()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var produtos=CarregamentoDeXamlTests.CriarProdutos();var catalogo=produtos.CriarCatalogoComplementos();
            catalogo.NovoCommand.Execute(null);catalogo.NomeProduto="Bacon";catalogo.PrecoProdutoReais="3,50";catalogo.QuantidadeEstoqueComplemento="12";
            catalogo.MinimoComplemento="1";catalogo.MaximoComplemento="3";
            catalogo.SalvarCommand.Execute(null);Assert.Empty(catalogo.Mensagem);
            var bacon=Assert.Single(catalogo.Produtos);Assert.True(bacon.EhComplemento);
            Assert.Equal(1,bacon.MinimoComplemento);Assert.Equal(3,bacon.MaximoComplemento);
            foreach(var escuro in new[] { false,true })
            {
                TemaInterface.Aplicar(escuro,false);
                var view=new ProdutosView(catalogo);Organizar(view,1100,650);
                Assert.Null(view.FindName("BotaoVincularComplementos"));
                Assert.Equal(Visibility.Collapsed,((FrameworkElement)view.FindName("GrupoComplementosProduto")).Visibility);
                Assert.Equal("Complementos (1)",catalogo.TituloLista);
                Assert.Equal(Visibility.Collapsed,((FrameworkElement)view.FindName("GrupoCategoriaEMedida")).Visibility);
                Assert.Equal(Visibility.Visible,((FrameworkElement)view.FindName("GrupoEstoqueComplemento")).Visibility);
                Assert.DoesNotContain(((DataGrid)view.FindName("TabelaProdutos")).Columns,c=>Equals(c.Header,"Categoria")||Equals(c.Header,"Medida"));
                Assert.Contains(((DataGrid)view.FindName("TabelaProdutos")).Columns,c=>Equals(c.Header,"Estoque disponível"));
                Assert.Null(bacon.CategoriaId);Assert.Equal(12,bacon.QuantidadeEstoque);
                Assert.Same(catalogo.Produtos,((DataGrid)view.FindName("TabelaProdutos")).ItemsSource);
                Capturar(view,$"catalogo-complementos-{escuro}");
                catalogo.ProdutoSelecionado=bacon;
                var camada=(Grid)view.FindName("ModalProduto");var formulario=(Border)camada.Children[0];camada.Children.Remove(formulario);formulario.DataContext=catalogo;
                formulario.Width=790;formulario.Height=650;formulario.Margin=new Thickness(0);formulario.HorizontalAlignment=HorizontalAlignment.Left;formulario.VerticalAlignment=VerticalAlignment.Top;
                var host=new Grid { Width=790,Height=650 };host.Children.Add(formulario);Organizar(host,790,650);
                Assert.DoesNotContain(Descendentes<Button>(formulario),b=>b.IsVisible&&Equals(b.Content,"Configurar complementos…"));
                Capturar(host,$"cadastro-complemento-{escuro}");
                var vinculos=new JanelaComplementosProduto([],catalogo.ComplementosParaVinculo());var raiz=(FrameworkElement)vinculos.Content;Organizar(raiz,900,590);
                Descendentes<Button>(raiz).Single(b=>Equals(b.Content,"Adicionar grupo")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));raiz.UpdateLayout();
                Descendentes<ComboBox>(raiz).Single().SelectedItem=bacon;
                Descendentes<Button>(raiz).Single(b=>Equals(b.Content,"Adicionar opção")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));raiz.UpdateLayout();
                var tabela=Descendentes<DataGrid>(raiz).Single();Assert.True(tabela.IsReadOnly);
                Assert.Equal(bacon.Id,((OpcaoComplementoEdicao)Assert.Single(tabela.Items.Cast<object>())).ComplementoId);
                Capturar(raiz,$"vinculos-complementos-{escuro}");vinculos.Close();
            }
            TemaInterface.Aplicar(false,false);
        });
    }

    [Fact]
    public void EditorIsolaRascunhoESelecaoAtualizaTotalEObrigatoriedadeNosDoisTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var queijo=new OpcaoComplemento { Nome="Queijo extra",PrecoCentavos=250 };
            var bacon=new OpcaoComplemento { Nome="Bacon",PrecoCentavos=350 };
            var grupos=new List<GrupoComplementos> { new() { Nome="Adicionais",Minimo=1,Maximo=1,Opcoes=[queijo,bacon] } };
            using var banco=new BancoDeHomologacao();
            var produto=banco.CriarProduto("Hambúrguer",1500);produto.GruposComplementos=grupos;
            new EfProdutoRepository(banco.Fabrica).Salvar(produto);
            var pedido=OperacoesComandaRegressaoTests.Criar(banco,new EfComandaRepository(banco.Fabrica));
            using var catalogo=new LocalizarProdutoViewModel(pedido);
            foreach(var escuro in new[] { false,true })
            {
                TemaInterface.Aplicar(escuro,false);
                var editor=new JanelaComplementosProduto(grupos);
                var raiz=(FrameworkElement)editor.Content;Organizar(raiz,900,590);
                var campo=Descendentes<TextBox>(raiz).First();campo.Text="Rascunho";
                Assert.Equal("Adicionais",produto.GruposComplementos[0].Nome);
                Capturar(raiz,$"complementos-cadastro-{escuro}");editor.Close();
                var janela=new PersonalizarItemView(catalogo,new(produto,"Lanches"));janela.ViewModel.Quantidade="2";
                raiz=(FrameworkElement)janela.Content;Organizar(raiz,860,560);
                var confirmar=Descendentes<Button>(raiz).Single(b=>Equals(b.Content,"Adicionar item"));
                Assert.False(confirmar.IsEnabled);
                var opcoes=janela.ViewModel.GruposComplementos.Single().Opcoes;
                Assert.False(opcoes[0].DiminuirCommand.CanExecute(null));Assert.True(opcoes[0].AumentarCommand.CanExecute(null));
                opcoes[0].AumentarCommand.Execute(null);raiz.UpdateLayout();
                Assert.True(confirmar.IsEnabled);Assert.Contains(Descendentes<TextBlock>(raiz),t=>t.Text.Contains("35,00"));
                Assert.Equal(1,opcoes[0].QuantidadeSelecionada);Assert.False(opcoes[1].AumentarCommand.CanExecute(null));
                opcoes[0].DiminuirCommand.Execute(null);Assert.Equal(0,opcoes[0].QuantidadeSelecionada);
                Assert.True(opcoes[1].AumentarCommand.CanExecute(null));opcoes[1].AumentarCommand.Execute(null);raiz.UpdateLayout();
                Assert.False(opcoes[0].Selecionado);Assert.True(confirmar.IsEnabled);
                Capturar(raiz,$"complementos-selecao-{escuro}");janela.Close();
            }
            TemaInterface.Aplicar(false,false);
        });
    }
    [Fact]
    public void PersonalizacaoVerificaSaldoPorQuantidadeEReconsultaAntesDeAdicionar()
    {
        ThreadingHelper.EmSta(()=>
        {
            using var banco=new BancoDeHomologacao();var produtos=new EfProdutoRepository(banco.Fabrica);
            var complemento=produtos.Salvar(new Produto { Id=0,EhComplemento=true,Nome="Bacon",PrecoCentavos=200,Ativo=true,ControlarEstoque=true,QuantidadeEstoque=2,MinimoComplemento=1,CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow });
            var produto=banco.CriarProduto("Lanche",1500);var opcao=new OpcaoComplemento { ComplementoId=complemento.Id,Nome="Bacon",PrecoCentavos=200 };
            produto.GruposComplementos=[new() { Nome="Extras",Minimo=1,Opcoes=[opcao] }];produtos.Salvar(produto);
            var repo=new EfComandaRepository(banco.Fabrica);var comanda=repo.AbrirComanda(1,DateTime.UtcNow);
            var pedido=OperacoesComandaRegressaoTests.Criar(banco,repo);pedido.SelecionarComandaCommand.Execute(comanda);
            using var catalogo=new LocalizarProdutoViewModel(pedido);var item=new ProdutoLocalizado(produto,"Lanches");
            Assert.True(item.AdicaoDireta);Assert.False(pedido.AdicionarProduto(produto,1,null));
            var vm=new PersonalizarItemViewModel(catalogo,item);Assert.False(vm.PodeAdicionar);
            Assert.Single(vm.GruposComplementos.Single().Opcoes).Selecionado=true;vm.Quantidade="3";
            Assert.False(vm.PodeAdicionar);Assert.Contains("Estoque insuficiente",vm.OrientacaoComplementos);Assert.Empty(repo.BuscarComItens(comanda.Id)!.Itens);
            vm.Quantidade="2";vm.Observacao="Sem cebola";Assert.True(vm.PodeAdicionar);
            var outro=repo.AbrirComanda(2,DateTime.UtcNow);repo.AdicionarItem(outro.Id,produto,2,DateTime.UtcNow,null,null,[opcao.Id]);
            Assert.False(vm.Adicionar());Assert.Contains("mudaram",vm.Erro);Assert.False(vm.GruposComplementos.Single().Opcoes.Single().Disponivel);
            Assert.Equal("Sem cebola",vm.Observacao);Assert.Equal("2",vm.Quantidade);Assert.Empty(repo.BuscarComItens(comanda.Id)!.Itens);
        });
    }

    [Fact]
    public void PersonalizacaoExibeSomentePermitidosESalvaEscolhasComBaixaDeEstoque()
    {
        ThreadingHelper.EmSta(()=>
        {
            using var banco=new BancoDeHomologacao();var produtos=new EfProdutoRepository(banco.Fabrica);
            var complemento=produtos.Salvar(new Produto { Id=0,EhComplemento=true,Nome="Queijo",PrecoCentavos=250,Ativo=true,ControlarEstoque=true,QuantidadeEstoque=4,CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow });
            var produto=banco.CriarProduto("Hambúrguer",1500);var opcao=new OpcaoComplemento { ComplementoId=complemento.Id,Nome="Queijo",PrecoCentavos=250 };
            produto.GruposComplementos=[new() { Nome="Extras",Maximo=4,Opcoes=[opcao] }];produtos.Salvar(produto);
            var simples=banco.CriarProduto("Suco",500);
            var repo=new EfComandaRepository(banco.Fabrica);var comanda=repo.AbrirComanda(1,DateTime.UtcNow);
            var pedido=OperacoesComandaRegressaoTests.Criar(banco,repo);pedido.SelecionarComandaCommand.Execute(comanda);
            using var catalogo=new LocalizarProdutoViewModel(pedido);
            Assert.Empty(new PersonalizarItemViewModel(catalogo,new(simples,"Bebidas")).GruposComplementos);
            var vm=new PersonalizarItemViewModel(catalogo,new(produto,"Lanches"));
            var escolha=Assert.Single(vm.GruposComplementos.Single().Opcoes);vm.Quantidade="2";
            escolha.AumentarCommand.Execute(null);escolha.AumentarCommand.Execute(null);
            Assert.Equal(2,escolha.QuantidadeSelecionada);Assert.Contains("40,00",vm.Total);Assert.False(escolha.AumentarCommand.CanExecute(null));
            escolha.DiminuirCommand.Execute(null);Assert.Contains("35,00",vm.Total);Assert.True(escolha.AumentarCommand.CanExecute(null));escolha.AumentarCommand.Execute(null);
            Assert.True(vm.Adicionar());Assert.Equal(0,produtos.BuscarPorId(complemento.Id)!.QuantidadeEstoque);
            var lancado=Assert.Single(repo.BuscarComItens(comanda.Id)!.Itens);Assert.Contains("Queijo",lancado.ComplementosJson);
            Assert.Equal(2,System.Text.Json.JsonSerializer.Deserialize<List<ComplementoCobrado>>(lancado.ComplementosJson)!.Count);
        });
    }

    [Fact]
    public void NovoGrupoComecaOpcionalECalculaMaximoPeloEstoque()
    {
        var grupo=new GrupoComplementoEdicao();Assert.Equal("0",grupo.Minimo);
        var queijo=new Produto { Id=1,Nome="Queijo",EhComplemento=true,ControlarEstoque=true,QuantidadeEstoque=12,PrecoCentavos=250,Ativo=true,CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow };
        grupo.Opcoes.Add(new() { ComplementoId=1 });grupo.AtualizarMaximoPorEstoque([queijo]);Assert.Equal("12",grupo.Maximo);
        grupo.Opcoes.Clear();grupo.AtualizarMaximoPorEstoque([queijo]);Assert.Equal("0",grupo.Maximo);
    }

    [Fact]
    public void ConfiguracaoParteDoProprioProdutoEPermaneceRascunhoAteSalvar()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var vm=CarregamentoDeXamlTests.CriarProdutos();var complementos=vm.CriarCatalogoComplementos();
            complementos.NovoCommand.Execute(null);complementos.NomeProduto="Queijo";complementos.PrecoProdutoReais="2,50";complementos.QuantidadeEstoqueComplemento="10";complementos.SalvarCommand.Execute(null);
            vm.NovoCommand.Execute(null);vm.NomeProduto="Hambúrguer";vm.PrecoProdutoReais="15,00";vm.CategoriaProduto=vm.Categorias.First();
            var view=new ProdutosView(vm);var camada=(Grid)view.FindName("ModalProduto");var formulario=(Border)camada.Children[0];camada.Children.Remove(formulario);formulario.DataContext=vm;
            formulario.Width=790;formulario.Height=650;formulario.Margin=new Thickness(0);
            var host=new Grid { Width=790,Height=650 };host.Children.Add(formulario);Organizar(host,790,650);
            var botao=Descendentes<Button>(formulario).Single(b=>Equals(b.Content,"Configurar complementos…"));
            Assert.Null(view.FindName("BotaoVincularComplementos"));
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(()=>
            {
                var janela=System.Windows.Application.Current.Windows.OfType<JanelaComplementosProduto>().Single();
                var raiz=(FrameworkElement)janela.Content;
                Descendentes<Button>(raiz).Single(b=>Equals(b.Content,"Adicionar grupo")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));raiz.UpdateLayout();
                Descendentes<ComboBox>(raiz).Single().SelectedIndex=0;
                Descendentes<Button>(raiz).Single(b=>Equals(b.Content,"Adicionar opção")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Descendentes<Button>(raiz).Single(b=>Equals(b.Content,"Salvar vínculo")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }));
            botao.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Single(vm.GruposComplementosProduto);Assert.DoesNotContain(vm.Produtos,p=>p.Nome=="Hambúrguer");
            Assert.Equal("Queijo",Assert.Single(vm.ComplementosVisiveis).Nome);Assert.Equal("Automático ao salvar",vm.CodigoAlimento);
            foreach(var escuro in new[] { false,true }) { TemaInterface.Aplicar(escuro,false);Organizar(host,790,650);Capturar(host,$"produto-configurar-complementos-{escuro}"); }
            vm.SalvarCommand.Execute(null);Assert.Empty(vm.Mensagem);
            var salvo=Assert.Single(vm.Produtos,p=>p.Nome=="Hambúrguer");Assert.Single(salvo.GruposComplementos.Single().Opcoes);
            vm.ProdutoSelecionado=salvo;Assert.Equal(salvo.Id.ToString(),vm.CodigoAlimento);
            TemaInterface.Aplicar(false,false);
        });
    }

    [Fact]
    public void CliqueDiretoAdicionaUnidadeSemComplementoESemAbrirPersonalizacao()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();using var banco=new BancoDeHomologacao();
            var produto=banco.CriarProduto("Lanche",1500);
            produto.GruposComplementos=[new() { Nome="Extras",Minimo=0,Maximo=3,Opcoes=[new() { Nome="Queijo",PrecoCentavos=250 }] }];
            new EfProdutoRepository(banco.Fabrica).Salvar(produto);
            var repo=new EfComandaRepository(banco.Fabrica);var comanda=repo.AbrirComanda(1,DateTime.UtcNow);
            var pedido=OperacoesComandaRegressaoTests.Criar(banco,repo);pedido.SelecionarComandaCommand.Execute(comanda);
            var janela=new LocalizarProdutoView(pedido);
            try
            {
                janela.AcionarProduto(new(produto,"Lanches"));
                var item=Assert.Single(repo.BuscarComItens(comanda.Id)!.Itens);
                Assert.Equal(1,item.Quantidade);Assert.Equal(1500,item.PrecoUnitarioCentavos);Assert.Equal("[]",item.ComplementosJson);
                Assert.DoesNotContain(System.Windows.Application.Current.Windows.OfType<Window>(),w=>w is PersonalizarItemView);
            }
            finally { janela.Close(); }
        });
    }

    private static void Organizar(FrameworkElement raiz,double largura,double altura)
    { raiz.Measure(new Size(largura,altura));raiz.Arrange(new Rect(0,0,largura,altura));raiz.UpdateLayout(); }
    private static IEnumerable<T> Descendentes<T>(DependencyObject raiz) where T:DependencyObject
    {
        for(var i=0;i<VisualTreeHelper.GetChildrenCount(raiz);i++)
        { var f=VisualTreeHelper.GetChild(raiz,i);if(f is T t)yield return t;foreach(var n in Descendentes<T>(f))yield return n; }
    }
    private static void Capturar(FrameworkElement raiz,string nome)
    {
        var pasta=Path.Combine(AppContext.BaseDirectory,"previas-complementos");Directory.CreateDirectory(pasta);
        raiz.UpdateLayout();
        var visual=new DrawingVisual();
        using(var desenho=visual.RenderOpen())
        {
            var area=new Rect(0,0,raiz.ActualWidth,raiz.ActualHeight);
            desenho.DrawRectangle((Brush)System.Windows.Application.Current.FindResource("Canvas"),null,area);
            desenho.DrawRectangle(new VisualBrush(raiz) { Stretch=Stretch.Fill },null,area);
        }
        var imagem=new RenderTargetBitmap((int)raiz.ActualWidth,(int)raiz.ActualHeight,96,96,PixelFormats.Pbgra32);imagem.Render(visual);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(imagem));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
}
