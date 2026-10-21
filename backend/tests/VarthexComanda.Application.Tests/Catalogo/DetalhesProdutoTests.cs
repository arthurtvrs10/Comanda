using VarthexComanda.Application.Catalogo;

namespace VarthexComanda.Application.Tests.Catalogo;

public class DetalhesProdutoTests
{
    [Fact]
    public void CadastroEEdicaoPersistemDetalhesSemApagarQuandoChamadorAntigoNaoEnvia()
    {
        var produtos=new FakeProdutoRepository();var categorias=new FakeCategoriaRepository();var relogio=new FakeClock();
        var categoria=new CadastrarCategoria(categorias,relogio).Executar("Refeições").Valor!;
        var cadastrar=new CadastrarProduto(produtos,categorias,relogio);var alterar=new AlterarProduto(produtos,categorias,relogio);
        var produto=cadastrar.Executar("Prato",categoria.Id,2500,new(1200,"  Arroz e feijão  "," REF-01 "),false).Valor!;
        Assert.Equal(1200,produto.CustoCentavos);Assert.Equal("Arroz e feijão",produto.Descricao);Assert.Equal("REF-01",produto.CodigoPersonalizado);Assert.False(produto.Ativo);
        Assert.True(alterar.Executar(produto.Id,"Prato do dia",categoria.Id,2600,true).Sucesso);Assert.Equal(1200,produto.CustoCentavos);Assert.Equal("REF-01",produto.CodigoPersonalizado);
        Assert.True(alterar.Executar(produto.Id,"Prato do dia",categoria.Id,2600,true,new(1300,"Nova descrição","REF-01")).Sucesso);
        Assert.Equal(1300,produto.CustoCentavos);Assert.Equal("Nova descrição",produto.Descricao);
    }
    [Fact]
    public void CodigoDuplicadoOuDadosInvalidosNaoAlteramProduto()
    {
        var produtos=new FakeProdutoRepository();var categorias=new FakeCategoriaRepository();var relogio=new FakeClock();
        var categoria=new CadastrarCategoria(categorias,relogio).Executar("Refeições").Valor!;var cadastrar=new CadastrarProduto(produtos,categorias,relogio);
        var primeiro=cadastrar.Executar("Prato",categoria.Id,2500,new(100,"Descrição","REF-01")).Valor!;
        Assert.False(cadastrar.Executar("Outro",categoria.Id,500,new(0,null,"ref-01")).Sucesso);
        var alterar=new AlterarProduto(produtos,categorias,relogio);
        Assert.False(alterar.Executar(primeiro.Id,"Não salvar",categoria.Id,2600,true,new(-1)).Sucesso);Assert.Equal("Prato",primeiro.Nome);Assert.Equal(100,primeiro.CustoCentavos);
        Assert.False(cadastrar.Executar("Outro",categoria.Id,500,new(0,new string('a',2001))).Sucesso);
        Assert.False(cadastrar.Executar("Outro",categoria.Id,500,new(0,null,"código com espaços")).Sucesso);
        Assert.Single(produtos.Pesquisar(null,null));
    }
}
