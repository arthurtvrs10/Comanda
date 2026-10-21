using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VarthexComanda.Desktop.Catalogo;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class AlinhamentoInterfaceTests
{
    [Fact]
    public void TodasAsCelulasDeProdutosFicamNoCentroComAlturasETemasDiferentes()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var vm=CarregamentoDeXamlTests.CriarProdutos();
            vm.Produtos.Add(new VarthexComanda.Domain.Produto { Id=201,CategoriaId=vm.Categorias[0].Id,Nome="Produto inativo com nome longo para conferir a linha inteira",PrecoCentavos=2590,Ativo=false,CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow });
            var view=new ProdutosView(vm);var tabela=(DataGrid)view.FindName("TabelaProdutos");
            var janela=new Window { Content=view,Width=1200,Height=700,Opacity=0,ShowInTaskbar=false };
            try
            {
                janela.Show();
                foreach(var escuro in new[]{false,true})
                foreach(var altura in new[]{32d,44d})
                {
                    TemaInterface.Aplicar(escuro,false);tabela.RowHeight=altura;tabela.FontSize=altura==32?12:16;tabela.SelectedIndex=0;janela.UpdateLayout();
                    var linhas=Filhos(tabela).OfType<DataGridRow>().ToArray();Assert.NotEmpty(linhas);
                    foreach(var linha in linhas)
                    foreach(var texto in Filhos(linha).OfType<TextBlock>().Where(t=>!string.IsNullOrEmpty(t.Text)&&t.ActualHeight>0))
                    {
                        var y=texto.TranslatePoint(new Point(0,texto.ActualHeight/2),linha).Y;
                        Assert.True(Math.Abs(y-linha.ActualHeight/2)<=1.1,$"{texto.Text}: centro {y:0.00}, linha {linha.ActualHeight/2:0.00}, tema {escuro}");
                    }
                    foreach(var linha in linhas)
                    foreach(var indicador in Filhos(linha).OfType<System.Windows.Shapes.Ellipse>())
                    {
                        var y=indicador.TranslatePoint(new Point(0,indicador.ActualHeight/2),linha).Y;
                        Assert.InRange(Math.Abs(y-linha.ActualHeight/2),0,1.1);
                    }
                    var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                    if(!string.IsNullOrWhiteSpace(pasta))
                    {
                        Directory.CreateDirectory(pasta);
                        var imagem=new System.Windows.Media.Imaging.RenderTargetBitmap((int)view.ActualWidth,(int)view.ActualHeight,96,96,PixelFormats.Pbgra32);imagem.Render(view);
                        var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(imagem));
                        using var arquivo=File.Create(Path.Combine(pasta,$"alinhamento-{(escuro?"escuro":"claro")}-{altura}.png"));encoder.Save(arquivo);
                    }
                }
            }
            finally { janela.Close();TemaInterface.Aplicar(false,false); }
        });
    }
    internal static IEnumerable<DependencyObject> Filhos(DependencyObject raiz)
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(raiz);i++){var f=VisualTreeHelper.GetChild(raiz,i);yield return f;foreach(var filho in Filhos(f))yield return filho;}
    }
}
