using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Configuracao;

public static class ComprovanteConta
{
    public static string MontarTexto(PreferenciasSistema p,string estabelecimento,int numero,IEnumerable<ItemComanda> itens,long? codigoPedido=null,bool senhaAtendimento=false,int? senhaComanda=null)
    {
        var lista=itens.ToList();var linhas=new List<string> { estabelecimento,p.CabecalhoCupom,$"Comanda {numero:00}",DateTime.Now.ToString("dd/MM/yyyy HH:mm") };
        if(codigoPedido is long codigo)linhas.Add($"Pedido: {codigo}");
        if(p.ExibirDocumentoCupom&&!string.IsNullOrWhiteSpace(p.Documento))linhas.Add("Documento: "+p.Documento);
        if(p.ExibirEnderecoCupom)linhas.Add(string.Join(" · ",new[]{p.Endereco,p.Numero,p.Complemento,p.Bairro,p.Cidade,p.Uf,p.Cep}.Where(x=>!string.IsNullOrWhiteSpace(x))));
        linhas.Add(new string('-',38));
        var grupos=p.AgruparItensCupom?lista.GroupBy(i=>new { i.ProdutoId,i.NomeProduto,i.PrecoUnitarioCentavos,i.Observacao,i.UnidadeVenda }).Select(g=>(Nome:g.Key.NomeProduto,Quantidade:g.Sum(i=>i.Quantidade),Subtotal:g.Sum(i=>i.SubtotalCentavos),Observacao:g.Key.Observacao,Unidade:g.Key.UnidadeVenda))
            :lista.Select(i=>(Nome:i.NomeProduto,Quantidade:i.Quantidade,Subtotal:i.SubtotalCentavos,Observacao:i.Observacao,Unidade:i.UnidadeVenda));
        foreach(var item in grupos)
        {
            linhas.Add($"{MedidasVenda.FormatarQuantidade(item.Quantidade,item.Unidade)} {item.Unidade.Simbolo()} × {item.Nome}  {Atendimento.CentavosParaMoedaConverter.Formatar(item.Subtotal)}");
            if(p.ExibirObservacoesCupom&&!string.IsNullOrWhiteSpace(item.Observacao))linhas.Add("  "+item.Observacao);
        }
        linhas.Add(new string('-',38));linhas.Add("TOTAL: "+Atendimento.CentavosParaMoedaConverter.Formatar(lista.Sum(i=>i.SubtotalCentavos)));
        linhas.Add("NÃO É DOCUMENTO FISCAL");linhas.Add(p.RodapeCupom);
        if(senhaComanda is int numeroSenha)linhas.Add($"SENHA DE ATENDIMENTO: {numeroSenha}");
        else if(senhaAtendimento && codigoPedido is long senha)linhas.Add($"SENHA DE ATENDIMENTO: {senha}");
        return string.Join("\n",linhas.Where(x=>!string.IsNullOrWhiteSpace(x)));
    }
    public static void VisualizarExemplo(Window owner,PreferenciasSistema prefs,string nome)=>Visualizar(owner,MontarTexto(prefs,nome,1,[new ItemComanda { Id=0,ComandaId=0,ProdutoId=0,NomeProduto="Produto de exemplo (dados fictícios)",PrecoUnitarioCentavos=1000,Quantidade=1,SubtotalCentavos=1000,Observacao="Exemplo de observação",CriadoEm=DateTime.UtcNow,AtualizadoEm=DateTime.UtcNow }]));
    public static bool Visualizar(Window owner,string texto,PreferenciaImpressora? impressora=null,string titulo="Conferência da conta · Não fiscal")
    {
        var janela=new Window { Title=titulo,Owner=owner,Width=560,Height=700,MinWidth=420,MinHeight=400,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false };
        var documento=new FlowDocument(new Paragraph(new Run(texto))) { FontFamily=new FontFamily("Consolas"),FontSize=12,PagePadding=new Thickness(18),Foreground=Brushes.Black,Background=Brushes.White };
        var raiz=new DockPanel();var imprimir=new Button { Content=impressora is null?"Imprimir…":"Imprimir",Margin=new Thickness(12),IsEnabled=impressora?.Habilitada??true,ToolTip=impressora is null?"Selecionar impressora":impressora.Habilitada?impressora.Caminho:"Impressão do caixa desabilitada em Configurações → Impressora" };DockPanel.SetDock(imprimir,Dock.Bottom);raiz.Children.Add(imprimir);
        raiz.Children.Add(new FlowDocumentScrollViewer { Document=documento,IsToolBarVisible=false });janela.Content=raiz;
        imprimir.Click+=(_,_)=>
        {
            try
            {
                if(impressora is not null){ImpressaoWindows.ImprimirTexto(impressora,texto,titulo);janela.DialogResult=true;return;}
                var dialogo=new PrintDialog();if(dialogo.ShowDialog()!=true)return;
                documento.PageWidth=dialogo.PrintableAreaWidth;documento.PageHeight=dialogo.PrintableAreaHeight;documento.ColumnWidth=Math.Max(1,dialogo.PrintableAreaWidth-36);
                dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator,titulo);janela.DialogResult=true;
            }
            catch { JanelaAviso.Mostrar("Impressão","Não foi possível imprimir. Confira a impressora e tente novamente.",TipoAviso.Erro); }
        };
        return janela.ShowDialog()==true;
    }
}
