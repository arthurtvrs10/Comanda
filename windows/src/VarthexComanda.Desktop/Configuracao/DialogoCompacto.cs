using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop.Configuracao;

public static class DialogoCompacto
{
    public static void Aplicar(Window janela)
    {
        AparenciaJanelaModal.Aplicar(janela);
        janela.FontSize = 12;
        janela.ShowInTaskbar = false;
        janela.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        janela.SetResourceReference(Control.BackgroundProperty, "Canvas");
        janela.SetResourceReference(Control.ForegroundProperty, "Ink");
        foreach (var tipo in new[] { typeof(TextBox), typeof(PasswordBox), typeof(ComboBox), typeof(Button), typeof(CheckBox), typeof(RadioButton), typeof(ComboBoxItem), typeof(GridViewColumnHeader) })
        {
            var estilo = new Style(tipo, System.Windows.Application.Current.TryFindResource(tipo) as Style);
            estilo.Setters.Add(new Setter(Control.FontSizeProperty, 12.0));
            estilo.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            estilo.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, tipo == typeof(CheckBox) || tipo == typeof(RadioButton) ? 24.0 : 28.0));
            estilo.Setters.Add(new Setter(Control.PaddingProperty, tipo == typeof(Button) ? new Thickness(12,5,12,5) : new Thickness(5,3,5,3)));
            janela.Resources[tipo] = estilo;
        }
    }
}
