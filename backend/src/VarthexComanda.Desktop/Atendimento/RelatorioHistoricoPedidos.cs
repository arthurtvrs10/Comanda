using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using VarthexComanda.Application.Atendimento;

namespace VarthexComanda.Desktop.Atendimento;

internal static class RelatorioHistoricoPedidos
{
    internal static string Formatar(object? valor) => valor switch
    {
        null => "", DateTime data => data.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR")),
        decimal numero => numero.ToString("N2", CultureInfo.GetCultureInfo("pt-BR")), _ => Convert.ToString(valor, CultureInfo.GetCultureInfo("pt-BR")) ?? ""
    };

    public static void ExportarExcel(Stream destino, IReadOnlyList<ColunaHistoricoPedido> colunas, IReadOnlyList<PedidoHistoricoResumo> pedidos)
        => ExportarTabelaExcel(destino, colunas.Select(c=>c.Titulo).ToArray(), pedidos.Select(p=>colunas.Select(c=>c.Valor(p)).ToArray()).ToArray(), "Pedidos");

    internal static void ExportarTabelaExcel(Stream destino, IReadOnlyList<string> titulos, IReadOnlyList<object?[]> linhas, string nome)
    {
        using var zip = new ZipArchive(destino, ZipArchiveMode.Create, true);
        XNamespace planilha = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        XNamespace pacote = "http://schemas.openxmlformats.org/package/2006/relationships";
        XNamespace tipos = "http://schemas.openxmlformats.org/package/2006/content-types";
        void Gravar(string nome, XDocument xml)
        { using var arquivo = zip.CreateEntry(nome).Open(); xml.Save(arquivo); }
        Gravar("[Content_Types].xml", new(new XElement(tipos + "Types",
            new XElement(tipos + "Default", new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
            new XElement(tipos + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")),
            new XElement(tipos + "Override", new XAttribute("PartName", "/xl/workbook.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
            new XElement(tipos + "Override", new XAttribute("PartName", "/xl/worksheets/sheet1.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
            new XElement(tipos + "Override", new XAttribute("PartName", "/xl/styles.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")))));
        Gravar("_rels/.rels", new(new XElement(pacote + "Relationships", new XElement(pacote + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", rel.NamespaceName + "/officeDocument"), new XAttribute("Target", "xl/workbook.xml")))));
        Gravar("xl/workbook.xml", new(new XElement(planilha + "workbook", new XAttribute(XNamespace.Xmlns + "r", rel), new XElement(planilha + "sheets", new XElement(planilha + "sheet", new XAttribute("name", nome), new XAttribute("sheetId", 1), new XAttribute(rel + "id", "rId1"))))));
        Gravar("xl/_rels/workbook.xml.rels", new(new XElement(pacote + "Relationships",
            new XElement(pacote + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", rel.NamespaceName + "/worksheet"), new XAttribute("Target", "worksheets/sheet1.xml")),
            new XElement(pacote + "Relationship", new XAttribute("Id", "rId2"), new XAttribute("Type", rel.NamespaceName + "/styles"), new XAttribute("Target", "styles.xml")))));
        Gravar("xl/styles.xml", XDocument.Parse("""
            <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
              <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>
              <borders count="1"><border/></borders>
              <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
              <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="4" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/></cellXfs>
              <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
            </styleSheet>
            """));
        XElement Celula(object? valor) => valor is int or long or decimal
            ? new(planilha + "c", new XAttribute("s", valor is decimal ? 1 : 0), new XElement(planilha + "v", Convert.ToString(valor, CultureInfo.InvariantCulture)))
            : new(planilha + "c", new XAttribute("t", "inlineStr"), new XElement(planilha + "is", new XElement(planilha + "t", new XAttribute(XNamespace.Xml + "space", "preserve"), LimparXml(Formatar(valor)))));
        var dados = new XElement(planilha + "sheetData", new XElement(planilha + "row", titulos.Select(t => Celula(t))));
        foreach (var linha in linhas) dados.Add(new XElement(planilha + "row", linha.Select(Celula)));
        Gravar("xl/worksheets/sheet1.xml", new(new XElement(planilha + "worksheet", dados)));
    }

    private static string LimparXml(string texto) => string.Concat(texto.EnumerateRunes().Where(r => r.Value is 9 or 10 or 13 || r.Value is >= 32 and <= 0xD7FF || r.Value is >= 0xE000 and <= 0xFFFD || r.Value is >= 0x10000 and <= 0x10FFFF).Select(r => r.ToString()));

    public static FlowDocument CriarDocumento(string filtro, IReadOnlyList<ColunaHistoricoPedido> colunas, IReadOnlyList<PedidoHistoricoResumo> pedidos)
    {
        var documento = new FlowDocument { FontFamily = new FontFamily("Segoe UI"), FontSize = 10, Foreground = Brushes.Black, Background = Brushes.White, PagePadding = new Thickness(24), ColumnWidth = double.PositiveInfinity };
        documento.Blocks.Add(new Paragraph(new Run("Histórico de pedidos")) { FontSize = 18, FontWeight = FontWeights.SemiBold });
        documento.Blocks.Add(new Paragraph(new Run(filtro)) { FontSize = 10 });
        var tabela = new Table { CellSpacing = 0 };
        foreach (var coluna in colunas) tabela.Columns.Add(new TableColumn { Width = new GridLength(coluna.Largura, GridUnitType.Star) });
        var linhas = new TableRowGroup(); tabela.RowGroups.Add(linhas);
        TableRow Linha(IEnumerable<string> valores, bool titulo = false)
        {
            var linha = new TableRow();
            foreach (var valor in valores) linha.Cells.Add(new TableCell(new Paragraph(new Run(valor)) { Margin = new Thickness(0) }) { Padding = new Thickness(4), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 0.5), FontWeight = titulo ? FontWeights.SemiBold : FontWeights.Normal, Background = titulo ? Brushes.WhiteSmoke : Brushes.White });
            return linha;
        }
        linhas.Rows.Add(Linha(colunas.Select(c => c.Titulo), true));
        foreach (var pedido in pedidos) linhas.Rows.Add(Linha(colunas.Select(c => Formatar(c.Valor(pedido)))));
        documento.Blocks.Add(tabela);
        documento.Blocks.Add(new Paragraph(new Run($"{pedidos.Count} pedido(s) · Total: {CentavosParaMoedaConverter.Formatar(pedidos.Sum(p => p.TotalCentavos))}")) { FontWeight = FontWeights.SemiBold });
        return documento;
    }
}
