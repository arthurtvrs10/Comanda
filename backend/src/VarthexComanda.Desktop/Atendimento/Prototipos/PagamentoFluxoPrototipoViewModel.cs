using System.ComponentModel;
using System.Collections.Specialized;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento.Prototipos;

/// <summary>Experimento isolado: reaproveita a gravação atual, sem substituir o fechamento publicado.</summary>
public sealed partial class PagamentoFluxoPrototipoViewModel : ObservableObject, IDisposable
{
    public EncerramentoViewModel Pagamento { get; }
    [ObservableProperty] private int modoDivisao;
    [ObservableProperty] private string pessoas="2";
    [ObservableProperty] private PagamentoRegistrado? remocaoPendente;
    [ObservableProperty] private bool finalizado;
    private bool _registrando;
    public bool MostrarFormulario=>!Finalizado&&!Pagamento.ContaQuitada;
    public bool MostrarQuitacao=>!Finalizado&&Pagamento.ContaQuitada;
    public bool SemRecebimentos=>Pagamento.Pagamentos.Count==0;
    public bool DividirPessoas=>ModoDivisao==1;
    public bool DividirItens=>ModoDivisao==2;
    public bool TemRemocao=>RemocaoPendente is not null;
    public bool PixSelecionado=>Pagamento.FormaSelecionada==FormaPagamento.Pix;
    public bool DinheiroSelecionado=>Pagamento.FormaSelecionada==FormaPagamento.Dinheiro;
    public bool DebitoSelecionado=>Pagamento.FormaSelecionada==FormaPagamento.Debito;
    public bool CreditoSelecionado=>Pagamento.FormaSelecionada==FormaPagamento.Credito;
    public string TituloSaldo=>Finalizado?"Venda finalizada":Pagamento.ContaQuitada?"Total recebido":"Falta receber";
    public long ValorDestaque=>Pagamento.ContaQuitada||Finalizado?Pagamento.TotalPagoCentavos:Pagamento.FaltaPagarCentavos;
    public string ProximoPasso=>Finalizado?"O recebimento foi concluído. A comanda está encerrada.":Pagamento.ContaQuitada?"Confira os recebimentos e finalize o pedido.":Pagamento.FormaSelecionada is null?"Escolha como o cliente pagou.":"Confira o valor e confirme somente depois de receber.";
    public string TextoRegistrar=>Ler(Pagamento.ValorPagamentoTexto,out var valor)&&valor>0&&Pagamento.FormaSelecionada is not null
        ?$"Confirmar {Moeda(valor)} em {Pagamento.FormaSelecionadaDescricao}":"Confirmar recebimento";
    public string ResumoRemocao=>RemocaoPendente is null?"":$"Remover o registro de {Moeda(RemocaoPendente.ValorCentavos)} em {RemocaoPendente.FormaDescricao}?";
    public string OrientacaoEntrada
    {
        get
        {
            if(Finalizado||Pagamento.ContaQuitada)return "";
            if(Pagamento.FormaSelecionada is not {} forma||!Enum.IsDefined(forma))return "Escolha uma forma de pagamento para continuar.";
            if(!Ler(Pagamento.ValorPagamentoTexto,out var valor)||valor<=0)return "Informe um valor maior que zero, com até duas casas decimais.";
            if(valor>Pagamento.FaltaPagarCentavos)return "O valor não pode ultrapassar o saldo de "+Moeda(Pagamento.FaltaPagarCentavos)+".";
            if(DividirItens&&valor!=Pagamento.TotalSelecionadoCentavos)return "Confira os itens selecionados antes de confirmar.";
            if(Pagamento.UsaDinheiro&&!string.IsNullOrWhiteSpace(Pagamento.ValorRecebidoTexto)&&
                (!Ler(Pagamento.ValorRecebidoTexto,out var recebido)||recebido<valor))return "O dinheiro recebido deve ser igual ou maior que o valor deste pagamento.";
            return "";
        }
    }
    public string DivisaoDescricao
    {
        get
        {
            if(!int.TryParse(Pessoas,out var n)||n is <1 or >999)return "Informe de 1 a 999 pessoas.";
            var valor=Pagamento.FaltaPagarCentavos/n;var extra=Pagamento.FaltaPagarCentavos%n;
            return extra==0?$"{n} pessoa(s) × {Moeda(valor)}":$"{extra} pessoa(s) × {Moeda(valor+1)} e {n-extra} × {Moeda(valor)}";
        }
    }
    public PagamentoFluxoPrototipoViewModel(EncerramentoViewModel pagamento)
    {
        Pagamento=pagamento;
        pagamento.PropertyChanged+=AlterouPagamento;
        pagamento.Pagamentos.CollectionChanged+=AlterouLista;
        pagamento.Concluido+=Concluiu;
    }
    private static string Moeda(long valor)=>CentavosParaMoedaConverter.Formatar(valor);
    private static bool Ler(string texto,out long valor)
    {
        valor=0;
        if(!decimal.TryParse(texto.Trim(),NumberStyles.AllowDecimalPoint,CultureInfo.GetCultureInfo("pt-BR"),out var numero)||numero<0||numero>long.MaxValue/100m||decimal.Round(numero,2)!=numero)return false;
        valor=(long)(numero*100);return true;
    }
    private void AlterouPagamento(object? sender,PropertyChangedEventArgs e)=>Atualizar();
    private void AlterouLista(object? sender,NotifyCollectionChangedEventArgs e)=>Atualizar();
    private void Concluiu(object? sender,bool sucesso) { if(sucesso)Finalizado=true; }
    private void Atualizar()
    {
        OnPropertyChanged(string.Empty);
        RegistrarCommand.NotifyCanExecuteChanged();FinalizarCommand.NotifyCanExecuteChanged();AplicarDivisaoCommand.NotifyCanExecuteChanged();
    }
    partial void OnPessoasChanged(string value)=>Atualizar();
    partial void OnFinalizadoChanged(bool value)=>Atualizar();
    partial void OnRemocaoPendenteChanged(PagamentoRegistrado? value)=>Atualizar();
    partial void OnModoDivisaoChanged(int value)
    {
        Pagamento.SelecionarParaDividir=value==2;
        if(value!=2)Pagamento.PreencherRestanteCommand.Execute(null);
        Atualizar();
    }
    [RelayCommand] private void SelecionarForma(FormaPagamento forma)
    {
        if(!MostrarFormulario||_registrando||Pagamento.Processando)return;
        Pagamento.SelecionarFormaCommand.Execute(forma);Pagamento.Mensagem="";Atualizar();
    }
    [RelayCommand] private void ValorLivre()=>ModoDivisao=0;
    [RelayCommand] private void PorPessoas()=>ModoDivisao=1;
    [RelayCommand] private void PorItens()=>ModoDivisao=2;
    [RelayCommand] private void UsarSaldo()
    {
        ModoDivisao=0;Pagamento.PreencherRestanteCommand.Execute(null);Pagamento.Mensagem="";
    }
    private bool PodeAplicarDivisao()=>MostrarFormulario&&int.TryParse(Pessoas,out var n)&&n is >=1 and <=999;
    [RelayCommand(CanExecute=nameof(PodeAplicarDivisao))] private void AplicarDivisao()
    {
        if(!PodeAplicarDivisao())return;
        int.TryParse(Pessoas,out var n);var parte=Pagamento.FaltaPagarCentavos/n+(Pagamento.FaltaPagarCentavos%n>0?1:0);
        Pagamento.ValorPagamentoTexto=(parte/100m).ToString("0.00",CultureInfo.GetCultureInfo("pt-BR"));
    }
    private bool PodeRegistrar()=>MostrarFormulario&&!_registrando&&!Pagamento.Processando&&RemocaoPendente is null&&OrientacaoEntrada.Length==0;
    [RelayCommand(CanExecute=nameof(PodeRegistrar))] private void Registrar()
    {
        if(!PodeRegistrar())return;
        _registrando=true;Atualizar();
        try
        {
            var antes=Pagamento.TotalPagoCentavos;
            Pagamento.PagamentoRecebido=true;
            if(Pagamento.RegistrarPagamentoCommand.CanExecute(null))Pagamento.RegistrarPagamentoCommand.Execute(null);
            if(Pagamento.TotalPagoCentavos>antes)
                Pagamento.FormaSelecionada=null; // Uma nova parcela exige uma nova escolha; duplo clique não paga o saldo seguinte.
            else if(Pagamento.ComandaAtual is {} comanda)
            {
                var mensagem=Pagamento.Mensagem;var forma=Pagamento.FormaSelecionada;var valor=Pagamento.ValorPagamentoTexto;var recebido=Pagamento.ValorRecebidoTexto;
                Pagamento.Carregar(comanda.Id);Pagamento.FormaSelecionada=forma;Pagamento.ValorPagamentoTexto=valor;Pagamento.ValorRecebidoTexto=recebido;
                Pagamento.Mensagem=mensagem+" Saldo atualizado. Confira antes de tentar novamente.";
            }
        }
        finally { Pagamento.PagamentoRecebido=false;_registrando=false;Atualizar(); }
    }
    private bool PodeFinalizar()=>!Finalizado&&!_registrando&&RemocaoPendente is null&&Pagamento.ContaQuitada&&Pagamento.ConfirmarEncerrarCommand.CanExecute(null);
    [RelayCommand(CanExecute=nameof(PodeFinalizar))] private void Finalizar()
    {
        if(PodeFinalizar())Pagamento.ConfirmarEncerrarCommand.Execute(null);
    }
    [RelayCommand] private void PedirRemocao(PagamentoRegistrado? registro)
    {
        if(!Finalizado&&!_registrando&&registro is not null&&Pagamento.Pagamentos.Contains(registro))RemocaoPendente=registro;
    }
    [RelayCommand] private void ManterRegistro()=>RemocaoPendente=null;
    [RelayCommand] private void ConfirmarRemocao()
    {
        if(Finalizado||_registrando||RemocaoPendente is not {} registro)return;
        Pagamento.RemoverRegistro(registro);RemocaoPendente=null;
    }
    public void Dispose()
    {
        Pagamento.PropertyChanged-=AlterouPagamento;Pagamento.Pagamentos.CollectionChanged-=AlterouLista;Pagamento.Concluido-=Concluiu;
    }
}
