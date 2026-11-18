using System.Windows;
using System.Windows.Media;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public static class FundoSistema
{
    // Native vector image: keeps the wordmark sharp on any monitor without a raster dependency.
    public static DrawingImage ImagemPadrao { get; } = CriarImagemPadrao(false);
    public static DrawingImage ImagemPadraoEscura { get; } = CriarImagemPadrao(true);

    public static string Cor(PreferenciasSistema preferencias, bool escuro = false)
    {
        var padrao = string.IsNullOrWhiteSpace(preferencias.ImagemFundo) &&
            (string.Equals(preferencias.CorFundo, "#F3F5F2", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(preferencias.CorFundo, PreferenciasSistema.CorFundoPadrao, StringComparison.OrdinalIgnoreCase));
        return padrao ? (escuro ? "#17252C" : PreferenciasSistema.CorFundoPadrao) : preferencias.CorFundo;
    }

    public static ImageBrush CriarPincel(PreferenciasSistema preferencias, bool escuro = false)
    {
        var personalizada = JanelaConfiguracoesSistema.CriarImagem(preferencias.ImagemFundo);
        var imagem = (ImageSource?)personalizada ?? (escuro ? ImagemPadraoEscura : ImagemPadrao);
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

    private static DrawingImage CriarImagemPadrao(bool escuro)
    {
        var marca = escuro ? MarcaComanda.LogoEscuro : MarcaComanda.LogoClaro;
        const double largura = 540;
        var altura = largura * marca.Height / marca.Width;
        var desenho = new DrawingGroup();
        using (var contexto = desenho.Open())
        {
            contexto.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 1600, 900));
            contexto.DrawImage(marca, new Rect((1600 - largura) / 2, (900 - altura) / 2, largura, altura));
        }
        var imagem = new DrawingImage(desenho);imagem.Freeze();return imagem;
    }
}
