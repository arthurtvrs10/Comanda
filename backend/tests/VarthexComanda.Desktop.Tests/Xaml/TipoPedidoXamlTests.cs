using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Tests.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;
public class TipoPedidoXamlTests
{
    [Fact] public void CamposSalvamAoAlterarTipoEAoSairSemPerderClienteOuObservacao()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var c=repo.AbrirComanda(1,DateTime.UtcNow);
            var vm=OperacoesComandaRegressaoTests.Criar(banco,repo);vm.SelecionarComandaCommand.Execute(c);var view=new AtendimentoView(vm);
            Assert.Null(view.FindName("ResponsavelPedido"));var cliente=(TextBox)view.FindName("ClientePedidoTexto");var tipo=(ComboBox)view.FindName("TipoPedidoSelecionado");var obs=(TextBox)view.FindName("ObservacaoPedido");var fechamento=(CheckBox)view.FindName("BloqueioPedido");
            var painel=(Panel)((FrameworkElement)cliente.Parent).Parent;Assert.True(painel.Children.IndexOf((UIElement)cliente.Parent)<painel.Children.IndexOf(tipo));Assert.True(painel.Children.IndexOf(tipo)<painel.Children.IndexOf((UIElement)obs.Parent));Assert.True(painel.Children.IndexOf((UIElement)obs.Parent)<painel.Children.IndexOf(fechamento));
            Assert.True(System.Windows.Automation.AutomationProperties.GetIsRequiredForForm(tipo));Assert.Equal(5,tipo.Items.Count);
            Assert.DoesNotContain(AlinhamentoInterfaceTests.Filhos(view).OfType<Button>(),b=>b.Tag as string=="cliente");
            cliente.Text="Ana";obs.Text="Alergia a leite";tipo.SelectedValue=TipoPedido.Delivery;
            var salvo=repo.BuscarComItens(c.Id)!.Comanda;Assert.Equal("Ana",salvo.NomeCliente);Assert.Equal(TipoPedido.Delivery,salvo.TipoPedido);Assert.Equal("Alergia a leite",salvo.Observacao);
            cliente.Text="Bruna";obs.Text="Carne bem passada";vm.FecharEdicaoCommand.Execute(null);
            salvo=repo.BuscarComItens(c.Id)!.Comanda;Assert.Equal("Bruna",salvo.NomeCliente);Assert.Equal("Carne bem passada",salvo.Observacao);Assert.Equal(TipoPedido.Delivery,salvo.TipoPedido);
        });
    }
    [Fact] public void FormularioMantemOperadorAutomaticoELayoutNosDoisTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var c=repo.AbrirComanda(1,DateTime.UtcNow);
            var vm=OperacoesComandaRegressaoTests.Criar(banco,repo);vm.ObterOperador=()=>"Operador de teste";vm.SelecionarComandaCommand.Execute(c);vm.RegistrarAberturaPedido();
            Assert.Equal("Operador de teste",repo.BuscarComItens(c.Id)!.Comanda.CriadoPor);
            Assert.Equal("Operador de teste",repo.BuscarComItens(c.Id)!.Comanda.Responsavel);
            var view=new AtendimentoView(vm);var panel=(FrameworkElement)view.FindName("PainelEdicao");var parent=(Panel)panel.Parent;parent.Children.Remove(panel);panel.DataContext=vm;
            var window=new Window {Title="Comanda · validação do formulário",Width=900,Height=740,Content=panel};
            try
            {
                window.Show();
                foreach(var dark in new[]{false,true})
                {
                    TemaInterface.Aplicar(dark,false);window.UpdateLayout();
                    Assert.Null(view.FindName("ResponsavelPedido"));
                    var path=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(path is null)continue;Directory.CreateDirectory(path);
                    var image=new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);image.Render(window);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(image));using var stream=File.Create(Path.Combine(path,$"comanda-formulario-{(dark?"escuro":"claro")}.png"));png.Save(stream);
                }
            }
            finally{window.Content=null;window.Close();parent.Children.Add(panel);TemaInterface.Aplicar(false,false);}
        });
    }
}
