using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VarthexComanda.Desktop.RecuperacaoEmail;

namespace VarthexComanda.Desktop;

public sealed partial class AcessoGestor
{
    internal string ImpressaoCredencial=>Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(caminho)));
    public ClienteRecuperacaoEmail? CriarRecuperacaoEmail()
    {
        var config=ConfiguracaoRecuperacaoEmail.Carregar();
        return config is null?null:new ClienteRecuperacaoEmail(this,caminho+".email",config);
    }
    internal void ValidarAutorizacaoEmail(string token,string publicKey,string installation,string challenge,string nonce,DateTimeOffset? agora=null)
    {
        try
        {
            if(token.Length>8192)throw new FormatException();
            var parts=token.Split('.');if(parts.Length!=3||parts[0]!="v1")throw new FormatException();
            static byte[] Decode(string s)=>Convert.FromBase64String(s.Replace('-','+').Replace('_','/')+new string('=',(4-s.Length%4)%4));
            using var rsa=RSA.Create();rsa.ImportFromPem(publicKey);
            if(rsa.KeySize<2048||!rsa.VerifyData(Encoding.UTF8.GetBytes($"v1.{parts[1]}"),Decode(parts[2]),HashAlgorithmName.SHA256,RSASignaturePadding.Pss))throw new FormatException();
            using var doc=JsonDocument.Parse(Decode(parts[1]));var p=doc.RootElement;
            var now=(agora??DateTimeOffset.UtcNow).ToUnixTimeSeconds();var issued=p.GetProperty("iat").GetInt64();var expires=p.GetProperty("exp").GetInt64();
            if(p.GetProperty("aud").GetString()!="vcomanda-gestor-reset"||p.GetProperty("installationId").GetString()!=installation||
               p.GetProperty("challengeId").GetString()!=challenge||p.GetProperty("nonce").GetString()!=nonce||
               p.GetProperty("credentialHash").GetString()!=ImpressaoCredencial||issued>now+30||issued<now-600||expires<=now||expires<=issued||expires-issued>600)
                throw new FormatException();
        }
        catch(Exception ex) when(ex is FormatException or CryptographicException or JsonException or KeyNotFoundException or InvalidOperationException or ArgumentException)
        {throw new InvalidOperationException("A autorização expirou ou não pertence a este acesso. Solicite outro código.");}
    }
    internal void RecuperarSenhaPorEmail(string token,string nova,string publicKey,string installation,string challenge,string nonce)
    {
        ValidarNovaSenha(nova);ValidarAutorizacaoEmail(token,publicKey,installation,challenge,nonce);
        var dados=File.ReadAllBytes(caminho);
        if(dados.Length is not (64 or 96 or 128))throw new InvalidOperationException("Credencial do gestor inválida.");
        if(dados.Length==128&&CryptographicOperations.FixedTimeEquals(Rfc2898DeriveBytes.Pbkdf2(nova,dados.AsSpan(64,32),210_000,HashAlgorithmName.SHA256,32),dados.AsSpan(96,32)))
            throw new ArgumentException("A senha de acesso deve ser diferente da senha de recuperação.");
        var salt=RandomNumberGenerator.GetBytes(32);
        GravarCredencial([..salt,..Rfc2898DeriveBytes.Pbkdf2(nova,salt,210_000,HashAlgorithmName.SHA256,32),..dados[64..]]);
        Sair();_falhasRecuperacao=0;
    }
}
