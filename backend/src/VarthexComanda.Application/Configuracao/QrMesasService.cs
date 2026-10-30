using System.Security.Cryptography;
using System.Text;

namespace VarthexComanda.Application.Configuracao;

/// <summary>Credenciais próprias das mesas; não concedem acesso ao atendimento da equipe.</summary>
public sealed class QrMesasService(IConfiguracaoRepository repositorio)
{
    public const string ChaveQuantidade="mesas.chamadas.quantidade.v1";
    private const string ChaveSegredo="mesas.chamadas.segredo.v1";
    private static readonly object Trava=new();
    public int Quantidade=>int.TryParse(repositorio.ObterValor(ChaveQuantidade),out var n)&&n is >=1 and <=999?n:20;
    public void DefinirQuantidade(int quantidade)
    {
        if(quantidade is <1 or >999)throw new ArgumentException("Informe de 1 a 999 mesas.");
        lock(Trava)repositorio.Definir(ChaveQuantidade,quantidade.ToString(),DateTime.UtcNow);
    }
    public string Token(int mesa)
    {
        if(mesa<1||mesa>Quantidade)throw new ArgumentException("Mesa fora da quantidade configurada.");
        lock(Trava)
        {
            var segredo=repositorio.ObterValor(ChaveSegredo);
            if(string.IsNullOrEmpty(segredo)){segredo=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));repositorio.Definir(ChaveSegredo,segredo,DateTime.UtcNow);}
            return Convert.ToHexString(HMACSHA256.HashData(Convert.FromHexString(segredo),Encoding.UTF8.GetBytes("mesa:"+mesa)));
        }
    }
    public bool Autorizar(int mesa,string? token)
    {
        if(mesa<1||mesa>Quantidade||token?.Length!=64||repositorio.ObterValor(ChaveSegredo) is null)return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Token(mesa)),Encoding.ASCII.GetBytes(token));
    }
}
