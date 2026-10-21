using System.IO;
using System.Windows;
using System.Windows.Media;

namespace VarthexComanda.Desktop;

public static class TemaInterface
{
    [ThreadStatic] private static Dictionary<string,SolidColorBrush>? _pinceis;
    private static Dictionary<string,SolidColorBrush> Pinceis => _pinceis ??= new(StringComparer.OrdinalIgnoreCase);
    private static string? _arquivo;
    private sealed class CorTema : System.ComponentModel.INotifyPropertyChanged
    {
        public Color Color { get; private set; }
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        public CorTema(Color cor) => Color=cor;
        public void Atualizar(Color cor){Color=cor;PropertyChanged?.Invoke(this,new(nameof(Color)));}
    }
    public static bool Escuro { get; private set; }
    public static SolidColorBrush Pincel(string cor)
    {
        if(!Pinceis.TryGetValue(cor,out var pincel))
        {
            pincel=new SolidColorBrush(Converter(cor,Escuro));
            System.Windows.Data.BindingOperations.SetBinding(pincel,SolidColorBrush.ColorProperty,
                new System.Windows.Data.Binding(nameof(CorTema.Color)){Source=new CorTema(pincel.Color)});
            Pinceis[cor]=pincel;
        }
        return pincel;
    }
    public static void Inicializar(string pasta)
    {
        _arquivo=Path.Combine(pasta,"aparencia.txt");
        bool escuro=false;
        try{escuro=File.Exists(_arquivo)&&File.ReadAllText(_arquivo).Trim()=="escuro";}catch(IOException){}catch(UnauthorizedAccessException){}
        Aplicar(escuro,false);
    }
    public static void Aplicar(bool escuro,bool salvar=false)
    {
        Escuro=escuro;
        foreach(var (cor,pincel) in Pinceis)
            ((CorTema)System.Windows.Data.BindingOperations.GetBinding(pincel,SolidColorBrush.ColorProperty)!.Source).Atualizar(Converter(cor,escuro));
        var recursos=System.Windows.Application.Current.Resources;
        foreach(var cor in Cores) recursos["Color"+cor.TrimStart('#')]=Pincel(cor);
        foreach(var cor in Cores)
        {
            var original=(Color)ColorConverter.ConvertFromString(cor);
            var luz=(Math.Max(original.R,Math.Max(original.G,original.B))+Math.Min(original.R,Math.Min(original.G,original.B)))/2;
            recursos["Background"+cor.TrimStart('#')]=new SolidColorBrush(!escuro?original:luz>180?Converter(cor,true):original);
            recursos["Text"+cor.TrimStart('#')]=new SolidColorBrush(escuro&&luz>180?Color.FromRgb(223,233,227):Converter(cor,escuro));
        }
        recursos["Surface"]=Pincel("#FFFFFF");
        recursos["Canvas"]=Pincel("#F3F5F2");
        recursos["Ink"]=Pincel("#263C32");
        recursos["Accent"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(escuro?"#8FD5B0":"#245640"));
        recursos["Line"]=Pincel("#87988E");
        recursos["RibbonBar"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0A0A"));
        recursos["RibbonHover"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString("#484644"));
        recursos["HoverOverlay"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(escuro ? "#26FFFFFF" : "#16000000"));
        recursos["RibbonSurface"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(escuro?"#323130":"#E5E4E3"));
        recursos["RibbonSubtitle"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(escuro?"#B1B9BD":"#59666E"));
        recursos["RibbonInk"]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(escuro?"#F1F3F2":"#263C32"));
        // A central de ajuda usa cinzas neutros; o verde fica nas ações.
        void Ajuda(string chave, string claro, string noturno) => recursos["Ajuda" + chave] =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(escuro ? noturno : claro));
        Ajuda("Canvas", "#F1F1F1", "#252525");
        Ajuda("Surface", "#FAFAFA", "#303030");
        Ajuda("Ink", "#292929", "#F0F0F0");
        Ajuda("Muted", "#616161", "#BDBDBD");
        Ajuda("Line", "#D1D1D1", "#505050");
        Ajuda("Soft", "#E7E7E7", "#383838");
        Ajuda("Hover", "#D4D4D4", "#4B4B4B");
        Ajuda("Action", "#245640", "#98D6B4");
        Ajuda("ActionHover", "#1B4433", "#B0E3C7");
        Ajuda("ActionInk", "#FFFFFF", "#173725");
        void Prioridade(string chave, string cor) => recursos["Prioridade" + chave] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cor));
        Prioridade("Normal", "#2D8050"); Prioridade("Mediana", "#F2CB59"); Prioridade("Alta", "#B6322C");
        Prioridade("NumeroNormal", escuro ? "#80D59F" : "#286D44");
        Prioridade("NumeroMediana", escuro ? "#F2CB59" : "#8B6900");
        Prioridade("NumeroAlta", escuro ? "#FF9993" : "#B6322C");
        recursos["AjudaLogo"] = new System.Windows.Media.Imaging.BitmapImage(new Uri(
            "pack://application:,,,/VarthexComanda;component/Assets/" + (escuro ? "LogoAjudaEscura.png" : "LogoAjudaClara.png")));
        foreach(var key in new[]{SystemColors.WindowBrushKey,SystemColors.ControlBrushKey,SystemColors.ControlLightBrushKey,SystemColors.ControlLightLightBrushKey,SystemColors.MenuBrushKey})recursos[key]=Pincel("#FFFFFF");
        foreach(var key in new[]{SystemColors.WindowTextBrushKey,SystemColors.ControlTextBrushKey,SystemColors.MenuTextBrushKey})recursos[key]=Pincel("#263C32");
        recursos[SystemColors.HighlightBrushKey]=new SolidColorBrush(Color.FromRgb(36,86,64));
        recursos[SystemColors.HighlightTextBrushKey]=Brushes.White;
        if(salvar && _arquivo!=null){Directory.CreateDirectory(Path.GetDirectoryName(_arquivo)!);File.WriteAllText(_arquivo,escuro?"escuro":"claro");}
    }
    private static Color Converter(string cor,bool escuro)
    {
        var c=(Color)ColorConverter.ConvertFromString(cor);
        if(!escuro)return c;
        if(cor is "#E1E8E4" or "#EDEDEA" or "#CCDCD1" or "#CCE3D6")return Color.FromRgb(52,77,63);
        if(cor is "#EDF2EE" or "#FBE3C2")return Color.FromRgb(58,65,53);
        int max=Math.Max(c.R,Math.Max(c.G,c.B)),min=Math.Min(c.R,Math.Min(c.G,c.B));
        double luz=(max+min)/2.0;
        // Preserve solid brand surfaces; lift text, borders and pale surfaces into a dark palette.
        if(luz>225)return Color.FromRgb(29,36,33);
        if(luz>180)return Color.FromRgb(46,58,51);
        if(max-min<42 && luz>115)return Color.FromRgb(157,177,165);
        if(max-min<65 && luz<=115)return Color.FromRgb(223,233,227);
        if(c.R>c.G*1.3 && c.R>c.B*1.2)return Color.FromRgb(255,157,145);
        if(c.R>c.B*1.4 && c.G>c.B*1.2)return Color.FromRgb(213,167,99);
        if(c.G>c.R && c.G>c.B && luz<90)return Color.FromRgb(143,213,176);
        return Color.FromRgb((byte)Math.Min(255,c.R+70),(byte)Math.Min(255,c.G+70),(byte)Math.Min(255,c.B+70));
    }
    private static readonly string[] Cores =
    {
        "#1F5FBF",
        "#20372C",
        "#23252A",
        "#23262B",
        "#245640",
        "#263C32",
        "#326F4C",
        "#387651",
        "#3F8F5F",
        "#46A8E8",
        "#486153",
        "#4A4A47",
        "#4A6A59",
        "#4C3A1F",
        "#52675A",
        "#555555",
        "#59665E",
        "#596D61",
        "#665133",
        "#666666",
        "#6B6B67",
        "#749D89",
        "#786037",
        "#87582D",
        "#87988E",
        "#8A8A86",
        "#999999",
        "#A12822",
        "#A42B25",
        "#B22222",
        "#B23B32",
        "#B71C1C",
        "#BDBDBD",
        "#BF701F",
        "#C4C4C4",
        "#C4CCC6",
        "#C5D0C8",
        "#CCDCD1",
        "#CCE3D6",
        "#D5DED7",
        "#D6DED8",
        "#D6EADF",
        "#D9822B",
        "#DCE3DD",
        "#E1E5E2",
        "#E1E8E4",
        "#EAE2D1",
        "#EDEDEA",
        "#EDF2EE",
        "#F2F2F0",
        "#F3F5F2",
        "#FBE3C2",
        "#FFF5F3",
        "#FFFFFF",
    };
}
