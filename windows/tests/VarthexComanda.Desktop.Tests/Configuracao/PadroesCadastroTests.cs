using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Tests.Xaml;

namespace VarthexComanda.Desktop.Tests.Configuracao;
public class PadroesCadastroTests
{
    [Fact] public void AjudaMantemRotuloDinamicoEVinculoDoCampo()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var origem=new TextBlock {Text="Preço de custo (R$)"};var campo=new TextBox();var label=new Label {Target=campo};label.SetBinding(ContentControl.ContentProperty,new Binding("Text"){Source=origem});
            AjudaCampos.Padronizar(label);label.Measure(new Size(350,70));label.Arrange(new Rect(0,0,350,70));label.UpdateLayout();
            Assert.Same(campo,label.Target);Assert.True(BindingOperations.IsDataBound(label,ContentControl.ContentProperty));
            origem.Text="Preço de venda (R$)";Assert.Equal(origem.Text,label.Content);
            var botoes=AlinhamentoInterfaceTests.Filhos(label).OfType<Button>().ToArray();Assert.Single(botoes);
        });
    }
    [Fact] public void TelefoneCompartilhadoPreservaPaisEPermiteColarNumero()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var campo=new CampoTelefonePadrao("+1 2025550100","US");var alteracoes=0;campo.Alterado+=(_,_)=>alteracoes++;
            Assert.Equal("US",campo.CodigoPais);Assert.Equal(0,alteracoes);
            campo.Numero.Text="2025550199";Assert.Equal("+1 2025550199",campo.Telefone);Assert.Equal("US",campo.CodigoPais);Assert.True(alteracoes>0);
            var brasileiro=new CampoTelefonePadrao("61900000900","BR");Assert.Equal("(61) 9 0000-0900",brasileiro.Numero.Text);
            var raiz=new StackPanel {Margin=new Thickness(20),Width=720};
            raiz.Children.Add(new RotuloAjuda {Texto="Nome *",Margin=new Thickness(0,0,0,5)});raiz.Children.Add(new TextBox {Foreground=TemaInterface.PincelDoPapel("Ink"),Background=TemaInterface.PincelDoPapel("Surface"),Text="Dados do cadastro",MinHeight=36,Margin=new Thickness(0,0,0,15)});
            raiz.Children.Add(new RotuloAjuda {Texto="Telefone principal",Margin=new Thickness(0,0,0,5)});raiz.Children.Add(brasileiro);
            var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(pasta is null)return;
            try{foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);raiz.SetResourceReference(Panel.BackgroundProperty,"Surface");raiz.Measure(new Size(760,220));raiz.Arrange(new Rect(0,0,760,220));raiz.UpdateLayout();
                Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap(760,220,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var saida=File.Create(Path.Combine(pasta,$"padrao-cadastro-{(escuro?"escuro":"claro")}.png"));png.Save(saida);
            }}finally{TemaInterface.Aplicar(false,false);}
        });
    }
}

