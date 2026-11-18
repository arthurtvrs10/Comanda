namespace VarthexComanda.Application.Configuracao;

public interface IConfiguracaoRepository
{
    string? ObterValor(string chave);
    void Definir(string chave, string valor, DateTime atualizadoEm);
    void DefinirValores(IReadOnlyDictionary<string,string> valores, DateTime atualizadoEm)
    {
        foreach (var (chave,valor) in valores) Definir(chave,valor,atualizadoEm);
    }
}
