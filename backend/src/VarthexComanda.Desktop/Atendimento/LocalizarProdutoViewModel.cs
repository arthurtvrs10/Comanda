using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public sealed record ProdutoLocalizado(Produto Produto, string Categoria)
{
    public int Codigo => Produto.Id;
    public string Nome => Produto.Nome;
    public string? Foto => Produto.FotoArquivo;
    public string Preco => CentavosParaMoedaConverter.Formatar(Produto.PrecoCentavos);
    public string PrecoVenda => (Produto.PrecoCentavos / 100m).ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) + (Produto.UnidadeVenda == UnidadeVenda.Unidade ? "" : " / " + Produto.Medida);
}

public partial class LocalizarProdutoViewModel : ObservableObject, IDisposable
{
    public AtendimentoViewModel Pedido { get; }
    public ObservableCollection<ProdutoLocalizado> Produtos { get; } = new();
    [ObservableProperty] private ProdutoLocalizado? selecionado;
    [ObservableProperty] private Categoria? categoria;
    [ObservableProperty] private bool fotos = true;
    [ObservableProperty] private string mensagem = "";
    public bool Lista { get => !Fotos; set { if (value) Fotos = false; } }
    public string TotalEncontrados => $"{Produtos.Count} produto(s) encontrado(s)";
    public bool SemProdutos => Produtos.Count == 0;
    public LocalizarProdutoViewModel(AtendimentoViewModel pedido)
    {
        Pedido = pedido;
        categoria = pedido.CategoriaCatalogo;
        pedido.ProdutosCatalogo.CollectionChanged += ProdutosAlterados;
        RecarregarLista();
    }
    partial void OnFotosChanged(bool value) => OnPropertyChanged(nameof(Lista));
    partial void OnCategoriaChanged(Categoria? value) => Pedido.CategoriaCatalogo = value;
    private void ProdutosAlterados(object? sender, NotifyCollectionChangedEventArgs e) => RecarregarLista();
    private void RecarregarLista()
    {
        var id = Selecionado?.Codigo;
        Produtos.Clear();
        foreach (var produto in Pedido.ProdutosCatalogo)
            Produtos.Add(new(produto, Pedido.Categorias.FirstOrDefault(c => c.Id == produto.CategoriaId)?.Nome ?? "Sem categoria"));
        Selecionado = Produtos.FirstOrDefault(p => p.Codigo == id) ?? Produtos.FirstOrDefault();
        OnPropertyChanged(nameof(TotalEncontrados)); OnPropertyChanged(nameof(SemProdutos));
    }
    [RelayCommand] private void Todas() { Categoria = null; Pedido.CategoriaCatalogo = null; }
    [RelayCommand] private void Atualizar()
    {
        try { Pedido.AtualizarCategorias(); Categoria = Pedido.CategoriaCatalogo; RecarregarLista(); Mensagem = "Produtos atualizados."; }
        catch { Mensagem = "Não foi possível atualizar. Tente novamente."; }
    }
    [RelayCommand] private void Adicionar(ProdutoLocalizado? item) => Lancar(item ?? Selecionado, 1, Pedido.ObservacaoNovoItem);
    public bool Lancar(ProdutoLocalizado? item, decimal quantidade, string? observacao)
    {
        if (item is null) { Mensagem = "Selecione um produto."; return false; }
        Selecionado = item;
        var sucesso = Pedido.AdicionarProduto(item.Produto, quantidade, observacao);
        Mensagem = sucesso ? $"{quantidade} × {item.Nome} adicionado(s) ao pedido." : Pedido.Mensagem;
        return sucesso;
    }
    public void Dispose() => Pedido.ProdutosCatalogo.CollectionChanged -= ProdutosAlterados;
}
