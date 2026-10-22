using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop;

public sealed class JanelaLogin : Window
{
    public string Perfil { get; private set; } = "gestor";

    public JanelaLogin(AcessoGestor gestor, AcessoAtendimento atendimento, bool somenteGestor = false, ServicoColaboradores? colaboradores = null)
    {
        Title = "Login";
        Width = 420;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        SetResourceReference(BackgroundProperty, "Surface");
        SetResourceReference(ForegroundProperty, "Ink");
        var painel = new StackPanel { Margin = new Thickness(24) };
        Content = painel;
        var marca = new Image { Width = 240, Height = 60, Stretch = System.Windows.Media.Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 24) };
        marca.SetResourceReference(Image.SourceProperty, "MarcaLogo");
        System.Windows.Automation.AutomationProperties.SetName(marca, "Comanda, por Varthex");
        painel.Children.Add(marca);
        painel.Children.Add(new TextBlock { Text = "Entrar no sistema", FontSize = 23, FontWeight = FontWeights.SemiBold });
        painel.Children.Add(new TextBlock { Text = "Informe sua credencial para desbloquear o acesso.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,18) });
        var perfis = new ComboBox { MinHeight = 46, SelectedValuePath = "Tag", DisplayMemberPath = "Content" };
        perfis.Items.Add(new ComboBoxItem { Content = "Gestor", Tag = "gestor" });
        if (colaboradores is not null) perfis.Items.Add(new ComboBoxItem { Content = "Colaborador · login individual", Tag = "colaborador" });
        if (!somenteGestor)
        {
            perfis.Items.Add(new ComboBoxItem { Content = "Garçom", Tag = "garcom" });
            perfis.Items.Add(new ComboBoxItem { Content = "Cozinha", Tag = "cozinha" });
        }
        perfis.SelectedIndex = 0;
        System.Windows.Automation.AutomationProperties.SetName(perfis,"Perfil de acesso");
        painel.Children.Add(new Label { Content="Perfil de acesso", Target=perfis, Padding=new Thickness(0,0,0,6) });
        painel.Children.Add(perfis);
        var login = new TextBox { MinHeight = 36, MaxLength = 60, Visibility = Visibility.Collapsed };
        var rotuloLogin = new Label { Content = "Login do colaborador", Target = login, Visibility = Visibility.Collapsed, Padding = new Thickness(0,12,0,4) };
        System.Windows.Automation.AutomationProperties.SetName(login, "Login do colaborador");
        painel.Children.Add(rotuloLogin);painel.Children.Add(login);
        var senha = new PasswordBox { MinHeight = 46, Padding = new Thickness(8), MaxLength = 128 };
        var rotulo = new Label { Content = "Senha do gestor", Target=senha, Padding = new Thickness(0,16,0,6) };
        System.Windows.Automation.AutomationProperties.SetName(senha,"Senha ou PIN");
        painel.Children.Add(rotulo);
        painel.Children.Add(senha);
        var erro = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,10,0,10) };
        erro.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");
        painel.Children.Add(erro);
        var entrar = new Button { Content = "Entrar", IsDefault = true, MinHeight = 46 };
        entrar.SetResourceReference(StyleProperty, "BotaoPrimario");
        painel.Children.Add(entrar);
        painel.Children.Add(new Button { Content="Cancelar", IsCancel=true, Margin=new Thickness(0,8,0,0) });
        perfis.SelectionChanged += (_,_) => {
            senha.Clear(); erro.Text="";
            var individual = Equals(perfis.SelectedValue,"colaborador");
            login.Visibility = rotuloLogin.Visibility = individual ? Visibility.Visible : Visibility.Collapsed;
            rotulo.Content = individual ? "Senha do colaborador" : Equals(perfis.SelectedValue,"gestor") ? "Senha do gestor" : "PIN da equipe";
            if(individual)login.Focus();else senha.Focus();
        };
        entrar.Click += (_,_) =>
        {
            try
            {
                Perfil = (string)perfis.SelectedValue;
                if (Perfil == "colaborador" && colaboradores is not null)
                {
                    if (!colaboradores.Autenticar(login.Text,senha.Password,out var mensagem)) { erro.Text=mensagem;senha.Clear();senha.Focus();return; }
                    Perfil = colaboradores.SessaoAtual!.Perfil;
                    if(somenteGestor && Perfil != "gestor") { colaboradores.Sair();erro.Text="Esta função exige um colaborador com perfil Gestor.";senha.Clear();return; }
                    var valido = Perfil == "gestor" ? gestor.EntrarColaborador(colaboradores) : atendimento.EntrarColaborador(colaboradores);
                    if (!valido) { colaboradores.Sair();erro.Text="O acesso deste colaborador foi alterado. Entre novamente.";return; }
                }
                else if (Perfil == "gestor")
                {
                    if (!gestor.Configurado)
                    {
                        if (new JanelaAcessoGestor(gestor) { Owner=this }.ShowDialog()!=true) return;
                    }
                    else if (!gestor.Entrar(senha.Password)) { erro.Text="Senha incorreta."; senha.Clear(); senha.Focus(); return; }
                    colaboradores?.Sair();
                }
                else
                {
                    if (!atendimento.Entrar(senha.Password,out var mensagem)) { erro.Text=mensagem; senha.Clear(); senha.Focus(); return; }
                    colaboradores?.Sair();
                }
                DialogResult=true;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { erro.Text=ex.Message; }
            catch (Exception) { erro.Text="Não foi possível verificar o acesso. Tente novamente."; }
        };
        Loaded += (_,_)=>senha.Focus();
    }
}
