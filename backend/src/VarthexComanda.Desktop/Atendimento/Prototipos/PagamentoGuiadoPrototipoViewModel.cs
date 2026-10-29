using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento.Prototipos;

public sealed partial class PagamentoGuiadoPrototipoViewModel : ObservableObject, IDisposable
{
    public PagamentoFluxoPrototipoViewModel Fluxo { get; }
    [ObservableProperty] private int etapa;
    [ObservableProperty] private string ultimoRecebimento="";
    public bool EscolhendoForma=>Etapa==0;
    public bool InformandoValor=>Etapa==1;
    public bool Conferindo=>Etapa==2;
    public bool ParcelaConcluida=>Etapa==3;
    public bool Quitado=>Etapa==4;
    public bool Encerrado=>Etapa==5;
    public bool PodeVoltarEtapa=>Etapa is 1 or 2;
    public string OrientacaoEtapa=>Etapa is 1 or 2?Fluxo.OrientacaoEntrada:"";
    public string EtapaDescricao=>Etapa switch {0=>"Forma de pagamento",1=>"Valor do pagamento",2=>"Conferência do recebimento",3=>"Recebimento registrado",4=>"Conta quitada",_=>"Venda finalizada"};
    public string TextoAcao=>Etapa switch {0=>"Escolha uma forma para continuar",1=>"Conferir pagamento",2=>Fluxo.TextoRegistrar,3=>"Receber o restante",4=>"Finalizar pedido",_=>"Pedido finalizado"};
    public string DinheiroRecebido=>string.IsNullOrWhiteSpace(Fluxo.Pagamento.ValorRecebidoTexto)?Fluxo.Pagamento.ValorPagamentoTexto:Fluxo.Pagamento.ValorRecebidoTexto;
    public PagamentoGuiadoPrototipoViewModel(PagamentoFluxoPrototipoViewModel fluxo)
    {
        Fluxo=fluxo;etapa=fluxo.Finalizado?5:fluxo.Pagamento.ContaQuitada?4:0;fluxo.PropertyChanged+=AlterouFluxo;
    }
    private void AlterouFluxo(object? sender,PropertyChangedEventArgs e)
    {
        if(Fluxo.Finalizado)Etapa=5;
        else if(Etapa==4&&!Fluxo.Pagamento.ContaQuitada)Etapa=0;
        Atualizar();
    }
    private void Atualizar() { OnPropertyChanged(string.Empty);AcaoCommand.NotifyCanExecuteChanged(); }
    partial void OnEtapaChanged(int value)=>Atualizar();
    [RelayCommand] private void SelecionarForma(FormaPagamento forma)
    {
        if(!Fluxo.MostrarFormulario||!Enum.IsDefined(forma))return;
        Fluxo.SelecionarFormaCommand.Execute(forma);if(Fluxo.Pagamento.FormaSelecionada==forma)Etapa=1;
    }
    [RelayCommand] private void VoltarEtapa() { if(PodeVoltarEtapa)Etapa--; }
    [RelayCommand] private void TrocarForma() { if(Fluxo.MostrarFormulario)Etapa=0; }
    [RelayCommand] private void Metade()
    {
        Fluxo.PorPessoasCommand.Execute(null);Fluxo.Pessoas="2";Fluxo.AplicarDivisaoCommand.Execute(null);
    }
    private bool PodeAcao()=>Etapa switch
    {
        1 or 2=>Fluxo.RegistrarCommand.CanExecute(null),
        3=>Fluxo.MostrarFormulario&&Fluxo.RemocaoPendente is null,
        4=>Fluxo.FinalizarCommand.CanExecute(null),
        _=>false
    };
    [RelayCommand(CanExecute=nameof(PodeAcao))] private void Acao()
    {
        if(!PodeAcao())return;
        switch(Etapa)
        {
            case 1:Etapa=2;break;
            case 2:
                var antes=Fluxo.Pagamento.TotalPagoCentavos;var forma=Fluxo.Pagamento.FormaSelecionadaDescricao;
                Fluxo.RegistrarCommand.Execute(null);var acrescimo=Fluxo.Pagamento.TotalPagoCentavos-antes;
                if(acrescimo>0){UltimoRecebimento=CentavosParaMoedaConverter.Formatar(acrescimo)+" em "+forma;Etapa=Fluxo.Pagamento.ContaQuitada?4:3;}
                else Etapa=Fluxo.Pagamento.ContaQuitada?4:1;
                break;
            case 3:Fluxo.UsarSaldoCommand.Execute(null);Etapa=0;break;
            case 4:Fluxo.FinalizarCommand.Execute(null);break;
        }
    }
    public void Dispose()=>Fluxo.PropertyChanged-=AlterouFluxo;
}
