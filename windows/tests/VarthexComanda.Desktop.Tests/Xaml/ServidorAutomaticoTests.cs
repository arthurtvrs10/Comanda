using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Rede;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class ServidorAutomaticoTests
{
    private const BindingFlags Privado=BindingFlags.Instance|BindingFlags.NonPublic;
    private static int PortaLivre()
    {
        using var reserva=new TcpListener(IPAddress.Loopback,0);reserva.Start();return ((IPEndPoint)reserva.LocalEndpoint).Port;
    }
    private static ServidorLocal Criar(BancoDeHomologacao banco,Func<bool>? permitido=null)
    {
        var repo=new EfComandaRepository(banco.Fabrica);var produtos=new EfProdutoRepository(banco.Fabrica);var config=new ObterConfiguracao(new EfConfiguracaoRepository(banco.Fabrica));var clock=new RelogioFixo();
        return new(new AtendimentoRemoto(repo,produtos,new EfCategoriaRepository(banco.Fabrica),new AbrirComanda(repo,clock,config),new AdicionarItem(repo,produtos,clock),config),permitido);
    }
    [Theory]
    [InlineData(true,true,false,true)]
    [InlineData(false,true,false,false)]
    [InlineData(true,false,false,false)]
    [InlineData(true,true,true,false)]
    public async Task JanelaPrincipalIniciaConformePreferenciaLicencaERestauracao(bool automatico,bool licenca,bool restauracao,bool esperado)
    {
        using var banco=new BancoDeHomologacao();var repoConfig=new EfConfiguracaoRepository(banco.Fabrica);
        new ConfiguracaoServidorService(repoConfig).Salvar(new(automatico,PortaLivre()),DateTime.UtcNow);
        await using var servidor=Criar(banco,()=>licenca);MainWindow? janela=null;Task inicio=Task.CompletedTask;
        try
        {
            ThreadingHelper.EmSta(()=>
            {
                CarregamentoDeXamlTests.GarantirApp();
                janela=CarregamentoDeXamlTests.CriarMainWindow(banco.Paths,configuracoes:repoConfig,servidor:servidor);
                janela.Opacity=0;janela.ShowInTaskbar=false;if(restauracao)janela.EntrarModoRestauracao();
                janela.Show();janela.UpdateLayout();janela.Dispatcher.Invoke(()=>{},DispatcherPriority.Loaded);
                inicio=(Task)typeof(MainWindow).GetField("_inicioAutomaticoServidor",Privado)!.GetValue(janela)!;
                Assert.True(janela.IsVisible);
            });
            await inicio.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(esperado,servidor.Ativo);
            ThreadingHelper.EmSta(()=>
            {
                Assert.True(janela!.IsVisible);
                if(!licenca&&!restauracao&&automatico)
                {
                    var modelo=(ServidorViewModel)typeof(MainWindow).GetField("_controleServidor",Privado)!.GetValue(janela)!;
                    Assert.Equal("erro",modelo.TipoEstado);Assert.Contains("licença",modelo.Mensagem);
                }
            });
            if(esperado)
            {
                using var cliente=new HttpClient();Assert.Contains("Atendimento pelo celular",await cliente.GetStringAsync($"http://127.0.0.1:{servidor.Porta}/"));
                Assert.Empty(servidor.Acessos);
            }
        }
        finally{if(janela is not null)ThreadingHelper.EmSta(()=>janela.Close());}
        Assert.False(servidor.Ativo);
    }
    [Fact]
    public async Task PortaOcupadaNaoDerrubaDesktopEPreferenciaDeInicioPersiste()
    {
        using var banco=new BancoDeHomologacao();using var reserva=new TcpListener(IPAddress.Any,0);reserva.Start();var porta=((IPEndPoint)reserva.LocalEndpoint).Port;
        var preferencias=new ConfiguracaoServidorService(new EfConfiguracaoRepository(banco.Fabrica));preferencias.Salvar(new(true,porta),DateTime.UtcNow);
        await using var servidor=Criar(banco);ServidorViewModel? modelo=null;Task inicio=Task.CompletedTask;
        ThreadingHelper.EmSta(()=>{CarregamentoDeXamlTests.GarantirApp();modelo=new(servidor,banco.Paths.DatabasePath,preferencias);inicio=modelo.IniciarAutomaticamenteAsync();});
        await inicio.WaitAsync(TimeSpan.FromSeconds(10));
        ThreadingHelper.EmSta(()=>
        {
            Assert.False(servidor.Ativo);Assert.Equal("erro",modelo!.TipoEstado);Assert.Contains("porta",modelo.Mensagem);Assert.True(modelo.AlternarCommand.CanExecute(null));
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);var janela=new JanelaServidor(modelo){ShowInTaskbar=false};
                try{janela.Show();janela.UpdateLayout();janela.Dispatcher.Invoke(()=>{},DispatcherPriority.ContextIdle);janela.UpdateLayout();Capturar(janela,$"servidor-falha-{(escuro?"escuro":"claro")}");}
                finally{janela.Close();}
            }
            TemaInterface.Aplicar(false,false);
            modelo.IniciarAutomaticamente=false;modelo.SalvarInicializacaoCommand.Execute(null);Assert.False(preferencias.Carregar().IniciarAutomaticamente);
        });
        reserva.Stop();
        ThreadingHelper.EmSta(()=>inicio=modelo!.AlternarCommand.ExecuteAsync(null));await inicio.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(servidor.Ativo);Assert.False(preferencias.Carregar().IniciarAutomaticamente);
    }
    [Fact]
    public async Task RepetirCargaNaoReiniciaServidorEParadaAtualizaQrSemTrocarJanela()
    {
        using var banco=new BancoDeHomologacao();var preferencias=new ConfiguracaoServidorService(new EfConfiguracaoRepository(banco.Fabrica));preferencias.Salvar(new(true,PortaLivre()),DateTime.UtcNow);
        await using var servidor=Criar(banco);ServidorViewModel? modelo=null;Task tarefa=Task.CompletedTask;
        ThreadingHelper.EmSta(()=>{CarregamentoDeXamlTests.GarantirApp();modelo=new(servidor,banco.Paths.DatabasePath,preferencias);tarefa=modelo.IniciarAutomaticamenteAsync();});
        await tarefa.WaitAsync(TimeSpan.FromSeconds(10));var link=servidor.LinkAcesso(servidor.Enderecos.First());
        ThreadingHelper.EmSta(()=>tarefa=modelo!.IniciarAutomaticamenteAsync());await tarefa;
        Assert.True(link==servidor.LinkAcesso(servidor.Enderecos.First()));
        ThreadingHelper.EmSta(()=>tarefa=modelo!.TestarCommand.ExecuteAsync(null));await tarefa;
        ThreadingHelper.EmSta(()=>{Assert.Contains("respondeu",modelo!.Mensagem);tarefa=modelo.AlternarCommand.ExecuteAsync(null);});await tarefa;
        ThreadingHelper.EmSta(()=>{Assert.False(servidor.Ativo);Assert.Null(modelo!.LinkAcesso);Assert.True(modelo.PodeMudarPorta);});
    }
    [Fact]
    public async Task PainelRealTemDoisPaineisEmTelaAmplaERolagemEmTelaMenorNosDoisTemas()
    {
        using var banco=new BancoDeHomologacao();await using var servidor=Criar(banco);
        await servidor.IniciarAsync(0,true);
        using(var cliente=new HttpClient())
        {
            cliente.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",servidor.LinkAcesso(servidor.Enderecos.Single()).Split("#chave=")[1]);
            await cliente.GetAsync($"http://127.0.0.1:{servidor.Porta}/api/catalogo");
        }
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try
            {
                foreach(var escuro in new[]{false,true})
                foreach(var largura in new[]{1160d,800d})
                {
                    TemaInterface.Aplicar(escuro,false);
                    var modelo=new ServidorViewModel(servidor,banco.Paths.DatabasePath,lerRedes:()=>[new("Wi-Fi de exemplo","Wi-Fi","192.168.10.20","Conectada"),new("Ethernet de exemplo","Ethernet","—","Sem conexão")]);
                    var janela=new JanelaServidor(modelo){Width=largura,Height=760,ShowInTaskbar=false};
                    try
                    {
                        janela.Show();janela.UpdateLayout();janela.Dispatcher.Invoke(()=>{},DispatcherPriority.ContextIdle);janela.UpdateLayout();
                        Assert.True(servidor.Ativo);Assert.NotNull(((Image)janela.FindName("Qr")).Source);
                        Assert.Equal(banco.Paths.DatabasePath,((TextBox)janela.FindName("CaminhoBanco")).Text);
                        Assert.NotEmpty(((DataGrid)janela.FindName("TabelaAcessos")).Items);
                        var direito=(FrameworkElement)janela.FindName("Conectar");Assert.Equal(largura==1160?1:0,Grid.GetColumn(direito));
                        Capturar(janela,$"servidor-info-{(escuro?"escuro":"claro")}-{largura}");
                        if(largura==800)
                        {
                            var rolagem=AlinhamentoInterfaceTests.Filhos(janela).OfType<ScrollViewer>().First();Assert.True(rolagem.ScrollableHeight>0);rolagem.ScrollToBottom();janela.UpdateLayout();
                            Capturar(janela,$"servidor-info-{(escuro?"escuro":"claro")}-800-final");
                        }
                    }
                    finally{janela.Close();}
                    Assert.True(servidor.Ativo);
                }
            }
            finally{TemaInterface.Aplicar(false,false);}
        });
    }
    private static void Capturar(Window janela,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrWhiteSpace(pasta))return;Directory.CreateDirectory(pasta);
        var bitmap=new RenderTargetBitmap((int)janela.ActualWidth,(int)janela.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(janela);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
}
