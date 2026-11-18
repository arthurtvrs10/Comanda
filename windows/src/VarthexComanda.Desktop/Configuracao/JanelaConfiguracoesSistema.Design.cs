using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace VarthexComanda.Desktop.Configuracao;

public sealed partial class JanelaConfiguracoesSistema
{
    private WrapPanel? _acoesSeguranca;
    private readonly TextBlock _estado=new() { FontSize=12,VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,0,4) };

    private static string ResumoSecao(string secao)=>secao switch
    {
        "Geral"=>"Identificação do estabelecimento e informações usadas nos comprovantes.",
        "Segurança"=>"Ative a proteção e altere suas senhas de acesso.",
        "Ajustes de cupom"=>"Defina o que aparece na conta do cliente e confira antes de imprimir.",
        "Imagem de fundo"=>"Personalize a área de trabalho e acompanhe o resultado na prévia.",
        "Origem do Cliente"=>"Organize as opções para registrar como o cliente conheceu o estabelecimento.",
        "Locais / cozinhas"=>"Organize os setores de produção, suas impressoras e os produtos de cada local.",
        "Código de Barras por Kg"=>"Configure e confira a leitura das etiquetas emitidas pela sua balança.",
        _=>"Consulte as informações do sistema e compartilhe o diagnóstico com o suporte."
    };

    private static TextBlock Descricao(string texto)
    {
        var t=new TextBlock { Text=texto,FontSize=13,TextWrapping=TextWrapping.Wrap,LineHeight=20,Margin=new Thickness(0,5,0,12) };
        t.SetResourceReference(TextBlock.ForegroundProperty,"AjudaMuted");return t;
    }

    private void AtualizarEstado()
    {
        if(_acoesSeguranca is not null)
        {
            _acoesSeguranca.Visibility=_prefs.SolicitarSenha?Visibility.Visible:Visibility.Collapsed;
            _acoesSeguranca.IsEnabled=!TemAlteracoes;
            _acoesSeguranca.ToolTip=TemAlteracoes?"Salve as alterações para gerenciar as senhas.":null;
        }
        _estado.Text=TemAlteracoes?"Alterações não salvas":"Tudo salvo";
        System.Windows.Automation.AutomationProperties.SetLiveSetting(_estado,System.Windows.Automation.AutomationLiveSetting.Polite);
    }

    private CheckBox CriarToggle(string texto,bool valor)
    {
        var c=new CheckBox { Content=new TextBlock { Text=texto,TextWrapping=TextWrapping.Wrap,FontSize=13,FontWeight=FontWeights.SemiBold },IsChecked=valor,MinHeight=40,HorizontalContentAlignment=HorizontalAlignment.Stretch };
        System.Windows.Automation.AutomationProperties.SetName(c,texto);
        c.FocusVisualStyle=(Style)FindResource("FocoVisivel");
        c.Template=(ControlTemplate)XamlReader.Parse("""
            <ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" TargetType="CheckBox">
              <Grid Background="Transparent" MinHeight="40">
                <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="72"/><ColumnDefinition Width="48"/></Grid.ColumnDefinitions>
                <ContentPresenter VerticalAlignment="Center" Margin="0,0,12,0"/>
                <TextBlock x:Name="Estado" Grid.Column="1" Text="Desativado" Foreground="{DynamicResource AjudaMuted}" FontSize="11" VerticalAlignment="Center" HorizontalAlignment="Right" Margin="0,0,10,0"/>
                <Border x:Name="Trilho" Grid.Column="2" Width="44" Height="24" CornerRadius="12" Background="{DynamicResource AjudaSoft}" BorderBrush="{DynamicResource AjudaMuted}" BorderThickness="1" VerticalAlignment="Center">
                  <Ellipse x:Name="Pino" Width="18" Height="18" Margin="2,0" HorizontalAlignment="Left" Fill="{DynamicResource AjudaMuted}"/>
                </Border>
              </Grid>
              <ControlTemplate.Triggers>
                <Trigger Property="IsChecked" Value="True"><Setter TargetName="Estado" Property="Text" Value="Ativado"/><Setter TargetName="Trilho" Property="Background" Value="{DynamicResource ToggleActive}"/><Setter TargetName="Trilho" Property="BorderBrush" Value="{DynamicResource ToggleActive}"/><Setter TargetName="Pino" Property="HorizontalAlignment" Value="Right"/><Setter TargetName="Pino" Property="Fill" Value="{DynamicResource ToggleInk}"/></Trigger>
                <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Trilho" Property="BorderBrush" Value="{DynamicResource Ink}"/></Trigger>
                <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.5"/></Trigger>
              </ControlTemplate.Triggers>
            </ControlTemplate>
            """);return c;
    }
}
