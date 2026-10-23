using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VarthexComanda.Desktop.Licenciamento;

public sealed class JanelaLicenca : Window
{
    public JanelaLicenca(ServicoLicenca servico, string mensagem)
    {
        Title = "Ativação — Comanda";
        Width = 650; Height = Math.Min(760, SystemParameters.WorkArea.Height); WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = TemaInterface.Pincel("#F3F5F2");
        var painel = new StackPanel { Margin = new Thickness(24) };
        Content = new ScrollViewer { Content = painel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        painel.Children.Add(new TextBlock { Text = "Licença do Comanda", FontSize = 24, Margin = new Thickness(0, 0, 0, 16) });
        var aviso = new TextBlock { Text = mensagem, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 16) };
        painel.Children.Add(aviso);
        painel.Children.Add(new TextBlock { Text = "Escaneie com o aplicativo Varthex Licenças", FontSize = 16, TextWrapping = TextWrapping.Wrap });
        using var stream = new MemoryStream(QrAtivacao.Imagem(servico.Computador));
        var bitmap = new BitmapImage();
        bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.StreamSource = stream; bitmap.EndInit(); bitmap.Freeze();
        var imagem = new Image { Source = bitmap, Width = 240, Height = 240, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 12, 0, 12) };
        RenderOptions.SetBitmapScalingMode(imagem, BitmapScalingMode.NearestNeighbor);
        System.Windows.Automation.AutomationProperties.SetName(imagem, "QR Code com o identificador deste computador");
        painel.Children.Add(imagem);
        painel.Children.Add(new TextBlock { Text = "Ou envie o código do computador ao responsável:" });
        painel.Children.Add(new TextBox { Text = servico.Computador, IsReadOnly = true, TextWrapping = TextWrapping.Wrap });
        var copiar = new Button { Content = "Copiar código do computador", Margin = new Thickness(0, 8, 0, 16) };
        copiar.Click += (_, _) => { try { Clipboard.SetText(servico.Computador); } catch { aviso.Text = "Selecione o código e copie com Ctrl+C."; } };
        painel.Children.Add(copiar);
        var contato=new Button { Content="Solicitar ativação ou renovação pelo WhatsApp",MinHeight=38,Margin=new Thickness(0,0,0,12) };
        contato.Click+=(_,_)=>
        {
            try { Ajuda.CanaisAjuda.Abrir(Ajuda.CanaisAjuda.Whatsapp("Olá! Preciso ativar ou renovar o Comanda. Código do computador: "+servico.Computador)); }
            catch { aviso.Text="Entre em contato pelo WhatsApp "+Ajuda.CanaisAjuda.Telefone+" e envie o código do computador."; }
        };
        painel.Children.Add(contato);

        painel.Children.Add(new TextBlock { Text = "Código de ativação recebido", FontSize = 16 });
        var entrada = new TextBox { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 110, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        painel.Children.Add(entrada);
        var ativar = new Button { Content = "Ativar sistema", MinHeight = 46, Margin = new Thickness(0, 12, 0, 12), Background = new SolidColorBrush(Color.FromRgb(36,86,64)), Foreground = Brushes.White };
        ativar.Click += (_, _) =>
        {
            try { servico.Verificar(entrada.Text); DialogResult = true; }
            catch (Exception e) { aviso.Text = e is InvalidOperationException ? e.Message : "Não foi possível salvar a licença. Verifique as permissões da pasta de dados."; }
        };
        painel.Children.Add(ativar);
        painel.Children.Add(new TextBlock { Text = "A ativação funciona sem internet. Seus dados permanecem guardados ao vencer a licença.", TextWrapping = TextWrapping.Wrap });
    }
}
