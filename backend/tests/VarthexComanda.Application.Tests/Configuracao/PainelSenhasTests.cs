using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Application.Tests.Configuracao;

public class PainelSenhasTests
{
    [Fact]
    public void ChamadaMoveAnterioresParaDireitaSemDuplicarRepeticao()
    {
        var estado=new EstadoPainelSenhas();
        estado.Chamar("001");estado.Chamar("002");estado.Chamar("003");
        Assert.Equal("003",estado.Atual);Assert.Equal(new[]{"002","001"},estado.Anteriores);
        estado.Chamar("003");Assert.Equal(new[]{"002","001"},estado.Anteriores);
        estado.Chamar("001");Assert.Equal("001",estado.Atual);Assert.Equal(new[]{"003","002"},estado.Anteriores);
        for(int i=4;i<10;i++)estado.Chamar(i.ToString("000"));
        Assert.Equal("009",estado.Atual);Assert.Equal(new[]{"008","007","006","005","004"},estado.Anteriores);
    }
    [Theory]
    [InlineData("")][InlineData("12a")][InlineData("1234567890")][InlineData("１２")][InlineData("-1")]
    public void SenhaInvalidaNaoAlteraChamada(string senha)
    {
        var estado=new EstadoPainelSenhas();estado.Chamar("100");
        Assert.Throws<ArgumentException>(()=>estado.Chamar(senha));Assert.Equal("100",estado.Atual);Assert.Empty(estado.Anteriores);
    }
    [Fact]
    public void ExpiracaoLimpaApenasAtualEMantemHistoricoERechamadaRenovaTempo()
    {
        var relogio=new Relogio();var estado=new EstadoPainelSenhas(relogio);
        estado.Chamar("101");relogio.Agora+=TimeSpan.FromSeconds(59);estado.VerificarExpiracao(true);Assert.Equal("101",estado.Atual);
        estado.Chamar("101");relogio.Agora+=TimeSpan.FromSeconds(2);estado.VerificarExpiracao(true);Assert.Equal("101",estado.Atual);
        relogio.Agora+=TimeSpan.FromSeconds(58);estado.VerificarExpiracao(false);Assert.Equal("101",estado.Atual);
        estado.VerificarExpiracao(true);Assert.Null(estado.Atual);Assert.Equal(new[]{"101"},estado.Anteriores);
        estado.Chamar("102");Assert.Equal(new[]{"101"},estado.Anteriores);
        estado.Limpar();Assert.Null(estado.Atual);Assert.Empty(estado.Anteriores);
    }
    [Fact]
    public void PreferenciasPersistemEntreSessoesESalvamentoInvalidoPreservaDados()
    {
        var repo=new FakeConfiguracaoRepository();var service=new PainelSenhasService(repo);
        var p=service.Carregar();p.Monitor="DISPLAY2";p.CorInicial="#000000";p.CorNumeros="#FFFFFF";p.Voz=false;p.TelaCheia=false;service.Salvar(p);
        var carregada=new PainelSenhasService(repo).Carregar();Assert.Equal("DISPLAY2",carregada.Monitor);Assert.False(carregada.TelaCheia);Assert.False(carregada.Voz);Assert.Equal("#000000",carregada.CorInicial);
        p.CorInicial="vermelho";Assert.Throws<ArgumentException>(()=>service.Salvar(p));Assert.Equal("#000000",service.Carregar().CorInicial);
    }
    private sealed class Relogio:TimeProvider
    {
        public DateTimeOffset Agora { get; set; }=new(2026,10,20,12,0,0,TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow()=>Agora;
    }
}
