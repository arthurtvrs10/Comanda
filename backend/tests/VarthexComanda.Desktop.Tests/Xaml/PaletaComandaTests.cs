using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class PaletaComandaTests
{
    [Fact]
    public void ParesDeLeituraEControlesMantemContrasteNosDoisTemas()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try
            {
                foreach(var escuro in new[]{false,true,false})
                {
                    TemaInterface.Aplicar(escuro);
                    Color Cor(string recurso) => ((SolidColorBrush)System.Windows.Application.Current.Resources[recurso]).Color;
                    foreach(var (texto,fundo) in new[]{("Ink","Canvas"),("Ink","Surface"),("TextMuted","Surface"),("TextMuted","Canvas"),("ActionPrimaryInk","ActionPrimary"),("ActionPrimaryInk","ActionPrimaryHover"),("ComandaAbertaInk","ComandaAberta"),("SelectionInk","Selection")})
                        Assert.True(Contraste(Cor(texto),Cor(fundo))>=4.5,$"{texto}/{fundo}, escuro={escuro}: {Contraste(Cor(texto),Cor(fundo)):F2}");
                    Assert.True(Contraste(Cor("Line"),Cor("Surface"))>=3);
                    var botao=new Button { Style=(Style)System.Windows.Application.Current.Resources["BotaoPrimario"] };
                    Assert.Equal(Cor("ActionPrimary"),((SolidColorBrush)botao.Background).Color);
                    Assert.Equal(Cor("ActionPrimaryInk"),((SolidColorBrush)botao.Foreground).Color);
                    // Recursos antigos devem usar a mesma escala neutra dos componentes novos.
                    Assert.Equal(Cor("Canvas"),Cor("BackgroundF3F5F2"));
                    Assert.Equal(Cor("Selection"),Cor("BackgroundE1E8E4"));
                }
            }
            finally { TemaInterface.Aplicar(false); }
        });
    }

    private static double Contraste(Color a,Color b)
    {
        static double L(Color c)
        {
            static double Canal(byte v) { double n=v/255d;return n<=0.04045?n/12.92:Math.Pow((n+0.055)/1.055,2.4); }
            return .2126*Canal(c.R)+.7152*Canal(c.G)+.0722*Canal(c.B);
        }
        var x=L(a);var y=L(b);return (Math.Max(x,y)+.05)/(Math.Min(x,y)+.05);
    }
}
