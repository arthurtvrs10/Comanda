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
        Func<int,IReadOnlyList<AlteracaoPreparo>,Resultado<ConfirmacaoPreparo>>? alterar=null,IConfiguracaoRepository? configuracoes=null)
    {
        InitializeComponent();ViewModel=new(consultar,alterar);DataContext=ViewModel;
        if(configuracoes is not null)ViewModel.PreferenciasQuadro=new(configuracoes);
        InicializarAudio(configuracoes);
        var timer=new DispatcherTimer { Interval=TimeSpan.FromSeconds(3) };
        var contador=new DispatcherTimer {Interval=TimeSpan.FromSeconds(1)};
        contador.Tick+=(_,_)=>ViewModel.AtualizarContagem();
        timer.Tick+=async(_,_)=>{if(IsVisible&&!_arrastando&&!RolagemEmUso)await ViewModel.AtualizarAsync();VerificarVolume();};
        Loaded+=async(_,_)=>{await ViewModel.AtualizarAsync();timer.Start();contador.Start();_ = Dispatcher.BeginInvoke(new Action(VerificarVolume));};
        Unloaded+=(_,_)=>{timer.Stop();contador.Stop();PararRolagemAlca();};
    }
    public void Atualizar()=>ViewModel.Atualizar();
    private async void Atualizar_Click(object sender,RoutedEventArgs e)=>await ViewModel.AtualizarAsync();
    private async void Item_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is ItemKds item)await ViewModel.AvancarAsync(item);}
    private async void Pronto_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is PedidoKds pedido)await ViewModel.MarcarProntosAsync(pedido);}
    private async void Entregue_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is PedidoKds pedido)await ViewModel.EntregarAsync(pedido);}
    private async void Preparar_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is PedidoKds pedido)await ViewModel.IniciarAsync(pedido);}
    private void MenuColuna_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:ColunaCozinha coluna} botao)return;
        var menu=new ContextMenu();var editar=new MenuItem {Header="Editar título"};var restaurar=new MenuItem {Header="Restaurar título"};
        editar.Click+=(_,_)=>Formulario("Editar título",[("Título",coluna.Titulo)],valores=>ViewModel.Renomear(coluna,valores[0]));
        restaurar.Click+=(_,_)=>Executar(()=>ViewModel.Renomear(coluna,coluna.Padrao));menu.Items.Add(editar);menu.Items.Add(restaurar);menu.PlacementTarget=botao;menu.IsOpen=true;
    }
    private void NovaChamada_Click(object sender,RoutedEventArgs e)=>Formulario("Registrar chamada da mesa",[("Mesa",""),("Solicitação","Solicita atendimento")],valores=>
    {
        if(!int.TryParse(valores[0],out var mesa))throw new ArgumentException("Digite o número da mesa.");ViewModel.RegistrarChamada(mesa,valores[1]);
    });
    private void AtenderChamada_Click(object sender,RoutedEventArgs e){if((sender as Button)?.Tag is ChamadaMesa chamada)Executar(()=>ViewModel.AtenderChamada(chamada));}
    private static void Executar(Action acao){try{acao();}catch(Exception ex){JanelaAviso.Mostrar("Quadro da cozinha",ex.Message,TipoAviso.Erro);}}
    private void Formulario(string titulo,(string Nome,string Valor)[] campos,Action<string[]> salvar)
    {
        var janela=new Window {Title=titulo,Width=420,SizeToContent=SizeToContent.Height,Owner=Window.GetWindow(this),WindowStartupLocation=WindowStartupLocation.CenterOwner,ResizeMode=ResizeMode.NoResize};
        janela.SetResourceReference(BackgroundProperty,"Canvas");janela.SetResourceReference(ForegroundProperty,"Ink");
        var painel=new StackPanel {Margin=new Thickness(20)};var entradas=new List<TextBox>();
        foreach(var campo in campos){painel.Children.Add(new TextBlock {Text=campo.Nome,Margin=new Thickness(0,8,0,6)});var entrada=new TextBox {Text=campo.Valor,MinHeight=34,MaxLength=campos.Length==1?24:120};entradas.Add(entrada);painel.Children.Add(entrada);}
        var erro=new TextBlock {TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,0)};painel.Children.Add(erro);
        var botao=new Button {Content="Salvar",MinHeight=38,Margin=new Thickness(0,16,0,0),IsDefault=true};botao.Click+=(_,_)=>{try{salvar(entradas.Select(t=>t.Text).ToArray());janela.DialogResult=true;}catch(Exception ex){erro.Text=ex.Message;}};painel.Children.Add(botao);
        janela.Content=painel;janela.Loaded+=(_,_)=>entradas[0].Focus();janela.ShowDialog();
    }
    private void Rolagem_SizeChanged(object sender,SizeChangedEventArgs e)
    {
        if(ViewModel is null)return;
        AjustarLarguraQuadro(e.NewSize.Width);
    }
    private void Rolagem_ScrollChanged(object sender,ScrollChangedEventArgs e)
    {
        if(ReferenceEquals(e.OriginalSource,sender)&&e.ViewportWidthChange!=0)
            AjustarLarguraQuadro(e.ViewportWidth);
    }
    private void AjustarLarguraQuadro(double largura)
    {
        if(!double.IsFinite(largura)||largura<=0)return;
        Quadro.Width=Math.Max(1500,largura);
        ViewModel.Colunas[0].Largura=220;
        foreach(var coluna in ViewModel.Colunas.Skip(1))coluna.Largura=(Quadro.Width-220)/4;
    }
}
