using System.Runtime.InteropServices;

namespace VarthexComanda.Desktop.Atendimento;

internal static class VolumeWindows
{
    public static (int Percentual,bool Mudo)? Consultar()
    {
        object? enumerador=null;IDevice? dispositivo=null;object? volume=null;
        try
        {
            enumerador=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"))!);
            ((IEnumerator)enumerador!).Default(0,1,out dispositivo);
            var iid=typeof(IVolume).GUID;dispositivo.Activate(ref iid,23,IntPtr.Zero,out volume);
            var endpoint=(IVolume)volume;endpoint.GetScalar(out var valor);endpoint.GetMute(out var mudo);
            return ((int)Math.Round(valor*100),mudo);
        }
        catch{return null;}
        finally{foreach(var obj in new[]{volume,dispositivo,enumerador})if(obj is not null&&Marshal.IsComObject(obj))Marshal.ReleaseComObject(obj);}
    }
    [ComImport,Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IEnumerator
    {
        void Enum(int fluxo,int estado,out IntPtr dispositivos);
        void Default(int fluxo,int papel,out IDevice dispositivo);
    }
    [ComImport,Guid("D666063F-1587-4E43-81F1-B948E807363F"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDevice{void Activate(ref Guid iid,int contexto,IntPtr parametros,[MarshalAs(UnmanagedType.IUnknown)]out object instancia);}
    [ComImport,Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IVolume
    {
        void Register(IntPtr callback);void Unregister(IntPtr callback);void Count(out uint canais);
        void SetDb(float valor,IntPtr contexto);void SetScalar(float valor,IntPtr contexto);void GetDb(out float valor);void GetScalar(out float valor);
        void SetChannelDb(uint canal,float valor,IntPtr contexto);void SetChannelScalar(uint canal,float valor,IntPtr contexto);void GetChannelDb(uint canal,out float valor);void GetChannelScalar(uint canal,out float valor);
        void SetMute([MarshalAs(UnmanagedType.Bool)]bool mudo,IntPtr contexto);void GetMute([MarshalAs(UnmanagedType.Bool)]out bool mudo);
    }
}
