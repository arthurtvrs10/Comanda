using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public sealed partial class OpcaoPersonalizacao : ObservableObject
{
    public OpcaoComplemento Opcao { get; }
    private readonly Action<OpcaoPersonalizacao> _alterou;
    private readonly Func<bool> _podeSelecionar;
    [ObservableProperty] private int quantidadeSelecionada;
    public bool Selecionado { get=>QuantidadeSelecionada>0;set=>QuantidadeSelecionada=value?Math.Max(1,QuantidadeSelecionada):0; }
    public string Nome=>Opcao.Nome;
    public string Limites=>$"Mínimo: {Opcao.MinimoQuantidade} · Máximo: {(Opcao.MaximoQuantidade is int limite?limite.ToString():Opcao.QuantidadeDisponivel?.ToString()??"conforme disponibilidade")}";
    public string Detalhes=>$"{Nome}\n{Limites}\n{Estoque}";
    public bool Disponivel=>Opcao.Disponivel;
    public string Preco=>Opcao.PrecoCentavos==0?"Grátis":"+ "+CentavosParaMoedaConverter.Formatar(Opcao.PrecoCentavos);
    public string Estoque=>!Disponivel?(Opcao.QuantidadeDisponivel==0?"Esgotado":"Indisponível"):
        Opcao.QuantidadeDisponivel is long q?$"{q:N0} UN disponíveis":"Disponível";
    public OpcaoPersonalizacao(OpcaoComplemento opcao,Action<OpcaoPersonalizacao> alterou,Func<bool> podeSelecionar) { Opcao=opcao;_alterou=alterou;_podeSelecionar=podeSelecionar; }
    private bool PodeAumentar()=>Disponivel&&_podeSelecionar();
    private bool PodeDiminuir()=>Selecionado;
    [RelayCommand(CanExecute=nameof(PodeAumentar))] private void Aumentar()=>QuantidadeSelecionada++;
    [RelayCommand(CanExecute=nameof(PodeDiminuir))] private void Diminuir()=>QuantidadeSelecionada--;
    public void AtualizarControles() { AumentarCommand.NotifyCanExecuteChanged();DiminuirCommand.NotifyCanExecuteChanged(); }
    partial void OnQuantidadeSelecionadaChanged(int value) { OnPropertyChanged(nameof(Selecionado));_alterou(this);AtualizarControles(); }
}

public sealed class GrupoPersonalizacao : ObservableObject
{
    public GrupoComplementos Grupo { get; }
    public string Nome=>Grupo.Nome;
    public string Regras=>Grupo.Opcoes.All(o=>o.ComplementoId is not null)?"Limites definidos no cadastro de cada complemento.":$"{(Grupo.Minimo==0?"Opcional":"Obrigatório")} · escolha de {Grupo.Minimo} a {Grupo.Maximo}";
    public string ResumoSelecao=>$"{Opcoes.Sum(o=>o.QuantidadeSelecionada)} de {Grupo.Maximo} selecionados";
    public ObservableCollection<OpcaoPersonalizacao> Opcoes { get; }=[];
    public GrupoPersonalizacao(GrupoComplementos grupo)=>Grupo=grupo;
    public void NotificarSelecao() { OnPropertyChanged(nameof(ResumoSelecao));foreach(var opcao in Opcoes)opcao.AtualizarControles(); }
}

public partial class PersonalizarItemViewModel : ObservableObject
{
    private readonly LocalizarProdutoViewModel _catalogo;
    private bool _atualizando;
    private string _falhaConsulta="";
    public ProdutoLocalizado Produto { get; private set; }
    public ObservableCollection<GrupoPersonalizacao> GruposComplementos { get; }=[];
    public bool SemComplementos=>GruposComplementos.Count==0;
    [ObservableProperty] private string quantidade="1";
    [ObservableProperty] private string observacao;
    [ObservableProperty] private string erro="";
    public string RotuloQuantidade=>"Quantidade ("+Produto.Produto.Medida+")";
    public bool PodeAdicionar=>ValidacaoAtual() is null;
    public string OrientacaoComplementos=>ValidacaoAtual()??"";
    private Guid[] Escolhas()=>GruposComplementos.SelectMany(g=>g.Opcoes).SelectMany(o=>Enumerable.Repeat(o.Opcao.Id,o.QuantidadeSelecionada)).ToArray();
    public string Total
    {
        get
        {
            if(!MedidasVenda.TentarLerQuantidade(Quantidade,out var q)||MedidasVenda.ValidarQuantidade(q,Produto.Produto.UnidadeVenda) is not null||q>999)return "—";
            try
            {
                var adicional=GruposComplementos.SelectMany(g=>g.Opcoes).Sum(o=>checked(o.Opcao.PrecoCentavos*o.QuantidadeSelecionada));
                return CentavosParaMoedaConverter.Formatar(MedidasVenda.CalcularSubtotal(checked(Produto.Produto.PrecoCentavos+adicional),q));
            }
            catch(OverflowException) { return "—"; }
        }
    }
    public PersonalizarItemViewModel(LocalizarProdutoViewModel catalogo,ProdutoLocalizado produto)
    {
        _catalogo=catalogo;Produto=produto;observacao=catalogo.Pedido.ObservacaoNovoItem;
        Quantidade=produto.Produto.UnidadeVenda==UnidadeVenda.Quilo?"0,100":"1";
        AtualizarComplementos();
    }
    private string? ValidacaoAtual()
    {
        if(_falhaConsulta.Length>0)return _falhaConsulta;
        if(!MedidasVenda.TentarLerQuantidade(Quantidade,out var q))return "Informe uma quantidade válida.";
        if(MedidasVenda.ValidarQuantidade(q,Produto.Produto.UnidadeVenda) is string erroQuantidade)return erroQuantidade;
        if(q>999)return "Informe uma quantidade até 999 "+Produto.Produto.Medida+".";
        if(Observacao.Length>300)return "Use até 300 caracteres na observação.";
        try { ComplementosProduto.VerificarEstoque(Produto.Produto,Escolhas(),q);return null; }
        catch(InvalidOperationException ex) { return ex.Message; }
    }
    private void NotificarEstado()
    {
        foreach(var grupo in GruposComplementos)grupo.NotificarSelecao();
        OnPropertyChanged(nameof(Total));OnPropertyChanged(nameof(PodeAdicionar));OnPropertyChanged(nameof(OrientacaoComplementos));
    }
    partial void OnQuantidadeChanged(string value) { Erro="";NotificarEstado(); }
    partial void OnObservacaoChanged(string value) { Erro="";NotificarEstado(); }
    [RelayCommand]
    private void AtualizarComplementos()
    {
        var escolhidos=Escolhas();_atualizando=true;
        try
        {
            var atual=_catalogo.Pedido.ConsultarProdutoAtual(Produto.Codigo);
            if(atual is null){_falhaConsulta="Este produto não está mais disponível. Atualize o catálogo.";return;}
            _falhaConsulta="";Produto=new(atual,Produto.Categoria);GruposComplementos.Clear();
            foreach(var g in atual.GruposComplementos)
            {
                var grupo=new GrupoPersonalizacao(g);
                foreach(var o in g.Opcoes)
                {
                    var opcao=new OpcaoPersonalizacao(o,selecionada=>
                    {
                        if(_atualizando)return;
                        _atualizando=true;
                        if(selecionada.Selecionado&&g.Maximo==1)
                            foreach(var outra in grupo.Opcoes.Where(x=>x!=selecionada))outra.Selecionado=false;
                        _atualizando=false;Erro="";NotificarEstado();
                    },()=>grupo.Opcoes.Sum(x=>x.QuantidadeSelecionada)<g.Maximo && GruposComplementos.SelectMany(x=>x.Opcoes).Where(x=>o.ComplementoId is not null?x.Opcao.ComplementoId==o.ComplementoId:x.Opcao.Id==o.Id).Sum(x=>x.QuantidadeSelecionada)<(o.MaximoQuantidade??int.MaxValue) && GruposComplementos.SelectMany(x=>x.Opcoes).Sum(x=>x.QuantidadeSelecionada)<10000 &&
                        (o.QuantidadeDisponivel is null || (MedidasVenda.TentarLerQuantidade(Quantidade,out var q)&&q>0&&
                            q*(1+GruposComplementos.SelectMany(x=>x.Opcoes).Where(x=>o.ComplementoId is not null?x.Opcao.ComplementoId==o.ComplementoId:x.Opcao.Id==o.Id).Sum(x=>x.QuantidadeSelecionada))<=o.QuantidadeDisponivel)));
                    grupo.Opcoes.Add(opcao);opcao.QuantidadeSelecionada=o.Disponivel?escolhidos.Count(id=>id==o.Id):0;
                }
                GruposComplementos.Add(grupo);
            }
            OnPropertyChanged(nameof(Produto));OnPropertyChanged(nameof(RotuloQuantidade));OnPropertyChanged(nameof(SemComplementos));
        }
        catch { _falhaConsulta="Não foi possível consultar os complementos e o estoque. Clique em Atualizar estoque."; }
        finally { _atualizando=false;NotificarEstado(); }
    }
    [RelayCommand] private void Aumentar()=>Ajustar(1);
    [RelayCommand] private void Diminuir()=>Ajustar(-1);
    private void Ajustar(int diferenca)
    {
        if(!MedidasVenda.TentarLerQuantidade(Quantidade,out var q)||MedidasVenda.ValidarQuantidade(q,Produto.Produto.UnidadeVenda) is not null||q>999)
        {Erro="Informe uma quantidade válida até 999 "+Produto.Produto.Medida+".";return;}
        var passo=Produto.Produto.UnidadeVenda==UnidadeVenda.Unidade?1m:0.100m;
        Quantidade=MedidasVenda.FormatarQuantidade(Math.Clamp(q+diferenca*passo,Produto.Produto.UnidadeVenda==UnidadeVenda.Unidade?1m:0.001m,999m),Produto.Produto.UnidadeVenda);
    }
    public bool Adicionar()
    {
        var escolhas=Escolhas();AtualizarComplementos();
        if(escolhas.Except(Escolhas()).Any()) { Erro="Os complementos ou o estoque mudaram. Confira a seleção antes de adicionar.";return false; }
        if(ValidacaoAtual() is string falha){Erro=falha;return false;}
        MedidasVenda.TentarLerQuantidade(Quantidade,out var valor);
        if(!_catalogo.Lancar(Produto,valor,Observacao,Escolhas()))
        { var falhaLancamento=_catalogo.Mensagem;AtualizarComplementos();Erro=falhaLancamento;return false; }
        return true;
    }
}
