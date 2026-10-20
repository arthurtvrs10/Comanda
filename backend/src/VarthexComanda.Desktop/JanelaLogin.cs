using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop;

public sealed class JanelaLogin : Window
{
    public string Perfil { get; private set; } = "gestor";

    public JanelaLogin(AcessoGestor gestor, AcessoAtendimento atendimento, bool somenteGestor = false)
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
        painel.Children.Add(new TextBlock { Text = "Entrar no sistema", FontSize = 23, FontWeight = FontWeights.SemiBold });
        painel.Children.Add(new TextBlock { Text = "Informe sua credencial para desbloquear o acesso.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,18) });
        var perfis = new ComboBox { MinHeight = 46, SelectedValuePath = "Tag", DisplayMemberPath = "Content" };
        perfis.Items.Add(new ComboBoxItem { Content = "Gestor", Tag = "gestor" });
        if (!somenteGestor)
        {
            perfis.Items.Add(new ComboBoxItem { Content = "Garçom", Tag = "garcom" });
            perfis.Items.Add(new ComboBoxItem { Content = "Cozinha", Tag = "cozinha" });
        }
        perfis.SelectedIndex = 0;
        System.Windows.Automation.AutomationProperties.SetName(perfis,"Perfil de acesso");
        painel.Children.Add(new Label { Content="Perfil de acesso", Target=perfis, Padding=new Thickness(0,0,0,6) });
        painel.Children.Add(perfis);
        var senha = new PasswordBox { MinHeight = 46, Padding = new Thickness(8), MaxLength = 128 };
        var rotulo = new Label { Content = "Senha do gestor", Target=senha, Padding = new Thickness(0,16,0,6) };
        System.Windows.Automation.AutomationProperties.SetName(senha,"Senha ou PIN");
        painel.Children.Add(rotulo);
        painel.Children.Add(senha);
        var erro = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,10,0,10) };
        erro.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");
        painel.Children.Add(erro);
        var entrar = new Button { Content = "Entrar", IsDefault = true, MinHeight = 46 };
        painel.Children.Add(entrar);
        painel.Children.Add(new Button { Content="Cancelar", IsCancel=true, Margin=new Thickness(0,8,0,0) });
        perfis.SelectionChanged += (_,_) => { senha.Clear(); erro.Text=""; rotulo.Content=Equals(perfis.SelectedValue,"gestor")?"Senha do gestor":"PIN da equipe"; senha.Focus(); };
        entrar.Click += (_,_) =>
        {
            try
            {
                Perfil = (string)perfis.SelectedValue;
                if (Perfil == "gestor")
                {
                    if (!gestor.Configurado)
                    {
                        if (new JanelaAcessoGestor(gestor) { Owner=this }.ShowDialog()!=true) return;
                    }
                    else if (!gestor.Entrar(senha.Password)) { erro.Text="Senha incorreta."; senha.Clear(); senha.Focus(); return; }
                }
                else if (!atendimento.Entrar(senha.Password,out var mensagem)) { erro.Text=mensagem; senha.Clear(); senha.Focus(); return; }
                DialogResult=true;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { erro.Text=ex.Message; }
            catch (Exception) { erro.Text="Não foi possível verificar o acesso. Tente novamente."; }
        };
        Loaded += (_,_)=>senha.Focus();
    }
}
