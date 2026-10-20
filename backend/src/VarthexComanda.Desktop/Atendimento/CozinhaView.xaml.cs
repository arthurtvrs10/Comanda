using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VarthexComanda.Application.Atendimento;

namespace VarthexComanda.Desktop.Atendimento;

public partial class CozinhaView : UserControl
{
    private readonly Func<IReadOnlyList<ComandaComItens>> _consultar;
    public Func<IReadOnlyList<VarthexComanda.Application.Configuracao.LocalCozinha>>? ConsultarLocais { get; set; }
    private bool _carregandoLocais;
    private void FiltroLocal_SelectionChanged(object sender,SelectionChangedEventArgs e){if(!_carregandoLocais && _consultar is not null)Atualizar();}
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
            var locais=ConsultarLocais?.Invoke()??Array.Empty<VarthexComanda.Application.Configuracao.LocalCozinha>();
            var selecionado=FiltroLocal.SelectedItem as string??"Todos os locais";
            _carregandoLocais=true;
            FiltroLocal.ItemsSource=new[]{"Todos os locais"}.Concat(locais.Select(l=>l.Nome)).ToArray();
            FiltroLocal.SelectedItem=locais.Any(l=>l.Nome==selecionado)?selecionado:"Todos os locais";
            _carregandoLocais=false;
            var local=locais.FirstOrDefault(l=>l.Nome==FiltroLocal.SelectedItem as string);
            if(local is not null)
            {
                var vinculados=locais.SelectMany(l=>l.Produtos).ToHashSet();
                comandas=comandas.Select(c=>new ComandaComItens { Comanda=c.Comanda,Itens=c.Itens.Where(i=>local.Produtos.Contains(i.ProdutoId)||!vinculados.Contains(i.ProdutoId)).ToList() }).Where(c=>c.Itens.Count>0).ToList();
            }
            Lista.ItemsSource = comandas.Select(c => new
            {
                c.Comanda.Numero, c.Comanda.NomeCliente, c.Itens,
                Mesa = c.Comanda.NumeroMesa is int mesa ? $"Mesa {mesa}" : "Balcão",
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
