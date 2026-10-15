namespace VarthexComanda.Desktop.Tests;

public class AcessoAtendimentoTests
{
    private sealed class Relogio : TimeProvider
    {
        public DateTimeOffset Agora = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Agora;
    }

    [Fact]
    public void PinPersisteBloqueioRevogaERepetidasFalhasExigemEspera()
    {
        var arquivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pin");
        try
        {
            var relogio = new Relogio();
            var acesso = new AcessoAtendimento(arquivo, relogio);
            Assert.False(acesso.Configurado);
            Assert.Throws<ArgumentException>(() => acesso.DefinirPin("abcd"));
            Assert.Throws<ArgumentException>(() => acesso.DefinirPin("123"));
            acesso.DefinirPin("012345");
            Assert.False(acesso.Autenticado);
            Assert.True(acesso.Entrar("012345", out _));
            acesso.Bloquear();
            Assert.False(acesso.Autenticado);
            acesso = new AcessoAtendimento(arquivo, relogio);
            Assert.False(acesso.Autenticado);
            for (int i = 0; i < 5; i++) Assert.False(acesso.Entrar("9999", out _));
            Assert.False(acesso.Entrar("012345", out var aviso));
            Assert.Contains("Aguarde", aviso);
            relogio.Agora = relogio.Agora.AddSeconds(31);
            Assert.True(acesso.Entrar("012345", out _));
            acesso.DefinirPin("9876");
            Assert.False(acesso.Autenticado);
            Assert.False(acesso.Entrar("012345", out _));
            Assert.True(acesso.Entrar("9876", out _));
            File.WriteAllBytes(arquivo, [0]);
            Assert.Throws<InvalidOperationException>(() => acesso.Entrar("9876", out _));
            Assert.False(acesso.Autenticado);
        }
        finally { File.Delete(arquivo); }
    }
}
