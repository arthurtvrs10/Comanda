using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Application.Tests.Configuracao;

public class ConfiguracaoServidorTests
{
    [Fact]
    public void PadraoIniciaAutomaticamenteEPortaPersisteSemAlterarOutrasPreferencias()
    {
        var repo=new FakeConfiguracaoRepository();repo.Definir("backup.pasta_externa","D:\\backups",DateTime.UtcNow);
        var service=new ConfiguracaoServidorService(repo);
        Assert.Equal(new ConfiguracaoServidor(true,3001),service.Carregar());
        service.Salvar(new(false,4567),DateTime.UtcNow);
        Assert.Equal(new ConfiguracaoServidor(false,4567),new ConfiguracaoServidorService(repo).Carregar());
        Assert.Equal("D:\\backups",repo.ObterValor("backup.pasta_externa"));
        Assert.Throws<ArgumentException>(()=>service.Salvar(new(true,80),DateTime.UtcNow));
        Assert.Equal(new ConfiguracaoServidor(false,4567),service.Carregar());
    }
    [Theory]
    [InlineData("servidor.porta","0")]
    [InlineData("servidor.porta","70000")]
    [InlineData("servidor.iniciar_automaticamente","talvez")]
    public void ConfiguracaoInvalidaNaoViraInicioSilencioso(string chave,string valor)
    {
        var repo=new FakeConfiguracaoRepository();repo.Definir(chave,valor,DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(()=>new ConfiguracaoServidorService(repo).Carregar());
    }
}
