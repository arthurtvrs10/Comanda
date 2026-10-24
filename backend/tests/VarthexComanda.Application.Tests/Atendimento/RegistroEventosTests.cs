using VarthexComanda.Application.Abstractions;
using VarthexComanda.Application.Catalogo;

namespace VarthexComanda.Application.Tests.Atendimento;

public class RegistroEventosTests
{
    [Fact]
    public void AberturaDoDesktopRegistraSucessoERecusaSemDadosDoPedido()
    {
        var registro = new RegistroEventos(); var repo = new FakeComandaRepository();
        var abrir = new VarthexComanda.Application.Atendimento.AbrirComanda(repo,
            new VarthexComanda.Application.Tests.Catalogo.FakeClock(),
            new VarthexComanda.Application.Configuracao.ObterConfiguracao(new VarthexComanda.Application.Tests.Configuracao.FakeConfiguracaoRepository()), registro);
        Assert.True(abrir.Executar(1).Sucesso); Assert.False(abrir.Executar(1).Sucesso);
        Assert.Equal(new[] { EstadoEvento.Recusado, EstadoEvento.Concluido }, registro.Consultar().Select(e => e.Estado));
        Assert.All(registro.Consultar(), e => { Assert.Equal(OrigemEvento.Atendimento, e.Origem); Assert.Equal(AcaoEvento.AbrirComanda, e.Acao); });
        Assert.Single(repo.ListarAbertas());
    }
    [Fact]
    public void EstadosReaisSaoObservadosEGravacaoNaoDependeDoDiagnostico()
    {
        var salvos = new List<EventoSistema>(); var registro = new RegistroEventos(salvos.Add);
        using (var op = registro.Iniciar(OrigemEvento.Servidor, AcaoEvento.ReceberItem, true))
        {
            Assert.Equal(EstadoEvento.Aguardando, registro.Consultar().Single().Estado);
            op.Processar(); Assert.Equal(EstadoEvento.Processando, registro.Consultar().Single().Estado);
            op.Finalizar(EstadoEvento.Concluido);
        }
        Assert.Single(salvos); Assert.Equal(EstadoEvento.Concluido, salvos[0].Estado);
        Assert.True(new RegistroEventos(_ => throw new IOException()).Executar(OrigemEvento.Atendimento, AcaoEvento.AdicionarItem, () => Resultado<object>.Ok(new())).Sucesso);
        Assert.False(registro.Executar(OrigemEvento.Atendimento, AcaoEvento.AdicionarItem, () => Resultado<object>.Falha("Conteúdo privado")).Sucesso);
        Assert.Equal(EstadoEvento.Recusado, registro.Consultar()[0].Estado);
        Assert.DoesNotContain("Conteúdo privado", string.Join("", registro.Consultar()));
        Assert.Throws<IOException>(() => registro.Executar<object>(OrigemEvento.Atendimento, AcaoEvento.AbrirComanda, () => throw new IOException("segredo")));
        Assert.Equal(EstadoEvento.Falha, registro.Consultar()[0].Estado);
    }
    [Fact]
    public void RetencaoLimitadaPreservaPendentesSobConcorrencia()
    {
        var registro = new RegistroEventos();
        using var pendente = registro.Iniciar(OrigemEvento.Servidor, AcaoEvento.ReceberItem, true);
        Parallel.For(0, 1500, _ => { using var op = registro.Iniciar(OrigemEvento.Atendimento, AcaoEvento.AdicionarItem); op.Finalizar(EstadoEvento.Concluido); });
        Assert.Equal(RegistroEventos.Capacidade, registro.Consultar().Count);
        Assert.Single(registro.Consultar(), e => e.Estado == EstadoEvento.Aguardando);
        Assert.Equal(RegistroEventos.Capacidade, registro.Consultar().Select(e => e.Id).Distinct().Count());
    }
}
