using System.Windows;
using System.Windows.Controls;

namespace VarthexComanda.Desktop.Atendimento;

/// <summary>Distribui os cartões pela linha sem reservar uma coluna vazia à direita.</summary>
public sealed class GradeComandasPanel : Panel
{
    private const double LarguraPreferida=144;
    private const double LarguraMinima=128;
    private const double AlturaLinha=144;

    private static int Colunas(double largura)
    {
        var preferidas=(int)Math.Round(largura/LarguraPreferida,MidpointRounding.AwayFromZero);
        var maximas=(int)Math.Floor(largura/LarguraMinima);
        return Math.Max(1,Math.Min(preferidas,maximas));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var largura=double.IsInfinity(availableSize.Width)?LarguraPreferida:Math.Max(0,availableSize.Width);
        var colunas=Colunas(largura);
        foreach(UIElement filho in InternalChildren)filho.Measure(new Size(largura/colunas,AlturaLinha));
        return new Size(largura,Math.Ceiling((double)InternalChildren.Count/colunas)*AlturaLinha);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var colunas=Colunas(finalSize.Width);
        var largura=finalSize.Width/colunas;
        for(var i=0;i<InternalChildren.Count;i++)
            InternalChildren[i].Arrange(new Rect(i%colunas*largura,i/colunas*AlturaLinha,largura,AlturaLinha));
        return finalSize;
    }
}
