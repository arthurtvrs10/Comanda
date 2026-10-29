using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VarthexComanda.Desktop.Atendimento;

public partial class CozinhaView
{
    private void Som_Click(object sender,RoutedEventArgs e)=>ViewModel.SomAtivo=!ViewModel.SomAtivo;
    private void LimparEntregues_Click(object sender,RoutedEventArgs e)
    {
        var itens=ViewModel.CapturarEntregues();var quantidade=ViewModel.Colunas[4].Quantidade;
        if(quantidade==0){JanelaAviso.Mostrar("Limpar painel","Não há pedidos entregues nesta visualização para limpar.",TipoAviso.Informacao);return;}
        var janela=new Window {Title="Limpar pedidos entregues do KDS?",Owner=Window.GetWindow(this),Width=540,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner};
        janela.SetResourceReference(BackgroundProperty,"Surface");janela.SetResourceReference(ForegroundProperty,"Ink");
        var raiz=new StackPanel {Margin=new Thickness(24)};
        raiz.Children.Add(new TextBlock {Text="Limpar pedidos entregues do KDS?",FontSize=18,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap});
        var resumo=new TextBlock {Text=quantidade==1?"1 pedido entregue será removido desta visualização do KDS.":$"{quantidade} pedidos entregues serão removidos desta visualização do KDS.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,8)};resumo.SetResourceReference(TextBlock.ForegroundProperty,"AjudaMuted");raiz.Children.Add(resumo);
        raiz.Children.Add(new TextBlock {Text="O status, o histórico de vendas e os dados dos pedidos serão preservados.",TextWrapping=TextWrapping.Wrap,FontSize=14});
        var erro=new TextBlock {TextWrapping=TextWrapping.Wrap};raiz.Children.Add(erro);
        var acoes=new StackPanel {Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,20,0,0)};
        var cancelar=new Button {Content="Cancelar",IsCancel=true,MinHeight=40,Padding=new Thickness(16,8,16,8),Margin=new Thickness(0,0,8,0)};acoes.Children.Add(cancelar);
        var confirmar=new Button {Content="Sim, limpar painel",MinHeight=40,Padding=new Thickness(16,8,16,8),Background=new SolidColorBrush(Color.FromRgb(220,38,38)),Foreground=Brushes.White};
        confirmar.Click+=(_,_)=>{try{ViewModel.LimparEntregues(itens);janela.DialogResult=true;}catch(Exception ex){erro.Text=ex.Message;}};acoes.Children.Add(confirmar);raiz.Children.Add(acoes);janela.Content=raiz;
        janela.Loaded+=(_,_)=>cancelar.Focus();janela.ShowDialog();
    }
}
