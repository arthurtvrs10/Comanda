using System.IO;
using System.Security.Cryptography;

namespace VarthexComanda.Desktop;

public sealed class AcessoAtendimento(string caminho, TimeProvider? relogio = null)
{
    private readonly TimeProvider _relogio = relogio ?? TimeProvider.System;
    private int _tentativas;
    private DateTimeOffset _aguardarAte;
    public bool Configurado => File.Exists(caminho);
    public bool Autenticado { get; private set; }
    public void Bloquear() => Autenticado = false;
    public static bool PinValido(string pin) => pin.Length is >= 4 and <= 8 && pin.All(char.IsAsciiDigit);

    public void DefinirPin(string pin)
    {
        if (!PinValido(pin)) throw new ArgumentException("Use de 4 a 8 números.");
        var sal = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(pin, sal, 210_000, HashAlgorithmName.SHA256, 32);
        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        var temporario = caminho + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllBytes(temporario, [.. sal, .. hash]);
            File.Move(temporario, caminho, true);
        }
        finally { if (File.Exists(temporario)) File.Delete(temporario); }
        _tentativas = 0;
        _aguardarAte = default;
        Bloquear();
    }

    public bool Entrar(string pin, out string mensagem)
    {
        Bloquear();
        if (_relogio.GetUtcNow() < _aguardarAte)
        {
            mensagem = $"Aguarde {(int)Math.Ceiling((_aguardarAte - _relogio.GetUtcNow()).TotalSeconds)} segundos para tentar novamente.";
            return false;
        }
        if (!PinValido(pin)) { mensagem = "Digite um PIN de 4 a 8 números."; return false; }
        var dados = File.ReadAllBytes(caminho);
        if (dados.Length != 64) throw new InvalidOperationException("O PIN não pôde ser lido. Peça ao gestor para redefini-lo.");
        var hash = Rfc2898DeriveBytes.Pbkdf2(pin, dados.AsSpan(0,32), 210_000, HashAlgorithmName.SHA256, 32);
        if (CryptographicOperations.FixedTimeEquals(hash, dados.AsSpan(32,32)))
        {
            _tentativas = 0;
            Autenticado = true;
            mensagem = string.Empty;
            return true;
        }
        if (++_tentativas >= 5)
        {
            _tentativas = 0;
            _aguardarAte = _relogio.GetUtcNow().AddSeconds(30);
            mensagem = "Muitas tentativas. Aguarde 30 segundos.";
        }
        else mensagem = "PIN incorreto. Tente novamente.";
        return false;
    }
}
