using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Threading;

namespace VarthexComanda.Desktop.RecuperacaoEmail;

public sealed class JanelaRecuperacaoEmail : Window
{
    public JanelaRecuperacaoEmail(AcessoGestor gestor,bool cadastrar,Window owner)
        :this(gestor,cadastrar,owner,gestor.CriarRecuperacaoEmail) { }
    internal JanelaRecuperacaoEmail(AcessoGestor gestor,bool cadastrar,Window owner,Func<ClienteRecuperacaoEmail?> criarCliente)
    {
        Title=cadastrar?"E-mail de recuperação":"Recuperar senha do gestor";
        Owner=owner;Width=480;MaxHeight=SystemParameters.WorkArea.Height;SizeToContent=SizeToContent.Height;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;ResizeMode=ResizeMode.NoResize;ShowInTaskbar=false;
        SetResourceReference(BackgroundProperty,"Surface");SetResourceReference(ForegroundProperty,"Ink");
        Configuracao.DialogoCompacto.Aplicar(this);
        var cts=new CancellationTokenSource();var painel=new StackPanel {Margin=new Thickness(24)};
        var scroll=new ScrollViewer {Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        scroll.SetResourceReference(Control.BackgroundProperty,"Surface");Content=scroll;
        TextBlock Texto(string text)
        {
            var t=new TextBlock {Text=text,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)};
            painel.Children.Add(t);return t;
        }
        var descricao=Texto(cadastrar?"Verifique um e-mail para recuperar o acesso do gestor neste computador. A senha de recuperação offline continuará disponível.":"Receba um código no e-mail verificado para escolher uma nova senha. Você precisa estar conectado à internet.");
        var camposCadastro=new StackPanel();painel.Children.Add(camposCadastro);
        var camposCodigo=new StackPanel {Visibility=Visibility.Collapsed};
        var camposSenha=new StackPanel {Visibility=Visibility.Collapsed};
        static void Campo(Panel panel,string title,Control input)
        {
            var label=new Label {Content=new RotuloAjuda {Texto=title+" *"},Target=input,Padding=new Thickness(0,8,0,5)};
            panel.Children.Add(label);input.MinHeight=38;panel.Children.Add(input);
            AutomationProperties.SetName(input,title);AutomationProperties.SetIsRequiredForForm(input,true);
        }
        var atual=new PasswordBox {MaxLength=128};var email=new TextBox {MaxLength=254};
        if(cadastrar){Campo(camposCadastro,"Senha atual do gestor",atual);Campo(camposCadastro,"E-mail de recuperação",email);}
        var enviar=new Button {Content="Enviar código",MinHeight=40,Margin=new Thickness(0,16,0,12),IsDefault=true};painel.Children.Add(enviar);
        painel.Children.Add(camposCodigo);
        var codigo=new TextBox {MaxLength=6,InputScope=new System.Windows.Input.InputScope {Names={new System.Windows.Input.InputScopeName(System.Windows.Input.InputScopeNameValue.Number)}}};
        Campo(camposCodigo,"Código de seis números",codigo);
        var validar=new Button {Content="Verificar código",MinHeight=40,Margin=new Thickness(0,16,0,12)};camposCodigo.Children.Add(validar);
        painel.Children.Add(camposSenha);
        var nova=new PasswordBox {MaxLength=128};var confirmar=new PasswordBox {MaxLength=128};
        Campo(camposSenha,"Nova senha (4 a 128 caracteres)",nova);Campo(camposSenha,"Confirmar nova senha",confirmar);
        var salvar=new Button {Content="Redefinir senha",MinHeight=40,Margin=new Thickness(0,16,0,12)};salvar.SetResourceReference(StyleProperty,"BotaoPrimario");camposSenha.Children.Add(salvar);
        var status=Texto("");status.SetResourceReference(TextBlock.ForegroundProperty,"Ink");AutomationProperties.SetLiveSetting(status,AutomationLiveSetting.Polite);
        var offline=new Button {Content="Usar senha de recuperação offline",MinHeight=36,Margin=new Thickness(0,8,0,0),Visibility=cadastrar?Visibility.Collapsed:Visibility.Visible};painel.Children.Add(offline);
        var cancelar=new Button {Content="Voltar",IsCancel=true,MinHeight=36,Margin=new Thickness(0,8,0,0)};painel.Children.Add(cancelar);
        ClienteRecuperacaoEmail? cliente=null;string? grant=null;var ocupado=false;
        try {cliente=criarCliente();}
        catch(InvalidOperationException ex){status.Text=ex.Message;}
        if(cliente is null)
        {
            if(status.Text.Length==0)status.Text="A recuperação por e-mail ainda não foi habilitada nesta versão. A senha de recuperação offline continua disponível.";
            camposCadastro.Visibility=enviar.Visibility=Visibility.Collapsed;
        }
        else if(!cadastrar&&!cliente.Verificado)
        {
            status.Text="Não há e-mail verificado neste computador. Use a senha de recuperação offline e cadastre seu e-mail em Configurações > Segurança.";
            enviar.Visibility=Visibility.Collapsed;
        }
        else if(cliente.Verificado)descricao.Text=(cadastrar?"E-mail verificado: ":"O código será enviado para ")+cliente.EmailMascarado+(cadastrar?". Para trocar, confirme a senha atual e verifique o novo endereço.":". Não compartilhe o código.");
        status.Visibility=status.Text.Length==0?Visibility.Collapsed:Visibility.Visible;
        void AtualizarEnvio()
        {
            var segundos=(int)Math.Ceiling(cliente?.EsperaReenvio.TotalSeconds??0);
            enviar.IsEnabled=!ocupado&&segundos==0;
            enviar.Content=segundos>0?$"Reenviar em {segundos}s":camposCodigo.Visibility==Visibility.Visible?"Reenviar código":"Enviar código";
        }
        async Task Executar(Func<Task> acao)
        {
            if(ocupado)return;ocupado=true;status.Visibility=Visibility.Visible;status.Text="Aguarde…";
            camposCadastro.IsEnabled=camposCodigo.IsEnabled=camposSenha.IsEnabled=offline.IsEnabled=false;AtualizarEnvio();
            try {await acao();}
            catch(OperationCanceledException) when(cts.IsCancellationRequested) { }
            catch(Exception ex) when(ex is ArgumentException or InvalidOperationException){if(!cts.IsCancellationRequested){status.Text=ex.Message;status.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");}}
            catch{if(!cts.IsCancellationRequested){status.Text="Não foi possível concluir. Tente novamente ou use a recuperação offline.";status.SetResourceReference(TextBlock.ForegroundProperty,"TextB22222");}}
            finally{ocupado=false;if(!cts.IsCancellationRequested){camposCadastro.IsEnabled=camposCodigo.IsEnabled=camposSenha.IsEnabled=offline.IsEnabled=true;AtualizarEnvio();}}
        }
        enviar.Click+=async(_,_)=>await Executar(async()=>
        {
            var desafio=cadastrar?await cliente!.SolicitarCadastroAsync(atual.Password,email.Text,cts.Token):await cliente!.SolicitarRecuperacaoAsync(cts.Token);
            if(cts.IsCancellationRequested)return;
            camposCodigo.Visibility=Visibility.Visible;camposSenha.Visibility=Visibility.Collapsed;grant=null;codigo.Clear();
            status.SetResourceReference(TextBlock.ForegroundProperty,"Ink");status.Text=$"Código enviado para {desafio.MaskedEmail}. Válido por dez minutos. Confira também a pasta de spam.";
            enviar.IsDefault=false;validar.IsDefault=true;codigo.Focus();
        });
        validar.Click+=async(_,_)=>await Executar(async()=>
        {
            if(cadastrar)
            {
                await cliente!.ConfirmarCadastroAsync(codigo.Text.Trim(),cts.Token);
                if(!cts.IsCancellationRequested){DialogResult=true;}
            }
            else
            {
                grant=await cliente!.ConfirmarCodigoAsync(codigo.Text.Trim(),cts.Token);
                if(cts.IsCancellationRequested)return;
                camposCodigo.Visibility=enviar.Visibility=Visibility.Collapsed;camposSenha.Visibility=Visibility.Visible;
                validar.IsDefault=false;salvar.IsDefault=true;status.SetResourceReference(TextBlock.ForegroundProperty,"Ink");status.Text="Código verificado. Escolha sua nova senha de acesso.";nova.Focus();
            }
        });
        salvar.Click+=async(_,_)=>await Executar(()=>
        {
            if(nova.Password!=confirmar.Password)throw new ArgumentException("As senhas não coincidem.");
            cliente!.RedefinirSenha(grant!,nova.Password);DialogResult=true;return Task.CompletedTask;
        });
        offline.Click+=(_,_)=>
        {
            if(!gestor.RecuperacaoConfigurada){status.Visibility=Visibility.Visible;status.Text="Este gestor ainda não possui uma senha de recuperação offline. Contate o suporte.";return;}
            if(new JanelaRecuperacaoGestor(gestor,true,this).ShowDialog()==true)DialogResult=true;
        };
        var timer=new DispatcherTimer {Interval=TimeSpan.FromSeconds(1)};timer.Tick+=(_,_)=>AtualizarEnvio();timer.Start();
        Closed+=(_,_)=>{cts.Cancel();timer.Stop();atual.Clear();nova.Clear();confirmar.Clear();codigo.Clear();grant=null;};
        Loaded+=(_,_)=>{if(cadastrar&&cliente is not null)atual.Focus();else if(enviar.Visibility==Visibility.Visible)enviar.Focus();else cancelar.Focus();};
    }
}
