using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;
using Xunit;

namespace VarthexComanda.Application.Tests.Configuracao;

public class PreferenciasSistemaTests
{
    [Fact]
    public void SalvarERecarregarPreservaPreferenciasSemAlterarBackup()
    {
        var repo=new FakeConfiguracaoRepository();repo.Definir("backup.pasta_externa","D:\\copias",DateTime.UtcNow);
        var service=new ConfiguracoesSistemaService(repo);var p=service.Carregar();
        p.Cidade="São Paulo";p.CorFundo="#323130";p.OrigensCliente.Add("Evento");p.Cozinhas[0].Produtos.Add(42);
        service.Salvar(p,"Meu estabelecimento",30,DateTime.UtcNow);
        var salvo=new ConfiguracoesSistemaService(repo).Carregar();
        Assert.Equal("São Paulo",salvo.Cidade);Assert.Contains("Evento",salvo.OrigensCliente);Assert.Contains(42,salvo.Cozinhas[0].Produtos);
        Assert.Equal("30",repo.ObterValor("comandas.quantidade_maxima"));Assert.Equal("D:\\copias",repo.ObterValor("backup.pasta_externa"));
    }
    [Fact]
    public void DadosInvalidosNaoGravamPreferencias()
    {
        var repo=new FakeConfiguracaoRepository();var service=new ConfiguracoesSistemaService(repo);var p=new PreferenciasSistema { CorFundo="azul" };
        Assert.Throws<ArgumentException>(()=>service.Salvar(p,"Loja",20,DateTime.UtcNow));Assert.Null(repo.ObterValor(ConfiguracoesSistemaService.Chave));
        p.CorFundo="#FFFFFF";p.OrigensCliente=["Google","google"];
        Assert.Throws<ArgumentException>(()=>service.Salvar(p,"Loja",20,DateTime.UtcNow));
    }
    [Fact]
    public void EtiquetaPorValorConfereDigitoVerificadorSemRegistrarVenda()
    {
        var prefixo="200042001234";var soma=prefixo.Select((c,i)=>(c-'0')*(i%2==0?1:3)).Sum();var codigo=prefixo+((10-soma%10)%10);
        var resultado=ConfiguracoesSistemaService.InterpretarEtiqueta(codigo,new());
        Assert.Equal(42,resultado.Produto);Assert.Equal(1234,resultado.ValorCentavos);
        Assert.Throws<ArgumentException>(()=>ConfiguracoesSistemaService.InterpretarEtiqueta(codigo[..12]+((codigo[12]-'0'+1)%10),new()));
    }
}
