using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Ajuda;
public sealed class JanelaDiagnosticoServicos : Window
{
    private readonly Func<IReadOnlyList<VerificacaoServico>> _avaliar;
    private readonly ListView _lista=new();
    private readonly TextBlock _mensagem=new(){TextWrapping=TextWrapping.Wrap};
    private readonly Button _executar=new(){Content="Verificar novamente",Margin=new Thickness(0,0,8,0)};
    public JanelaDiagnosticoServicos(Func<IReadOnlyList<VerificacaoServico>> avaliar)
    {
        _avaliar=avaliar;Title="Diagnóstico de serviços";Width=900;Height=570;MinWidth=640;MinHeight=420;DialogoCompacto.Aplicar(this);
        var raiz=new DockPanel { Margin=new Thickness(14) };Content=raiz;
        var cabecalho=new StackPanel { Margin=new Thickness(0,0,0,12) };cabecalho.Children.Add(new TextBlock { Text="Verificação de funcionamento",FontSize=18,FontWeight=FontWeights.SemiBold });cabecalho.Children.Add(new TextBlock { Text="Resultados obtidos neste computador. A verificação não altera cadastros ou pedidos.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,6,0,0) });DockPanel.SetDock(cabecalho,Dock.Top);raiz.Children.Add(cabecalho);
        var rodape=new StackPanel { Margin=new Thickness(0,10,0,0) };rodape.Children.Add(_mensagem);
        var botoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,8,0,0) };_executar.Click+=async(_,_)=>await Executar();botoes.Children.Add(_executar);
        var copiar=new Button { Content="Copiar resultado",Margin=new Thickness(0,0,8,0) };copiar.Click+=(_,_)=>{try{Clipboard.SetText(string.Join(Environment.NewLine,_lista.Items.Cast<VerificacaoServico>().Select(r=>$"{r.Servico}: {r.Estado} — {r.Detalhe}")));}catch{_mensagem.Text="Não foi possível copiar. Tente novamente.";}};botoes.Children.Add(copiar);
        var fechar=new Button { Content="Fechar",IsCancel=true };fechar.Click+=(_,_)=>Close();botoes.Children.Add(fechar);rodape.Children.Add(botoes);DockPanel.SetDock(rodape,Dock.Bottom);raiz.Children.Add(rodape);
        var vista=new GridView();vista.Columns.Add(new(){Header="Serviço",Width=160,DisplayMemberBinding=new Binding(nameof(VerificacaoServico.Servico))});vista.Columns.Add(new(){Header="Resultado",Width=125,CellTemplate=(DataTemplate)System.Windows.Markup.XamlReader.Parse("<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><StackPanel Orientation='Horizontal' VerticalAlignment='Center'><Ellipse Width='8' Height='8' Margin='0,0,6,0'><Ellipse.Style><Style TargetType='Ellipse'><Setter Property='Fill' Value='{DynamicResource RibbonSubtitle}'/><Style.Triggers><DataTrigger Binding='{Binding Estado}' Value='Funcionando'><Setter Property='Fill' Value='{DynamicResource Success}'/></DataTrigger><DataTrigger Binding='{Binding Estado}' Value='Falha'><Setter Property='Fill' Value='{DynamicResource TextB71C1C}'/></DataTrigger><DataTrigger Binding='{Binding Estado}' Value='Atenção'><Setter Property='Fill' Value='{DynamicResource Text87582D}'/></DataTrigger></Style.Triggers></Style></Ellipse.Style></Ellipse><TextBlock Text='{Binding Estado}' VerticalAlignment='Center'/></StackPanel></DataTemplate>")});
        var template=(DataTemplate)System.Windows.Markup.XamlReader.Parse("<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><TextBlock Text='{Binding Detalhe}' TextWrapping='Wrap' VerticalAlignment='Center'/></DataTemplate>");
        var detalhe=new GridViewColumn { Header="Verificação / orientação",Width=530,CellTemplate=template };vista.Columns.Add(detalhe);_lista.View=vista;
        var estilo=new Style(typeof(ListViewItem),System.Windows.Application.Current.TryFindResource(typeof(ListViewItem)) as Style);estilo.Setters.Add(new Setter(Control.PaddingProperty,new Thickness(6)));estilo.Setters.Add(new Setter(Control.MinHeightProperty,44d));_lista.ItemContainerStyle=estilo;
        _lista.SizeChanged+=(_,_)=>detalhe.Width=Math.Max(200,_lista.ActualWidth-325);System.Windows.Automation.AutomationProperties.SetName(_lista,"Resultado do diagnóstico de serviços");raiz.Children.Add(_lista);
        Loaded+=async(_,_)=>await Executar();
    }
    public async Task Executar()
    {
        if(!_executar.IsEnabled)return;_executar.IsEnabled=false;_mensagem.Text="Verificando os serviços…";
        try{var resultado=await Task.Run(_avaliar);_lista.ItemsSource=resultado;_mensagem.Text=$"Verificado em {DateTime.Now:dd/MM/yyyy HH:mm:ss} · {resultado.Count(r=>r.Estado=="Falha")} falha(s) · {resultado.Count(r=>r.Estado=="Atenção")} aviso(s).";}
        catch{_mensagem.Text="Não foi possível concluir o diagnóstico. Tente novamente ou contate o suporte.";}
        finally{_executar.IsEnabled=true;}
    }
}
