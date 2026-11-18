using System.IO;
using System.Security.Cryptography;

namespace VarthexComanda.Desktop;

public sealed partial class AcessoGestor
{
    private int _falhasRecuperacao;
    private DateTimeOffset _recuperacaoBloqueadaAte;
    public bool RecuperacaoConfigurada => Configurado && new FileInfo(caminho).Length==128;

    public void RecuperarSenha(string codigo,string novaSenha)
    {
        Sair();ValidarNovaSenha(novaSenha);
        if(DateTimeOffset.UtcNow<_recuperacaoBloqueadaAte)throw new InvalidOperationException("Aguarde 30 segundos antes de tentar recuperar a senha novamente.");
        var dados=File.ReadAllBytes(caminho);
        if(dados.Length==128)
        {
            var hash=Rfc2898DeriveBytes.Pbkdf2(codigo,dados.AsSpan(64,32),210_000,HashAlgorithmName.SHA256,32);
            if(!CryptographicOperations.FixedTimeEquals(hash,dados.AsSpan(96,32)))
            {
                if(++_falhasRecuperacao>=5){_falhasRecuperacao=0;_recuperacaoBloqueadaAte=DateTimeOffset.UtcNow.AddSeconds(30);}
                throw new InvalidOperationException("Senha de recuperação incorreta.");
            }
            if(novaSenha==codigo)throw new ArgumentException("A senha de acesso deve ser diferente da senha de recuperação.");
            var sal=RandomNumberGenerator.GetBytes(32);
            GravarCredencial([..sal,..Rfc2898DeriveBytes.Pbkdf2(novaSenha,sal,210_000,HashAlgorithmName.SHA256,32),..dados[64..]]);
            _falhasRecuperacao=0;return;
        }
        throw new InvalidOperationException("Configure uma senha de recuperação em Segurança antes de usar este recurso.");
    }

    public void CadastrarComSenhaRecuperacao(string senha,string recuperacao)
    {
        if(Configurado)throw new InvalidOperationException("A senha do gestor já foi definida.");
        ValidarSenhas(senha,recuperacao);
        var dados=CredencialComSenhaRecuperacao(senha,recuperacao);
        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        using(var arquivo=new FileStream(caminho,FileMode.CreateNew,FileAccess.Write,FileShare.None)){arquivo.Write(dados);arquivo.Flush(true);}
        Autenticado=true;AutenticadoPorColaborador=false;
    }
    internal void DefinirSenhasNaAtivacao(string senha,string recuperacao)
    {
        if(!Autenticado)throw new InvalidOperationException("Abra as configurações em uma sessão autorizada para ativar a proteção.");
        ValidarSenhas(senha,recuperacao);
        if(!Configurado){CadastrarComSenhaRecuperacao(senha,recuperacao);return;}
        GravarCredencial(CredencialComSenhaRecuperacao(senha,recuperacao));
        AutenticadoPorColaborador=false;
    }

    public void DefinirSenhaRecuperacao(string atual,string recuperacao)
    {
        if(!ConferirSenha(atual))throw new InvalidOperationException("Senha atual do gestor incorreta.");
        ValidarSenhas(atual,recuperacao);
        GravarCredencial(CredencialComSenhaRecuperacao(atual,recuperacao));
    }
    private static void ValidarSenhas(string senha,string recuperacao)
    {
        ValidarNovaSenha(senha);ValidarNovaSenha(recuperacao);
        if(senha==recuperacao)throw new ArgumentException("A senha de recuperação deve ser diferente da senha de acesso.");
    }
    private static byte[] CredencialComSenhaRecuperacao(string senha,string recuperacao)
    {
        var sal=RandomNumberGenerator.GetBytes(32);var salRecuperacao=RandomNumberGenerator.GetBytes(32);
        return [..sal,..Rfc2898DeriveBytes.Pbkdf2(senha,sal,210_000,HashAlgorithmName.SHA256,32),..salRecuperacao,..Rfc2898DeriveBytes.Pbkdf2(recuperacao,salRecuperacao,210_000,HashAlgorithmName.SHA256,32)];
    }

    private static void ValidarNovaSenha(string senha)
    {
        if(senha.Length is <4 or >128)throw new ArgumentException("Use uma senha com 4 a 128 caracteres.");
    }
    private void GravarCredencial(byte[] dados)
    {
        var temporario=caminho+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            using(var arquivo=new FileStream(temporario,FileMode.CreateNew,FileAccess.Write,FileShare.None)){arquivo.Write(dados);arquivo.Flush(true);}
            File.Move(temporario,caminho,true);
        }
        finally{if(File.Exists(temporario))File.Delete(temporario);}
    }
}
