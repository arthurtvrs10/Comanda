using VarthexComanda.Domain;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VarthexComanda.Desktop.Atendimento;

public partial class AtendimentoViewModel
{
    public Func<string?>? ObterOperador { get; set; }
    public Func<IReadOnlyList<string>>? ObterResponsaveis { get; set; }
    public Func<bool>? PrepararSaidaPedido { get; set; }
    [ObservableProperty] private bool exibicaoCompleta = true;
    public bool PodeAlterarPedido => ComandaAtual is { Bloqueada: false };
    public string SituacaoPedido => ComandaAtual?.Bloqueada == true ? "Em fechamento" : "Em aberto";
    public string IdentificacaoPedido => $"Pedido #{ComandaAtual?.CodigoPedido ?? ComandaAtual?.Id}";
    public string NumeroPedidoDestacado => $"#{ComandaAtual?.CodigoPedido ?? ComandaAtual?.Id}";
    public string DescricaoPrioridadePedido => "Prioridade: " + NomePrioridade(ComandaAtual?.Prioridade ?? PrioridadeComanda.Normal) + ". Clique para alterar.";
    public static string NomePrioridade(PrioridadeComanda prioridade) => prioridade switch { PrioridadeComanda.Urgente => "Alta", PrioridadeComanda.Alta => "Mediana", _ => "Normal" };
    public string AberturaPedido => ComandaAtual is null ? "" : FusoBrasilia.ParaLocal(ComandaAtual.AbertaEm).ToString("dd/MM HH:mm");
    public string AutorPedido => ComandaAtual?.CriadoPor ?? "Não informado";
    public string ClientePedido => string.IsNullOrWhiteSpace(ComandaAtual?.NomeCliente) ? "Vincular cliente" : ComandaAtual.NomeCliente;
    public string TotalPedido => CentavosParaMoedaConverter.Formatar(ComandaAtual?.TotalCentavos ?? 0);
    public string CabecalhoItens => $"Itens do pedido ({Itens.Count})";

    private void AtualizarResumoPedido()
    {
        foreach (var nome in new[] { nameof(PodeAlterarPedido), nameof(SituacaoPedido), nameof(IdentificacaoPedido), nameof(NumeroPedidoDestacado), nameof(DescricaoPrioridadePedido), nameof(AberturaPedido), nameof(AutorPedido), nameof(ClientePedido), nameof(TotalPedido), nameof(CabecalhoItens) })
            OnPropertyChanged(nome);
    }
    public void NotificarItensPedido() => AtualizarResumoPedido();

    private bool PermitirAlteracao()
    {
        if (ComandaAtual is not { Bloqueada: true }) return true;
        Mensagem = "Comanda em fechamento. Desative o fechamento para alterar os itens.";
        return false;
    }

    public bool ExecutarOperacaoPedido(Func<ComandaComItens> operacao, string mensagem)
    {
        try { AplicarResultado(Resultado<ComandaComItens>.Ok(operacao())); Mensagem = mensagem; return true; }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha na operação da comanda");
            Mensagem = ex is InvalidOperationException or ComandaNaoAbertaException or ArgumentException ? ex.Message : "Não foi possível concluir. Atualize o pedido e tente novamente.";
            return false;
        }
    }

    public bool DefinirBloqueio(bool bloqueada) => ComandaAtual is not null && ExecutarOperacaoPedido(() =>
    {
        var comanda = _comandas.DefinirBloqueio(ComandaAtual.Id, bloqueada);
        return new() { Comanda = comanda, Itens = _comandas.BuscarComItens(comanda.Id)!.Itens };
    }, bloqueada ? "Comanda em fechamento. O pagamento continua disponível." : "Comanda reaberta para alterações.");

    public bool SalvarAtendimento(string? responsavel, string? observacao) => ComandaAtual is not null && ExecutarOperacaoPedido(() =>
    {
        var comanda = _comandas.DefinirAtendimento(ComandaAtual.Id, null, responsavel, observacao);
        return new() { Comanda = comanda, Itens = _comandas.BuscarComItens(comanda.Id)!.Itens };
    }, "Dados do pedido salvos.");

    public void RegistrarAberturaPedido()
    {
        if (ComandaAtual is not { Bloqueada: false, CriadoPor: null } atual || ObterOperador?.Invoke() is not string operador) return;
        ExecutarOperacaoPedido(() =>
        {
            var comanda = _comandas.DefinirAtendimento(atual.Id, operador, atual.Responsavel ?? operador, atual.Observacao);
            return new() { Comanda = comanda, Itens = _comandas.BuscarComItens(comanda.Id)!.Itens };
        }, "");
    }

    public bool RecalcularPedido() => ComandaAtual is not null && ExecutarOperacaoPedido(() => _comandas.Recalcular(ComandaAtual.Id), "Pedido recalculado. Os preços lançados foram preservados.");
    public bool TransferirItens(int destinoId, IReadOnlyCollection<int> ids, bool copiar) => ComandaAtual is not null && ExecutarOperacaoPedido(
        () => _comandas.TransferirItens(ComandaAtual.Id, destinoId, ids, copiar, _relogio.UtcNow), copiar ? "Itens copiados para o destino." : "Itens transferidos para o destino.");
    public void RegistrarImpressao(IReadOnlyDictionary<int, int> quantidades, bool cozinha)
    {
        if (ComandaAtual is null) return;
        _comandas.RegistrarImpressao(ComandaAtual.Id, quantidades, cozinha);
        var detalhe = _comandas.BuscarComItens(ComandaAtual.Id)!;
        AplicarResultado(Resultado<ComandaComItens>.Ok(detalhe));
    }
}
