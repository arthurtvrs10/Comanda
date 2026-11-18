using System.Text.Json;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Atendimento;

public partial class EfComandaRepository
{
    private static Dictionary<int,long> Reservas(ItemComanda item) => JsonSerializer.Deserialize<Dictionary<int,long>>(item.EstoqueReservadoJson) ?? [];

    private static Dictionary<int,int> Referencias(VarthexComandaDbContext contexto,ItemComanda item)
    {
        var escolhas=JsonSerializer.Deserialize<List<ComplementoCobrado>>(item.ComplementosJson) ?? [];
        var referencias=new List<int>();
        for(var i=0;i<escolhas.Count;i++)
        {
            var escolha=escolhas[i];var id=escolha.ComplementoId;
            if(id is null)
            {
                var produto=contexto.Produtos.Find(item.ProdutoId);
                id=produto?.GruposComplementos.SelectMany(g=>g.Opcoes).FirstOrDefault(o=>o.Id==escolha.OpcaoId)?.ComplementoId;
                if(id is null)
                {
                    var origem=new Guid(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{escolha.OpcaoId:N}|{escolha.Nome}|{escolha.PrecoCentavos}")).AsSpan(0,16));
                    id=contexto.Produtos.Where(p=>p.EhComplemento && p.OrigemComplementoId==origem).Select(p=>(int?)p.Id).SingleOrDefault();
                }
                if(id is not null)escolhas[i]=escolha with { ComplementoId=id };
                else if(contexto.Produtos.Any(p=>p.EhComplemento))
                    throw new InvalidOperationException("Os complementos deste item mudaram. Adicione novamente o produto para atualizar o estoque.");
            }
            if(id is int codigo)referencias.Add(codigo);
        }
        item.ComplementosJson=JsonSerializer.Serialize(escolhas);
        return referencias.GroupBy(id=>id).ToDictionary(g=>g.Key,g=>g.Count());
    }

    private static void ReservarEstoque(VarthexComandaDbContext contexto,ItemComanda item,decimal quantidade)
    {
        if(quantidade<=0)return;
        var referencias=Referencias(contexto,item);
        if(referencias.Count==0)return;
        if(quantidade!=decimal.Truncate(quantidade))throw new InvalidOperationException("Complementos usam quantidades inteiras.");
        var reservas=Reservas(item);
        foreach(var referencia in referencias)
        {
            var complemento=contexto.Produtos.Find(referencia.Key);
            if(complemento is not { EhComplemento:true,Ativo:true })throw new InvalidOperationException("Um complemento não está mais disponível.");
            if(!complemento.ControlarEstoque)continue;
            var consumir=checked((long)quantidade*referencia.Value);
            if(complemento.QuantidadeEstoque<consumir)
                throw new InvalidOperationException($"Estoque insuficiente para {complemento.Nome}. Disponível: {complemento.QuantidadeEstoque} UN.");
            complemento.QuantidadeEstoque-=consumir;
            complemento.VersaoEstoque=checked(complemento.VersaoEstoque+1);
            reservas[referencia.Key]=checked(reservas.GetValueOrDefault(referencia.Key)+consumir);
        }
        item.EstoqueReservadoJson=JsonSerializer.Serialize(reservas);
    }

    private static void DevolverEstoque(VarthexComandaDbContext contexto,ItemComanda item,decimal? quantidade=null)
    {
        if(item.StatusPreparo!=StatusPreparo.Pendente)return;
        var reservas=Reservas(item);
        var escolhas=JsonSerializer.Deserialize<List<ComplementoCobrado>>(item.ComplementosJson) ?? [];
        foreach(var reserva in reservas.ToArray())
        {
            var frequencia=escolhas.Count(c=>c.ComplementoId==reserva.Key);
            var devolver=quantidade is null?reserva.Value:Math.Min(reserva.Value,checked((long)quantidade.Value*Math.Max(1,frequencia)));
            if(devolver<=0)continue;
            var complemento=contexto.Produtos.Find(reserva.Key);
            if(complemento is null)continue;
            complemento.QuantidadeEstoque=checked(complemento.QuantidadeEstoque+devolver);
            complemento.VersaoEstoque=checked(complemento.VersaoEstoque+1);
            reservas[reserva.Key]-=devolver;
            if(reservas[reserva.Key]==0)reservas.Remove(reserva.Key);
        }
        item.EstoqueReservadoJson=JsonSerializer.Serialize(reservas);
    }
}
