using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Suporte;

/// <summary>Excecao tecnica de teste; a mensagem nunca carrega dados de negocio.</summary>
internal sealed class FalhaTecnicaSimuladaException : Exception
{
    public FalhaTecnicaSimuladaException() : base("falha tecnica simulada")
    {
    }
}

internal sealed class ComandaRepositoryQueFalha : IComandaRepository
{
    private readonly IComandaRepository _interno;
    public Comanda DefinirOrigemCliente(int id,string? origem){Verificar();return _interno.DefinirOrigemCliente(id,origem);}
    public ComandaComItens JuntarComandas(int origemId, int destinoId, DateTime agora) { Verificar(); return _interno.JuntarComandas(origemId, destinoId, agora); }
    public Comanda DefinirMesa(int id, int? numeroMesa) { Verificar(); return _interno.DefinirMesa(id, numeroMesa); }
    public Comanda MoverPara(int id, int numero) { Verificar(); return _interno.MoverPara(id, numero); }
    public Comanda DefinirQuantidadePessoas(int id, int quantidade) { Verificar(); return _interno.DefinirQuantidadePessoas(id, quantidade); }
    public Comanda DefinirPrioridade(int comandaId, PrioridadeComanda prioridade) { Verificar(); return _interno.DefinirPrioridade(comandaId, prioridade); }
    public Comanda DefinirNomeCliente(int comandaId, string? nome) { Verificar(); return _interno.DefinirNomeCliente(comandaId, nome); }

    public ComandaRepositoryQueFalha(IComandaRepository interno) => _interno = interno;

    public bool Falhar { get; set; }

    private void Verificar()
    {
        if (Falhar)
        {
            throw new FalhaTecnicaSimuladaException();
        }
    }

    public IReadOnlyList<Comanda> ListarAbertas() => _interno.ListarAbertas();

    public ComandaComItens? BuscarComItens(int comandaId) => _interno.BuscarComItens(comandaId);

    public Comanda AbrirComanda(int numero, DateTime agora)
    {
        Verificar();
        return _interno.AbrirComanda(numero, agora);
    }

    public ComandaComItens AdicionarItem(int comandaId, Produto produto, decimal quantidade, DateTime agora, string? observacao = null)
    {
        Verificar();
        return _interno.AdicionarItem(comandaId, produto, quantidade, agora, observacao);
    }

    public ComandaComItens AlterarQuantidade(int itemId, decimal quantidade, DateTime agora)
    {
        Verificar();
        return _interno.AlterarQuantidade(itemId, quantidade, agora);
    }

    public ComandaComItens RemoverItem(int itemId, DateTime agora)
    {
        Verificar();
        return _interno.RemoverItem(itemId, agora);
    }

    public Comanda CancelarComanda(int comandaId, DateTime agora)
    {
        Verificar();
        return _interno.CancelarComanda(comandaId, agora);
    }

    public Venda EncerrarComanda(int comandaId, DateTime agora, FormaPagamento? formaPagamento = null)
    {
        Verificar();
        return _interno.EncerrarComanda(comandaId, agora, formaPagamento);
    }
}

internal sealed class ProdutoRepositoryQueFalha : IProdutoRepository
{
    private readonly IProdutoRepository _interno;

    public ProdutoRepositoryQueFalha(IProdutoRepository interno) => _interno = interno;

    public bool Falhar { get; set; }

    public Produto? BuscarPorId(int id) => _interno.BuscarPorId(id);

    public IReadOnlyList<Produto> Pesquisar(int? categoriaId, string? texto) => _interno.Pesquisar(categoriaId, texto);

    public IReadOnlyList<string> ListarNomesDeFotos() => _interno.ListarNomesDeFotos();

    public Produto Salvar(Produto produto) =>
        Falhar ? throw new FalhaTecnicaSimuladaException() : _interno.Salvar(produto);
}

internal sealed class CategoriaRepositoryQueFalha : ICategoriaRepository
{
    private readonly ICategoriaRepository _interno;

    public CategoriaRepositoryQueFalha(ICategoriaRepository interno) => _interno = interno;

    public bool Falhar { get; set; }

    public Categoria? BuscarPorId(int id) => _interno.BuscarPorId(id);

    public IReadOnlyList<Categoria> ListarAtivas() => _interno.ListarAtivas();

    public bool ExisteNome(string nome, int? ignorarId = null) => _interno.ExisteNome(nome, ignorarId);

    public Categoria Salvar(Categoria categoria) =>
        Falhar ? throw new FalhaTecnicaSimuladaException() : _interno.Salvar(categoria);
}

internal sealed class FotoStorageQueFalha : IFotoStorage
{
    public bool Falhar { get; set; }

    public string Importar(string caminhoOrigem) =>
        Falhar ? throw new FalhaTecnicaSimuladaException() : "foto1.jpg";

    public void Excluir(string nomeArquivo)
    {
    }
}

internal sealed class ConfiguracaoRepositoryQueFalha : IConfiguracaoRepository
{
    private readonly IConfiguracaoRepository _interno;

    public ConfiguracaoRepositoryQueFalha(IConfiguracaoRepository interno) => _interno = interno;

    public bool Falhar { get; set; }

    public string? ObterValor(string chave) => _interno.ObterValor(chave);

    public void Definir(string chave, string valor, DateTime atualizadoEm)
    {
        if (Falhar)
        {
            throw new FalhaTecnicaSimuladaException();
        }

        _interno.Definir(chave, valor, atualizadoEm);
    }
}
