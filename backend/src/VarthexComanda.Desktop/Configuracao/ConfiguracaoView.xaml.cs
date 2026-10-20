using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;

namespace VarthexComanda.Desktop.Configuracao;

public partial class ConfiguracaoView : UserControl
{
    public ConfiguracaoViewModel ViewModel { get; }
    public event Action<string>? AcaoSolicitada;
    public Func<string>? ConsultarLicenca { get; set; }
    public Func<string>? ConsultarBackup { get; set; }
    private int _secao;
    private bool _trocando;
    public ConfiguracaoView(ConfiguracaoViewModel viewModel)
    {
        InitializeComponent(); ViewModel=viewModel; DataContext=viewModel;
        Selecionar(0);
    }
    public bool ConfirmarSaida()
    {
        if (!ViewModel.TemAlteracoes) return true;
        if(!new Atendimento.JanelaConfirmador().Confirmar("Descartar alterações?","Há alterações não salvas. Deseja descartá-las e continuar?"))return false;
        ViewModel.DescartarCommand.Execute(null); return true;
    }
    public void AbrirSecao(int secao) => Selecionar(secao);
    private void TrocarSecao(object sender,RoutedEventArgs e)
    {
        if(_trocando || ViewModel is null)return;
        int destino=int.Parse((string)((RadioButton)sender).Tag);
        Selecionar(destino);
    }
    private void Selecionar(int secao)
    {
        _secao=secao;_trocando=true;
        foreach(var b in Navegacao.Children.OfType<RadioButton>())b.IsChecked=(string)b.Tag==secao.ToString();
        _trocando=false;Formulario.Children.Clear();
        string[] titulos=["Estabelecimento","Atendimento","Segurança","Backup","Licença e sistema"];
        string[] descricoes=["Identifique o seu negócio no sistema.","Organize as comandas disponíveis para a equipe.","Gerencie o acesso do gestor e da equipe de atendimento.","Mantenha uma cópia dos dados e consulte os backups disponíveis.","Confira a ativação deste computador e a versão instalada."];
        Titulo.Text=titulos[secao];Descricao.Text=descricoes[secao];
        Acoes.Visibility=secao is 0 or 1 or 3?Visibility.Visible:Visibility.Collapsed;
        switch(secao)
        {
            case 0: Campo("Nome do estabelecimento",nameof(ViewModel.NomeEstabelecimento),"Use o nome pelo qual seu estabelecimento é conhecido.");break;
            case 1: Campo("Quantidade de comandas",nameof(ViewModel.QuantidadeComandasTexto),"Informe um número maior que zero. Em branco, mantém o comportamento automático do sistema. Comandas abertas são preservadas.",160);break;
            case 2:
                Linha("Senha do gestor","Protege produtos, configurações, backups e licença.","Alterar senha","senha");
                Linha("PIN do atendimento","Código compartilhado para entrar nos menus do garçom e da cozinha.","Alterar PIN","pin");
                Linha("Bloqueio ao concluir lançamento","Ativo. Ao concluir, o atendimento volta ao menu principal e solicita o PIN.");break;
            case 3:
                Campo("Pasta de backup externa",nameof(ViewModel.PastaBackupExterna),"Remover este destino não apaga os arquivos de backup existentes.",double.NaN,true);
                var botoes=new WrapPanel { Margin=new Thickness(0,0,0,24) };
                var escolher=new Button { Content="Escolher pasta",Margin=new Thickness(0,0,8,8) };escolher.Click+=EscolherPasta_Click;botoes.Children.Add(escolher);
                var limpar=new Button { Content="Remover pasta externa",Margin=new Thickness(0,0,0,8) };limpar.Click+=LimparPasta_Click;botoes.Children.Add(limpar);Formulario.Children.Add(botoes);
                Linha("Último backup",ConsultarBackup?.Invoke() ?? "Consulte o histórico de backups.","Criar backup agora","criar-backup");
                Linha("Histórico e restauração","Consulte uma cópia antes de restaurar os dados.","Ver backups","backups");break;
            case 4:
                Linha("Licença",ConsultarLicenca?.Invoke() ?? "Abra a ativação para conferir a licença deste computador.","Ver ativação","licenca");
                Linha("Versão instalada",ViewModel.VersaoTexto+" · Windows");
                Linha("Identificação do computador","O identificador completo e o QR Code ficam na tela de ativação.");break;
        }
    }
    public void AtualizarSecao() => Selecionar(_secao);
    private void Campo(string rotulo,string propriedade,string ajuda,double largura=double.NaN,bool somenteLeitura=false)
    {
        var label=new Label { Content=rotulo,FontWeight=FontWeights.SemiBold,Padding=new Thickness(0,0,0,8) };Formulario.Children.Add(label);
        var input=new TextBox { MinHeight=46,Width=largura,MaxWidth=600,HorizontalAlignment=HorizontalAlignment.Left,IsReadOnly=somenteLeitura,MinWidth=double.IsNaN(largura)?200:largura };
        if(double.IsNaN(largura)){input.HorizontalAlignment=HorizontalAlignment.Left; input.Width=Math.Min(600,Math.Max(200,ActualWidth-(ActualWidth<800?110:330)));}
        input.SetBinding(TextBox.TextProperty,new Binding(propriedade){Mode=somenteLeitura?BindingMode.OneWay:BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});
        label.Target=input;Formulario.Children.Add(input);Formulario.Children.Add(new TextBlock { Text=ajuda,TextWrapping=TextWrapping.Wrap,Foreground=TemaInterface.Pincel("#59665E"),Margin=new Thickness(0,8,0,16) });
    }
    private void Linha(string titulo,string texto,string? botao=null,string? acao=null)
    {
        var painel=new StackPanel { Margin=new Thickness(0,0,0,22) };
        painel.Children.Add(new TextBlock { Text=titulo,FontSize=17,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap });
        painel.Children.Add(new TextBlock { Text=texto,TextWrapping=TextWrapping.Wrap,Foreground=TemaInterface.Pincel("#59665E"),Margin=new Thickness(0,6,0,12) });
        if(botao!=null){var b=new Button { Content=botao,HorizontalAlignment=HorizontalAlignment.Left,MinHeight=46 };b.Click+=(_,_)=>{if(ViewModel.TemAlteracoes && acao is "criar-backup" or "backups"){ViewModel.MensagemErro=true;ViewModel.Mensagem="Salve ou descarte as alterações antes de abrir o backup.";return;}AcaoSolicitada?.Invoke(acao!);};painel.Children.Add(b);}
        var linha=new Grid { Margin=new Thickness(0,0,0,18) };
        linha.ColumnDefinitions.Add(new ColumnDefinition());linha.ColumnDefinitions.Add(new ColumnDefinition { Width=GridLength.Auto });
        if(botao!=null && ActualWidth>=800){var acaoBotao=(Button)painel.Children[painel.Children.Count-1];painel.Children.Remove(acaoBotao);acaoBotao.Margin=new Thickness(20,0,0,0);acaoBotao.VerticalAlignment=VerticalAlignment.Center;Grid.SetColumn(acaoBotao,1);linha.Children.Add(acaoBotao);}
        linha.Children.Add(painel);Formulario.Children.Add(linha);
    }
    private void AoRedimensionar(object sender,SizeChangedEventArgs e)
    {
        bool compacto=ActualWidth<800;
        ColunaMenu.Width=compacto?new GridLength(0):new GridLength(220);
        Grid.SetColumn(Menu,compacto?1:0);Grid.SetRowSpan(Menu,compacto?1:2);
        Grid.SetRow(Area,compacto?1:0);Grid.SetRowSpan(Area,compacto?1:2);
        Navegacao.Orientation=compacto?Orientation.Horizontal:Orientation.Vertical;
        if (e.WidthChanged && ViewModel is not null) Selecionar(_secao);
    }
    private void EscolherPasta_Click(object sender,RoutedEventArgs e)
    {
        var dialogo=new OpenFolderDialog { Title="Escolher pasta de backup externa" };
        if(dialogo.ShowDialog()==true)ViewModel.DefinirPastaBackupExterna(dialogo.FolderName);
    }
    private void LimparPasta_Click(object sender,RoutedEventArgs e)=>ViewModel.DefinirPastaBackupExterna(null);
}
