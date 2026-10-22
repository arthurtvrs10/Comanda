using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VarthexComanda.Desktop;

public partial class BloqueioAtendimentoView : UserControl
{
    private readonly AcessoAtendimento _acesso;
    private readonly AcessoGestor? _gestor;
    private readonly ServicoColaboradores? _colaboradores;
    private bool _configurando;
    private string? _primeiroPin;
    public string Perfil { get; private set; } = "garcom";
    public event Action? Desbloqueado;

    public BloqueioAtendimentoView(AcessoAtendimento acesso,AcessoGestor? gestor=null,ServicoColaboradores? colaboradores=null)
    {
        InitializeComponent();_acesso=acesso;_gestor=gestor;_colaboradores=colaboradores;
        Reiniciar();Loaded+=(_,_)=>Login.Focus();
    }
    public void Reiniciar()
    {
        _configurando=false;_primeiroPin=null;Login.Clear();LimparSenha();Mensagem.Text="";
        Titulo.Text="Sistema bloqueado";Instrucao.Text="Informe seu login e sua senha para continuar.";
        CamposLogin.Visibility=Visibility.Visible;RotuloSenha.Content="Senha";
        BotaoEntrar.Content="Entrar no sistema";BotaoEntrar.IsEnabled=true;BotaoConfigurar.Visibility=Visibility.Collapsed;
    }
    public void IniciarConfiguracao()
    {
        Reiniciar();_configurando=true;CamposLogin.Visibility=Visibility.Collapsed;
        Titulo.Text="Definir PIN da equipe";Instrucao.Text="Escolha de 4 a 8 números para a equipe.";
        RotuloSenha.Content="Novo PIN";BotaoEntrar.Content="Continuar";BotaoConfigurar.Visibility=Visibility.Visible;Senha.Focus();
    }
    private void LimparSenha(){Senha.Clear();SenhaVisivel.Clear();MostrarSenha.IsChecked=false;}
    private void MostrarSenha_Click(object sender,RoutedEventArgs e)
    {
        if(SenhaVisivel is null||Senha is null)return;
        if(MostrarSenha.IsChecked==true){SenhaVisivel.Text=Senha.Password;Senha.Visibility=Visibility.Collapsed;SenhaVisivel.Visibility=Visibility.Visible;}
        else {Senha.Password=SenhaVisivel.Text;SenhaVisivel.Clear();SenhaVisivel.Visibility=Visibility.Collapsed;Senha.Visibility=Visibility.Visible;}
        AtualizarPlaceholder();
    }
    private void Senha_Changed(object sender,RoutedEventArgs e)=>AtualizarPlaceholder();
    private void SenhaVisivel_Changed(object sender,TextChangedEventArgs e)=>AtualizarPlaceholder();
    private void AtualizarPlaceholder()
    {
        if(PlaceholderSenha is null||MostrarSenha is null)return;
        var vazia=MostrarSenha.IsChecked==true?string.IsNullOrEmpty(SenhaVisivel.Text):string.IsNullOrEmpty(Senha.Password);
        PlaceholderSenha.Visibility=vazia?Visibility.Visible:Visibility.Collapsed;
    }
    private void Recuperar_Click(object sender,RoutedEventArgs e)
    {
        if(_gestor is null||!_gestor.RecuperacaoConfigurada){Mensagem.Text="O gestor ainda não configurou uma senha de recuperação. Se você é colaborador, peça ao gestor para redefinir sua senha.";return;}
        if(new JanelaRecuperacaoGestor(_gestor,true,Window.GetWindow(this)).ShowDialog()==true)
        {Reiniciar();Login.Text="gestor";Mensagem.Text="Senha redefinida. Entre com a nova senha.";Senha.Focus();}
    }
    private void Configurar_Click(object sender,RoutedEventArgs e){Reiniciar();}
    private void Entrar_Click(object sender,RoutedEventArgs e)=>Confirmar();
    private void Confirmar()
    {
        var senha=MostrarSenha.IsChecked==true?SenhaVisivel.Text:Senha.Password;
        try
        {
            if(_configurando)
            {
                if(!AcessoAtendimento.PinValido(senha)){Mensagem.Text="Use de 4 a 8 números.";return;}
                if(_primeiroPin is null){_primeiroPin=senha;LimparSenha();Instrucao.Text="Repita o PIN para confirmar.";BotaoEntrar.Content="Salvar PIN";return;}
                if(_primeiroPin!=senha){LimparSenha();Mensagem.Text="Os PINs não coincidem. Tente novamente.";return;}
                _acesso.DefinirPin(senha);Reiniciar();Mensagem.Text="PIN salvo.";return;
            }
            var login=Login.Text.Trim();
            if(login.Length==0||senha.Length==0){Mensagem.Text="Informe o login e a senha.";return;}
            bool valido=false;
            // Contas individuais têm prioridade, inclusive quando usam nomes de perfis.
            if(_colaboradores is not null&&_colaboradores.Listar().Any(c=>c.Login.Equals(login,StringComparison.OrdinalIgnoreCase)))
            {
                if(!_colaboradores.Autenticar(login,senha,out var erro)){Falha(erro);return;}
                Perfil=_colaboradores.SessaoAtual!.Perfil;
                valido=Perfil=="gestor"?_gestor?.EntrarColaborador(_colaboradores)==true:_acesso.EntrarColaborador(_colaboradores);
            }
            else if(login.Equals("gestor",StringComparison.OrdinalIgnoreCase)&&_gestor?.Configurado==true)
            {valido=_gestor.Entrar(senha);Perfil="gestor";}
            else if(_colaboradores is not null){_colaboradores.Autenticar(login,senha,out _);}
            if(!valido){Falha("Login ou senha incorretos, ou usuário inativo.");return;}
            LimparSenha();Mensagem.Text="";Desbloqueado?.Invoke();
        }
        catch{_acesso.Bloquear();_gestor?.Sair();_colaboradores?.Sair();Falha("Não foi possível verificar o acesso. Tente novamente.");}
    }
    private void Falha(string texto){LimparSenha();Mensagem.Text=texto;Senha.Focus();}
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if(e.Key==Key.Enter){Confirmar();e.Handled=true;}
    }
}
