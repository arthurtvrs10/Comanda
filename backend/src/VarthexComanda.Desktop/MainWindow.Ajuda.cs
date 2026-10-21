using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private Ajuda.JanelaSuporte? _janelaSuporte;
    private void AbrirAjuda()=>AbrirSuporte();
    private void AjudaFlutuante_Click(object sender, RoutedEventArgs e)
    {
        if (_janelaSuporte is { IsVisible: true })
        {
            _janelaSuporte.Close();
            return;
        }
        ExecutarDestino(_destinosRibbon.Single(d => d.Categoria == "Ajuda" && d.Nome == "Ajuda"));
    }
    private void AtualizarBotaoAjuda()
    {
        var aberta = _janelaSuporte is { IsVisible: true };
        BotaoAjudaFlutuante.Content = IconesVarthex.Criar(aberta ? IconeVarthex.FecharAjuda : IconeVarthex.Ajuda, 32);
        BotaoAjudaFlutuante.ToolTip = aberta ? "Fechar ajuda" : "Opções de ajuda";
        System.Windows.Automation.AutomationProperties.SetName(BotaoAjudaFlutuante, aberta ? "Fechar ajuda" : "Abrir opções de ajuda");
    }
    private void AbrirSuporte()
    {
        if (_janelaSuporte is { IsVisible: true })
        {
            _janelaSuporte.Activate();
            return;
        }
        var janela = new Ajuda.JanelaSuporte(
            treinamento: AbrirTreinamento,
            ocultarBotao: () => BotaoAjudaFlutuante.Visibility = Visibility.Collapsed,
            ancora: BotaoAjudaFlutuante) { Owner=this };
        _janelaSuporte = janela;
        janela.Closed += (_, _) =>
        {
            if (ReferenceEquals(_janelaSuporte, janela))
            {
                _janelaSuporte = null;
                AtualizarBotaoAjuda();
            }
        };
        janela.Show();
        AtualizarBotaoAjuda();
    }
    private void AbrirFeedback()=>new Ajuda.JanelaFeedback { Owner=this }.ShowDialog();
    private void AbrirTreinamento()=>AbrirTreinamento(this);
    private void AbrirTreinamento(Window owner)
    {
        var anterior=_categoriaRibbon;
        try{new Ajuda.JanelaTreinamento(SelecionarCategoriaRibbon) { Owner=owner }.ShowDialog();}
        finally{SelecionarCategoriaRibbon(anterior);}
    }
    private void AbrirDiagnostico()
    {
        if(_pathsSistema is null)return;
        new Ajuda.JanelaDiagnosticoServicos(()=>Ajuda.DiagnosticoServicos.Avaliar(_pathsSistema,()=>
        {
            try{new Licenciamento.ServicoLicenca().Verificar();return true;}catch{return false;}
        })) { Owner=this }.ShowDialog();
    }
    private void AbrirInformacoes(string titulo,Func<string> obter,bool diagnostico)
    {
        var janela=new Window { Title=titulo,Owner=this,Width=760,Height=610,MinWidth=520,MinHeight=380,ResizeMode=ResizeMode.CanResize };
        DialogoCompacto.Aplicar(janela);
        var raiz=new DockPanel { Margin=new Thickness(14) };janela.Content=raiz;
        var acoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,10,0,0) };DockPanel.SetDock(acoes,Dock.Bottom);raiz.Children.Add(acoes);
        var texto=new TextBox { Text=obter(),IsReadOnly=true,AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,VerticalContentAlignment=VerticalAlignment.Top,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,FontFamily=new FontFamily(diagnostico?"Consolas":"Segoe UI"),FontSize=12 };
        if(diagnostico){var atualizar=new Button { Content="Atualizar",Padding=new Thickness(12,5,12,5),Margin=new Thickness(0,0,8,0) };atualizar.Click+=(_,_)=>texto.Text=obter();acoes.Children.Add(atualizar);}
        var copiar=new Button { Content="Copiar informações",Padding=new Thickness(12,5,12,5),Margin=new Thickness(0,0,8,0) };copiar.Click+=(_,_)=>{try{Clipboard.SetText(texto.Text);}catch{JanelaAviso.Mostrar("Copiar","Não foi possível copiar. Tente novamente.",TipoAviso.Aviso);}};acoes.Children.Add(copiar);
        var fechar=new Button { Content="Fechar",IsCancel=true,Padding=new Thickness(12,5,12,5) };fechar.Click+=(_,_)=>janela.Close();acoes.Children.Add(fechar);
        raiz.Children.Add(texto);janela.ShowDialog();
    }
}
