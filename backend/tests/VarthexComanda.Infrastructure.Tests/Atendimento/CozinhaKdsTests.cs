using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;

public class CozinhaKdsTests
{
    [Fact]
    public void PreparoPersisteSemAlterarConsumoPagamentoOuEncerramento()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var cozinha=new EfCozinhaRepository(banco.Fabrica);
        var agora=DateTime.UtcNow;var produto=banco.CriarProduto("Lanche",1250);var c=repo.AbrirComanda(1,agora);
        var item=repo.AdicionarItem(c.Id,produto,2,agora,"Sem cebola","Ana").Itens.Single();
        var caso=new AtualizarPreparo(cozinha,new RelogioFixo(agora));
        Assert.True(caso.Executar(c.Id,[new(item.Id,StatusPreparo.Pendente,2,StatusPreparo.EmPreparo)]).Sucesso);
        Assert.True(caso.Executar(c.Id,[new(item.Id,StatusPreparo.EmPreparo,2,StatusPreparo.Pronto)]).Sucesso);
        var leitura=repo.BuscarComItens(c.Id)!;Assert.Equal(StatusComanda.Aberta,leitura.Comanda.Status);Assert.Equal(2500,leitura.Comanda.TotalCentavos);
        Assert.Equal("[]",leitura.Comanda.PagamentosJson);Assert.Equal("Sem cebola",leitura.Itens.Single().Observacao);Assert.Equal(1250,leitura.Itens.Single().PrecoUnitarioCentavos);
        repo.EncerrarComanda(c.Id,agora.AddMinutes(1));Assert.Single(new EfCozinhaRepository(banco.Fabrica).ListarFila());
        Assert.True(caso.Executar(c.Id,[new(item.Id,StatusPreparo.Pronto,2,StatusPreparo.Entregue)]).Sucesso);
        Assert.Empty(cozinha.ListarFila());Assert.Equal(StatusPreparo.Entregue,repo.BuscarComItens(c.Id)!.Itens.Single().StatusPreparo);
        Assert.Equal(StatusComanda.Fechada,repo.BuscarComItens(c.Id)!.Comanda.Status);
    }
    [Fact]
    public void LoteRecusaQuantidadeOuEstadoDesatualizadoSemAlteracaoParcial()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var cozinha=new EfCozinhaRepository(banco.Fabrica);var agora=DateTime.UtcNow;
        var c=repo.AbrirComanda(2,agora);var a=repo.AdicionarItem(c.Id,banco.CriarProduto("Lanche",1000),1,agora).Itens.Single();
        var b=repo.AdicionarItem(c.Id,banco.CriarProduto("Suco",500),1,agora).Itens.Last();
        repo.AlterarQuantidade(b.Id,2,agora);
        Assert.Throws<InvalidOperationException>(()=>cozinha.AlterarPreparo(c.Id,[new(a.Id,StatusPreparo.Pendente,1,StatusPreparo.Pronto),new(b.Id,StatusPreparo.Pendente,1,StatusPreparo.Pronto)],agora));
        Assert.All(repo.BuscarComItens(c.Id)!.Itens,i=>Assert.Equal(StatusPreparo.Pendente,i.StatusPreparo));
        cozinha.AlterarPreparo(c.Id,[new(a.Id,StatusPreparo.Pendente,1,StatusPreparo.EmPreparo)],agora);
        Assert.Throws<InvalidOperationException>(()=>cozinha.AlterarPreparo(c.Id,[new(a.Id,StatusPreparo.Pendente,1,StatusPreparo.Pronto),new(b.Id,StatusPreparo.Pendente,2,StatusPreparo.Pronto)],agora));
        Assert.Equal(StatusPreparo.Pendente,repo.BuscarComItens(c.Id)!.Itens.Last().StatusPreparo);
    }
    [Fact]
    public void ConsumoAdicionalDepoisDeIniciarPreparoCriaNovoItemPendente()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var cozinha=new EfCozinhaRepository(banco.Fabrica);var agora=DateTime.UtcNow;
        var p=banco.CriarProduto("Água",500);var c=repo.AbrirComanda(3,agora);var primeiro=repo.AdicionarItem(c.Id,p,1,agora).Itens.Single();
        cozinha.AlterarPreparo(c.Id,[new(primeiro.Id,StatusPreparo.Pendente,1,StatusPreparo.Pronto)],agora);
        var estado=repo.AdicionarItem(c.Id,p,2,agora.AddMinutes(5));
        Assert.Equal(2,estado.Itens.Count);Assert.Equal(1,estado.Itens.First().Quantidade);Assert.Equal(StatusPreparo.Pronto,estado.Itens.First().StatusPreparo);
        Assert.Equal(2,estado.Itens.Last().Quantidade);Assert.Equal(StatusPreparo.Pendente,estado.Itens.Last().StatusPreparo);Assert.Equal(1500,estado.Comanda.TotalCentavos);
        Assert.Throws<InvalidOperationException>(()=>repo.AlterarQuantidade(primeiro.Id,4,agora));
        Assert.Equal(1500,repo.AlterarQuantidade(primeiro.Id,1,agora).Comanda.TotalCentavos);
    }
    [Fact]
    public void AcaoDoCartaoNaoIncluiLançamentoQueChegouDepoisDaLeitura()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var cozinha=new EfCozinhaRepository(banco.Fabrica);var agora=DateTime.UtcNow;
        var c=repo.AbrirComanda(4,agora);var a=repo.AdicionarItem(c.Id,banco.CriarProduto("Sopa",1800),1,agora).Itens.Single();
        var snapshot=cozinha.ListarFila().Single();repo.AdicionarItem(c.Id,banco.CriarProduto("Pão",400),1,agora);
        cozinha.AlterarPreparo(c.Id,snapshot.Itens.Select(i=>new AlteracaoPreparo(i.Id,i.StatusPreparo,i.Quantidade,StatusPreparo.Pronto)).ToArray(),agora);
        var itens=repo.BuscarComItens(c.Id)!.Itens;Assert.Equal(StatusPreparo.Pronto,itens.First().StatusPreparo);Assert.Equal(StatusPreparo.Pendente,itens.Last().StatusPreparo);
    }
    [Fact]
    public void RecusaEntregaPrematuraRegressoCancelamentoEItemMovido()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var cozinha=new EfCozinhaRepository(banco.Fabrica);var agora=DateTime.UtcNow;
        var a=repo.AbrirComanda(5,agora);var b=repo.AbrirComanda(6,agora);var i=repo.AdicionarItem(a.Id,banco.CriarProduto("Lanche",1000),1,agora).Itens.Single();
        var caso=new AtualizarPreparo(cozinha,new RelogioFixo());
        Assert.False(caso.Executar(a.Id,[new(i.Id,StatusPreparo.Pendente,1,StatusPreparo.Entregue)]).Sucesso);
        Assert.False(caso.Executar(a.Id,[new(i.Id,StatusPreparo.Pronto,1,StatusPreparo.Pendente)]).Sucesso);
        repo.TransferirItens(a.Id,b.Id,[i.Id],false,agora);
        Assert.False(caso.Executar(a.Id,[new(i.Id,StatusPreparo.Pendente,1,StatusPreparo.Pronto)]).Sucesso);
        repo.CancelarComanda(b.Id,agora);Assert.Empty(cozinha.ListarFila());
        Assert.False(caso.Executar(b.Id,[new(i.Id,StatusPreparo.Pendente,1,StatusPreparo.Pronto)]).Sucesso);
    }
    [Fact]
    public void MoverPreservaPreparoCopiarCriaNovaDemandaEEntregaNaoFechaComanda()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var cozinha=new EfCozinhaRepository(banco.Fabrica);var agora=DateTime.UtcNow;
        var a=repo.AbrirComanda(7,agora);var b=repo.AbrirComanda(8,agora);var i=repo.AdicionarItem(a.Id,banco.CriarProduto("Lanche",1000),1,agora).Itens.Single();
        cozinha.AlterarPreparo(a.Id,[new(i.Id,StatusPreparo.Pendente,1,StatusPreparo.Pronto)],agora);
        repo.TransferirItens(a.Id,b.Id,[i.Id],true,agora);Assert.Equal(StatusPreparo.Pendente,repo.BuscarComItens(b.Id)!.Itens.Single().StatusPreparo);
        repo.TransferirItens(a.Id,b.Id,[i.Id],false,agora);Assert.Equal(StatusPreparo.Pronto,repo.BuscarComItens(b.Id)!.Itens.Single(x=>x.Id==i.Id).StatusPreparo);
        cozinha.AlterarPreparo(b.Id,[new(i.Id,StatusPreparo.Pronto,1,StatusPreparo.Entregue)],agora);
        Assert.Equal(StatusComanda.Aberta,repo.BuscarComItens(b.Id)!.Comanda.Status);Assert.Equal(2000,repo.BuscarComItens(b.Id)!.Comanda.TotalCentavos);
    }
    [Fact]
    public void MigracaoPreservaValoresENaoReenviaVendasAntigas()
    {
        using var banco=new BancoDeHomologacao(migrar:false);
        using(var db=banco.Fabrica.CreateDbContext())
        {
            db.GetService<IMigrator>().Migrate("20261023005913_AddRecebimentosComandaVenda");
            db.Database.ExecuteSqlRaw("""
                INSERT INTO categoria(id,nome,ativo,criado_em,atualizado_em) VALUES(1,'Geral',1,'2026-10-23','2026-10-23');
                INSERT INTO produto(id,categoria_id,nome,preco_centavos,ativo,criado_em,atualizado_em) VALUES(1,1,'Legado',750,1,'2026-10-23','2026-10-23');
                INSERT INTO comanda(id,numero,status,aberta_em,total_centavos) VALUES(1,1,'ABERTA','2026-10-23',750);
                INSERT INTO comanda(id,numero,status,aberta_em,fechada_em,total_centavos) VALUES(2,2,'FECHADA','2026-10-23','2026-10-23',750);
                INSERT INTO item_comanda(id,comanda_id,produto_id,nome_produto,preco_unitario_centavos,quantidade,subtotal_centavos,criado_em,atualizado_em) VALUES(1,1,1,'Legado',750,1000,750,'2026-10-23','2026-10-23');
                INSERT INTO item_comanda(id,comanda_id,produto_id,nome_produto,preco_unitario_centavos,quantidade,subtotal_centavos,criado_em,atualizado_em) VALUES(2,2,1,'Legado',750,1000,750,'2026-10-23','2026-10-23');
                """);
            Assert.True(VarthexComanda.Infrastructure.Persistence.MigracaoDoBanco.ExigeBackupPreventivo(db.Database,out _));
            db.Database.Migrate();
        }
        var repo=new EfComandaRepository(banco.Fabrica);Assert.Equal(StatusPreparo.Pendente,repo.BuscarComItens(1)!.Itens.Single().StatusPreparo);
        Assert.Equal(StatusPreparo.Entregue,repo.BuscarComItens(2)!.Itens.Single().StatusPreparo);Assert.Equal(750,repo.BuscarComItens(2)!.Comanda.TotalCentavos);
        Assert.Equal(1,new EfCozinhaRepository(banco.Fabrica).ListarFila().Single().Comanda.Id);
    }
}
