using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using VarthexComanda.Application.Atendimento;

namespace VarthexComanda.Desktop.Atendimento;

public partial class HistoricoView : UserControl
{
    public HistoricoViewModel ViewModel { get; }
    private sealed record ColunaVenda(string Titulo,string Caminho,double Largura,bool Numerica,Func<VendaResumo,object?> Valor);
    private readonly ColunaVenda[] _colunas =
    [
        new("Identificador","Venda.ComandaId",100,true,v=>v.Venda.ComandaId),
        new("Comanda","NumeroComanda",85,true,v=>v.NumeroComanda),
        new("Cliente","NomeCliente",180,false,v=>v.NomeCliente),
        new("Total","Venda.TotalCentavos",115,true,v=>v.Venda.TotalCentavos/100m),
        new("Pagamento","FormaPagamentoDescricao",130,false,v=>v.FormaPagamentoDescricao),
        new("Horário","Venda.FinalizadaEm",115,false,v=>FusoBrasilia.ParaLocal(v.Venda.FinalizadaEm))
    ];
    private readonly Button _exportar,_imprimir,_detalhes;

    public HistoricoView(HistoricoViewModel viewModel)
    {
        InitializeComponent(); ViewModel=viewModel; DataContext=viewModel;
        Button Acao(string texto,string? desenho,Action executar)
        {
            var b=UiHistorico.Botao(texto,desenho);b.Margin=new Thickness(0,0,6,5);
            b.Click+=(_,_)=>executar();PainelAcoes.Children.Add(b);return b;
        }
        Acao("Atualizar",UiHistorico.Atualizar,viewModel.AtualizarVendas);
        Acao("Mais colunas",UiHistorico.Colunas,EscolherColunas);
        _exportar=Acao("Exportar Excel",UiHistorico.Excel,Exportar);
        _imprimir=Acao("Imprimir",UiHistorico.Imprimir,Imprimir);
        _detalhes=Acao("Detalhes",null,AbrirDetalhes);
        foreach(var c in _colunas)
        {
            var binding=new Binding(c.Caminho) { TargetNullValue="" };
            if(c.Titulo=="Total")binding.Converter=new CentavosParaMoedaConverter();
            if(c.Titulo=="Horário")binding.Converter=new UtcParaHorarioLocalConverter();
            var coluna=new DataGridTextColumn { Header=c.Titulo,Binding=binding,SortMemberPath=c.Caminho,Width=new DataGridLength(c.Largura,DataGridLengthUnitType.Star),MinWidth=c.Titulo=="Cliente"?110:75,
                ElementStyle=(Style)FindResource(c.Numerica?"NumeroTabela":"TextoTabela") };
            TabelaVendas.Columns.Add(coluna);
        }
        viewModel.PropertyChanged+=(_,_)=>AtualizarAcoes();AtualizarAcoes();
    }
    private void AtualizarAcoes()
    {
        _exportar.IsEnabled=_imprimir.IsEnabled=ViewModel.QuantidadeExibida>0;
        _detalhes.IsEnabled=ViewModel.VendaSelecionada is not null;
        ResumoDia.Text=$"Total do dia: {CentavosParaMoedaConverter.Formatar(ViewModel.TotalDiaCentavos)} · Ticket médio: {CentavosParaMoedaConverter.Formatar(ViewModel.TicketMedioCentavos)}";
        ResumoDia.ToolTip="Indicadores de todas as vendas do dia. A busca por comanda filtra somente a tabela.";
    }
    private void EscolherColunas()
    {
        var menu=new ContextMenu();
        foreach(var c in TabelaVendas.Columns)
        {
            var item=new MenuItem { Header=c.Header,IsCheckable=true,IsChecked=c.Visibility==Visibility.Visible,StaysOpenOnClick=true };
            item.Click+=(_,_)=>
            {
                if(!item.IsChecked && TabelaVendas.Columns.Count(x=>x.Visibility==Visibility.Visible)==1){item.IsChecked=true;return;}
                c.Visibility=item.IsChecked?Visibility.Visible:Visibility.Collapsed;
            };
            menu.Items.Add(item);
        }
        menu.PlacementTarget=PainelAcoes;menu.Placement=PlacementMode.Bottom;menu.IsOpen=true;
    }
    private void AutoAjustar_Changed(object sender,RoutedEventArgs e)
    {
        if(TabelaVendas is null)return;
        for(var i=0;i<TabelaVendas.Columns.Count;i++)TabelaVendas.Columns[i].Width=new DataGridLength(_colunas[i].Largura,
            AutoAjustarColunas.IsChecked==true?DataGridLengthUnitType.Star:DataGridLengthUnitType.Pixel);
    }
    private (ColunaVenda[] Colunas,VendaResumo[] Vendas) DadosVisiveis() =>
        (TabelaVendas.Columns.Where(c=>c.Visibility==Visibility.Visible).OrderBy(c=>c.DisplayIndex).Select(c=>_colunas[TabelaVendas.Columns.IndexOf(c)]).ToArray(),TabelaVendas.Items.OfType<VendaResumo>().ToArray());

    public void ExportarPara(Stream destino)
    {
        var (colunas,vendas)=DadosVisiveis();
        RelatorioHistoricoPedidos.ExportarTabelaExcel(destino,colunas.Select(c=>c.Titulo).ToArray(),vendas.Select(v=>colunas.Select(c=>c.Valor(v)).ToArray()).ToArray(),"Vendas");
    }
    private void Exportar()
    {
        if(ViewModel.QuantidadeExibida==0)return;
        var dialogo=new SaveFileDialog { Title="Exportar histórico de vendas",FileName=$"Vendas-{ViewModel.DataSelecionada:yyyy-MM-dd}.xlsx",Filter="Planilha Excel (*.xlsx)|*.xlsx",DefaultExt=".xlsx",OverwritePrompt=true };
        if(dialogo.ShowDialog(Window.GetWindow(this))!=true)return;
        var temporario=dialogo.FileName+"."+Guid.NewGuid().ToString("N")+".tmp";
        try { using(var arquivo=File.Create(temporario))ExportarPara(arquivo);File.Move(temporario,dialogo.FileName,true);ViewModel.Mensagem="Planilha exportada com sucesso."; }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or ArgumentException or System.Xml.XmlException)
        { ViewModel.Mensagem="Não foi possível exportar. Confira a pasta e feche a planilha se estiver em uso."; }
        finally { try { if(File.Exists(temporario))File.Delete(temporario); } catch(IOException){} catch(UnauthorizedAccessException){} }
    }
    public FlowDocument CriarDocumento()
    {
        var (colunas,vendas)=DadosVisiveis();
        var documento=new FlowDocument { FontFamily=new FontFamily("Segoe UI"),FontSize=10,Foreground=Brushes.Black,Background=Brushes.White,PagePadding=new Thickness(24),ColumnWidth=double.PositiveInfinity };
        documento.Blocks.Add(new Paragraph(new Run($"Histórico de vendas · {ViewModel.DataSelecionada:dd/MM/yyyy}")) { FontSize=18,FontWeight=FontWeights.SemiBold });
        if(!string.IsNullOrWhiteSpace(ViewModel.TextoBuscaNumero))documento.Blocks.Add(new Paragraph(new Run("Filtro de comanda: "+ViewModel.TextoBuscaNumero.Trim())));
        var tabela=new Table { CellSpacing=0 };foreach(var c in colunas)tabela.Columns.Add(new TableColumn { Width=new GridLength(c.Largura,GridUnitType.Star) });
        var grupo=new TableRowGroup();tabela.RowGroups.Add(grupo);
        void Linha(IEnumerable<string> valores,bool titulo=false)
        {
            var linha=new TableRow();foreach(var texto in valores)linha.Cells.Add(new TableCell(new Paragraph(new Run(texto))) { Padding=new Thickness(4),FontWeight=titulo?FontWeights.Bold:FontWeights.Normal });grupo.Rows.Add(linha);
        }
        Linha(colunas.Select(c=>c.Titulo),true);foreach(var venda in vendas)Linha(colunas.Select(c=>RelatorioHistoricoPedidos.Formatar(c.Valor(venda))));
        documento.Blocks.Add(tabela);
        documento.Blocks.Add(new Paragraph(new Run($"{vendas.Length} venda(s) exibida(s) · Total exibido: {CentavosParaMoedaConverter.Formatar(vendas.Sum(v=>v.Venda.TotalCentavos))}")));
        documento.Blocks.Add(new Paragraph(new Run(ResumoDia.Text)));return documento;
    }
    private void Imprimir()
    {
        if(ViewModel.QuantidadeExibida==0)return;
        var documento=CriarDocumento();documento.PageWidth=1122;documento.PageHeight=793;
        var janela=new Window { Title="Imprimir histórico de vendas",Width=1000,Height=680,MinWidth=600,MinHeight=400,Owner=Window.GetWindow(this) };UiHistorico.ConfigurarJanela(janela);
        var raiz=new DockPanel();var botao=UiHistorico.Botao("Selecionar impressora e imprimir",UiHistorico.Imprimir);botao.Margin=new Thickness(10);botao.HorizontalAlignment=HorizontalAlignment.Right;
        DockPanel.SetDock(botao,Dock.Bottom);raiz.Children.Add(botao);raiz.Children.Add(new FlowDocumentPageViewer { Document=documento });janela.Content=raiz;
        botao.Click+=(_,_)=>
        {
            try
            {
                var dialogo=new PrintDialog();dialogo.PrintTicket.PageOrientation=System.Printing.PageOrientation.Landscape;if(dialogo.ShowDialog()!=true)return;
                documento.PageWidth=dialogo.PrintableAreaWidth;documento.PageHeight=dialogo.PrintableAreaHeight;documento.ColumnWidth=Math.Max(1,dialogo.PrintableAreaWidth-48);
                dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator,"Histórico de vendas");janela.Close();
            }
            catch(Exception ex) when(ex is System.Printing.PrintSystemException or InvalidOperationException or IOException)
            { JanelaAviso.Mostrar("Impressão","Não foi possível imprimir. Confira a impressora e tente novamente.",TipoAviso.Erro); }
        };
        janela.ShowDialog();
    }
    public Window? CriarDetalhesVenda()
    {
        var venda=ViewModel.VendaSelecionada;if(venda is null)return null;
        var janela=new Window { Title=$"Venda · Comanda {venda.NumeroComanda:00}",Width=850,Height=480,MinWidth=620,MinHeight=350,Owner=Window.GetWindow(this) };UiHistorico.ConfigurarJanela(janela);
        janela.Resources.MergedDictionaries.Add(new ResourceDictionary { Source=new Uri("/VarthexComanda;component/Atendimento/EstilosHistorico.xaml",UriKind.Relative) });
        var raiz=new DockPanel { Margin=new Thickness(12) };janela.Content=raiz;
        var fechar=UiHistorico.Botao("Fechar");fechar.IsCancel=true;fechar.HorizontalAlignment=HorizontalAlignment.Right;fechar.Margin=new Thickness(0,10,0,0);fechar.Click+=(_,_)=>janela.Close();DockPanel.SetDock(fechar,Dock.Bottom);raiz.Children.Add(fechar);
        var titulo=new TextBlock { Text=$"Comanda {venda.NumeroComanda:00} · {venda.NomeCliente}\n{venda.FormaPagamentoDescricao} · {FusoBrasilia.ParaLocal(venda.Venda.FinalizadaEm):dd/MM/yyyy HH:mm} · Total: {CentavosParaMoedaConverter.Formatar(venda.Venda.TotalCentavos)}",FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12) };DockPanel.SetDock(titulo,Dock.Top);raiz.Children.Add(titulo);
        var itens=new DataGrid { ItemsSource=ViewModel.ItensDaVendaSelecionada,AutoGenerateColumns=false,IsReadOnly=true,CanUserAddRows=false,RowHeight=34,HeadersVisibility=DataGridHeadersVisibility.Column };
        foreach(var (nome,caminho,numerico) in new[]{("Produto","NomeProduto",false),("Observação","Observacao",false),("Qtd","QuantidadeFormatada",true),("Medida","Medida",false),("Preço unit.","PrecoUnitarioCentavos",true),("Subtotal","SubtotalCentavos",true)})
        {
            var binding=new Binding(caminho);if(nome is "Preço unit." or "Subtotal")binding.Converter=new CentavosParaMoedaConverter();
            itens.Columns.Add(new DataGridTextColumn { Header=nome,Binding=binding,MinWidth=65,Width=new DataGridLength(nome is "Produto" or "Observação"?2:1,DataGridLengthUnitType.Star),ElementStyle=(Style)FindResource(numerico?"NumeroTabela":"TextoTabela") });
        }
        itens.SetResourceReference(Control.BackgroundProperty,"Surface");itens.SetResourceReference(Control.ForegroundProperty,"Ink");itens.SetResourceReference(DataGrid.RowBackgroundProperty,"Surface");itens.SetResourceReference(DataGrid.AlternatingRowBackgroundProperty,"Canvas");
        raiz.Children.Add(itens);return janela;
    }
    private void AbrirDetalhes()=>CriarDetalhesVenda()?.ShowDialog();
    private void Tabela_DoubleClick(object sender,MouseButtonEventArgs e)
    { if(ItemsControl.ContainerFromElement(TabelaVendas,e.OriginalSource as DependencyObject) is DataGridRow)AbrirDetalhes(); }
    private void Tabela_KeyDown(object sender,KeyEventArgs e)
    { if(e.Key==Key.Enter && ViewModel.VendaSelecionada is not null){AbrirDetalhes();e.Handled=true;} }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if(!e.Handled && e.Key==Key.F && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
        { CampoBuscaNumero.Focus();CampoBuscaNumero.SelectAll();e.Handled=true; }
    }
}
