using System.Text.Json;

namespace VarthexComanda.Application.Configuracao;

public sealed class PreferenciasPainelSenhas
{
    public string Monitor { get; set; } = "";
    public bool TelaCheia { get; set; } = true;
    public bool LimparAposUmMinuto { get; set; }
    public bool Beep { get; set; } = true;
    public bool Voz { get; set; } = true;
    public bool VozFeminina { get; set; } = true;
    public string CorInicial { get; set; } = "#DCDCDC";
    public string CorFinal { get; set; } = "#1E90FF";
    public string CorNumeros { get; set; } = "#000000";
}

public sealed class PainelSenhasService(IConfiguracaoRepository repositorio)
{
    public const string Chave = "apps.painel-senhas.v1";
    public PreferenciasPainelSenhas Carregar()
    {
        var json = repositorio.ObterValor(Chave);
        var p = string.IsNullOrWhiteSpace(json) ? new PreferenciasPainelSenhas() : JsonSerializer.Deserialize<PreferenciasPainelSenhas>(json) ?? new PreferenciasPainelSenhas();
        Validar(p);
        return p;
    }
    public void Salvar(PreferenciasPainelSenhas preferencias)
    {
        Validar(preferencias);
        repositorio.Definir(Chave,JsonSerializer.Serialize(preferencias),DateTime.UtcNow);
    }
    public static void Validar(PreferenciasPainelSenhas p)
    {
        foreach (var cor in new[] { p.CorInicial,p.CorFinal,p.CorNumeros })
            if (cor is null || (cor.Length!=7 && cor.Length!=9) || cor[0]!='#' || !cor[1..].All(char.IsAsciiHexDigit))
                throw new ArgumentException("Informe as cores no formato #RRGGBB ou #AARRGGBB.");
        if (p.Monitor is null || p.Monitor.Length>100) throw new ArgumentException("Selecione um monitor válido.");
    }
}

/// <summary>Chamadas desta sessão; números mantêm zeros à esquerda e não alteram pedidos.</summary>
public sealed class EstadoPainelSenhas(TimeProvider? relogio=null)
{
    private readonly TimeProvider _relogio=relogio??TimeProvider.System;
    private readonly List<string> _anteriores=[];
    public string? Atual { get; private set; }
    public DateTimeOffset? ChamadaEm { get; private set; }
    public IReadOnlyList<string> Anteriores => _anteriores.AsReadOnly();
    public event Action? Alterado;
    public static bool SenhaValida(string senha) => senha.Length is >=1 and <=9 && senha.All(char.IsAsciiDigit);
    public void Chamar(string senha)
    {
        senha=senha.Trim();
        if(!SenhaValida(senha))throw new ArgumentException("Digite uma senha com 1 a 9 números.");
        if(Atual!=senha)
        {
            _anteriores.Remove(senha);
            ArquivarAtual();
            Atual=senha;
        }
        ChamadaEm=_relogio.GetUtcNow();
        Alterado?.Invoke();
    }
    private void ArquivarAtual()
    {
        if(Atual is null)return;
        _anteriores.Remove(Atual);_anteriores.Insert(0,Atual);
        if(_anteriores.Count>5)_anteriores.RemoveRange(5,_anteriores.Count-5);
    }
    public void Limpar()
    {
        Atual=null;ChamadaEm=null;_anteriores.Clear();Alterado?.Invoke();
    }
    public void VerificarExpiracao(bool habilitada)
    {
        if(!habilitada || ChamadaEm is null || _relogio.GetUtcNow()-ChamadaEm.Value<TimeSpan.FromMinutes(1))return;
        ArquivarAtual();Atual=null;ChamadaEm=null;Alterado?.Invoke();
    }
}
