using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaLayoutTicket:Window
{
    private readonly LayoutTicketService _servico;
    private readonly LayoutTicket _layout;
    private readonly FlowDocumentScrollViewer _previa=new(){IsToolBarVisible=false,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Background=Brushes.White,Foreground=Brushes.Black};
    private readonly TextBlock _erro=new(){TextWrapping=TextWrapping.Wrap};
    private string _snapshot;
    private readonly PreferenciasSistema _preferencias;
    private readonly Func<string> _nome;
    public LayoutTicket LayoutAtual=>_layout;
    public PreferenciasSistema PreferenciasAtuais=>_preferencias;
    public void ConfirmarSalvo()=>_snapshot=Estado();
    private string Estado()=>JsonSerializer.Serialize(new {Layout=_layout,Cupom=_preferencias});
    public void DefinirPapel(int mm){_layout.PapelMm=mm;Atualizar();}
    public bool TemAlteracoes=>_snapshot!=Estado();
    public void SalvarLayout(){_servico.Salvar(_layout,_preferencias);_snapshot=Estado();}
    public FrameworkElement ExtrairEditor(){var editor=(FrameworkElement)Content;Content=null;return editor;}
    public JanelaLayoutTicket(LayoutTicketService servico,bool embutido=false,PreferenciasSistema? preferencias=null,Func<string>? nome=null)
    {
        _servico=servico;_layout=servico.Carregar()??new();_preferencias=preferencias??servico.Preferencias;_nome=nome??(()=>servico.NomeEstabelecimento);_snapshot=Estado();
        Title="Layout da conta da mesa";Width=1040;Height=750;MinWidth=800;MinHeight=570;WindowStartupLocation=WindowStartupLocation.CenterOwner;FontSize=12;
        SetResourceReference(BackgroundProperty,"Canvas");SetResourceReference(ForegroundProperty,"Ink");
        var raiz=new Grid {Margin=new Thickness(embutido?12:20)};raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");raiz.SetResourceReference(TextElement.ForegroundProperty,"Ink");raiz.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});
        var corpo=new Grid();corpo.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});corpo.ColumnDefinitions.Add(new(){Width=new GridLength(320)});raiz.Children.Add(corpo);
        var opcoes=new StackPanel {Margin=new Thickness(0,0,24,0)};
        opcoes.Children.Add(new TextBlock {Text="Elementos do ticket",FontSize=21,FontWeight=FontWeights.SemiBold});
        var papel=new ComboBox {ItemsSource=new[]{58,80},SelectedItem=_layout.PapelMm,MinHeight=34,Width=130,HorizontalAlignment=HorizontalAlignment.Left};
        var linhaOpcoes=new StackPanel {Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,12)};
        var campoPapel=new StackPanel();campoPapel.Children.Add(new TextBlock {Text="Papel (mm)",Margin=new Thickness(0,0,0,6)});campoPapel.Children.Add(papel);linhaOpcoes.Children.Add(campoPapel);
        papel.SelectionChanged+=(_,_)=>{if(papel.SelectedItem is int mm){_layout.PapelMm=mm;Atualizar();}};
        opcoes.Children.Add(linhaOpcoes);
        var grade=new UniformGrid {Columns=2};opcoes.Children.Add(grade);
        var logo=Opcao(grade,"Imprimir logo",_layout.ImprimirLogo,v=>_layout.ImprimirLogo=v);
        logo.IsEnabled=!string.IsNullOrWhiteSpace(_layout.LogoBase64);
        logo.ToolTip="Cadastre a imagem em Configurações do sistema > Geral e salve as alterações.";
        if(!logo.IsEnabled)opcoes.Children.Add(new TextBlock {Text="Para imprimir a logo, cadastre a imagem em Configurações do sistema > Geral.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)});
        Opcao(grade,"Nome do estabelecimento",_layout.NomeEstabelecimento,v=>_layout.NomeEstabelecimento=v);
        Opcao(grade,"Número da mesa / comanda",_layout.NumeroMesa,v=>_layout.NumeroMesa=v);
        Opcao(grade,"Nome do cliente",_layout.NomeCliente,v=>_layout.NomeCliente=v);
        Opcao(grade,"Data e hora da impressão",_layout.DataHora,v=>_layout.DataHora=v);
        Opcao(grade,"Observações do item",_layout.ObservacoesItem,v=>{_layout.ObservacoesItem=v;_preferencias.ExibirObservacoesCupom=v;});
        Opcao(grade,"Código do pedido (texto)",_layout.CodigoPedido,v=>_layout.CodigoPedido=v);
        Opcao(grade,"Observações do pedido",_layout.ObservacoesPedido,v=>_layout.ObservacoesPedido=v);
        Opcao(grade,"Agrupar itens iguais",_preferencias.AgruparItensCupom,v=>_preferencias.AgruparItensCupom=v);
        Opcao(grade,"Exibir CPF / CNPJ",_preferencias.ExibirDocumentoCupom,v=>_preferencias.ExibirDocumentoCupom=v);
        Opcao(grade,"Exibir endereço",_preferencias.ExibirEnderecoCupom,v=>_preferencias.ExibirEnderecoCupom=v);
        Mensagem(opcoes,"Mensagem do cabeçalho",_preferencias.CabecalhoCupom,v=>_preferencias.CabecalhoCupom=v);
        Mensagem(opcoes,"Mensagem do rodapé",_preferencias.RodapeCupom,v=>_preferencias.RodapeCupom=v);
        opcoes.Children.Add(new TextBlock {Text="Modelo de conferência da conta já utilizado nas mesas. A largura configurada na impressora prevalece na saída em papel.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,16,0,0)});
        corpo.Children.Add(new ScrollViewer {Content=opcoes,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});
        var painelPrevia=new DockPanel();Grid.SetColumn(painelPrevia,1);corpo.Children.Add(painelPrevia);
        corpo.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});corpo.RowDefinitions.Add(new(){Height=new GridLength(0)});
        corpo.SizeChanged+=(_,e)=>
        {
            var compacto=e.NewSize.Width<600;
            corpo.ColumnDefinitions[1].Width=compacto?new GridLength(0):new GridLength(320);
            corpo.RowDefinitions[0].Height=compacto?new GridLength(480):new GridLength(1,GridUnitType.Star);
            corpo.RowDefinitions[1].Height=compacto?new GridLength(540):new GridLength(0);
            Grid.SetColumn(painelPrevia,compacto?0:1);Grid.SetRow(painelPrevia,compacto?1:0);
            painelPrevia.Margin=new Thickness(0,compacto?16:0,0,0);
            if(embutido)raiz.MinHeight=compacto?1080:620;
        };
        var rotulo=new TextBlock {Text="Prévia · dados de exemplo",FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,12)};DockPanel.SetDock(rotulo,Dock.Top);painelPrevia.Children.Add(rotulo);
        var folha=new Border {Background=Brushes.White,BorderThickness=new Thickness(1),BorderBrush=Brushes.LightGray,Padding=new Thickness(8),Child=_previa};painelPrevia.Children.Add(folha);
        var rodape=new DockPanel {Margin=new Thickness(0,16,0,0)};Grid.SetRow(rodape,1);raiz.Children.Add(rodape);
        if(embutido)rodape.Visibility=Visibility.Collapsed;
        var botoes=new StackPanel {Orientation=Orientation.Horizontal};DockPanel.SetDock(botoes,Dock.Right);rodape.Children.Add(botoes);
        var cancelar=new Button {Content="Voltar",MinHeight=38,MinWidth=90,Margin=new Thickness(0,0,8,0)};cancelar.Click+=(_,_)=>Close();botoes.Children.Add(cancelar);
        var salvar=new Button {Content="Salvar layout",MinHeight=38,MinWidth=120};salvar.Click+=(_,_)=>{try{_servico.Salvar(_layout,_preferencias);_snapshot=Estado();_erro.Text="Layout salvo.";}catch(Exception ex){_erro.Text=ex.Message;}};botoes.Children.Add(salvar);rodape.Children.Add(_erro);
        Content=raiz;Atualizar();Loaded+=(_,_)=>Atualizar();Closing+=(_,e)=>{if(_snapshot!=Estado()&&!new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?","O layout do ticket não foi salvo. Deseja descartar as alterações?"))e.Cancel=true;};
    }
    private CheckBox Opcao(Panel grade,string titulo,bool ativo,Action<bool> alterar)
    {
        var alternancia=new CheckBox {Content=new TextBlock {Text=titulo,TextWrapping=TextWrapping.Wrap,MaxWidth=140},IsChecked=ativo,MinHeight=42,VerticalContentAlignment=VerticalAlignment.Center};
        alternancia.SetResourceReference(StyleProperty,"AlternanciaCompacta");alternancia.Click+=(_,_)=>{alterar(alternancia.IsChecked==true);Atualizar();};
        var moldura=new Border {Child=alternancia,Padding=new Thickness(10),BorderThickness=new Thickness(1),Margin=new Thickness(0,0,10,12)};moldura.SetResourceReference(Border.BorderBrushProperty,"Line");moldura.SetResourceReference(Border.BackgroundProperty,"Surface");grade.Children.Add(moldura);
        System.Windows.Automation.AutomationProperties.SetName(alternancia,titulo);
        return alternancia;
    }
    private void Mensagem(Panel painel,string titulo,string valor,Action<string> alterar)
    {
        painel.Children.Add(new RotuloAjuda {Texto=titulo,Margin=new Thickness(0,4,0,4)});
        var campo=new TextBox {Text=valor,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,MinHeight=52,MaxLength=500,Margin=new Thickness(0,0,0,12)};
        System.Windows.Automation.AutomationProperties.SetName(campo,titulo);campo.TextChanged+=(_,_)=>{alterar(campo.Text);Atualizar();};painel.Children.Add(campo);
    }
    private void Atualizar()
    {
        try
        {
            var c=new Comanda {Id=1,Numero=12,NumeroMesa=12,NomeCliente="João Silva",CodigoPedido=102,Status=StatusComanda.Aberta,AbertaEm=new DateTime(2026,10,29,20,19,0),TotalCentavos=0,Observacao="Enviar os itens juntos"};
            var nomes=new[]{"X-Burger Especial","Batata Frita G","Refrigerante Cola"};
            var itens=nomes.Select((nome,i)=>new ItemComanda {Id=i+1,ComandaId=1,ProdutoId=i+1,NomeProduto=nome,Quantidade=i==1?1:2,PrecoUnitarioCentavos=1000,SubtotalCentavos=(i==1?1000:2000),CriadoEm=c.AbertaEm,AtualizadoEm=c.AbertaEm,Observacao=i==0?"Sem cebola":null}).ToArray();
            c.TotalCentavos=itens.Sum(i=>i.SubtotalCentavos);
            _previa.Document=CupomTermico.Conta(_preferencias,string.IsNullOrWhiteSpace(_nome())?"Seu estabelecimento":_nome(),c,itens,_layout.PapelMm,layout:_layout);
        }
        catch(Exception ex){_erro.Text="Não foi possível mostrar a prévia: "+ex.Message;}
    }
}
