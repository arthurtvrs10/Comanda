using System.IO;
using System.Security.Cryptography;

namespace VarthexComanda.Desktop;

public sealed class AcessoGestor(string caminho)
{
    public bool Configurado => File.Exists(caminho);
    public bool Autenticado { get; private set; }
    public void Sair() => Autenticado = false;

    public void Cadastrar(string senha)
    {
        if (Configurado) throw new InvalidOperationException("A senha do gestor já foi definida.");
        if (senha.Length < 8) throw new ArgumentException("Use uma senha com pelo menos 8 caracteres.");
        var sal = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, 210_000, HashAlgorithmName.SHA256, 32);
        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        using var arquivo = new FileStream(caminho, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        arquivo.Write(sal);
        arquivo.Write(hash);
        Autenticado = true;
    }

    public bool Entrar(string senha)
    {
        Autenticado = false;
        var dados = File.ReadAllBytes(caminho);
        if (dados.Length != 64) throw new InvalidOperationException("O arquivo de acesso do gestor está inválido. Contate o suporte.");
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, dados.AsSpan(0, 32), 210_000, HashAlgorithmName.SHA256, 32);
        return Autenticado = CryptographicOperations.FixedTimeEquals(hash, dados.AsSpan(32, 32));
    }
}
