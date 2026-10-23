using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Atendimento;

public partial class CozinhaView : UserControl
{
    public CozinhaViewModel ViewModel { get; }
    public Func<IReadOnlyList<LocalCozinha>>? ConsultarLocais { get=>ViewModel.ConsultarLocais;set=>ViewModel.ConsultarLocais=value; }
    public CozinhaView(Func<IReadOnlyList<ComandaComItens>> consultar,
        Func<int,IReadOnlyList<AlteracaoPreparo>,Resultado<ConfirmacaoPreparo>>? alterar=null)
    {
        InitializeComponent();ViewModel=new(consultar,alterar);DataContext=ViewModel;
        ViewModel.NotificarNovoItem=()=>System.Media.SystemSounds.Asterisk.Play();
        var timer=new DispatcherTimer { Interval=TimeSpan.FromSeconds(3) };
        timer.Tick+=async(_,_)=>{if(IsVisible)await ViewModel.AtualizarAsync();};
        Loaded+=async(_,_)=>{await ViewModel.AtualizarAsync();timer.Start();};
        Unloaded+=(_,_)=>timer.Stop();
    }
    public void Atualizar()=>ViewModel.Atualizar();
    private async void Atualizar_Click(object sender,RoutedEventArgs e)=>await ViewModel.AtualizarAsync();
    private async void Item_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is ItemKds item)await ViewModel.AvancarAsync(item);}
    private async void Pronto_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is PedidoKds pedido)await ViewModel.MarcarProntosAsync(pedido);}
    private async void Entregue_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is PedidoKds pedido)await ViewModel.EntregarAsync(pedido);}
    private void Rolagem_SizeChanged(object sender,SizeChangedEventArgs e)
    {
        if(ViewModel is null)return;
        var largura=Math.Max(1,((ScrollViewer)sender).ViewportWidth);
        if(double.IsNaN(largura)||largura<=1)largura=e.NewSize.Width-18;
        var colunas=Math.Max(1,(int)Math.Floor((largura+12)/332));
        ViewModel.LarguraCartao=Math.Max(220,largura/colunas-12);
    }
}
