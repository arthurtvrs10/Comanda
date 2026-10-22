using System.Globalization;

namespace VarthexComanda.Domain;

public enum UnidadeVenda { Unidade = 0, Quilo = 1, Litro = 2 }

public static class MedidasVenda
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");
    public static string Simbolo(this UnidadeVenda unidade) => unidade switch { UnidadeVenda.Quilo => "KG", UnidadeVenda.Litro => "L", _ => "UN" };
    public static string Descricao(this UnidadeVenda unidade) => unidade switch { UnidadeVenda.Quilo => "Quilo (KG)", UnidadeVenda.Litro => "Litro (L)", _ => "Unidade (UN)" };
    public static string FormatarQuantidade(decimal quantidade, UnidadeVenda unidade) => quantidade.ToString(unidade == UnidadeVenda.Unidade ? "0" : "0.000", Cultura);
    public static string? ValidarQuantidade(decimal quantidade, UnidadeVenda unidade)
    {
        if (!Enum.IsDefined(unidade)) return "Selecione uma unidade de venda válida.";
        if (quantidade <= 0 || quantidade > 1_000_000m) return "Informe uma quantidade maior que zero e até 1.000.000.";
        if (decimal.Round(quantidade, 3) != quantidade) return "Use no máximo três casas decimais na quantidade.";
        if (unidade == UnidadeVenda.Unidade && decimal.Truncate(quantidade) != quantidade) return "Produtos em UN aceitam somente quantidades inteiras.";
        return null;
    }
    public static long CalcularSubtotal(long precoCentavos, decimal quantidade) => checked((long)decimal.Round(precoCentavos * quantidade, 0, MidpointRounding.AwayFromZero));
    public static bool TentarLerQuantidade(string texto, out decimal quantidade) => decimal.TryParse(texto.Trim().Replace('.', ','), NumberStyles.AllowDecimalPoint, Cultura, out quantidade);
}
