using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public partial class PersonalizarItemViewModel : ObservableObject
{
    private readonly LocalizarProdutoViewModel _catalogo;
    public ProdutoLocalizado Produto { get; }
    [ObservableProperty] private string quantidade = "1";
    [ObservableProperty] private string observacao;
    [ObservableProperty] private string erro = "";
    public string RotuloQuantidade => "Quantidade (" + Produto.Produto.Medida + ")";
    public string Total => MedidasVenda.TentarLerQuantidade(Quantidade, out var valor) && MedidasVenda.ValidarQuantidade(valor,Produto.Produto.UnidadeVenda) is null && valor <= 999
        ? CentavosParaMoedaConverter.Formatar(MedidasVenda.CalcularSubtotal(Produto.Produto.PrecoCentavos,valor)) : "—";

    public PersonalizarItemViewModel(LocalizarProdutoViewModel catalogo, ProdutoLocalizado produto)
    {
        _catalogo = catalogo;
        Produto = produto;
        Quantidade = produto.Produto.UnidadeVenda == UnidadeVenda.Quilo ? "0,100" : "1";
        observacao = catalogo.Pedido.ObservacaoNovoItem;
    }

    partial void OnQuantidadeChanged(string value) { Erro = ""; OnPropertyChanged(nameof(Total)); }
    [RelayCommand] private void Aumentar() => Ajustar(1);
    [RelayCommand] private void Diminuir() => Ajustar(-1);
    private void Ajustar(int diferenca)
    {
        if (!MedidasVenda.TentarLerQuantidade(Quantidade, out var valor) || MedidasVenda.ValidarQuantidade(valor,Produto.Produto.UnidadeVenda) is not null || valor > 999)
        { Erro = "Informe uma quantidade válida até 999 " + Produto.Produto.Medida + "."; return; }
        var passo=Produto.Produto.UnidadeVenda==UnidadeVenda.Unidade?1m:0.100m;
        Quantidade = MedidasVenda.FormatarQuantidade(Math.Clamp(valor + diferenca*passo,Produto.Produto.UnidadeVenda==UnidadeVenda.Unidade?1m:0.001m,999m),Produto.Produto.UnidadeVenda);
    }
    public bool Adicionar()
    {
        if (!MedidasVenda.TentarLerQuantidade(Quantidade, out var valor)) { Erro="Informe uma quantidade válida.";return false; }
        if(MedidasVenda.ValidarQuantidade(valor,Produto.Produto.UnidadeVenda) is string erro){Erro=erro;return false;}
        if(valor>999){Erro="Informe uma quantidade até 999 "+Produto.Produto.Medida+".";return false;}
        if (!_catalogo.Lancar(Produto, valor, Observacao)) { Erro = _catalogo.Mensagem; return false; }
        return true;
    }
}
