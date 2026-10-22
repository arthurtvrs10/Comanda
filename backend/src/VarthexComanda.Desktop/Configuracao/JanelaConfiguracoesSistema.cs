using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaConfiguracoesSistema : Window
{
    public static readonly string[] Secoes = ["Geral", "Ajustes de cupom", "Imagem de fundo", "Origem do Cliente", "Locais / cozinhas", "Código de Barras por Kg", "Sobre"];
    private readonly ConfiguracoesSistemaService _service;
    private readonly Func<int> _maiorComanda;
    private readonly Func<IReadOnlyList<Produto>> _produtos;
    private readonly Func<string> _diagnostico;
    private readonly Action<string> _acao;
    private readonly Action _salvo;
    private readonly Func<VarthexComanda.Application.Licenciamento.DadosLicenca?> _assinatura;
    private PreferenciasSistema _prefs;
    private string _nome;
    private string _quantidade;
    private string _snapshot;
    private readonly StackPanel _form=new() { Margin=new Thickness(12) };
    private readonly TextBlock _mensagem=new() { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(12) };
    private readonly TextBlock _titulo=new() { FontSize=14,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,10) };
    private string _secao="Geral";
    private Panel? _camposGeral;
    public bool TemAlteracoes => _snapshot != Estado();
    private int _inicioComandas = 1;
    private string Estado()=>JsonSerializer.Serialize(new { Preferencias=_prefs,Nome=_nome,Quantidade=_quantidade });

    public JanelaConfiguracoesSistema(ConfiguracoesSistemaService service, ConfiguracaoEstabelecimento atual,
        Func<int> maiorComanda, Func<IReadOnlyList<Produto>> produtos, Func<string> diagnostico, Action<string> acao, Action salvo,
        Func<VarthexComanda.Application.Licenciamento.DadosLicenca?>? assinatura=null)
    {
        _service=service;_prefs=service.Carregar();_nome=atual.NomeEstabelecimento;_quantidade=atual.QuantidadeMaximaComandas?.ToString()??"";_inicioComandas=atual.NumeroInicialComandas;
        _maiorComanda=maiorComanda;_produtos=produtos;_diagnostico=diagnostico;_acao=acao;_salvo=salvo;_snapshot=Estado();
        _assinatura=assinatura??(()=>new Licenciamento.ServicoLicenca().Verificar());
        Title="Configurações do sistema";Width=Math.Min(1180,SystemParameters.WorkArea.Width);Height=Math.Min(760,SystemParameters.WorkArea.Height);
        MinWidth=800;MinHeight=580;ResizeMode=ResizeMode.CanResize;ShowInTaskbar=false;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        SetResourceReference(BackgroundProperty,"Canvas");SetResourceReference(ForegroundProperty,"Ink");FontSize=12;AplicarDensidade(this);
        var raiz=new Grid();raiz.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});
        raiz.ColumnDefinitions.Add(new(){Width=new GridLength(180)});raiz.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});Content=raiz;
        raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");
        var template=(ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='RadioButton'><Border x:Name='Fundo' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' Background='Transparent' Padding='{TemplateBinding Padding}'><ContentPresenter VerticalAlignment='Center'/></Border><ControlTemplate.Triggers><Trigger Property='IsChecked' Value='True'><Setter TargetName='Fundo' Property='Background' Value='{DynamicResource BackgroundE1E8E4}'/></Trigger><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Fundo' Property='BorderBrush' Value='{DynamicResource Line}'/><Setter TargetName='Fundo' Property='BorderThickness' Value='1'/></Trigger></ControlTemplate.Triggers></ControlTemplate>");
        var navegacao=new StackPanel { Margin=new Thickness(5,10,5,0) };
        foreach(var secao in Secoes)
        {
            var b=new RadioButton { Content=secao,GroupName="SecoesSistema",MinHeight=40,Padding=new Thickness(8),FontSize=12,IsChecked=secao=="Geral" };
            b.Template=template;b.FocusVisualStyle=(Style)FindResource("FocoVisivel");
            b.Checked+=(_,_)=>Mostrar(secao);navegacao.Children.Add(b);
        }
        var lateral=new Border { Child=navegacao,BorderThickness=new Thickness(0,0,1,0) };lateral.SetResourceReference(Border.BackgroundProperty,"Surface");lateral.SetResourceReference(Border.BorderBrushProperty,"Line");raiz.Children.Add(lateral);
        var scroll=new ScrollViewer { Content=_form,VerticalScrollBarVisibility=ScrollBarVisibility.Auto };Grid.SetColumn(scroll,1);raiz.Children.Add(scroll);
        var rodape=new DockPanel { Margin=new Thickness(12,0,12,8),LastChildFill=true };
        var acoes=new StackPanel { Orientation=Orientation.Horizontal };
        var avancadas=new Button { Content="Opções avançadas",Margin=new Thickness(0,0,8,0) };avancadas.Click+=(_,_)=>Avancadas(avancadas);acoes.Children.Add(avancadas);
        var salvar=new Button { Content="Salvar alterações",MinWidth=105,MinHeight=28,FontSize=12,Padding=new Thickness(12,5,12,5) };
        salvar.SetResourceReference(StyleProperty,"BotaoPrimario");salvar.Click+=(_,_)=>Salvar();acoes.Children.Add(salvar);
        DockPanel.SetDock(acoes,Dock.Right);rodape.Children.Add(acoes);rodape.Children.Add(_mensagem);Grid.SetRow(rodape,1);Grid.SetColumnSpan(rodape,2);raiz.Children.Add(rodape);
        Closing+=(_,e)=>{if(TemAlteracoes && !new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?","Há alterações não salvas. Deseja descartá-las e fechar?"))e.Cancel=true;};
        Mostrar("Geral");
    }
    private static void AplicarDensidade(Window janela)
    {
        janela.FontSize=12;
        foreach(var tipo in new[]{typeof(TextBox),typeof(ComboBox),typeof(Button),typeof(CheckBox),typeof(RadioButton),typeof(ComboBoxItem),typeof(GridViewColumnHeader)})
        {
            var estilo=new Style(tipo,System.Windows.Application.Current.TryFindResource(tipo) as Style);
            estilo.Setters.Add(new Setter(Control.FontSizeProperty,12.0));
            estilo.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty,VerticalAlignment.Center));
            estilo.Setters.Add(new Setter(FrameworkElement.MinHeightProperty,tipo==typeof(CheckBox)||tipo==typeof(RadioButton)?24.0:28.0));
            estilo.Setters.Add(new Setter(Control.PaddingProperty,tipo==typeof(Button)?new Thickness(12,5,12,5):new Thickness(5,3,5,3)));
            janela.Resources[tipo]=estilo;
        }
    }
    private static bool? MostrarJanelaCompacta(Window janela)
    {
        AplicarDensidade(janela);return janela.ShowDialog();
    }
    private void Texto(string texto)=>_form.Children.Add(new TextBlock { Text=texto,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,10) });
    private void Subtitulo(string texto)=>_form.Children.Add(new TextBlock { Text=texto,FontSize=12,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,8,0,8) });
    private TextBox Campo(string nome,string valor,Action<string>? alterar=null,bool multilinha=false)
    {
        var input=new TextBox { Text=valor,MinHeight=28,MaxLength=multilinha?500:200,AcceptsReturn=multilinha,TextWrapping=multilinha?TextWrapping.Wrap:TextWrapping.NoWrap,IsReadOnly=alterar is null };
        if(multilinha)input.Height=58;
        System.Windows.Automation.AutomationProperties.SetName(input,nome);
        var destino=_camposGeral;
        if(destino is null){_form.Children.Add(new Label { Content=nome,Target=input,Padding=new Thickness(0,0,0,3) });_form.Children.Add(input);}
        else {var bloco=new StackPanel { Margin=new Thickness(0,0,12,0) };bloco.Children.Add(new Label { Content=nome,Target=input,Padding=new Thickness(0,0,0,3) });bloco.Children.Add(input);destino.Children.Add(bloco);}
        input.Margin=new Thickness(0,0,0,7);if(alterar!=null)input.TextChanged+=(_,_)=>alterar(input.Text);return input;
    }
    private void Marcar(string texto,bool valor,Action<bool> alterar)
    {
        var c=new CheckBox { Content=texto,IsChecked=valor,MinHeight=26,Margin=new Thickness(0,0,0,3) };c.Click+=(_,_)=>alterar(c.IsChecked==true);_form.Children.Add(c);
    }
    private Button Botao(string texto,Action acao)
    {
        var b=new Button { Content=texto,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,8,12) };b.Click+=(_,_)=>Executar(acao);_form.Children.Add(b);return b;
    }
    private void Executar(Action acao){try{acao();}catch(Exception ex){_mensagem.Text=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível concluir a operação. Verifique os dados e tente novamente.";}}
    private void Mostrar(string secao)
    {
        _secao=secao;_camposGeral=null;_form.Children.Clear();_titulo.Text=secao;_form.Children.Add(_titulo);
        switch(secao)
        {
            case "Geral": Geral();break;
            case "Ajustes de cupom": Cupom();break;
            case "Imagem de fundo": Fundo();break;
            case "Origem do Cliente": Origens();break;
            case "Locais / cozinhas": Cozinhas();break;
            case "Código de Barras por Kg": Codigo();break;
            case "Sobre": Sobre();break;
        }
    }
    private void Geral()
    {
        Subtitulo("Dados da assinatura");
        try
        {
            var dados=_assinatura()??throw new InvalidOperationException();
            _camposGeral=new System.Windows.Controls.Primitives.UniformGrid { Columns=3 };_form.Children.Add(_camposGeral);
            Campo("Registrada para",dados.Cliente);Campo("Identificação da licença",dados.Id);
            Campo("Validade",$"{dados.Inicio.ToLocalTime():dd/MM/yyyy} a {(dados.Vencimento is null?"Vitalícia":dados.Vencimento.Value.ToLocalTime().ToString("dd/MM/yyyy"))}");
            Botao("Consultar ativação",()=>_acao("licenca"));
        }
        catch { Texto("Não foi possível consultar a assinatura. Abra a ativação para verificar.");Botao("Consultar ativação",()=>_acao("licenca")); }
        Subtitulo("Dados do estabelecimento");Texto("Estes dados são usados no sistema e no comprovante não fiscal. Alterá-los não modifica a assinatura.");
        _camposGeral=new System.Windows.Controls.Primitives.UniformGrid { Columns=2 };_form.Children.Add(_camposGeral);
        Campo("Nome fantasia",_nome,v=>_nome=v);Campo("Razão social",_prefs.RazaoSocial,v=>_prefs.RazaoSocial=v);
        Campo("CNPJ / CPF",_prefs.Documento,v=>_prefs.Documento=v);Campo("Inscrição estadual",_prefs.InscricaoEstadual,v=>_prefs.InscricaoEstadual=v);
        Campo("Telefone principal",_prefs.Telefone,v=>_prefs.Telefone=v);Campo("Endereço",_prefs.Endereco,v=>_prefs.Endereco=v);
        Campo("Número",_prefs.Numero,v=>_prefs.Numero=v);Campo("Complemento",_prefs.Complemento,v=>_prefs.Complemento=v);
        Campo("Bairro",_prefs.Bairro,v=>_prefs.Bairro=v);Campo("CEP",_prefs.Cep,v=>_prefs.Cep=v);Campo("Cidade",_prefs.Cidade,v=>_prefs.Cidade=v);Campo("UF",_prefs.Uf,v=>_prefs.Uf=v);
        _camposGeral=null;
        Subtitulo("Atendimento");var limite=Campo("Quantidade de comandas",_quantidade,v=>_quantidade=v);limite.Width=140;limite.HorizontalAlignment=HorizontalAlignment.Left;
        Texto("Deixe vazio para a quantidade automática. Comandas abertas são preservadas; taxa de serviço não é aplicada nesta versão.");
    }
    private void Cupom()
    {
        Texto("Configure o comprovante não fiscal de conferência. A impressão é manual, pelo diálogo de impressoras do Windows.");
        Marcar("Agrupar itens de mesmo produto, preço e observação",_prefs.AgruparItensCupom,v=>_prefs.AgruparItensCupom=v);
        Marcar("Exibir observações dos itens",_prefs.ExibirObservacoesCupom,v=>_prefs.ExibirObservacoesCupom=v);
        Marcar("Exibir documento do estabelecimento",_prefs.ExibirDocumentoCupom,v=>_prefs.ExibirDocumentoCupom=v);
        Marcar("Exibir endereço do estabelecimento",_prefs.ExibirEnderecoCupom,v=>_prefs.ExibirEnderecoCupom=v);
        Campo("Mensagem do cabeçalho",_prefs.CabecalhoCupom,v=>_prefs.CabecalhoCupom=v,true);Campo("Mensagem do rodapé",_prefs.RodapeCupom,v=>_prefs.RodapeCupom=v,true);
        Botao("Visualizar exemplo do cupom",()=>ComprovanteConta.VisualizarExemplo(this,_prefs,_nome));
        Texto("Impressão automática, impressão fiscal, etiquetas e integração com entregador não estão habilitadas no Varthex.");
    }
    private void Fundo()
    {
        _prefs.CorFundo=FundoSistema.Cor(_prefs);
        Subtitulo("Cor de fundo");
        var paleta=new WrapPanel { Margin=new Thickness(0,0,0,14) };_form.Children.Add(paleta);
        var previa=new Border { Height=250,Width=430,MaxWidth=430,BorderThickness=new Thickness(1),ClipToBounds=true,HorizontalAlignment=HorizontalAlignment.Left };previa.SetResourceReference(Border.BorderBrushProperty,"Line");
        void Atualizar()
        {
            previa.Background=(Brush)new BrushConverter().ConvertFromString(_prefs.CorFundo)!;
            previa.Child=new Border { Background=FundoSistema.CriarPincel(_prefs) };
        }
        foreach(var (nome,cor) in new[]{("Branco","#FFFFFF"),("Cinza 5%","#F2F2F2"),("Cinza 20%","#CCCCCC"),("Cinza 30%","#B3B3B3"),("Cinza 40%","#999999"),("Cinza 50%","#808080"),("Cinza 60%","#666666"),("Preto","#000000")})
        {
            var item=new StackPanel { Width=105,Margin=new Thickness(0,0,8,12) };var radio=new RadioButton { Content=nome,GroupName="CorFundo",IsChecked=string.Equals(cor,_prefs.CorFundo,StringComparison.OrdinalIgnoreCase),Margin=new Thickness(0,0,0,8) };radio.Checked+=(_,_)=>{_prefs.CorFundo=cor;Atualizar();};item.Children.Add(radio);item.Children.Add(new Border { Height=22,Background=(Brush)new BrushConverter().ConvertFromString(cor)!,BorderBrush=Brushes.Gray,BorderThickness=new Thickness(1) });paleta.Children.Add(item);
        }
        var outra=new StackPanel { Width=130 };var escolher=new RadioButton { Content="Outra",GroupName="CorFundo",IsChecked=!new[]{"#FFFFFF","#F2F2F2","#CCCCCC","#B3B3B3","#999999","#808080","#666666","#000000"}.Contains(_prefs.CorFundo,StringComparer.OrdinalIgnoreCase),Margin=new Thickness(0,0,0,8) };outra.Children.Add(escolher);var hex=new TextBox { Text=_prefs.CorFundo,MaxLength=7,ToolTip="Cor personalizada no formato #RRGGBB" };outra.Children.Add(hex);paleta.Children.Add(outra);
        escolher.Checked+=(_,_)=>{var cor=hex.Text.Trim();if(cor.Length==7&&cor[0]=='#'&&cor[1..].All(char.IsAsciiHexDigit)){_prefs.CorFundo=cor;Atualizar();}};
        hex.TextChanged+=(_,_)=>{var cor=hex.Text.Trim();if(cor.Length==7&&cor[0]=='#'&&cor[1..].All(char.IsAsciiHexDigit)){_prefs.CorFundo=cor;escolher.IsChecked=true;Atualizar();}};
        Texto("Dica: prefira uma imagem com fundo transparente e até 800 pixels de largura. O fundo é aplicado à área de trabalho vazia.");
        var visual=new WrapPanel();var quadro=new StackPanel { Margin=new Thickness(0,0,18,16) };quadro.Children.Add(new TextBlock { Text="Visualização",FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,8) });quadro.Children.Add(previa);visual.Children.Add(quadro);
        var posicoes=new StackPanel();posicoes.Children.Add(new TextBlock { Text="Posição:",Margin=new Thickness(0,0,0,12) });
        foreach(var posicao in new[]{"Centralizar","Preencher","Zoom","Repetir"}){var radio=new RadioButton { Content=posicao,GroupName="PosicaoFundo",IsChecked=_prefs.PosicaoImagem==posicao,Margin=new Thickness(0,0,0,10) };radio.Checked+=(_,_)=>{_prefs.PosicaoImagem=posicao;Atualizar();};posicoes.Children.Add(radio);}visual.Children.Add(posicoes);_form.Children.Add(visual);Atualizar();
        var barra=new StackPanel { Orientation=Orientation.Horizontal };_form.Children.Add(barra);
        void Acao(string titulo,Action executar){var botao=new Button { Content=titulo,Padding=new Thickness(12,5,12,5),Margin=new Thickness(0,0,12,0) };botao.Click+=(_,_)=>Executar(executar);barra.Children.Add(botao);}
        Acao("Selecionar imagem…",()=>
        {
            var arquivo=new OpenFileDialog { Filter="Imagens|*.png;*.jpg;*.jpeg;*.bmp",Title="Imagem de fundo" };if(arquivo.ShowDialog(this)!=true)return;
            if(new FileInfo(arquivo.FileName).Length>3*1024*1024)throw new ArgumentException("Escolha uma imagem de até 3 MB.");
            var dados=Convert.ToBase64String(File.ReadAllBytes(arquivo.FileName));var bitmap=CriarImagem(dados)??throw new ArgumentException("Imagem inválida.");if(bitmap.PixelWidth>8000||bitmap.PixelHeight>8000)throw new ArgumentException("A imagem deve ter até 8000 pixels por dimensão.");_prefs.ImagemFundo=dados;Atualizar();
        });
        Acao("Restaurar padrão",()=>{_prefs.ImagemFundo="";_prefs.CorFundo=PreferenciasSistema.CorFundoPadrao;_prefs.PosicaoImagem="Centralizar";Mostrar("Imagem de fundo");});
    }
    public static BitmapImage? CriarImagem(string dados)
    {
        if(string.IsNullOrWhiteSpace(dados))return null;
        using var stream=new MemoryStream(Convert.FromBase64String(dados));var bitmap=new BitmapImage();bitmap.BeginInit();bitmap.CacheOption=BitmapCacheOption.OnLoad;bitmap.StreamSource=stream;bitmap.EndInit();bitmap.Freeze();return bitmap;
    }
    private void Origens()
    {
        var barra=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,0,0,12) };_form.Children.Add(barra);
        var lista=new ListView { ItemsSource=_prefs.OrigensCliente,MinHeight=260,SelectionMode=SelectionMode.Single };var vista=new GridView();var coluna=new GridViewColumn { Header="Origens",Width=650,DisplayMemberBinding=new System.Windows.Data.Binding(".") };vista.Columns.Add(coluna);lista.View=vista;lista.SizeChanged+=(_,_)=>coluna.Width=Math.Max(100,lista.ActualWidth-30);
        var estilo=new Style(typeof(ListViewItem),System.Windows.Application.Current.TryFindResource(typeof(ListViewItem)) as Style);estilo.Setters.Add(new Setter(Control.PaddingProperty,new Thickness(6,4,6,4)));estilo.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty,HorizontalAlignment.Stretch));estilo.Setters.Add(new Setter(Control.BorderThicknessProperty,new Thickness(0,0,0,1)));estilo.Setters.Add(new Setter(Control.BorderBrushProperty,new DynamicResourceExtension("Line")));lista.ItemContainerStyle=estilo;
        void Editar(int indice)
        {
            var janela=new Window { Title=indice<0?"Nova origem de cliente":"Editar origem de cliente",Owner=this,Width=460,Height=220,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false };janela.SetResourceReference(BackgroundProperty,"Canvas");
            var painel=new StackPanel { Margin=new Thickness(20) };painel.Children.Add(new TextBlock { Text="Nome da origem",Margin=new Thickness(0,0,0,8) });var nome=new TextBox { Text=indice<0?"":_prefs.OrigensCliente[indice],MaxLength=80 };painel.Children.Add(nome);
            var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,18,0,0) };var cancelar=new Button { Content="Cancelar",Padding=new Thickness(12,5,12,5),Margin=new Thickness(0,0,8,0),IsCancel=true };var salvar=new Button { Content="Salvar",Padding=new Thickness(12,5,12,5),IsDefault=true };acoes.Children.Add(cancelar);acoes.Children.Add(salvar);painel.Children.Add(acoes);janela.Content=painel;
            salvar.Click+=(_,_)=>Executar(()=>{var valor=nome.Text.Trim();if(valor.Length==0)throw new ArgumentException("Informe o nome da origem.");if(_prefs.OrigensCliente.Where((_,i)=>i!=indice).Contains(valor,StringComparer.OrdinalIgnoreCase))throw new ArgumentException("Já existe uma origem com esse nome.");if(indice<0)_prefs.OrigensCliente.Add(valor);else _prefs.OrigensCliente[indice]=valor;janela.DialogResult=true;});janela.Loaded+=(_,_)=>{nome.Focus();nome.SelectAll();};if(MostrarJanelaCompacta(janela)==true)Mostrar("Origem do Cliente");
        }
        foreach(var texto in new[]{"Editar","Novo"}){var botao=new Button { Content=texto,Padding=new Thickness(12,5,12,5),Margin=new Thickness(8,0,0,0) };botao.Click+=(_,_)=>Executar(()=>{if(texto=="Editar"&&lista.SelectedIndex<0)throw new ArgumentException("Selecione uma origem.");Editar(texto=="Novo"?-1:lista.SelectedIndex);});barra.Children.Add(botao);}
        lista.MouseDoubleClick+=(_,_)=>{if(lista.SelectedIndex>=0)Editar(lista.SelectedIndex);};_form.Children.Add(lista);
    }
    private void Cozinhas()
    {
        var barra=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,0,0,12) };_form.Children.Add(barra);
        var lista=new ListView { ItemsSource=_prefs.Cozinhas,MinHeight=340,SelectionMode=SelectionMode.Single,HorizontalContentAlignment=HorizontalAlignment.Stretch };
        var colunas=new GridView();colunas.Columns.Add(new GridViewColumn { Header="Nome do local / cozinha",Width=360,DisplayMemberBinding=new System.Windows.Data.Binding(nameof(LocalCozinha.Nome)) });colunas.Columns.Add(new GridViewColumn { Header="Impressora utilizada",Width=310,DisplayMemberBinding=new System.Windows.Data.Binding(nameof(LocalCozinha.SaidaDescricao)) });lista.View=colunas;
        void Acao(string texto,Action executar){var botao=new Button { Content=texto,Padding=new Thickness(12,5,12,5),Margin=new Thickness(8,0,0,0) };botao.Click+=(_,_)=>Executar(executar);barra.Children.Add(botao);}
        LocalCozinha Selecionado()=>lista.SelectedItem as LocalCozinha??throw new ArgumentException("Selecione um local / cozinha.");
        Acao("Produtos vinculados",()=>VincularProdutos(Selecionado()));Acao("Editar",()=>EditarCozinha(Selecionado()));Acao("Novo",()=>EditarCozinha(null));lista.MouseDoubleClick+=(_,_)=>{if(lista.SelectedItem is LocalCozinha local)EditarCozinha(local);};
        _form.Children.Add(lista);Texto("A coluna indica a saída configurada de cada local. O envio automático dos pedidos às impressoras ainda não está disponível. Produtos sem vínculo aparecem em todos os locais no monitor.");
    }
    private void EditarCozinha(LocalCozinha? local)
    {
        var janela=new Window { Title="Local / Cozinha",Owner=this,Width=620,Height=450,MinWidth=500,MinHeight=400,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false };
        janela.SetResourceReference(BackgroundProperty,"Canvas");var raiz=new DockPanel { Margin=new Thickness(18) };janela.Content=raiz;
        var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };DockPanel.SetDock(acoes,Dock.Bottom);raiz.Children.Add(acoes);
        var corpo=new StackPanel();raiz.Children.Add(corpo);
        corpo.Children.Add(new TextBlock { Text="Descrição:",Margin=new Thickness(0,0,0,3) });var nome=new TextBox { Text=local?.Nome??"",MaxLength=80,Margin=new Thickness(0,0,0,10) };corpo.Children.Add(nome);
        corpo.Children.Add(new TextBlock { Text="Exemplo: cozinha de lanches, cozinha de sucos…",Margin=new Thickness(0,0,0,20) });
        corpo.Children.Add(new TextBlock { Text="Saída:",FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,12) });
        var monitor=new RadioButton { Content=new TextBlock { Text="Somente monitor — exibir os itens no monitor de preparo.",TextWrapping=TextWrapping.Wrap },GroupName="SaidaCozinha",IsChecked=local?.Imprimir!=true,Margin=new Thickness(0,0,0,10) };corpo.Children.Add(monitor);
        var imprimir=new RadioButton { Content=new TextBlock { Text="Monitor e impressora — configurar também uma saída de impressão.",TextWrapping=TextWrapping.Wrap },GroupName="SaidaCozinha",IsChecked=local?.Imprimir==true,Margin=new Thickness(0,0,0,10) };corpo.Children.Add(imprimir);
        var painelImpressora=new StackPanel { Margin=new Thickness(24,0,0,0),IsEnabled=imprimir.IsChecked==true };corpo.Children.Add(painelImpressora);
        var impressora=new TextBox { Text=local?.Impressora??"Mesma do caixa",IsReadOnly=true,Margin=new Thickness(0,0,0,8) };painelImpressora.Children.Add(impressora);
        var configurar=new Button { Content="Configurar impressora…",HorizontalAlignment=HorizontalAlignment.Right,Padding=new Thickness(12,5,12,5) };painelImpressora.Children.Add(configurar);
        configurar.Click+=(_,_)=>Executar(()=>{var dialogo=new PrintDialog();if(dialogo.ShowDialog()==true)impressora.Text=dialogo.PrintQueue.FullName;});
        imprimir.Checked+=(_,_)=>painelImpressora.IsEnabled=true;monitor.Checked+=(_,_)=>painelImpressora.IsEnabled=false;
        void BotaoJanela(string texto,Action acao){var b=new Button { Content=texto,Margin=new Thickness(8,12,0,0),Padding=new Thickness(12,5,12,5) };b.Click+=(_,_)=>Executar(acao);acoes.Children.Add(b);}
        if(local is not null)BotaoJanela("Excluir",()=>{if(_prefs.Cozinhas.Count==1)throw new ArgumentException("Mantenha pelo menos um local / cozinha.");_prefs.Cozinhas.Remove(local);janela.DialogResult=true;});
        BotaoJanela("Voltar",()=>janela.DialogResult=false);
        BotaoJanela("Salvar",()=>{var descricao=nome.Text.Trim();if(descricao.Length==0)throw new ArgumentException("Informe a descrição do local.");if(_prefs.Cozinhas.Any(c=>c!=local&&string.Equals(c.Nome,descricao,StringComparison.OrdinalIgnoreCase)))throw new ArgumentException("Já existe um local com essa descrição.");var destino=local??new LocalCozinha();destino.Nome=descricao;destino.Imprimir=imprimir.IsChecked==true;destino.Impressora=impressora.Text;if(local is null)_prefs.Cozinhas.Add(destino);janela.DialogResult=true;});
        janela.Loaded+=(_,_)=>{nome.Focus();nome.SelectAll();};if(MostrarJanelaCompacta(janela)==true)Mostrar("Locais / cozinhas");
    }
    private void VincularProdutos(LocalCozinha local)
    {
        var janela=new Window { Title="Produtos · "+local.Nome,Owner=this,WindowStartupLocation=WindowStartupLocation.CenterOwner,Width=500,Height=560,MinWidth=420,MinHeight=400,ShowInTaskbar=false };
        var raiz=new DockPanel { Margin=new Thickness(16) };var salvar=new Button { Content="Aplicar vínculos" };DockPanel.SetDock(salvar,Dock.Bottom);raiz.Children.Add(salvar);
        var painel=new StackPanel();var checks=new List<(int Id,CheckBox Check)>();foreach(var produto in _produtos()){var check=new CheckBox { Content=produto.Nome,IsChecked=local.Produtos.Contains(produto.Id),MinHeight=28 };painel.Children.Add(check);checks.Add((produto.Id,check));}
        raiz.Children.Add(new ScrollViewer { Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto });janela.Content=raiz;
        salvar.Click+=(_,_)=>{local.Produtos=checks.Where(x=>x.Check.IsChecked==true).Select(x=>x.Id).ToList();janela.DialogResult=true;};MostrarJanelaCompacta(janela);
    }
    private void Codigo()
    {
        var grade=new Grid();grade.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(360) });grade.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(1,GridUnitType.Star) });
        var esquerda=new StackPanel { Margin=new Thickness(0,0,22,0) };var direita=new StackPanel { Margin=new Thickness(24,0,0,0) };
        var divisoria=new Border { BorderThickness=new Thickness(1,0,0,0),Child=direita };divisoria.SetResourceReference(Border.BorderBrushProperty,"Line");Grid.SetColumn(divisoria,1);grade.Children.Add(esquerda);grade.Children.Add(divisoria);_form.Children.Add(grade);
        grade.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });grade.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });
        grade.SizeChanged+=(_,_)=>{bool estreito=grade.ActualWidth<720;grade.ColumnDefinitions[0].Width=estreito?new GridLength(1,GridUnitType.Star):new GridLength(360);grade.ColumnDefinitions[1].Width=estreito?new GridLength(0):new GridLength(1,GridUnitType.Star);Grid.SetColumn(divisoria,estreito?0:1);Grid.SetRow(divisoria,estreito?1:0);divisoria.BorderThickness=estreito?new Thickness(0,1,0,0):new Thickness(1,0,0,0);direita.Margin=estreito?new Thickness(0,24,0,0):new Thickness(24,0,0,0);};
        TextBlock TextoLocal(Panel painel,string texto,bool titulo=false)
        {
            var t=new TextBlock { Text=texto,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,10),FontSize=12,FontWeight=titulo?FontWeights.SemiBold:FontWeights.Normal };painel.Children.Add(t);return t;
        }
        var ativar=new CheckBox { Content=new TextBlock { Text="Ativar código de barras de produto por quilo",TextWrapping=TextWrapping.Wrap,MaxWidth=310 },IsChecked=_prefs.CodigoPesoAtivo,Margin=new Thickness(0,0,0,14) };esquerda.Children.Add(ativar);
        var controles=new StackPanel();esquerda.Children.Add(controles);controles.IsEnabled=_prefs.CodigoPesoAtivo;
        ativar.Click+=(_,_)=>{_prefs.CodigoPesoAtivo=ativar.IsChecked==true;controles.IsEnabled=_prefs.CodigoPesoAtivo;};
        TextoLocal(controles,"Composição do Código de Barras — 13 caracteres:").TextAlignment=TextAlignment.Center;
        var desenho=new Grid { Width=250,Height=60,Background=Brushes.White,Margin=new Thickness(0,0,0,8) };var barras=new Canvas();desenho.Children.Add(barras);
        // Exemplo esquemático: a linha destaca a leitura da etiqueta.
        double x=20;var modulos="101001100101100110100010100110110011101010101110010110011011001001000101110010101";
        foreach(char modulo in modulos){if(modulo=='1'){var barra=new System.Windows.Shapes.Rectangle { Width=2.6,Height=54,Fill=Brushes.Black };Canvas.SetLeft(barra,x);barras.Children.Add(barra);}x+=2.6;}
        desenho.Children.Add(new Border { Height=5,Background=Brushes.Red,VerticalAlignment=VerticalAlignment.Center });controles.Children.Add(desenho);
        var composicao=TextoLocal(controles,"");composicao.FontFamily=new FontFamily("Consolas");composicao.FontSize=20;composicao.FontWeight=FontWeights.Bold;composicao.TextAlignment=TextAlignment.Center;
        void AtualizarComposicao()=>composicao.Text=$"{_prefs.PrefixoCodigoPeso} {new string('0',_prefs.DigitosProdutoPeso)} {new string('0',11-_prefs.DigitosProdutoPeso)} 0";
        AtualizarComposicao();TextoLocal(controles,"Dígito inicial | Cód. do produto | Total a pagar | Dígito final").TextAlignment=TextAlignment.Center;
        var campos=new Grid();campos.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(100) });campos.ColumnDefinitions.Add(new ColumnDefinition());controles.Children.Add(campos);
        var prefixoPainel=new StackPanel();campos.Children.Add(prefixoPainel);TextoLocal(prefixoPainel,"Dígito inicial:");
        var prefixo=new ComboBox { ItemsSource=Enumerable.Range(0,10).Select(n=>n.ToString()).ToArray(),SelectedItem=_prefs.PrefixoCodigoPeso,Width=70,HorizontalAlignment=HorizontalAlignment.Left };prefixo.SelectionChanged+=(_,_)=>{_prefs.PrefixoCodigoPeso=(string)prefixo.SelectedItem;AtualizarComposicao();};prefixoPainel.Children.Add(prefixo);
        var quantidade=new StackPanel();Grid.SetColumn(quantidade,1);campos.Children.Add(quantidade);TextoLocal(quantidade,"Caracteres do código do produto:");var escolhas=new StackPanel { Orientation=Orientation.Horizontal };quantidade.Children.Add(escolhas);
        foreach(var n in new[]{4,5,6}){var radio=new RadioButton { Content=n.ToString(),GroupName="DigitosEtiqueta",IsChecked=n==_prefs.DigitosProdutoPeso,Margin=new Thickness(0,0,20,0) };radio.Checked+=(_,_)=>{_prefs.DigitosProdutoPeso=n;AtualizarComposicao();};escolhas.Children.Add(radio);}
        TextoLocal(direita,"O que é isto?",true);
        TextoLocal(direita,"Sua balança pode imprimir etiquetas que contêm o código do produto e o total a pagar. Exemplo:");
        var etiqueta=new StackPanel { Margin=new Thickness(12) };var exemplo=new Border { Background=Brushes.White,BorderBrush=Brushes.LightGray,BorderThickness=new Thickness(1),HorizontalAlignment=HorizontalAlignment.Center,Width=240,Margin=new Thickness(0,0,0,20),Child=etiqueta };
        var nome=TextoLocal(etiqueta,"BANANA NANICA",true);nome.Foreground=Brushes.Black;nome.TextAlignment=TextAlignment.Center;nome.Margin=new Thickness(0,0,0,5);
        var dados=TextoLocal(etiqueta,"PESO: 0,410 kg     PREÇO/kg: R$ 4,98\nTOTAL A PAGAR: R$ 2,04",true);dados.Foreground=Brushes.Black;dados.Margin=new Thickness(0);direita.Children.Add(exemplo);
        TextoLocal(direita,"A etiqueta é interpretada pelo valor total em centavos, conforme o prefixo e a quantidade de dígitos configurados à esquerda.");
        TextoLocal(direita,"Como cadastrar os produtos na balança?",true);
        TextoLocal(direita,"Use o programa ou a interface da sua balança. O código cadastrado nela deve ser o mesmo código do produto no Varthex. Consulte o suporte do fabricante para configurar preços, validade e formato de etiqueta.");
        TextoLocal(direita,"No Varthex, esta configuração permite conferir etiquetas. O lançamento automático por peso e a exportação no formato Toledo ainda não estão disponíveis.");
        TextoLocal(direita,"Conferir uma etiqueta EAN-13",true);
        var codigo=new TextBox { MaxLength=13,Margin=new Thickness(0,0,0,10) };direita.Children.Add(codigo);var resultado=TextoLocal(direita,"");
        var conferir=new Button { Content="Conferir etiqueta",HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(12,5,12,5) };conferir.Click+=(_,_)=>Executar(()=>{var valor=ConfiguracoesSistemaService.InterpretarEtiqueta(codigo.Text.Trim(),_prefs);resultado.Text=$"Produto: {valor.Produto} · Total: {Atendimento.CentavosParaMoedaConverter.Formatar(valor.ValorCentavos)}";});direita.Children.Add(conferir);
    }
    private void Sobre()
    {
        Texto("Informações de registro e ambiente do Varthex. O relatório não inclui chaves de ativação, senhas ou tokens.");
        var relatorio=new TextBox { Text=_diagnostico(),IsReadOnly=true,AcceptsReturn=true,TextWrapping=TextWrapping.NoWrap,FontFamily=new FontFamily("Consolas"),FontSize=12,MinHeight=350,VerticalContentAlignment=VerticalAlignment.Top,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto };
        _form.Children.Add(relatorio);
        Botao("Atualizar informações",()=>relatorio.Text=_diagnostico());
        Botao("Copiar diagnóstico",()=>Clipboard.SetText(relatorio.Text));
        Botao("Salvar diagnóstico",()=>{var arquivo=new SaveFileDialog { Filter="Texto|*.txt",FileName="Varthex-diagnostico.txt" };if(arquivo.ShowDialog(this)==true)File.WriteAllText(arquivo.FileName,relatorio.Text,Encoding.UTF8);});
    }
    private void Avancadas(Button alvo)
    {
        var menu=new ContextMenu { PlacementTarget=alvo };
        foreach(var (nome,acao) in new[]{("Criar backup agora","criar-backup"),("Backups e restauração","backups"),("Consultar ativação","licenca"),("Alterar PIN da equipe","pin")})
        {
            var b=new MenuItem { Header=nome,FontSize=12,Padding=new Thickness(10) };b.Click+=(_,_)=>Executar(()=>{if(TemAlteracoes)throw new InvalidOperationException("Salve as alterações antes de abrir esta opção.");_acao(acao);});menu.Items.Add(b);
        }
        menu.IsOpen=true;
    }
    private void Salvar()=>Executar(()=>
    {
        int? quantidade=null;if(!string.IsNullOrWhiteSpace(_quantidade)){if(!int.TryParse(_quantidade,out var n))throw new ArgumentException("Informe uma quantidade válida de comandas.");quantidade=n;}
        if(quantidade is int limite&&_maiorComanda()>(long)_inicioComandas+limite-1)throw new ArgumentException("Há comandas abertas acima desse limite. Encerre ou transfira antes de reduzir.");
        _service.Salvar(_prefs,_nome,quantidade,DateTime.UtcNow);_snapshot=Estado();_mensagem.Text="Configurações salvas.";_salvo();
    });
}
