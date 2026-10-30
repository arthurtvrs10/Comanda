using System.IO;
namespace VarthexComanda.Desktop.Backup;

internal static class PastasOneDrive
{
    internal static IReadOnlyList<string> Detectar(Func<string,string?>? ambiente=null,Func<string,bool>? existe=null,string? perfil=null)
    {
        ambiente??=Environment.GetEnvironmentVariable;existe??=Directory.Exists;
        perfil??=Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidatas=new[]{ambiente("OneDriveConsumer"),ambiente("OneDriveCommercial"),ambiente("OneDrive"),Path.Combine(perfil,"OneDrive")};
        var pastas=new List<string>();
        foreach(var candidata in candidatas)
        {
            if(string.IsNullOrWhiteSpace(candidata)||!Path.IsPathFullyQualified(candidata))continue;
            try{var pasta=Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidata));if(existe(pasta)&&!pastas.Contains(pasta,StringComparer.OrdinalIgnoreCase))pastas.Add(pasta);}catch(ArgumentException){}catch(IOException){}catch(UnauthorizedAccessException){}
        }
        return pastas;
    }
}
