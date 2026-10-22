using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class LoginCentralizadoTests
{
    [Theory]
    [InlineData("gestor")]
    [InlineData("garcom")]
    [InlineData("cozinha")]
    public void LoginIndividualRespeitaPerfilERejeitaSenhaIncorreta(string perfil)
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var pasta=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"login-central-"+Guid.NewGuid());
            try
            {
                var atendimento=new AcessoAtendimento(System.IO.Path.Combine(pasta,"pin"));
                var gestor=new AcessoGestor(System.IO.Path.Combine(pasta,"gestor"));
                var contas=new ServicoColaboradores(System.IO.Path.Combine(pasta,"contas.json"));
                contas.Salvar(new(){Nome="Pessoa de teste",Login="pessoa",Perfil=perfil},"Ab12");
                var view=new BloqueioAtendimentoView(atendimento,gestor,contas);
                var login=(TextBox)view.FindName("Login");var senha=(PasswordBox)view.FindName("Senha");
                var entrar=(Button)view.FindName("BotaoEntrar");var desbloqueou=false;
                view.Desbloqueado+=()=>desbloqueou=true;
                login.Text="pessoa";senha.Password="incorreta";entrar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.False(desbloqueou);Assert.False(atendimento.Autenticado);Assert.False(gestor.Autenticado);
                senha.Password="Ab12";entrar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.True(desbloqueou);Assert.Equal(perfil,view.Perfil);
                Assert.Equal(perfil=="gestor",gestor.Autenticado);Assert.Equal(perfil!="gestor",atendimento.Autenticado);
                Assert.Empty(senha.Password);
                view.Reiniciar();Assert.Empty(login.Text);
                var saida=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                if(saida is not null&&perfil=="gestor")
                {
                    System.IO.Directory.CreateDirectory(saida);
                    foreach(var escuro in new[]{false,true})
                    {
                        TemaInterface.Aplicar(escuro);
                        view.Measure(new Size(1000,720));view.Arrange(new Rect(0,0,1000,720));view.UpdateLayout();
                        var bitmap=new RenderTargetBitmap(1000,720,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
                        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));
                        using var stream=System.IO.File.Create(System.IO.Path.Combine(saida,$"login-central-{escuro}.png"));png.Save(stream);
                    }
                    TemaInterface.Aplicar(false);
                }
            }
            finally{if(System.IO.Directory.Exists(pasta))System.IO.Directory.Delete(pasta,true);}
        });
    }
}
