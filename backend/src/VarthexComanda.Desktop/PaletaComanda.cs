using System.Windows.Media;

namespace VarthexComanda.Desktop;

/// <summary>Papéis explícitos para os dois temas; cores de estado não são cores de decoração.</summary>
internal static class PaletaComanda
{
    internal static readonly Dictionary<string, (string Claro, string Escuro)> Papeis = new()
    {
        ["Canvas"] = ("#F4F5F6", "#242628"),
        ["Surface"] = ("#FFFFFF", "#303336"),
        ["SurfaceRaised"] = ("#FFFFFF", "#383C40"),
        ["Ink"] = ("#17252C", "#F4F6F7"),
        ["TextMuted"] = ("#535D65", "#BEC5CB"),
        ["Line"] = ("#859099", "#78838C"),
        ["Soft"] = ("#E8ECEF", "#3A3F44"),
        ["Selection"] = ("#FFE3D9", "#53352D"),
        ["SelectionInk"] = ("#17252C", "#FFF4EF"),
        ["ActionPrimary"] = ("#FFFFFF", "#303336"),
        ["ActionPrimaryInk"] = ("#17252C", "#F4F6F7"),
        ["ActionPrimaryHover"] = ("#E8ECEF", "#383C40"),
        ["ToggleActive"] = ("#535D65", "#BEC5CB"),
        ["ToggleInk"] = ("#FFFFFF", "#242628"),
        ["Accent"] = ("#B33418", "#FF795B"),
        ["Focus"] = ("#1F5FBF", "#82B6FF"),
        ["ComandaAberta"] = ("#FF5934", "#FF5934"),
        ["ComandaAbertaInk"] = ("#17252C", "#17252C"),
        ["Success"] = ("#286D44", "#80D59F"),
        ["RibbonBar"] = ("#17252C", "#17252C"),
        ["RibbonHover"] = ("#394D58", "#394D58"),
        ["RibbonSurface"] = ("#E8ECEF", "#303336"),
        ["RibbonSubtitle"] = ("#535D65", "#BEC5CB"),
        ["RibbonInk"] = ("#17252C", "#F4F6F7"),
    };

    internal static Color Cor(string papel, bool escuro) => (Color)ColorConverter.ConvertFromString(
        escuro ? Papeis[papel].Escuro : Papeis[papel].Claro);

    // Compatibilidade com os nomes legados: migrar componentes sem reintroduzir tons esverdeados.
    internal static string? PapelNeutro(string hex) => hex.ToUpperInvariant() switch
    {
        "#FFFFFF" => "Surface",
        "#F3F5F2" or "#F2F2F0" or "#EDF2EE" or "#EDEDEA" => "Canvas",
        "#E1E8E4" or "#CCDCD1" or "#CCE3D6" or "#FBE3C2" => "Selection",
        "#D5DED7" or "#D6DED8" or "#D6EADF" or "#DCE3DD" or "#E1E5E2" or "#C5D0C8" or "#C4CCC6" or "#EAE2D1" => "Soft",
        "#20372C" or "#263C32" or "#23252A" or "#23262B" => "Ink",
        "#486153" or "#4A4A47" or "#4A6A59" or "#52675A" or "#555555" or "#59665E" or "#596D61" or "#666666" or "#6B6B67" or "#8A8A86" or "#999999" => "TextMuted",
        "#87988E" or "#749D89" or "#BDBDBD" or "#C4C4C4" => "Line",
        _ => null
    };
}
