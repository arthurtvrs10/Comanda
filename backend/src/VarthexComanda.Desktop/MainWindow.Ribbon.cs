using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VarthexComanda.Desktop.Catalogo;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private string _categoriaRibbon = "Principal";
    private sealed record DestinoRibbon(string Categoria, string Grupo, string Nome, IconeVarthex Icone, Func<bool> Permitido, Action Abrir, bool ExibirNaRibbon = true);
    private sealed record ResultadoPesquisa(string Nome, Action Abrir);
    private List<DestinoRibbon> _destinosRibbon = new();
    private void InicializarRibbon(string pasta)
    {
        TemaInterface.Inicializar(pasta);
        ModoEscuro.IsChecked = TemaInterface.Escuro;
        AtualizarBotaoAjuda();
        _destinosRibbon = new()
        {
            new("Principal", "Pedido", "Mesas / comandas", IconeVarthex.Mesas, () => !_bloqueado && !_cozinhaAtiva && !_restauracao, IrParaAtendimento),
            new("Relatórios", "Vendas", "Histórico de pedidos", IconeVarthex.Historico, () => !_bloqueado && !_cozinhaAtiva && !_restauracao && _historicoPedidosView is not null, IrParaHistoricoPedidos),
            new("Apps", "Operação", "Cozinha", IconeVarthex.Cozinha, () => !_bloqueado && !_restauracao, () => { _cozinhaView.Atualizar(); MostrarConteudo(_cozinhaView); }),
            new("Apps", "Atendimento", "Painel de senhas", IconeVarthex.Painel, PodeChamarSenha, AbrirPainelSenhas),
            new("Produtos", "Catálogo", "Produtos", IconeVarthex.Produtos, PodeGerir, IrParaProdutos),
            new("Relatórios", "Vendas", "Histórico de vendas", IconeVarthex.Historico, () => !_bloqueado && !_cozinhaAtiva && !_restauracao, IrParaHistorico),
            new("Configurações", "Configurações", "Configurações do sistema", IconeVarthex.Configuracoes, PodeGerir, IrParaConfiguracoes),
            new("Configurações", "Configurações", "Numeração de pedidos", IconeVarthex.Numeracao, PodeGerir, AbrirNumeracaoPedidos),
            new("Configurações", "Configurações", "Impressoras", IconeVarthex.Impressoras, PodeGerir, AbrirImpressoras),
            new("Configurações", "Configurações", "Colaboradores", IconeVarthex.Colaboradores, PodeGerir, AbrirColaboradores),
            new("Configurações", "Configurações", "Alterar senha", IconeVarthex.Senha, PodeAlterarSenha, () => AbrirAlteracaoAcesso(false)),
            new("Configurações", "Configurações", "Backup e restauração", IconeVarthex.Backup, () => !_bloqueado && _acesso.Autenticado, IrParaBackup),
            new("Configurações", "Versão premium", "Minha assinatura", IconeVarthex.Assinatura, PodeGerir, AbrirMinhaAssinatura),
            new("Ajuda", "Ajuda e suporte", "Ajuda", IconeVarthex.Ajuda, () => !_bloqueado, AbrirAjuda),
            new("Ajuda", "Ideias e aprendizado", "Feedback", IconeVarthex.Feedback, () => !_bloqueado, AbrirFeedback, false),
            new("Ajuda", "Ideias e aprendizado", "Show training", IconeVarthex.Treinamento, () => !_bloqueado, AbrirTreinamento, false),
            new("Ajuda", "Verificações", "Diagnóstico de serviços", IconeVarthex.Diagnostico, PodeGerir, AbrirDiagnostico, false)

        };
        foreach (var nome in new[] { "Principal", "Produtos", "Marketing", "Financeiro", "Relatórios", "Apps", "Configurações", "Ajuda" })
        {
            var botao = new Button { Content = nome, Tag = nome, Padding = new Thickness(16,7,16,7), MinHeight = 36, FontSize = 12, Style = (Style)FindResource("AbaNavegacao"), Foreground=Brushes.White };
            botao.ContentTemplate = (DataTemplate)FindResource("RotuloCategoriaRibbon");
            botao.SetResourceReference(Control.BackgroundProperty,"RibbonBar");
            botao.Click += (_,_) => SelecionarCategoriaRibbon(nome);
            CategoriasRibbon.Children.Add(botao);
        }
        AreaSistema.SizeChanged += (_,_) => AtualizarRibbonCompacta();
        SelecionarCategoriaRibbon("Principal");
    }
    private bool PodeGerir() => !_bloqueado && _acesso.Autenticado && !_restauracao;
    private void AbrirConfiguracaoRibbon(int secao)
    {
        IrParaConfiguracoes();
        if (ReferenceEquals(ConteudoPrincipal.JanelaAtiva?.Conteudo, _configuracaoView)) _configuracaoView.AbrirSecao(secao);
    }
    private void SelecionarCategoriaRibbon(string categoria)
    {
        _categoriaRibbon = categoria;
        foreach (Button b in CategoriasRibbon.Children)
        {
            b.Tag = Equals(b.Content,categoria) ? "ativa" : null;
            b.SetResourceReference(Control.ForegroundProperty,Equals(b.Content,categoria)?"RibbonInk":"TextFFFFFF");
            System.Windows.Automation.AutomationProperties.SetItemStatus(b, Equals(b.Content,categoria) ? "Categoria selecionada" : "");
        }
        AtualizarRibbon();
    }
    private void AtualizarRibbon()
    {
        if (_destinosRibbon.Count == 0) return;
        GruposRibbon.Children.Clear();
        foreach (var grupo in _destinosRibbon.Where(d=>d.Categoria==_categoriaRibbon && d.ExibirNaRibbon).GroupBy(d=>d.Grupo))
        {
            var painel = new StackPanel();
            var comandos = new StackPanel { Orientation=Orientation.Horizontal };
            foreach (var destino in grupo)
            {
                var conteudo = new Grid();
                conteudo.RowDefinitions.Add(new RowDefinition { Height=new GridLength(32) });
                conteudo.RowDefinitions.Add(new RowDefinition { Height=new GridLength(32) });
                var icone = IconesVarthex.Criar(destino.Icone,destino.Icone==IconeVarthex.Configuracoes?32:30);
                conteudo.Children.Add(icone);
                var rotulo=new TextBlock { Text=destino.Nome, TextWrapping=TextWrapping.Wrap, TextAlignment=TextAlignment.Center, MaxWidth=130,VerticalAlignment=VerticalAlignment.Center,LineHeight=14,LineStackingStrategy=LineStackingStrategy.BlockLineHeight };
                Grid.SetRow(rotulo,1);conteudo.Children.Add(rotulo);
                var b=new Button { Content=conteudo, Padding=new Thickness(10,5,10,5), MinWidth=100, Height=78, Margin=new Thickness(0,0,4,0), BorderThickness=new Thickness(0), Background=Brushes.Transparent, FontSize=12,HorizontalContentAlignment=HorizontalAlignment.Center,VerticalContentAlignment=VerticalAlignment.Center };
                System.Windows.Automation.AutomationProperties.SetName(b,destino.Nome);
                b.Click+=(_,_)=>ExecutarDestino(destino);
                comandos.Children.Add(b);
            }
            painel.Children.Add(comandos);
            var subtitulo=new TextBlock { Text=grupo.Key, FontSize=11, HorizontalAlignment=HorizontalAlignment.Center, Margin=new Thickness(0,4,0,0) };
            subtitulo.SetResourceReference(TextBlock.ForegroundProperty,"RibbonSubtitle");
            painel.Children.Add(subtitulo);
            var borda=new Border { Child=painel, BorderThickness=new Thickness(0,0,1,0), Padding=new Thickness(4,0,12,0), Margin=new Thickness(0,0,8,0) };
            borda.SetResourceReference(Border.BorderBrushProperty,"Line");
            GruposRibbon.Children.Add(borda);
        }
        if(GruposRibbon.Children.Count==0) GruposRibbon.Children.Add(new TextBlock { Text=_categoriaRibbon is "Marketing" or "Apps" ? "Nenhum módulo disponível nesta categoria nesta versão." : "Nenhum comando disponível para o perfil atual.", Margin=new Thickness(12,20,12,20), TextWrapping=TextWrapping.Wrap });
        AtualizarRibbonCompacta();
        AtualizarPesquisa();
    }
    private void ExecutarDestino(DestinoRibbon destino)
    {
        if (!GarantirAcessoDestino(destino)) return;
        destino.Abrir();
        ResultadosPesquisa.Visibility=Visibility.Collapsed;
    }
    private bool GarantirAcessoDestino(DestinoRibbon destino)
    {
        if (_restauracao && destino.Nome!="Backup e restauração")
        {
            JanelaAviso.Mostrar("Recuperação", "Restaure um backup antes de acessar esta função.", TipoAviso.Informacao);
            return false;
        }
        if (destino.Permitido()) return true;
        if (_bloqueado && destino.Nome=="Mesas / comandas")
        {
            EscolherArea("garcom");
            return false;
        }
        if (destino.Categoria is "Produtos" or "Configurações")
        {
            if (new JanelaAcessoGestor(_acesso) { Owner=this }.ShowDialog()!=true) return false;
            _bloqueado=false;
            _cozinhaAtiva=false;
            AreaSistema.IsEnabled=true;
            CamadaBloqueio.Visibility=Visibility.Collapsed;
            AtualizarPerfil();
        }
        else
        {
            var login=new JanelaLogin(_acesso,_acessoAtendimento,colaboradores:_colaboradores) { Owner=this };
            if (login.ShowDialog()!=true) return false;
            if (login.Perfil=="gestor" && _acesso.Autenticado)
            {
                _bloqueado=false;
                _cozinhaAtiva=false;
                AreaSistema.IsEnabled=true;
                CamadaBloqueio.Visibility=Visibility.Collapsed;
                AtualizarPerfil();
            }
            else
            {
                _acesso.Sair();
                _areaPendente=login.Perfil;
                DesbloquearAtendimento();
                ConteudoPrincipal.Limpar();
                AtualizarAbaAtiva();
            }
        }
        return destino.Permitido();
    }
    private void AtualizarRibbonCompacta()
    {
        bool compacto=(AreaSistema.ActualWidth>0?AreaSistema.ActualWidth:Width)<1050;
        GruposRibbon.Visibility=compacto?Visibility.Collapsed:Visibility.Visible;
        RibbonCompacta.Visibility=compacto?Visibility.Visible:Visibility.Collapsed;
        RibbonCompacta.Content=_categoriaRibbon+" · comandos ▾";
    }
    private void RibbonCompacta_Click(object sender,RoutedEventArgs e)
    {
        var menu=new ContextMenu();
        foreach(var grupo in _destinosRibbon.Where(d=>d.Categoria==_categoriaRibbon && d.ExibirNaRibbon).GroupBy(d=>d.Grupo))
        {
            var item=new MenuItem { Header=grupo.Key };
            foreach(var destino in grupo){var comando=new MenuItem { Header=destino.Nome, Icon=IconesVarthex.Criar(destino.Icone,20) };comando.Click+=(_,_)=>ExecutarDestino(destino);item.Items.Add(comando);}
            menu.Items.Add(item);
        }
        if(menu.Items.Count==0)menu.Items.Add(new MenuItem { Header="Nenhum comando disponível",IsEnabled=false });
        menu.PlacementTarget=RibbonCompacta;menu.IsOpen=true;
    }
    private static string NormalizarPesquisa(string texto) => new(texto.Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark).Select(char.ToLowerInvariant).ToArray());
    private void AtualizarPesquisa()
    {
        if(PesquisaGlobal is null || _destinosRibbon.Count==0)return;
        var busca=NormalizarPesquisa(PesquisaGlobal.Text.Trim());
        var resultados=new List<ResultadoPesquisa>();
        if(busca.Length>0)
        {
            resultados.AddRange(_destinosRibbon.Where(d=>NormalizarPesquisa(d.Nome+" "+d.Categoria).Contains(busca)).Select(d=>new ResultadoPesquisa(d.Nome,()=>ExecutarDestino(d))));
            if(!_bloqueado && !_cozinhaAtiva && !_restauracao)
            {
                foreach(var produto in _atendimentoView.ViewModel.PesquisarProdutosGlobais(PesquisaGlobal.Text.Trim()).Take(8))
                {
                    var capturado=produto;
                    resultados.Add(new("Produto · "+produto.Nome,()=>{ if(_bloqueado || _cozinhaAtiva || _restauracao)return; if(_acesso.Autenticado){IrParaProdutos();((ProdutosViewModel)_produtosView.DataContext).TextoBusca=capturado.Nome;}else {IrParaAtendimento();_atendimentoView.ViewModel.TextoBuscaCatalogo=capturado.Nome;} }));
                }
            }
        }
        ResultadosPesquisa.ItemsSource=resultados;
        ResultadosPesquisa.SelectedIndex=resultados.Count>0?0:-1;
        ResultadosPesquisa.Visibility=busca.Length>0?Visibility.Visible:Visibility.Collapsed;
    }
    private void PesquisaGlobal_TextChanged(object sender,TextChangedEventArgs e)=>AtualizarPesquisa();
    private void AbrirPesquisa_Click(object sender,RoutedEventArgs e)=>AbrirPesquisa();
    private void AbrirPesquisa()
    {
        BotaoPesquisar.Visibility=Visibility.Collapsed;
        PesquisaGlobal.Visibility=Visibility.Visible;
        PesquisaGlobal.Focus();
        PesquisaGlobal.SelectAll();
    }
    private void RecolherPesquisa()
    {
        PesquisaGlobal.Clear();
        PesquisaGlobal.Visibility=Visibility.Collapsed;
        BotaoPesquisar.Visibility=Visibility.Visible;
    }
    private void PesquisaGlobal_LostKeyboardFocus(object sender,KeyboardFocusChangedEventArgs e)
    {
        if(string.IsNullOrWhiteSpace(PesquisaGlobal.Text))RecolherPesquisa();
    }
    private void PesquisaGlobal_KeyDown(object sender,KeyEventArgs e)
    {
        if(e.Key==Key.Down){ResultadosPesquisa.Focus();e.Handled=true;}
        else if(e.Key==Key.Enter){AbrirResultadoPesquisa();e.Handled=true;}
        else if(e.Key==Key.Escape){RecolherPesquisa();BotaoPesquisar.Focus();e.Handled=true;}
    }
    private void ResultadosPesquisa_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){AbrirResultadoPesquisa();e.Handled=true;}else if(e.Key==Key.Escape){PesquisaGlobal.Clear();PesquisaGlobal.Focus();e.Handled=true;}}
    private void ResultadosPesquisa_MouseDoubleClick(object sender,MouseButtonEventArgs e)=>AbrirResultadoPesquisa();
    private void AbrirResultadoPesquisa(){if(ResultadosPesquisa.SelectedItem is ResultadoPesquisa r){r.Abrir();RecolherPesquisa();}}
    private void ModoEscuro_Changed(object sender,RoutedEventArgs e)
    {
        if (ModoEscuro.IsChecked == TemaInterface.Escuro) { AplicarFundoSistema(); return; }
        try { TemaInterface.Aplicar(ModoEscuro.IsChecked==true,true); }
        catch(Exception ex) when(ex is System.IO.IOException or UnauthorizedAccessException){JanelaAviso.Mostrar("Aparência","O tema foi aplicado, mas não foi possível salvar a preferência.",TipoAviso.Aviso);}
        finally { AplicarFundoSistema(); }
    }

    private void MenuSessao_Click(object sender,RoutedEventArgs e)
    {
        if (!PodeSairDasConfiguracoes()) return;
        MostrarAreaPrincipal();
        var login = new JanelaLogin(_acesso, _acessoAtendimento, _restauracao, _colaboradores) { Owner=this };
        if (login.ShowDialog()!=true) return;
        if (login.Perfil=="gestor" && _acesso.Autenticado)
        {
            _bloqueado=false;
            _cozinhaAtiva=false;
            CamadaBloqueio.Visibility=Visibility.Collapsed;
            AreaSistema.IsEnabled=true;
            AtualizarPerfil();
            if (_restauracao) IrParaBackup();
        }
        else
        {
            _areaPendente=login.Perfil;
            DesbloquearAtendimento();
            if (login.Perfil=="garcom")
            {
                ConteudoPrincipal.Limpar();
                AtualizarAbaAtiva();
            }
        }
    }

    private void BackupRodape_Click(object sender,RoutedEventArgs e)
    {
        ExecutarDestino(_destinosRibbon.Single(d=>d.Nome=="Backup e restauração"));
    }
}
