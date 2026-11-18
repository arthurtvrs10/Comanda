using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Servidor;

public sealed record ProdutoRemoto(int Id, string Nome, int CategoriaId, long PrecoCentavos, string Medida, bool TemFoto, IReadOnlyList<GrupoComplementos>? Complementos = null);
public sealed record CategoriaRemota(int Id, string Nome);
public sealed record ItemRemoto(int Id, string Nome, string Quantidade, string Medida, string? Observacao, long SubtotalCentavos);
public sealed record ComandaRemota(int Id, int Numero, int? Mesa, string? Cliente, bool EmFechamento, long TotalCentavos, IReadOnlyList<ItemRemoto> Itens);
public sealed record AberturaRemota(int Numero);
public sealed record LancamentoRemoto(int ProdutoId, decimal Quantidade, string? Observacao, string? Operador, IReadOnlyList<Guid>? Complementos = null);
public sealed record RespostaRemota(int Status, object Dados);

/// <summary>Reutiliza os casos de uso existentes. Não expõe entidades, pagamentos ou administração.</summary>
public sealed class AtendimentoRemoto(IComandaRepository comandas, IProdutoRepository produtos,
    ICategoriaRepository categorias, AbrirComanda abrir, AdicionarItem adicionar, ObterConfiguracao configuracao, FotosProdutos? fotos = null,IConfiguracaoRepository? configuracoes=null)
{
    public int QuantidadeMesas=>configuracoes is null?20:new QrMesasService(configuracoes).Quantidade;
    public void DefinirQuantidadeMesas(int quantidade)=>(configuracoes is null?throw new InvalidOperationException("Configurações indisponíveis."):new QrMesasService(configuracoes)).DefinirQuantidade(quantidade);
    public string TokenMesa(int mesa)=>(configuracoes is null?throw new InvalidOperationException("Configurações indisponíveis."):new QrMesasService(configuracoes)).Token(mesa);
    public bool AutorizarMesa(int mesa,string? token)=>configuracoes is not null&&new QrMesasService(configuracoes).Autorizar(mesa,token);
    public RespostaRemota ChamarMesa(int mesa,PedidoChamada pedido)
    {
        if(configuracoes is null)return Erro(503,"Chamadas indisponíveis.");
        try{return new(200,new ChamadasMesasService(configuracoes).Registrar(-mesa,mesa,pedido.Solicitacao,pedido.Identificacao));}
        catch(ArgumentException ex){return Erro(400,ex.Message);}
        catch(InvalidOperationException ex){return Erro(409,ex.Message);}
    }
    public object SituacaoMesa(int mesa)=>new {Mesa=mesa,Chamada=configuracoes is null?null:new ChamadasMesasService(configuracoes).Listar().FirstOrDefault(c=>c.Mesa==mesa)};
    public RespostaRemota ChamarAtendente(int id,string? solicitacao,Guid identificacao)
    {
        if(configuracoes is null)return Erro(503,"Chamadas indisponíveis neste computador.");
        var pedido=comandas.BuscarComItens(id)?.Comanda;
        if(pedido is null||pedido.Status!=StatusComanda.Aberta)return Erro(404,"Comanda não encontrada ou já encerrada.");
        if(pedido.NumeroMesa is not int mesa)return Erro(409,"Vincule uma mesa à comanda no computador para chamar o atendente.");
        try{return new(200,new ChamadasMesasService(configuracoes).Registrar(id,mesa,solicitacao,identificacao));}
        catch(ArgumentException ex){return Erro(400,ex.Message);}
        catch(InvalidOperationException ex){return Erro(409,ex.Message);}
    }
    public object Catalogo()
    {
        var ativas = categorias.ListarAtivas();
        var ids = ativas.Select(c => c.Id).ToHashSet();
        return new { Categorias = ativas.Select(c => new CategoriaRemota(c.Id,c.Nome)).ToArray(),
            Produtos = produtos.Pesquisar(null,null).Where(p => !p.EhComplemento && p.Ativo && ids.Contains(p.CategoriaId??0))
                .Select(p => new ProdutoRemoto(p.Id,p.Nome,p.CategoriaId??0,p.PrecoCentavos,p.Medida,fotos is not null && FotosProdutos.NomePermitido(p.FotoArquivo),p.GruposComplementos)).ToArray() };
    }
    public FotoRemota? Foto(int id)
    {
        var produto = produtos.BuscarPorId(id);
        return produto?.Ativo == true && !produto.EhComplemento && categorias.BuscarPorId(produto.CategoriaId??0)?.Ativo == true
            ? fotos?.Ler(produto.FotoArquivo) : null;
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
        if (produto is null || categorias.BuscarPorId(produto.CategoriaId??0)?.Ativo != true)
            return Erro(409,"Produto indisponível.");
        var resultado = adicionar.Executar(id,pedido.ProdutoId,pedido.Quantidade,pedido.Observacao,nome,pedido.Complementos);
        return resultado.Sucesso ? new(200,Resumir(resultado.Valor!.Comanda,resultado.Valor.Itens)) : Erro(409,string.Join(" ",resultado.Erros));
    }
    private static ComandaRemota Resumir(Comanda c, IReadOnlyList<ItemComanda> itens) => new(c.Id,c.Numero,c.NumeroMesa,c.NomeCliente,c.Bloqueada,c.TotalCentavos,
        itens.Select(i => new ItemRemoto(i.Id,i.NomeProduto,i.QuantidadeFormatada,i.Medida,i.Observacao,i.SubtotalCentavos)).ToArray());
    internal static RespostaRemota Erro(int status, string mensagem) => new(status,new { Erro = mensagem });
}

public sealed record PedidoChamada(string? Solicitacao,Guid Identificacao);

