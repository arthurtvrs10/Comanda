namespace VarthexComanda.Desktop.Tests;

public sealed class RecuperacaoGestorTests
{
    [Fact]
    public void CredencialAntigaPermiteCadastrarRecuperacaoSomenteComSenhaCorreta()
    {
        var caminho=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".credencial");
        try
        {
            var acesso=new AcessoGestor(caminho);acesso.Cadastrar("1234");
            Assert.False(acesso.RecuperacaoConfigurada);
            Assert.Throws<InvalidOperationException>(()=>acesso.DefinirSenhaRecuperacao("errada","recuperacao"));
            Assert.Equal(64,File.ReadAllBytes(caminho).Length);
            acesso.DefinirSenhaRecuperacao("1234","recuperacao");
            Assert.True(acesso.RecuperacaoConfigurada);
            Assert.True(acesso.Entrar("1234"));acesso.AlterarSenha("1234","abcd");
            acesso.RecuperarSenha("recuperacao","9876");Assert.True(acesso.Entrar("9876"));
        }
        finally{File.Delete(caminho);}
    }
}
