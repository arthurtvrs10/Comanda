using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;
using Xunit;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;
public class TipoPedidoTests
{
    [Theory]
    [InlineData(TipoPedido.LocalMesa)] [InlineData(TipoPedido.Salao)] [InlineData(TipoPedido.Balcao)] [InlineData(TipoPedido.ParaLevar)] [InlineData(TipoPedido.Delivery)]
    public void DadosPersistemJuntosEValidacaoNaoAlteraRegistro(TipoPedido tipo)
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var c=repo.AbrirComanda(1,DateTime.UtcNow);
        repo.DefinirDadosPedido(c.Id,"Bruno"," Ana ",tipo,"Alergia a leite.\nCarne bem passada.");
        var salvo=new EfComandaRepository(banco.Fabrica).BuscarComItens(c.Id)!.Comanda;
        Assert.Equal(tipo,salvo.TipoPedido);Assert.Equal("Ana",salvo.NomeCliente);Assert.Contains("Alergia a leite",salvo.Observacao);Assert.Equal("Bruno",salvo.Responsavel);
        Assert.Throws<ArgumentException>(()=>repo.DefinirDadosPedido(c.Id,"Outro","Cliente",(TipoPedido)99,"Apagar"));
        Assert.Throws<ArgumentException>(()=>repo.DefinirDadosPedido(c.Id,"Outro",new string('x',81),tipo,"Apagar"));
        salvo=repo.BuscarComItens(c.Id)!.Comanda;Assert.Equal("Ana",salvo.NomeCliente);Assert.Equal("Bruno",salvo.Responsavel);
        repo.DefinirBloqueio(c.Id,true);Assert.Throws<InvalidOperationException>(()=>repo.DefinirDadosPedido(c.Id,null,null,tipo,null));
    }
    [Fact] public void ModeloDaReleaseNaoTemMigracoesPendentesNemComplementos()
    {
        using var banco=new BancoDeHomologacao();using var db=banco.Fabrica.CreateDbContext();
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.DoesNotContain(db.Database.GetMigrations(),m=>m.Contains("Complementos"));
    }
    [Fact] public void MesaAtualizaTipoPadraoSemTrocarTiposExplicitos()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var c=repo.AbrirComanda(1,DateTime.UtcNow);
        Assert.Equal(TipoPedido.LocalMesa,repo.DefinirMesa(c.Id,5).TipoPedido);
        Assert.Equal(TipoPedido.Balcao,repo.DefinirMesa(c.Id,null).TipoPedido);
        repo.DefinirDadosPedido(c.Id,"Operador","Ana",TipoPedido.Delivery,"Alergia");
        Assert.Equal(TipoPedido.Delivery,repo.DefinirMesa(c.Id,6).TipoPedido);
    }
    [Fact] public void CriacaoExplicitaLocalNaoEConfundidaComValorPadraoDoBanco()
    {
        using var banco=new BancoDeHomologacao();using var db=banco.Fabrica.CreateDbContext();
        db.Comandas.Add(new(){Id=0,Numero=1,Status=StatusComanda.Aberta,AbertaEm=DateTime.UtcNow,TotalCentavos=0,TipoPedido=TipoPedido.LocalMesa});db.SaveChanges();db.ChangeTracker.Clear();
        Assert.Equal(TipoPedido.LocalMesa,db.Comandas.Single().TipoPedido);
    }
    [Fact] public void MigracaoPreservaComandasAntigasEInfereMesaOuBalcao()
    {
        using var banco=new BancoDeHomologacao(false);using var db=banco.Fabrica.CreateDbContext();
        var anterior=db.Database.GetMigrations().Last(m=>string.CompareOrdinal(m,"20261029190000_AddTipoPedido")<0);
        db.GetService<IMigrator>().Migrate(anterior);
        db.Database.ExecuteSqlRaw("INSERT INTO comanda (numero,status,aberta_em,total_centavos,numero_mesa) VALUES (1,'ABERTA','2026-10-29 12:00:00',1234,5),(2,'ABERTA','2026-10-29 12:00:00',5678,NULL);");
        db.Database.Migrate();var repo=new EfComandaRepository(banco.Fabrica);var comandas=repo.ListarAbertas();
        Assert.Equal(TipoPedido.LocalMesa,comandas.Single(c=>c.Numero==1).TipoPedido);Assert.Equal(TipoPedido.Balcao,comandas.Single(c=>c.Numero==2).TipoPedido);Assert.Equal(1234,comandas.Single(c=>c.Numero==1).TotalCentavos);Assert.Equal(5678,comandas.Single(c=>c.Numero==2).TotalCentavos);
    }
}

