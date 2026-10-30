namespace VarthexComanda.Domain;

public enum TipoPedido { LocalMesa=0, Salao=1, Balcao=2, ParaLevar=3, Delivery=4 }
public static class TiposPedido
{
    public static string Descricao(this TipoPedido tipo)=>tipo switch
    {
        TipoPedido.LocalMesa=>"Local (Mesa)",TipoPedido.Salao=>"Salão",TipoPedido.Balcao=>"Balcão",TipoPedido.ParaLevar=>"Para levar",TipoPedido.Delivery=>"Delivery",_=>throw new ArgumentOutOfRangeException(nameof(tipo),"Selecione um tipo de pedido válido.")
    };
}
