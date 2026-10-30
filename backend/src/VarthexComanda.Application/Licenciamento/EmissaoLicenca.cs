using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace VarthexComanda.Application.Licenciamento;

public enum PlanoLicenca { Vitalicia=0, UmMes=1, DoisMeses=2, TresMeses=3, Dias=4, Data=5 }
public sealed record PeriodoEmissao(DateTimeOffset Inicio,DateTimeOffset? Vencimento);

public static class EmissaoLicenca
{
    public static readonly TimeSpan FusoBrasilia=TimeSpan.FromHours(-3);
    private const string PrefixoQr="varthex://ativacao?v=1&maquina=";
    public static string LerComputador(string? entrada)
    {
        var valor=entrada?.Trim()??"";
        if(valor.StartsWith(PrefixoQr,StringComparison.Ordinal))
        {
            valor=valor[PrefixoQr.Length..];
            if(!Regex.IsMatch(valor,@"\A[0-9A-F]{64}\z"))throw new ArgumentException("QR Code de ativação inválido.");
        }
        if(!Regex.IsMatch(valor,@"\A[0-9A-Fa-f]{64}\z"))throw new ArgumentException("Informe os 64 caracteres do computador ou o conteúdo do QR Code de ativação do VComanda.");
        return valor.ToUpperInvariant();
    }
    public static PeriodoEmissao Calcular(PlanoLicenca plano,DateOnly hoje,int dias=0,DateOnly? dataFinal=null,DateOnly? vencimentoAtual=null)
    {
        if(!Enum.IsDefined(plano))throw new ArgumentException("Selecione uma validade válida.");
        if(hoje.Year is <2000 or >9998)throw new ArgumentException("Confira a data atual do computador.");
        var inicio=new DateTimeOffset(hoje.ToDateTime(TimeOnly.MinValue),FusoBrasilia);
        var referencia=vencimentoAtual is { } atual&&atual>hoje?atual:hoje;
        DateOnly? fim;
        try
        {
            fim=plano switch
            {
                PlanoLicenca.Vitalicia=>null,
                PlanoLicenca.UmMes or PlanoLicenca.DoisMeses or PlanoLicenca.TresMeses=>referencia.AddMonths((int)plano),
                PlanoLicenca.Dias when dias is >=1 and <=36500=>referencia.AddDays(dias),
                PlanoLicenca.Dias=>throw new ArgumentException("Informe entre 1 e 36.500 dias."),
                PlanoLicenca.Data when dataFinal is { } selecionada&&selecionada>=hoje=>selecionada.AddDays(1),
                _=>throw new ArgumentException("Escolha hoje ou uma data futura para o último dia de uso.")
            };
        }
        catch(ArgumentOutOfRangeException){throw new ArgumentException("A validade ultrapassa o limite de datas permitido.");}
        return new(inicio,fim is { } dia?new DateTimeOffset(dia.ToDateTime(TimeOnly.MinValue),FusoBrasilia):null);
    }
    public static void ConferirChave(string privada,string publica)
    {
        using var emissor=RSA.Create();emissor.ImportFromPem(privada);
        using var esperado=RSA.Create();esperado.ImportFromPem(publica);
        if(!CryptographicOperations.FixedTimeEquals(emissor.ExportSubjectPublicKeyInfo(),esperado.ExportSubjectPublicKeyInfo()))throw new ArgumentException("A chave privada não corresponde a esta versão do VComanda. Use a chave do emissor original.");
    }
}
