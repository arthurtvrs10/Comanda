using VarthexComanda.Application.Abstractions;
using VarthexComanda.Domain;

namespace VarthexComanda.Application.Catalogo;

public class AlterarProduto
{
    private readonly IProdutoRepository _produtos;
    private readonly ICategoriaRepository _categorias;
    private readonly IClock _relogio;

    public AlterarProduto(IProdutoRepository produtos, ICategoriaRepository categorias, IClock relogio)
    {
        _produtos = produtos;
        _categorias = categorias;
        _relogio = relogio;
    }

    public Resultado<Produto> Executar(int id, string nome, int categoriaId, long precoCentavos, bool ativo,DetalhesProduto? detalhes=null)
    {
        var produto = _produtos.BuscarPorId(id);
        if (produto is null)
        {
            return Resultado<Produto>.Falha("Produto não encontrado.");
        }

        var nomeNormalizado = (nome ?? string.Empty).Trim();
        if (nomeNormalizado.Length == 0)
        {
            return Resultado<Produto>.Falha("Informe o nome do produto.");
        }
        if (precoCentavos < 0 || (precoCentavos == 0 && !produto.EhComplemento))
        {
            return Resultado<Produto>.Falha("O preço deve ser maior que zero.");
        }
        var categoria = _categorias.BuscarPorId(categoriaId);
        if (!produto.EhComplemento && (categoria is null || !categoria.Ativo))
        {
            return Resultado<Produto>.Falha("Selecione uma categoria ativa.");
        }

        if(produto.EhComplemento && (detalhes?.UnidadeVenda is not (null or UnidadeVenda.Unidade) || detalhes?.Complementos?.Count>0))return Resultado<Produto>.Falha("Complementos usam a medida UN e não podem conter outros complementos.");
        if(produto.EhComplemento && detalhes?.VersaoEstoqueEsperada is long versao && versao!=produto.VersaoEstoque)return Resultado<Produto>.Falha("O estoque deste complemento mudou. Atualize o cadastro antes de salvar.");
        if(detalhes?.Validar() is string erro)return Resultado<Produto>.Falha(erro);
        if(!string.IsNullOrWhiteSpace(detalhes?.CodigoPersonalizado)&&_produtos.CodigoEmUso(detalhes.CodigoPersonalizado.Trim(),id))return Resultado<Produto>.Falha("Já existe um produto com este código personalizado.");
        detalhes?.Aplicar(produto);
        produto.Nome = nomeNormalizado;
        produto.CategoriaId = produto.EhComplemento ? null : categoriaId;
        produto.PrecoCentavos = precoCentavos;
        produto.Ativo = ativo;
        produto.AtualizadoEm = _relogio.UtcNow;
        try { return Resultado<Produto>.Ok(_produtos.Salvar(produto)); }
        catch(CodigoProdutoDuplicadoException ex){return Resultado<Produto>.Falha(ex.Message);}
    }
}
