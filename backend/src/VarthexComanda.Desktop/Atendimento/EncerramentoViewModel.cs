using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class EncerramentoViewModel : ObservableObject
{
    private readonly EncerrarComanda _encerrarComanda;
    private readonly IComandaRepository _comandas;
    private int _comandaId;
    private readonly ILogger? _logger;

    public EncerramentoViewModel(EncerrarComanda encerrarComanda, IComandaRepository comandas, ILogger? logger = null)
    {
        _encerrarComanda = encerrarComanda;
        _comandas = comandas;
        _logger = logger;
        Itens = new ObservableCollection<ItemComanda>();
    }

    public event EventHandler<bool>? Concluido;

    public ObservableCollection<ItemComanda> Itens { get; }

    [ObservableProperty]
    private int numeroComanda;
    [ObservableProperty]
    private string? nomeCliente;

    [ObservableProperty]
    private long totalCentavos;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarEncerrarCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegistrarPagamentoCommand))]
    private FormaPagamento? formaSelecionada;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarEncerrarCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegistrarPagamentoCommand))]
    private bool processando;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarEncerrarCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegistrarPagamentoCommand))]
    private bool pagamentoRecebido;
    [ObservableProperty] private bool exibirObservacoes=true;
    public Comanda? ComandaAtual { get; private set; }
    public string SituacaoPagamento=>FaltaPagarCentavos==0&&TotalCentavos>0?"Total recebido":PagamentoRecebido?"Pronto para concluir":"Aguardando pagamento";
    public string ResumoConfirmacao=>FaltaPagarCentavos==0&&TotalCentavos>0?"Confira os registros e confirme para encerrar a comanda.":PagamentoRecebido?"Ao confirmar, a venda será registrada e a comanda será encerrada.":"Receba o valor total e marque Pagamento recebido para continuar.";
    partial void OnPagamentoRecebidoChanged(bool value){OnPropertyChanged(nameof(SituacaoPagamento));OnPropertyChanged(nameof(ResumoConfirmacao));}
    private bool _registrado;
    public string Identificador => $"Comanda #{_comandaId:0000}";
    public string FormaSelecionadaDescricao => FormaSelecionada switch
    {
        FormaPagamento.Dinheiro => "Dinheiro", FormaPagamento.Pix => "Pix",
        FormaPagamento.Debito => "Débito", FormaPagamento.Credito => "Crédito", _ => "Escolha a forma de pagamento"
    };
    partial void OnFormaSelecionadaChanged(FormaPagamento? value)
    {
        PagamentoRecebido = false;
        OnPropertyChanged(nameof(FormaSelecionadaDescricao));OnPropertyChanged(nameof(UsaDinheiro));ValorRecebidoTexto="";AtualizarEntrada();
    }

    [RelayCommand]
    private void SelecionarForma(FormaPagamento forma)
    {
        if (!Processando && !_registrado && Enum.IsDefined(forma)) FormaSelecionada = forma;
    }

    [ObservableProperty]
    private string mensagem = string.Empty;

    public void Carregar(int comandaId)
    {
        PagamentoRecebido = false;
        FormaSelecionada = null;
        Processando = false;
        _registrado = false;Pagamentos.Clear();
        Itens.Clear();
        ComandaAtual=null;
        NumeroComanda = 0;
        NomeCliente = null;
        TotalCentavos = 0;
        Mensagem = string.Empty;

        _comandaId = comandaId;
        OnPropertyChanged(nameof(Identificador));
        var detalhe = _comandas.BuscarComItens(comandaId);
        if (detalhe is null)
        {
            Mensagem = "Comanda não encontrada.";
            return;
        }

        ComandaAtual=detalhe.Comanda;
        NumeroComanda = detalhe.Comanda.Numero;
        NomeCliente = detalhe.Comanda.NomeCliente;
        TotalCentavos = detalhe.Comanda.TotalCentavos;
        Itens.Clear();
        foreach (var item in detalhe.Itens)
        {
            Itens.Add(item);
        }
        QuantidadePessoasTexto=Math.Max(1,detalhe.Comanda.QuantidadePessoas).ToString();AtualizarPagamentos();
    }

    [RelayCommand(CanExecute = nameof(PodeConfirmarEncerrar))]
    private void ConfirmarEncerrar()
    {
        if (!PodeConfirmarEncerrar())
        {
            return;
        }

        Processando = true;
        try
        {
            if(Pagamentos.Count==0&&TotalCentavos>0)
            {
                LerValor(ValorPagamentoTexto,out var valor);
                var recebido=UsaDinheiro&&!string.IsNullOrWhiteSpace(ValorRecebidoTexto)&&LerValor(ValorRecebidoTexto,out var r)?r:valor;
                ComandaAtual=_comandas.RegistrarPagamento(_comandaId,new(){Forma=FormaSelecionada!.Value,ValorCentavos=valor,RecebidoCentavos=recebido,RegistradoEm=DateTime.UtcNow});
                AtualizarPagamentos();
            }
            var resultado = _encerrarComanda.Executar(_comandaId, FormaSelecionada);
            if (!resultado.Sucesso)
            {
                Mensagem = string.Join(" ", resultado.Erros);
                return;
            }

            _registrado = true;
            Mensagem = "Pagamento registrado com sucesso.";
            Concluido?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Falha em {Operacao}", "EncerrarComanda");
            Mensagem = "A venda não foi registrada e a comanda continua aberta.";
        }
        finally
        {
            Processando = false;
        }
    }

    private bool PodeConfirmarEncerrar() => !Processando&&!_registrado&&(Pagamentos.Count>0?FaltaPagarCentavos==0:!SelecionarParaDividir&&PagamentoRecebido&&FormaSelecionada.HasValue&&Enum.IsDefined(FormaSelecionada.Value)&&LerValor(ValorPagamentoTexto,out var valor)&&valor==TotalCentavos&&(TotalCentavos==0||RecebidoValido(valor)));

    [RelayCommand]
    private void Voltar() => Concluido?.Invoke(this, false);
}
