using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class EncerramentoViewModel
{
    [ObservableProperty] private string observacaoPagamento="";
    public bool ContaQuitada=>TotalCentavos>0&&FaltaPagarCentavos==0;
    public double ProgressoPagamento=>TotalCentavos<=0?0:Math.Clamp(100d*TotalPagoCentavos/TotalCentavos,0,100);
    public string RotuloSaldo=>ContaQuitada?"100% pago":"Falta pagar";
    public string RotuloFinalizar=>ContaQuitada?"Finalizar pedido":"Aguardando pagamento";
    public ObservableCollection<ItemPagamentoSelecao> ItensParaDividir { get; }=[];
    [ObservableProperty] private bool selecionarParaDividir;
    public long TotalSelecionadoCentavos=>ItensParaDividir.Where(i=>i.Selecionado).Sum(i=>i.Item.SubtotalCentavos);
    public string ResumoSelecao=>$"Selecionado: {CentavosParaMoedaConverter.Formatar(TotalSelecionadoCentavos)}"+(TotalSelecionadoCentavos>FaltaPagarCentavos?" · A seleção supera o saldo restante. Confira os pagamentos já registrados.":"");
    partial void OnSelecionarParaDividirChanged(bool value)
    {
        foreach(var item in ItensParaDividir)item.Selecionado=false;
        AtualizarSelecao();
    }
    private void AtualizarSelecao()
    {
        OnPropertyChanged(nameof(TotalSelecionadoCentavos));OnPropertyChanged(nameof(ResumoSelecao));
        ValorPagamentoTexto=((SelecionarParaDividir?TotalSelecionadoCentavos:FaltaPagarCentavos)/100m).ToString("0.00",CultureInfo.GetCultureInfo("pt-BR"));
        PagamentoRecebido=false;AtualizarEntrada();
    }
    private void AtualizarItensParaDividir()
    {
        ItensParaDividir.Clear();
        var pagos=Pagamentos.SelectMany(p=>p.ItensIds).ToHashSet();
        foreach(var item in Itens)ItensParaDividir.Add(new(item,pagos.Contains(item.Id),AtualizarSelecao));
        AtualizarSelecao();
    }
    public ObservableCollection<PagamentoRegistrado> Pagamentos { get; }=[];
    [ObservableProperty] private string valorPagamentoTexto="";
    [ObservableProperty] private string valorRecebidoTexto="";
    [ObservableProperty] private string quantidadePessoasTexto="1";
    public long TotalPagoCentavos=>Pagamentos.Sum(p=>p.ValorCentavos);
    public long FaltaPagarCentavos=>Math.Max(0,TotalCentavos-TotalPagoCentavos);
    public long TrocoCentavos=>FormaSelecionada==FormaPagamento.Dinheiro&&LerValor(ValorPagamentoTexto,out var valor)&&LerValor(ValorRecebidoTexto,out var recebido)?Math.Max(0,recebido-valor):0;
    public bool UsaDinheiro=>FormaSelecionada==FormaPagamento.Dinheiro;
    public string DivisaoDescricao
    {
        get
        {
            if(!int.TryParse(QuantidadePessoasTexto,out var n)||n is <1 or >999)return "Informe de 1 a 999 pessoas.";
            var basePessoa=TotalCentavos/n;var extra=TotalCentavos%n;
            return extra==0?$"{n} × {CentavosParaMoedaConverter.Formatar(basePessoa)}":$"{extra} pessoa(s): {CentavosParaMoedaConverter.Formatar(basePessoa+1)} · {n-extra} pessoa(s): {CentavosParaMoedaConverter.Formatar(basePessoa)}";
        }
    }
    partial void OnValorPagamentoTextoChanged(string value)=>AtualizarEntrada();
    partial void OnValorRecebidoTextoChanged(string value)=>AtualizarEntrada();
    partial void OnQuantidadePessoasTextoChanged(string value)=>OnPropertyChanged(nameof(DivisaoDescricao));
    private static bool LerValor(string texto,out long valor)
    {
        valor=0;
        if(!decimal.TryParse(texto.Trim(),NumberStyles.AllowDecimalPoint,CultureInfo.GetCultureInfo("pt-BR"),out var numero)||numero<0||numero>long.MaxValue/100m||decimal.Round(numero,2)!=numero)return false;
        valor=(long)(numero*100);return true;
    }
    private void AtualizarEntrada()
    {
        OnPropertyChanged(nameof(TrocoCentavos));RegistrarPagamentoCommand.NotifyCanExecuteChanged();ConfirmarEncerrarCommand.NotifyCanExecuteChanged();
    }
    private void AtualizarPagamentos()
    {
        Pagamentos.Clear();foreach(var p in PagamentosConta.Ler(ComandaAtual?.PagamentosJson).Where(p=>!p.Cancelado))Pagamentos.Add(p);
        OnPropertyChanged(nameof(TotalPagoCentavos));OnPropertyChanged(nameof(FaltaPagarCentavos));OnPropertyChanged(nameof(DivisaoDescricao));
        ValorPagamentoTexto=(FaltaPagarCentavos/100m).ToString("0.00",CultureInfo.GetCultureInfo("pt-BR"));ValorRecebidoTexto="";
        OnPropertyChanged(nameof(ContaQuitada));OnPropertyChanged(nameof(ProgressoPagamento));OnPropertyChanged(nameof(RotuloSaldo));OnPropertyChanged(nameof(RotuloFinalizar));
        ObservacaoPagamento="";AtualizarItensParaDividir();
        PagamentoRecebido=false;OnPropertyChanged(nameof(SituacaoPagamento));OnPropertyChanged(nameof(ResumoConfirmacao));AtualizarEntrada();
    }
    [RelayCommand] private void PreencherRestante(){SelecionarParaDividir=false;PreencherValorRestante();}
    private void PreencherValorRestante()=>ValorPagamentoTexto=(FaltaPagarCentavos/100m).ToString("0.00",CultureInfo.GetCultureInfo("pt-BR"));
    private bool RecebidoValido(long valor)=>!UsaDinheiro||string.IsNullOrWhiteSpace(ValorRecebidoTexto)||LerValor(ValorRecebidoTexto,out var recebido)&&recebido>=valor;
    private bool PodeRegistrarPagamento()=>!Processando&&!_registrado&&PagamentoRecebido&&FormaSelecionada.HasValue&&Enum.IsDefined(FormaSelecionada.Value)&&LerValor(ValorPagamentoTexto,out var valor)&&valor>0&&valor<=FaltaPagarCentavos&&(!SelecionarParaDividir||valor==TotalSelecionadoCentavos)&&(!UsaDinheiro||string.IsNullOrWhiteSpace(ValorRecebidoTexto)||LerValor(ValorRecebidoTexto,out var recebido)&&recebido>=valor);
    [RelayCommand(CanExecute=nameof(PodeRegistrarPagamento))]
    private void RegistrarPagamento()
    {
        if(!PodeRegistrarPagamento())return;
        try
        {
            Processando=true;LerValor(ValorPagamentoTexto,out var valor);
            var recebido=UsaDinheiro&&!string.IsNullOrWhiteSpace(ValorRecebidoTexto)&&LerValor(ValorRecebidoTexto,out var r)?r:valor;
            ComandaAtual=_comandas.RegistrarPagamento(_comandaId,new(){Forma=FormaSelecionada!.Value,ValorCentavos=valor,RecebidoCentavos=recebido,RegistradoEm=DateTime.UtcNow,Observacao=string.IsNullOrWhiteSpace(ObservacaoPagamento)?null:ObservacaoPagamento.Trim(),ItensIds=SelecionarParaDividir?ItensParaDividir.Where(i=>i.Selecionado).Select(i=>i.Item.Id).ToArray():[]});
            AtualizarPagamentos();Mensagem="Recebimento salvo na comanda.";
        }
        catch(Exception ex){Mensagem=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível salvar o recebimento. Verifique antes de tentar novamente.";}
        finally{Processando=false;AtualizarEntrada();}
    }
    public void RemoverRegistro(PagamentoRegistrado registro)
    {
        if(Processando||_registrado)return;
        try{ComandaAtual=_comandas.RemoverRegistroPagamento(_comandaId,registro.Id);AtualizarPagamentos();Mensagem="Registro removido. Nenhuma devolução foi feita pelo sistema.";}
        catch(Exception ex){Mensagem=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível remover o registro.";}
    }
}

public sealed partial class ItemPagamentoSelecao : ObservableObject
{
    public ItemComanda Item { get; }
    public bool PodeSelecionar { get; }
    public string Situacao=>PodeSelecionar?"":"Pago";
    private readonly Action _alterado;
    public ItemPagamentoSelecao(ItemComanda item,bool pago,Action alterado){Item=item;PodeSelecionar=!pago;_alterado=alterado;}
    [ObservableProperty] private bool selecionado;
    partial void OnSelecionadoChanged(bool value)
    {
        if(value&&!PodeSelecionar){Selecionado=false;return;}
        _alterado();
    }
}
