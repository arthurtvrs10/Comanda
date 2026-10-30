using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Backup;
using VarthexComanda.Application.Tests.Backup;
using VarthexComanda.Desktop.Backup;
using VarthexComanda.Desktop.Tests.Xaml;
using VarthexComanda.Infrastructure.Backup;
using VarthexComanda.Infrastructure.Storage;
namespace VarthexComanda.Desktop.Tests.Backup;
public class OneDrivePadraoTests
{
    private static BackupViewModel Criar(AppPaths paths)=>new(new CriarBackupManual(new FakeBackupService()),new RestaurarBackup(new FakeBackupService()),new ListarBackupsRecentes(new FakeBackupRegistroRepository()),new FakeConfirmadorDeBackup(),paths:paths);
    [Fact] public void DetectaPastasExistentesSemDuplicarAliasOuCriarPastas()
    {
        var vars=new Dictionary<string,string>{{"OneDriveConsumer",@"C:\Nuvem\Pessoal"},{"OneDriveCommercial",@"D:\Empresa"},{"OneDrive",@"c:\nuvem\pessoal\"}};
        var pastas=PastasOneDrive.Detectar(n=>vars.GetValueOrDefault(n),p=>p.StartsWith(@"C:\Nuvem",StringComparison.OrdinalIgnoreCase)||p==@"D:\Empresa",@"C:\Perfil");
        Assert.Equal(new[]{@"C:\Nuvem\Pessoal",@"D:\Empresa"},pastas);
        Assert.Empty(PastasOneDrive.Detectar(_=>"relativa",_=>false,@"C:\Perfil"));
    }
    [Fact] public void ConfiguracaoSoEhGravadaAoConfirmarEPreservaOutrosDestinos()
    {
        using var pasta=new PastaTeste();var paths=new AppPaths(Path.Combine(pasta.Raiz,"dados"));var nuvem=Directory.CreateDirectory(Path.Combine(pasta.Raiz,"OneDrive")).FullName;
        new PreferenciasBackup(paths).Salvar(new(Local:pasta.Raiz,GoogleDrive:"drive-anterior"));
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var vm=Criar(paths);var janela=new ConfigurarOneDriveView(vm,[nuvem]);janela.Loaded+=(_,_)=>AlinhamentoInterfaceTests.Filhos((FrameworkElement)janela.Content).OfType<Button>().Single(b=>Equals(b.Content,"Usar OneDrive")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.True(janela.ShowDialog());
            var salvo=new PreferenciasBackup(paths).Ler();Assert.Equal(nuvem,salvo.OneDrive);Assert.Equal(pasta.Raiz,salvo.Local);Assert.Equal("drive-anterior",salvo.GoogleDrive);Assert.False(Criar(paths).DeveSugerirOneDrive);
        });
    }
    [Fact] public void AdiarNaoConfiguraPastaNemExigeEspera()
    {
        using var pasta=new PastaTeste();var paths=new AppPaths(pasta.Raiz);
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var vm=Criar(paths);Assert.True(vm.DeveSugerirOneDrive);var janela=new ConfigurarOneDriveView(vm,[]);janela.Loaded+=(_,_)=>{var depois=AlinhamentoInterfaceTests.Filhos((FrameworkElement)janela.Content).OfType<Button>().Single(b=>Equals(b.Content,"Configurar depois"));Assert.True(depois.IsEnabled);depois.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));};janela.ShowDialog();Assert.Null(new PreferenciasBackup(paths).Ler().OneDrive);
        });
    }
    [Fact] public void TelaExplicaSincronizacaoEFuncionaNosDoisTemas()
    {
        using var pasta=new PastaTeste();
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();try{foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);var janela=new ConfigurarOneDriveView(Criar(new(pasta.Raiz)),[]);var raiz=(FrameworkElement)janela.Content;raiz.Measure(new Size(500,520));raiz.Arrange(new Rect(0,0,500,520));raiz.UpdateLayout();var filhos=AlinhamentoInterfaceTests.Filhos(raiz).ToArray();
                Assert.Contains(filhos.OfType<Button>(),b=>System.Windows.Automation.AutomationProperties.GetName(b).StartsWith("Ajuda: Pasta do OneDrive"));Assert.Contains(filhos.OfType<TextBlock>(),t=>t.Text.Contains("aguardando sincronização"));Assert.DoesNotContain(filhos.OfType<Button>(),b=>b.Content?.ToString()?.Contains("Google")==true);
                var dest=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(dest is null)continue;Directory.CreateDirectory(dest);var bitmap=new RenderTargetBitmap(500,520,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var f=File.Create(Path.Combine(dest,$"onedrive-padrao-{(escuro?"escuro":"claro")}.png"));png.Save(f);
            }}finally{TemaInterface.Aplicar(false,false);}
        });
    }
    private sealed class PastaTeste:IDisposable
    {
        public string Raiz {get;}=Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),"vcomanda-onedrive-"+Guid.NewGuid().ToString("N"))).FullName;
        public void Dispose()=>Directory.Delete(Raiz,true);
    }
}
