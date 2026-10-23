using System.Text.Json;

namespace VarthexComanda.Domain;

public sealed record PagamentoRegistrado
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? Observacao { get; init; }
    public string FormaDescricao => PagamentosConta.Nome(Forma);
    public FormaPagamento Forma { get; init; }
    public long ValorCentavos { get; init; }
    public long RecebidoCentavos { get; init; }
    public DateTime RegistradoEm { get; init; }
    public int[] ItensIds { get; init; } = [];
    public bool Cancelado { get; init; }
    public long TrocoCentavos => RecebidoCentavos-ValorCentavos;
    public string Descricao => $"{PagamentosConta.Nome(Forma)} · R$ {(ValorCentavos/100m).ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))}";
}

public static class PagamentosConta
{
    public static List<PagamentoRegistrado> Ler(string? json)=>string.IsNullOrWhiteSpace(json)?[]:JsonSerializer.Deserialize<List<PagamentoRegistrado>>(json)??[];
    public static long Total(string? json)=>Ler(json).Where(p=>!p.Cancelado).Sum(p=>p.ValorCentavos);
    public static string Adicionar(string? json,PagamentoRegistrado pagamento,long totalConta)
    {
        var lista=Ler(json);
        if(lista.Any(p=>p.Id==pagamento.Id))return json??"[]";
        if(lista.Count>=1000)throw new InvalidOperationException("Limite de registros de pagamento atingido.");
        if(pagamento.Observacao?.Length>500)throw new ArgumentException("A observação deve ter até 500 caracteres.");
        if(pagamento.Cancelado||pagamento.Id==Guid.Empty||!Enum.IsDefined(pagamento.Forma)||pagamento.ValorCentavos<=0||pagamento.RecebidoCentavos<pagamento.ValorCentavos)
            throw new ArgumentException("Informe uma forma e um valor de pagamento válidos.");
        if(pagamento.Forma!=FormaPagamento.Dinheiro&&pagamento.RecebidoCentavos!=pagamento.ValorCentavos)
            throw new ArgumentException("Troco é permitido somente em dinheiro.");
        if(pagamento.ItensIds.Any(id=>id<=0)||pagamento.ItensIds.Distinct().Count()!=pagamento.ItensIds.Length)
            throw new ArgumentException("Seleção de itens inválida.");
        if(lista.Where(p=>!p.Cancelado).SelectMany(p=>p.ItensIds).Intersect(pagamento.ItensIds).Any())
            throw new ArgumentException("Um dos itens selecionados já foi pago. Confira os recebimentos.");
        var restante=totalConta-lista.Where(p=>!p.Cancelado).Sum(p=>p.ValorCentavos);
        if(pagamento.ValorCentavos>restante)throw new ArgumentException("O pagamento não pode superar o valor que falta pagar.");
        lista.Add(pagamento);return JsonSerializer.Serialize(lista);
    }
    public static string Remover(string? json,Guid id)
    {
        var lista=Ler(json);var indice=lista.FindIndex(p=>p.Id==id&&!p.Cancelado);
        if(indice<0)throw new ArgumentException("Registro de pagamento não encontrado.");
        lista[indice]=lista[indice] with { Cancelado=true };return JsonSerializer.Serialize(lista);
    }
    public static string Descrever(string? json,FormaPagamento? forma)
    {
        var formas=Ler(json).Where(p=>!p.Cancelado).Select(p=>p.Forma).Distinct().ToArray();
        if(formas.Length==0)return Nome(forma);
        return string.Join(" + ",formas.Select(f=>Nome(f)));
    }
    public static string Nome(FormaPagamento? forma)=>forma switch { FormaPagamento.Dinheiro=>"Dinheiro",FormaPagamento.Pix=>"Pix",FormaPagamento.Debito=>"Débito",FormaPagamento.Credito=>"Crédito",_=>"Não informado" };
}
