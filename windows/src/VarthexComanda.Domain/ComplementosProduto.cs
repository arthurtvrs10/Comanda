using System.Text.Json;

namespace VarthexComanda.Domain;

public sealed record OpcaoComplemento
{
    public int? ComplementoId { get; init; }
    public long? QuantidadeDisponivel { get; init; }
    public int MinimoQuantidade { get; init; }
    public int? MaximoQuantidade { get; init; }
    public bool Disponivel { get; init; } = true;
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Nome { get; init; } = "";
    public long PrecoCentavos { get; init; }
}

public sealed record GrupoComplementos
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Nome { get; init; } = "";
    public int Minimo { get; init; }
    public int Maximo { get; init; } = 1;
    public List<OpcaoComplemento> Opcoes { get; init; } = [];
}

public sealed record ComplementoCobrado(Guid GrupoId, Guid OpcaoId, string Grupo, string Nome, long PrecoCentavos, int? ComplementoId = null);

public static class ComplementosProduto
{
    public static List<GrupoComplementos> Ler(string json) => JsonSerializer.Deserialize<List<GrupoComplementos>>(json) ?? [];
    public static string Gravar(IReadOnlyList<GrupoComplementos> grupos) => JsonSerializer.Serialize(grupos);

    public static string? Validar(IReadOnlyList<GrupoComplementos> grupos, UnidadeVenda unidade)
    {
        if (grupos.Count > 10) return "Use até 10 grupos de complementos por produto.";
        if (grupos.Count > 0 && unidade != UnidadeVenda.Unidade) return "Complementos estão disponíveis para produtos vendidos por unidade.";
        var ids = new HashSet<Guid>();
        foreach (var g in grupos)
        {
            if (g.Id == Guid.Empty || !ids.Add(g.Id)) return "Os grupos devem ter identificadores únicos.";
            if (string.IsNullOrWhiteSpace(g.Nome) || g.Nome.Trim().Length > 80) return "Informe um nome de até 80 caracteres para cada grupo.";
            if (g.Opcoes.Count is < 1 or > 30) return "Cada grupo deve conter de 1 a 30 opções.";
            if (g.Minimo < 0 || g.Maximo < 0 || g.Minimo > g.Maximo) return $"Revise os limites de seleção do grupo {g.Nome}.";
            foreach (var o in g.Opcoes)
            {
                if(o.MinimoQuantidade<0 || o.MaximoQuantidade<0 || o.MaximoQuantidade is int maximo && o.MinimoQuantidade>maximo)return $"Revise os limites de {o.Nome}.";
                if (o.Id == Guid.Empty || !ids.Add(o.Id)) return "As opções devem ter identificadores únicos.";
                if (string.IsNullOrWhiteSpace(o.Nome) || o.Nome.Trim().Length > 80) return "Informe um nome de até 80 caracteres para cada opção.";
                if (o.PrecoCentavos is < 0 or > 100_000_000) return "O preço de um complemento deve estar entre R$ 0,00 e R$ 1.000.000,00.";
            }
        }
        return null;
    }

    public static void VerificarEstoque(Produto produto,IReadOnlyList<Guid> ids,decimal quantidade)
    {
        Selecionar(produto,ids);
        var opcoes=produto.GruposComplementos.SelectMany(g=>g.Opcoes).ToDictionary(o=>o.Id);
        var escolhas=ids.Select(id=>opcoes[id]);
        foreach(var grupo in escolhas.GroupBy(o=>o.ComplementoId is int id?$"c{id}":$"o{o.Id:N}"))
        {
            var saldos=grupo.Where(o=>o.QuantidadeDisponivel is not null).Select(o=>o.QuantidadeDisponivel!.Value).ToArray();
            if(saldos.Length==0)continue;
            var saldo=saldos.Min();
            if(quantidade*grupo.Count()>saldo)throw new InvalidOperationException($"Estoque insuficiente para {grupo.First().Nome}. Disponível: {saldo} UN.");
        }
    }

    public static IReadOnlyList<ComplementoCobrado> Selecionar(Produto produto, IReadOnlyList<Guid>? ids)
    {
        var grupos = produto.GruposComplementos;
        if (Validar(grupos, produto.UnidadeVenda) is string erro) throw new InvalidOperationException(erro);
        ids ??= [];
        if (ids.Count > 10000) throw new InvalidOperationException("Use até 10.000 unidades de complementos por item.");
        if (ids.Any(id => !grupos.SelectMany(g=>g.Opcoes).Any(o=>o.Id==id && o.Disponivel))) throw new InvalidOperationException("Um complemento não está mais disponível. Selecione novamente.");
        foreach(var conjunto in grupos.SelectMany(g=>g.Opcoes).GroupBy(o=>o.ComplementoId is int codigo?$"c{codigo}":$"o{o.Id:N}"))
        {
            var opcoesPermitidas=conjunto.ToArray();var quantidade=ids.Count(id=>opcoesPermitidas.Any(o=>o.Id==id));
            var minimo=opcoesPermitidas.Max(o=>o.MinimoQuantidade);
            var maximos=opcoesPermitidas.Where(o=>o.MaximoQuantidade is not null).Select(o=>o.MaximoQuantidade!.Value).ToArray();
            if(quantidade<minimo)throw new InvalidOperationException($"{opcoesPermitidas[0].Nome}: selecione pelo menos {minimo} unidade(s).");
            if(maximos.Length>0 && quantidade>maximos.Min())throw new InvalidOperationException($"{opcoesPermitidas[0].Nome}: selecione no máximo {maximos.Min()} unidade(s).");
        }
        var resultado = new List<ComplementoCobrado>();
        foreach (var g in grupos.OrderBy(g=>g.Id))
        {
            if(g.Opcoes.Where(o=>o.Disponivel).Sum(o=>(decimal)(o.QuantidadeDisponivel??g.Maximo))<g.Minimo)throw new InvalidOperationException($"{g.Nome}: os complementos obrigatórios estão indisponíveis. Confira o estoque.");
            var opcoes = g.Opcoes.Where(o=>o.Disponivel).SelectMany(o=>Enumerable.Repeat(o,ids.Count(id=>id==o.Id))).OrderBy(o=>o.Id).ToArray();
            if (opcoes.Length < g.Minimo || opcoes.Length > g.Maximo) throw new InvalidOperationException($"{g.Nome}: escolha de {g.Minimo} a {g.Maximo} opções.");
            resultado.AddRange(opcoes.Select(o=>new ComplementoCobrado(g.Id,o.Id,g.Nome.Trim(),o.Nome.Trim(),o.PrecoCentavos,o.ComplementoId)));
        }
        return resultado;
    }
}
