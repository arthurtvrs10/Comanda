using System.Windows;
using System.Windows.Controls;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Atendimento;

public sealed class JanelaColunasPedidos : Window
{
    private readonly Dictionary<string, CheckBox> _colunas = [];
    private readonly Dictionary<string, CheckBox> _grupos = [];
    public string[] ColunasSelecionadas => _colunas.Where(p => p.Value.IsChecked == true).Select(p => p.Key).ToArray();
    public string[] GruposSelecionados => _grupos.Where(p => p.Value.IsChecked == true).Select(p => p.Key).ToArray();

    public JanelaColunasPedidos(HistoricoPedidosViewModel modelo)
    {
        Title = "Exibir / ocultar colunas"; Width = 350; Height = 590; MinWidth = 320; MinHeight = 400;
        UiHistorico.ConfigurarJanela(this);
        var raiz = new DockPanel { Margin = new Thickness(12) }; Content = raiz;
        var rodape = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
        var voltar = UiHistorico.Botao("Voltar"); voltar.IsCancel = true; rodape.Children.Add(voltar);
        var aplicar = UiHistorico.Botao("Aplicar"); aplicar.IsDefault = true; aplicar.Click += (_, _) => DialogResult = true; rodape.Children.Add(aplicar);
        DockPanel.SetDock(rodape, Dock.Bottom); raiz.Children.Add(rodape);
        var agrupamento = new Border { BorderThickness = new Thickness(1), Margin = new Thickness(0, 10, 0, 0), Padding = new Thickness(8) };
        agrupamento.SetResourceReference(Border.BorderBrushProperty, "AjudaLine");
        var painelGrupos = new StackPanel(); painelGrupos.Children.Add(UiHistorico.Titulo("Agrupar por"));
        var opcoes = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2 };
        foreach (var (chave, nome) in new[] { ("CriadoPor", "Criado por"), ("Origem", "Origem"), ("Tipo", "Tipo"), ("Pagamento", "Pagamento") })
        {
            var caixa = new CheckBox { Content = nome, FontSize = 12, MinHeight = 28, IsChecked = modelo.Preferencias.AgruparPor.Contains(chave) };
            _grupos.Add(chave, caixa); opcoes.Children.Add(caixa);
        }
        painelGrupos.Children.Add(opcoes); agrupamento.Child = painelGrupos; DockPanel.SetDock(agrupamento, Dock.Bottom); raiz.Children.Add(agrupamento);
        var arvore = new TreeView { BorderThickness = new Thickness(1), Padding = new Thickness(4) };
        arvore.SetResourceReference(Control.BackgroundProperty, "AjudaSurface"); arvore.SetResourceReference(Control.ForegroundProperty, "AjudaInk");
        arvore.SetResourceReference(Control.BorderBrushProperty, "AjudaLine");
        foreach (var grupo in new[] { "Dados do cliente", "Totais do pedido", "Pagamento", "Outras informações" })
        {
            var item = new TreeViewItem { Header = grupo, IsExpanded = true, FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 3, 0, 3) };
            foreach (var coluna in modelo.Colunas.Where(c => c.Grupo == grupo))
            {
                var caixa = new CheckBox { Content = coluna.Titulo, IsChecked = coluna.Visivel, IsEnabled = coluna.Chave != "Codigo", FontWeight = FontWeights.Normal, FontSize = 12, MinHeight = 26 };
                if (coluna.Chave == "Codigo") caixa.ToolTip = "O código permanece visível para identificar o pedido.";
                _colunas.Add(coluna.Chave, caixa); item.Items.Add(caixa);
            }
            arvore.Items.Add(item);
        }
        raiz.Children.Add(arvore);
    }
}
