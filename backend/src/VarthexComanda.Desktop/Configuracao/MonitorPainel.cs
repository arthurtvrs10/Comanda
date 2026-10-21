using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace VarthexComanda.Desktop.Configuracao;

public sealed record MonitorPainel(string Id,string Nome,int X,int Y,int Largura,int Altura,bool Principal)
{
    public override string ToString()=>$"{Nome}: [Resolução: {Largura}x{Altura}]";
}

public static class MonitoresPainel
{
    public static IReadOnlyList<MonitorPainel> Listar()
    {
        var monitores=new List<MonitorPainel>();
        Enumerador callback=(IntPtr monitor,IntPtr dc,ref Retangulo area,IntPtr dados)=>
        {
            var info=new InformacaoMonitor { Tamanho=Marshal.SizeOf<InformacaoMonitor>(),Dispositivo="" };
            if(GetMonitorInfo(monitor,ref info))
                monitores.Add(new(info.Dispositivo,(info.Flags&1)!=0?"Monitor Principal":"Monitor Secundário",info.Area.X,info.Area.Y,info.Area.Direita-info.Area.X,info.Area.Inferior-info.Area.Y,(info.Flags&1)!=0));
            return true;
        };
        if(!EnumDisplayMonitors(IntPtr.Zero,IntPtr.Zero,callback,IntPtr.Zero) || monitores.Count==0)
            throw new InvalidOperationException("Não foi possível consultar os monitores do Windows.");
        return monitores.OrderByDescending(m=>m.Principal).ThenBy(m=>m.Id)
            .Select(m=>!m.Principal&&monitores.Count(x=>!x.Principal)>1?m with { Nome=m.Nome+" ("+m.Id.Split('\\').Last()+")" }:m).ToArray();
    }
    public static void Posicionar(Window janela,MonitorPainel monitor,bool telaCheia)
    {
        janela.WindowState=WindowState.Normal;
        janela.WindowStyle=telaCheia?WindowStyle.None:WindowStyle.SingleBorderWindow;
        janela.ResizeMode=telaCheia?ResizeMode.NoResize:ResizeMode.CanResize;
        var largura=telaCheia?monitor.Largura:Math.Min(1280,(int)(monitor.Largura*0.85));
        var altura=telaCheia?monitor.Altura:Math.Min(800,(int)(monitor.Altura*0.8));
        var handle=new WindowInteropHelper(janela).EnsureHandle();
        if(!SetWindowPos(handle,IntPtr.Zero,monitor.X+(monitor.Largura-largura)/2,monitor.Y+(monitor.Altura-altura)/2,largura,altura,0x0010|0x0004|0x0040))
            throw new InvalidOperationException("Não foi possível posicionar o painel no monitor selecionado.");
    }
    [StructLayout(LayoutKind.Sequential)] private struct Retangulo { public int X,Y,Direita,Inferior; }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
    private struct InformacaoMonitor
    {
        public int Tamanho;public Retangulo Area,Trabalho;public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)] public string Dispositivo;
    }
    private delegate bool Enumerador(IntPtr monitor,IntPtr dc,ref Retangulo area,IntPtr dados);
    [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool EnumDisplayMonitors(IntPtr dc,IntPtr clip,Enumerador callback,IntPtr dados);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,EntryPoint="GetMonitorInfoW")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetMonitorInfo(IntPtr monitor,ref InformacaoMonitor info);
    [DllImport("user32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool SetWindowPos(IntPtr janela,IntPtr anterior,int x,int y,int largura,int altura,uint flags);
}

public sealed class AudioPainelSenhas : IDisposable
{
    private dynamic? _voz;
    public string Anunciar(string senha,bool beep,bool voz,bool feminina)
    {
        if(beep){try{System.Media.SystemSounds.Beep.Play();}catch{}}
        if(!voz)return "";
        try
        {
            _voz??=Activator.CreateInstance(Type.GetTypeFromProgID("SAPI.SpVoice")??throw new InvalidOperationException());
            dynamic tokens=_voz!.GetVoices("Language=416;Gender="+(feminina?"Female":"Male"),"");
            if(tokens.Count==0)
            {
                dynamic portugues=_voz.GetVoices("Language=416","");
                if(portugues.Count==0)return "Chamada exibida. Instale uma voz em português no Windows para habilitar o anúncio.";
                _voz.Voice=portugues.Item(0);
                _voz.Speak("Senha "+senha,3);
                return "Chamada exibida com a voz em português disponível no Windows.";
            }
            _voz.Voice=tokens.Item(0);_voz.Speak("Senha "+senha,3);
            return "";
        }
        catch { return "Chamada exibida. O Windows não disponibilizou a voz; verifique as configurações de fala."; }
    }
    public void Parar()
    {
        if(_voz is not null){try{_voz.Speak("",3);}catch{}}
    }
    public void Dispose()
    {
        if(_voz is null)return;
        Parar();try { Marshal.FinalReleaseComObject(_voz); } catch { }
        _voz=null;
    }
}
