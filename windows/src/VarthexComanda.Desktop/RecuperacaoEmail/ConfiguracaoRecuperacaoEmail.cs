using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace VarthexComanda.Desktop.RecuperacaoEmail;

public sealed record ConfiguracaoRecuperacaoEmail(string ApiUrl,string ChavePublicaPem)
{
    public static ConfiguracaoRecuperacaoEmail? Carregar()
    {
        var arquivo=Path.Combine(AppContext.BaseDirectory,"recuperacao-email.json");
        if(!File.Exists(arquivo))return null;
        try
        {
            var config=JsonSerializer.Deserialize<ConfiguracaoRecuperacaoEmail>(File.ReadAllText(arquivo));
            config?.Validar();return config;
        }
        catch(Exception ex) when(ex is JsonException or ArgumentException or CryptographicException or IOException or UnauthorizedAccessException){return null;}
    }
    internal void Validar()
    {
        if(!Uri.TryCreate(ApiUrl,UriKind.Absolute,out var url)||url.Scheme!="https"||url.UserInfo.Length>0||url.Query.Length>0||url.Fragment.Length>0)
            throw new ArgumentException("Endereço seguro da recuperação não configurado.");
        using var rsa=RSA.Create();rsa.ImportFromPem(ChavePublicaPem);
        if(rsa.KeySize<2048)throw new ArgumentException("Chave de recuperação inválida.");
    }
}
