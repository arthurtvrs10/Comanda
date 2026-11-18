using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VarthexComanda.Desktop.Atendimento;

public partial class RecebimentoView : Window
{
    private readonly EncerramentoViewModel _vm;
    public RecebimentoView(EncerramentoViewModel vm)
    {
        InitializeComponent();MaxHeight=Math.Max(540,SystemParameters.WorkArea.Height-40);_vm=vm;DataContext=vm;vm.PagamentoRecebido=false;vm.Mensagem="";
        void Atualizar()=>Salvar.IsEnabled=vm.RegistrarPagamentoCommand.CanExecute(null);
        EventHandler mudou=(_,_)=>Atualizar();vm.RegistrarPagamentoCommand.CanExecuteChanged+=mudou;
        Closed+=(_,_)=>{vm.RegistrarPagamentoCommand.CanExecuteChanged-=mudou;vm.PagamentoRecebido=false;};
        Loaded+=(_,_)=>{Valor.Focus();Valor.SelectAll();};Atualizar();
    }
    private void Salvar_Click(object sender,RoutedEventArgs e)
    {
        if(!_vm.RegistrarPagamentoCommand.CanExecute(null))return;
        var antes=_vm.Pagamentos.Count;_vm.RegistrarPagamentoCommand.Execute(null);
        if(_vm.Pagamentos.Count>antes){if(IsVisible)DialogResult=true;else Close();}
    }
    private void ValorRapido_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:string texto})return;
        if(!_vm.UsaDinheiro&&_vm.SelecionarParaDividir)return;
        var valor=decimal.Parse(texto,CultureInfo.InvariantCulture);
        var atual=_vm.UsaDinheiro?_vm.ValorRecebidoTexto:_vm.ValorPagamentoTexto;
        if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)&&decimal.TryParse(atual,CultureInfo.GetCultureInfo("pt-BR"),out var anterior))valor+=anterior;
        if(_vm.UsaDinheiro)_vm.ValorRecebidoTexto=valor.ToString("0.00",CultureInfo.GetCultureInfo("pt-BR"));
        else _vm.ValorPagamentoTexto=valor.ToString("0.00",CultureInfo.GetCultureInfo("pt-BR"));
    }
}
