using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaEditarColaborador : Window
{
    public JanelaEditarColaborador(ServicoColaboradores servico, AcessoGestor gestor, Colaborador atual, bool novo)
    {
        Title = novo ? "Novo colaborador" : "Editar colaborador";
        Width = Math.Min(1020, SystemParameters.WorkArea.Width); Height = Math.Min(710, SystemParameters.WorkArea.Height);
        MinWidth = Math.Min(940, SystemParameters.WorkArea.Width); MinHeight = Math.Min(670, SystemParameters.WorkArea.Height);
        DialogoCompacto.Aplicar(this);
        foreach (var tipo in new[] { typeof(Label), typeof(TextBlock) })
        {
            var estilo = new Style(tipo, TryFindResource(tipo) as Style);
            estilo.Setters.Add(new Setter(tipo == typeof(Label) ? Control.ForegroundProperty : TextBlock.ForegroundProperty, new System.Windows.DynamicResourceExtension("Ink")));
            Resources[tipo] = estilo;
        }
        var raiz = new DockPanel { Margin = new Thickness(16) };
        raiz.SetResourceReference(BackgroundProperty, "Canvas"); Content = raiz;
        var alterou = false; var salvo = false;
        var rodape = new StackPanel(); DockPanel.SetDock(rodape, Dock.Bottom); raiz.Children.Add(rodape);
        var erro = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,8) };
        erro.SetResourceReference(TextBlock.ForegroundProperty, "TextB22222"); rodape.Children.Add(erro);
        var acoes = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var voltar = new Button { Content = "Voltar", MinWidth = 96, Margin = new Thickness(0,0,8,0) };
        voltar.Click += (_,_) => Close(); acoes.Children.Add(voltar);
        var salvar = new Button { Content = "Salvar", IsDefault = true, MinWidth = 96 }; acoes.Children.Add(salvar); rodape.Children.Add(acoes);
        var abas = new TabControl(); abas.SetResourceReference(Control.BackgroundProperty, "Canvas"); abas.SetResourceReference(Control.ForegroundProperty, "Ink"); raiz.Children.Add(abas);
        var geral = new StackPanel { Margin = new Thickness(8) }; geral.SetResourceReference(Panel.BackgroundProperty, "Canvas");
        abas.Items.Add(new TabItem { Header = "Geral", Content = geral });
        Panel destinoCampos = geral;
        TextBox Campo(string titulo, string valor, int max)
        {
            var campo = new TextBox { Text = valor, MaxLength = max, Margin = new Thickness(0,0,0,5) };
            System.Windows.Automation.AutomationProperties.SetName(campo, titulo);
            var grupo = new StackPanel { Margin = new Thickness(0,0,10,0) };
            grupo.Children.Add(new Label { Content = titulo, Target = campo, Padding = new Thickness(0,0,0,3) }); grupo.Children.Add(campo); destinoCampos.Children.Add(grupo);
            campo.TextChanged += (_,_) => alterou = true; return campo;
        }
        void Secao(string titulo) => geral.Children.Add(new TextBlock { Text = titulo, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,8,0,5) });
        CheckBox Opcao(string texto, bool marcado, Panel onde)
        {
            var c = new CheckBox { Content = new TextBlock { Text = texto, TextWrapping = TextWrapping.Wrap }, ToolTip = texto, IsChecked = marcado, Margin = new Thickness(0,0,12,6), VerticalContentAlignment = VerticalAlignment.Center };
            System.Windows.Automation.AutomationProperties.SetName(c, texto);
            c.Click += (_,_) => alterou = true; onde.Children.Add(c); return c;
        }
        Secao("Dados pessoais");
        destinoCampos = new System.Windows.Controls.Primitives.UniformGrid { Columns = 3 }; geral.Children.Add(destinoCampos);
        var nome = Campo("Nome *", atual.Nome, 120);
        var cpf = Campo("CPF (opcional)", PrimeiraConfiguracao.Mascaras.Documento(atual.Cpf,"CPF"), 14);
        var telefone=new CampoTelefonePadrao(atual.Telefone,atual.PaisTelefone);
        System.Windows.Automation.AutomationProperties.SetName(telefone.Numero,"Telefone");
        var grupoTelefone=new StackPanel {Margin=new Thickness(0,0,10,5)};
        grupoTelefone.Children.Add(new Label {Content="Telefone",Target=telefone.Numero,Padding=new Thickness(0,0,0,3)});grupoTelefone.Children.Add(telefone);
        destinoCampos.Children.Add(grupoTelefone);telefone.Alterado+=(_,_)=>alterou=true;

        JanelaConfiguracoesSistema.VincularEntrada(cpf,_=>{},v=>PrimeiraConfiguracao.Mascaras.Documento(v,"CPF"));
        var email = Campo("E-mail", atual.Email, 254);
        var comissao = Campo("Comissão por atendimento (%)", atual.ComissaoPercentual.ToString(CultureInfo.CurrentCulture), 8);
        destinoCampos = geral;
        Secao("Funções no estabelecimento");
        var funcoes = new WrapPanel(); geral.Children.Add(funcoes);
        var marcadas = new Dictionary<string, CheckBox>();
        foreach (var f in new[] { "Supervisor", "Atendente", "Entregador", "Cozinha" })
            marcadas[f] = Opcao(f, atual.Funcoes.Contains(f) || atual.Funcao == f, funcoes);
        // Mantém funções livres dos cadastros anteriores, sem descartá-las na edição.
        var funcao = Campo("Outra função (opcional)", string.Join(", ", atual.Funcao.Split(", ").Where(f => !atual.Funcoes.Contains(f) && !marcadas.ContainsKey(f))), 80);
        var ativo = Opcao("Colaborador ativo", atual.Ativo, geral);
        Secao("Acesso ao sistema");
        var criar = Opcao("Criar usuário de acesso ao programa", novo ? false : atual.CriarUsuario, geral);
        var acesso = new StackPanel(); geral.Children.Add(acesso);
        var credenciais = new System.Windows.Controls.Primitives.UniformGrid { Columns = 3 }; acesso.Children.Add(credenciais);
        void Credencial(string titulo, Control campo) { var grupo = new StackPanel { Margin = new Thickness(0,0,10,0) }; grupo.Children.Add(new Label { Content = titulo, Target = campo }); grupo.Children.Add(campo); credenciais.Children.Add(grupo); }
        var login = new TextBox { Text = atual.Login, MaxLength = 60, Margin = new Thickness(0,0,0,8) };
        Credencial("Login *", login); login.TextChanged += (_,_) => alterou = true;
        var propria = servico.SessaoAtual?.Id == atual.Id;
        var senha = new PasswordBox { MaxLength = 128, Margin = new Thickness(0,0,0,8) };
        var confirmar = new PasswordBox { MaxLength = 128, Margin = new Thickness(0,0,0,8) };
        if (!propria)
        {
            Credencial(novo || !atual.CriarUsuario ? "Senha *" : "Nova senha (vazia mantém a atual)", senha);
            Credencial("Confirmar senha", confirmar);
        }
        else acesso.Children.Add(new TextBlock { Text = "Para mudar sua senha, use Configurações > Alterar senha.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,8) });
        senha.PasswordChanged += (_,_) => alterou = true; confirmar.PasswordChanged += (_,_) => alterou = true;
        var estadosAcesso = new WrapPanel(); acesso.Children.Add(estadosAcesso);
        var liberado = Opcao("Acesso ao programa liberado", atual.AcessoLiberado, estadosAcesso);
        var admin = Opcao("Administrador do sistema — acesso completo", atual.Administrador, estadosAcesso);
        var mestra = Opcao("Pode autorizar operações com sua senha (senha mestra)", atual.SenhaMestra, estadosAcesso);
        acesso.Children.Add(new TextBlock { Text = "A senha mestra autoriza somente as operações que esta pessoa pode executar. Cada autorização é registrada.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,8) });

        var painelPermissoes = new DockPanel { Margin = new Thickness(8) }; painelPermissoes.SetResourceReference(Panel.BackgroundProperty, "Canvas");
        var abaPermissoes = new TabItem { Header = "Permissões de acesso", Content = painelPermissoes }; abas.Items.Add(abaPermissoes);
        var ferramentas = new StackPanel(); DockPanel.SetDock(ferramentas, Dock.Top); painelPermissoes.Children.Add(ferramentas);
        var explicacao = new TextBlock { Text = "Selecione o que esta pessoa poderá fazer no computador. As funções no estabelecimento não concedem permissões automaticamente.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,12) }; ferramentas.Children.Add(explicacao);
        var copiarLinha = new DockPanel { Margin = new Thickness(0,0,0,10) };
        var copiar = new Button { Content = "Copiar", MinWidth = 80, Margin = new Thickness(8,0,0,0) }; DockPanel.SetDock(copiar, Dock.Right); copiarLinha.Children.Add(copiar);
        var origem = new ComboBox { ItemsSource = servico.Listar().Where(c => c.Id != atual.Id && c.CriarUsuario).ToArray(), DisplayMemberPath = "Nome", ToolTip = "Copiar permissões de outro colaborador" };
        System.Windows.Automation.AutomationProperties.SetName(origem, "Copiar permissões de");
        ferramentas.Children.Add(new Label { Content = "Copiar permissões de", Target = origem }); copiarLinha.Children.Add(origem); ferramentas.Children.Add(copiarLinha);
        var pesquisa = new TextBox { MaxLength = 100, Margin = new Thickness(0,0,0,8), ToolTip = "Pesquisar permissões" };
        System.Windows.Automation.AutomationProperties.SetName(pesquisa, "Pesquisar permissões"); ferramentas.Children.Add(new Label { Content = "Pesquisar permissões", Target = pesquisa }); ferramentas.Children.Add(pesquisa);
        var todos = new CheckBox { Content = "Marcar todas", Margin = new Thickness(0,0,0,10) }; ferramentas.Children.Add(todos);
        var lista = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2, VerticalAlignment = VerticalAlignment.Top }; painelPermissoes.Children.Add(lista);
        var permissoes = new Dictionary<string,CheckBox>();
        var iniciais = atual.Permissoes ?? PermissoesColaborador.Padrao(novo ? "garcom" : atual.Perfil);
        foreach (var p in PermissoesColaborador.Todas) permissoes[p.Id] = Opcao(p.Nome, iniciais.Contains(p.Id), lista);
        void EstadoTodos() => todos.IsChecked = permissoes.Values.All(c => c.IsChecked == true);
        foreach (var c in permissoes.Values) c.Click += (_,_) => EstadoTodos();
        todos.Click += (_,_) => { foreach (var c in permissoes.Values) c.IsChecked = todos.IsChecked == true; alterou = true; };
        copiar.Click += (_,_) =>
        {
            if (origem.SelectedItem is not Colaborador c) { erro.Text = "Escolha o colaborador de quem deseja copiar as permissões."; return; }
            foreach (var p in permissoes) p.Value.IsChecked = c.Administrador || (c.Permissoes ?? PermissoesColaborador.Padrao(c.Perfil)).Contains(p.Key);
            EstadoTodos(); alterou = true; erro.Text = "Permissões copiadas. Clique em Salvar para aplicar.";
        };
        pesquisa.TextChanged += (_,_) => { foreach (var p in permissoes.Values) p.Visibility = ((TextBlock)p.Content).Text.Contains(pesquisa.Text.Trim(), StringComparison.CurrentCultureIgnoreCase) ? Visibility.Visible : Visibility.Collapsed; };
        foreach (TabItem aba in abas.Items)
        {
            aba.SetResourceReference(Control.ForegroundProperty, "Ink"); aba.SetResourceReference(Control.BackgroundProperty, "Canvas");
            aba.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='TabItem'><Border x:Name='Aba' Background='{TemplateBinding Background}' BorderBrush='{DynamicResource Line}' BorderThickness='1' Padding='12,7' Margin='0,0,4,0'><ContentPresenter ContentSource='Header' RecognizesAccessKey='True'/></Border><ControlTemplate.Triggers><Trigger Property='IsSelected' Value='True'><Setter TargetName='Aba' Property='Background' Value='{DynamicResource Surface}'/><Setter TargetName='Aba' Property='BorderBrush' Value='{DynamicResource Ink}'/></Trigger><Trigger Property='IsKeyboardFocusWithin' Value='True'><Setter TargetName='Aba' Property='BorderBrush' Value='{DynamicResource Color1F5FBF}'/></Trigger></ControlTemplate.Triggers></ControlTemplate>");
        }
        void AtualizarAcesso()
        {
            acesso.IsEnabled = criar.IsChecked == true;
            abaPermissoes.IsEnabled = criar.IsChecked == true;
            painelPermissoes.IsEnabled = admin.IsChecked != true;
            explicacao.Text = admin.IsChecked == true ? "Administradores têm acesso completo. Desmarque Administrador na aba Geral para limitar as permissões." : "Selecione o que esta pessoa poderá fazer no computador. As funções no estabelecimento não concedem permissões automaticamente.";
        }
        criar.Click += (_,_) => AtualizarAcesso(); admin.Click += (_,_) => AtualizarAcesso(); AtualizarAcesso(); EstadoTodos();
        salvar.Click += (_,_) =>
        {
            try
            {
                if ((!gestor.Autenticado && !servico.Pode("colaboradores")) || (servico.SessaoIndividualIniciada && !servico.Pode("colaboradores"))) throw new InvalidOperationException("Você não tem permissão para gerenciar colaboradores.");
                if (!decimal.TryParse(comissao.Text, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out var percentual)) throw new ArgumentException("Informe a comissão em percentual, por exemplo 2,5.");
                if (criar.IsChecked == true && senha.Password != confirmar.Password) throw new ArgumentException("As senhas não coincidem.");
                if (propria && (criar.IsChecked != true || ativo.IsChecked != true || liberado.IsChecked != true || admin.IsChecked != atual.Administrador || mestra.IsChecked != atual.SenhaMestra || !iniciais.Order().SequenceEqual(permissoes.Where(p=>p.Value.IsChecked==true).Select(p=>p.Key).Order())))
                    throw new InvalidOperationException("Entre com outro administrador para alterar seu próprio acesso e suas permissões.");
                var selecionadas = marcadas.Where(p=>p.Value.IsChecked==true).Select(p=>p.Key).ToArray();
                var perfil = admin.IsChecked == true ? "gestor" : permissoes["cozinha"].IsChecked == true && permissoes.Where(p=>p.Value.IsChecked==true).All(p=>p.Key=="cozinha") ? "cozinha" : "garcom";
                servico.Salvar(atual with { Nome = nome.Text, Cpf = cpf.Text, Telefone = telefone.Telefone, PaisTelefone=telefone.CodigoPais, Email = email.Text, ComissaoPercentual = percentual,
                    Funcoes = selecionadas, Funcao = string.Join(", ", selecionadas.Append(funcao.Text.Trim()).Where(f=>f.Length>0).Distinct()), Login = login.Text, Perfil = perfil,
                    Ativo = ativo.IsChecked == true, CriarUsuario = criar.IsChecked == true, AcessoLiberado = liberado.IsChecked == true, SenhaMestra = mestra.IsChecked == true,
                    Permissoes = permissoes.Where(p=>p.Value.IsChecked==true).Select(p=>p.Key).ToArray() }, string.IsNullOrEmpty(senha.Password) ? null : senha.Password);
                salvo = true; DialogResult = true;
            }
            catch (Exception ex) { erro.Text = ex is ArgumentException or InvalidOperationException ? ex.Message : "Não foi possível salvar. Verifique os dados e tente novamente."; }
        };
        Closing += (_,e) => { if (!salvo && alterou && !new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?", "As alterações não foram salvas. Deseja descartá-las?")) e.Cancel = true; };
        Closed += (_,_) => { senha.Clear(); confirmar.Clear(); }; Loaded += (_,_) => nome.Focus();
    }
}




