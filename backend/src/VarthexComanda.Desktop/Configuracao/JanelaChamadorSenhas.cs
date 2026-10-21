using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaChamadorSenhas : Window
{
    private readonly GerenciadorPainelSenhas _painel;
    private readonly TextBox _senha=new() { MaxLength=9,FontSize=24,MinHeight=42,Margin=new Thickness(0,4,0,10),TextAlignment=TextAlignment.Center };
    private readonly TextBlock _mensagem=new() { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,4,0,0) };
    private readonly List<Border> _previas=[];
    private readonly List<TextBlock> _numeros=[];
    private readonly List<TextBlock> _historicos=[];
    private readonly List<bool> _demonstracao=[];
    private PreferenciasPainelSenhas Preferencias=>_painel.Preferencias;
    public JanelaChamadorSenhas(GerenciadorPainelSenhas painel)
    {
        _painel=painel;Title="Painel Eletrônico de Senha";Width=660;Height=Math.Min(660,SystemParameters.WorkArea.Height);MinWidth=640;MinHeight=600;
        DialogoCompacto.Aplicar(this);
        var raiz=new DockPanel { Margin=new Thickness(10) };raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");Content=raiz;
        var rodape=new DockPanel { Margin=new Thickness(0,10,0,0) };
        var acoes=new StackPanel { Orientation=Orientation.Horizontal };
        acoes.Children.Add(BotaoAcao("Encerrar","M4,4 L20,20 M20,4 L4,20",()=>{_painel.Encerrar();return "";}));
        acoes.Children.Add(BotaoAcao("Ocultar esta janela","M22,12 A10,10 0 1 1 2,12 A10,10 0 1 1 22,12 M7,12 H17",()=>{Hide();return "O painel continua disponível em Apps.";}));
        var salvar=Botao("Salvar",()=>{_painel.Salvar();return "Configurações salvas.";});salvar.HorizontalAlignment=HorizontalAlignment.Right;salvar.Visibility=Visibility.Collapsed;DockPanel.SetDock(salvar,Dock.Right);rodape.Children.Add(salvar);rodape.Children.Add(acoes);
        DockPanel.SetDock(rodape,Dock.Bottom);raiz.Children.Add(rodape);
        var estiloMensagem=new Style(typeof(TextBlock));var vazia=new Trigger { Property=TextBlock.TextProperty,Value="" };vazia.Setters.Add(new Setter(VisibilityProperty,Visibility.Collapsed));estiloMensagem.Triggers.Add(vazia);_mensagem.Style=estiloMensagem;
        DockPanel.SetDock(_mensagem,Dock.Bottom);raiz.Children.Add(_mensagem);
        var abas=new TabControl();abas.SetResourceReference(Control.BackgroundProperty,"Canvas");abas.SetResourceReference(Control.ForegroundProperty,"Ink");raiz.Children.Add(abas);
        var estiloAba=new Style(typeof(TabItem));
        estiloAba.Setters.Add(new Setter(Control.TemplateProperty,(ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='TabItem'><Border x:Name='Fundo' Background='{DynamicResource Surface}' BorderBrush='{DynamicResource Line}' BorderThickness='0,0,0,2' Padding='12,7'><ContentPresenter ContentSource='Header' RecognizesAccessKey='True'/></Border><ControlTemplate.Triggers><Trigger Property='IsSelected' Value='True'><Setter TargetName='Fundo' Property='Background' Value='{DynamicResource BackgroundE1E8E4}'/><Setter TargetName='Fundo' Property='BorderBrush' Value='{DynamicResource Accent}'/></Trigger></ControlTemplate.Triggers></ControlTemplate>")));
        abas.Resources[typeof(TabItem)]=estiloAba;
        var chamador=new TabItem { Header="Chamador" };chamador.SetResourceReference(Control.BackgroundProperty,"Surface");chamador.SetResourceReference(Control.ForegroundProperty,"Ink");abas.Items.Add(chamador);
        var cores=new TabItem { Header="Configurações de Cor" };cores.SetResourceReference(Control.BackgroundProperty,"Surface");cores.SetResourceReference(Control.ForegroundProperty,"Ink");abas.Items.Add(cores);
        chamador.Content=CriarChamador();cores.Content=CriarCores();
        abas.SelectionChanged+=(_,_)=>salvar.Visibility=abas.SelectedItem==cores?Visibility.Visible:Visibility.Collapsed;
        _painel.Estado.Alterado+=AtualizarPrevias;
        Closed+=(_,_)=>_painel.Estado.Alterado-=AtualizarPrevias;
        Loaded+=(_,_)=>_senha.Focus();
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Enter&&abas.SelectedItem==chamador){Chamar();e.Handled=true;}};
        AtualizarPrevias();
    }
    private UIElement CriarChamador()
    {
        var grade=new Grid { Margin=new Thickness(10) };grade.ColumnDefinitions.Add(new(){Width=new GridLength(272)});grade.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        var esquerda=new StackPanel { Margin=new Thickness(0,0,24,0) };grade.Children.Add(esquerda);
        esquerda.Children.Add(new Label { Content="Senha:",Target=_senha,Padding=new Thickness(0) });esquerda.Children.Add(_senha);
        System.Windows.Automation.AutomationProperties.SetName(_senha,"Senha para chamar");
        _senha.PreviewTextInput+=(_,e)=>e.Handled=!e.Text.All(char.IsAsciiDigit);
        DataObject.AddPastingHandler(_senha,(_,e)=>{if(e.DataObject.GetData(DataFormats.UnicodeText) is not string texto||!texto.All(char.IsAsciiDigit))e.CancelCommand();});
        var teclado=new Grid();for(int i=0;i<3;i++)teclado.ColumnDefinitions.Add(new());for(int i=0;i<4;i++)teclado.RowDefinitions.Add(new(){Height=GridLength.Auto});
        var valores=new[]{"7","8","9","4","5","6","1","2","3","0","Apagar"};
        for(int i=0;i<valores.Length;i++)
        {
            var valor=valores[i];var tecla=new Button { Content=valor,Height=34,MinHeight=32,FontSize=16,Margin=new Thickness(0,0,i%3==2||valor=="Apagar"?0:4,4),Padding=new Thickness(4) };
            Grid.SetRow(tecla,i/3);Grid.SetColumn(tecla,i%3);if(valor=="Apagar")Grid.SetColumnSpan(tecla,2);
            System.Windows.Automation.AutomationProperties.SetName(tecla,valor);
            tecla.Click+=(_,_)=>{if(valor=="Apagar")_senha.Clear();else if(_senha.Text.Length-_senha.SelectionLength<9)_senha.SelectedText=valor;_senha.CaretIndex=_senha.Text.Length;_senha.Focus();};teclado.Children.Add(tecla);
        }
        esquerda.Children.Add(teclado);
        var chamar=new Button { Content=ConteudoAcao("Chamar Senha (ENTER)","M6,2 L22,12 L6,22 Z"),MinHeight=48,Margin=new Thickness(0,0,0,0),IsEnabled=false };chamar.Click+=(_,_)=>Chamar();_senha.TextChanged+=(_,_)=>chamar.IsEnabled=EstadoPainelSenhas.SenhaValida(_senha.Text);esquerda.Children.Add(chamar);
        var direita=new StackPanel();Grid.SetColumn(direita,1);grade.Children.Add(new ScrollViewer { Content=direita,VerticalScrollBarVisibility=ScrollBarVisibility.Auto }.NaColuna(1));
        direita.Children.Add(new TextBlock { Text="Painel Eletrônico",Margin=new Thickness(0,0,0,8) });direita.Children.Add(CriarPrevia(164));
        var acoes=new UniformGrid { Columns=3,Margin=new Thickness(0,3,0,5) };
        acoes.Children.Add(BotaoAcao("Exibir","M14,3 H22 V11 M22,3 L10,15 M11,4 H2 V23 H21 V14",()=>{_painel.Salvar();return _painel.Exibir();}));
        acoes.Children.Add(BotaoAcao("Limpar","M15,2 L22,9 M19,6 L10,15 M7,11 L14,18 L7,23 L1,18 Z M3,19 L7,15 M6,22 L10,18",()=>{_painel.Limpar();return "Chamadas apagadas.";}));
        acoes.Children.Add(BotaoAcao("Ocultar","M22,12 A10,10 0 1 1 2,12 A10,10 0 1 1 22,12 M7,12 H17",()=>{_painel.Ocultar();return "Painel oculto. Chamar ou Exibir abre novamente.";}));direita.Children.Add(acoes);
        Marcar(direita,"Limpar número após 1 minuto",Preferencias.LimparAposUmMinuto,v=>Preferencias.LimparAposUmMinuto=v);
        var vozes=new StackPanel { Orientation=Orientation.Horizontal,Margin=new Thickness(0,4,0,10) };
        var feminina=new RadioButton { Content="Feminina",GroupName="VozPainel",IsChecked=Preferencias.VozFeminina,Margin=new Thickness(0,0,24,0) };feminina.Checked+=(_,_)=>{Preferencias.VozFeminina=true;PersistirOpcoes();};vozes.Children.Add(feminina);
        var masculina=new RadioButton { Content="Masculina",Margin=new Thickness(60,0,0,0),GroupName="VozPainel",IsChecked=!Preferencias.VozFeminina };masculina.Checked+=(_,_)=>{Preferencias.VozFeminina=false;PersistirOpcoes();};vozes.Children.Add(masculina);direita.Children.Add(vozes);
        Marcar(direita,"Abrir em modo tela cheia",Preferencias.TelaCheia,v=>Preferencias.TelaCheia=v);
        Marcar(direita,"Emitir beep ao chamar",Preferencias.Beep,v=>Preferencias.Beep=v);
        Marcar(direita,"Habilitar voz ao chamar",Preferencias.Voz,v=>Preferencias.Voz=v);
        direita.Children.Add(new TextBlock { Text="Monitor do painel:",Margin=new Thickness(0,2,0,4) });
        var monitores=new ComboBox { HorizontalAlignment=HorizontalAlignment.Stretch };
        void AtualizarMonitores()
        {
            Executar(()=>{var lista=MonitoresPainel.Listar();monitores.ItemsSource=lista;monitores.SelectedItem=lista.FirstOrDefault(m=>m.Id==Preferencias.Monitor)??lista.FirstOrDefault(m=>!m.Principal)??lista[0];return lista.Count==1?"Um monitor conectado. A tela pública abrirá neste monitor.":"";});
        }
        monitores.SelectionChanged+=(_,_)=>{if(monitores.SelectedItem is MonitorPainel monitor){Preferencias.Monitor=monitor.Id;PersistirOpcoes();}};direita.Children.Add(monitores);
        monitores.DropDownOpened+=(_,_)=>AtualizarMonitores();AtualizarMonitores();
        return grade;
    }
    public sealed record CorPainel(string Nome,string Hex);
    private UIElement CriarCores()
    {
        var grade=new Grid { Margin=new Thickness(12,10,12,10) };
        grade.ColumnDefinitions.Add(new(){Width=new GridLength(302)});
        grade.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        var campos=new StackPanel { Margin=new Thickness(0,0,44,0) };grade.Children.Add(campos);
        // A mesma paleta nomeada nos dois seletores, com a amostra dentro do item.
        var paleta=typeof(Colors).GetProperties(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static)
            .Select(p=>(Nome:p.Name,Cor:(Color)p.GetValue(null)!))
            .OrderBy(p=>p.Nome=="Transparent"?0:p.Nome=="Black"?1:p.Nome=="DimGray"?2:p.Nome=="Gray"?3:p.Nome=="DarkGray"?4:p.Nome=="Silver"?5:p.Nome=="LightGray"?6:p.Nome=="Gainsboro"?7:8)
            .ThenBy(p=>p.Nome).ToList();
        ComboBox Cor(string rotulo,string valor,Action<string> alterar)
        {
            campos.Children.Add(new TextBlock { Text=rotulo,Margin=new Thickness(0,0,0,3) });
            var seletor=new ComboBox { HorizontalContentAlignment=HorizontalAlignment.Left,MinHeight=28,MaxDropDownHeight=240,Margin=new Thickness(0,0,0,5) };
            System.Windows.Automation.AutomationProperties.SetName(seletor,rotulo);
            seletor.ItemTemplate=(DataTemplate)System.Windows.Markup.XamlReader.Parse("<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><StackPanel Orientation='Horizontal'><Border Width='21' Height='12' BorderThickness='1' BorderBrush='Gray' Background='{Binding Hex}' Margin='0,0,5,0'/><TextBlock Text='{Binding Nome}' VerticalAlignment='Center'/></StackPanel></DataTemplate>");
            foreach(var (nome,cor) in paleta)
            {
                var hex=cor.A==255?$"#{cor.R:X2}{cor.G:X2}{cor.B:X2}":cor.ToString();
                seletor.Items.Add(new CorPainel(nome,hex));
            }
            seletor.SelectedItem=seletor.Items.Cast<CorPainel>().FirstOrDefault(i=>string.Equals(i.Hex,valor,StringComparison.OrdinalIgnoreCase));
            if(seletor.SelectedItem is null){var personalizado=new CorPainel(valor,valor);seletor.Items.Add(personalizado);seletor.SelectedItem=personalizado;}
            seletor.SelectionChanged+=(_,_)=>{if(seletor.SelectedItem is CorPainel item){alterar(item.Hex);AtualizarPrevias();}};
            campos.Children.Add(seletor);return seletor;
        }
        var primeira=Cor("Cor de fundo:",Preferencias.CorInicial,v=>Preferencias.CorInicial=v);
        var segunda=Cor("Para criar degradê, selecione outra cor:",Preferencias.CorFinal,v=>Preferencias.CorFinal=v);
        campos.Children.Add(new TextBlock { Text="Cor dos números:",Margin=new Thickness(0,0,0,3) });
        var radios=new List<RadioButton>();var numeros=new StackPanel { Orientation=Orientation.Horizontal };
        foreach(var (nome,cor) in new[]{("Branco","#FFFFFF"),("Preto","#000000")})
        {
            var radio=new RadioButton { Content=nome,GroupName="CorNumerosPainel",IsChecked=string.Equals(Preferencias.CorNumeros,cor,StringComparison.OrdinalIgnoreCase),Margin=new Thickness(0,0,60,0) };
            radio.Checked+=(_,_)=>{Preferencias.CorNumeros=cor;AtualizarPrevias();};numeros.Children.Add(radio);radios.Add(radio);
        }
        campos.Children.Add(numeros);
        var visual=new StackPanel();Grid.SetColumn(visual,1);grade.Children.Add(visual);
        visual.Children.Add(CriarPrevia(164,true));
        visual.Children.Add(BotaoAcao("Voltar à configuração padrão","M3,10 A9,9 0 0 1 20,7 L23,11 M23,4 V11 H16 M21,16 A9,9 0 0 1 4,19 L1,15 M1,22 V15 H8",()=>
        {
            primeira.SelectedItem=primeira.Items.Cast<CorPainel>().First(i=>Equals(i.Hex,"#DCDCDC"));
            segunda.SelectedItem=segunda.Items.Cast<CorPainel>().First(i=>Equals(i.Hex,"#1E90FF"));
            radios[1].IsChecked=true;AtualizarPrevias();return "Cores restauradas. Clique em Salvar para aplicar.";
        }));
        return new ScrollViewer { Content=grade,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled };
    }
    private Border CriarPrevia(double altura,bool demonstracao=false)
    {
        var numero=new TextBlock { FontSize=72,FontWeight=demonstracao?FontWeights.Normal:FontWeights.SemiBold,Text="000" };var anteriores=new TextBlock { TextWrapping=TextWrapping.Wrap,FontSize=14,Margin=new Thickness(6),VerticalAlignment=VerticalAlignment.Center };
        var grade=new Grid();grade.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});grade.ColumnDefinitions.Add(new(){Width=new GridLength(0.25,GridUnitType.Star)});
        var ampliacao=new Viewbox { Child=numero,Margin=new Thickness(12) };if(demonstracao)Grid.SetColumnSpan(ampliacao,2);grade.Children.Add(ampliacao);Grid.SetColumn(anteriores,1);grade.Children.Add(anteriores);
        var borda=new Border { Height=altura,Child=grade,Margin=new Thickness(0,0,0,8) };_previas.Add(borda);_numeros.Add(numero);_historicos.Add(anteriores);_demonstracao.Add(demonstracao);return borda;
    }
    private void AtualizarPrevias()
    {
        try
        {
            PainelSenhasService.Validar(Preferencias);
            for(int i=0;i<_previas.Count;i++){_previas[i].Background=JanelaExibicaoSenhas.Fundo(Preferencias);var cor=(Brush)new BrushConverter().ConvertFromString(Preferencias.CorNumeros)!;_numeros[i].Foreground=cor;_historicos[i].Foreground=cor;_numeros[i].Text=_demonstracao[i]?"000":_painel.Estado.Atual??"";_historicos[i].Text=_demonstracao[i]?"":string.Join("\n",_painel.Estado.Anteriores.Take(3));}
        }
        catch(ArgumentException){ }
    }
    private void Chamar()
    {
        Executar(()=>{var aviso=_painel.Chamar(_senha.Text);_senha.SelectAll();_senha.Focus();return string.IsNullOrWhiteSpace(aviso)?"Senha chamada: "+_painel.Estado.Atual:aviso;});
    }
    private static StackPanel ConteudoAcao(string texto,string geometria)
    {
        var linha=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center };
        var icone=new System.Windows.Shapes.Path { Data=Geometry.Parse(geometria),Width=23,Height=23,StrokeThickness=1,Stretch=Stretch.Uniform,Margin=new Thickness(0,0,9,0),VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center };icone.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty,"Line");linha.Children.Add(icone);
        linha.Children.Add(new TextBlock { Text=texto,VerticalAlignment=VerticalAlignment.Center });return linha;
    }
    private Button BotaoAcao(string texto,string geometria,Func<string> acao)
    {
        var b=Botao(texto,acao);b.Content=ConteudoAcao(texto,geometria);b.MinHeight=36;b.Padding=new Thickness(5);b.Margin=new Thickness(0,0,8,0);b.BorderThickness=new Thickness(0);b.Background=Brushes.Transparent;
        System.Windows.Automation.AutomationProperties.SetName(b,texto);return b;
    }
    private Button Botao(string texto,Func<string> acao)
    {
        var b=new Button { Content=texto,Margin=new Thickness(0,0,6,6),Padding=new Thickness(9,5,9,5) };b.Click+=(_,_)=>Executar(acao);return b;
    }
    private void Executar(Func<string> acao)
    {
        try { _mensagem.Text=acao(); }
        catch(Exception ex){_mensagem.Text=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível concluir. Verifique o monitor e as configurações do Windows.";}
    }
    private void PersistirOpcoes()=>Executar(()=>{_painel.Salvar();return "";});
    private void Marcar(Panel painel,string texto,bool valor,Action<bool> alterar)
    {
        var b=new CheckBox { Content=texto,IsChecked=valor,Margin=new Thickness(0,0,0,8) };b.Click+=(_,_)=>{alterar(b.IsChecked==true);PersistirOpcoes();};painel.Children.Add(b);
    }
}

internal static class ColunaPainelExtensions
{
    public static T NaColuna<T>(this T elemento,int coluna) where T:UIElement { Grid.SetColumn(elemento,coluna);return elemento; }
}
