using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using Microsoft.Win32;

namespace VarthexComanda.Desktop.Backup;

public sealed class ConfigurarOneDriveView:Window
{
    private readonly BackupViewModel _viewModel;
    private readonly ComboBox _pastas=new(){MinHeight=38,HorizontalContentAlignment=HorizontalAlignment.Stretch};
    private readonly TextBlock _mensagem=new(){TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,0)};
    internal ConfigurarOneDriveView(BackupViewModel viewModel,IReadOnlyList<string>? detectadas=null)
    {
        _viewModel=viewModel;Title="Backup no OneDrive";Width=520;Height=560;MinWidth=420;MinHeight=470;ShowInTaskbar=false;WindowStartupLocation=WindowStartupLocation.CenterOwner;FontSize=13;
        SetResourceReference(BackgroundProperty,"Canvas");SetResourceReference(ForegroundProperty,"Ink");
        var raiz=new Grid {Margin=new Thickness(28,22,28,22)};raiz.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});raiz.RowDefinitions.Add(new(){Height=GridLength.Auto});raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");
        var painel=new StackPanel();raiz.Children.Add(new ScrollViewer {Content=painel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled});
        painel.Children.Add(IconesVarthex.Criar(IconeVarthex.OneDrive,52));
        painel.Children.Add(new TextBlock {Text="Backup no OneDrive",FontSize=23,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,16,0,10)});
        Texto(painel,"Use a pasta do OneDrive conectada ao Windows para guardar uma cópia dos dados e das fotos.");
        Texto(painel,"Uma cópia local continuará neste computador. O VComanda grava os backups em Varthex\\Backups; o aplicativo OneDrive faz a sincronização com a nuvem.");
        painel.Children.Add(new RotuloAjuda {Texto="Pasta do OneDrive *",Margin=new Thickness(0,10,0,6)});
        AutomationProperties.SetName(_pastas,"Pasta do OneDrive");_pastas.ToolTip="Confirme que esta pasta pertence à conta do OneDrive que deseja utilizar.";
        var encontradas=(detectadas??PastasOneDrive.Detectar()).ToList();
        if(viewModel.OneDriveConfigurado&&!encontradas.Contains(viewModel.PastaOneDrive,StringComparer.OrdinalIgnoreCase))encontradas.Insert(0,viewModel.PastaOneDrive);
        foreach(var pasta in encontradas)_pastas.Items.Add(pasta);
        if(_pastas.Items.Count>0)_pastas.SelectedItem=viewModel.OneDriveConfigurado?viewModel.PastaOneDrive:_pastas.Items[0];
        painel.Children.Add(_pastas);AutomationProperties.SetIsRequiredForForm(_pastas,true);
        AutomationProperties.SetLiveSetting(_mensagem,AutomationLiveSetting.Polite);painel.Children.Add(_mensagem);
        var escolher=new Button {Content="Escolher outra pasta",MinHeight=36,Margin=new Thickness(0,8,0,12),HorizontalAlignment=HorizontalAlignment.Left};escolher.Click+=(_,_)=>Escolher();painel.Children.Add(escolher);
        Texto(painel,"Se o OneDrive ainda não estiver configurado, abra-o no menu Iniciar, entre com sua conta Microsoft e volte para escolher a pasta sincronizada.");
        Texto(painel,"Confira o envio pelo ícone do OneDrive no Windows. Uma cópia gravada na pasta pode ainda estar aguardando sincronização.");
        if(encontradas.Count==0)_mensagem.Text="Não encontramos uma pasta do OneDrive neste computador. Você pode escolhê-la ou configurar depois.";
        var acoes=new WrapPanel {HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,18,0,0)};Grid.SetRow(acoes,1);raiz.Children.Add(acoes);
        var depois=new Button {Content="Configurar depois",MinHeight=38,Margin=new Thickness(0,0,8,0)};depois.Click+=(_,_)=>Close();acoes.Children.Add(depois);
        var usar=new Button {Content="Usar OneDrive",MinHeight=38,IsDefault=true};usar.SetResourceReference(StyleProperty,"BotaoPrimario");usar.Click+=(_,_)=>Salvar();acoes.Children.Add(usar);
        Content=raiz;
    }
    private static void Texto(Panel painel,string mensagem)=>painel.Children.Add(new TextBlock {Text=mensagem,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)});
    private void Escolher()
    {
        var dialogo=new OpenFolderDialog {Title="Escolha a pasta sincronizada pelo OneDrive"};if(dialogo.ShowDialog(this)!=true)return;
        if(!_pastas.Items.Contains(dialogo.FolderName))_pastas.Items.Add(dialogo.FolderName);_pastas.SelectedItem=dialogo.FolderName;
    }
    private void Salvar()
    {
        if(_pastas.SelectedItem is not string pasta){_mensagem.Text="Escolha a pasta sincronizada pelo OneDrive para continuar.";return;}
        _viewModel.ConfigurarPasta("onedrive",pasta);
        if(_viewModel.MensagemErro){_mensagem.Text=_viewModel.Mensagem;return;}
        if(!_viewModel.OneDriveConfigurado){_mensagem.Text="Não foi possível salvar agora. Aguarde o backup em andamento e tente novamente.";return;}
        DialogResult=true;
    }
}
