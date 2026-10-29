using System.Collections.ObjectModel;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Atendimento;

public sealed partial class CozinhaViewModel
{
    public PreferenciasQuadroCozinha? PreferenciasQuadro { get; set; }
    private Dictionary<int,long> _ocultos=[];
    public string SomDescricao=>SomAtivo?"Som ativado. Clique para silenciar":"Som desativado. Clique para ativar";
    public string SomIcone=>SomAtivo?"M3 9H7L12 5V19L7 15H3Z M16 8Q21 12 16 16 M19 5Q26 12 19 19":"M3 9H7L12 5V19L7 15H3Z M17 9L23 15 M23 9L17 15";
    public IReadOnlyDictionary<int,long> CapturarEntregues()=>_dados.SelectMany(c=>c.Itens).Where(i=>Colunas[4].Pedidos.Any(p=>p.Itens.Any(v=>v.Id==i.Id))).ToDictionary(i=>i.Id,i=>i.AtualizadoEm.Ticks);
    public void LimparEntregues(IReadOnlyDictionary<int,long> itens)
    {
        if(Autorizar?.Invoke()==false)return;
        var novos=new Dictionary<int,long>(_ocultos);foreach(var par in itens)novos[par.Key]=par.Value;
        PreferenciasQuadro?.SalvarOcultos(novos);_ocultos=novos;AplicarFiltros();
    }
    public ObservableCollection<ColunaCozinha> Colunas { get; }=
    [
        new("chamadas","Chamadas","M6 16V10A6 6 0 0 1 18 10V16L20 18H4Z M10 21H14 M3 7L5 5 M19 5L21 7"),
        new("pendente","Pendente","M12 2A10 10 0 1 0 12 22A10 10 0 1 0 12 2 M12 6V12L16 15"),
        new("preparando","Preparando","M6 11C0 11 1 3 7 5C9 0 15 0 17 5C23 3 24 11 18 11V21H6Z M6 16H18"),
        new("pronto","Pronto","M21 10A9 9 0 1 1 17 4 M7 11L11 15L22 3"),
        new("entregue","Entregue","M3 7L12 2L21 7V18L12 23L3 18Z M3 7L12 12L21 7 M12 12V23 M7 4L17 10")
    ];
    private void LimparQuadro(){Pedidos.Clear();foreach(var c in Colunas){c.Pedidos.Clear();c.Chamadas.Clear();c.AtualizarResumo();}}
    private void AtualizarColunas(LocalCozinha? local,HashSet<int> vinculados)
    {
        var titulos=PreferenciasQuadro?.Titulos();
        if(PreferenciasQuadro is not null)_ocultos=PreferenciasQuadro.EntreguesOcultos();
        foreach(var c in Colunas)
        {
            if(titulos is not null)c.Titulo=titulos.GetValueOrDefault(c.Chave,c.Padrao);
            if(!c.EhEntregues&&!c.EhChamadas)
            {
                var estado=c.Chave switch{"pendente"=>"Pendente","pronto"=>"Pronto",_=>"Em preparo"};
                Sincronizar(c.Pedidos,Pedidos.Where(p=>p.Estado==estado).ToArray());
            }
        }
        var entregues=Colunas[4];var lista=new List<PedidoKds>();
        // Valores refletem somente os itens entregues no dia, mesmo em entregas parciais.
        // O mesmo pedido pode ter um cartão ativo e seu comprovante parcial em Entregue.
        var hoje=FusoBrasilia.ParaLocal(Agora()).Date;
        if(_estado=="Todos os itens")foreach(var dados in _dados.Where(c=>c.Comanda.Status!=StatusComanda.Cancelada))
        {
            var itens=dados.Itens.Where(i=>i.StatusPreparo==StatusPreparo.Entregue&&FusoBrasilia.ParaLocal(i.AtualizadoEm).Date==hoje)
                .Where(i=>!_ocultos.TryGetValue(i.Id,out var oculto)||oculto!=i.AtualizadoEm.Ticks)
                .Where(i=>local is null||local.Produtos.Contains(i.ProdutoId)||!vinculados.Contains(i.ProdutoId)).ToArray();
            if(itens.Length==0)continue;
            var cartao=entregues.Pedidos.FirstOrDefault(p=>p.Id==dados.Comanda.Id)??new PedidoKds(dados.Comanda.Id);
            cartao.Atualizar(dados.Comanda,itens.Select(i=>new ItemKds(dados.Comanda.Id,i.Id,i.ProdutoId,i.NomeProduto,i.Quantidade,i.UnidadeVenda,i.Observacao,i.StatusPreparo,i.CriadoEm)).ToArray(),Agora());
            cartao.DefinirTotal(itens.Sum(i=>i.SubtotalCentavos));lista.Add(cartao);
        }
        Sincronizar(entregues.Pedidos,lista);
        if(PreferenciasQuadro is not null){var chamadas=PreferenciasQuadro.Chamadas();Colunas[0].Chamadas.Clear();foreach(var chamada in chamadas.OrderBy(c=>c.CriadaEm))Colunas[0].Chamadas.Add(chamada);}
        foreach(var c in Colunas)c.AtualizarResumo();
    }
    private static void Sincronizar(ObservableCollection<PedidoKds> destino,IReadOnlyList<PedidoKds> novos)
    {
        foreach(var antigo in destino.Where(p=>!novos.Contains(p)).ToArray())destino.Remove(antigo);
        for(var i=0;i<novos.Count;i++){var indice=destino.IndexOf(novos[i]);if(indice<0)destino.Insert(i,novos[i]);else if(indice!=i)destino.Move(indice,i);}
    }
    public void Renomear(ColunaCozinha coluna,string titulo)
    {
        if(Autorizar?.Invoke()==false)return;
        titulo=titulo.Trim();if(titulo.Length is <1 or >24||titulo.Any(char.IsControl))throw new ArgumentException("Use um título de 1 a 24 caracteres.");
        var titulos=Colunas.ToDictionary(c=>c.Chave,c=>c==coluna?titulo:c.Titulo);
        PreferenciasQuadro?.SalvarTitulos(titulos);coluna.Titulo=titulo;
    }
    public void RegistrarChamada(int mesa,string solicitacao)
    {
        if(Autorizar?.Invoke()==false)return;
        solicitacao=solicitacao.Trim();
        if(mesa is <1 or >9999||solicitacao.Length>120||solicitacao.Any(char.IsControl))throw new ArgumentException("Informe a mesa de 1 a 9999 e uma solicitação de até 120 caracteres.");
        var chamadas=Colunas[0].Chamadas;
        if(chamadas.Any(c=>c.Mesa==mesa))throw new InvalidOperationException("Esta mesa já tem uma chamada pendente.");
        if(chamadas.Count>=300)throw new InvalidOperationException("Atenda as chamadas pendentes antes de registrar novas.");
        var nova=new ChamadaMesa(Guid.NewGuid(),mesa,string.IsNullOrWhiteSpace(solicitacao)?"Solicita atendimento":solicitacao,Agora());
        PreferenciasQuadro?.SalvarChamadas(chamadas.Append(nova));chamadas.Add(nova);Colunas[0].AtualizarResumo();
        if(SomAtivo)try{NotificarNovoItem?.Invoke();}catch{}
    }
    public void AtenderChamada(ChamadaMesa chamada)
    {
        if(Autorizar?.Invoke()==false)return;
        PreferenciasQuadro?.SalvarChamadas(Colunas[0].Chamadas.Where(c=>c.Id!=chamada.Id));Colunas[0].Chamadas.Remove(chamada);Colunas[0].AtualizarResumo();
    }
    public Task IniciarAsync(PedidoKds pedido)=>AlterarAsync(pedido.Id,pedido.Itens.Where(i=>i.Status==StatusPreparo.Pendente).Select(i=>i.AlterarPara(StatusPreparo.EmPreparo)).ToArray());
    public bool PodeMover(PedidoKds pedido,ColunaCozinha destino)
    {
        if(!PodeAlterar||Autorizar?.Invoke()==false||!Pedidos.Contains(pedido))return false;
        return destino.Chave switch
        {
            "preparando"=>pedido.Estado=="Pendente",
            "pronto"=>pedido.PodeMarcarPronto,
            "entregue"=>pedido.PodeEntregar,
            _=>false
        };
    }
    public Task MoverAsync(PedidoKds pedido,ColunaCozinha destino)
    {
        if(!PodeMover(pedido,destino))return Task.CompletedTask;
        return destino.Chave switch {"preparando"=>IniciarAsync(pedido),"pronto"=>MarcarProntosAsync(pedido),"entregue"=>EntregarAsync(pedido),_=>Task.CompletedTask};
    }
}
