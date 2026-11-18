using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Catalogo;

public sealed class GrupoComplementoEdicao : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();
    private string _nome = "Novo grupo";
    public string Nome { get=>_nome; set=>SetProperty(ref _nome,value); }
    public string Minimo { get; set; } = "0";
    private string _maximo="0";
    public string Maximo { get=>_maximo;set=>SetProperty(ref _maximo,value); }
    public ObservableCollection<OpcaoComplementoEdicao> Opcoes { get; } = [];
    public void AtualizarMaximoPorEstoque(IReadOnlyList<Produto>? catalogo)
    {
        var total=Opcoes.Sum(o=>catalogo?.FirstOrDefault(p=>p.Id==o.ComplementoId) is Produto p
            ?(p.ControlarEstoque?(decimal)p.QuantidadeEstoque:999m):1m);
        Maximo=Math.Min(int.MaxValue,total).ToString(CultureInfo.InvariantCulture);
    }
}

public sealed class OpcaoComplementoEdicao
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int? ComplementoId { get; init; }
    public string Nome { get; set; } = "";
    public string Preco { get; set; } = "0,00";
}

public sealed class JanelaComplementosProduto : Window
{
    public List<GrupoComplementos> Resultado { get; private set; } = [];
    public JanelaComplementosProduto(IReadOnlyList<GrupoComplementos> atuais, IReadOnlyList<Produto>? catalogo = null)
    {
        Title = catalogo is null ? "Complementos do produto" : "Vínculo de complementos"; Width=940; Height=650; MinWidth=800; MinHeight=550;
        DialogoCompacto.Aplicar(this);
        foreach(var tipo in new[] { typeof(Label),typeof(TextBlock) })
        {
            var estilo=new Style(tipo,TryFindResource(tipo) as Style);
            estilo.Setters.Add(new Setter(tipo==typeof(Label)?Control.ForegroundProperty:TextBlock.ForegroundProperty,new DynamicResourceExtension("Ink")));
            Resources[tipo]=estilo;
        }
        var cultura=CultureInfo.GetCultureInfo("pt-BR");
        var grupos=new ObservableCollection<GrupoComplementoEdicao>(atuais.Select(g=>
        {
            var draft=new GrupoComplementoEdicao { Id=g.Id,Nome=g.Nome,Minimo=g.Minimo.ToString(),Maximo=g.Maximo.ToString() };
            foreach(var o in g.Opcoes)draft.Opcoes.Add(new() { Id=o.Id,ComplementoId=o.ComplementoId,Nome=o.Nome,Preco=(o.PrecoCentavos/100m).ToString("0.00",cultura) });
            return draft;
        }));
        var raiz=new DockPanel { Margin=new Thickness(16) }; raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas"); Content=raiz;
        var rodape=new StackPanel();DockPanel.SetDock(rodape,Dock.Bottom);raiz.Children.Add(rodape);
        var erro=new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,8) };erro.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");rodape.Children.Add(erro);
        var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };
        var voltar=new Button { Content="Voltar",IsCancel=true,MinWidth=95,Margin=new Thickness(0,0,8,0) };acoes.Children.Add(voltar);
        var aplicar=new Button { Content=catalogo is null?"Aplicar ao produto":"Salvar vínculo",MinWidth=150 };acoes.Children.Add(aplicar);rodape.Children.Add(acoes);
        var introducao=new TextBlock { Text=(catalogo is null?"Crie grupos como Adicionais ou Molhos. Mínimo 0 torna o grupo opcional. O preço é somado a cada unidade do produto.":"Organize os complementos permitidos em grupos, como Adicionais ou Molhos. Nome, preço, estoque e limites são definidos no cadastro de cada complemento."),TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,14) };DockPanel.SetDock(introducao,Dock.Top);raiz.Children.Add(introducao);
        var colunas=new Grid();colunas.ColumnDefinitions.Add(new() { Width=new GridLength(210) });colunas.ColumnDefinitions.Add(new());raiz.Children.Add(colunas);
        var esquerda=new DockPanel { Margin=new Thickness(0,0,16,0) };colunas.Children.Add(esquerda);
        var comandos=new StackPanel();DockPanel.SetDock(comandos,Dock.Bottom);esquerda.Children.Add(comandos);
        var novo=new Button { Content="Adicionar grupo",Margin=new Thickness(0,8,0,4) };comandos.Children.Add(novo);
        var remover=new Button { Content="Remover grupo" };comandos.Children.Add(remover);
        var lista=new ListBox { ItemsSource=grupos,DisplayMemberPath="Nome" };esquerda.Children.Add(lista);
        var direita=new DockPanel();Grid.SetColumn(direita,1);colunas.Children.Add(direita);
        var campos=new StackPanel();DockPanel.SetDock(campos,Dock.Top);direita.Children.Add(campos);
        TextBox Campo(string titulo,string propriedade,int limite)
        {
            var t=new TextBox { MaxLength=limite,Margin=new Thickness(0,0,0,8) };t.SetBinding(TextBox.TextProperty,new Binding(propriedade) { Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged });
            campos.Children.Add(new Label { Content=titulo,Target=t });campos.Children.Add(t);return t;
        }
        Campo("Nome do grupo","Nome",80);
        if(catalogo is null) { Campo("Mínimo de opções obrigatórias","Minimo",10);Campo("Máximo de opções permitidas","Maximo",10); }
        campos.Children.Add(new TextBlock { Text=catalogo is null?"Novos grupos começam com mínimo 0. O máximo considera o estoque disponível.":"Configure mínimo, máximo e estoque em Complementos → Editar complemento. Aqui você escolhe apenas os complementos permitidos neste produto.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,8) });
        campos.Children.Add(new TextBlock { Text="Opções e preços adicionais (R$) · use 0,00 para uma opção gratuita",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,4,0,8) });
        var botoesOpcoes=new StackPanel { Orientation=Orientation.Horizontal,Margin=new Thickness(0,8,0,0) };DockPanel.SetDock(botoesOpcoes,Dock.Bottom);direita.Children.Add(botoesOpcoes);
        var adicionarOpcao=new Button { Content="Adicionar opção",Margin=new Thickness(0,0,8,0) };botoesOpcoes.Children.Add(adicionarOpcao);
        var removerOpcao=new Button { Content="Remover opção" };botoesOpcoes.Children.Add(removerOpcao);
        var seletor=new ComboBox { ItemsSource=catalogo,DisplayMemberPath="Nome",MinWidth=240,MaxWidth=400,Margin=new Thickness(0,0,8,0),Visibility=catalogo is null?Visibility.Collapsed:Visibility.Visible };
        System.Windows.Automation.AutomationProperties.SetName(seletor,"Complemento cadastrado");
        botoesOpcoes.Children.Insert(0,seletor);
        var tabela=new DataGrid { IsReadOnly=catalogo is not null,AutoGenerateColumns=false,CanUserAddRows=false,CanUserDeleteRows=false,SelectionMode=DataGridSelectionMode.Single,MinRowHeight=32 };
        tabela.Columns.Add(new DataGridTextColumn { Header="Nome do complemento",MinWidth=220,Binding=new Binding("Nome") { UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged },Width=new DataGridLength(1,DataGridLengthUnitType.Star) });
        tabela.Columns.Add(new DataGridTextColumn { Header="Preço adicional (R$)",Binding=new Binding("Preco") { UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged },Width=150 });direita.Children.Add(tabela);
        lista.SelectionChanged+=(_,_)=>{direita.DataContext=lista.SelectedItem;direita.IsEnabled=lista.SelectedItem is GrupoComplementoEdicao;tabela.ItemsSource=(lista.SelectedItem as GrupoComplementoEdicao)?.Opcoes;};
        novo.Click+=(_,_)=>{if(grupos.Count>=10){erro.Text="Use até 10 grupos por produto.";return;}var g=new GrupoComplementoEdicao();if(catalogo is null)g.Opcoes.Add(new());g.AtualizarMaximoPorEstoque(catalogo);grupos.Add(g);lista.SelectedItem=g;};
        remover.Click+=(_,_)=>{if(lista.SelectedItem is GrupoComplementoEdicao g){grupos.Remove(g);lista.SelectedItem=grupos.FirstOrDefault();}};
        adicionarOpcao.Click+=(_,_)=>{if(lista.SelectedItem is GrupoComplementoEdicao g && g.Opcoes.Count<30){if(catalogo is not null && seletor.SelectedItem is not Produto){erro.Text="Selecione um complemento cadastrado.";return;}
            var escolhido=seletor.SelectedItem as Produto;
            if(escolhido is not null && g.Opcoes.Any(o=>o.ComplementoId==escolhido.Id)){erro.Text="Este complemento já está no grupo.";return;}
            var o=escolhido is null?new OpcaoComplementoEdicao():new OpcaoComplementoEdicao { ComplementoId=escolhido.Id,Nome=escolhido.Nome,Preco=(escolhido.PrecoCentavos/100m).ToString("0.00",cultura) };g.Opcoes.Add(o);g.AtualizarMaximoPorEstoque(catalogo);tabela.SelectedItem=o;tabela.ScrollIntoView(o);}};
        removerOpcao.Click+=(_,_)=>{if(lista.SelectedItem is GrupoComplementoEdicao g && tabela.SelectedItem is OpcaoComplementoEdicao o){g.Opcoes.Remove(o);g.AtualizarMaximoPorEstoque(catalogo);}};
        aplicar.Click+=(_,_)=>
        {
            tabela.CommitEdit(DataGridEditingUnit.Cell,true);tabela.CommitEdit(DataGridEditingUnit.Row,true);
            var resultado=new List<GrupoComplementos>();
            foreach(var g in grupos)
            {
                if(!int.TryParse(g.Minimo,out var min)||!int.TryParse(g.Maximo,out var max)){erro.Text="Informe números inteiros nos limites de seleção.";return;}
                var opcoes=new List<OpcaoComplemento>();
                foreach(var o in g.Opcoes)
                {
                    if(!decimal.TryParse(o.Preco,NumberStyles.AllowDecimalPoint,cultura,out var valor)||valor<0||valor>1_000_000||decimal.Round(valor,2)!=valor){erro.Text="Informe preços positivos ou zero, com até duas casas decimais.";return;}
                    opcoes.Add(new() { Id=o.Id,ComplementoId=o.ComplementoId,Nome=o.Nome.Trim(),PrecoCentavos=(long)(valor*100) });
                }
                resultado.Add(new() { Id=g.Id,Nome=g.Nome.Trim(),Minimo=min,Maximo=max,Opcoes=opcoes });
            }
            if(ComplementosProduto.Validar(resultado,UnidadeVenda.Unidade) is string mensagem){erro.Text=mensagem;return;}
            Resultado=resultado;DialogResult=true;
        };
        lista.SelectedItem=grupos.FirstOrDefault();direita.IsEnabled=lista.SelectedItem is not null;
    }
}
