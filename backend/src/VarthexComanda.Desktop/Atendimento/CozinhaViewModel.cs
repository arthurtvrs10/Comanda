using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public sealed record ItemKds(int ComandaId, int Id, int ProdutoId, string Nome, decimal Quantidade,
    UnidadeVenda Unidade, string? Observacao, StatusPreparo Status, DateTime CriadoEm)
{
    public string Descricao => $"{MedidasVenda.FormatarQuantidade(Quantidade,Unidade)} {Unidade.Simbolo()} · {Nome}";
    public string Estado => Status switch { StatusPreparo.EmPreparo=>"Em preparo",StatusPreparo.Pronto=>"Pronto",StatusPreparo.Entregue=>"Entregue",_=>"Pendente" };
    public StatusPreparo Proximo => Status switch { StatusPreparo.Pendente=>StatusPreparo.EmPreparo,StatusPreparo.EmPreparo=>StatusPreparo.Pronto,_=>StatusPreparo.Entregue };
    public string Acao => Status switch { StatusPreparo.Pendente=>"Iniciar preparo",StatusPreparo.EmPreparo=>"Marcar pronto",_=>"Entregar item" };
    public bool TemObservacao=>!string.IsNullOrWhiteSpace(Observacao);
    public string NomeAcessivel => $"{Descricao}. {Estado}."+(TemObservacao?$" Observação: {Observacao}.":"")+$" {Acao}";
    public AlteracaoPreparo AlterarPara(StatusPreparo destino) => new(Id,Status,Quantidade,destino);
}

public sealed class PedidoKds : ObservableObject
{
    public int Id { get; }
    private string _local="",_referencia="",_cliente="",_tempo="",_nivel="normal",_alerta="";
    private string _estadoPreparo="Pendente",_observacao="",_abertura="";
    private long _totalCentavos;
    public string Abertura { get=>_abertura;private set=>SetProperty(ref _abertura,value); }
    public long TotalCentavos { get=>_totalCentavos;private set { if(SetProperty(ref _totalCentavos,value))OnPropertyChanged(nameof(Valor)); } }
    public string Valor=>CentavosParaMoedaConverter.Formatar(TotalCentavos);
    public bool Entregue=>Estado=="Entregue";
    public string ProximaRaia=>Estado switch {"Pendente"=>"Preparando","Em preparo"=>"Pronto","Pronto"=>"Entregue",_=>""};
    public string DicaAvancar=>Entregue?"Pedido já entregue":$"Mover para {ProximaRaia}";
    public bool PodeIniciar=>Itens.Any(i=>i.Status==StatusPreparo.Pendente);
    private bool _encerrada,_podeMarcarPronto,_podeEntregar;
    private DateTime _desde;
    private PrioridadeComanda _prioridade;
    public string Local { get=>_local; private set=>SetProperty(ref _local,value); }
    public string Referencia { get=>_referencia; private set=>SetProperty(ref _referencia,value); }
    public string Cliente { get=>_cliente; private set=>SetProperty(ref _cliente,value); }
    public string Tempo { get=>_tempo; private set=>SetProperty(ref _tempo,value); }
    public string Nivel { get=>_nivel; private set=>SetProperty(ref _nivel,value); }
    public string Alerta { get=>_alerta;private set=>SetProperty(ref _alerta,value); }
    public string Estado { get=>_estadoPreparo;private set=>SetProperty(ref _estadoPreparo,value); }
    public string Observacao { get=>_observacao;private set { if(SetProperty(ref _observacao,value))OnPropertyChanged(nameof(TemObservacao)); } }
    public bool TemObservacao=>!string.IsNullOrWhiteSpace(Observacao);
    public bool Encerrada { get=>_encerrada; private set=>SetProperty(ref _encerrada,value); }
    public bool PodeMarcarPronto { get=>_podeMarcarPronto; private set=>SetProperty(ref _podeMarcarPronto,value); }
    public bool PodeEntregar { get=>_podeEntregar; private set=>SetProperty(ref _podeEntregar,value); }
    public ObservableCollection<ItemKds> Itens { get; }=[];
    public PedidoKds(int id)=>Id=id;
    public void Atualizar(Comanda comanda,IReadOnlyList<ItemKds> itens,DateTime agora)
    {
        Local=comanda.NumeroMesa is int mesa?$"Mesa {mesa:00}":"Balcão";
        Referencia=$"Comanda {comanda.Numero:00}"+(comanda.CodigoPedido is long numero?$" · Pedido #{numero}":"");
        Cliente=comanda.NomeCliente??"";Encerrada=comanda.Status==StatusComanda.Fechada;
        Observacao=comanda.Observacao??"";
        Estado=itens.All(i=>i.Status==StatusPreparo.Entregue)?"Entregue":itens.All(i=>i.Status==StatusPreparo.Pronto)?"Pronto":itens.All(i=>i.Status==StatusPreparo.Pendente)?"Pendente":"Em preparo";
        Abertura=$"{FusoBrasilia.ParaLocal(comanda.AbertaEm):HH:mm} · Brasília";
        _desde=itens.Min(i=>i.CriadoEm);_prioridade=comanda.Prioridade;
        if(!Itens.SequenceEqual(itens)) { Itens.Clear();foreach(var item in itens)Itens.Add(item); }
        PodeMarcarPronto=itens.Any(i=>i.Status is StatusPreparo.Pendente or StatusPreparo.EmPreparo);
        PodeEntregar=itens.Count>0&&itens.All(i=>i.Status==StatusPreparo.Pronto);
        OnPropertyChanged(nameof(Entregue));OnPropertyChanged(nameof(PodeIniciar));OnPropertyChanged(nameof(ProximaRaia));OnPropertyChanged(nameof(DicaAvancar));
        AtualizarTempo(agora);
    }
    public void DefinirTotal(long total)=>TotalCentavos=total;
    public void AtualizarTempo(DateTime agora)
    {
        var minutos=Math.Max(0,(long)(agora-_desde).TotalMinutes);
        Tempo=$"{minutos} min";
        Nivel=minutos>=20 || _prioridade==PrioridadeComanda.Urgente?"atraso":minutos>=10 || _prioridade==PrioridadeComanda.Alta?"atencao":"normal";
        Alerta=minutos>=20?"Atraso":_prioridade==PrioridadeComanda.Urgente?"Alta prioridade":minutos>=10?"Atenção":_prioridade==PrioridadeComanda.Alta?"Prioridade mediana":"";
    }
}

public sealed partial class CozinhaViewModel : ObservableObject
{
    private readonly Func<IReadOnlyList<ComandaComItens>> _consultar;
    private readonly Func<int,IReadOnlyList<AlteracaoPreparo>,Resultado<ConfirmacaoPreparo>>? _alterar;
    private IReadOnlyList<ComandaComItens> _dados=[];
    private HashSet<int>? _conhecidos;
    private Dictionary<int,bool>? _prontosConhecidos;
    private bool _ocupado,_falha,_somAtivo=true,_filtrando;
    private string _mensagem="",_atualizacao="Aguardando atualização",_local="Todos os locais",_estado="Todos os itens";
    private double _larguraCartao=320;
    public Func<IReadOnlyList<LocalCozinha>>? ConsultarLocais { get; set; }
    public Action? NotificarNovoItem { get; set; }
    public Action? NotificarPedidoPronto { get; set; }
    public Func<DateTime> Agora { get; set; }=()=>DateTime.UtcNow;
    public ObservableCollection<PedidoKds> Pedidos { get; }=[];
    public IReadOnlyList<string> Estados { get; }=["Todos os itens","Pendentes","Em preparo","Prontos"];
    public ObservableCollection<string> Locais { get; }=["Todos os locais"];
    public bool Ocupado { get=>_ocupado; private set { if(SetProperty(ref _ocupado,value))OnPropertyChanged(nameof(PodeAlterar)); } }
    public bool Falha { get=>_falha; private set { if(SetProperty(ref _falha,value)){OnPropertyChanged(nameof(PodeAlterar));OnPropertyChanged(nameof(TituloVazio));OnPropertyChanged(nameof(DescricaoVazio));} } }
    public bool PodeAlterar=>_alterar is not null&&!Ocupado&&!Falha;
    public bool SomAtivo { get=>_somAtivo;set {if(SetProperty(ref _somAtivo,value)){OnPropertyChanged(nameof(SomDescricao));OnPropertyChanged(nameof(SomIcone));}} }
    public string Mensagem { get=>_mensagem;private set=>SetProperty(ref _mensagem,value); }
    public string Atualizacao { get=>_atualizacao;private set=>SetProperty(ref _atualizacao,value); }
    private DateTime? _ultimaAtualizacao;
    public string UltimaAtualizacao=>Falha?"Falha ao atualizar":_ultimaAtualizacao is DateTime ultima?$"Última atualização: {Math.Max(0,(int)(Agora()-ultima).TotalSeconds)}s atrás":"Aguardando atualização";
    public bool AtualizacaoOk=>_ultimaAtualizacao is not null&&!Falha;
    public void AtualizarContagem(){OnPropertyChanged(nameof(UltimaAtualizacao));OnPropertyChanged(nameof(AtualizacaoOk));}
    public bool Vazio=>Pedidos.Count==0;
    public string TituloVazio=>Falha?"Cozinha indisponível":"Nenhum item nesta fila";
    public string DescricaoVazio=>Falha?"Aguardando a próxima tentativa automática de atualização.":"Novos lançamentos aparecem automaticamente.\nConfira os filtros de local e situação.";
    public string Contagem=>$"{Pedidos.Count} comanda{(Pedidos.Count==1?"":"s")} · {Pedidos.Sum(p=>p.Itens.Count)} lançamento{(Pedidos.Sum(p=>p.Itens.Count)==1?"":"s")}";
    public string Local { get=>_local;set { if(value is not null&&SetProperty(ref _local,value))AplicarFiltros(); } }
    public string Estado { get=>_estado;set { if(value is not null&&SetProperty(ref _estado,value))AplicarFiltros(); } }
    public double LarguraCartao { get=>_larguraCartao;set=>SetProperty(ref _larguraCartao,value); }
    public CozinhaViewModel(Func<IReadOnlyList<ComandaComItens>> consultar,Func<int,IReadOnlyList<AlteracaoPreparo>,Resultado<ConfirmacaoPreparo>>? alterar=null)
    { _consultar=consultar;_alterar=alterar; }

    public Func<bool>? Autorizar { get; set; }
    public void Atualizar() { if (Autorizar?.Invoke() == false) { LimparQuadro(); return; } try { Receber(_consultar()); }catch { RegistrarFalha(); } }
    public async Task AtualizarAsync()
    {
        if (Autorizar?.Invoke() == false) { LimparQuadro(); return; }
        if(Ocupado)return;Ocupado=true;
        try { Receber(await Task.Run(_consultar)); }
        catch { RegistrarFalha(); }
        finally { Ocupado=false; }
    }
    private void RegistrarFalha()
    {
        Falha=true;Mensagem="Não foi possível atualizar. Os dados anteriores foram mantidos; uma nova tentativa será feita automaticamente.";
        Atualizacao="Atualização interrompida";
        AtualizarContagem();
    }
    private void Receber(IReadOnlyList<ComandaComItens> dados)
    {
        _dados=dados;
        var ids=dados.SelectMany(c=>c.Itens).Select(i=>i.Id).ToHashSet();
        var novos=_conhecidos is not null&&ids.Except(_conhecidos).Any();
        var prontos=dados.Where(c=>c.Comanda.Status!=StatusComanda.Cancelada).ToDictionary(c=>c.Comanda.Id,c=>
        {
            var ativos=c.Itens.Where(i=>i.StatusPreparo!=StatusPreparo.Entregue).ToArray();
            return ativos.Length>0&&ativos.All(i=>i.StatusPreparo==StatusPreparo.Pronto);
        });
        var ficouPronto=_prontosConhecidos is not null&&prontos.Any(p=>p.Value&&_prontosConhecidos.TryGetValue(p.Key,out var antes)&&!antes);
        _prontosConhecidos=prontos;
        _conhecidos??=[];_conhecidos.UnionWith(ids);
        Falha=false;Mensagem="";AplicarFiltros();Atualizacao=$"Atualizado às {FusoBrasilia.ParaLocal(Agora()):HH:mm:ss} · Brasília · automático a cada 3 s";
        _ultimaAtualizacao=Agora();AtualizarContagem();
        if(novos&&SomAtivo)try{NotificarNovoItem?.Invoke();}catch{ /* A atualização não depende do dispositivo de áudio. */ }
        else if(ficouPronto&&SomAtivo)try{NotificarPedidoPronto?.Invoke();}catch{ /* Áudio não impede a confirmação visual. */ }
    }
    private void AplicarFiltros()
    {
        if(_filtrando)return;_filtrando=true;
        try
        {
        var locais=ConsultarLocais?.Invoke()??Array.Empty<LocalCozinha>();
        var nomes=new[]{"Todos os locais"}.Concat(locais.Select(l=>l.Nome)).ToArray();
        if(!Locais.SequenceEqual(nomes)) { Locais.Clear();foreach(var nome in nomes)Locais.Add(nome);OnPropertyChanged(nameof(Local)); }
        if(!nomes.Contains(_local)){_local="Todos os locais";OnPropertyChanged(nameof(Local));}
        var escolhido=locais.FirstOrDefault(l=>l.Nome==_local);
        var vinculados=locais.SelectMany(l=>l.Produtos).ToHashSet();
        var visiveis=new List<PedidoKds>();
        foreach(var c in _dados.Where(c=>c.Comanda.Status!=StatusComanda.Cancelada))
        {
            var itens=c.Itens.Where(i=>i.StatusPreparo!=StatusPreparo.Entregue)
                .Where(i=>escolhido is null || escolhido.Produtos.Contains(i.ProdutoId)||!vinculados.Contains(i.ProdutoId))
                .Where(i=>_estado switch { "Pendentes"=>i.StatusPreparo==StatusPreparo.Pendente,"Em preparo"=>i.StatusPreparo==StatusPreparo.EmPreparo,"Prontos"=>i.StatusPreparo==StatusPreparo.Pronto,_=>true })
                .Select(i=>new ItemKds(c.Comanda.Id,i.Id,i.ProdutoId,i.NomeProduto,i.Quantidade,i.UnidadeVenda,i.Observacao,i.StatusPreparo,i.CriadoEm)).ToArray();
            if(itens.Length==0)continue;
            foreach(var grupo in itens.GroupBy(i=>i.Status))
            {
            var itensEtapa=grupo.ToArray();
            var pedido=Pedidos.FirstOrDefault(p=>p.Id==c.Comanda.Id&&!visiveis.Contains(p)&&p.Itens.Count>0&&p.Itens[0].Status==grupo.Key)
                ??Pedidos.FirstOrDefault(p=>p.Id==c.Comanda.Id&&!visiveis.Contains(p)&&p.Itens.Any(v=>itensEtapa.Any(i=>i.Id==v.Id)));
            if(pedido is null){pedido=new(c.Comanda.Id);Pedidos.Add(pedido);}
            pedido.Atualizar(c.Comanda,itensEtapa,Agora());visiveis.Add(pedido);
            pedido.DefinirTotal(c.Itens.Where(i=>itensEtapa.Any(v=>v.Id==i.Id)).Sum(i=>i.SubtotalCentavos));
            }
        }
        foreach(var remover in Pedidos.Where(p=>!visiveis.Contains(p)).ToArray())Pedidos.Remove(remover);
        for(var i=0;i<visiveis.Count;i++){var indice=Pedidos.IndexOf(visiveis[i]);if(indice!=i)Pedidos.Move(indice,i);}
        OnPropertyChanged(nameof(Contagem));OnPropertyChanged(nameof(Vazio));
        AtualizarColunas(escolhido,vinculados);
        }
        finally { _filtrando=false; }
    }
    public Task AvancarAsync(ItemKds item)=>item.Status==StatusPreparo.Entregue?Task.CompletedTask:AlterarAsync(item.ComandaId,[item.AlterarPara(item.Proximo)]);
    public Task MarcarProntosAsync(PedidoKds pedido)=>AlterarAsync(pedido.Id,pedido.Itens.Where(i=>i.Status is StatusPreparo.Pendente or StatusPreparo.EmPreparo).Select(i=>i.AlterarPara(StatusPreparo.Pronto)).ToArray());
    public Task EntregarAsync(PedidoKds pedido)=>pedido.PodeEntregar?AlterarAsync(pedido.Id,pedido.Itens.Select(i=>i.AlterarPara(StatusPreparo.Entregue)).ToArray()):Task.CompletedTask;
    private async Task AlterarAsync(int id,IReadOnlyList<AlteracaoPreparo> alteracoes)
    {
        if (Autorizar?.Invoke() == false) return;
        if(!PodeAlterar||alteracoes.Count==0)return;Ocupado=true;
        try
        {
            var resultado=await Task.Run(()=>_alterar!(id,alteracoes));
            if(!resultado.Sucesso){Mensagem=string.Join(" ",resultado.Erros);Falha=true;return;}
            Receber(await Task.Run(_consultar));Mensagem="Preparo atualizado.";
        }
        catch { RegistrarFalha(); }
        finally { Ocupado=false; }
    }
}
