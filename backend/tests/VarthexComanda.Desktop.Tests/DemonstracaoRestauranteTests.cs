using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Storage;
using VarthexComanda.Infrastructure.Tests.Homologacao;
namespace VarthexComanda.Desktop.Tests;
public class DemonstracaoRestauranteTests
{
    private static string Origem=>Path.Combine(AppContext.BaseDirectory,"demo-restaurante");
    [Fact] public void RestauranteTemDezesseisProdutosComFotosEUnidadeCorreta()
    {
        using var b=new BancoDeHomologacao();using var db=b.Fabrica.CreateDbContext();Assert.True(CatalogoDemonstracao.Inicializar(db,b.Paths,Origem));Assert.Equal(16,db.Produtos.Count());Assert.Equal(4,db.Categorias.Count());Assert.Empty(db.Comandas);Assert.Empty(db.Vendas);
        Assert.Equal(1,db.Produtos.Count(p=>p.UnidadeVenda==UnidadeVenda.Quilo));var kg=db.Produtos.Single(p=>p.Nome=="Marmita por quilo");Assert.Equal(5490,kg.PrecoCentavos);
        foreach(var p in db.Produtos){Assert.True(p.Ativo);Assert.True(File.Exists(Path.Combine(b.Paths.FotosDirectory,p.FotoArquivo!)));}
    }
    [Fact] public void MarmitaNoQuiloCalculaConsumoPorPesoEAAtualizacaoPreservaEdicao()
    {
        using var b=new BancoDeHomologacao();int id;
        using(var db=b.Fabrica.CreateDbContext()){CatalogoDemonstracao.Inicializar(db,b.Paths,Origem);id=db.Produtos.Single(p=>p.UnidadeVenda==UnidadeVenda.Quilo).Id;}
        var r=new EfComandaRepository(b.Fabrica);var c=r.AbrirComanda(1,DateTime.UtcNow);using var leitura=b.Fabrica.CreateDbContext();var selecionado=leitura.Produtos.AsNoTracking().Single(p=>p.Id==id);r.AdicionarItem(c.Id,selecionado,.450m,DateTime.UtcNow);
        using(var db=b.Fabrica.CreateDbContext()){var item=Assert.Single(db.ItensComanda);Assert.Equal(.450m,item.Quantidade);Assert.Equal(2471,item.SubtotalCentavos);var produto=db.Produtos.Single(p=>p.Id==id);produto.PrecoCentavos=6000;produto.Ativo=false;db.SaveChanges();Assert.False(CatalogoDemonstracao.Inicializar(db,b.Paths,Origem));}
        using(var db=b.Fabrica.CreateDbContext()){Assert.Equal(16,db.Produtos.Count());var p=db.Produtos.Single(p=>p.Id==id);Assert.Equal(6000,p.PrecoCentavos);Assert.False(p.Ativo);}
    }
    [Fact] public void RestauranteNuncaUsaBaseRealOuDaLanchonete()
    {
        var perfil=Path.Combine(Path.GetTempPath(),"perfil-demo-teste");Assert.True(ModoExecucao.Demonstracao(["--demonstracao-restaurante"],perfil));Assert.True(ModoExecucao.Restaurante(["--demonstracao-restaurante"],perfil));
        var restaurante=ModoExecucao.RaizDados(true,"dados-reais",perfil,true);Assert.Equal(Path.Combine(perfil,"VarthexComandaDemoRestaurante"),restaurante);Assert.NotEqual(ModoExecucao.RaizDados(true,"dados-reais",perfil),restaurante);Assert.NotEqual("dados-reais",restaurante);
    }
    [Theory][InlineData("INVALIDA")][InlineData("L")]
    public void UnidadeInvalidaNaoCriaCatalogoParcial(string unidade)
    {
        using var b=new BancoDeHomologacao();var origem=Path.Combine(b.Paths.Root,"catalogo-invalido");Directory.CreateDirectory(origem);File.WriteAllText(Path.Combine(origem,"catalogo.json"),JsonSerializer.Serialize(new[]{new CatalogoDemonstracao.Item("Refeições","Marmita",2200,"foto.png",unidade)}));
        using var db=b.Fabrica.CreateDbContext();Assert.Throws<InvalidDataException>(()=>CatalogoDemonstracao.Inicializar(db,b.Paths,origem));Assert.Empty(db.Produtos);Assert.Empty(db.Categorias);
    }
}
