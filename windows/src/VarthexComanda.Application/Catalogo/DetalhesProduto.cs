using VarthexComanda.Domain;

namespace VarthexComanda.Application.Catalogo;

public sealed record DetalhesProduto(long CustoCentavos=0,string? Descricao=null,string? CodigoPersonalizado=null,UnidadeVenda UnidadeVenda=UnidadeVenda.Unidade,IReadOnlyList<GrupoComplementos>? Complementos=null,bool? ControlarEstoque=null,long? QuantidadeEstoque=null,long? VersaoEstoqueEsperada=null,int MinimoComplemento=0,int? MaximoComplemento=null)
{
    public string? Validar()
    {
        if(!Enum.IsDefined(UnidadeVenda))return "Selecione uma unidade de venda válida.";
        if(QuantidadeEstoque<0)return "A quantidade em estoque não pode ser negativa.";
        if(MinimoComplemento<0 || MaximoComplemento<0 || MaximoComplemento is int maximo && MinimoComplemento>maximo)return "O mínimo deve ser zero ou maior e não pode ultrapassar o máximo do complemento.";
        if(CustoCentavos<0)return "O preço de custo não pode ser negativo.";
        if(Descricao?.Trim().Length>2000)return "Use até 2.000 caracteres na descrição.";
        var codigo=CodigoPersonalizado?.Trim();
        if(codigo?.Length>40 || (codigo?.Any(c=>!char.IsAsciiLetterOrDigit(c)&&c is not ('-' or '_' or '.' or '/'))??false))
            return "Use até 40 letras, números, ponto, hífen, barra ou sublinhado no código personalizado.";
        return ComplementosProduto.Validar(Complementos ?? [], UnidadeVenda);
    }
    public void Aplicar(Produto produto)
    {
        produto.GruposComplementos=ComplementosProduto.Ler(ComplementosProduto.Gravar(Complementos ?? []));
        if(ControlarEstoque is bool controlar)produto.ControlarEstoque=controlar;
        if(QuantidadeEstoque is long quantidade)produto.QuantidadeEstoque=quantidade;
        if(produto.EhComplemento) { produto.MinimoComplemento=MinimoComplemento;produto.MaximoComplemento=MaximoComplemento; }
        produto.CustoCentavos=CustoCentavos;
        produto.UnidadeVenda=UnidadeVenda;
        produto.Descricao=string.IsNullOrWhiteSpace(Descricao)?null:Descricao.Trim();
        produto.CodigoPersonalizado=string.IsNullOrWhiteSpace(CodigoPersonalizado)?null:CodigoPersonalizado.Trim();
    }
}

public sealed class CodigoProdutoDuplicadoException():InvalidOperationException("Já existe um produto com este código personalizado.");
