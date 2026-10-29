using System.Text;

namespace VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;
public static class Mascaras
{
    public static string Digitos(string texto)=>new(texto.Where(c=>c is >= '0' and <= '9').ToArray());
    private static string Aplicar(string texto,string padrao)
    {
        var digitos=Digitos(texto);var saida=new StringBuilder();var indice=0;
        foreach(var c in padrao){if(indice>=digitos.Length)break;if(c=='#')saida.Append(digitos[indice++]);else saida.Append(c);}
        return saida.ToString();
    }
    public static string Documento(string texto,string tipo)=>Aplicar(texto,tipo=="CPF"?"###.###.###-##":"##.###.###/####-##");
    public static string Telefone(string texto)=>Aplicar(texto,Digitos(texto).Length>10?"(##) #####-####":"(##) ####-####");
    public static int PosicaoDoCursor(string formatado,int quantidadeDigitos)
    {
        if(quantidadeDigitos<=0)return formatado.StartsWith('(')?1:0;
        var vistos=0;for(var i=0;i<formatado.Length;i++)if(char.IsAsciiDigit(formatado[i])&&++vistos==quantidadeDigitos)return i+1;
        return formatado.Length;
    }
}
