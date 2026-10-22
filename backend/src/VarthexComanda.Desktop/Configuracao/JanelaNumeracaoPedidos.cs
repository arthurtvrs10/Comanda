using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class JanelaNumeracaoPedidos : Window
{
    private readonly NumeracaoComandasService _service;
    private readonly ConfiguracaoNumeracaoComandas _configuracao;
    private readonly TextBox _inicio,_quantidade;
    private readonly Func<int> _maiorComanda;
    private readonly CheckBox _ativa,_senha;
    private readonly TextBlock _mensagem,_previa;
    private bool _salvou;
    public bool TemAlteracoes=>!_salvou&&(_ativa.IsChecked!=_configuracao.Ativa||_senha.IsChecked!=_configuracao.UsarComoSenhaAtendimento||_quantidade.Text!=(_service.Quantidade?.ToString(CultureInfo.InvariantCulture)??"")||_inicio.Text!=_configuracao.Inicio.ToString(CultureInfo.InvariantCulture));
    public JanelaNumeracaoPedidos(NumeracaoComandasService service,Func<int>? maiorComanda=null)
    {
        _maiorComanda=maiorComanda??(()=>0);_service=service;_configuracao=service.Carregar();Title="Numeração das comandas";Width=560;Height=480;MinWidth=470;MinHeight=350;ResizeMode=ResizeMode.CanResize;
        DialogoCompacto.Aplicar(this);
        var raiz=new DockPanel { Margin=new Thickness(20,16,20,12) };raiz.SetResourceReference(Panel.BackgroundProperty,"Canvas");Content=raiz;
        var rodape=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,12,0,0) };
        var voltar=new Button { Content="Voltar",IsCancel=true,MinWidth=94,Margin=new Thickness(0,0,10,0) };var salvar=new Button { Content="Salvar",MinWidth=100 };salvar.Click+=(_,_)=>Salvar();rodape.Children.Add(voltar);rodape.Children.Add(salvar);DockPanel.SetDock(rodape,Dock.Bottom);raiz.Children.Add(rodape);
        var conteudo=new StackPanel();raiz.Children.Add(new ScrollViewer { Content=conteudo,VerticalScrollBarVisibility=ScrollBarVisibility.Auto });
        _ativa=new CheckBox { Content="Definir o número inicial das mesas / comandas",IsChecked=_configuracao.Ativa,Margin=new Thickness(0,0,0,14) };conteudo.Children.Add(_ativa);
        _inicio=new TextBox { Text=_configuracao.Inicio.ToString(CultureInfo.InvariantCulture),MaxLength=9,TextAlignment=TextAlignment.Right,Margin=new Thickness(0,3,0,12),IsEnabled=_configuracao.Ativa };
        System.Windows.Automation.AutomationProperties.SetName(_inicio,"Começa com");conteudo.Children.Add(new Label { Content="Começa com",Target=_inicio,Padding=new Thickness(0) });conteudo.Children.Add(_inicio);
        _quantidade=new TextBox { Text=service.Quantidade?.ToString(CultureInfo.InvariantCulture)??"",MaxLength=4,Margin=new Thickness(0,3,0,12) };
        System.Windows.Automation.AutomationProperties.SetName(_quantidade,"Quantidade de comandas");conteudo.Children.Add(new Label { Content="Quantidade de comandas",Target=_quantidade,Padding=new Thickness(0) });conteudo.Children.Add(_quantidade);
        _senha=new CheckBox { Content=new TextBlock { Text="Imprimir o número da comanda como senha no final do comprovante",TextWrapping=TextWrapping.Wrap },IsChecked=_configuracao.UsarComoSenhaAtendimento,Margin=new Thickness(0,0,0,14) };conteudo.Children.Add(_senha);
        conteudo.Children.Add(new TextBlock { Text="Sequência na grade",FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,4,0,5) });_previa=new TextBlock { TextWrapping=TextWrapping.Wrap };conteudo.Children.Add(_previa);
        var explicacao=new TextBlock { Text="Deixe a quantidade vazia para usar a quantidade automática. Comandas já abertas mantêm seus números e continuam disponíveis até serem encerradas.",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,14,0,0) };explicacao.SetResourceReference(TextBlock.ForegroundProperty,"Text59665E");conteudo.Children.Add(explicacao);
        _mensagem=new TextBlock { TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,10,0,0) };_mensagem.SetResourceReference(TextBlock.ForegroundProperty,"TextA12822");conteudo.Children.Add(_mensagem);
        _inicio.TextChanged+=(_,_)=>AtualizarPrevia();_ativa.Click+=(_,_)=>{_inicio.IsEnabled=_ativa.IsChecked==true;AtualizarPrevia();};AtualizarPrevia();
        Closing+=(_,e)=>{if(TemAlteracoes&&!new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?","Há alterações de numeração não salvas. Deseja descartá-las e fechar?"))e.Cancel=true;};
    }
    private void AtualizarPrevia()
    {
        var inicio=_ativa.IsChecked==true&&int.TryParse(_inicio.Text,out var valor)?valor:1;
        _previa.Text=inicio>0&&inicio<=999_999_997?$"{inicio}, {inicio+1}, {inicio+2}…":"Informe um início válido.";
    }
    private void Salvar()
    {
        if(!int.TryParse(_inicio.Text,NumberStyles.None,CultureInfo.InvariantCulture,out var inicio)||inicio is <1 or >999_999_999){_mensagem.Text="Informe um número inicial de 1 a 999.999.999.";_inicio.Focus();return;}
        try
        {
            var nova=new ConfiguracaoNumeracaoComandas { Ativa=_ativa.IsChecked==true,Inicio=inicio,UsarComoSenhaAtendimento=_senha.IsChecked==true };
            int? quantidade=null;
            if(!string.IsNullOrWhiteSpace(_quantidade.Text)){if(!int.TryParse(_quantidade.Text,out var n))throw new ArgumentException("Informe uma quantidade válida de comandas.");quantidade=n;}
            if(quantidade is int limite&&_maiorComanda()>(long)nova.PrimeiroNumero+limite-1)throw new ArgumentException("Há comandas abertas acima desse limite. Encerre ou transfira antes de reduzir.");
            _service.Salvar(nova,DateTime.UtcNow,quantidade);_salvou=true;if(IsVisible)DialogResult=true;
        }
        catch(Exception ex){_mensagem.Text=ex is ArgumentException or InvalidOperationException?ex.Message:"Não foi possível salvar a numeração. Tente novamente.";}
    }
}
