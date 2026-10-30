using System.IO;

namespace VarthexComanda.Desktop;

public static class ModoExecucao
{
    public static bool Demonstracao(IEnumerable<string> argumentos, string pastaAplicativo) =>
        Restaurante(argumentos,pastaAplicativo) || argumentos.Contains("--demonstracao", StringComparer.OrdinalIgnoreCase)
        || File.Exists(Path.Combine(pastaAplicativo, "edicao-demonstracao.txt"));

    public static bool Restaurante(IEnumerable<string> argumentos,string pastaAplicativo)=>
        argumentos.Contains("--demonstracao-restaurante",StringComparer.OrdinalIgnoreCase)
        ||File.Exists(Path.Combine(pastaAplicativo,"edicao-demonstracao-restaurante.txt"));

    // A demonstração nunca usa a variável de ambiente que possa apontar para dados reais.
    public static string? RaizDados(bool demonstracao, string? ambiente, string localAppData,bool restaurante=false) =>
        demonstracao ? Path.Combine(localAppData, restaurante?"VarthexComandaDemoRestaurante":"VarthexComandaDemo")
            : string.IsNullOrWhiteSpace(ambiente) ? null : ambiente;
}
