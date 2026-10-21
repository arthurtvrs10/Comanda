using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Catalogo;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class OrdenacaoProdutosTests
{
    [Fact]
    public void CabecalhosSinalizamOrdenacaoEAlternamNumerosTextosECategorias()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();TemaInterface.Aplicar(false,false);
            var vm=CarregamentoDeXamlTests.CriarProdutos();vm.Produtos.Clear();vm.Categorias.Clear();
            var agora=DateTime.UtcNow;
            vm.Categorias.Add(new Categoria { Id=9,Nome="Lanches",Ativo=true,CriadoEm=agora,AtualizadoEm=agora });
            vm.Categorias.Add(new Categoria { Id=3,Nome="Bebidas",Ativo=true,CriadoEm=agora,AtualizadoEm=agora });
            Produto Produto(int id,int categoria,string nome,long preco,bool ativo)=>new()
            { Id=id,CategoriaId=categoria,Nome=nome,PrecoCentavos=preco,CustoCentavos=preco/2,Ativo=ativo,CodigoPersonalizado="P"+id,CriadoEm=agora,AtualizadoEm=agora };
            vm.Produtos.Add(Produto(10,3,"Café coado",500,true));
            vm.Produtos.Add(Produto(2,9,"Pão de queijo",1200,false));
            vm.Produtos.Add(Produto(30,9,"Suco de laranja",2000,true));
            var view=new ProdutosView(vm);var tabela=(DataGrid)view.FindName("TabelaProdutos");
            var janela=new Window { Content=view,Width=1200,Height=550,Opacity=0,ShowInTaskbar=false };
            try
            {
                janela.Show();janela.UpdateLayout();
                DataGridColumnHeader Cabecalho(string nome)=>AlinhamentoInterfaceTests.Filhos(tabela)
                    .OfType<DataGridColumnHeader>().Single(c=>Equals(c.Column?.Header,nome));
                System.Windows.Shapes.Path Indicador(DataGridColumnHeader cabecalho)=>AlinhamentoInterfaceTests.Filhos(cabecalho)
                    .OfType<System.Windows.Shapes.Path>().Single(p=>p.Name=="IndicadorOrdenavel");
                void Clicar(string nome)
                {
                    typeof(DataGridColumnHeader).GetMethod("OnClick",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(Cabecalho(nome),null);
                    janela.UpdateLayout();
                }
                int[] Ordem()=>tabela.Items.OfType<Produto>().Select(p=>p.Id).ToArray();
                foreach(var coluna in tabela.Columns.Where(c=>c.CanUserSort))
                    Assert.Equal(Visibility.Visible,Indicador(Cabecalho((string)coluna.Header)).Visibility);
                Assert.Equal(Visibility.Collapsed,Indicador(Cabecalho("Medida")).Visibility);
                var neutro=Indicador(Cabecalho("Cód. sistema")).Data.ToString();
                Salvar(view,"produtos-ordenaveis");
                Clicar("Cód. sistema");Assert.Equal(new[]{2,10,30},Ordem());
                Assert.Equal(ListSortDirection.Ascending,Cabecalho("Cód. sistema").SortDirection);
                Assert.Equal(Visibility.Visible,Indicador(Cabecalho("Cód. sistema")).Visibility);
                var crescente=Indicador(Cabecalho("Cód. sistema")).Data.ToString();Assert.NotEqual(neutro,crescente);
                Assert.Contains("decrescente",(string)Cabecalho("Cód. sistema").ToolTip);
                Salvar(view,"produtos-crescente");
                Clicar("Cód. sistema");Assert.Equal(new[]{30,10,2},Ordem());
                Assert.Equal(ListSortDirection.Descending,Cabecalho("Cód. sistema").SortDirection);
                Assert.NotEqual(crescente,Indicador(Cabecalho("Cód. sistema")).Data.ToString());
                Salvar(view,"produtos-decrescente");
                Clicar("Categoria");Assert.Equal(new[]{10,2,30},Ordem());
                Clicar("Categoria");Assert.Equal(new[]{2,30,10},Ordem());
                Clicar("Preço venda");Assert.Equal(new[]{10,2,30},Ordem());
                Clicar("Preço venda");Assert.Equal(new[]{30,2,10},Ordem());
                Clicar("Nome");Assert.Equal(new[]{10,2,30},Ordem());
                Assert.Null(Cabecalho("Categoria").SortDirection);
                Clicar("Status venda");Assert.False(tabela.Items.OfType<Produto>().First().Ativo);
                Assert.Equal(new[]{10,2,30},vm.Produtos.Select(p=>p.Id));
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,false);janela.UpdateLayout();Salvar(view,$"produtos-ordenacao-{escuro}");
                }
            }
            finally { janela.Close();TemaInterface.Aplicar(false,false); }
        });
    }

    private static void Salvar(FrameworkElement view,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrWhiteSpace(pasta))return;
        Directory.CreateDirectory(pasta);var w=(int)view.ActualWidth;var h=(int)view.ActualHeight;
        var desenho=new DrawingVisual();using(var d=desenho.RenderOpen())d.DrawRectangle(new VisualBrush(view),null,new Rect(0,0,w,h));
        var bitmap=new RenderTargetBitmap(w,h,96,96,PixelFormats.Pbgra32);bitmap.Render(desenho);
        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));
        using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));png.Save(arquivo);
    }
}
