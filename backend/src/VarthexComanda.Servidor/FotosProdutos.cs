namespace VarthexComanda.Servidor;

public sealed record FotoRemota(byte[] Conteudo, string Tipo);

/// <summary>Lê somente imagens do armazenamento de produtos, sem publicar pastas ou nomes locais.</summary>
public sealed class FotosProdutos(string diretorio)
{
    private const long Limite = 10L * 1024 * 1024;
    private static string? Tipo(string? nome) => Path.GetExtension(nome ?? "").ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", ".bmp" => "image/bmp", _ => null
    };

    public static bool NomePermitido(string? nome) => !string.IsNullOrWhiteSpace(nome)
        && nome.IndexOfAny(['/', '\\', ':']) < 0 && nome == Path.GetFileName(nome)
        && nome.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && Tipo(nome) is not null;

    public FotoRemota? Ler(string? nome)
    {
        if (!NomePermitido(nome)) return null;
        try
        {
            var caminho = Path.Combine(diretorio, nome!);
            if ((File.GetAttributes(caminho) & FileAttributes.ReparsePoint) != 0) return null;
            using var fluxo = File.Open(caminho, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fluxo.Length is <= 0 or > Limite) return null;
            var dados = new byte[(int)fluxo.Length];
            fluxo.ReadExactly(dados);
            ReadOnlySpan<byte> cabecalho = dados;
            var tipo = Tipo(nome);
            var valido = tipo switch
            {
                "image/jpeg" => cabecalho.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }),
                "image/png" => cabecalho.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                "image/bmp" => cabecalho.StartsWith(new byte[] { 0x42, 0x4D }), _ => false
            };
            return valido ? new(dados, tipo!) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException) { return null; }
    }
}
