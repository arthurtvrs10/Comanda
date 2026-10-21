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
    private readonly TextBlock _vazio = new() { Text = "Nenhum colaborador cadastrado. Clique em Novo para criar um login individual.", TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(24) };
    private readonly Button _editar;

    public JanelaColaboradores(ServicoColaboradores servico, AcessoGestor gestor)
    {
        if (!gestor.Autenticado) throw new InvalidOperationException("Entre como gestor para gerenciar colaboradores.");
        _servico = servico;_gestor = gestor;
        Title = "Colaboradores";Width = Math.Min(1100,SystemParameters.WorkArea.Width);Height = Math.Min(660,SystemParameters.WorkArea.Height);
        MinWidth = 760;MinHeight = 420;ResizeMode = ResizeMode.CanResize;
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
        _pesquisa.TextChanged += (_,_)=>Atualizar();busca.Children.Add(_pesquisa);barra.Children.Add(busca);raiz.Children.Add(barra);
        var view = new GridView { AllowsColumnReorder = false };
        void Coluna(string titulo,string propriedade,double largura)=>view.Columns.Add(new GridViewColumn { Header = titulo,DisplayMemberBinding = new Binding(propriedade),Width = largura });
        Coluna("Nome",nameof(Colaborador.Nome),220);Coluna("CPF",nameof(Colaborador.Cpf),115);Coluna("Função",nameof(Colaborador.Funcao),140);
        Coluna("Telefone",nameof(Colaborador.Telefone),125);Coluna("Login",nameof(Colaborador.Login),130);Coluna("Perfil",nameof(Colaborador.PerfilDescricao),100);Coluna("Ativo",nameof(Colaborador.AtivoDescricao),65);
        _lista.View = view;
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
        _mensagem.Margin = new Thickness(0,8,0,0);Grid.SetRow(_mensagem,2);raiz.Children.Add(_mensagem);
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
            _lista.ItemsSource = dados;_lista.SelectedItem = dados.FirstOrDefault(c=>c.Id==selecionado)??dados.FirstOrDefault();
            _vazio.Visibility = dados.Length==0 ? Visibility.Visible : Visibility.Collapsed;
            _vazio.Text = texto.Length==0 ? "Nenhum colaborador cadastrado. Clique em Novo para criar um login individual." : "Nenhum colaborador encontrado. Revise a pesquisa.";
            _mensagem.Text = $"{dados.Length} colaborador(es). As credenciais de gestor e PIN da equipe continuam disponíveis no login.";
        }
        catch { _mensagem.Text = "Não foi possível carregar os colaboradores. Preserve os dados e contate o suporte."; }
    }
    private void Editar(bool novo=false)
    {
        if(!_gestor.Autenticado){_mensagem.Text="Entre novamente como gestor para editar colaboradores.";return;}
        if(!novo && _lista.SelectedItem is not Colaborador)return;
        var atual = novo ? new Colaborador() : (Colaborador)_lista.SelectedItem;
        var janela = new JanelaEditarColaborador(_servico,_gestor,atual,novo) { Owner=this };
        if(janela.ShowDialog()==true)Atualizar();
    }
}

public sealed class JanelaEditarColaborador : Window
{
    public JanelaEditarColaborador(ServicoColaboradores servico,AcessoGestor gestor,Colaborador atual,bool novo)
    {
        Title=novo?"Novo colaborador":"Editar colaborador";Width=550;MaxHeight=SystemParameters.WorkArea.Height;SizeToContent=SizeToContent.Height;ResizeMode=ResizeMode.NoResize;
        DialogoCompacto.Aplicar(this);
        var painel=new StackPanel { Margin=new Thickness(16) };var scroll=new ScrollViewer { Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto };scroll.SetResourceReference(Control.BackgroundProperty,"Canvas");Content=scroll;
        var alterou=false;var salvo=false;
        TextBox Campo(string nome,string valor,int max)
        {
            var campo=new TextBox { Text=valor,MaxLength=max,Margin=new Thickness(0,0,0,8) };
            System.Windows.Automation.AutomationProperties.SetName(campo,nome);
            painel.Children.Add(new Label { Content=nome,Target=campo,Padding=new Thickness(0,0,0,3) });painel.Children.Add(campo);
            campo.TextChanged+=(_,_)=>alterou=true;return campo;
        }
        var nome=Campo("Nome",atual.Nome,120);var cpf=Campo("CPF (opcional)",atual.Cpf,14);var funcao=Campo("Função",atual.Funcao,80);var telefone=Campo("Telefone",atual.Telefone,30);var login=Campo("Login",atual.Login,60);
        var perfil=new ComboBox { SelectedValuePath="Tag",DisplayMemberPath="Content",Margin=new Thickness(0,0,0,8) };
        perfil.Items.Add(new ComboBoxItem { Content="Garçom · pedidos e histórico",Tag="garcom" });perfil.Items.Add(new ComboBoxItem { Content="Cozinha · consulta de pedidos",Tag="cozinha" });perfil.Items.Add(new ComboBoxItem { Content="Gestor · administração do sistema",Tag="gestor" });
        perfil.SelectedValue=atual.Perfil;perfil.SelectionChanged+=(_,_)=>alterou=true;
        painel.Children.Add(new Label { Content="Perfil de acesso",Target=perfil,Padding=new Thickness(0,0,0,3) });painel.Children.Add(perfil);
        var ativo=new CheckBox { Content="Colaborador ativo",IsChecked=atual.Ativo,Margin=new Thickness(0,0,0,8) };ativo.Click+=(_,_)=>alterou=true;painel.Children.Add(ativo);
        var propriaConta=servico.SessaoAtual?.Id==atual.Id;
        var senha=new PasswordBox { MaxLength=128,Margin=new Thickness(0,0,0,8) };var confirmar=new PasswordBox { MaxLength=128,Margin=new Thickness(0,0,0,8) };
        if(!propriaConta)
        {
            painel.Children.Add(new Label { Content=novo?"Senha (8 a 128 caracteres)":"Nova senha (deixe vazia para manter)",Target=senha,Padding=new Thickness(0,0,0,3) });painel.Children.Add(senha);
            painel.Children.Add(new Label { Content="Confirmar senha",Target=confirmar,Padding=new Thickness(0,0,0,3) });painel.Children.Add(confirmar);
            senha.PasswordChanged+=(_,_)=>alterou=true;confirmar.PasswordChanged+=(_,_)=>alterou=true;
        }
        else painel.Children.Add(new TextBlock { Text="Para alterar sua própria senha, use Alterar senha na faixa Configurações.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,8) });
        var erro=new TextBlock { TextWrapping=TextWrapping.Wrap };erro.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");painel.Children.Add(erro);
        var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,10,0,0) };
        acoes.Children.Add(new Button { Content="Voltar",IsCancel=true,MinWidth=95,Margin=new Thickness(0,0,8,0) });
        var salvar=new Button { Content="Salvar",IsDefault=true,MinWidth=95 };acoes.Children.Add(salvar);painel.Children.Add(acoes);
        salvar.Click+=(_,_)=>
        {
            try
            {
                if(!gestor.Autenticado)throw new InvalidOperationException("Entre como gestor para salvar este colaborador.");
                if(senha.Password!=confirmar.Password)throw new ArgumentException("As senhas não coincidem. Digite novamente.");
                if(propriaConta && (ativo.IsChecked!=true || !Equals(perfil.SelectedValue,atual.Perfil)))throw new InvalidOperationException("Para alterar seu perfil ou desativar sua conta, entre com outro gestor.");
                servico.Salvar(atual with { Nome=nome.Text,Cpf=cpf.Text,Funcao=funcao.Text,Telefone=telefone.Text,Login=login.Text,Perfil=(string)perfil.SelectedValue,Ativo=ativo.IsChecked==true },string.IsNullOrEmpty(senha.Password)?null:senha.Password);
                salvo=true;DialogResult=true;
            }
            catch(Exception ex) { erro.Text=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível salvar o colaborador. Verifique os dados e tente novamente."; }
        };
        Closing+=(_,e)=>{if(!salvo && alterou && !new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?","As alterações deste colaborador não foram salvas. Deseja descartá-las?"))e.Cancel=true;};
        Closed+=(_,_)=>{senha.Clear();confirmar.Clear();};Loaded+=(_,_)=>nome.Focus();
    }
}
