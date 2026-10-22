using System.Windows.Controls;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class PrimeiraSenhaTests
{
    [Fact]
    public void PrimeiroCadastroNaoMostraSenhaAtual()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var pasta=System.IO.Path.Combine(System.IO.Path.GetTempPath(),Guid.NewGuid().ToString());
            var gestor=new AcessoGestor(System.IO.Path.Combine(pasta,"gestor"));
            var atendimento=new AcessoAtendimento(System.IO.Path.Combine(pasta,"pin"));
            var janela=new JanelaAlterarAcesso(gestor,atendimento,false);
            var painel=(StackPanel)((ScrollViewer)janela.Content).Content;
            Assert.Equal("Definir senha do gestor",janela.Title);
            Assert.Equal(4,painel.Children.OfType<PasswordBox>().Count());
            Assert.DoesNotContain(painel.Children.OfType<Label>(),l=>Equals(l.Content,"Senha atual"));
            janela.Close();
        });
    }
}
