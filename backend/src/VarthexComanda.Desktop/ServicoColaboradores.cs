using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace VarthexComanda.Desktop;

public sealed record Colaborador
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Nome { get; init; } = "";
    public string Cpf { get; init; } = "";
    public string Funcao { get; init; } = "";
    public string Telefone { get; init; } = "";
    public string Login { get; init; } = "";
    public string Perfil { get; init; } = "garcom";
    public bool Ativo { get; init; } = true;
    public string PerfilDescricao => Perfil switch { "gestor" => "Gestor", "cozinha" => "Cozinha", _ => "Garçom" };
    public string AtivoDescricao => Ativo ? "Sim" : "Não";
}

/// <summary>Contas locais individuais; não substitui a credencial de recuperação do gestor.</summary>
public sealed class ServicoColaboradores(string caminho, TimeProvider? relogio = null)
{
    private readonly TimeProvider _relogio = relogio ?? TimeProvider.System;
    private Colaborador? _sessao;
    private int _falhas;
    private DateTimeOffset _aguardarAte;
    public Colaborador? SessaoAtual => _sessao;
    public void Sair() => _sessao = null;

    public IReadOnlyList<Colaborador> Listar() => Ler().Select(c => c.Dados).OrderBy(c => c.Nome, StringComparer.CurrentCultureIgnoreCase).ToArray();

    public void Salvar(Colaborador dados, string? novaSenha)
    {
        dados = Normalizar(dados);
        Alterar(contas =>
        {
            var existente = contas.SingleOrDefault(c => c.Dados.Id == dados.Id);
            if (contas.Any(c => c.Dados.Id != dados.Id && c.Dados.Login.Equals(dados.Login, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Este login já está em uso. Escolha outro.");
            if (existente is null && string.IsNullOrEmpty(novaSenha)) throw new ArgumentException("Defina uma senha para o novo colaborador.");
            var conta = existente ?? new Conta();
            if (!string.IsNullOrEmpty(novaSenha)) DefinirSenha(conta, novaSenha);
            conta.Dados = dados;
            if (existente is null) contas.Add(conta);
        });
        if (_sessao?.Id == dados.Id) _sessao = dados.Ativo ? dados : null;
    }

    public bool Autenticar(string login, string senha, out string mensagem)
    {
        Sair();
        if (_relogio.GetUtcNow() < _aguardarAte)
        {
            mensagem = $"Aguarde {(int)Math.Ceiling((_aguardarAte - _relogio.GetUtcNow()).TotalSeconds)} segundos para tentar novamente.";
            return false;
        }
        var conta = Ler().SingleOrDefault(c => c.Dados.Login.Equals(login.Trim(), StringComparison.OrdinalIgnoreCase));
        if (conta is not null && Conferir(conta, senha) && conta.Dados.Ativo)
        {
            _sessao = conta.Dados;
            _falhas = 0;
            mensagem = "";
            return true;
        }
        if (++_falhas >= 5)
        {
            _falhas = 0;
            _aguardarAte = _relogio.GetUtcNow().AddSeconds(30);
            mensagem = "Muitas tentativas. Aguarde 30 segundos.";
        }
        else mensagem = "Login ou senha incorretos, ou colaborador inativo.";
        return false;
    }

    public void AlterarSenha(Guid id, string atual, string nova)
    {
        if (_sessao?.Id != id) throw new InvalidOperationException("Entre com este colaborador para alterar sua senha.");
        Alterar(contas =>
        {
            var conta = contas.SingleOrDefault(c => c.Dados.Id == id);
            if (conta is null || !conta.Dados.Ativo || !Conferir(conta, atual)) throw new InvalidOperationException("Senha atual incorreta.");
            DefinirSenha(conta, nova);
        });
    }

    internal Colaborador? ConferirSessao()
    {
        if (_sessao is null) return null;
        _sessao = Ler().SingleOrDefault(c => c.Dados.Id == _sessao.Id && c.Dados.Ativo)?.Dados;
        return _sessao;
    }

    private static Colaborador Normalizar(Colaborador c)
    {
        var nome = c.Nome.Trim();
        var login = c.Login.Trim();
        if (nome.Length is < 1 or > 120) throw new ArgumentException("Informe um nome de até 120 caracteres.");
        if (login.Length is < 3 or > 60 || login.Any(char.IsWhiteSpace)) throw new ArgumentException("Use um login com 3 a 60 caracteres, sem espaços.");
        if (c.Perfil is not ("gestor" or "garcom" or "cozinha")) throw new ArgumentException("Selecione um perfil de acesso válido.");
        var cpf = new string(c.Cpf.Where(char.IsAsciiDigit).ToArray());
        if (cpf.Length != 0 && (cpf.Length != 11 || !CpfValido(cpf))) throw new ArgumentException("Informe um CPF válido ou deixe o campo vazio.");
        if (c.Funcao.Trim().Length > 80 || c.Telefone.Trim().Length > 30) throw new ArgumentException("Revise o tamanho da função e do telefone.");
        return c with { Nome = nome, Login = login, Cpf = cpf, Funcao = c.Funcao.Trim(), Telefone = c.Telefone.Trim() };
    }

    private static bool CpfValido(string cpf)
    {
        if (cpf.All(c => c == cpf[0])) return false;
        for (var tamanho = 9; tamanho <= 10; tamanho++)
        {
            var soma = Enumerable.Range(0, tamanho).Sum(i => (cpf[i] - '0') * (tamanho + 1 - i));
            var digito = (soma * 10) % 11;
            if (digito == 10) digito = 0;
            if (cpf[tamanho] - '0' != digito) return false;
        }
        return true;
    }

    private static void DefinirSenha(Conta conta, string senha)
    {
        if (senha.Length is < 4 or > 128) throw new ArgumentException("Use uma senha com 4 a 128 caracteres.");
        var sal = RandomNumberGenerator.GetBytes(32);
        conta.Sal = Convert.ToBase64String(sal);
        conta.Hash = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(senha, sal, 210_000, HashAlgorithmName.SHA256, 32));
    }

    private static bool Conferir(Conta conta, string senha)
    {
        var sal = Convert.FromBase64String(conta.Sal);
        var esperado = Convert.FromBase64String(conta.Hash);
        if (sal.Length != 32 || esperado.Length != 32) throw new InvalidOperationException("A credencial do colaborador está inválida.");
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, 210_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, esperado);
    }

    private List<Conta> Ler()
    {
        if (!File.Exists(caminho)) return [];
        try { return JsonSerializer.Deserialize<List<Conta>>(File.ReadAllText(caminho)) ?? throw new JsonException(); }
        catch (Exception ex) when (ex is JsonException or FormatException)
        { throw new InvalidOperationException("Não foi possível ler as contas locais. Preserve o arquivo e contate o suporte.", ex); }
    }

    private void Alterar(Action<List<Conta>> atualizar)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(caminho))!);
        using var bloqueio = new FileStream(caminho + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        var contas = Ler();
        atualizar(contas);
        var temporario = caminho + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporario, JsonSerializer.Serialize(contas));
            File.Move(temporario, caminho, true);
        }
        finally { if (File.Exists(temporario)) File.Delete(temporario); }
    }

    private sealed class Conta
    {
        public Colaborador Dados { get; set; } = new();
        public string Sal { get; set; } = "";
        public string Hash { get; set; } = "";
    }
}
