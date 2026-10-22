using System.Globalization;
using System.Windows;
using System.Windows.Media;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public static class FundoSistema
{
    // Native vector image: keeps the wordmark sharp on any monitor without a raster dependency.
    public static DrawingImage ImagemPadrao { get; } = CriarImagemPadrao();

    public static string Cor(PreferenciasSistema preferencias) =>
        string.IsNullOrWhiteSpace(preferencias.ImagemFundo) && string.Equals(preferencias.CorFundo, "#F3F5F2", StringComparison.OrdinalIgnoreCase)
            ? PreferenciasSistema.CorFundoPadrao : preferencias.CorFundo;

    public static ImageBrush CriarPincel(PreferenciasSistema preferencias)
    {
        var personalizada = JanelaConfiguracoesSistema.CriarImagem(preferencias.ImagemFundo);
        var imagem = (ImageSource?)personalizada ?? ImagemPadrao;
        var pincel = new ImageBrush(imagem)
        {
            Stretch = personalizada is null ? Stretch.Uniform : preferencias.PosicaoImagem switch { "Preencher" => Stretch.Fill, "Zoom" => Stretch.UniformToFill, _ => Stretch.None },
            AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center
        };
        if (personalizada is not null && preferencias.PosicaoImagem == "Repetir")
        {
            pincel.TileMode = TileMode.Tile;
            pincel.ViewportUnits = BrushMappingMode.Absolute;
            pincel.Viewport = new Rect(0, 0, imagem.Width, imagem.Height);
        }
        return pincel;
    }

    private static DrawingImage CriarImagemPadrao()
    {
        var tinta = new SolidColorBrush(Color.FromRgb(133, 133, 133));
        var texto = new FormattedText("VARTHEX", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 100, tinta, 1);
        var letras = texto.BuildGeometry(new Point());
        var limites = letras.Bounds;
        letras.Transform = new TranslateTransform(800 - limites.Left - limites.Width / 2, 450 - limites.Top - limites.Height / 2);
        var desenho = new DrawingGroup();
        using (var contexto = desenho.Open())
        {
            contexto.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 1600, 900));
            contexto.DrawGeometry(tinta, null, letras);
        }
        var imagem = new DrawingImage(desenho);imagem.Freeze();return imagem;
    }
}
