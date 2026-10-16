namespace VarthexComanda.Desktop.Tests;

public class AcessoGestorTests
{
    [Fact]
    public void TrocaDeSenhaExigeSenhaAtualEPreservaArquivoEmFalha()
    {
        var caminho=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".credencial");
        try {
            var acesso=new AcessoGestor(caminho);acesso.Cadastrar("senha-antiga");
            Assert.Throws<InvalidOperationException>(()=>acesso.AlterarSenha("errada","senha-nova"));
            Assert.True(acesso.Autenticado);
            Assert.True(acesso.ConferirSenha("senha-antiga"));
            acesso.AlterarSenha("senha-antiga","senha-nova");
            var novo=new AcessoGestor(caminho);
            Assert.False(novo.Entrar("senha-antiga"));Assert.True(novo.Entrar("senha-nova"));
            novo.Sair();Assert.Throws<InvalidOperationException>(()=>novo.AlterarSenha("senha-nova","outra-senha"));
        } finally {File.Delete(caminho);}
    }
    [Fact]
    public void SenhaPersisteExigeAutenticacaoESairRevogaAcesso()
    {
        var caminho = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".credencial");
        try
        {
            var acesso = new AcessoGestor(caminho);
            Assert.False(acesso.Autenticado);
            Assert.Throws<ArgumentException>(() => acesso.Cadastrar("123"));
            acesso.Cadastrar("senha-teste-123");
            Assert.True(acesso.Autenticado);
            Assert.Throws<InvalidOperationException>(() => acesso.Cadastrar("outra-senha"));
            acesso.Sair();
            Assert.False(acesso.Autenticado);
            var novaSessao = new AcessoGestor(caminho);
            Assert.True(novaSessao.Configurado);
            Assert.False(novaSessao.Autenticado);
            Assert.False(novaSessao.Entrar("incorreta"));
            Assert.True(novaSessao.Entrar("senha-teste-123"));
            Assert.False(novaSessao.Entrar("incorreta"));
            Assert.False(novaSessao.Autenticado);
            File.WriteAllBytes(caminho, [1, 2]);
            Assert.Throws<InvalidOperationException>(() => novaSessao.Entrar("senha-teste-123"));
            Assert.False(novaSessao.Autenticado);
        }
        finally { File.Delete(caminho); }
    }
}
