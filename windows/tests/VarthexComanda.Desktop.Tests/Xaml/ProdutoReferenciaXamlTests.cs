using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Catalogo;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class ProdutoReferenciaXamlTests
{
    [Fact]
    public void NovoEEdicaoUsamMesmoEditorCompactoPreservandoDadosAoCancelar()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var vm=CarregamentoDeXamlTests.CriarProdutos();
            void Capturar(string nome)
            {
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,false);
                    var view=new ProdutosView(vm);var camada=(Grid)view.FindName("ModalProduto");var formulario=(Border)camada.Children[0];camada.Children.Remove(formulario);
                    formulario.DataContext=vm;formulario.Margin=new Thickness(0);formulario.Width=790;formulario.Height=650;
                    formulario.HorizontalAlignment=HorizontalAlignment.Left;formulario.VerticalAlignment=VerticalAlignment.Top;
                    var tela=new Grid { Width=790,Height=650 };tela.Children.Add(formulario);
                    tela.Measure(new Size(790,650));tela.Arrange(new Rect(0,0,790,650));tela.UpdateLayout();
                    Assert.Equal(790,formulario.ActualWidth);
                    Assert.Equal(28,((TextBox)view.FindName("CampoNomeProduto")).MinHeight);
                    Assert.Equal(28,((TextBox)view.FindName("CampoCustoProduto")).MinHeight);
                    Assert.Equal(((SolidColorBrush)System.Windows.Application.Current.FindResource("Canvas")).Color,((SolidColorBrush)formulario.Background).Color);
                    var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                    if(!string.IsNullOrWhiteSpace(pasta))
                    {
                        Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap(790,650,96,96,PixelFormats.Pbgra32);bitmap.Render(tela);
                        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,$"produto-{nome}-{(escuro?"escuro":"claro")}.png"));encoder.Save(arquivo);
                    }
                }
            }
            try
            {
                vm.NovoCommand.Execute(null);Capturar("novo");
                vm.NomeProduto="Prato de teste";vm.CategoriaProduto=vm.Categorias[0];vm.PrecoProdutoReais="25,00";vm.CustoProdutoReais="12,50";vm.CodigoPersonalizadoProduto="REF-02";vm.DescricaoProduto="Descrição preservada";vm.SalvarCommand.Execute(null);
                var produto=vm.Produtos.Single(p=>p.CodigoPersonalizado=="REF-02");vm.ProdutoSelecionado=produto;
                Assert.Equal("12,50",vm.CustoProdutoReais);Assert.Equal("Descrição preservada",vm.DescricaoProduto);
                Capturar("edicao");vm.DescricaoProduto="Não salvar";vm.NovoCommand.Execute(null);
                Assert.Equal("Descrição preservada",produto.Descricao);Assert.Empty(vm.DescricaoProduto);
            }
            finally { TemaInterface.Aplicar(false,false); }
        });
    }
}
