using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;
using Xunit;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;

public class UnidadesVendaTests
{
    [Theory]
    [InlineData(UnidadeVenda.Quilo)]
    [InlineData(UnidadeVenda.Litro)]
    public void FracaoPersisteCalculaArredondaEConservaUnidadeHistorica(UnidadeVenda unidade)
    {
        using var banco=new BancoDeHomologacao();
        var produto=banco.CriarProduto("Produto por medida",4500);
        using(var db=banco.Fabrica.CreateDbContext()){db.Produtos.Single().UnidadeVenda=unidade;db.SaveChanges();}
        produto.UnidadeVenda=unidade;
        var repo=new EfComandaRepository(banco.Fabrica);
        var comanda=repo.AbrirComanda(1,DateTime.UtcNow);
        var item=repo.AdicionarItem(comanda.Id,produto,0.350m,DateTime.UtcNow).Itens.Single();
        Assert.Equal(1575,item.SubtotalCentavos);
        Assert.Equal("0,350",item.QuantidadeFormatada);
        repo.RegistrarImpressao(comanda.Id,new Dictionary<int,decimal> { [item.Id]=0.350m },true);
        using(var db=banco.Fabrica.CreateDbContext()){db.Produtos.Single().UnidadeVenda=UnidadeVenda.Unidade;db.SaveChanges();}
        var salvo=repo.BuscarComItens(comanda.Id)!.Itens.Single();
        Assert.Equal(unidade,salvo.UnidadeVenda);Assert.Equal(0.350m,salvo.QuantidadeCozinha);
        Assert.Equal(2025,repo.AlterarQuantidade(item.Id,0.450m,DateTime.UtcNow).Comanda.TotalCentavos);
        produto.UnidadeVenda=UnidadeVenda.Unidade;
        Assert.Equal(2,repo.AdicionarItem(comanda.Id,produto,1,DateTime.UtcNow).Itens.Count);
        Assert.Throws<InvalidOperationException>(()=>repo.AdicionarItem(comanda.Id,produto,0.5m,DateTime.UtcNow));
        Assert.Equal(101,MedidasVenda.CalcularSubtotal(100,1.005m));
        Assert.NotNull(MedidasVenda.ValidarQuantidade(0.0001m,unidade));
    }

    [Fact]
    public void AtualizacaoPreservaQuantidadesTotaisEContadoresAntigos()
    {
        using var banco=new BancoDeHomologacao(migrar:false);
        using(var db=banco.Fabrica.CreateDbContext())
        {
            db.GetService<IMigrator>().Migrate("20261021222856_AddOperacoesComanda");
            db.Database.ExecuteSqlRaw("""
                INSERT INTO categoria (id,nome,ativo,criado_em,atualizado_em) VALUES (1,'Geral',1,'2026-10-21','2026-10-21');
                INSERT INTO produto (id,categoria_id,nome,preco_centavos,ativo,criado_em,atualizado_em) VALUES (1,1,'Produto legado',500,1,'2026-10-21','2026-10-21');
                INSERT INTO comanda (id,numero,status,aberta_em,total_centavos) VALUES (1,1,'ABERTA','2026-10-21',1500);
                INSERT INTO item_comanda (id,comanda_id,produto_id,nome_produto,preco_unitario_centavos,quantidade,subtotal_centavos,quantidade_cozinha,quantidade_ficha,criado_em,atualizado_em)
                VALUES (1,1,1,'Produto legado',500,3,1500,2,1,'2026-10-21','2026-10-21');
                """);
            db.Database.Migrate();
        }
        var repo=new EfComandaRepository(banco.Fabrica);
        var atual=repo.BuscarComItens(1)!;var item=Assert.Single(atual.Itens);
        Assert.Equal(3,item.Quantidade);Assert.Equal(2,item.QuantidadeCozinha);Assert.Equal(1,item.QuantidadeFicha);
        Assert.Equal(UnidadeVenda.Unidade,item.UnidadeVenda);Assert.Equal(1500,atual.Comanda.TotalCentavos);
        Assert.Equal(2000,repo.AlterarQuantidade(1,4,DateTime.UtcNow).Comanda.TotalCentavos);
    }
}
