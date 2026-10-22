using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Infrastructure.Tests.Atendimento;

public sealed class HistoricoPedidosTests : IDisposable
{
    private readonly BancoDeHomologacao _banco = new();
    private EfComandaRepository Comandas => new(_banco.Fabrica);
    private ConsultarHistoricoPedidos Consulta => new(new EfHistoricoPedidosRepository(_banco.Fabrica));
    private static readonly DateTime Dia = new(2026, 10, 22);
    private static FiltroHistoricoPedidos Filtro => new() { De = Dia, Ate = Dia.AddDays(1).AddMinutes(-1) };

    [Fact]
    public void ConsultaIncluiTodoOMinutoFinalEFusoDeBrasiliaSemTrazerODiaSeguinte()
    {
        var foraAntes = Comandas.AbrirComanda(1, FusoBrasilia.ParaUtc(Dia).AddTicks(-1));
        var primeira = Comandas.AbrirComanda(2, FusoBrasilia.ParaUtc(Dia));
        var ultima = Comandas.AbrirComanda(3, FusoBrasilia.ParaUtc(Dia.AddDays(1)).AddSeconds(-1));
        Comandas.AbrirComanda(4, FusoBrasilia.ParaUtc(Dia.AddDays(1)));
        var resultado = Consulta.Executar(Filtro);
        Assert.Equal(new[] { ultima.Id, primeira.Id }, resultado.Select(p => p.Comanda.Id));
        Assert.Equal(Dia, resultado.Last().DataAbertura);
        Assert.DoesNotContain(resultado, p => p.Comanda.Id == foraAntes.Id);
    }

    [Fact]
    public void StatusDatasECanceladosUsamRegistrosReaisESemCriarVendas()
    {
        var produto = _banco.CriarProduto("Suco", 725);
        var aberta = Comandas.AbrirComanda(10, FusoBrasilia.ParaUtc(Dia.AddHours(10)));
        Comandas.DefinirBloqueio(aberta.Id, true);
        var cancelada = Comandas.AbrirComanda(11, FusoBrasilia.ParaUtc(Dia.AddHours(11)));
        Comandas.CancelarComanda(cancelada.Id, FusoBrasilia.ParaUtc(Dia.AddHours(12)));
        var finalizada = Comandas.AbrirComanda(12, FusoBrasilia.ParaUtc(Dia.AddDays(-1).AddHours(18)));
        Comandas.AdicionarItem(finalizada.Id, produto, 2, FusoBrasilia.ParaUtc(Dia.AddDays(-1).AddHours(18)));
        Comandas.EncerrarComanda(finalizada.Id, FusoBrasilia.ParaUtc(Dia.AddHours(13)), FormaPagamento.Pix);
        Assert.Single(Consulta.Executar(Filtro));
        Assert.Equal("Em fechamento", Assert.Single(Consulta.Executar(Filtro with { Status = StatusFiltroPedido.Abertos })).Status);
        Assert.Equal(2, Consulta.Executar(Filtro with { IncluirCancelados = true }).Count);
        var pago = Assert.Single(Consulta.Executar(Filtro with { Status = StatusFiltroPedido.FinalizadosEPagos, Data = DataFiltroPedido.Fechamento }));
        Assert.Equal(finalizada.Id, pago.Comanda.Id); Assert.Equal("Pix", pago.Pagamento);
        Assert.Equal(1450, pago.TotalCentavos); Assert.Equal(1, pago.TotalItens);
        Assert.Equal(2, Consulta.BuscarDetalhes(finalizada.Id)!.Itens.Single().Quantidade);
        using var db = _banco.Fabrica.CreateDbContext();
        Assert.Single(db.Vendas); Assert.Equal(3, db.Comandas.Count());
    }

    [Fact]
    public void NomesAcentuadosCodigosMultiplosEOrigemRespeitamSnapshot()
    {
        var primeira = Comandas.AbrirComanda(1, FusoBrasilia.ParaUtc(Dia.AddHours(8)));
        var segunda = Comandas.AbrirComanda(2, FusoBrasilia.ParaUtc(Dia.AddHours(9)));
        using (var db = _banco.Fabrica.CreateDbContext())
        {
            var registro = db.Comandas.Single(c => c.Id == primeira.Id);
            registro.CodigoPedido = 111; registro.NomeCliente = "Érica"; registro.CriadoPor = "Áurea"; registro.Responsavel = "João"; registro.NumeroMesa = 12;
            db.SaveChanges();
        }
        var resultado = Assert.Single(Consulta.Executar(Filtro with { Cliente = "érica", Colaborador = "áurea", Codigos = "#111, 222" }));
        Assert.Equal(primeira.Id, resultado.Comanda.Id); Assert.Equal(111, resultado.Codigo); Assert.Equal("Mesa", resultado.Tipo);
        Assert.Empty(Consulta.Executar(Filtro with { Codigos = primeira.Id.ToString() }));
        Assert.Single(Consulta.Executar(Filtro with { Colaborador = "joão" }));
        Assert.Equal(2, Consulta.Executar(Filtro with { Codigos = $"111, {segunda.Id}" }).Count);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("111, texto")]
    [InlineData("111 OR 1=1")]
    public void RejeitaCodigosInvalidosAntesDaConsulta(string codigos)
    { Assert.Throws<ArgumentException>(() => Consulta.Executar(Filtro with { Codigos = codigos })); }

    [Fact]
    public void PeriodoInvertidoNaoConsultaBase()
    { Assert.Throws<ArgumentException>(() => Consulta.Executar(Filtro with { De = Dia.AddDays(2) })); }
    public void Dispose() => _banco.Dispose();
}
