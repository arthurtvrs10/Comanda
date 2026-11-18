using VarthexComanda.Domain;

namespace VarthexComanda.Application.Catalogo;

public class PesquisarProdutos
{
    private readonly IProdutoRepository _produtos;

    public PesquisarProdutos(IProdutoRepository produtos)
    {
        _produtos = produtos;
    }

    public Produto? BuscarAtivo(int id) => _produtos.BuscarPorId(id) is { Ativo:true,EhComplemento:false } produto?produto:null;
    public void PrepararComplementos() => _produtos.PrepararCatalogoComplementos();
    public IReadOnlyList<Produto> Executar(int? categoriaId, string? texto, bool complementos = false) => _produtos.Pesquisar(categoriaId, texto).Where(p=>p.EhComplemento==complementos).ToArray();
}
