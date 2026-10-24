using System.Text;
using System.Text.RegularExpressions;

namespace VarthexComanda.Infrastructure.Logging;

public sealed record RegistroLog(string Data, string Nivel, string Mensagem);

public sealed partial class ConsultarLogs(string diretorio)
{
    public const int Limite = 500;
    // Leitura limitada, compartilhada com o logger; nunca carrega todo o histórico na UI.
    public IReadOnlyList<RegistroLog> Executar()
    {
        if (!Directory.Exists(diretorio)) return [];
        var arquivos = Directory.EnumerateFiles(diretorio, "varthex-comanda-*.log")
            .OrderByDescending(Path.GetFileName).Take(2).Reverse().ToArray();
        var registros = new List<RegistroLog>();
        foreach (var arquivo in arquivos)
        {
            using var stream = new FileStream(arquivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var comprimento = stream.Length;
            var tamanho = (int)Math.Min(comprimento, 512 * 1024);
            var inicio = comprimento - tamanho;
            stream.Seek(inicio, SeekOrigin.Begin);
            var bytes = new byte[tamanho];
            // O arquivo pode girar ou continuar recebendo linhas durante a leitura.
            var lidos = 0;
            while (lidos < tamanho) { var n = stream.Read(bytes, lidos, tamanho - lidos); if (n == 0) break; lidos += n; }
            using var leitor = new StringReader(Encoding.UTF8.GetString(bytes, 0, lidos));
            if (inicio > 0) leitor.ReadLine();
            // Um registro com exceção fica limitado a 8 KB na prévia.
            while (leitor.ReadLine() is { } linha)
            {
                var match = Cabecalho().Match(linha);
                if (match.Success)
                    registros.Add(new(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value[..Math.Min(8192, match.Groups[3].Value.Length)]));
                else if (registros.Count > 0 && registros[^1].Mensagem.Length < 8192)
                    registros[^1] = registros[^1] with { Mensagem = (registros[^1].Mensagem + "\n" + linha)[..Math.Min(8192, registros[^1].Mensagem.Length + 1 + linha.Length)] };
                if (registros.Count > Limite) registros.RemoveAt(0);
            }
        }
        return registros.AsEnumerable().Reverse().ToArray();
    }
    [GeneratedRegex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) \[([A-Z]{3})\] (.*)$")]
    private static partial Regex Cabecalho();
}
