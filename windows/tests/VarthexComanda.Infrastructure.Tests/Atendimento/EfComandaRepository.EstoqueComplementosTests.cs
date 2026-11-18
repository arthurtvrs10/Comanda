using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;

public partial class EfComandaRepositoryTests
{
    private (EfComandaRepository Repo,Produto Produto,int Pedido,Produto Complemento,OpcaoComplemento Opcao) PrepararEstoque(long saldo=5,bool controlar=true)
    {
        var repo=new EfComandaRepository(_fabrica);var (produto,pedido)=PrepararComandaEProduto(repo);
        var produtos=new EfProdutoRepository(_fabrica);var agora=DateTime.UtcNow;
        var complemento=produtos.Salvar(new() { Id=0,EhComplemento=true,Nome="Bacon",PrecoCentavos=200,Ativo=true,CriadoEm=agora,AtualizadoEm=agora,ControlarEstoque=controlar,QuantidadeEstoque=saldo });
        var opcao=new OpcaoComplemento { ComplementoId=complemento.Id,Nome=complemento.Nome,PrecoCentavos=200 };
        produto.GruposComplementos=[new() { Nome="Adicionais",Maximo=1,Opcoes=[opcao] }];produtos.Salvar(produto);
        return(repo,produto,pedido,complemento,opcao);
    }
    private long Saldo(Produto complemento)=>new EfProdutoRepository(_fabrica).BuscarPorId(complemento.Id)!.QuantidadeEstoque;
    private sealed class RelogioEstoque:IClock { public DateTime UtcNow=>DateTime.UtcNow; }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void RegrasDoCadastroDoComplementoValemParaTodosOsProdutosVinculados()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque();var produtos=new EfProdutoRepository(_fabrica);
        var alterar=new AlterarProduto(produtos,new EfCategoriaRepository(_fabrica),new RelogioEstoque());
        Assert.True(alterar.Executar(complemento.Id,complemento.Nome,0,200,true,new(ControlarEstoque:true,QuantidadeEstoque:5,MinimoComplemento:1,MaximoComplemento:2)).Sucesso);
        var outro=new Produto { Id=0,CategoriaId=produto.CategoriaId,Nome="Outro lanche",PrecoCentavos=1000,Ativo=true,CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow,GruposComplementos=produto.GruposComplementos };
        produtos.Salvar(outro);
        foreach(var id in new[] { produto.Id,outro.Id })
        {
            var atual=produtos.BuscarPorId(id)!;var grupo=Assert.Single(atual.GruposComplementos);var regra=Assert.Single(grupo.Opcoes);
            Assert.Equal(1,regra.MinimoQuantidade);Assert.Equal(2,regra.MaximoQuantidade);
            Assert.Throws<InvalidOperationException>(()=>ComplementosProduto.Selecionar(atual,[]));
            Assert.Throws<InvalidOperationException>(()=>ComplementosProduto.Selecionar(atual,[opcao.Id,opcao.Id,opcao.Id]));
            Assert.Equal(2,ComplementosProduto.Selecionar(atual,[opcao.Id,opcao.Id]).Count);
        }
        repo.AdicionarItem(pedido,produtos.BuscarPorId(produto.Id)!,1,DateTime.UtcNow,null,null,[opcao.Id,opcao.Id]);Assert.Equal(3,Saldo(complemento));
        Assert.True(alterar.Executar(complemento.Id,complemento.Nome,0,200,true,new(ControlarEstoque:true,QuantidadeEstoque:3,MinimoComplemento:0,MaximoComplemento:1)).Sucesso);
        Assert.Equal(1,produtos.BuscarPorId(outro.Id)!.GruposComplementos.Single().Opcoes.Single().MaximoQuantidade);
        Assert.Throws<InvalidOperationException>(()=>ComplementosProduto.Selecionar(produtos.BuscarPorId(outro.Id)!,[opcao.Id,opcao.Id]));
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void FalhaEmUmComplementoDesfazABaixaDosDemais()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque();var produtos=new EfProdutoRepository(_fabrica);
        var outro=produtos.Salvar(new() { Id=0,EhComplemento=true,Nome="Queijo",PrecoCentavos=100,ControlarEstoque=true,QuantidadeEstoque=0,Ativo=true,CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow });
        var escolha=new OpcaoComplemento { ComplementoId=outro.Id,Nome=outro.Nome,PrecoCentavos=100 };
        produto.GruposComplementos=[new() { Nome="Extras",Maximo=2,Opcoes=[opcao,escolha] }];produtos.Salvar(produto);
        Assert.Throws<InvalidOperationException>(()=>repo.AdicionarItem(pedido,produto,1,DateTime.UtcNow,null,null,[opcao.Id,escolha.Id]));
        Assert.Equal(5,Saldo(complemento));Assert.Equal(0,Saldo(outro));Assert.Empty(repo.BuscarComItens(pedido)!.Itens);
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void MesmoComplementoEmDoisGruposConsomeEDevolveDuasUnidadesPorProduto()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque();var outra=opcao with { Id=Guid.NewGuid() };
        produto.GruposComplementos=[new() { Nome="Primeiro adicional",Opcoes=[opcao] },new() { Nome="Segundo adicional",Opcoes=[outra] }];
        new EfProdutoRepository(_fabrica).Salvar(produto);
        var item=repo.AdicionarItem(pedido,produto,2,DateTime.UtcNow,null,null,[opcao.Id,outra.Id]).Itens.Single();Assert.Equal(1,Saldo(complemento));
        repo.AlterarQuantidade(item.Id,1,DateTime.UtcNow);Assert.Equal(3,Saldo(complemento));
        repo.RemoverItem(item.Id,DateTime.UtcNow);Assert.Equal(5,Saldo(complemento));
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void ComplementoComEstoquePodeSerCadastradoSemNenhumaCategoria()
    {
        var produtos=new EfProdutoRepository(_fabrica);
        var caso=new CadastrarProduto(produtos,new EfCategoriaRepository(_fabrica),new RelogioEstoque());
        var resultado=caso.Executar("Bacon",0,200,new(ControlarEstoque:true,QuantidadeEstoque:8),complemento:true);
        Assert.True(resultado.Sucesso);var salvo=produtos.BuscarPorId(resultado.Valor!.Id)!;
        Assert.Null(salvo.CategoriaId);Assert.Equal(8,salvo.QuantidadeEstoque);Assert.True(salvo.ControlarEstoque);
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void LancarAlterarERemoverMovimentamEstoqueSemAceitarSaldoNegativo()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque();
        var item=repo.AdicionarItem(pedido,produto,2,DateTime.UtcNow,null,null,[opcao.Id]).Itens.Single();Assert.Equal(3,Saldo(complemento));
        Assert.Throws<InvalidOperationException>(()=>repo.AdicionarItem(pedido,produto,4,DateTime.UtcNow,null,null,[opcao.Id]));
        Assert.Equal(3,Saldo(complemento));Assert.Equal(2,repo.BuscarComItens(pedido)!.Itens.Single().Quantidade);
        repo.AlterarQuantidade(item.Id,3,DateTime.UtcNow);Assert.Equal(2,Saldo(complemento));
        repo.AlterarQuantidade(item.Id,1,DateTime.UtcNow);Assert.Equal(4,Saldo(complemento));
        repo.RemoverItem(item.Id,DateTime.UtcNow);Assert.Equal(5,Saldo(complemento));
        Assert.Throws<InvalidOperationException>(()=>repo.RemoverItem(item.Id,DateTime.UtcNow));Assert.Equal(5,Saldo(complemento));
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void CancelamentoDevolveSomenteAntesDoPreparoEFinalizacaoNaoDevolve()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque();
        repo.AdicionarItem(pedido,produto,2,DateTime.UtcNow,null,null,[opcao.Id]);repo.CancelarComanda(pedido,DateTime.UtcNow);Assert.Equal(5,Saldo(complemento));
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(()=>repo.CancelarComanda(pedido,DateTime.UtcNow));Assert.Equal(5,Saldo(complemento));
        pedido=repo.AbrirComanda(20,DateTime.UtcNow).Id;var item=repo.AdicionarItem(pedido,produto,2,DateTime.UtcNow,null,null,[opcao.Id]).Itens.Single();
        using(var contexto=_fabrica.CreateDbContext()){contexto.ItensComanda.Single(i=>i.Id==item.Id).StatusPreparo=StatusPreparo.EmPreparo;contexto.SaveChanges();}
        repo.CancelarComanda(pedido,DateTime.UtcNow);Assert.Equal(3,Saldo(complemento));
        pedido=repo.AbrirComanda(30,DateTime.UtcNow).Id;repo.AdicionarItem(pedido,produto,1,DateTime.UtcNow,null,null,[opcao.Id]);
        repo.EncerrarComanda(pedido,DateTime.UtcNow);Assert.Equal(2,Saldo(complemento));
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void CopiarConsomeNovamenteEMoverNaoMovimentaEstoque()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque();
        var item=repo.AdicionarItem(pedido,produto,2,DateTime.UtcNow,null,null,[opcao.Id]).Itens.Single();
        var destino=repo.AbrirComanda(20,DateTime.UtcNow);repo.TransferirItens(pedido,destino.Id,[item.Id],true,DateTime.UtcNow);Assert.Equal(1,Saldo(complemento));
        var outro=repo.AbrirComanda(30,DateTime.UtcNow);Assert.Throws<InvalidOperationException>(()=>repo.TransferirItens(pedido,outro.Id,[item.Id],true,DateTime.UtcNow));
        Assert.Empty(repo.BuscarComItens(outro.Id)!.Itens);Assert.Equal(1,Saldo(complemento));
        repo.TransferirItens(pedido,outro.Id,[item.Id],false,DateTime.UtcNow);Assert.Equal(1,Saldo(complemento));
        repo.CancelarComanda(pedido,DateTime.UtcNow);Assert.Equal(1,Saldo(complemento));
        repo.CancelarComanda(destino.Id,DateTime.UtcNow);Assert.Equal(3,Saldo(complemento));
        repo.EncerrarComanda(outro.Id,DateTime.UtcNow);Assert.Equal(3,Saldo(complemento));
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void CadastroDesatualizadoNaoSobrescreveEstoqueBaixadoPeloPedido()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque();var versao=complemento.VersaoEstoque;
        repo.AdicionarItem(pedido,produto,1,DateTime.UtcNow,null,null,[opcao.Id]);
        var alterar=new AlterarProduto(new EfProdutoRepository(_fabrica),new EfCategoriaRepository(_fabrica),new RelogioEstoque());
        var resultado=alterar.Executar(complemento.Id,complemento.Nome,0,200,true,new(ControlarEstoque:true,QuantidadeEstoque:5,VersaoEstoqueEsperada:versao));
        Assert.False(resultado.Sucesso);Assert.Equal(4,Saldo(complemento));
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public void AtivarControleDepoisDeUmLancamentoNaoCriaEstoqueAoCancelar()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque(controlar:false);
        var item=repo.AdicionarItem(pedido,produto,1,DateTime.UtcNow,null,null,[opcao.Id]).Itens.Single();Assert.Equal(5,Saldo(complemento));
        complemento.ControlarEstoque=true;new EfProdutoRepository(_fabrica).Salvar(complemento);
        repo.RemoverItem(item.Id,DateTime.UtcNow);Assert.Equal(5,Saldo(complemento));
    }

    [Fact, Trait("Recurso","EstoqueComplementos")]
    public async Task DoisPedidosSimultaneosNaoVendemOMesmoUltimoComplemento()
    {
        var (repo,produto,pedido,complemento,opcao)=PrepararEstoque(1);var segundo=repo.AbrirComanda(20,DateTime.UtcNow).Id;
        var resultados=await Task.WhenAll(new[] { pedido,segundo }.Select(id=>Task.Run(()=>
        {
            try { new EfComandaRepository(_fabrica).AdicionarItem(id,produto,1,DateTime.UtcNow,null,null,[opcao.Id]);return true; }
            catch(InvalidOperationException) { return false; }
        })));
        Assert.Single(resultados,r=>r);Assert.Equal(0,Saldo(complemento));
        Assert.Equal(1,repo.BuscarComItens(pedido)!.Itens.Count+repo.BuscarComItens(segundo)!.Itens.Count);
    }
}
