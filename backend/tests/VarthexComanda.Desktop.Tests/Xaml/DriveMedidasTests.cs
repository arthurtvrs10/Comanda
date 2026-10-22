using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Backup;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Desktop.Catalogo;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class DriveMedidasTests
{
    [Fact]
    public void ConviteAguardaCincoSegundosEPermiteAdiarComErroSemCredenciais()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try
            {
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,false);var tempo=new Relogio();
                    var view=new ConectarGoogleDriveView(new SemCredenciais(),tempo);
                    view.Show();view.UpdateLayout();
                    Assert.False(view.PodeAdiar);Assert.False(((Button)view.FindName("Depois")).IsEnabled);
                    view.Close();Assert.True(view.IsVisible);
                    Capturar(view,$"drive-convite-{escuro}-espera");
                    tempo.Segundos=4.999;view.AtualizarContagem();Assert.False(view.PodeAdiar);
                    tempo.Segundos=5;view.AtualizarContagem();Assert.True(view.PodeAdiar);Assert.True(((Button)view.FindName("Depois")).IsEnabled);
                    ((Button)view.FindName("Conectar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.Contains("configurada",((TextBlock)view.FindName("Mensagem")).Text);
                    view.UpdateLayout();Capturar(view,$"drive-convite-{escuro}-adiar");view.Close();Assert.False(view.IsVisible);
                    var backup = new BackupView(BackupLayoutTests.Criar(google:new SemCredenciais()));
                    backup.Measure(new Size(1100,630));backup.Arrange(new Rect(0,0,1100,630));backup.UpdateLayout();Capturar(backup,$"drive-backup-{escuro}");
                    Assert.Contains("responsável",backup.ViewModel.StatusGoogleDrive);
                }
            }
            finally{TemaInterface.Aplicar(false,false);}
        });
    }
    [Fact]
    public void CadastroEdicaoEPersonalizacaoPreservamMedidasEPreco()
    {
        using var banco=new BancoDeHomologacao();var produto=banco.CriarProduto("Almoço por quilo",4500,"Refeições");
        using(var db=banco.Fabrica.CreateDbContext()){db.Produtos.Single().UnidadeVenda=UnidadeVenda.Quilo;db.SaveChanges();}
        var repo=new EfComandaRepository(banco.Fabrica);var comanda=repo.AbrirComanda(1,DateTime.UtcNow);
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var pedido=OperacoesComandaRegressaoTests.Criar(banco,repo);pedido.SelecionarComandaCommand.Execute(comanda);
            using var catalogo=new LocalizarProdutoViewModel(pedido);
            var item=new PersonalizarItemViewModel(catalogo,catalogo.Produtos.Single());
            Assert.Equal("0,100",item.Quantidade);item.Quantidade="0,350";Assert.Contains("15,75",item.Total);Assert.True(item.Adicionar());
            var gravado=repo.BuscarComItens(comanda.Id)!.Itens.Single();Assert.Equal(0.350m,gravado.Quantidade);Assert.Equal(1575,gravado.SubtotalCentavos);
            try
            {
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,false);
                    var janela=new PersonalizarItemView(catalogo,catalogo.Produtos.Single()) { Width=700,Height=440 };
                    janela.ViewModel.Quantidade="0,350";janela.Show();janela.UpdateLayout();Capturar(janela,$"medida-personalizar-{escuro}");janela.Close();
                    var vm=CarregamentoDeXamlTests.CriarProdutos();vm.ProdutoSelecionado=vm.Produtos.Single();vm.VendaQuilo=true;vm.SalvarCommand.Execute(null);
                    Assert.Equal(UnidadeVenda.Quilo,vm.Produtos.Single().UnidadeVenda);
                    vm.NovoCommand.Execute(null);Assert.True(vm.VendaUnidade);vm.NomeProduto="Suco por litro";vm.CategoriaProduto=vm.Categorias.First();vm.PrecoProdutoReais="12,00";vm.VendaLitro=true;vm.SalvarCommand.Execute(null);
                    Assert.Equal(UnidadeVenda.Litro,vm.Produtos.Single(p=>p.Nome=="Suco por litro").UnidadeVenda);
                    var view=new ProdutosView(vm);
                    view.Dispatcher.BeginInvoke(new Action(()=>
                    {
                        var modal=System.Windows.Application.Current.Windows.OfType<Window>().Single(w=>w.Title=="Novo produto" && ReferenceEquals(w.DataContext,vm));
                        vm.VendaQuilo=true;modal.Width=760;modal.Height=520;modal.UpdateLayout();Capturar(modal,$"medida-cadastro-{escuro}");
                        Assert.Equal(3,AlinhamentoInterfaceTests.Filhos((FrameworkElement)modal.Content).OfType<RadioButton>().Count(r=>r.GroupName=="MedidaVenda"));modal.Close();
                    }),System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    view.AbrirNovoProduto();
                }
            }
            finally{TemaInterface.Aplicar(false,false);}
        });
    }
    private sealed class Relogio:TimeProvider
    {
        public double Segundos;public override long TimestampFrequency=>1000;public override long GetTimestamp()=>(long)(Segundos*1000);
    }
    private sealed class SemCredenciais:IGoogleDriveConnection
    {
        public bool Configurada=>false;public bool Conectada=>false;
        public void ImportarCredenciais(string arquivo)=>throw new NotSupportedException();
        public Task ConectarAsync(CancellationToken ct=default)=>Task.FromException(new InvalidOperationException("Conexão ainda não configurada nesta instalação."));
        public Task DesconectarAsync(CancellationToken ct=default)=>Task.CompletedTask;
        public Task<string> ObterAccessTokenAsync(CancellationToken ct=default)=>throw new NotSupportedException();
    }
    private static void Capturar(FrameworkElement view,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrEmpty(pasta))return;
        Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap((int)view.ActualWidth,(int)view.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));png.Save(arquivo);
    }
}
