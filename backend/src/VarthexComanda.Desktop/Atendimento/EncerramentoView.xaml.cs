using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class EncerramentoView : Window
{
    private readonly Action<Window>? _imprimir;
    private readonly Action<EncerramentoViewModel>? _editarRecebimento;
    public bool ImprimirAposFinalizar { get; private set; }
    private bool _concluiu;
    private void Forma_Click(object sender,RoutedEventArgs e)=>AbrirRecebimento();
    private void AbrirRecebimento()
    {
        if(DataContext is not EncerramentoViewModel vm||vm.Processando||vm.ContaQuitada||vm.FormaSelecionada is null)return;
        if(vm.SelecionarParaDividir&&vm.TotalSelecionadoCentavos==0){vm.Mensagem="Selecione os itens que esta pessoa vai pagar.";return;}
        if(_editarRecebimento is not null)_editarRecebimento(vm);
        else new RecebimentoView(vm){Owner=this}.ShowDialog();
    }
    private void FinalizarImprimir_Click(object sender,RoutedEventArgs e)
    {
        if(DataContext is not EncerramentoViewModel vm||!vm.ContaQuitada||!vm.ConfirmarEncerrarCommand.CanExecute(null))return;
        ImprimirAposFinalizar=true;vm.ConfirmarEncerrarCommand.Execute(null);
        if(!_concluiu)ImprimirAposFinalizar=false;
    }
    private void Imprimir_Click(object sender,RoutedEventArgs e)
    {
        try{_imprimir?.Invoke(this);}
        catch{if(DataContext is EncerramentoViewModel vm)vm.Mensagem="Não foi possível abrir a impressão. Verifique a impressora e tente novamente.";}
    }
    private void RemoverPagamento_Click(object sender,RoutedEventArgs e)
    {
        if(DataContext is EncerramentoViewModel vm&&sender is FrameworkElement { DataContext: PagamentoRegistrado registro }&&new JanelaConfirmador().Confirmar("Remover registro?","O registro será desconsiderado na conta. Isso não devolve dinheiro nem estorna pagamentos externos. Continuar?"))vm.RemoverRegistro(registro);
    }
    private void Conferir_Click(object sender,RoutedEventArgs e)
    {
        if(ConferirItens?.IsChecked!=true&&DataContext is EncerramentoViewModel vm)vm.SelecionarParaDividir=false;
        AtualizarLayoutPagamento();
    }
    private void AoRedimensionar(object sender,SizeChangedEventArgs e)=>AtualizarLayoutPagamento();
    private bool _atualizandoLayout;
    private int _ultimoModo=-1;
    private bool? _ultimaConferencia;
    private void AtualizarLayoutPagamento()
    {
        if(PainelItens is null||ConferirItens is null||_atualizandoLayout)return;
        _atualizandoLayout=true;
        try
        {
        var largura=Content is FrameworkElement raiz&&raiz.ActualWidth>0?raiz.ActualWidth:Width;
        var modo=largura<700?0:largura<900?1:2;
        if(_ultimoModo==modo&&_ultimaConferencia==ConferirItens.IsChecked)return;
        _ultimoModo=modo;_ultimaConferencia=ConferirItens.IsChecked;
        var compacto=largura<700;
        var medio=largura>=700&&largura<900;
        var conferir=ConferirItens.IsChecked==true;
        PainelItens.Visibility=conferir?Visibility.Visible:Visibility.Collapsed;
        ColunaItens.Width=medio?new GridLength(210):compacto||!conferir?new GridLength(0):new GridLength(1,GridUnitType.Star);
        ColunaFormas.Width=compacto||medio?new GridLength(1,GridUnitType.Star):new GridLength(210);
        ColunaResumo.Width=compacto||medio?new GridLength(0):new GridLength(1,GridUnitType.Star);
        Grid.SetColumn(PainelItens,compacto?1:0);Grid.SetRow(PainelItens,0);
        Grid.SetColumn(PainelFormas,1);Grid.SetRow(PainelFormas,compacto?1:0);
        Grid.SetColumn(PainelResumo,compacto?1:2);Grid.SetRow(PainelResumo,compacto?2:0);
        PainelItens.Margin=compacto?new Thickness(0,0,0,16):new Thickness(0,0,16,0);
        PainelFormas.Margin=compacto?new Thickness(0,0,0,16):new Thickness(0,0,16,0);
        Grid.SetColumnSpan(PainelItens,1);
        if(medio)
        {
            ColunaItens.Width=new GridLength(210);ColunaFormas.Width=new GridLength(1,GridUnitType.Star);ColunaResumo.Width=new GridLength(0);
            Grid.SetColumn(PainelItens,0);Grid.SetColumnSpan(PainelItens,2);Grid.SetRow(PainelItens,0);
            Grid.SetColumn(PainelFormas,0);Grid.SetRow(PainelFormas,1);
            Grid.SetColumn(PainelResumo,1);Grid.SetRow(PainelResumo,1);
            PainelItens.Margin=new Thickness(0,0,0,16);PainelFormas.Margin=new Thickness(0,0,16,0);
        }
        }
        finally { _atualizandoLayout=false; }
    }

    private void Forma_Checked(object sender, RoutedEventArgs e)
    {
        if (DataContext is EncerramentoViewModel vm && sender is RadioButton { CommandParameter: FormaPagamento forma })
            vm.SelecionarFormaCommand.Execute(forma);
    }

    private void Pagamento_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if(e.OriginalSource is TextBox or PasswordBox)return;
        if (Keyboard.Modifiers != ModifierKeys.None || DataContext is not EncerramentoViewModel vm) return;
        RadioButton? opcao = e.Key switch
        {
            Key.D1 or Key.NumPad1 => FormaDinheiro,
            Key.D2 or Key.NumPad2 => FormaPix,
            Key.D3 or Key.NumPad3 => FormaDebito,
            Key.D4 or Key.NumPad4 => FormaCredito,
            _ => null
        };
        if (opcao is not null)
        {
            e.Handled = true;
            if (e.IsRepeat || vm.Processando) return;
            opcao.IsChecked = true;
            opcao.Focus();
            AbrirRecebimento();
        }
        else if (e.Key == Key.Space)
        {
            e.Handled = true;
            if (e.IsRepeat || vm.Processando || !vm.FormaSelecionada.HasValue) return;
            AbrirRecebimento();
        }
    }

    public EncerramentoView(EncerramentoViewModel viewModel,Action<Window>? imprimir=null,Action<EncerramentoViewModel>? editarRecebimento=null)
    {
        InitializeComponent();
        DataContext = viewModel;_imprimir=imprimir;_editarRecebimento=editarRecebimento;
        BotaoImprimir.Visibility=imprimir is null?Visibility.Collapsed:Visibility.Visible;
        BotaoFinalizarImprimir.Visibility=imprimir is null?Visibility.Collapsed:Visibility.Visible;
        AtualizarLayoutPagamento();
        ((FrameworkElement)Content).SizeChanged+=(_,_)=>AtualizarLayoutPagamento();
        viewModel.Concluido += (sender, sucesso) =>
        {
            _concluiu=sucesso;
            DialogResult = sucesso;
            Close();
        };
    }
}
