using System.Reflection;
using System.Windows;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class FluxoSenhaTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreferenciaPersisteEControlaBloqueioAposAtendimento(bool solicitar)
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var repo=new FakeConfiguracaoRepository();
            new ConfiguracoesSistemaService(repo).Salvar(new(){SolicitarSenha=solicitar},"Loja",20,DateTime.UtcNow);
            Assert.Equal(solicitar,new ConfiguracoesSistemaService(repo).Carregar().SolicitarSenha);
            var pasta=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"fluxo-senha-"+Guid.NewGuid());
            var janela=CarregamentoDeXamlTests.CriarMainWindow(new(pasta),configuracoes:repo);
            try
            {
                var bloquear=typeof(MainWindow).GetMethod("BloquearAtendimento",BindingFlags.NonPublic|BindingFlags.Instance,null,Type.EmptyTypes,null)!;
                var camada=(FrameworkElement)janela.FindName("CamadaBloqueio");
                Assert.Equal(Visibility.Visible,camada.Visibility);
                var acesso=(AcessoAtendimento)typeof(MainWindow).GetField("_acessoAtendimento",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(janela)!;
                acesso.DefinirPin("1234");Assert.True(acesso.Entrar("1234",out _));
                typeof(MainWindow).GetMethod("DesbloquearAtendimento",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(janela,null);
                bloquear.Invoke(janela,null);
                Assert.Equal(solicitar?Visibility.Visible:Visibility.Collapsed,camada.Visibility);
                if(!solicitar)
                {
                    typeof(MainWindow).GetMethod("MenuSessao_Click",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(janela,new object[]{janela,new RoutedEventArgs()});
                    Assert.Equal(Visibility.Collapsed,camada.Visibility);
                    typeof(MainWindow).GetMethod("Bloquear_Click",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(janela,new object[]{janela,new RoutedEventArgs()});
                    Assert.Equal(Visibility.Visible,camada.Visibility);
                    bloquear.Invoke(janela,null);Assert.Equal(Visibility.Visible,camada.Visibility);
                    Assert.True(acesso.Entrar("1234",out _));
                    typeof(MainWindow).GetMethod("DesbloquearAtendimento",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(janela,null);
                    new ConfiguracoesSistemaService(repo).Salvar(new(){SolicitarSenha=true},"Loja",20,DateTime.UtcNow);
                    bloquear.Invoke(janela,null);
                    Assert.Equal(Visibility.Visible,camada.Visibility);
                }
            }
            finally
            {
                janela.Close();
                if(System.IO.Directory.Exists(pasta))System.IO.Directory.Delete(pasta,true);
            }
        });
    }
}
