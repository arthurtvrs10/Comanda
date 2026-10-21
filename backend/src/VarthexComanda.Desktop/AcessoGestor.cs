using System.IO;
using System.Security.Cryptography;

namespace VarthexComanda.Desktop;

public sealed class AcessoGestor(string caminho)
{
    public bool Configurado => File.Exists(caminho);
    public bool Autenticado { get; private set; }
    public bool AutenticadoPorColaborador { get; private set; }
    public void Sair() { Autenticado = false; AutenticadoPorColaborador = false; }

    public bool EntrarColaborador(ServicoColaboradores colaboradores)
    {
        Sair();
        return Autenticado = AutenticadoPorColaborador = colaboradores.ConferirSessao()?.Perfil == "gestor";
    }

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
        AutenticadoPorColaborador = false;
        Autenticado = true;
    }

    public bool Entrar(string senha)
    {
        Sair();
        var dados = File.ReadAllBytes(caminho);
        if (dados.Length != 64) throw new InvalidOperationException("O arquivo de acesso do gestor está inválido. Contate o suporte.");
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, dados.AsSpan(0, 32), 210_000, HashAlgorithmName.SHA256, 32);
        return Autenticado = CryptographicOperations.FixedTimeEquals(hash, dados.AsSpan(32, 32));
    }

    public bool ConferirSenha(string senha)
    {
        var dados = File.ReadAllBytes(caminho);
        if (dados.Length != 64) throw new InvalidOperationException("Credencial do gestor inválida.");
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, dados.AsSpan(0,32), 210_000, HashAlgorithmName.SHA256,32);
        return CryptographicOperations.FixedTimeEquals(hash,dados.AsSpan(32,32));
    }

    public void AlterarSenha(string atual, string nova)
    {
        if (!Autenticado || AutenticadoPorColaborador || !ConferirSenha(atual)) throw new InvalidOperationException("Senha atual incorreta.");
        if (nova.Length < 8) throw new ArgumentException("Use pelo menos 8 caracteres na nova senha.");
        var sal = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(nova,sal,210_000,HashAlgorithmName.SHA256,32);
        var temporario = caminho + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllBytes(temporario,[..sal,..hash]); File.Move(temporario,caminho,true); }
        finally { if(File.Exists(temporario))File.Delete(temporario); }
    }
}
