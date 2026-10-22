using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;
using VarthexComanda.Desktop.Atendimento;

namespace VarthexComanda.Desktop.Configuracao;

/// <summary>Documentos vetoriais de impressão não fiscal, dimensionados em milímetros.</summary>
public static class CupomTermico
{
    public static double Largura(int mm) => mm is 58 or 80 ? mm*96d/25.4 : throw new ArgumentException("Selecione papel de 58 mm ou 80 mm.");
    private static readonly System.Globalization.CultureInfo Cultura=System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
    private static FlowDocument Novo(int mm) => new() { PageWidth=Largura(mm),ColumnWidth=Largura(mm)-16,PagePadding=new Thickness(8),FontFamily=new FontFamily("Consolas"),FontSize=mm==58?10:11,Foreground=Brushes.Black,Background=Brushes.White,TextAlignment=TextAlignment.Left };
    public static void AjustarLargura(FlowDocument documento,double larguraDisponivel)
    {
        if(!double.IsFinite(larguraDisponivel)||larguraDisponivel<=16)throw new ArgumentException("Área de impressão inválida.");
        documento.PageWidth=Math.Min(documento.PageWidth,larguraDisponivel);
        var util=documento.PageWidth-documento.PagePadding.Left-documento.PagePadding.Right;documento.ColumnWidth=util;
        foreach(var tabela in documento.Blocks.OfType<Table>())
        {
            var total=tabela.Columns.Sum(c=>c.Width.Value);
            if(total>util)foreach(var coluna in tabela.Columns)coluna.Width=new GridLength(coluna.Width.Value*util/total);
        }
    }
    private static Paragraph Linha(FlowDocument d,string? texto,bool forte=false,double? tamanho=null,bool centro=false)
    {
        var p=new Paragraph(new Run(texto?.Trim()??"")) { Margin=new Thickness(0,2,0,2),FontWeight=forte?FontWeights.Bold:FontWeights.Normal,TextAlignment=centro?TextAlignment.Center:TextAlignment.Left,KeepTogether=true };
        if(tamanho is double f)p.FontSize=f;d.Blocks.Add(p);return p;
    }
    private static void Regua(FlowDocument d) => d.Blocks.Add(new Paragraph { Margin=new Thickness(0,7,0,7),BorderBrush=Brushes.Black,BorderThickness=new Thickness(0,0,0,0.7),FontSize=1,LineHeight=1 });
    private static string Juntar(string separador,params string?[] valores)=>string.Join(separador,valores.Where(v=>!string.IsNullOrWhiteSpace(v)).Select(v=>v!.Trim()));
    private static void Cabecalho(FlowDocument d,PreferenciasSistema p,string nome)
    {
        if(!string.IsNullOrWhiteSpace(nome))Linha(d,nome,true,15,true);
        if(p.ExibirDocumentoCupom&&!string.IsNullOrWhiteSpace(p.Documento))Linha(d,p.Documento,centro:true);
        if(p.ExibirEnderecoCupom)
        {
            var local=Juntar(" · ",Juntar(", ",p.Endereco,p.Numero),p.Bairro,Juntar("/",p.Cidade,p.Uf));
            if(local.Length>0)Linha(d,local,tamanho:9,centro:true);
        }
        Regua(d);
    }
    private static void Identificar(FlowDocument d,Comanda c)
    {
        Linha(d,c.NumeroMesa is int mesa?$"MESA {mesa:00}":"BALCÃO",true,18);
        Linha(d,$"COMANDA {c.Numero:00}"+(c.CodigoPedido is long codigo?$" · Pedido: {codigo}":""),true);
        Linha(d,FusoBrasilia.ParaLocal(DateTime.UtcNow).ToString("dd/MM/yyyy HH:mm",Cultura),tamanho:9);
    }
    private static void Marca(FlowDocument d)
    {
        Regua(d);
        var logo=new Image { Source=new BitmapImage(new Uri("pack://application:,,,/VarthexComanda;component/Assets/LogoAjudaClara.png")),Width=105,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center };
        d.Blocks.Add(new BlockUIContainer(logo) { Margin=new Thickness(0,3,0,2) });
        Linha(d,"SISTEMA DE GESTÃO",true,8,true);
    }
    public static FlowDocument Conta(PreferenciasSistema p,string nome,Comanda comanda,IEnumerable<ItemComanda> itens,int mm,int? senha=null)
    {
        var lista=itens.ToArray();var d=Novo(mm);Cabecalho(d,p,nome);
        Linha(d,string.IsNullOrWhiteSpace(p.CabecalhoCupom)?"CONFERÊNCIA DA CONTA":p.CabecalhoCupom,true,12,true);
        Regua(d);Identificar(d,comanda);Regua(d);
        var grupos=p.AgruparItensCupom?lista.GroupBy(i=>new{i.ProdutoId,i.NomeProduto,i.PrecoUnitarioCentavos,i.Observacao,i.UnidadeVenda})
            .Select(g=>(Nome:g.Key.NomeProduto,Qtd:g.Sum(i=>i.Quantidade),Unit:g.Key.PrecoUnitarioCentavos,Total:g.Sum(i=>i.SubtotalCentavos),Obs:g.Key.Observacao,Unidade:g.Key.UnidadeVenda))
            :lista.Select(i=>(Nome:i.NomeProduto,Qtd:i.Quantidade,Unit:i.PrecoUnitarioCentavos,Total:i.SubtotalCentavos,Obs:i.Observacao,Unidade:i.UnidadeVenda));
        foreach(var item in grupos)
        {
            Linha(d,item.Nome,true).Margin=new Thickness(0,8,0,2);
            var tabela=new Table { CellSpacing=0,Margin=new Thickness(0) };
            var util=d.PageWidth-d.PagePadding.Left-d.PagePadding.Right;
            tabela.Columns.Add(new TableColumn { Width=new GridLength(util*.62) });tabela.Columns.Add(new TableColumn { Width=new GridLength(util*.38) });
            var grupo=new TableRowGroup();var linha=new TableRow();
            linha.Cells.Add(new TableCell(new Paragraph(new Run($"{MedidasVenda.FormatarQuantidade(item.Qtd,item.Unidade)} {item.Unidade.Simbolo()} × {CentavosParaMoedaConverter.Formatar(item.Unit)}")) { Margin=new Thickness(0) }));
            linha.Cells.Add(new TableCell(new Paragraph(new Run(CentavosParaMoedaConverter.Formatar(item.Total))) { TextAlignment=TextAlignment.Right,FontWeight=FontWeights.Bold,Margin=new Thickness(0) }));
            grupo.Rows.Add(linha);tabela.RowGroups.Add(grupo);d.Blocks.Add(tabela);
            if(p.ExibirObservacoesCupom&&!string.IsNullOrWhiteSpace(item.Obs))Linha(d,"Obs.: "+item.Obs,tamanho:9);
        }
        Regua(d);Linha(d,"TOTAL: "+CentavosParaMoedaConverter.Formatar(lista.Sum(i=>i.SubtotalCentavos)),true,17);
        Linha(d,$"{lista.Length} lançamento(s)",tamanho:9);Linha(d,"NÃO É DOCUMENTO FISCAL",tamanho:9,centro:true);
        if(!string.IsNullOrWhiteSpace(p.RodapeCupom))Linha(d,p.RodapeCupom,centro:true);
        if(senha is int s){Regua(d);Linha(d,$"SENHA DE ATENDIMENTO: {s}",true,16,true);}
        Marca(d);return d;
    }
    public static FlowDocument Producao(PreferenciasSistema p,string nome,Comanda c,IEnumerable<ItemComanda> itens,IReadOnlyDictionary<int,decimal> quantidades,bool reimpressao,int mm)
    {
        var d=Novo(mm);Cabecalho(d,p,nome);Linha(d,reimpressao?"REIMPRESSÃO · COZINHA":"COZINHA",true,17,true);Regua(d);Identificar(d,c);
        if(!string.IsNullOrWhiteSpace(c.Observacao))Linha(d,"OBSERVAÇÃO DO PEDIDO\n"+c.Observacao,true).BorderThickness=new Thickness(1);
        foreach(var i in itens.Where(i=>quantidades.TryGetValue(i.Id,out var q)&&q>0))
        {
            Regua(d);Linha(d,$"{MedidasVenda.FormatarQuantidade(quantidades[i.Id],i.UnidadeVenda)} {i.Medida} × {i.NomeProduto}",true,14);
            if(!string.IsNullOrWhiteSpace(i.Observacao))Linha(d,i.Observacao.ToUpper(Cultura),true,12);
        }
        Linha(d,reimpressao?"REIMPRESSÃO — confira antes de preparar novamente.":"Somente itens enviados nesta impressão.",tamanho:9,centro:true);Marca(d);return d;
    }
    public static FlowDocument Fichas(PreferenciasSistema p,string nome,Comanda c,IEnumerable<ItemComanda> itens,IReadOnlyDictionary<int,decimal> quantidades,int mm)
    {
        var d=Novo(mm);var primeira=true;
        foreach(var i in itens.Where(i=>quantidades.TryGetValue(i.Id,out var q)&&q>0))
        {
            var q=quantidades[i.Id];var fichas=i.UnidadeVenda==UnidadeVenda.Unidade?(int)q:1;
            for(var n=1;n<=fichas;n++)
            {
                if(!primeira)Regua(d);primeira=false;Cabecalho(d,p,nome);Linha(d,"FICHA DE CONSUMAÇÃO",true,12,true);Identificar(d,c);Regua(d);
                Linha(d,$"{(i.UnidadeVenda==UnidadeVenda.Unidade?"1":MedidasVenda.FormatarQuantidade(q,i.UnidadeVenda))} {i.Medida} × {i.NomeProduto}",true,15);
                if(!string.IsNullOrWhiteSpace(i.Observacao))Linha(d,i.Observacao,true);
                if(i.UnidadeVenda==UnidadeVenda.Unidade)Linha(d,$"Ficha {i.QuantidadeFicha+n}/{i.Quantidade}",tamanho:9,centro:true);
                Marca(d);
            }
        }
        return d;
    }
    public static FlowDocument Texto(string texto,int mm)
    { var d=Novo(mm);foreach(var linha in texto.Split('\n'))Linha(d,linha);Marca(d);return d; }
}
