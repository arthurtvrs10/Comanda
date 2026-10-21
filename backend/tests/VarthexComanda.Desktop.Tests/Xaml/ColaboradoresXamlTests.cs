using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class ColaboradoresXamlTests
{
    [Fact]
    public void ListaCompactaPesquisaDadosReaisEEditorRespeitaTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var pasta=Path.Combine(Path.GetTempPath(),"varthex-colaboradores-ui-"+Guid.NewGuid().ToString("N"));
            try
            {
                var servico=new ServicoColaboradores(Path.Combine(pasta,"colaboradores.json"));
                var gestor=new AcessoGestor(Path.Combine(pasta,"gestor.credencial"));gestor.Cadastrar("senha-segura");
                var pessoa=new Colaborador { Nome="Ana Teste",Login="ana",Funcao="Atendimento",Telefone="(11) 90000-0000" };
                servico.Salvar(pessoa,"senha-segura");servico.Salvar(new Colaborador { Nome="Carlos Teste",Login="carlos",Perfil="cozinha",Funcao="Preparo" },"senha-segura");
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro);
                    var janela=new JanelaColaboradores(servico,gestor);
                    var raiz=(FrameworkElement)janela.Content;
                    raiz.Measure(new Size(1080,590));raiz.Arrange(new Rect(0,0,1080,590));raiz.UpdateLayout();
                    var lista=Descendentes<ListView>(raiz).Single();Assert.Equal(2,lista.Items.Count);
                    var busca=Descendentes<TextBox>(raiz).Single();busca.Text="Carlos";Assert.Single(lista.Items);busca.Text="";raiz.UpdateLayout();
                    Assert.Equal(2,Descendentes<ListViewItem>(lista).Count());
                    var selecionado=(ListViewItem)lista.ItemContainerGenerator.ContainerFromItem(lista.SelectedItem);
                    Assert.Same(System.Windows.Application.Current.FindResource("BackgroundE1E8E4"),selecionado.Background);
                    Assert.Equal(12,janela.FontSize);
                    SalvarPrevia(raiz,$"colaboradores-{(escuro?"escuro":"claro")}.png");janela.Close();
                    var editor=new JanelaEditarColaborador(servico,gestor,pessoa,false);
                    var formulario=(FrameworkElement)editor.Content;
                    formulario.Measure(new Size(540,690));formulario.Arrange(new Rect(0,0,540,690));formulario.UpdateLayout();
                    Assert.Equal(5,Descendentes<TextBox>(formulario).Count());
                    SalvarPrevia(formulario,$"colaborador-editor-{(escuro?"escuro":"claro")}.png");editor.Close();
                }
                TemaInterface.Aplicar(false);
            }
            finally { if(Directory.Exists(pasta))Directory.Delete(pasta,true); }
        });
    }
    private static IEnumerable<T> Descendentes<T>(DependencyObject raiz) where T:DependencyObject
    {
        for(var i=0;i<VisualTreeHelper.GetChildrenCount(raiz);i++)
        {
            var filho=VisualTreeHelper.GetChild(raiz,i);if(filho is T encontrado)yield return encontrado;
            foreach(var item in Descendentes<T>(filho))yield return item;
        }
    }
    private static void SalvarPrevia(FrameworkElement elemento,string nome)
    {
        var visual=new DrawingVisual();
        using(var desenho=visual.RenderOpen())
        {
            var area=new Rect(0,0,elemento.ActualWidth,elemento.ActualHeight);
            desenho.DrawRectangle((Brush)System.Windows.Application.Current.FindResource("Canvas"),null,area);
            desenho.DrawRectangle(new VisualBrush(elemento) { Stretch=Stretch.Fill },null,area);
        }
        var imagem=new RenderTargetBitmap((int)elemento.ActualWidth,(int)elemento.ActualHeight,96,96,PixelFormats.Pbgra32);imagem.Render(visual);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(imagem));
        var pasta=Path.Combine(AppContext.BaseDirectory,"previas-colaboradores");Directory.CreateDirectory(pasta);
        using var arquivo=File.Create(Path.Combine(pasta,nome));encoder.Save(arquivo);
    }
}
