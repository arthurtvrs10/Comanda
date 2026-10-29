using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

namespace VarthexComanda.Desktop;

public sealed class CampoTelefonePadrao : DockPanel
{
    public ComboBox Pais {get;}
    public TextBox Numero {get;}
    public string CodigoPais=>((JanelaConfiguracoesSistema.PaisTelefone)Pais.SelectedItem).Iso;
    public string Telefone=>string.IsNullOrWhiteSpace(Numero.Text)?"":((JanelaConfiguracoesSistema.PaisTelefone)Pais.SelectedItem).Ddi+" "+Numero.Text;
    public event EventHandler? Alterado;
    public CampoTelefonePadrao(string valor,string? iso=null)
    {
        var paises=JanelaConfiguracoesSistema.PaisesTelefone();
        var preferido=paises.FirstOrDefault(p=>p.Iso==iso);
        var inferir=valor.TrimStart().StartsWith('+')&&(preferido is null||!valor.TrimStart().StartsWith(preferido.Ddi,StringComparison.Ordinal));
        var pais=inferir?paises.Where(p=>valor.TrimStart().StartsWith(p.Ddi,StringComparison.Ordinal)).OrderByDescending(p=>p.Ddi.Length).FirstOrDefault():preferido;
        pais??=paises.First(p=>p.Iso=="BR");
        var nacional=valor.Trim();if(nacional.StartsWith(pais.Ddi,StringComparison.Ordinal))nacional=nacional[pais.Ddi.Length..];
        if(pais.Iso=="BR"&&Mascaras.Digitos(nacional).Length is 12 or 13&&Mascaras.Digitos(nacional).StartsWith("55",StringComparison.Ordinal))nacional=Mascaras.Digitos(nacional)[2..];
        Pais=new ComboBox {ItemsSource=paises,SelectedItem=pais,Width=140,MinHeight=36,Margin=new Thickness(0,0,8,0),MaxDropDownHeight=320,IsTextSearchEnabled=true};TextSearch.SetTextPath(Pais,"Nome");
        var fabrica=new FrameworkElementFactory(typeof(StackPanel));fabrica.SetValue(StackPanel.OrientationProperty,Orientation.Horizontal);
        var imagem=new FrameworkElementFactory(typeof(Image));imagem.SetValue(FrameworkElement.WidthProperty,22d);imagem.SetValue(FrameworkElement.HeightProperty,15d);imagem.SetValue(FrameworkElement.MarginProperty,new Thickness(0,0,6,0));imagem.SetBinding(Image.SourceProperty,new Binding("Iso"){Converter=new Bandeira()});fabrica.AppendChild(imagem);
        var texto=new FrameworkElementFactory(typeof(TextBlock));texto.SetBinding(TextBlock.TextProperty,new Binding("."));fabrica.AppendChild(texto);Pais.ItemTemplate=new DataTemplate {VisualTree=fabrica};
        Numero=new TextBox {Text=JanelaConfiguracoesSistema.FormatarTelefoneNacional(nacional,pais.Iso),MinHeight=36,MaxLength=24};
        foreach(var controle in new Control[]{Pais,Numero})
        {
            controle.Foreground=TemaInterface.PincelDoPapel("Ink");controle.Background=TemaInterface.PincelDoPapel("Surface");controle.BorderBrush=TemaInterface.PincelDoPapel("Line");
        }
        System.Windows.Automation.AutomationProperties.SetName(Pais,"País e código do telefone");System.Windows.Automation.AutomationProperties.SetName(Numero,"Telefone principal");
        SetDock(Pais,Dock.Left);Children.Add(Pais);Children.Add(Numero);
        JanelaConfiguracoesSistema.VincularEntrada(Numero,_=>Alterado?.Invoke(this,EventArgs.Empty),v=>JanelaConfiguracoesSistema.FormatarTelefoneNacional(v,CodigoPais));
        Pais.SelectionChanged+=(_,e)=>
        {
            if(CodigoPais=="BR"&&Mascaras.Digitos(Numero.Text).Length>11)
            {
                Pais.SelectedItem=e.RemovedItems[0];Numero.ToolTip="Ajuste o número para até 11 dígitos antes de selecionar Brasil.";return;
            }
            Numero.Text=JanelaConfiguracoesSistema.FormatarTelefoneNacional(Numero.Text,CodigoPais);Alterado?.Invoke(this,EventArgs.Empty);
        };
    }
    private sealed class Bandeira:IValueConverter
    {
        public object Convert(object value,Type type,object parameter,System.Globalization.CultureInfo culture)=>new BitmapImage(new Uri($"pack://application:,,,/VarthexComanda;component/Assets/Bandeiras/{value.ToString()!.ToLowerInvariant()}.png"));
        public object ConvertBack(object value,Type type,object parameter,System.Globalization.CultureInfo culture)=>throw new NotSupportedException();
    }
}
