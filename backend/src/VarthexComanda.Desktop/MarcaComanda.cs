using System.Windows;
using System.Windows.Media;

namespace VarthexComanda.Desktop;

/// <summary>Marca vetorial em curvas, compartilhada entre entrada, ajuda e área de trabalho.</summary>
public static class MarcaComanda
{
    private static readonly ResourceDictionary Recursos = (ResourceDictionary)System.Windows.Application.LoadComponent(
        new Uri("/VarthexComanda;component/Assets/MarcaComanda.xaml", UriKind.Relative));

    public static DrawingImage LogoClaro => (DrawingImage)Recursos["ComandaLogoClaro"];
    public static DrawingImage LogoEscuro => (DrawingImage)Recursos["ComandaLogoEscuro"];
}
