using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Ajuda;
public static class CanaisAjuda
{
    public const string Telefone="(61) 98553-2004";
    public const string RustDesk="https://rustdesk.com/";
    public static string Whatsapp(string mensagem)=>"https://wa.me/5561985532004?text="+Uri.EscapeDataString(mensagem);
    public static void Abrir(string url)=>Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
}
public sealed class JanelaFeedback : Window
{
    public JanelaFeedback(Action<string>? abrir=null)
    {
        abrir??=CanaisAjuda.Abrir;Title="Ideias e sugestões";Width=590;Height=440;MinWidth=470;MinHeight=390;DialogoCompacto.Aplicar(this);
        var raiz=new DockPanel { Margin=new Thickness(20) };Content=raiz;
        var rodape=new StackPanel { Margin=new Thickness(0,12,0,0) };var mensagem=new TextBlock { TextWrapping=TextWrapping.Wrap };rodape.Children.Add(mensagem);
        var botoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,10,0,0) };var copiar=new Button { Content="Copiar feedback",Margin=new Thickness(0,0,8,0) };var enviar=new Button { Content="Enviar pelo WhatsApp" };botoes.Children.Add(copiar);botoes.Children.Add(enviar);rodape.Children.Add(botoes);DockPanel.SetDock(rodape,Dock.Bottom);raiz.Children.Add(rodape);
        var topo=new StackPanel();topo.Children.Add(new TextBlock { Text="Conte sua ideia",FontSize=20,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,12) });
        var tipo=new ComboBox { ItemsSource=new[]{"Ideia","Sugestão","Problema"},SelectedIndex=1,Margin=new Thickness(0,3,0,12) };topo.Children.Add(new Label { Content="Tipo de feedback",Target=tipo,Padding=new Thickness(0) });topo.Children.Add(tipo);
        topo.Children.Add(new TextBlock { Text="Descreva sua sugestão ou o que aconteceu (10 a 2000 caracteres).",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,5) });DockPanel.SetDock(topo,Dock.Top);raiz.Children.Add(topo);
        var campo=new TextBox { AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,VerticalContentAlignment=VerticalAlignment.Top,MaxLength=2000 };System.Windows.Automation.AutomationProperties.SetName(campo,"Mensagem de feedback");raiz.Children.Add(campo);
        string? Texto(){if(campo.Text.Trim().Length<10){mensagem.Text="Escreva pelo menos 10 caracteres para descrever seu feedback.";campo.Focus();return null;}return $"Varthex Comanda {VersaoDoAplicativo.Atual} — {tipo.SelectedItem}\n\n{campo.Text.Trim()}";}
        copiar.Click+=(_,_)=>{var texto=Texto();if(texto is null)return;try{Clipboard.SetText(texto);mensagem.Text="Feedback copiado.";}catch{mensagem.Text="Não foi possível copiar. Tente novamente.";}};
        enviar.Click+=(_,_)=>{var texto=Texto();if(texto is null)return;try{abrir(CanaisAjuda.Whatsapp(texto));mensagem.Text="Revise a mensagem no WhatsApp e confirme o envio por lá.";}catch{mensagem.Text="Não foi possível abrir o WhatsApp. Você pode copiar o feedback e enviá-lo ao suporte.";}};
    }
}
public sealed record EtapaTreinamento(string Categoria,string Titulo,string Texto);
public sealed class JanelaTreinamento : Window
{
    public static IReadOnlyList<EtapaTreinamento> Etapas { get; } = new EtapaTreinamento[]
    {
        new("Principal","Conheça a área de trabalho","Use as categorias no topo para encontrar os comandos. As janelas internas podem ser movidas, maximizadas e fechadas sem sair do sistema."),
        new("Principal","Abra mesas e comandas","Clique em Mesas / comandas. Digite o número no teclado para localizar uma comanda e pressione Enter para abrir. Em Opções, você pode atualizar a grade ou juntar pedidos."),
        new("Produtos","Cadastre e edite produtos","Clique em Produtos para abrir a listagem. Use Novo para cadastrar. Para editar, selecione uma linha e clique em Editar, ou dê dois cliques no produto. Confira os valores antes de salvar."),
        new("Apps","Acompanhe a cozinha","Abra Cozinha para consultar comandas abertas e seus itens. O filtro de local ajuda a separar os pedidos por cozinha."),
        new("Apps","Chame as senhas","No Painel de senhas, escolha o monitor, digite o número e clique em Chamar. A tela pública mostra o número atual e mantém as chamadas anteriores à direita."),
        new("Configurações","Prepare o sistema","Ajuste impressoras, colaboradores e o número inicial das comandas. Consulte Backup e restauração para preservar seus dados."),
        new("Ajuda","Encontre ajuda quando precisar","Aqui ficam a ajuda, o WhatsApp do suporte, o download do RustDesk, o feedback e o diagnóstico de funcionamento. Você pode repetir este treinamento sempre que quiser.")
    };
    private int _indice;
    public int EtapaAtual=>_indice;
    public JanelaTreinamento(Action<string>? mostrarCategoria=null)
    {
        Title="Show training · Tutorial guiado";Width=570;Height=350;MinWidth=460;MinHeight=320;DialogoCompacto.Aplicar(this);
        var raiz=new DockPanel { Margin=new Thickness(22) };Content=raiz;
        var botoes=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };
        var sair=new Button { Content="Fechar",IsCancel=true,Margin=new Thickness(0,0,8,0) };sair.Click+=(_,_)=>Close();botoes.Children.Add(sair);
        var anterior=new Button { Content="Anterior",Margin=new Thickness(0,0,8,0) };var proximo=new Button { Content="Próximo" };botoes.Children.Add(anterior);botoes.Children.Add(proximo);DockPanel.SetDock(botoes,Dock.Bottom);raiz.Children.Add(botoes);
        var conteudo=new StackPanel();var progresso=new TextBlock { Margin=new Thickness(0,0,0,14) };conteudo.Children.Add(progresso);
        var titulo=new TextBlock { FontSize=21,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12) };conteudo.Children.Add(titulo);var texto=new TextBlock { TextWrapping=TextWrapping.Wrap,FontSize=14,LineHeight=22 };conteudo.Children.Add(texto);raiz.Children.Add(new ScrollViewer { Content=conteudo,VerticalScrollBarVisibility=ScrollBarVisibility.Auto });
        void Mostrar(){var etapa=Etapas[_indice];progresso.Text=$"Etapa {_indice+1} de {Etapas.Count}";titulo.Text=etapa.Titulo;texto.Text=etapa.Texto;anterior.IsEnabled=_indice>0;proximo.Content=_indice==Etapas.Count-1?"Concluir":"Próximo";mostrarCategoria?.Invoke(etapa.Categoria);}
        anterior.Click+=(_,_)=>{if(_indice>0){_indice--;Mostrar();}};proximo.Click+=(_,_)=>{if(_indice==Etapas.Count-1)Close();else{_indice++;Mostrar();}};Mostrar();
    }
}
