using System.Globalization;
using VarthexComanda.Domain;

namespace VarthexComanda.Application.Atendimento;

public enum StatusFiltroPedido { Todos, Abertos, Finalizados, FinalizadosEPagos }
public enum DataFiltroPedido { Abertura, Fechamento }

public sealed record FiltroHistoricoPedidos
{
    public required DateTime De { get; init; }
    public required DateTime Ate { get; init; }
    public StatusFiltroPedido Status { get; init; }
    public DataFiltroPedido Data { get; init; }
    public string Cliente { get; init; } = "";
    public string Colaborador { get; init; } = "";
    public string Codigos { get; init; } = "";
    public bool IncluirCancelados { get; init; }
    public string StatusDescricao => Status switch
    {
        StatusFiltroPedido.Abertos => "Abertos", StatusFiltroPedido.Finalizados => "Finalizados",
        StatusFiltroPedido.FinalizadosEPagos => "Finalizados e pagos", _ => "Todos os pedidos"
    };
    public string DataDescricao => Data == DataFiltroPedido.Abertura ? "Data de abertura" : "Data de fechamento";
    public string Resumo => $"{StatusDescricao} > {DataDescricao} [{De:dd/MM/yyyy HH:mm} – {Ate:dd/MM/yyyy HH:mm}]";

    public long[] LerCodigos()
    {
        var partes = Codigos.Split([',', ';', ' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length > 1000) throw new ArgumentException("Pesquise até 1.000 códigos por vez.");
        return partes.Select(parte => long.TryParse(parte.TrimStart('#'), NumberStyles.None, CultureInfo.InvariantCulture, out var codigo) && codigo > 0
            ? codigo : throw new ArgumentException("Informe códigos positivos separados por vírgula. Exemplo: 111, 222.")).Distinct().ToArray();
    }
    public void Validar()
    {
        if (Ate < De || Ate.Date == DateTime.MaxValue.Date) throw new ArgumentException("Confira o período: a data final deve ser igual ou posterior à inicial.");
        if (!Enum.IsDefined(Status) || !Enum.IsDefined(Data)) throw new ArgumentException("Escolha um status e uma data para o filtro.");
        if (Cliente.Length > 80 || Colaborador.Length > 120) throw new ArgumentException("Reduza o nome do cliente ou colaborador para pesquisar.");
        LerCodigos();
    }
}

public sealed class PedidoHistoricoResumo
{
    public required Comanda Comanda { get; init; }
    public Venda? Venda { get; init; }
    public int TotalItens { get; init; }
    public long Codigo => Comanda.CodigoPedido ?? Comanda.Id;
    public string CodigoFormatado => $"#{Codigo}";
    public DateTime DataAbertura => FusoBrasilia.ParaLocal(Comanda.AbertaEm);
    public DateTime? DataFechamento => Comanda.FechadaEm is DateTime data ? FusoBrasilia.ParaLocal(data) : null;
    public string Status => Comanda.Status switch { StatusComanda.Cancelada => "Cancelado", StatusComanda.Fechada => "Finalizado", _ => Comanda.Bloqueada ? "Em fechamento" : "Em aberto" };
    public string CriadoPor => Comanda.CriadoPor ?? "Não informado";
    public string Cliente => Comanda.NomeCliente ?? "";
    public string Responsavel => Comanda.Responsavel ?? "";
    public string Tipo => Comanda.NumeroMesa.HasValue ? "Mesa" : "Balcão";
    public string Origem => Comanda.OrigemCliente ?? "Não informada";
    public int NumeroComanda => Comanda.Numero;
    public int? Mesa => Comanda.NumeroMesa;
    public int Pessoas => Comanda.QuantidadePessoas;
    public string Observacao => Comanda.Observacao ?? "";
    public long TotalCentavos => Comanda.TotalCentavos;
    public decimal Total => TotalCentavos / 100m;
    public string Pagamento => Venda is null?"Não informado":PagamentosConta.Descrever(Venda.PagamentosJson,Venda.FormaPagamento);
}

public interface IHistoricoPedidosRepository
{
    IReadOnlyList<PedidoHistoricoResumo> Pesquisar(FiltroHistoricoPedidos filtro);
    ComandaComItens? BuscarDetalhes(int comandaId);
}

public sealed class ConsultarHistoricoPedidos(IHistoricoPedidosRepository repositorio)
{
    public IReadOnlyList<PedidoHistoricoResumo> Executar(FiltroHistoricoPedidos filtro)
    {
        filtro.Validar();
        return repositorio.Pesquisar(filtro);
    }
    public ComandaComItens? BuscarDetalhes(int comandaId) => repositorio.BuscarDetalhes(comandaId);
}
