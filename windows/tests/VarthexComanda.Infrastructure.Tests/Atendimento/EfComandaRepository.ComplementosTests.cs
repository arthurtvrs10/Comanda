using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;

public partial class EfComandaRepositoryTests
{
    [Fact]
    public void CatalogoImportaLegadosUmaVezERefletePrecosEInativacaoSemAlterarItensAnteriores()
    {
        var repo=new EfComandaRepository(_fabrica);var (produto,id)=PrepararComandaEProduto(repo);
        var gratis=new OpcaoComplemento { Nome="Molho grátis" };var pago=new OpcaoComplemento { Nome="Bacon",PrecoCentavos=250 };
        produto.GruposComplementos=[new() { Nome="Extras",Maximo=2,Opcoes=[gratis,pago] }];
        var produtos=new EfProdutoRepository(_fabrica);produtos.Salvar(produto);
        var antigo=repo.AdicionarItem(id,produto,1,DateTime.UtcNow,null,null,[pago.Id]).Itens.Single();
        produtos.PrepararCatalogoComplementos();produtos.PrepararCatalogoComplementos();
        var complementos=produtos.Pesquisar(null,null).Where(p=>p.EhComplemento).ToArray();Assert.Equal(2,complementos.Length);
        Assert.Contains(complementos,c=>c.PrecoCentavos==0);
        var vinculado=produtos.BuscarPorId(produto.Id)!;
        Assert.All(vinculado.GruposComplementos.Single().Opcoes,o=>Assert.NotNull(o.ComplementoId));
        var bacon=complementos.Single(c=>c.Nome=="Bacon");bacon.PrecoCentavos=500;produtos.Salvar(bacon);
        var atualizado=produtos.BuscarPorId(produto.Id)!;
        Assert.Equal(500,atualizado.GruposComplementos.Single().Opcoes.Single(o=>o.Id==pago.Id).PrecoCentavos);
        repo.AdicionarItem(id,atualizado,1,DateTime.UtcNow,null,null,[pago.Id]);
        var itens=repo.BuscarComItens(id)!.Itens;Assert.Equal(2,itens.Count);Assert.Equal(750,itens.Single(i=>i.Id==antigo.Id).SubtotalCentavos);
        bacon.Ativo=false;produtos.Salvar(bacon);
        Assert.Throws<InvalidOperationException>(()=>ComplementosProduto.Selecionar(produtos.BuscarPorId(produto.Id)!,[pago.Id]));
        Assert.Equal(2,new VarthexComanda.Application.Catalogo.PesquisarProdutos(produtos).Executar(null,null,true).Count);
        Assert.Single(new VarthexComanda.Application.Catalogo.PesquisarProdutos(produtos).Executar(null,null));
    }

    [Fact]
    public void ComplementosPersistemSomamPorUnidadeENaoMisturamEscolhasDoMesmoPreco()
    {
        var repo=new EfComandaRepository(_fabrica);var (produto,id)=PrepararComandaEProduto(repo);
        var a=new OpcaoComplemento { Nome="Queijo",PrecoCentavos=200 };var b=new OpcaoComplemento { Nome="Bacon",PrecoCentavos=200 };
        produto.GruposComplementos=[new() { Nome="Adicionais",Minimo=0,Maximo=2,Opcoes=[a,b] }];
        var produtos=new EfProdutoRepository(_fabrica);produtos.Salvar(produto);
        Assert.Equal(2,produtos.BuscarPorId(produto.Id)!.GruposComplementos.Single().Opcoes.Count);
        repo.AdicionarItem(id,produto,2,DateTime.UtcNow,null,"Caixa",[a.Id]);
        repo.AdicionarItem(id,produto,1,DateTime.UtcNow,null,"Caixa",[b.Id]);
        var estado=repo.AdicionarItem(id,produto,1,DateTime.UtcNow,null,"Caixa",[a.Id]);
        Assert.Equal(2,estado.Itens.Count);Assert.Equal(2800,estado.Comanda.TotalCentavos);
        Assert.Equal(3,estado.Itens.Single(i=>i.NomeProduto.Contains("Queijo")).Quantidade);
        Assert.All(estado.Itens,i=>Assert.Equal(700,i.PrecoUnitarioCentavos));
        var snapshot=estado.Itens[0].ComplementosJson;
        produto.GruposComplementos=[new() { Nome="Novo grupo",Opcoes=[new() { Nome="Outro",PrecoCentavos=900 }] }];produtos.Salvar(produto);
        var historico=new EfComandaRepository(_fabrica).BuscarComItens(id)!;
        Assert.Equal(snapshot,historico.Itens[0].ComplementosJson);Assert.Contains("Queijo",historico.Itens[0].NomeProduto);
        Assert.Equal(2800,repo.EncerrarComanda(id,DateTime.UtcNow).TotalCentavos);
    }

    [Fact]
    public void ComplementosObrigatoriosELimitesNaoGravamConsumoECopiaPreservaEscolhas()
    {
        var repo=new EfComandaRepository(_fabrica);var (produto,id)=PrepararComandaEProduto(repo);
        var a=new OpcaoComplemento { Nome="Molho A" };var b=new OpcaoComplemento { Nome="Molho B",PrecoCentavos=100 };
        produto.GruposComplementos=[new() { Nome="Molho",Minimo=1,Maximo=1,Opcoes=[a,b] }];new EfProdutoRepository(_fabrica).Salvar(produto);
        Assert.Throws<InvalidOperationException>(()=>repo.AdicionarItem(id,produto,1,DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(()=>repo.AdicionarItem(id,produto,1,DateTime.UtcNow,null,null,[a.Id,b.Id]));
        Assert.Throws<InvalidOperationException>(()=>repo.AdicionarItem(id,produto,1,DateTime.UtcNow,null,null,[Guid.NewGuid()]));
        Assert.Throws<InvalidOperationException>(()=>repo.AdicionarItem(id,produto,1,DateTime.UtcNow,null,null,[a.Id,a.Id]));
        Assert.Empty(repo.BuscarComItens(id)!.Itens);
        var item=repo.AdicionarItem(id,produto,1,DateTime.UtcNow,null,null,[b.Id]).Itens.Single();
        var destino=repo.AbrirComanda(20,DateTime.UtcNow);
        var copia=repo.TransferirItens(id,destino.Id,[item.Id],true,DateTime.UtcNow).Itens.Single();
        Assert.Equal(item.ComplementosJson,copia.ComplementosJson);Assert.Equal(600,copia.SubtotalCentavos);
    }
}
