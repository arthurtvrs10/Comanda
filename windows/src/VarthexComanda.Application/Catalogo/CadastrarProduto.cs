using VarthexComanda.Application.Abstractions;
using VarthexComanda.Domain;

namespace VarthexComanda.Application.Catalogo;

public class CadastrarProduto
{
    private readonly IProdutoRepository _produtos;
    private readonly ICategoriaRepository _categorias;
    private readonly IClock _relogio;

    public CadastrarProduto(IProdutoRepository produtos, ICategoriaRepository categorias, IClock relogio)
    {
        _produtos = produtos;
        _categorias = categorias;
        _relogio = relogio;
    }

    public Resultado<Produto> Executar(string nome, int categoriaId, long precoCentavos,DetalhesProduto? detalhes=null,bool ativo=true,bool complemento=false)
    {
        var nomeNormalizado = (nome ?? string.Empty).Trim();
        if (nomeNormalizado.Length == 0)
        {
            return Resultado<Produto>.Falha("Informe o nome do produto.");
        }
        if (precoCentavos < 0 || (precoCentavos == 0 && !complemento))
        {
            return Resultado<Produto>.Falha("O preço deve ser maior que zero.");
        }
        var categoria = _categorias.BuscarPorId(categoriaId);
        if (!complemento && (categoria is null || !categoria.Ativo))
        {
            return Resultado<Produto>.Falha("Selecione uma categoria ativa.");
        }

        if(complemento && (detalhes?.UnidadeVenda is not (null or UnidadeVenda.Unidade) || detalhes?.Complementos?.Count>0))return Resultado<Produto>.Falha("Complementos usam a medida UN e não podem conter outros complementos.");
        if(detalhes?.Validar() is string erro)return Resultado<Produto>.Falha(erro);
        if(!string.IsNullOrWhiteSpace(detalhes?.CodigoPersonalizado)&&_produtos.CodigoEmUso(detalhes.CodigoPersonalizado.Trim()))return Resultado<Produto>.Falha("Já existe um produto com este código personalizado.");
        var agora = _relogio.UtcNow;
        var produto = new Produto
        {
            Id = 0,
            EhComplemento = complemento,
            CategoriaId = complemento ? null : categoriaId,
            Nome = nomeNormalizado,
            PrecoCentavos = precoCentavos,
            Ativo = ativo,
            CriadoEm = agora,
            AtualizadoEm = agora
        };
        detalhes?.Aplicar(produto);
        try { return Resultado<Produto>.Ok(_produtos.Salvar(produto)); }
        catch(CodigoProdutoDuplicadoException ex){return Resultado<Produto>.Falha(ex.Message);}
    }
}
