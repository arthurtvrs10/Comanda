using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using Xunit;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;

public class EfComandaRepositoryTests : IDisposable
{
    [Fact]
    public void OrigemClientePersisteENaoPodeSerAlteradaAposEncerramento()
    {
        var repo=new EfComandaRepository(_fabrica);var(produto,id)=PrepararComandaEProduto(repo);
        repo.DefinirOrigemCliente(id,"Instagram");repo.AdicionarItem(id,produto,1,DateTime.UtcNow);
        repo.EncerrarComanda(id,DateTime.UtcNow);
        Assert.Equal("Instagram",new EfComandaRepository(_fabrica).BuscarComItens(id)!.Comanda.OrigemCliente);
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(()=>repo.DefinirOrigemCliente(id,"Google"));
    }
    [Fact]
    public void JuntarComandasTransfereItensPreservaPrecosEObservacoesELiberaOrigem()
    {
        var repo=new EfComandaRepository(_fabrica);
        var (produto, origemId)=PrepararComandaEProduto(repo);
        var destino=repo.AbrirComanda(11,DateTime.UtcNow);
        repo.AdicionarItem(origemId,produto,2,DateTime.UtcNow,"Sem gelo");
        repo.AdicionarItem(destino.Id,produto,1,DateTime.UtcNow,"Com gelo");
        var totalAntes=repo.ListarAbertas().Sum(c=>c.TotalCentavos);
        repo.JuntarComandas(origemId,destino.Id,DateTime.UtcNow);
        var salvo=new EfComandaRepository(_fabrica).BuscarComItens(destino.Id)!;
        Assert.Equal(totalAntes,salvo.Comanda.TotalCentavos);
        Assert.Equal(1500,salvo.Comanda.TotalCentavos);
        Assert.Equal(2,salvo.Itens.Count);
        Assert.Contains(salvo.Itens,i=>i.Observacao=="Sem gelo" && i.Quantidade==2 && i.PrecoUnitarioCentavos==500);
        Assert.DoesNotContain(repo.ListarAbertas(),c=>c.Id==origemId);
        var origem=repo.BuscarComItens(origemId)!;
        Assert.Empty(origem.Itens);
        Assert.Equal(0,origem.Comanda.TotalCentavos);
        Assert.Contains("transferidos",origem.Comanda.Observacao);
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(()=>repo.JuntarComandas(origemId,destino.Id,DateTime.UtcNow));
        Assert.Equal(1500,repo.BuscarComItens(destino.Id)!.Comanda.TotalCentavos);
        Assert.Equal(10,repo.AbrirComanda(10,DateTime.UtcNow).Numero);
        using var contexto=_fabrica.CreateDbContext();
        Assert.Empty(contexto.Vendas);
    }

    [Fact]
    public void JuntarComDestinoFechadoOuIgualNaoAlteraOrigem()
    {
        var repo=new EfComandaRepository(_fabrica);
        var (produto, origemId)=PrepararComandaEProduto(repo);
        repo.AdicionarItem(origemId,produto,2,DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(()=>repo.JuntarComandas(origemId,origemId,DateTime.UtcNow));
        var destino=repo.AbrirComanda(11,DateTime.UtcNow);
        repo.CancelarComanda(destino.Id,DateTime.UtcNow);
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(()=>repo.JuntarComandas(origemId,destino.Id,DateTime.UtcNow));
        Assert.Equal(1000,repo.BuscarComItens(origemId)!.Comanda.TotalCentavos);
        Assert.Single(repo.BuscarComItens(origemId)!.Itens);
    }
    [Fact]
    public void MesaPersistePodeSerCompartilhadaELiberadaSemAlterarConsumo()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        repo.AdicionarItem(id, produto, 2, DateTime.UtcNow);
        repo.DefinirMesa(id, 7);
        var segunda = repo.AbrirComanda(11, DateTime.UtcNow);
        repo.DefinirMesa(segunda.Id, 7);
        var persistida = new EfComandaRepository(_fabrica).BuscarComItens(id)!;
        Assert.Equal(7, persistida.Comanda.NumeroMesa);
        Assert.Equal(1000, persistida.Comanda.TotalCentavos);
        Assert.Equal(2, persistida.Itens.Single().Quantidade);
        repo.DefinirMesa(id, null);
        Assert.Null(repo.BuscarComItens(id)!.Comanda.NumeroMesa);
        Assert.Equal(7, repo.BuscarComItens(segunda.Id)!.Comanda.NumeroMesa);
    }

    [Fact]
    public void MesaInvalidaOuComandaEncerradaNaoPodeSerAlterada()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        Assert.Throws<ArgumentOutOfRangeException>(() => repo.DefinirMesa(id, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => repo.DefinirMesa(id, 1000));
        repo.DefinirMesa(id, 5);
        repo.AdicionarItem(id, produto, 1, DateTime.UtcNow);
        repo.EncerrarComanda(id, DateTime.UtcNow);
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(() => repo.DefinirMesa(id, 6));
        Assert.Equal(5, repo.BuscarComItens(id)!.Comanda.NumeroMesa);
    }

    [Fact]
    public void ObservacoesDiferentesSeparamItensEPersistemAposEncerramento()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        repo.AdicionarItem(id, produto, 1, DateTime.UtcNow, "  Sem gelo  ");
        repo.AdicionarItem(id, produto, 2, DateTime.UtcNow, "Sem gelo");
        repo.AdicionarItem(id, produto, 1, DateTime.UtcNow, "Com gelo");
        repo.AdicionarItem(id, produto, 1, DateTime.UtcNow);
        var detalhe = repo.BuscarComItens(id)!;
        Assert.Equal(3, detalhe.Itens.Count);
        Assert.Equal(3, detalhe.Itens.Single(i => i.Observacao == "Sem gelo").Quantidade);
        Assert.Equal(2500, detalhe.Comanda.TotalCentavos);
        var venda = repo.EncerrarComanda(id, DateTime.UtcNow);
        Assert.Equal(2500, venda.TotalCentavos);
        var historico = new EfComandaRepository(_fabrica).BuscarComItens(id)!;
        Assert.Equal(3, historico.Itens.Count);
        Assert.Contains(historico.Itens, i => i.Observacao == "Com gelo");
    }

    private readonly string _dbPath;
    private readonly ServiceProvider _provedor;
    private readonly IDbContextFactory<VarthexComandaDbContext> _fabrica;

    public EfComandaRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"varthex-comanda-tests-{Guid.NewGuid()}.db");

        var servicos = new ServiceCollection();
        servicos.AddDbContextFactory<VarthexComandaDbContext>(options =>
            options.UseSqlite($"Data Source={_dbPath};Foreign Keys=True"));
        _provedor = servicos.BuildServiceProvider();
        _fabrica = _provedor.GetRequiredService<IDbContextFactory<VarthexComandaDbContext>>();

        using var contexto = _fabrica.CreateDbContext();
        contexto.Database.Migrate();
    }

    public void Dispose()
    {
        _provedor.Dispose();
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    [Fact]
    public void MigracaoPreservaVendaAntigaSemInventarFormaDePagamento()
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        var agora = DateTime.UtcNow;
        repo.AdicionarItem(id, produto, 1, agora);
        repo.EncerrarComanda(id, agora);
        using (var contexto = _fabrica.CreateDbContext())
        {
            var migrador = contexto.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
            migrador.Migrate("20261012140331_AddQuantidadePessoasComanda");
            contexto.Database.Migrate();
        }
        var venda = Assert.Single(new EfVendaRepository(_fabrica).ListarPorData(agora.AddMinutes(-1), agora.AddMinutes(1)));
        Assert.Null(venda.Venda.FormaPagamento);
        Assert.Equal("Não informado", venda.FormaPagamentoDescricao);
        Assert.Equal(id, venda.Venda.ComandaId);
        Assert.Equal(500, venda.Venda.TotalCentavos);
    }

    [Theory]
    [InlineData(FormaPagamento.Dinheiro)]
    [InlineData(FormaPagamento.Pix)]
    [InlineData(FormaPagamento.Debito)]
    [InlineData(FormaPagamento.Credito)]
    public void PagamentoPersisteNaVendaENoHistorico(FormaPagamento forma)
    {
        var repo = new EfComandaRepository(_fabrica);
        var (produto, id) = PrepararComandaEProduto(repo);
        var agora = DateTime.UtcNow;
        repo.AdicionarItem(id, produto, 1, agora);
        repo.EncerrarComanda(id, agora, forma);
        using var contexto = _fabrica.CreateDbContext();
        Assert.Equal(forma, contexto.Vendas.Single(v => v.ComandaId == id).FormaPagamento);
        var historico = new EfVendaRepository(_fabrica).ListarPorData(agora.AddMinutes(-1), agora.AddMinutes(1));
        Assert.Equal(forma, Assert.Single(historico).Venda.FormaPagamento);
    }

    [Fact]
    public void NomePersisteENaoPassaParaProximaComandaDoMesmoNumero()
    {
        var repo = new EfComandaRepository(_fabrica);
        var comanda = repo.AbrirComanda(1, DateTime.UtcNow);
        repo.DefinirNomeCliente(comanda.Id, "João");
        Assert.Equal("João", new EfComandaRepository(_fabrica).BuscarComItens(comanda.Id)!.Comanda.NomeCliente);
        repo.CancelarComanda(comanda.Id, DateTime.UtcNow);
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(() => repo.DefinirNomeCliente(comanda.Id, "Outro"));
        Assert.Null(repo.AbrirComanda(1, DateTime.UtcNow).NomeCliente);
        Assert.Equal("João", repo.BuscarComItens(comanda.Id)!.Comanda.NomeCliente);
    }

    [Fact]
    public void PrioridadePersisteEReaberturaComecaNormal()
    {
        var repo = new EfComandaRepository(_fabrica);
        var comanda = repo.AbrirComanda(1, DateTime.UtcNow);
        repo.DefinirPrioridade(comanda.Id, PrioridadeComanda.Urgente);
        Assert.Equal(PrioridadeComanda.Urgente, new EfComandaRepository(_fabrica).BuscarComItens(comanda.Id)!.Comanda.Prioridade);
        Assert.Throws<ArgumentOutOfRangeException>(() => repo.DefinirPrioridade(comanda.Id, (PrioridadeComanda)99));
        repo.CancelarComanda(comanda.Id, DateTime.UtcNow);
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(() => repo.DefinirPrioridade(comanda.Id, PrioridadeComanda.Alta));
        Assert.Equal(PrioridadeComanda.Normal, repo.AbrirComanda(1, DateTime.UtcNow).Prioridade);
    }

    [Fact]
    public void MoverPreservaIdentidadeTempoItensEPessoasERecusaDestinoOcupado()
    {
        var repo = new EfComandaRepository(_fabrica);
        var inicio = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc);
        var comanda = repo.AbrirComanda(1, inicio);
        repo.DefinirNomeCliente(comanda.Id, "João");
        repo.DefinirPrioridade(comanda.Id, PrioridadeComanda.Alta);
        repo.DefinirQuantidadePessoas(comanda.Id, 4);
        using (var contexto = _fabrica.CreateDbContext())
        {
            contexto.Categorias.Add(new Categoria { Id = 1, Nome = "Bebidas", Ativo = true, CriadoEm = inicio, AtualizadoEm = inicio });
            contexto.Produtos.Add(new Produto { Id = 1, Nome = "Água", CategoriaId = 1, PrecoCentavos = 500, Ativo = true, CriadoEm = inicio, AtualizadoEm = inicio });
            contexto.SaveChanges();
            repo.AdicionarItem(comanda.Id, contexto.Produtos.Single(), 2, inicio);
        }
        repo.AbrirComanda(2, inicio);
        Assert.Throws<VarthexComanda.Application.Atendimento.NumeroComandaOcupadoException>(() => repo.MoverPara(comanda.Id, 2));
        Assert.Equal(1, repo.BuscarComItens(comanda.Id)!.Comanda.Numero);
        repo.MoverPara(comanda.Id, 3);
        var detalhe = new EfComandaRepository(_fabrica).BuscarComItens(comanda.Id)!;
        Assert.Equal(3, detalhe.Comanda.Numero);
        Assert.Equal(inicio, detalhe.Comanda.AbertaEm);
        Assert.Equal("João", detalhe.Comanda.NomeCliente);
        Assert.Equal(PrioridadeComanda.Alta, detalhe.Comanda.Prioridade);
        Assert.Equal(4, detalhe.Comanda.QuantidadePessoas);
        Assert.Equal(1000, detalhe.Comanda.TotalCentavos);
        Assert.Single(detalhe.Itens);
        Assert.Equal(1, repo.AbrirComanda(1, inicio).QuantidadePessoas);
        Assert.Throws<ArgumentOutOfRangeException>(() => repo.DefinirQuantidadePessoas(comanda.Id, 0));
        repo.CancelarComanda(comanda.Id, inicio);
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(() => repo.MoverPara(comanda.Id, 4));
        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(() => repo.DefinirQuantidadePessoas(comanda.Id, 2));
    }

    [Fact]
    [Trait("Caso", "CT01")]
    public void AbrirComanda_NumeroLivre_CriaComandaAberta()
    {
        var repositorio = new EfComandaRepository(_fabrica);

        var comanda = repositorio.AbrirComanda(10, DateTime.UtcNow);

        Assert.True(comanda.Id > 0);
        Assert.Equal(StatusComanda.Aberta, comanda.Status);
        Assert.Equal(0, comanda.TotalCentavos);
    }

    [Fact]
    [Trait("Caso", "CT02")]
    public void AbrirComanda_MesmoNumeroDuasVezes_SegundaLancaExcecao()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        repositorio.AbrirComanda(10, DateTime.UtcNow);

        Assert.Throws<VarthexComanda.Application.Atendimento.NumeroComandaOcupadoException>(
            () => repositorio.AbrirComanda(10, DateTime.UtcNow));
    }

    [Fact]
    public void ListarAbertas_RetornaSomenteAbertasOrdenadasPorNumero()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        repositorio.AbrirComanda(20, DateTime.UtcNow);
        repositorio.AbrirComanda(10, DateTime.UtcNow);

        var abertas = repositorio.ListarAbertas();

        Assert.Equal(new[] { 10, 20 }, abertas.Select(c => c.Numero).ToArray());
    }

    [Fact]
    public void BuscarComItens_ComandaSemItens_RetornaListaVazia()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var comanda = repositorio.AbrirComanda(10, DateTime.UtcNow);

        var detalhe = repositorio.BuscarComItens(comanda.Id);

        Assert.NotNull(detalhe);
        Assert.Empty(detalhe!.Itens);
    }

    [Fact]
    public void BuscarComItens_ComandaInexistente_RetornaNull()
    {
        var repositorio = new EfComandaRepository(_fabrica);

        var detalhe = repositorio.BuscarComItens(999);

        Assert.Null(detalhe);
    }

    private (Produto produto, int comandaId) PrepararComandaEProduto(EfComandaRepository comandas)
    {
        var categoriaRepositorio = new EfCategoriaRepository(_fabrica);
        var agora = DateTime.UtcNow;
        var categoria = categoriaRepositorio.Salvar(new Categoria { Id = 0, Nome = "Bebidas", Ativo = true, CriadoEm = agora, AtualizadoEm = agora });
        var produtoRepositorio = new EfProdutoRepository(_fabrica);
        var produto = produtoRepositorio.Salvar(new Produto { Id = 0, CategoriaId = categoria.Id, Nome = "Refrigerante", PrecoCentavos = 500, Ativo = true, CriadoEm = agora, AtualizadoEm = agora });
        var comanda = comandas.AbrirComanda(10, agora);
        return (produto, comanda.Id);
    }

    [Fact]
    public void AdicionarItem_ProdutoNovo_CriaItemERecalculaTotal()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);

        var detalhe = repositorio.AdicionarItem(comandaId, produto, 2, DateTime.UtcNow);

        Assert.Single(detalhe.Itens);
        Assert.Equal(2, detalhe.Itens[0].Quantidade);
        Assert.Equal(1000, detalhe.Itens[0].SubtotalCentavos);
        Assert.Equal(1000, detalhe.Comanda.TotalCentavos);
    }

    [Fact]
    [Trait("Caso", "CT03")]
    public void AdicionarItem_MesmoProdutoMesmoPreco_IncrementaQuantidadeEmVezDeDuplicar()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow);

        var detalhe = repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow);

        Assert.Single(detalhe.Itens);
        Assert.Equal(2, detalhe.Itens[0].Quantidade);
        Assert.Equal(1000, detalhe.Comanda.TotalCentavos);
    }

    [Fact]
    [Trait("Caso", "CT04")]
    public void AdicionarItem_MesmoProdutoPrecoDiferente_CriaLinhaNova()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow);

        var produtoComNovoPreco = new Produto
        {
            Id = produto.Id, CategoriaId = produto.CategoriaId, Nome = produto.Nome,
            PrecoCentavos = 700, Ativo = true, CriadoEm = produto.CriadoEm, AtualizadoEm = DateTime.UtcNow
        };
        var detalhe = repositorio.AdicionarItem(comandaId, produtoComNovoPreco, 1, DateTime.UtcNow);

        Assert.Equal(2, detalhe.Itens.Count);
        Assert.Equal(500 + 700, detalhe.Comanda.TotalCentavos);
    }

    [Fact]
    [Trait("Caso", "CT05")]
    public void AlterarQuantidade_ItemExistente_RecalculaSubtotalETotal()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        var item = repositorio.AdicionarItem(comandaId, produto, 3, DateTime.UtcNow).Itens[0];

        var detalhe = repositorio.AlterarQuantidade(item.Id, 1, DateTime.UtcNow);

        Assert.Equal(1, detalhe.Itens[0].Quantidade);
        Assert.Equal(500, detalhe.Itens[0].SubtotalCentavos);
        Assert.Equal(500, detalhe.Comanda.TotalCentavos);
    }

    [Fact]
    public void RemoverItem_ItemExistente_RemoveERecalculaTotal()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        var item = repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow).Itens[0];

        var detalhe = repositorio.RemoverItem(item.Id, DateTime.UtcNow);

        Assert.Empty(detalhe.Itens);
        Assert.Equal(0, detalhe.Comanda.TotalCentavos);
    }

    [Fact]
    public void CancelarComanda_ComandaAberta_MarcaCanceladaEDefineFechadaEm()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var comanda = repositorio.AbrirComanda(20, DateTime.UtcNow);

        var cancelada = repositorio.CancelarComanda(comanda.Id, DateTime.UtcNow);

        Assert.Equal(StatusComanda.Cancelada, cancelada.Status);
        Assert.NotNull(cancelada.FechadaEm);
    }

    [Fact]
    public void AdicionarItem_ComandaCancelada_LancaExcecao()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        repositorio.CancelarComanda(comandaId, DateTime.UtcNow);

        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(
            () => repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow));
    }

    [Fact]
    public void AlterarQuantidade_ComandaCancelada_LancaExcecao()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        var item = repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow).Itens[0];
        repositorio.CancelarComanda(comandaId, DateTime.UtcNow);

        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(
            () => repositorio.AlterarQuantidade(item.Id, 2, DateTime.UtcNow));
    }

    [Fact]
    public void RemoverItem_ComandaCancelada_LancaExcecao()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        var item = repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow).Itens[0];
        repositorio.CancelarComanda(comandaId, DateTime.UtcNow);

        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(
            () => repositorio.RemoverItem(item.Id, DateTime.UtcNow));
    }

    [Fact]
    public void CancelarComanda_ComandaJaCancelada_LancaExcecao()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var comanda = repositorio.AbrirComanda(20, DateTime.UtcNow);
        repositorio.CancelarComanda(comanda.Id, DateTime.UtcNow);

        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(
            () => repositorio.CancelarComanda(comanda.Id, DateTime.UtcNow));
    }

    [Fact]
    [Trait("Caso", "CT10")]
    public void EncerrarComanda_ComandaAberta_GravaVendaEFechaComanda()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        repositorio.AdicionarItem(comandaId, produto, 2, DateTime.UtcNow);

        var venda = repositorio.EncerrarComanda(comandaId, DateTime.UtcNow);

        Assert.Equal(comandaId, venda.ComandaId);
        Assert.Equal(1000, venda.TotalCentavos);
        Assert.Equal(StatusVenda.Concluida, venda.Status);

        var detalhe = repositorio.BuscarComItens(comandaId);
        Assert.Equal(StatusComanda.Fechada, detalhe!.Comanda.Status);
        Assert.NotNull(detalhe.Comanda.FechadaEm);
    }

    [Fact]
    public void EncerrarComanda_DuasVendasSeguidas_NumeroSequencial()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produtoA, comandaIdA) = PrepararComandaEProduto(repositorio);
        repositorio.AdicionarItem(comandaIdA, produtoA, 1, DateTime.UtcNow);
        var comandaB = repositorio.AbrirComanda(30, DateTime.UtcNow);
        repositorio.AdicionarItem(comandaB.Id, produtoA, 1, DateTime.UtcNow);

        var vendaA = repositorio.EncerrarComanda(comandaIdA, DateTime.UtcNow);
        var vendaB = repositorio.EncerrarComanda(comandaB.Id, DateTime.UtcNow);

        Assert.Equal(1, vendaA.Numero);
        Assert.Equal(2, vendaB.Numero);
    }

    [Fact]
    public void EncerrarComanda_LiberaNumeroDaComanda()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow);

        repositorio.EncerrarComanda(comandaId, DateTime.UtcNow);

        Assert.Empty(repositorio.ListarAbertas());
        var reaberta = repositorio.AbrirComanda(10, DateTime.UtcNow);
        Assert.True(reaberta.Id > 0);
    }

    [Fact]
    public void EncerrarComanda_ComandaJaFechada_LancaExcecao()
    {
        var repositorio = new EfComandaRepository(_fabrica);
        var (produto, comandaId) = PrepararComandaEProduto(repositorio);
        repositorio.AdicionarItem(comandaId, produto, 1, DateTime.UtcNow);
        repositorio.EncerrarComanda(comandaId, DateTime.UtcNow);

        Assert.Throws<VarthexComanda.Application.Atendimento.ComandaNaoAbertaException>(
            () => repositorio.EncerrarComanda(comandaId, DateTime.UtcNow));
    }
}
