using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VarthexComanda.Desktop;

public sealed class FaixaConfiguracaoInicial : Border
{
    private static readonly Brush Azul = new SolidColorBrush(Color.FromRgb(21,95,156));
    private static readonly Brush Tinta = new SolidColorBrush(Color.FromRgb(41,70,81));
    private readonly TextBlock _legenda = new() { FontSize=12, Foreground=Tinta };
    private readonly ProgressBar _progresso = new() { Minimum=0, Maximum=7, Height=6, Margin=new Thickness(0,6,0,0), Foreground=Azul, Background=new SolidColorBrush(Color.FromRgb(210,224,234)), BorderThickness=new Thickness(0) };
    public bool OcultadaNestaSessao { get; private set; }
    public event EventHandler? Configurar;
    public FaixaConfiguracaoInicial()
    {
        Visibility=Visibility.Collapsed; Height=52; Padding=new Thickness(16,0,16,0);
        Background=new SolidColorBrush(Color.FromRgb(239,246,251)); BorderBrush=new SolidColorBrush(Color.FromRgb(204,221,233)); BorderThickness=new Thickness(0,0,0,1);
        var grade=new Grid();
        grade.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
        grade.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        grade.ColumnDefinitions.Add(new(){Width=new GridLength(216)});
        grade.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
        grade.ColumnDefinitions.Add(new(){Width=GridLength.Auto}); Child=grade;
        var titulo=new TextBlock {FontSize=20,FontWeight=FontWeights.Bold,VerticalAlignment=VerticalAlignment.Center};
        titulo.Inlines.Add(new Run("PRIMEIROS "){Foreground=new SolidColorBrush(Color.FromRgb(20,39,47))});
        titulo.Inlines.Add(new Run("PASSOS"){Foreground=Azul}); grade.Children.Add(titulo);
        var descricao=new Border { BorderBrush=BorderBrush,BorderThickness=new Thickness(1,0,0,0),Margin=new Thickness(16,0,16,0),Padding=new Thickness(12,0,0,0),VerticalAlignment=VerticalAlignment.Center,Child=new TextBlock {Text="Prepare seu sistema para o primeiro atendimento.",FontSize=13,Foreground=Tinta,TextTrimming=TextTrimming.CharacterEllipsis}};
        Grid.SetColumn(descricao,1); grade.Children.Add(descricao);
        var andamento=new StackPanel {VerticalAlignment=VerticalAlignment.Center}; andamento.Children.Add(_legenda); andamento.Children.Add(_progresso); Grid.SetColumn(andamento,2); grade.Children.Add(andamento);
        var conteudo=new StackPanel {Orientation=Orientation.Horizontal};
        conteudo.Children.Add(new TextBlock {Text="Configurar agora",VerticalAlignment=VerticalAlignment.Center});
        conteudo.Children.Add(new Path {Data=Geometry.Parse("M0,5 H12 M7,0 L12,5 7,10"),Stroke=Brushes.White,StrokeThickness=1.5,Width=13,Height=11,Margin=new Thickness(10,0,0,0),VerticalAlignment=VerticalAlignment.Center});
        var botao=new Button {Content=conteudo,Height=36,MinHeight=36,MinWidth=180,Margin=new Thickness(28,0,20,0),Padding=new Thickness(16,4,16,4),VerticalAlignment=VerticalAlignment.Center};
        botao.SetResourceReference(StyleProperty,"BotaoPrimario"); botao.Background=Azul; botao.Foreground=Brushes.White;
        AutomationProperties.SetName(botao,"Configurar agora o estabelecimento"); botao.Click+=(_,_)=>Configurar?.Invoke(this,EventArgs.Empty); Grid.SetColumn(botao,3);grade.Children.Add(botao);
        var fechar=new Button {Content=new Path {Data=Geometry.Parse("M0,0 L10,10 M10,0 L0,10"),Stroke=Tinta,StrokeThickness=1.5,Width=11,Height=11},Width=32,Height=32,MinWidth=32,MinHeight=32,Padding=new Thickness(0),Background=Brushes.Transparent,BorderThickness=new Thickness(0),ToolTip="Ocultar até a próxima abertura do sistema",VerticalAlignment=VerticalAlignment.Center};
        AutomationProperties.SetName(fechar,"Ocultar primeiros passos nesta sessão"); fechar.Click+=(_,_)=>{OcultadaNestaSessao=true;Visibility=Visibility.Collapsed;}; Grid.SetColumn(fechar,4);grade.Children.Add(fechar);
        SizeChanged+=(_,_)=>{descricao.Visibility=ActualWidth<1180?Visibility.Collapsed:Visibility.Visible;titulo.FontSize=ActualWidth<900?16:20;};
        AtualizarProgresso(0);
    }
    public void AtualizarProgresso(int concluidas)
    {
        concluidas=Math.Clamp(concluidas,0,7);_legenda.Text=$"{concluidas} de 7 etapas concluídas";_progresso.Value=concluidas;AutomationProperties.SetName(_progresso,_legenda.Text);
    }
}
