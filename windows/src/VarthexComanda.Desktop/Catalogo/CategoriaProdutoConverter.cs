using System.Globalization;
using System.Windows.Data;
using VarthexComanda.Domain;
namespace VarthexComanda.Desktop.Catalogo;
public sealed class CategoriaProdutoConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        values.Length == 2 && values[0] is int id && values[1] is IEnumerable<Categoria> categorias
            ? categorias.FirstOrDefault(c => c.Id == id)?.Nome ?? "" : "";
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
