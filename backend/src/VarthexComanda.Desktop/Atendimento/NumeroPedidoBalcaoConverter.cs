using System.Globalization;
using System.Windows.Data;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public sealed class NumeroPedidoBalcaoConverter : IValueConverter
{
    public object Convert(object value,Type targetType,object parameter,CultureInfo culture)=>value is Comanda c?$"#{c.CodigoPedido??c.Id}":"";
    public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture)=>throw new NotSupportedException();
}
