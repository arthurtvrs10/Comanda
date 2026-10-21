using Microsoft.EntityFrameworkCore;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using Xunit;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;

public partial class EfComandaRepositoryTests
{
    [Fact]
    public void BloqueioPersisteImpedeAlterarItensEPermitePagar()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        var item = repo.AdicionarItem(id, produto, 2, DateTime.UtcNow, null, "Ana").Itens.Single();
        repo.DefinirAtendimento(id, "Ana", "Bruno", "Entregar juntos");
        repo.DefinirBloqueio(id, true);
        repo = new(_fabrica);
        var salvo = repo.BuscarComItens(id)!.Comanda;
        Assert.True(salvo.Bloqueada);
        Assert.Equal("Ana", salvo.CriadoPor);
        Assert.Equal("Bruno", salvo.Responsavel);
        Assert.Equal("Entregar juntos", salvo.Observacao);
        Assert.Throws<InvalidOperationException>(() => repo.AdicionarItem(id, produto, 1, DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(() => repo.AlterarQuantidade(item.Id, 3, DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(() => repo.RemoverItem(item.Id, DateTime.UtcNow));
        var destino = repo.AbrirComanda(11, DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(() => repo.TransferirItens(id, destino.Id, [item.Id], false, DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(() => repo.JuntarComandas(id, destino.Id, DateTime.UtcNow));
        Assert.Equal(1000, repo.EncerrarComanda(id, DateTime.UtcNow).TotalCentavos);
    }

    [Fact]
    public void TransferirECopiarSelecionadosPreservaPrecosETotaisSemDuplicarImpressao()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        var a = repo.AdicionarItem(id, produto, 2, DateTime.UtcNow, "Sem gelo", "Ana").Itens.Single();
        repo.AdicionarItem(id, produto, 1, DateTime.UtcNow, "Com gelo", "Ana");
        repo.RegistrarImpressao(id, new Dictionary<int,int> { [a.Id] = 2 }, true);
        var destino = repo.AbrirComanda(11, DateTime.UtcNow);
        repo.TransferirItens(id, destino.Id, [a.Id], true, DateTime.UtcNow);
        var copia = repo.BuscarComItens(destino.Id)!.Itens.Single();
        Assert.Equal(2, copia.Quantidade);
        Assert.Equal(500, copia.PrecoUnitarioCentavos);
        Assert.Equal("Sem gelo", copia.Observacao);
        Assert.Equal("Ana", copia.LancadoPor);
        Assert.Equal(0, copia.QuantidadeCozinha);
        Assert.Equal(1500, repo.BuscarComItens(id)!.Comanda.TotalCentavos);
        repo.TransferirItens(id, destino.Id, [a.Id], false, DateTime.UtcNow);
        Assert.Equal(500, repo.BuscarComItens(id)!.Comanda.TotalCentavos);
        Assert.Equal(2000, repo.BuscarComItens(destino.Id)!.Comanda.TotalCentavos);
        Assert.Equal(2, repo.BuscarComItens(destino.Id)!.Itens.Single(i => i.Id == a.Id).QuantidadeCozinha);
    }

    [Fact]
    public void TransferenciaInvalidaNaoAlteraOrigemNemDestino()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        var item = repo.AdicionarItem(id, produto, 1, DateTime.UtcNow).Itens.Single();
        var destino = repo.AbrirComanda(11, DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(() => repo.TransferirItens(id, destino.Id, [item.Id, int.MaxValue], false, DateTime.UtcNow));
        Assert.Single(repo.BuscarComItens(id)!.Itens);
        Assert.Empty(repo.BuscarComItens(destino.Id)!.Itens);
        repo.DefinirBloqueio(destino.Id, true);
        Assert.Throws<InvalidOperationException>(() => repo.TransferirItens(id, destino.Id, [item.Id], true, DateTime.UtcNow));
        Assert.Equal(500, repo.BuscarComItens(id)!.Comanda.TotalCentavos);
    }

    [Fact]
    public void ImpressoesSeparadasPersistemEQuantidadeAcrescidaFicaPendente()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        var item = repo.AdicionarItem(id, produto, 2, DateTime.UtcNow, null, "Ana").Itens.Single();
        repo.RegistrarImpressao(id, new Dictionary<int,int> { [item.Id] = 2 }, true);
        repo.RegistrarImpressao(id, new Dictionary<int,int> { [item.Id] = 1 }, false);
        repo.AdicionarItem(id, produto, 1, DateTime.UtcNow, null, "Ana");
        var salvo = new EfComandaRepository(_fabrica).BuscarComItens(id)!.Itens.Single();
        Assert.Equal(3, salvo.Quantidade);
        Assert.Equal(2, salvo.QuantidadeCozinha);
        Assert.Equal(1, salvo.QuantidadeFicha);
        repo.AlterarQuantidade(item.Id, 1, DateTime.UtcNow);
        salvo = repo.BuscarComItens(id)!.Itens.Single();
        Assert.Equal(1, salvo.QuantidadeCozinha);
        Assert.Equal(1, salvo.QuantidadeFicha);
        repo.AdicionarItem(id, produto, 1, DateTime.UtcNow, null, "Bruno");
        Assert.Equal(2, repo.BuscarComItens(id)!.Itens.Count);
    }

    [Fact]
    public void RecalculoUsaPrecoLancadoEAtualizaTotalPersistido()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        repo.AdicionarItem(id, produto, 2, DateTime.UtcNow);
        using (var contexto = _fabrica.CreateDbContext())
        {
            contexto.Comandas.Single(c => c.Id == id).TotalCentavos = 123;
            contexto.Produtos.Single(p => p.Id == produto.Id).PrecoCentavos = 999;
            contexto.SaveChanges();
        }
        Assert.Equal(1000, repo.Recalcular(id).Comanda.TotalCentavos);
        Assert.Equal(500, repo.BuscarComItens(id)!.Itens.Single().PrecoUnitarioCentavos);
    }
}
