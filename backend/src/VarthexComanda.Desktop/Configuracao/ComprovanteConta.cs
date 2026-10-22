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
        if(p.ExibirDocumentoCupom&&!string.IsNullOrWhiteSpace(p.Documento))linhas.Add(p.Documento.Trim());
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
    public static void VisualizarExemplo(Window owner,PreferenciasSistema prefs,string nome)
    {
        var agora=DateTime.UtcNow;
        var c=new Comanda { Id=0,Numero=12,CodigoPedido=1042,NumeroMesa=5,Status=StatusComanda.Aberta,AbertaEm=agora,TotalCentavos=8000,Observacao="EXEMPLO — enviar os lanches juntos." };
        var itens=new[]{new ItemComanda { Id=1,ComandaId=0,ProdutoId=1,NomeProduto="Hambúrguer da casa (exemplo)",PrecoUnitarioCentavos=2500,Quantidade=2,SubtotalCentavos=5000,Observacao="Sem cebola",CriadoEm=agora,AtualizadoEm=agora },new ItemComanda { Id=2,ComandaId=0,ProdutoId=2,NomeProduto="Batata frita (exemplo)",PrecoUnitarioCentavos=1800,Quantidade=1,SubtotalCentavos=1800,CriadoEm=agora,AtualizadoEm=agora },new ItemComanda { Id=3,ComandaId=0,ProdutoId=3,NomeProduto="Refrigerante (exemplo)",PrecoUnitarioCentavos=600,Quantidade=2,SubtotalCentavos=1200,CriadoEm=agora,AtualizadoEm=agora }};
        var quantidades=itens.ToDictionary(i=>i.Id,i=>i.Quantidade);
        var modelos=new Dictionary<string,Func<int,FlowDocument>>
        {
            ["Conta do cliente"]=mm=>CupomTermico.Conta(prefs,nome,c,itens,mm),
            ["Pedido da cozinha"]=mm=>CupomTermico.Producao(prefs,nome,c,itens,quantidades,false,mm),
            ["Reimpressão da cozinha"]=mm=>CupomTermico.Producao(prefs,nome,c,itens,quantidades,true,mm),
            ["Fichas de consumação"]=mm=>CupomTermico.Fichas(prefs,nome,c,itens,quantidades,mm),
            ["Teste de impressora"]=mm=>CupomTermico.Texto("COMANDA · POR VARTHEX\nTESTE — DADOS DE EXEMPLO\nTexto pequeno, acentos: pão, café, porção.\n1234567890\nConfira se o conteúdo está dentro do papel.",mm)
        };
        Visualizar(owner,"",titulo:"Modelos de notinhas · Dados de exemplo",modelos:modelos);
    }
    public static bool Visualizar(Window owner,string texto,PreferenciaImpressora? impressora=null,string titulo="Conferência da conta · Não fiscal",Func<int,FlowDocument>? criarDocumento=null,IReadOnlyDictionary<string,Func<int,FlowDocument>>? modelos=null)
    {
        var janela=new Window { Title=titulo,Owner=owner,Width=560,Height=700,MinWidth=420,MinHeight=400,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false };
        var mm=impressora?.LarguraPapelMm??80;
        FlowDocument Criar()=>modelos is not null?modelos.Values.First()(mm):criarDocumento?.Invoke(mm)??CupomTermico.Texto(texto,mm);
        var documento=Criar();
        var raiz=new DockPanel();var imprimir=new Button { Content=impressora is null?"Imprimir…":"Imprimir",Margin=new Thickness(12),IsEnabled=impressora?.Habilitada??true,ToolTip=impressora is null?"Selecionar impressora":impressora.Habilitada?impressora.Caminho:"Impressão do caixa desabilitada em Configurações → Impressora" };DockPanel.SetDock(imprimir,Dock.Bottom);raiz.Children.Add(imprimir);
        var controles=new StackPanel { Orientation=Orientation.Horizontal,Margin=new Thickness(12) };
        controles.Children.Add(new TextBlock { Text="Papel:",VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(0,0,8,0) });
        var papel=new ComboBox { ItemsSource=new[]{58,80},SelectedItem=mm,Width=75,MinHeight=28,FontSize=12 };
        System.Windows.Automation.AutomationProperties.SetName(papel,"Largura do papel em milímetros");controles.Children.Add(papel);
        controles.Children.Add(new TextBlock { Text="mm",VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(6,0,12,0) });
        ComboBox? modelo=null;
        if(modelos is not null){modelo=new ComboBox { ItemsSource=modelos.Keys,SelectedIndex=0,MinWidth=190,MinHeight=28,FontSize=12 };System.Windows.Automation.AutomationProperties.SetName(modelo,"Modelo de notinha");controles.Children.Add(modelo);}
        DockPanel.SetDock(controles,Dock.Top);raiz.Children.Add(controles);
        var visualizador=new FlowDocumentScrollViewer { Document=documento,IsToolBarVisible=false,Width=CupomTermico.Largura(mm)+24,HorizontalAlignment=HorizontalAlignment.Center,Background=Brushes.White };
        void Atualizar(){mm=(int)papel.SelectedItem;documento=modelos is not null&&modelo?.SelectedItem is string chave?modelos[chave](mm):Criar();visualizador.Document=documento;visualizador.Width=CupomTermico.Largura(mm)+24;}
        papel.SelectionChanged+=(_,_)=>Atualizar();if(modelo is not null)modelo.SelectionChanged+=(_,_)=>Atualizar();
        raiz.Children.Add(visualizador);janela.Content=raiz;
        imprimir.Click+=(_,_)=>
        {
            try
            {
                if(impressora is not null){ImpressaoWindows.ImprimirDocumento(impressora,documento,titulo);janela.DialogResult=true;return;}
                var dialogo=new PrintDialog();if(dialogo.ShowDialog()!=true)return;
                ImpressaoWindows.AjustarPagina(documento,dialogo);
                dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator,titulo);janela.DialogResult=true;
            }
            catch { JanelaAviso.Mostrar("Impressão","Não foi possível imprimir. Confira a impressora e tente novamente.",TipoAviso.Erro); }
        };
        return janela.ShowDialog()==true;
    }
}
