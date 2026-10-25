using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Rede;

public sealed class JanelaGuiaServidor : Window
{
    public JanelaGuiaServidor(Action? abrirServidor=null, Action? abrirFirewall=null, string? executavel=null)
    {
        Title="Como habilitar o acesso pelo celular";
        Width=Math.Min(860,SystemParameters.WorkArea.Width);Height=Math.Min(740,SystemParameters.WorkArea.Height);
        MinWidth=640;MinHeight=500;DialogoCompacto.Aplicar(this);
        foreach(var tipo in new[] { typeof(Label),typeof(TextBlock) })
        {
            var estilo=new Style(tipo,TryFindResource(tipo) as Style);
            estilo.Setters.Add(new Setter(tipo==typeof(Label)?Control.ForegroundProperty:TextBlock.ForegroundProperty,new DynamicResourceExtension("Ink")));
            Resources[tipo]=estilo;
        }
        var raiz=new DockPanel { Margin=new Thickness(20) };Content=raiz;
        raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");
        var rodape=new StackPanel();DockPanel.SetDock(rodape,Dock.Bottom);raiz.Children.Add(rodape);
        var mensagem=new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,8) };rodape.Children.Add(mensagem);
        var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };
        acoes.Children.Add(new Button { Content=abrirServidor is null?"Voltar ao servidor":"Fechar",IsCancel=true,MinWidth=110,Margin=new Thickness(0,0,8,0) });
        if(abrirServidor is not null)
        {
            var abrir=new Button { Content="Abrir tela do servidor",MinWidth=165 };
            abrir.Click+=(_,_)=>{Close();abrirServidor();};acoes.Children.Add(abrir);
        }
        rodape.Children.Add(acoes);
        var painel=new StackPanel();raiz.Children.Add(new ScrollViewer { Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled });
        void Texto(string texto,double margem=6)=>painel.Children.Add(new TextBlock { Text=texto,TextWrapping=TextWrapping.Wrap,LineHeight=20,Margin=new Thickness(0,0,0,margem) });
        void Secao(string texto)=>painel.Children.Add(new TextBlock { Text=texto,FontSize=15,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,14,0,8) });
        painel.Children.Add(new TextBlock { Text="Como habilitar o acesso pelo celular",FontSize=21,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,10) });
        Texto("Cancelou o aviso de rede do Windows? Você pode liberar o Comanda novamente seguindo estes passos.");
        Secao("1. Inicie o servidor no Comanda");
        Texto("Abra Configurações → Servidor / acesso pelo celular e clique em Iniciar servidor. Se aparecer Servidor em funcionamento, continue no passo 2.");
        Secao("2. Libere o Comanda no Firewall do Windows");
        Texto("• Clique em Abrir o Firewall abaixo. Escolha Permitir um aplicativo ou recurso pelo Firewall do Windows Defender.");
        Texto("• Clique em Alterar configurações e localize Comanda ou VarthexComanda na lista.");
        Texto("• Marque o aplicativo e a coluna Particular ou Privada, para a rede do estabelecimento. Confirme em OK.");
        Texto("• Se o Comanda não aparecer, clique em Permitir outro aplicativo → Procurar e selecione este executável:");
        var caminho=executavel??ObterExecutavel();
        var campo=new TextBox { Text=caminho,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,8,8) };
        System.Windows.Automation.AutomationProperties.SetName(campo,"Executável do Comanda para permitir no Firewall");painel.Children.Add(campo);
        var ferramentas=new WrapPanel { Margin=new Thickness(0,0,0,8) };
        var firewall=new Button { Content="Abrir o Firewall",MinWidth=140,Margin=new Thickness(0,0,8,4) };
        firewall.Click+=(_,_)=>
        {
            try
            {
                if(abrirFirewall is not null)abrirFirewall();
                else Process.Start(new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"control.exe")) { Arguments="/name Microsoft.WindowsFirewall",UseShellExecute=true });
                mensagem.Text="Na janela do Windows, escolha Permitir um aplicativo ou recurso pelo Firewall.";
            }
            catch { mensagem.Text="Abra o menu Iniciar do Windows, pesquise Firewall e escolha Permitir um aplicativo pelo Firewall."; }
        };
        ferramentas.Children.Add(firewall);
        var copiar=new Button { Content="Copiar caminho do EXE",Margin=new Thickness(0,0,8,4) };
        copiar.Click+=(_,_)=>{try{Clipboard.SetText(caminho);mensagem.Text="Caminho copiado. Use-o em Procurar, na janela de permissão do Windows.";}catch{mensagem.Text="Não foi possível copiar. Selecione o caminho acima e use Ctrl+C.";}};ferramentas.Children.Add(copiar);painel.Children.Add(ferramentas);
        Texto("O Firewall permanece ligado. A permissão é para o Comanda receber conexões na rede privada do estabelecimento.");
        Secao("3. Conecte o celular");
        Texto("Conecte o celular ao mesmo Wi-Fi deste computador. Volte à tela do servidor e leia o QR Code atualizado ou abra o link de acesso. Mantenha o computador ligado com o Comanda aberto.");
        var detalhes=new Expander { Header="Liberei o aplicativo, mas ainda não conecta",Margin=new Thickness(0,8,0,0) };
        detalhes.Content=new TextBlock { Text="Confira se a conexão do estabelecimento está definida como rede privada no Windows. Se o Comanda mudou de pasta, confira se você liberou o executável que está usando agora. Se você cancelou o aviso, pode ter ficado uma regra de bloqueio para este executável. Peça ao administrador para revisar as Regras de Entrada nas Configurações avançadas do Firewall. O teste neste PC confirma o servidor local, mas não confirma a liberação para outros aparelhos.",TextWrapping=TextWrapping.Wrap,LineHeight=20,Margin=new Thickness(0,8,8,0) };painel.Children.Add(detalhes);
    }
    private static string ObterExecutavel()
    {
        var local=typeof(App).Assembly.Location;
        var exe=string.IsNullOrEmpty(local)?null:Path.ChangeExtension(local,".exe");
        return exe is not null&&File.Exists(exe)?exe:Environment.ProcessPath??"VarthexComanda.exe";
    }
}
