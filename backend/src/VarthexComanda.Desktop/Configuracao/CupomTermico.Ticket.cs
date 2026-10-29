using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Configuracao;
public static partial class CupomTermico
{
    public static FlowDocument TicketCozinha(string nome,Comanda comanda,IEnumerable<ItemComanda> itens,IReadOnlyDictionary<int,decimal> quantidades,bool reimpressao,int mm,LayoutTicket layout)
    {
        var documento=Novo(mm);
        if(layout.ImprimirLogo&&layout.LogoBase64.Length>0)
        {
            using var stream=new MemoryStream(Convert.FromBase64String(layout.LogoBase64));
            var imagem=new BitmapImage();imagem.BeginInit();imagem.CacheOption=BitmapCacheOption.OnLoad;imagem.StreamSource=stream;imagem.EndInit();imagem.Freeze();
            documento.Blocks.Add(new BlockUIContainer(new Image {Source=imagem,Width=Math.Min(100,documento.ColumnWidth),MaxHeight=80,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center}){Margin=new Thickness(0,0,0,10)});
        }
        var faixa=Linha(documento,reimpressao?"REIMPRESSÃO · COZINHA":"COZINHA",true,13,true);faixa.Background=Brushes.Black;faixa.Foreground=Brushes.White;faixa.Padding=new Thickness(4,5,4,5);
        if(layout.NomeEstabelecimento&&!string.IsNullOrWhiteSpace(nome))Linha(documento,nome,true,11,true);
        Regua(documento);
        if(layout.NumeroMesa)Linha(documento,comanda.NumeroMesa is int mesa?$"MESA: {mesa:00}":$"BALCÃO · COMANDA {comanda.Numero:00}",true,17);
        if(layout.NomeCliente&&!string.IsNullOrWhiteSpace(comanda.NomeCliente))Linha(documento,"CLIENTE: "+comanda.NomeCliente);
        if(layout.CodigoPedido)Linha(documento,comanda.CodigoPedido is long codigo?$"PEDIDO: #{codigo} · COMANDA {comanda.Numero:00}":$"COMANDA: {comanda.Numero:00}",true,10);
        if(layout.DataHora)Linha(documento,$"DATA/HORA: {FusoBrasilia.ParaLocal(comanda.AbertaEm):dd/MM/yyyy HH:mm}",tamanho:9);
        if(layout.ObservacoesPedido&&!string.IsNullOrWhiteSpace(comanda.Observacao))Linha(documento,"OBS. PEDIDO: "+comanda.Observacao,true);
        Regua(documento);Linha(documento,"ITENS:",true,10);
        foreach(var item in itens.Where(i=>quantidades.TryGetValue(i.Id,out var qtd)&&qtd>0))
        {
            Linha(documento,$"{MedidasVenda.FormatarQuantidade(quantidades[item.Id],item.UnidadeVenda)} {item.Medida} × {item.NomeProduto}",true,mm==58?12:14).Margin=new Thickness(0,8,0,3);
            if(layout.ObservacoesItem&&!string.IsNullOrWhiteSpace(item.Observacao))Linha(documento,"OBS.: "+item.Observacao,true,11);
        }
        if(reimpressao){Regua(documento);Linha(documento,"REIMPRESSÃO — confira antes de preparar novamente.",true,9,true);}
        Marca(documento);return documento;
    }
}
