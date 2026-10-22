using System.Reflection;
using System.Windows;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class FluxoSenhaTests
{
    private static System.Windows.Controls.Button EncontrarBotao(DependencyObject pai)
    {
        if(pai is System.Windows.Controls.Button b&&Equals(b.Content,"Bloquear ou trocar usuário"))return b;
        for(var i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(pai);i++)
        {
            var encontrado=EncontrarBotao(System.Windows.Media.VisualTreeHelper.GetChild(pai,i));
            if(encontrado is not null)return encontrado;
        }
        return null!;
    }

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
                var camada=(FrameworkElement)janela.FindName("CamadaBloqueio");
                var area=(FrameworkElement)janela.FindName("AreaSistema");
                Assert.Equal(solicitar?Visibility.Visible:Visibility.Collapsed,camada.Visibility);
                Assert.Equal(!solicitar,area.IsEnabled);
                if(!solicitar)Assert.Null(((AreaTrabalhoInterna)janela.FindName("ConteudoPrincipal")).JanelaAtiva);
                var botao=EncontrarBotao((DependencyObject)janela.Content);
                botao.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                Assert.Equal(solicitar?Visibility.Visible:Visibility.Collapsed,camada.Visibility);
                if(!solicitar)
                {
                    var gestor=(AcessoGestor)typeof(MainWindow).GetField("_acesso",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(janela)!;
                    Assert.True(gestor.Autenticado);
                    Assert.False(gestor.Configurado);
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
