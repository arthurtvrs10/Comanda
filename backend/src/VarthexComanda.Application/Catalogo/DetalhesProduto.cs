using VarthexComanda.Domain;

namespace VarthexComanda.Application.Catalogo;

public sealed record DetalhesProduto(long CustoCentavos=0,string? Descricao=null,string? CodigoPersonalizado=null)
{
    public string? Validar()
    {
        if(CustoCentavos<0)return "O preço de custo não pode ser negativo.";
        if(Descricao?.Trim().Length>2000)return "Use até 2.000 caracteres na descrição.";
        var codigo=CodigoPersonalizado?.Trim();
        if(codigo?.Length>40 || (codigo?.Any(c=>!char.IsAsciiLetterOrDigit(c)&&c is not ('-' or '_' or '.' or '/'))??false))
            return "Use até 40 letras, números, ponto, hífen, barra ou sublinhado no código personalizado.";
        return null;
    }
    public void Aplicar(Produto produto)
    {
        produto.CustoCentavos=CustoCentavos;
        produto.Descricao=string.IsNullOrWhiteSpace(Descricao)?null:Descricao.Trim();
        produto.CodigoPersonalizado=string.IsNullOrWhiteSpace(CodigoPersonalizado)?null:CodigoPersonalizado.Trim();
    }
}

public sealed class CodigoProdutoDuplicadoException():InvalidOperationException("Já existe um produto com este código personalizado.");
