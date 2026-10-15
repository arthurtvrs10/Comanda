using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VarthexComanda.Application.Atendimento;

namespace VarthexComanda.Desktop.Atendimento;

public partial class CozinhaView : UserControl
{
    private readonly Func<IReadOnlyList<ComandaComItens>> _consultar;
    public CozinhaView(Func<IReadOnlyList<ComandaComItens>> consultar)
    {
        InitializeComponent();
        _consultar = consultar;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        timer.Tick += (_, _) => Atualizar();
        Loaded += (_, _) => { Atualizar(); timer.Start(); };
        Unloaded += (_, _) => timer.Stop();
    }
    private void Atualizar_Click(object sender, RoutedEventArgs e) => Atualizar();
    public void Atualizar()
    {
        try
        {
            var comandas = _consultar();
            Lista.ItemsSource = comandas.Select(c => new
            {
                c.Comanda.Numero, c.Comanda.NomeCliente, c.Itens,
                Resumo = $"{c.Comanda.Prioridade} · {ComandaSlotItem.FormatarTempo(c.Comanda.AbertaEm, DateTime.UtcNow)}",
                Vazio = c.Itens.Count == 0 ? "Ainda sem itens." : ""
            }).ToArray();
            Aviso.Text = comandas.Count == 0 ? "Nenhuma comanda aberta no momento." : $"{comandas.Count} comanda(s) aberta(s).";
        }
        catch
        {
            Lista.ItemsSource = null;
            Aviso.Text = "Não foi possível atualizar as comandas. Toque em Atualizar para tentar novamente.";
        }
    }
}
