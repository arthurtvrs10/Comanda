using System.Text.RegularExpressions;
using QRCoder;

namespace VarthexComanda.Desktop.Licenciamento;

public static class QrAtivacao
{
    public static string Conteudo(string computador)
    {
        if (!Regex.IsMatch(computador, "\\A[0-9A-Fa-f]{64}\\z"))
            throw new ArgumentException("Identificador de computador inválido.", nameof(computador));
        return "varthex://ativacao?v=1&maquina=" + computador.ToUpperInvariant();
    }

    public static byte[] Imagem(string computador)
    {
        using var dados = QRCodeGenerator.GenerateQrCode(Conteudo(computador), QRCodeGenerator.ECCLevel.M);
        using var qr = new PngByteQRCode(dados);
        return qr.GetGraphic(8);
    }
}
