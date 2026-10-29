using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace VarthexComanda.Desktop;

public sealed class FaixaConfiguracaoInicial : Border
{
    private readonly string[] _mensagens=["Conclua a configuração inicial do seu estabelecimento.","Confira os dados que aparecem na sua comanda impressa.","Prepare o catálogo e as formas de pagamento para começar."];
    private readonly TextBlock _texto=new(){TextWrapping=TextWrapping.Wrap,VerticalAlignment=VerticalAlignment.Center,FontSize=13};
    private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromSeconds(7)};
    private int _indice;
    public event EventHandler? Configurar;
    public FaixaConfiguracaoInicial()
    {
        Visibility=Visibility.Collapsed;Padding=new Thickness(16,10,16,10);BorderThickness=new Thickness(0,0,0,1);
        SetResourceReference(BackgroundProperty,"AjudaSoft");SetResourceReference(BorderBrushProperty,"Line");_texto.SetResourceReference(TextBlock.ForegroundProperty,"Ink");
        var grade=new Grid();grade.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});grade.ColumnDefinitions.Add(new(){Width=GridLength.Auto});Child=grade;
        _texto.Text=_mensagens[0];_texto.Margin=new Thickness(0,0,16,0);grade.Children.Add(_texto);
        var botao=new Button {Content="Configurar",MinHeight=36,MinWidth=110,Padding=new Thickness(16,6,16,6),VerticalAlignment=VerticalAlignment.Center};botao.SetResourceReference(StyleProperty,"BotaoPrimario");
        System.Windows.Automation.AutomationProperties.SetName(botao,"Configurar estabelecimento");botao.Click+=(_,_)=>Configurar?.Invoke(this,EventArgs.Empty);Grid.SetColumn(botao,1);grade.Children.Add(botao);
        _timer.Tick+=(_,_)=>
        {
            if(IsMouseOver||IsKeyboardFocusWithin)return;
            _indice=(_indice+1)%_mensagens.Length;_texto.Text=_mensagens[_indice];
            if(SystemParameters.ClientAreaAnimation)_texto.BeginAnimation(OpacityProperty,new DoubleAnimation(.3,1,TimeSpan.FromMilliseconds(280)));
        };
        Loaded+=(_,_)=>AtualizarAnimacao();Unloaded+=(_,_)=>_timer.Stop();IsVisibleChanged+=(_,_)=>AtualizarAnimacao();
    }
    private void AtualizarAnimacao(){if(IsLoaded&&IsVisible)_timer.Start();else _timer.Stop();}
}
