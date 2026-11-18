using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaColaboradores : Window
{
    private readonly ServicoColaboradores _servico;
    private readonly AcessoGestor _gestor;
    private readonly TextBox _pesquisa = new() { Width = 265, MaxLength = 120 };
    private readonly ListView _lista = new() { HorizontalContentAlignment = HorizontalAlignment.Stretch };
    private readonly TextBlock _mensagem = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _vazio = new() { Text = "Nenhum colaborador cadastrado. Clique em Novo para cadastrar uma pessoa.", TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(24) };
    private readonly Button _editar;
    private Colaborador[] _filtrados = [];
    private int _pagina, _porPagina = 12;
    private readonly Button _anterior = new() { Content = "Anterior", MinWidth = 85 };
    private readonly Button _proxima = new() { Content = "Próxima", MinWidth = 85, Margin = new Thickness(8,0,0,0) };
    private readonly TextBlock _posicao = new() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12,0,12,0) };

    public JanelaColaboradores(ServicoColaboradores servico, AcessoGestor gestor)
    {
        if (!gestor.Autenticado && !servico.Pode("colaboradores")) throw new InvalidOperationException("Entre como gestor para gerenciar colaboradores.");
        _servico = servico;_gestor = gestor;
        Title = "Colaboradores";Width = Math.Min(1380,SystemParameters.WorkArea.Width);Height = Math.Min(660,SystemParameters.WorkArea.Height);
        MinWidth = Math.Min(1024,SystemParameters.WorkArea.Width);MinHeight = 420;ResizeMode = ResizeMode.CanResize;
        DialogoCompacto.Aplicar(this);
        var raiz = new Grid { Margin = new Thickness(10) };
        raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");Content = raiz;
        raiz.RowDefinitions.Add(new() { Height = GridLength.Auto });raiz.RowDefinitions.Add(new());raiz.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var barra = new DockPanel { LastChildFill = true, Margin = new Thickness(0,0,0,10) };
        var acoes = new StackPanel { Orientation = Orientation.Horizontal };
        var opcoes = Botao("Opções", "M6,12 A2,2 0 1 1 2,12 A2,2 0 1 1 6,12 M14,12 A2,2 0 1 1 10,12 A2,2 0 1 1 14,12 M22,12 A2,2 0 1 1 18,12 A2,2 0 1 1 22,12",()=>{});
        opcoes.Click += (_,_) =>
        {
            var menu = new ContextMenu { Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom, HorizontalOffset = -100 };
            var atualizar = new MenuItem { Header = "Atualizar lista" };atualizar.Click += (_,_)=>Atualizar();menu.Items.Add(atualizar);
            opcoes.ContextMenu = menu;menu.PlacementTarget = opcoes;menu.IsOpen = true;
        };
        acoes.Children.Add(opcoes);
        _editar = Botao("Editar", "M15,5 L19,1 L23,5 L8,20 L2,22 L4,16 Z M14,6 L18,10",()=>Editar());acoes.Children.Add(_editar);
        acoes.Children.Add(Botao("Novo (F2)","M12,3 V21 M3,12 H21",()=>Editar(true)));
        DockPanel.SetDock(acoes,Dock.Right);barra.Children.Add(acoes);
        var busca = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        busca.Children.Add(Icone("M10,2 A8,8 0 1 1 9.99,2 M16,16 L23,23"));
        System.Windows.Automation.AutomationProperties.SetName(_pesquisa,"Pesquisar colaboradores por nome, login, CPF ou função");
        _pesquisa.ToolTip = "Pesquisar por nome, login, CPF ou função";
        _pesquisa.TextChanged += (_,_)=>{_pagina=0;Atualizar();};busca.Children.Add(_pesquisa);barra.Children.Add(busca);raiz.Children.Add(barra);
        var view = new GridView { AllowsColumnReorder = false };
        void Coluna(string titulo,string propriedade,double largura)=>view.Columns.Add(new GridViewColumn { Header = titulo,DisplayMemberBinding = new Binding(propriedade),Width = largura });
        void Estado(string titulo, string propriedade, string descricao, double largura)
        {
            var template = (DataTemplate)System.Windows.Markup.XamlReader.Parse("<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'><StackPanel Orientation='Horizontal' VerticalAlignment='Center'><Grid Width='16' Height='16' Margin='0,0,5,0'><Ellipse x:Name='Sinal' Fill='#C62828' Stroke='#851B1B'/><Path x:Name='Marca' Data='M5,5 L11,11 M11,5 L5,11' Stroke='White' StrokeThickness='1.6'/></Grid><TextBlock Text='{Binding " + descricao + "}' VerticalAlignment='Center'/></StackPanel><DataTemplate.Triggers><DataTrigger Binding='{Binding " + propriedade + "}' Value='True'><Setter TargetName='Sinal' Property='Fill' Value='#238636'/><Setter TargetName='Sinal' Property='Stroke' Value='#175D25'/><Setter TargetName='Marca' Property='Data' Value='M4,8 L7,11 L12,5'/></DataTrigger></DataTemplate.Triggers></DataTemplate>");
            view.Columns.Add(new GridViewColumn { Header = titulo, CellTemplate = template, Width = largura });
        }
        Coluna("Nome",nameof(Colaborador.Nome),220);Coluna("CPF",nameof(Colaborador.Cpf),115);Coluna("Função",nameof(Colaborador.Funcao),140);
        Coluna("Telefone",nameof(Colaborador.Telefone),125);Coluna("Login",nameof(Colaborador.Login),130);Coluna("% Comissão",nameof(Colaborador.ComissaoPercentual),90);Estado("Usuário criado",nameof(Colaborador.CriarUsuario),nameof(Colaborador.UsuarioCriadoDescricao),100);Estado("Ativo",nameof(Colaborador.Ativo),nameof(Colaborador.AtivoDescricao),65);Estado("Acesso",nameof(Colaborador.AcessoPermitido),nameof(Colaborador.AcessoPermitidoDescricao),90);Estado("Administrador",nameof(Colaborador.Administrador),nameof(Colaborador.AdministradorDescricao),110);Estado("Senha mestra",nameof(Colaborador.SenhaMestra),nameof(Colaborador.SenhaMestraDescricao),100);
        _lista.View = view;
        ScrollViewer.SetHorizontalScrollBarVisibility(_lista, ScrollBarVisibility.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(_lista, ScrollBarVisibility.Disabled);
        var larguras = view.Columns.Select(c=>c.Width).ToArray();
        _lista.SizeChanged += (_,_) =>
        {
            var disponivel = Math.Max(1,_lista.ActualWidth-28);
            for (var i=0;i<view.Columns.Count;i++)
            {
                view.Columns[i].Width = disponivel * larguras[i] / larguras.Sum();
                var titulo = view.Columns[i].Header is TextBlock t ? t.Text : view.Columns[i].Header.ToString();
                view.Columns[i].Header = new TextBlock { Text=titulo,TextWrapping=TextWrapping.Wrap,Width=Math.Max(1,view.Columns[i].Width-12),VerticalAlignment=VerticalAlignment.Center };
            }
            var capacidade = Math.Max(1,(int)Math.Floor((_lista.ActualHeight-62)/32));
            if(capacidade!=_porPagina){_porPagina=capacidade;MostrarPagina();}
        };
        var itemStyle = new Style(typeof(ListViewItem),TryFindResource(typeof(ListViewItem)) as Style);
        itemStyle.Setters.Add(new Setter(Control.PaddingProperty,new Thickness(4,6,4,6)));itemStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty,30.0));
        itemStyle.Setters.Add(new Setter(Control.TemplateProperty,(ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='ListViewItem'><Border x:Name='Linha' Background='{TemplateBinding Background}' BorderBrush='{DynamicResource Line}' BorderThickness='0,0,0,1' Padding='{TemplateBinding Padding}'><GridViewRowPresenter Content='{TemplateBinding Content}' Columns='{Binding View.Columns, RelativeSource={RelativeSource AncestorType=ListView}}' VerticalAlignment='Center'/></Border><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter Property='Background' Value='{DynamicResource BackgroundE1E5E2}'/></Trigger><Trigger Property='IsSelected' Value='True'><Setter Property='Background' Value='{DynamicResource BackgroundE1E8E4}'/><Setter Property='Foreground' Value='{DynamicResource Ink}'/></Trigger><Trigger Property='IsKeyboardFocusWithin' Value='True'><Setter TargetName='Linha' Property='BorderBrush' Value='{DynamicResource Color1F5FBF}'/><Setter TargetName='Linha' Property='BorderThickness' Value='1'/></Trigger></ControlTemplate.Triggers></ControlTemplate>")));
        _lista.ItemContainerStyle = itemStyle;
        var cabecalho = new Style(typeof(GridViewColumnHeader),TryFindResource(typeof(GridViewColumnHeader)) as Style);
        cabecalho.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty,HorizontalAlignment.Left));cabecalho.Setters.Add(new Setter(Control.PaddingProperty,new Thickness(6,4,6,4)));
        _lista.Resources[typeof(GridViewColumnHeader)] = cabecalho;
        _lista.SelectionChanged += (_,_)=>_editar.IsEnabled = _lista.SelectedItem is Colaborador;
        _lista.MouseDoubleClick += (_,_)=>Editar();_lista.KeyDown += (_,e)=>{if(e.Key==Key.Enter){Editar();e.Handled=true;}};
        System.Windows.Automation.AutomationProperties.SetName(_lista,"Lista de colaboradores");
        var conteudo = new Grid();conteudo.Children.Add(_lista);conteudo.Children.Add(_vazio);Grid.SetRow(conteudo,1);raiz.Children.Add(conteudo);
        var rodape = new DockPanel { Margin = new Thickness(0,8,0,0) };
        var paginas = new StackPanel { Orientation = Orientation.Horizontal };
        paginas.Children.Add(_anterior);paginas.Children.Add(_posicao);paginas.Children.Add(_proxima);
        DockPanel.SetDock(paginas,Dock.Right);rodape.Children.Add(paginas);rodape.Children.Add(_mensagem);
        _anterior.Click += (_,_)=>{_pagina--;MostrarPagina();};_proxima.Click += (_,_)=>{_pagina++;MostrarPagina();};
        Grid.SetRow(rodape,2);raiz.Children.Add(rodape);
        PreviewKeyDown += (_,e)=>{if(e.Key==Key.F2){Editar(true);e.Handled=true;}else if(e.Key==Key.Escape)Close();};
        Loaded += (_,_)=>_pesquisa.Focus();Atualizar();
    }

    private static Path Icone(string dados)
    {
        var icone = new Path { Data = Geometry.Parse(dados),Width = 17,Height = 17,Stretch = Stretch.Uniform,StrokeThickness = 1.5,Margin = new Thickness(0,0,8,0),VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center };
        icone.SetResourceReference(Shape.StrokeProperty,"Ink");return icone;
    }
    private static Button Botao(string texto,string dados,Action acao)
    {
        var painel = new StackPanel { Orientation = Orientation.Horizontal };painel.Children.Add(Icone(dados));painel.Children.Add(new TextBlock { Text = texto,VerticalAlignment = VerticalAlignment.Center });
        var botao = new Button { Content = painel,Margin = new Thickness(6,0,0,0),MinWidth = 95,MinHeight = 34 };botao.Click += (_,_)=>acao();
        System.Windows.Automation.AutomationProperties.SetName(botao,texto);return botao;
    }
    private void Atualizar()
    {
        try
        {
            var selecionado = (_lista.SelectedItem as Colaborador)?.Id;
            var texto = _pesquisa.Text.Trim();
            var dados = _servico.Listar().Where(c=>string.Join(" ",c.Nome,c.Cpf,c.Login,c.Funcao,c.PerfilDescricao).Contains(texto,StringComparison.CurrentCultureIgnoreCase)).ToArray();
            _filtrados = dados;MostrarPagina();
            _lista.SelectedItem = _lista.Items.Cast<Colaborador>().FirstOrDefault(c=>c.Id==selecionado)??_lista.Items.Cast<Colaborador>().FirstOrDefault();
            _vazio.Visibility = dados.Length==0 ? Visibility.Visible : Visibility.Collapsed;
            _vazio.Text = texto.Length==0 ? "Nenhum colaborador cadastrado. Clique em Novo para cadastrar uma pessoa." : "Nenhum colaborador encontrado. Revise a pesquisa.";
            _mensagem.Text = $"{dados.Length} colaborador(es). Para exigir identificação, ative Solicitar senha nas configurações de segurança.";
        }
        catch { _mensagem.Text = "Não foi possível carregar os colaboradores. Preserve os dados e contate o suporte."; }
    }
    private void MostrarPagina()
    {
        var total = Math.Max(1,(_filtrados.Length+_porPagina-1)/_porPagina);
        _pagina=Math.Clamp(_pagina,0,total-1);
        _lista.ItemsSource=_filtrados.Skip(_pagina*_porPagina).Take(_porPagina).ToArray();
        _lista.SelectedItem=_lista.Items.Cast<Colaborador>().FirstOrDefault();
        _anterior.IsEnabled=_pagina>0;_proxima.IsEnabled=_pagina<total-1;
        _posicao.Text=$"{_pagina+1} de {total}";
    }
    private void Editar(bool novo=false)
    {
        if((!_gestor.Autenticado && !_servico.Pode("colaboradores")) || (_servico.SessaoIndividualIniciada && !_servico.Pode("colaboradores"))){_mensagem.Text="Entre novamente como gestor para editar colaboradores.";return;}
        if(!novo && _lista.SelectedItem is not Colaborador)return;
        var atual = novo ? new Colaborador() : (Colaborador)_lista.SelectedItem;
        var janela = new JanelaEditarColaborador(_servico,_gestor,atual,novo) { Owner=this };
        if(janela.ShowDialog()==true)Atualizar();
    }
}
