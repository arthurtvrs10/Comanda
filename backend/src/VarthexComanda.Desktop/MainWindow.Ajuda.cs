using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private void AbrirAjuda()=>AbrirInformacoes("Ajuda e suporte",()=>
        "VARTHEX COMANDA — AJUDA\n\n"+
        "MESAS / COMANDAS\nAbra Principal e clique em Mesas / comandas. Digite o número para localizar uma comanda e pressione Enter para abrir. Use Opções para atualizar ou juntar pedidos.\n\n"+
        "PRODUTOS\nNa categoria Produtos, cadastre um novo produto ou selecione um existente para editar nome, categoria, preço e imagem.\n\n"+
        "APPS\nCozinha consulta os pedidos abertos. Painel de senhas permite chamar números e exibir o painel no monitor configurado.\n\n"+
        "CONFIGURAÇÕES\nConfigure impressão, colaboradores, numeração e preferências do sistema. Backup e restauração permite preservar os dados locais.\n\n"+
        "PRECISA DE AJUDA?\nAbra Diagnóstico de serviços, atualize e copie o relatório para compartilhar com seu responsável pelo suporte. O sistema não envia dados automaticamente.",false);

    private void AbrirDiagnostico()=>AbrirInformacoes("Diagnóstico de serviços",()=>
        _pathsSistema is null?"O ambiente desta sessão não está disponível.":DiagnosticoSistema.Gerar(_pathsSistema,_configuracaoView.ViewModel.NomeEstabelecimento,_configuracaoView.ConsultarBackup?.Invoke()??"Não disponível"),true);

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
