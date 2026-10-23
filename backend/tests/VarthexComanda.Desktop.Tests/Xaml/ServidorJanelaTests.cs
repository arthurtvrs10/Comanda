using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Rede;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class ServidorJanelaTests
{
    [Fact]
    public void PrincipalAgrupaDestinosECozinhaTemCategoriaPropria()
    {
        using var banco=new BancoDeHomologacao();
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var janela=CarregamentoDeXamlTests.CriarMainWindow(banco.Paths);
            try
            {
                var categorias=(WrapPanel)janela.FindName("CategoriasRibbon");
                Assert.Equal(new[]{"Principal","Cozinha (KDS)","Configurações","Ajuda"},categorias.Children.OfType<Button>().Select(b=>b.Content));
                foreach(var escuro in new[]{false,true})
                foreach(var categoria in new[]{"Principal","Cozinha (KDS)"})
                foreach(var largura in new[]{1280d,800d})
                {
                    TemaInterface.Aplicar(escuro,false);((CheckBox)janela.FindName("ModoEscuro")).IsChecked=escuro;categorias.Children.OfType<Button>().Single(b=>Equals(b.Content,categoria)).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    var botoes=AlinhamentoInterfaceTests.Filhos((FrameworkElement)janela.FindName("GruposRibbon")).OfType<Button>().Select(b=>System.Windows.Automation.AutomationProperties.GetName(b));
                    Assert.Equal(categoria=="Principal"?new[]{"Mesas / comandas","Produtos","Histórico de pedidos","Histórico de vendas","Painel de senhas"}:new[]{"Cozinha"},botoes);
                    janela.Width=largura;var raiz=(FrameworkElement)janela.Content;raiz.Measure(new Size(largura,780));raiz.Arrange(new Rect(0,0,largura,780));raiz.UpdateLayout();
                    Capturar(raiz,$"menu-{(categoria=="Principal"?"principal":"kds")}-{(escuro?"escuro":"claro")}-{largura}");
                }
            }
            finally {janela.Close();TemaInterface.Aplicar(false,false);}
        });
    }
    [Fact]
    public async Task JanelaMostraPortaEnderecosQrETemasSemIniciarAutomaticamente()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var prods=new EfProdutoRepository(banco.Fabrica);var clock=new RelogioFixo();var config=new ObterConfiguracao(new EfConfiguracaoRepository(banco.Fabrica));
        await using var servidor=new ServidorLocal(new AtendimentoRemoto(repo,prods,new EfCategoriaRepository(banco.Fabrica),new AbrirComanda(repo,clock,config),new AdicionarItem(repo,prods,clock),config));
        void Verificar(bool ativa)
        {
            ThreadingHelper.EmSta(()=>
            {
                CarregamentoDeXamlTests.GarantirApp();
                try
                {
                    foreach(var escuro in new[]{false,true})
                    {
                        TemaInterface.Aplicar(escuro,false);
                        var janela=new JanelaServidor(servidor);janela.Show();janela.UpdateLayout();
                        try
                        {
                            Assert.Equal(ativa,servidor.Ativo);
                            Assert.Equal(ativa,((Image)janela.FindName("Qr")).Source is not null);
                            Assert.Equal(!ativa,((TextBox)janela.FindName("Porta")).IsEnabled);
                            Assert.Equal(ativa,((Button)janela.FindName("Copiar")).IsEnabled);
                            Capturar(janela,$"servidor-{(ativa?"ativo":"parado")}-{(escuro?"escuro":"claro")}");
                        }
                        finally {janela.Close();}
                    }
                }
                finally {TemaInterface.Aplicar(false,false);}
            });
        }
        Verificar(false);await servidor.IniciarAsync(0,true);Verificar(true);
    }
    [Fact]
    public void AtualizarLancamentoPreservaRascunhoESelecaoDoOperador()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var (vm,_)=CarregamentoDeXamlTests.CriarAtendimento();
            vm.ObservacaoNovoItem="Rascunho local";vm.NomeCliente="Nome em edição";var selecionado=vm.ItemSelecionadoId;
            vm.AtualizarLancamentoRemoto(vm.ComandaAtual!.Id);
            Assert.Equal("Rascunho local",vm.ObservacaoNovoItem);Assert.Equal("Nome em edição",vm.NomeCliente);Assert.Equal(selecionado,vm.ItemSelecionadoId);
        });
    }
    private static void Capturar(Window janela,string nome)=>Capturar((FrameworkElement)janela,nome);
    private static void Capturar(FrameworkElement janela,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrEmpty(pasta))return;Directory.CreateDirectory(pasta);
        var b=new RenderTargetBitmap((int)janela.ActualWidth,(int)janela.ActualHeight,96,96,PixelFormats.Pbgra32);b.Render(janela);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(b));using var f=File.Create(Path.Combine(pasta,nome+".png"));png.Save(f);
    }
}
