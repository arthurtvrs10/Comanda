using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using VarthexComanda.Desktop;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

public sealed class JanelaConfiguracao:Window
{
    private static readonly string[] Etapas=["Estabelecimento","Comanda impressa","Atendimento","Produtos","Pagamentos","Equipe","Revisão"];
    private readonly string _arquivo=Path.Combine(AppContext.BaseDirectory,"rascunho-configuracao.json");
    private EstadoConfiguracao _estado=new();
    private readonly Grid _raiz=new();
    private readonly StackPanel _navegacao=new();
    private readonly ScrollViewer _visaoEtapas=new(){VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
    private bool _emEdicao;
    private readonly ProgressBar _progresso=new(){Height=6,Minimum=0,Maximum=100,Margin=new Thickness(0,7,0,0)};
    private readonly TextBlock _percentual=new(){HorizontalAlignment=HorizontalAlignment.Right};
    private readonly TextBlock _titulo=new(){FontSize=24,FontWeight=FontWeights.SemiBold};
    private readonly TextBlock _descricao=new(){TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,6,0,20)};
    private readonly TextBlock _erro=new(){TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12),Visibility=Visibility.Collapsed};
    private readonly TextBlock _situacao=new(){VerticalAlignment=VerticalAlignment.Center,FontSize=11};
    private readonly ContentControl _conteudo=new();
    private readonly Button _voltar=new(){Content="Voltar",MinWidth=88};
    private readonly Button _adiar=new(){Content="Configurar depois",Margin=new Thickness(0,0,10,0)};
    private readonly Button _continuar=new(){Content="Continuar",MinWidth=120};
    private FlowDocumentScrollViewer? _cupom;
    private bool _falhar;
    private int _paginaMesa;
    private bool _renderizando;
    private bool _somenteVerificacao;
    private readonly PrimeiraConfiguracaoService? _servico;
    private readonly Action<Window>? _abrirColaboradores;
    private readonly Func<IReadOnlyList<Colaborador>>? _listarColaboradores;

    public JanelaConfiguracao(PrimeiraConfiguracaoService? servico=null,Action<Window>? abrirColaboradores=null,Func<IReadOnlyList<Colaborador>>? listarColaboradores=null)
    {
        _servico=servico;_abrirColaboradores=abrirColaboradores;_listarColaboradores=listarColaboradores;
        _visaoEtapas.Content=_navegacao;
        if(servico is not null)_estado=servico.Carregar();
        else try{if(File.Exists(_arquivo))_estado=JsonSerializer.Deserialize<EstadoConfiguracao>(File.ReadAllText(_arquivo))??new();}catch{_estado=new();}
        _estado.Etapa=Math.Min(Math.Clamp(_estado.Etapa,0,6),_estado.PrimeiraPendente);
        Title="Primeira configuração · Comanda · Protótipo";Width=1180;Height=Math.Min(800,SystemParameters.WorkArea.Height-24);MinWidth=900;MinHeight=620;WindowStartupLocation=WindowStartupLocation.CenterScreen;FontFamily=new FontFamily("Segoe UI");FontSize=12;
        if(servico is not null)Title="Primeiros passos · Comanda";
        SetResourceReference(BackgroundProperty,"Canvas");SetResourceReference(ForegroundProperty,"Ink");_raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");_progresso.SetResourceReference(Control.ForegroundProperty,"Ink");
        _raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});_raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});_raiz.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});_raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});Content=_raiz;
        var topo=new DockPanel { Margin=new Thickness(24,18,24,14) };
        topo.Children.Add(new TextBlock { Text="Primeira configuração",FontSize=20,FontWeight=FontWeights.SemiBold });_raiz.Children.Add(topo);
        var etapas=new StackPanel { Margin=new Thickness(24,0,24,16) };
        var legenda=new Grid { Margin=new Thickness(0,10,0,0) };legenda.Children.Add(new TextBlock { Text="Progresso da configuração",FontSize=11 });legenda.Children.Add(_percentual);etapas.Children.Add(legenda);etapas.Children.Add(_progresso);AutomationProperties.SetName(_progresso,"Progresso da configuração");Grid.SetRow(etapas,1);_raiz.Children.Add(etapas);
        var corpo=new Grid { Margin=new Thickness(24,0,24,12) };corpo.RowDefinitions.Add(new(){Height=GridLength.Auto});corpo.RowDefinitions.Add(new(){Height=GridLength.Auto});corpo.RowDefinitions.Add(new(){Height=GridLength.Auto});corpo.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});corpo.Children.Add(_titulo);Grid.SetRow(_descricao,1);corpo.Children.Add(_descricao);Grid.SetRow(_erro,2);_erro.SetResourceReference(TextBlock.ForegroundProperty,"TextA42B25");corpo.Children.Add(_erro);Grid.SetRow(_conteudo,3);corpo.Children.Add(_conteudo);Grid.SetRow(corpo,2);_raiz.Children.Add(corpo);
        var rodape=new DockPanel { Margin=new Thickness(24,12,24,16) };var botoes=new StackPanel { Orientation=Orientation.Horizontal };botoes.Children.Add(_voltar);_voltar.Margin=new Thickness(0,0,10,0);botoes.Children.Add(_adiar);botoes.Children.Add(_continuar);DockPanel.SetDock(botoes,Dock.Right);rodape.Children.Add(botoes);rodape.Children.Add(_situacao);Grid.SetRow(rodape,3);_raiz.Children.Add(rodape);
        _continuar.SetResourceReference(StyleProperty,"BotaoPrimario");
        _voltar.Click+=(_,_)=>{_emEdicao=false;Mostrar();};_adiar.Click+=(_,_)=>Navegar(_estado.Etapa+1,true);_continuar.Click+=(_,_)=>{if(_estado.Finalizado){if(_servico is not null){DialogResult=true;return;}_estado.Finalizado=false;_emEdicao=false;Navegar(0,false);return;}if(_estado.Etapa<6)Navegar(_estado.Etapa+1,true);else Concluir();};
        Loaded+=(_,_)=>Mostrar();
    }
    private void Salvar()
    {
        if(_renderizando||_somenteVerificacao)return;
        try{if(_servico is not null){_servico.SalvarRascunho(_estado);_situacao.Text="Rascunho salvo · As configurações serão aplicadas ao concluir";}else{File.WriteAllText(_arquivo,_estado.Json());_situacao.Text="Protótipo · Rascunho separado dos dados reais";}}catch{_situacao.Text="Não foi possível salvar o rascunho. Confira a pasta de dados.";}
        AtualizarProgresso();
    }
    private void AtualizarProgresso(){_progresso.Value=_estado.Progresso;_percentual.Text=$"{(_estado.Finalizado?7:_estado.Concluidas.Count(i=>i<6&&_estado.Erro(i)==""))} de 7 etapas concluídas";var indice=0;foreach(var botao in _navegacao.Children.OfType<Button>()){var liberar=_estado.PodeAcessar(indice++);botao.IsEnabled=liberar;botao.Opacity=liberar?1:0.62;}}
    private void Alterado(){if(_renderizando)return;_estado.Finalizado=false;_estado.Concluidas.RemoveWhere(i=>i>=_estado.Etapa);Salvar();}
    private void Navegar(int etapa,bool validar)
    {
        if(validar&&_estado.Erro(_estado.Etapa) is {Length:>0} erro){Erro(erro);return;}
        if(validar)_estado.Concluidas.Add(_estado.Etapa);
        if(!_estado.PodeAcessar(etapa))return;
        _estado.Etapa=Math.Clamp(etapa,0,6);_estado.Finalizado=false;_emEdicao=false;Mostrar();Salvar();
    }
    private void Erro(string texto){_erro.Text=texto;_erro.Visibility=Visibility.Visible;}
    private void Concluir()
    {
        for(var i=0;i<6;i++)if(_estado.Erro(i) is {Length:>0} erro){_estado.Etapa=i;Mostrar();Erro(erro);return;}
        if(_falhar){Erro("Não foi possível concluir a simulação. Os dados foram preservados. Desmarque a opção de falha e tente novamente.");return;}
        if(_servico is not null){try{_servico.Concluir(_estado);_estado.Finalizado=true;_erro.Visibility=Visibility.Collapsed;_titulo.Text="Configuração concluída";_descricao.Text="Seu estabelecimento está pronto para começar. Você pode ajustar os cadastros pelas Configurações.";_conteudo.Content=new TextBlock {Text="Cadastre os demais produtos, confira sua impressora e inicie o primeiro atendimento.",TextWrapping=TextWrapping.Wrap};_continuar.Content="Abrir o sistema";_voltar.Visibility=Visibility.Collapsed;_adiar.Visibility=Visibility.Collapsed;AtualizarProgresso();_situacao.Text="Configuração salva";}catch(Exception ex){Erro(ex.Message);}return;}
        _estado.Finalizado=true;Mostrar();Salvar();
    }
    public void CarregarExemplo(int etapa){_estado=EstadoConfiguracao.Exemplo();_estado.Etapa=etapa;for(var i=0;i<etapa;i++)_estado.Concluidas.Add(i);Mostrar();}
    private void Mostrar()
    {
        _renderizando=true;_cupom=null;_erro.Visibility=Visibility.Collapsed;_navegacao.Children.Clear();
        string[] descricoes=["Informe os dados do seu estabelecimento. Você poderá atualizá-los depois.","Escolha o conteúdo e confira a prévia real de impressão do VComanda.","Defina a quantidade inicial de comandas e o atendimento em mesas.","Adicione os primeiros produtos ou configure o catálogo depois.","Selecione as formas de pagamento utilizadas no estabelecimento.","Informe o responsável e adicione colaboradores, se desejar.","Revise as escolhas antes de concluir."];
        _titulo.Text=Etapas[_estado.Etapa];_descricao.Text=descricoes[_estado.Etapa];_descricao.SetResourceReference(TextBlock.ForegroundProperty,"AjudaMuted");
        _voltar.Visibility=_emEdicao&&!_estado.Finalizado?Visibility.Visible:Visibility.Collapsed;_voltar.Content="Voltar aos passos";_adiar.Visibility=_emEdicao&&_estado.Etapa is 3 or 5&&!_estado.Finalizado?Visibility.Visible:Visibility.Collapsed;_continuar.Visibility=_emEdicao||_estado.Finalizado?Visibility.Visible:Visibility.Collapsed;_continuar.Content=_estado.Finalizado?"Revisar simulação":_estado.Etapa==6?(_servico is null?"Concluir simulação":"Salvar e começar"):"Continuar";
        if(_estado.Finalizado){var fim=Painel();Titulo(fim,"Configuração simulada concluída");Texto(fim,"O exemplo está preparado para validação. Nenhum cadastro real, senha ou impressão foi alterado.");Texto(fim,"Próximos passos no aplicativo: completar o catálogo, configurar a impressora, habilitar o servidor se necessário e validar um primeiro atendimento.");var exportar=Botao("Salvar exemplo em arquivo",()=>{var arquivo=Path.Combine(AppContext.BaseDirectory,"configuracao-exemplo.json");File.WriteAllText(arquivo,_estado.Json());_situacao.Text="Exemplo salvo na pasta do protótipo.";});fim.Children.Add(exportar);_conteudo.Content=fim;}
        else if(!_emEdicao){_titulo.Text="Primeiros passos";_descricao.Text="Siga a sequência para preparar seu estabelecimento e começar a atender.";_conteudo.Content=ListaEtapas();}
        else _conteudo.Content=_estado.Etapa switch {0=>Estabelecimento(),1=>ComandaImpressa(),2=>Atendimento(),3=>Produtos(),4=>Pagamentos(),5=>Equipe(),_=>Revisao()};
        AtualizarProgresso();_renderizando=false;_situacao.Text=_servico is null?"Protótipo · Dados de avaliação":"As alterações serão aplicadas ao concluir";
    }
    private FrameworkElement ListaEtapas()
    {
        _navegacao.Children.Clear();
        for(var i=0;i<Etapas.Length;i++)
        {
            var indice=i;var concluida=_estado.Concluidas.Contains(i)&&_estado.Erro(i)=="";var atual=i==_estado.PrimeiraPendente;var liberada=_estado.PodeAcessar(i);
            var sucessoFundo=new SolidColorBrush((Color)ColorConverter.ConvertFromString(TemaInterface.Escuro?"#203D30":"#EAF6EF"));
            var sucessoTexto=new SolidColorBrush((Color)ColorConverter.ConvertFromString(TemaInterface.Escuro?"#9BD7B0":"#245640"));
            var linha=new Grid();linha.ColumnDefinitions.Add(new(){Width=new GridLength(45)});linha.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});linha.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
            var numero=new Border {Width=30,Height=30,CornerRadius=new CornerRadius(15),HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Center,BorderThickness=new Thickness(1)};
            numero.SetResourceReference(Border.BorderBrushProperty,"Line");
            if(concluida)
            {
                numero.Background=sucessoTexto;numero.BorderThickness=new Thickness(0);
                numero.Child=new System.Windows.Shapes.Path {Data=Geometry.Parse("M2,8 L6,12 L14,3"),Stroke=sucessoFundo,StrokeThickness=2,Width=16,Height=16,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
            }
            else
            {
                if(atual)numero.SetResourceReference(Border.BackgroundProperty,"Ink");
                var textoNumero=new TextBlock {Text=(i+1).ToString(),FontWeight=FontWeights.SemiBold,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};textoNumero.SetResourceReference(TextBlock.ForegroundProperty,atual?"Surface":"Ink");numero.Child=textoNumero;
            }
            linha.Children.Add(numero);
            var nome=new TextBlock {Text=Etapas[i],FontSize=14,FontWeight=atual?FontWeights.SemiBold:FontWeights.Normal,VerticalAlignment=VerticalAlignment.Center};if(concluida)nome.Foreground=sucessoTexto;Grid.SetColumn(nome,1);linha.Children.Add(nome);
            var situacao=new TextBlock {Text=concluida?"Concluído · Revisar":atual?"Fazer agora":"Próxima etapa",FontSize=12,FontWeight=atual?FontWeights.SemiBold:FontWeights.Normal,Margin=new Thickness(18,0,4,0),VerticalAlignment=VerticalAlignment.Center};if(concluida)situacao.Foreground=sucessoTexto;else situacao.SetResourceReference(TextBlock.ForegroundProperty,atual?"Ink":"AjudaMuted");Grid.SetColumn(situacao,2);linha.Children.Add(situacao);
            var botao=new Button {Content=linha,HorizontalContentAlignment=HorizontalAlignment.Stretch,Padding=new Thickness(16,12,16,12),MinHeight=62,Margin=new Thickness(0,0,0,10),IsEnabled=liberada,Opacity=liberada?1:0.62,BorderThickness=new Thickness(atual?2:1)};
            if(concluida)botao.Background=sucessoFundo;else botao.SetResourceReference(Control.BackgroundProperty,"Surface");botao.SetResourceReference(Control.BorderBrushProperty,atual?"Ink":"Line");
            AutomationProperties.SetName(botao,$"Etapa {i+1}: {Etapas[i]}. {(concluida?"Concluída. Revisar":atual?"Fazer agora":"Próxima etapa, bloqueada")}");
            botao.Click+=(_,_)=>{if(!_estado.PodeAcessar(indice))return;_estado.Etapa=indice;_emEdicao=true;Mostrar();Salvar();};_navegacao.Children.Add(botao);
        }
        return _visaoEtapas;
    }
    private static StackPanel Painel()=>new(){Margin=new Thickness(0,0,12,0)};
    private static ScrollViewer Rolagem(UIElement filho)=>new(){Content=filho,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
    private static void Texto(Panel p,string texto){var t=new TextBlock { Text=texto,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12) };t.SetResourceReference(TextBlock.ForegroundProperty,"Ink");p.Children.Add(t);}
    private static void Titulo(Panel p,string texto){p.Children.Add(new TextBlock { Text=texto,FontSize=16,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,4,0,14) });}
    private Button Botao(string texto,Action acao){var b=new Button {Content=texto,HorizontalAlignment=HorizontalAlignment.Left,MinHeight=34,Padding=new Thickness(12,6,12,6),Margin=new Thickness(0,8,0,8)};b.Click+=(_,_)=>acao();return b;}
    private TextBox Campo(Panel p,string titulo,string valor,Action<string> alterar,bool obrigatorio=false,bool multilinha=false,Func<string,string>? mascara=null)
    {
        var bloco=new StackPanel { Margin=new Thickness(0,0,18,14) };var label=new RotuloAjuda {Texto=titulo+(obrigatorio?" *":""),Margin=new Thickness(0,0,0,6)};bloco.Children.Add(label);
        var caixa=new TextBox {Text=valor,MinHeight=34,Padding=new Thickness(8,6,8,6),AcceptsReturn=multilinha,TextWrapping=multilinha?TextWrapping.Wrap:TextWrapping.NoWrap,MaxLength=multilinha?300:120};if(multilinha)caixa.MinHeight=60;AutomationProperties.SetName(caixa,titulo);AutomationProperties.SetIsRequiredForForm(caixa,obrigatorio);VincularEdicao(caixa,alterar,mascara??(titulo=="CEP"?JanelaConfiguracoesSistema.FormatarCep:null));bloco.Children.Add(caixa);p.Children.Add(bloco);return caixa;
    }
    private void VincularEdicao(TextBox caixa,Action<string> alterar,Func<string,string>? mascara)
    {
        var formatando=false;
        if(mascara is not null)
        {
            caixa.Text=mascara(caixa.Text);alterar(caixa.Text);
            caixa.PreviewTextInput+=(_,e)=>e.Handled=e.Text.Any(c=>c is <'0' or >'9');
            caixa.PreviewKeyDown+=(_,e)=>
            {
                if(caixa.SelectionLength!=0)return;
                var posicao=caixa.CaretIndex;
                if(e.Key==Key.Back&&posicao>0&&!char.IsAsciiDigit(caixa.Text[posicao-1]))
                {var anterior=posicao-1;while(anterior>=0&&!char.IsAsciiDigit(caixa.Text[anterior]))anterior--;if(anterior>=0)caixa.Select(anterior,posicao-anterior);}
                else if(e.Key==Key.Delete&&posicao<caixa.Text.Length&&!char.IsAsciiDigit(caixa.Text[posicao]))
                {var seguinte=posicao;while(seguinte<caixa.Text.Length&&!char.IsAsciiDigit(caixa.Text[seguinte]))seguinte++;if(seguinte<caixa.Text.Length)caixa.Select(posicao,seguinte-posicao+1);}
            };
        }
        caixa.TextChanged+=(_,e)=>
        {
            if(formatando)return;
            if(mascara is not null)
            {
                var texto=caixa.Text;var mudanca=e.Changes.LastOrDefault();var cursor=mudanca is null?caixa.CaretIndex:mudanca.Offset+mudanca.AddedLength;var digitosAntes=Mascaras.Digitos(texto[..Math.Min(cursor,texto.Length)]).Length;
                var formatado=mascara(texto);
                if(formatado!=texto){formatando=true;caixa.Text=formatado;formatando=false;}
                caixa.CaretIndex=Mascaras.PosicaoDoCursor(formatado,digitosAntes);
            }
            alterar(caixa.Text);Alterado();AtualizarCupom();
        };
    }
    private void Documento(Panel p)
    {
        var bloco=new StackPanel {Margin=new Thickness(0,0,18,14)};
        var label=new RotuloAjuda {Texto="Documento"+(_estado.Cupom.ExibirDocumentoCupom?" *":""),Margin=new Thickness(0,0,0,6)};bloco.Children.Add(label);
        var linha=new Grid();linha.ColumnDefinitions.Add(new(){Width=new GridLength(98)});linha.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        var tipo=new ComboBox {ItemsSource=new[]{"CNPJ","CPF"},SelectedItem=_estado.TipoDocumento,MinHeight=34,Margin=new Thickness(0,0,8,0)};AutomationProperties.SetName(tipo,"Tipo de documento");linha.Children.Add(tipo);
        var numero=new TextBox {Text=_estado.Cupom.Documento,MinHeight=34,Padding=new Thickness(8,6,8,6),ToolTip="Digite somente os números"};Grid.SetColumn(numero,1);linha.Children.Add(numero);AutomationProperties.SetName(numero,"Número do "+_estado.TipoDocumento);AutomationProperties.SetIsRequiredForForm(numero,_estado.Cupom.ExibirDocumentoCupom);
        VincularEdicao(numero,v=>{_estado.Cupom.Documento=v;if(_estado.TipoDocumento=="CPF")_estado.DocumentoCpf=v;else _estado.DocumentoCnpj=v;},v=>Mascaras.Documento(v,_estado.TipoDocumento));
        tipo.SelectionChanged+=(_,_)=>
        {
            if(tipo.SelectedItem is not string selecionado||selecionado==_estado.TipoDocumento)return;
            _estado.TipoDocumento=selecionado;numero.Text=selecionado=="CPF"?_estado.DocumentoCpf:_estado.DocumentoCnpj;
            _estado.Cupom.Documento=numero.Text;AutomationProperties.SetName(numero,"Número do "+selecionado);Alterado();numero.Focus();numero.CaretIndex=numero.Text.Length;
        };
        bloco.Children.Add(linha);p.Children.Add(bloco);
    }
    private CheckBox Marcar(Panel p,string titulo,bool valor,Action<bool> alterar)
    {
        var c=new CheckBox {Content=titulo,IsChecked=valor,Margin=new Thickness(0,0,0,14),MinHeight=26};c.Click+=(_,_)=>{alterar(c.IsChecked==true);Alterado();AtualizarCupom();};p.Children.Add(c);return c;
    }
    public void VerificarEdicaoMascarada()
    {
        _renderizando=true;
        try
        {
            foreach(var caso in new[]{("12345678901","123.456.789-01","CPF"),("12345678000199","12.345.678/0001-99","CNPJ"),("11987654321","(11) 98765-4321","Telefone")})
            {
                var caixa=new TextBox();Func<string,string> mascara=caso.Item3=="Telefone"?Mascaras.Telefone:v=>Mascaras.Documento(v,caso.Item3);VincularEdicao(caixa,_=>{},mascara);
                foreach(var digito in caso.Item1){caixa.Select(caixa.CaretIndex,0);caixa.SelectedText=digito.ToString();}
                if(caixa.Text!=caso.Item2)throw new InvalidOperationException("Falha na digitação mascarada de "+caso.Item3+": "+caixa.Text);
            }
        }
        finally{_renderizando=false;}
    }
    public void VerificarFluxoLista()
    {
        var anterior=_estado;_somenteVerificacao=true;
        try
        {
            _estado=EstadoConfiguracao.Exemplo();_estado.Etapa=0;_emEdicao=false;Mostrar();
            if(_navegacao.Children.Count!=7||((Button)_navegacao.Children[1]).IsEnabled)throw new InvalidOperationException("Lista inicial inválida.");
            ((Button)_navegacao.Children[0]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if(!_emEdicao||_estado.Etapa!=0)throw new InvalidOperationException("Abertura do formulário falhou.");
            _continuar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if(_emEdicao||_estado.Etapa!=1||!_estado.Concluidas.Contains(0)||!((Button)_navegacao.Children[1]).IsEnabled||((Button)_navegacao.Children[2]).IsEnabled)throw new InvalidOperationException("Conclusão não liberou somente a próxima etapa.");
            ((Button)_navegacao.Children[0]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));_voltar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if(_emEdicao||_conteudo.Content!=_visaoEtapas)throw new InvalidOperationException("Retorno à lista falhou.");
        }
        finally{_estado=anterior;_somenteVerificacao=false;_emEdicao=false;}
    }
    private FrameworkElement Estabelecimento()
    {
        var p=Painel();var campos=new UniformGridNativo {Columns=2};p.Children.Add(campos);var c=_estado.Cupom;
        Campo(campos,"Nome do estabelecimento",_estado.Nome,v=>_estado.Nome=v,true);Documento(campos);Campo(campos,"Razão social",c.RazaoSocial,v=>c.RazaoSocial=v);Campo(campos,"Inscrição estadual",c.InscricaoEstadual,v=>c.InscricaoEstadual=v);var telefone=new CampoTelefonePadrao(c.Telefone,c.PaisTelefone);
        var blocoTelefone=new StackPanel {Margin=new Thickness(0,0,18,14)};blocoTelefone.Children.Add(new RotuloAjuda {Texto="Telefone principal",Margin=new Thickness(0,0,0,6)});blocoTelefone.Children.Add(telefone);campos.Children.Add(blocoTelefone);
        telefone.Alterado+=(_,_)=>{c.Telefone=telefone.Telefone;c.PaisTelefone=telefone.CodigoPais;Alterado();};
        var endereco=new UniformGridNativo {Columns=2};Campo(endereco,"Endereço",c.Endereco,v=>c.Endereco=v,c.ExibirEnderecoCupom);Campo(endereco,"Número",c.Numero,v=>c.Numero=v);Campo(endereco,"Complemento",c.Complemento,v=>c.Complemento=v);Campo(endereco,"Bairro",c.Bairro,v=>c.Bairro=v);Campo(endereco,"CEP",c.Cep,v=>c.Cep=v);Campo(endereco,"Cidade",c.Cidade,v=>c.Cidade=v);var estados=new ComboBox {ItemsSource=new[]{"","AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG","PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"},SelectedItem=c.Uf.Trim().ToUpperInvariant(),MinHeight=36};
        var blocoEstado=new StackPanel {Margin=new Thickness(0,0,18,14)};blocoEstado.Children.Add(new RotuloAjuda {Texto="Estado (UF)",Margin=new Thickness(0,0,0,6)});blocoEstado.Children.Add(estados);endereco.Children.Add(blocoEstado);estados.SelectionChanged+=(_,_)=>{c.Uf=estados.SelectedItem as string??"";Alterado();};
        p.Children.Add(new Expander {Header="Endereço do estabelecimento",Content=endereco,Margin=new Thickness(0,10,0,0)});return Rolagem(p);
    }
    private FrameworkElement ComandaImpressa()
    {
        var g=new Grid();g.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});g.ColumnDefinitions.Add(new(){Width=new GridLength(350)});var p=Painel();var c=_estado.Cupom;
        Titulo(p,"Conteúdo do comprovante");Marcar(p,"Exibir CNPJ / CPF",c.ExibirDocumentoCupom,v=>c.ExibirDocumentoCupom=v);Marcar(p,"Exibir endereço do estabelecimento",c.ExibirEnderecoCupom,v=>c.ExibirEnderecoCupom=v);Marcar(p,"Agrupar itens iguais",c.AgruparItensCupom,v=>c.AgruparItensCupom=v);Marcar(p,"Exibir observações dos itens",c.ExibirObservacoesCupom,v=>c.ExibirObservacoesCupom=v);
        Campo(p,"Mensagem do cabeçalho",c.CabecalhoCupom,v=>c.CabecalhoCupom=v,false,true);Campo(p,"Mensagem do rodapé",c.RodapeCupom,v=>c.RodapeCupom=v,false,true);Texto(p,"As opções seguem o gerador de impressão atual. Campos não usados por um modelo não são acrescentados artificialmente à prévia.");g.Children.Add(Rolagem(p));
        var preview=new DockPanel {Margin=new Thickness(18,0,0,0)};var barra=new StackPanel {Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,10)};barra.Children.Add(new TextBlock {Text="Papel: ",VerticalAlignment=VerticalAlignment.Center});var papel=new ComboBox {ItemsSource=new[]{58,80},SelectedItem=_estado.Papel,Width=75,MinHeight=30};papel.SelectionChanged+=(_,_)=>{_estado.Papel=(int)papel.SelectedItem;Alterado();AtualizarCupom();};barra.Children.Add(papel);barra.Children.Add(new TextBlock {Text=" mm · Conta do cliente",VerticalAlignment=VerticalAlignment.Center});DockPanel.SetDock(barra,Dock.Top);preview.Children.Add(barra);
        _cupom=new FlowDocumentScrollViewer {IsToolBarVisible=false,Background=Brushes.White,HorizontalAlignment=HorizontalAlignment.Center};preview.Children.Add(_cupom);Grid.SetColumn(preview,1);g.Children.Add(preview);AtualizarCupom();return g;
    }
    private void AtualizarCupom()
    {
        if(_cupom is null)return;var agora=DateTime.UtcNow;var c=new Comanda {Id=0,Numero=12,NumeroMesa=5,CodigoPedido=1042,TotalCentavos=8000,AbertaEm=agora,Status=StatusComanda.Aberta};var itens=new[]{new ItemComanda {ComandaId=0,Id=1,ProdutoId=1,NomeProduto="Hambúrguer da casa (exemplo)",Quantidade=2,PrecoUnitarioCentavos=2500,SubtotalCentavos=5000,Observacao="Sem cebola",CriadoEm=agora,AtualizadoEm=agora},new ItemComanda {ComandaId=0,Id=2,ProdutoId=2,NomeProduto="Batata frita (exemplo)",Quantidade=1,PrecoUnitarioCentavos=1800,SubtotalCentavos=1800,CriadoEm=agora,AtualizadoEm=agora},new ItemComanda {ComandaId=0,Id=3,ProdutoId=3,NomeProduto="Refrigerante (exemplo)",Quantidade=2,PrecoUnitarioCentavos=600,SubtotalCentavos=1200,CriadoEm=agora,AtualizadoEm=agora}};
        _cupom.Document=CupomTermico.Conta(_estado.Cupom,string.IsNullOrWhiteSpace(_estado.Nome)?"Estabelecimento de exemplo":_estado.Nome,c,itens,_estado.Papel);_cupom.Width=CupomTermico.Largura(_estado.Papel)+18;
    }
    private FrameworkElement Atendimento()
    {
        if(_servico is not null){var real=Painel();Campo(real,"Quantidade inicial de comandas",_estado.QuantidadeComandas.ToString(),v=>_estado.QuantidadeComandas=int.TryParse(v,out var n)?n:0,true);Texto(real,"Mesas são vinculadas às comandas durante o atendimento. O sistema não exige um cadastro separado de mesas.");return Rolagem(real);}
        var p=Painel();Marcar(p,"Balcão e comandas",_estado.Balcao,v=>_estado.Balcao=v);var painelMesas=Painel();Marcar(p,"Atendimento em mesas",_estado.Mesas,v=>{_estado.Mesas=v;painelMesas.Visibility=v?Visibility.Visible:Visibility.Collapsed;});
        Campo(p,"Quantidade inicial de comandas",_estado.QuantidadeComandas.ToString(),v=>_estado.QuantidadeComandas=int.TryParse(v,out var n)?n:0,true);Campo(painelMesas,"Quantidade inicial de mesas",_estado.QuantidadeMesas.ToString(),v=>_estado.QuantidadeMesas=int.TryParse(v,out var n)?n:0,true);painelMesas.Visibility=_estado.Mesas?Visibility.Visible:Visibility.Collapsed;p.Children.Add(painelMesas);
        var lista=new ListView {MinHeight=180,MaxHeight=220};var view=new GridView();view.Columns.Add(new GridViewColumn {Header="Número",DisplayMemberBinding=new System.Windows.Data.Binding("Numero"),Width=100});view.Columns.Add(new GridViewColumn {Header="Identificação",DisplayMemberBinding=new System.Windows.Data.Binding("Nome"),Width=340});lista.View=view;
        var info=new TextBlock {Margin=new Thickness(12,0,12,0),VerticalAlignment=VerticalAlignment.Center};void Atualizar(){var total=Math.Clamp(_estado.QuantidadeMesas,0,300);_paginaMesa=Math.Min(_paginaMesa,Math.Max(0,(total-1)/6));lista.ItemsSource=Enumerable.Range(_paginaMesa*6+1,Math.Min(6,Math.Max(0,total-_paginaMesa*6))).Select(n=>new{Numero=n.ToString("00"),Nome=$"Mesa {n:00}"});info.Text=$"{_paginaMesa+1} de {Math.Max(1,(int)Math.Ceiling(total/6d))}";}
        painelMesas.Children.Add(lista);var acoes=new StackPanel {Orientation=Orientation.Horizontal};acoes.Children.Add(Botao("Anterior",()=>{_paginaMesa=Math.Max(0,_paginaMesa-1);Atualizar();}));acoes.Children.Add(info);acoes.Children.Add(Botao("Próxima",()=>{if((_paginaMesa+1)*6<_estado.QuantidadeMesas)_paginaMesa++;Atualizar();}));painelMesas.Children.Add(acoes);Atualizar();return Rolagem(p);
    }
    private FrameworkElement Produtos()
    {
        var p=Painel();var lista=new ListView {ItemsSource=_estado.Produtos,Height=180};var view=new GridView();view.Columns.Add(new(){Header="Nome",Width=360,DisplayMemberBinding=new System.Windows.Data.Binding("Nome")});view.Columns.Add(new(){Header="Unidade",Width=90,DisplayMemberBinding=new System.Windows.Data.Binding("Unidade")});view.Columns.Add(new(){Header="Preço de venda",Width=140,DisplayMemberBinding=new System.Windows.Data.Binding("Preco")});lista.View=view;p.Children.Add(lista);
        var campos=new UniformGridNativo {Columns=3};p.Children.Add(campos);var nome=Campo(campos,"Nome do produto","",_=>{},true);var preco=Campo(campos,"Preço de venda (R$)","",_=>{},true);var unidade=new ComboBox {ItemsSource=new[]{"UN","KG"},SelectedIndex=0,Margin=new Thickness(0,22,0,14),MinHeight=34};campos.Children.Add(unidade);AutomationProperties.SetName(unidade,"Unidade de venda");
        p.Children.Add(Botao("Adicionar produto",()=>{var cents=EstadoConfiguracao.Preco(preco.Text);if(string.IsNullOrWhiteSpace(nome.Text)||cents is null){Erro("Informe o nome e um preço maior que zero, como 12,50.");return;}if(_estado.Produtos.Any(x=>x.Nome.Equals(nome.Text.Trim(),StringComparison.OrdinalIgnoreCase))){Erro("Já existe um produto com esse nome no protótipo.");return;}_estado.Produtos.Add(new(nome.Text.Trim(),(string)unidade.SelectedItem,cents.Value));Alterado();Mostrar();}));p.Children.Add(Botao("Remover produto selecionado",()=>{if(lista.SelectedItem is ProdutoInicial produto){_estado.Produtos.Remove(produto);Alterado();Mostrar();}}));Texto(p,"Os produtos serão cadastrados na categoria Produtos ao concluir. Produtos existentes não serão substituídos.");return Rolagem(p);
    }
    private FrameworkElement Pagamentos()
    {
        var p=Painel();var obrigatorio=new TextBlock { Margin=new Thickness(0,4,0,14),FontSize=16,FontWeight=FontWeights.SemiBold };obrigatorio.Inlines.Add(new Run("Formas aceitas"));obrigatorio.Inlines.Add(new Run(" *"){Foreground=Brushes.Firebrick});p.Children.Add(obrigatorio);foreach(var forma in new[]{"Dinheiro","Pix","Débito","Crédito"})Marcar(p,forma,_estado.Pagamentos.Contains(forma),v=>{if(v&&!_estado.Pagamentos.Contains(forma))_estado.Pagamentos.Add(forma);if(!v)_estado.Pagamentos.Remove(forma);});Texto(p,"A mesma comanda pode ter recebimentos parciais e formas combinadas. Esta configuração não realiza cobranças nem integra Pix ou maquininha.");return Rolagem(p);
    }
    private FrameworkElement Equipe()
    {
        if(_servico is not null){var real=Painel();Campo(real,"Nome do responsável",_estado.Responsavel,v=>_estado.Responsavel=v);Texto(real,"Use o cadastro de colaboradores para definir funções, login opcional e permissões. Alterações nesse cadastro são salvas ao confirmar o colaborador.");if(_abrirColaboradores is not null)real.Children.Add(Botao("Cadastrar ou revisar colaboradores",()=>{_abrirColaboradores(this);_estado.Colaboradores=_listarColaboradores?.Invoke().Select(c=>new ColaboradorInicial(c.Nome,c.Funcao)).ToList()??[];Alterado();Mostrar();}));else Texto(real,"Seu acesso não permite alterar colaboradores. Um administrador poderá configurá-los depois.");return Rolagem(real);}
        var p=Painel();Campo(p,"Nome do responsável",_estado.Responsavel,v=>_estado.Responsavel=v);Texto(p,"Login, senha, administrador, funções e permissões continuam no cadastro completo de colaboradores. Este protótipo não ativa ou desativa a segurança do sistema.");var lista=new ListView {ItemsSource=_estado.Colaboradores,Height=140};var view=new GridView();view.Columns.Add(new(){Header="Nome",Width=360,DisplayMemberBinding=new System.Windows.Data.Binding("Nome")});view.Columns.Add(new(){Header="Função",Width=180,DisplayMemberBinding=new System.Windows.Data.Binding("Funcao")});lista.View=view;p.Children.Add(lista);
        var nome=Campo(p,"Nome do colaborador","",_=>{},true);var funcao=new ComboBox {ItemsSource=new[]{"Atendente","Supervisor","Entregador"},SelectedIndex=0,MinHeight=34};AutomationProperties.SetName(funcao,"Função do colaborador");p.Children.Add(funcao);p.Children.Add(Botao("Adicionar colaborador",()=>{if(string.IsNullOrWhiteSpace(nome.Text)){Erro("Informe o nome do colaborador.");return;}_estado.Colaboradores.Add(new(nome.Text.Trim(),(string)funcao.SelectedItem));Alterado();Mostrar();}));return Rolagem(p);
    }
    private FrameworkElement Revisao()
    {
        var p=Painel();var resumos=new[]{_estado.Nome,$"Papel {_estado.Papel} mm · {(_estado.Cupom.ExibirDocumentoCupom?"com documento":"sem documento")} · {(_estado.Cupom.ExibirEnderecoCupom?"com endereço":"sem endereço")}",$"{_estado.QuantidadeComandas} comandas · {(_estado.Mesas?_estado.QuantidadeMesas:0)} mesas",$"{_estado.Produtos.Count} produtos",string.Join(", ",_estado.Pagamentos),$"{_estado.Responsavel} · {_estado.Colaboradores.Count} colaboradores"};
        for(var i=0;i<6;i++){var indice=i;var linha=new DockPanel {Margin=new Thickness(0,0,0,10)};var editar=Botao("Editar",()=>Navegar(indice,false));DockPanel.SetDock(editar,Dock.Right);linha.Children.Add(editar);var textos=Painel();Titulo(textos,Etapas[i]);Texto(textos,resumos[i]);linha.Children.Add(textos);p.Children.Add(linha);}
        if(_servico is null){Marcar(p,"Simular falha ao concluir",_falhar,v=>_falhar=v);Texto(p,"A conclusão salva somente um rascunho de avaliação na pasta do protótipo. Não altera os cadastros reais do VComanda.");}else Texto(p,"Ao concluir, os dados do estabelecimento, as comandas, o conteúdo do cupom e os produtos serão salvos neste sistema. Cadastros existentes não serão substituídos.");return Rolagem(p);
    }
    private sealed class UniformGridNativo:System.Windows.Controls.Primitives.UniformGrid{}
}
