using System.Globalization;
using System.Windows.Data;

namespace VarthexComanda.Desktop.Atendimento;

public sealed class NumeroComandaPesquisadoConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Length == 2 && values[0] is int numero && values[1] is string texto
        && int.TryParse(texto, out var pesquisado) && numero == pesquisado;

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
