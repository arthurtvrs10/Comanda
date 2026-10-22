namespace VarthexComanda.Desktop.Tests;

public sealed class SenhaRecuperacaoTests
{
    [Fact]
    public void SenhaRecuperacaoRedefineAcessoEPersisteAposTroca()
    {
        var caminho=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".credencial");
        try
        {
            var acesso=new AcessoGestor(caminho);
            Assert.Throws<ArgumentException>(()=>acesso.CadastrarComSenhaRecuperacao("1234","1234"));
            Assert.Throws<ArgumentException>(()=>acesso.CadastrarComSenhaRecuperacao("1234","abc"));
            Assert.False(File.Exists(caminho));
            acesso.CadastrarComSenhaRecuperacao("1234","MinhaRecuperacao");
            Assert.Equal(128,File.ReadAllBytes(caminho).Length);
            var antes=File.ReadAllBytes(caminho);
            Assert.Throws<InvalidOperationException>(()=>acesso.RecuperarSenha("errada","5678"));
            Assert.Equal(antes,File.ReadAllBytes(caminho));
            acesso.RecuperarSenha("MinhaRecuperacao","5678");
            Assert.False(acesso.Autenticado);Assert.False(acesso.Entrar("1234"));Assert.True(acesso.Entrar("5678"));
            acesso.AlterarSenha("5678","abcd");
            var outra=new AcessoGestor(caminho);outra.RecuperarSenha("MinhaRecuperacao","9876");Assert.True(outra.Entrar("9876"));
            outra.DefinirSenhaRecuperacao("9876","NovaRecuperacao");
            Assert.Throws<InvalidOperationException>(()=>outra.RecuperarSenha("MinhaRecuperacao","abcd"));
            outra.RecuperarSenha("NovaRecuperacao","abcd");Assert.True(outra.Entrar("abcd"));
        }
        finally{File.Delete(caminho);}
    }
}
