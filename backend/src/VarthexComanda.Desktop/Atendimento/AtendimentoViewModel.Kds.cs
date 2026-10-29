using VarthexComanda.Application.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;
public partial class AtendimentoViewModel
{
    public ComandaComItens? ConsultarPedidoKds(int id)=>_comandas.BuscarComItens(id);
    public void CancelarPedidoKds(int id)
    {
        if(!Exigir("cancelar-pedido"))return;
        var pedido=_comandas.BuscarComItens(id);
        if(pedido?.Comanda.Status!=StatusComanda.Aberta)return;
        if(!_confirmador.Confirmar("Cancelar pedido",$"Cancelar a comanda {pedido.Comanda.Numero}?\n\nO pedido inteiro será cancelado, incluindo itens de outras etapas. O número ficará livre."))return;
        var resultado=_cancelarComanda.Executar(id);
        if(!resultado.Sucesso)throw new InvalidOperationException(string.Join(" ",resultado.Erros));
        if(ComandaAtual?.Id==id)LimparEdicao();
        AtualizarComandasAbertas();
    }
}
