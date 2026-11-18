using Microsoft.EntityFrameworkCore;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Catalogo;

public class EfProdutoRepository : IProdutoRepository
{
    private readonly IDbContextFactory<VarthexComandaDbContext> _fabricaContexto;

    public EfProdutoRepository(IDbContextFactory<VarthexComandaDbContext> fabricaContexto)
    {
        _fabricaContexto = fabricaContexto;
    }

    public Produto? BuscarPorId(int id)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var produto=contexto.Produtos.SingleOrDefault(p => p.Id == id);
        if(produto is not null)ResolverReferencias(contexto,[produto]);
        return produto;
    }

    public IReadOnlyList<Produto> Pesquisar(int? categoriaId, string? texto)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        var consulta = contexto.Produtos.AsQueryable();

        if (categoriaId is not null)
        {
            consulta = consulta.Where(p => p.CategoriaId == categoriaId);
        }

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var textoBusca = texto.Trim().ToLower();
            consulta = consulta.Where(p => p.Nome.ToLower().Contains(textoBusca)||(p.CodigoPersonalizado!=null&&p.CodigoPersonalizado.ToLower().Contains(textoBusca)));
        }

        var encontrados=consulta.OrderBy(p => p.Nome).ToList();
        ResolverReferencias(contexto,encontrados);
        return encontrados;
    }

    private static void ResolverReferencias(VarthexComandaDbContext contexto, IReadOnlyList<Produto> produtos)
    {
        if(!produtos.Any(p=>!p.EhComplemento && p.ComplementosJson!="[]"))return;
        var catalogo=contexto.Produtos.AsNoTracking().Where(p=>p.EhComplemento).ToDictionary(p=>p.Id);
        foreach(var produto in produtos.Where(p=>!p.EhComplemento && p.ComplementosJson!="[]"))
            produto.GruposComplementos=produto.GruposComplementos.Select(g=>
            {
                var opcoes=g.Opcoes.Select(o=>
                o.ComplementoId is not int id ? o : catalogo.TryGetValue(id,out var c)
                    ? o with { Nome=c.Nome,PrecoCentavos=c.PrecoCentavos,Disponivel=c.Ativo && (!c.ControlarEstoque || c.QuantidadeEstoque>0),QuantidadeDisponivel=c.ControlarEstoque?c.QuantidadeEstoque:null,MinimoQuantidade=c.MinimoComplemento,MaximoQuantidade=c.MaximoComplemento }
                    : o with { Disponivel=false }).ToList();
                return opcoes.All(o=>o.ComplementoId is not null)
                    ?g with { Minimo=0,Maximo=(int)Math.Min(int.MaxValue,opcoes.Sum(o=>(decimal)(o.MaximoQuantidade??o.QuantidadeDisponivel??999))),Opcoes=opcoes }
                    :g with { Opcoes=opcoes };
            }).ToList();
    }

    public void PrepararCatalogoComplementos()
    {
        using var contexto=_fabricaContexto.CreateDbContext();
        using var transacao=contexto.Database.BeginTransaction();
        var produtos=contexto.Produtos.Where(p=>!p.EhComplemento).ToList();
        var existentes=contexto.Produtos.Where(p=>p.EhComplemento && p.OrigemComplementoId!=null).ToDictionary(p=>p.OrigemComplementoId!.Value);
        var vinculos=new List<(Produto Produto,List<GrupoComplementos> Grupos)>();
        foreach(var produto in produtos.Where(p=>p.ComplementosJson!="[]"))
        {
            var grupos=produto.GruposComplementos;
            if(grupos.SelectMany(g=>g.Opcoes).All(o=>o.ComplementoId is not null))continue;
            foreach(var opcao in grupos.SelectMany(g=>g.Opcoes).Where(o=>o.ComplementoId is null))
            {
                var origem=Origem(opcao);
                if(existentes.ContainsKey(origem))continue;
                var complemento=new Produto { Id=0,EhComplemento=true,OrigemComplementoId=origem,CategoriaId=null,Nome=opcao.Nome,
                    PrecoCentavos=opcao.PrecoCentavos,Ativo=true,CriadoEm=produto.CriadoEm,AtualizadoEm=produto.AtualizadoEm };
                existentes.Add(origem,complemento);contexto.Produtos.Add(complemento);
            }
            vinculos.Add((produto,grupos));
        }
        contexto.SaveChanges();
        foreach(var vinculo in vinculos)
            vinculo.Produto.GruposComplementos=vinculo.Grupos.Select(g=>g with { Opcoes=g.Opcoes.Select(o=>o.ComplementoId is not null?o:o with { ComplementoId=existentes[Origem(o)].Id }).ToList() }).ToList();
        contexto.SaveChanges();transacao.Commit();
    }
    private static Guid Origem(OpcaoComplemento o) => new(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{o.Id:N}|{o.Nome}|{o.PrecoCentavos}")).AsSpan(0,16));

    public IReadOnlyList<string> ListarNomesDeFotos()
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        return contexto.Produtos
            .AsNoTracking()
            .Where(p => p.FotoArquivo != null && p.FotoArquivo != "")
            .Select(p => p.FotoArquivo!)
            .ToList();
    }

    public Produto Salvar(Produto produto)
    {
        using var contexto = _fabricaContexto.CreateDbContext();
        if(produto.EhComplemento)produto.CategoriaId=null;
        var originalVersao=produto.VersaoEstoque;
        if(produto.EhComplemento && produto.Id!=0)produto.VersaoEstoque=checked(originalVersao+1);
        if (produto.Id == 0)
        {
            contexto.Produtos.Add(produto);
        }
        else
        {
            contexto.Produtos.Update(produto);
            contexto.Entry(produto).Property(p=>p.VersaoEstoque).OriginalValue=originalVersao;
        }
        try { contexto.SaveChanges(); }
        catch(DbUpdateConcurrencyException ex) { throw new InvalidOperationException("O estoque foi atualizado por outro pedido. Recarregue o cadastro e tente novamente.",ex); }
        catch(DbUpdateException ex) when(ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteExtendedErrorCode:2067 }&&!string.IsNullOrEmpty(produto.CodigoPersonalizado))
        { throw new CodigoProdutoDuplicadoException(); }
        return produto;
    }
    public bool CodigoEmUso(string codigo,int? excetoId=null)
    {
        using var contexto=_fabricaContexto.CreateDbContext();
        return contexto.Produtos.Any(p=>p.CodigoPersonalizado==codigo&&(excetoId==null||p.Id!=excetoId));
    }
}
