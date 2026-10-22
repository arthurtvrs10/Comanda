using System.Globalization;
using System.Windows.Data;

namespace VarthexComanda.Desktop;

public sealed class TextoMaiusculoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (value?.ToString() ?? string.Empty).ToUpper(CultureInfo.GetCultureInfo("pt-BR"));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
