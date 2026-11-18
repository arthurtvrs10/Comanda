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
            cliente.Text="Ana";obs.Text="Alergia a leite";tipo.SelectedValue=TipoPedido.Delivery;
            var salvo=repo.BuscarComItens(c.Id)!.Comanda;Assert.Equal("Ana",salvo.NomeCliente);Assert.Equal(TipoPedido.Delivery,salvo.TipoPedido);Assert.Equal("Alergia a leite",salvo.Observacao);
            cliente.Text="Bruna";obs.Text="Carne bem passada";vm.FecharEdicaoCommand.Execute(null);
            salvo=repo.BuscarComItens(c.Id)!.Comanda;Assert.Equal("Bruna",salvo.NomeCliente);Assert.Equal("Carne bem passada",salvo.Observacao);Assert.Equal(TipoPedido.Delivery,salvo.TipoPedido);
        });
    }
    [Fact] public void KdsAtualizaEtiquetasClienteEObservacaoPreservandoOsCartoes()
    {
        var pedido=CozinhaViewModelTests.Pedido(1,5,CozinhaViewModelTests.Item(1));var vm=new CozinhaViewModel(()=>[pedido]);vm.Atualizar();var cartao=Assert.Single(vm.Pedidos);
        foreach(var tipo in Enum.GetValues<TipoPedido>())
        {
            pedido.Comanda.TipoPedido=tipo;pedido.Comanda.NomeCliente="Ana "+tipo;pedido.Comanda.Observacao="Alergia: "+tipo;vm.Atualizar();Assert.Same(cartao,Assert.Single(vm.Pedidos));Assert.Equal(tipo.Descricao(),cartao.TipoPedidoTexto);Assert.Equal(tipo.ToString(),cartao.TipoPedidoChave);Assert.Equal(pedido.Comanda.NomeCliente,cartao.Cliente);Assert.Equal(pedido.Comanda.Observacao,cartao.Observacao);
        }
    }
    [Fact] public void EtiquetasSaoLegiveisNosDoisTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var estados=new[]{StatusPreparo.Pendente,StatusPreparo.EmPreparo,StatusPreparo.Pronto,StatusPreparo.Entregue,StatusPreparo.Pendente};
            var dados=Enum.GetValues<TipoPedido>().Select((tipo,i)=>
            {
                var pedido=CozinhaViewModelTests.Pedido(i+1,tipo==TipoPedido.LocalMesa?5:null,CozinhaViewModelTests.Item(i+1,"Hambúrguer",estados[i]));pedido.Comanda.TipoPedido=tipo;pedido.Comanda.NomeCliente="Ana";pedido.Comanda.Observacao="ALERGIA A LEITE. Não usar queijo ou manteiga.";return pedido;
            }).ToArray();
            try{foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);var view=new CozinhaView(()=>dados);view.ViewModel.Agora=()=>CozinhaViewModelTests.Agora;view.Atualizar();view.Measure(new Size(1800,950));view.Arrange(new Rect(0,0,1800,950));view.UpdateLayout();
                var etiquetas=AlinhamentoInterfaceTests.Filhos(view).OfType<Border>().Where(b=>b.Style==view.FindResource("EtiquetaTipoPedido")).ToArray();Assert.Equal(5,etiquetas.Length);
                foreach(var etiqueta in etiquetas){Assert.IsType<SolidColorBrush>(etiqueta.Background);Assert.False(string.IsNullOrWhiteSpace(System.Windows.Automation.AutomationProperties.GetName(etiqueta)));}
                var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(pasta is null)continue;Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap(1800,950,96,96,PixelFormats.Pbgra32);bitmap.Render(view);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(Path.Combine(pasta,$"kds-tipos-{(escuro?"escuro":"claro")}.png"));png.Save(stream);
            }}finally{TemaInterface.Aplicar(false,false);}
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
