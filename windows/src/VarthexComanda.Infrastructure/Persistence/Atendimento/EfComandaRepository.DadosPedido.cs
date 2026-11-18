using Microsoft.EntityFrameworkCore;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Atendimento;
public partial class EfComandaRepository
{
    public Comanda DefinirDadosPedido(int comandaId,string? responsavel,string? cliente,TipoPedido tipo,string? observacao)
    {
        if(!Enum.IsDefined(tipo))throw new ArgumentException("Selecione um tipo de pedido válido.");
        cliente=string.IsNullOrWhiteSpace(cliente)?null:cliente.Trim();
        if(cliente?.Length>80||cliente?.Any(char.IsControl)==true)throw new ArgumentException("Informe um cliente com até 80 caracteres, sem quebras de linha.");
        if(responsavel?.Length>120||observacao?.Length>1000)throw new ArgumentException("Use até 120 caracteres no responsável e até 1.000 nas observações.");
        using var contexto=_fabricaContexto.CreateDbContext();var comanda=Aberta(contexto,comandaId);ExigirDesbloqueada(comanda);
        comanda.Responsavel=string.IsNullOrWhiteSpace(responsavel)?null:responsavel.Trim();
        comanda.NomeCliente=cliente;comanda.TipoPedido=tipo;comanda.Observacao=string.IsNullOrWhiteSpace(observacao)?null:observacao.Trim();
        contexto.SaveChanges();return comanda;
    }
}
