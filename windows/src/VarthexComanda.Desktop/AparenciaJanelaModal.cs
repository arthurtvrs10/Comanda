using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shell;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop;

/// <summary>Cabeçalho comum das janelas auxiliares; o conteúdo e os eventos de fechamento permanecem na janela.</summary>
internal static class AparenciaJanelaModal
{
    private static readonly ConditionalWeakTable<Window, object> Aplicadas = new();

    public static void Aplicar(Window janela)
    {
        if (janela is MainWindow or JanelaExibicaoSenhas or JanelaAbertura || Aplicadas.TryGetValue(janela, out _)) return;
        Aplicadas.Add(janela, new object());
        if (janela.Title == "Comanda") janela.Title = "Aviso";
        else if (janela.Title == "Ativação — Comanda") janela.Title = "Ativação";
        janela.WindowStyle = WindowStyle.None;
        WindowChrome.SetWindowChrome(janela, new WindowChrome
        {
            CaptionHeight = 30, ResizeBorderThickness = new Thickness(5),
            GlassFrameThickness = new Thickness(0), CornerRadius = new CornerRadius(0), UseAeroCaptionButtons = false
        });
        janela.Template = (ControlTemplate)XamlReader.Parse(Modelo);
        janela.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand,
            (_, e) => { janela.Close(); e.Handled = true; },
            (_, e) => { e.CanExecute = true; e.Handled = true; }));
        janela.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand,
            (_, e) =>
            {
                janela.WindowState = janela.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                e.Handled = true;
            },
            (_, e) => { e.CanExecute = janela.ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip; e.Handled = true; }));
    }

    private const string Modelo = """
        <ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:shell="clr-namespace:System.Windows.Shell;assembly=PresentationFramework"
                         TargetType="Window">
          <ControlTemplate.Resources>
            <Style x:Key="AcaoTitulo" TargetType="Button">
              <Setter Property="Width" Value="42"/><Setter Property="Height" Value="30"/>
              <Setter Property="MinWidth" Value="0"/><Setter Property="MinHeight" Value="0"/>
              <Setter Property="Padding" Value="0"/><Setter Property="BorderThickness" Value="0"/>
              <Setter Property="Foreground" Value="White"/><Setter Property="Background" Value="Transparent"/>
              <Setter Property="HorizontalContentAlignment" Value="Center"/><Setter Property="VerticalContentAlignment" Value="Center"/>
              <Setter Property="FocusVisualStyle" Value="{DynamicResource FocoVisivel}"/>
              <Setter Property="shell:WindowChrome.IsHitTestVisibleInChrome" Value="True"/>
              <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Button">
                <Border x:Name="Fundo" Background="{TemplateBinding Background}">
                  <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                </Border>
                <ControlTemplate.Triggers>
                  <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Fundo" Property="Background" Value="#666462"/></Trigger>
                  <Trigger Property="IsPressed" Value="True"><Setter TargetName="Fundo" Property="Background" Value="#777572"/></Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate></Setter.Value></Setter>
            </Style>
            <Style x:Key="AcaoFechar" TargetType="Button" BasedOn="{StaticResource AcaoTitulo}">
              <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Button">
                <Border x:Name="Fundo" Background="Transparent"><ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/></Border>
                <ControlTemplate.Triggers>
                  <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Fundo" Property="Background" Value="#C42B1C"/></Trigger>
                  <Trigger Property="IsPressed" Value="True"><Setter TargetName="Fundo" Property="Background" Value="#9F2318"/></Trigger>
                </ControlTemplate.Triggers>
              </ControlTemplate></Setter.Value></Setter>
            </Style>
          </ControlTemplate.Resources>
          <Border BorderBrush="#A9A9A9" BorderThickness="1" Background="{TemplateBinding Background}" SnapsToDevicePixels="True">
            <DockPanel LastChildFill="True">
              <Grid x:Name="BarraTituloModal" DockPanel.Dock="Top" Height="30" Background="#484644">
                <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
                <TextBlock x:Name="TituloModal" Text="{TemplateBinding Title}" Foreground="White" FontFamily="Segoe UI" FontSize="12" FontWeight="SemiBold"
                           Margin="10,0,10,0" VerticalAlignment="Center" TextTrimming="CharacterEllipsis" IsHitTestVisible="False"/>
                <Button x:Name="MaximizarModal" Grid.Column="1" Visibility="Collapsed" Style="{StaticResource AcaoTitulo}" ToolTip="Maximizar janela" AutomationProperties.Name="Maximizar janela"
                        Command="{x:Static SystemCommands.MaximizeWindowCommand}" CommandTarget="{Binding RelativeSource={RelativeSource TemplatedParent}}">
                  <Path x:Name="SimboloMaximizar" Data="M1,1 H10 V10 H1 Z" Width="10" Height="10" Stretch="Uniform" Stroke="White" StrokeThickness="1"/>
                </Button>
                <Button x:Name="FecharModal" Grid.Column="2" Style="{StaticResource AcaoFechar}" ToolTip="Fechar" AutomationProperties.Name="Fechar"
                        Command="{x:Static SystemCommands.CloseWindowCommand}" CommandTarget="{Binding RelativeSource={RelativeSource TemplatedParent}}">
                  <Path Data="M1,1 L9,9 M9,1 L1,9" Width="9" Height="9" Stretch="Uniform" Stroke="White" StrokeThickness="1.2"/>
                </Button>
              </Grid>
              <AdornerDecorator><ContentPresenter x:Name="ConteudoModal" Content="{TemplateBinding Content}" ContentTemplate="{TemplateBinding ContentTemplate}"
                  ContentTemplateSelector="{TemplateBinding ContentTemplateSelector}" ContentStringFormat="{TemplateBinding ContentStringFormat}"/></AdornerDecorator>
            </DockPanel>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property="ResizeMode" Value="CanResize"><Setter TargetName="MaximizarModal" Property="Visibility" Value="Visible"/></Trigger>
            <Trigger Property="ResizeMode" Value="CanResizeWithGrip"><Setter TargetName="MaximizarModal" Property="Visibility" Value="Visible"/></Trigger>
            <Trigger Property="WindowState" Value="Maximized">
              <Setter TargetName="SimboloMaximizar" Property="Data" Value="M1,4 H7 V10 H1 Z M4,4 V1 H10 V7 H7"/>
              <Setter TargetName="MaximizarModal" Property="ToolTip" Value="Restaurar janela"/>
              <Setter TargetName="MaximizarModal" Property="AutomationProperties.Name" Value="Restaurar janela"/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
        """;
}
