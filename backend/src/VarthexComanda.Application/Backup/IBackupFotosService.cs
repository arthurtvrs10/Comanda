using VarthexComanda.Application.Catalogo;

namespace VarthexComanda.Application.Backup;

public sealed record FotosRestauradas(int Quantidade);

public interface IBackupFotosService
{
    Resultado<FotosRestauradas> RestaurarFotos(string arquivoZip);
}
