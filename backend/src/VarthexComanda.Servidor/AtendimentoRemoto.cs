using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Servidor;

public sealed record ProdutoRemoto(int Id, string Nome, int CategoriaId, long PrecoCentavos, string Medida);
public sealed record CategoriaRemota(int Id, string Nome);
public sealed record ItemRemoto(int Id, string Nome, string Quantidade, string Medida, string? Observacao, long SubtotalCentavos);
public sealed record ComandaRemota(int Id, int Numero, int? Mesa, string? Cliente, bool EmFechamento, long TotalCentavos, IReadOnlyList<ItemRemoto> Itens);
public sealed record AberturaRemota(int Numero);
public sealed record LancamentoRemoto(int ProdutoId, decimal Quantidade, string? Observacao, string? Operador);
public sealed record RespostaRemota(int Status, object Dados);

/// <summary>Reutiliza os casos de uso existentes. Não expõe entidades, pagamentos ou administração.</summary>
public sealed class AtendimentoRemoto(IComandaRepository comandas, IProdutoRepository produtos,
    ICategoriaRepository categorias, AbrirComanda abrir, AdicionarItem adicionar, ObterConfiguracao configuracao)
{
    public object Catalogo()
    {
        var ativas = categorias.ListarAtivas();
        var ids = ativas.Select(c => c.Id).ToHashSet();
        return new { Categorias = ativas.Select(c => new CategoriaRemota(c.Id,c.Nome)).ToArray(),
            Produtos = produtos.Pesquisar(null,null).Where(p => p.Ativo && ids.Contains(p.CategoriaId))
                .Select(p => new ProdutoRemoto(p.Id,p.Nome,p.CategoriaId,p.PrecoCentavos,p.Medida)).ToArray() };
    }
    public object Comandas()
    {
        var config = configuracao.Executar();
        return new { PrimeiroNumero = config.NumeroInicialComandas, Quantidade = config.QuantidadeMaximaComandas ?? 20,
            Comandas = comandas.ListarAbertas().Select(c => Resumir(c,[])).ToArray() };
    }
    public RespostaRemota Consultar(int id)
    {
        var estado = comandas.BuscarComItens(id);
        return estado is null || estado.Comanda.Status != StatusComanda.Aberta
            ? Erro(404,"Comanda não encontrada ou já encerrada.") : new(200,Resumir(estado.Comanda,estado.Itens));
    }
    public RespostaRemota Abrir(AberturaRemota pedido)
    {
        var resultado = abrir.Executar(pedido.Numero);
        return resultado.Sucesso ? new(201,Resumir(resultado.Valor!,[])) : Erro(409,string.Join(" ",resultado.Erros));
    }
    public RespostaRemota Adicionar(int id, LancamentoRemoto pedido)
    {
        var nome = pedido.Operador?.Trim();
        if (string.IsNullOrWhiteSpace(nome) || nome.Length > 80 || nome.Any(char.IsControl))
            return Erro(400,"Informe o nome do atendente com até 80 caracteres.");
        var produto = produtos.BuscarPorId(pedido.ProdutoId);
        if (produto is null || categorias.BuscarPorId(produto.CategoriaId)?.Ativo != true)
            return Erro(409,"Produto indisponível.");
        var resultado = adicionar.Executar(id,pedido.ProdutoId,pedido.Quantidade,pedido.Observacao,nome);
        return resultado.Sucesso ? new(200,Resumir(resultado.Valor!.Comanda,resultado.Valor.Itens)) : Erro(409,string.Join(" ",resultado.Erros));
    }
    private static ComandaRemota Resumir(Comanda c, IReadOnlyList<ItemComanda> itens) => new(c.Id,c.Numero,c.NumeroMesa,c.NomeCliente,c.Bloqueada,c.TotalCentavos,
        itens.Select(i => new ItemRemoto(i.Id,i.NomeProduto,i.QuantidadeFormatada,i.Medida,i.Observacao,i.SubtotalCentavos)).ToArray());
    internal static RespostaRemota Erro(int status, string mensagem) => new(status,new { Erro = mensagem });
}
