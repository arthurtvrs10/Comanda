using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VarthexComanda.Desktop.Atendimento;

public partial class PersonalizarItemViewModel : ObservableObject
{
    private readonly LocalizarProdutoViewModel _catalogo;
    public ProdutoLocalizado Produto { get; }
    [ObservableProperty] private string quantidade = "1";
    [ObservableProperty] private string observacao;
    [ObservableProperty] private string erro = "";
    public string Total => int.TryParse(Quantidade, out var valor) && valor is >= 1 and <= 999
        ? CentavosParaMoedaConverter.Formatar(checked(Produto.Produto.PrecoCentavos * valor)) : "—";

    public PersonalizarItemViewModel(LocalizarProdutoViewModel catalogo, ProdutoLocalizado produto)
    {
        _catalogo = catalogo;
        Produto = produto;
        observacao = catalogo.Pedido.ObservacaoNovoItem;
    }

    partial void OnQuantidadeChanged(string value) { Erro = ""; OnPropertyChanged(nameof(Total)); }
    [RelayCommand] private void Aumentar() => Ajustar(1);
    [RelayCommand] private void Diminuir() => Ajustar(-1);
    private void Ajustar(int diferenca)
    {
        if (!int.TryParse(Quantidade, out var valor) || valor is < 1 or > 999)
        { Erro = "Informe uma quantidade entre 1 e 999 unidades."; return; }
        Quantidade = Math.Clamp(valor + diferenca, 1, 999).ToString();
    }
    public bool Adicionar()
    {
        if (!int.TryParse(Quantidade, out var valor) || valor is < 1 or > 999)
        { Erro = "Informe uma quantidade entre 1 e 999 unidades."; return false; }
        if (!_catalogo.Lancar(Produto, valor, Observacao)) { Erro = _catalogo.Mensagem; return false; }
        return true;
    }
}
