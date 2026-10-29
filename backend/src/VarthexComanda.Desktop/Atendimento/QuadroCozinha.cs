using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Atendimento;

public sealed record ChamadaMesa(Guid Id,int Mesa,string Solicitacao,DateTime CriadaEm)
{
    public string Local=>$"Mesa {Mesa:00}";
    public string Horario=>$"{Application.Atendimento.FusoBrasilia.ParaLocal(CriadaEm):HH:mm} · Brasília";
}

public sealed class ColunaCozinha(string chave,string titulo,string icone) : ObservableObject
{
    public string Chave { get; }=chave;
    public string Padrao { get; }=titulo;
    public string Icone { get; }=icone;
    private string _titulo=titulo;
    private double _largura=chave=="chamadas"?220:320;
    public double Largura { get=>_largura;internal set=>SetProperty(ref _largura,value); }
    public string Titulo { get=>_titulo;internal set=>SetProperty(ref _titulo,value); }
    public bool EhChamadas=>Chave=="chamadas";
    public bool EhEntregues=>Chave=="entregue";
    public ObservableCollection<PedidoKds> Pedidos { get; }=[];
    public ObservableCollection<ChamadaMesa> Chamadas { get; }=[];
    public int Quantidade=>EhChamadas?Chamadas.Count:Pedidos.Count;
    public bool Vazia=>Quantidade==0;
    public string TextoVazio=>EhChamadas?"Nenhuma chamada":"Nenhum pedido";
    public string ValorTotal=>CentavosParaMoedaConverter.Formatar(Pedidos.Sum(p=>p.TotalCentavos));
    public void AtualizarResumo(){OnPropertyChanged(nameof(Quantidade));OnPropertyChanged(nameof(Vazia));OnPropertyChanged(nameof(ValorTotal));}
}

/// <summary>Preferências do quadro e solicitações registradas no computador; independente do preparo e pagamento.</summary>
public sealed class PreferenciasQuadroCozinha(IConfiguracaoRepository repositorio)
{
    public const string ChaveTitulos="cozinha.kanban.titulos.v1";
    public const string ChaveChamadas="cozinha.kanban.chamadas.v1";
    public Dictionary<int,long> EntreguesOcultos()=>Ler<Dictionary<int,long>>("cozinha.kanban.ocultos.v1")??[];
    public void SalvarOcultos(Dictionary<int,long> itens)=>repositorio.Definir("cozinha.kanban.ocultos.v1",JsonSerializer.Serialize(itens),DateTime.UtcNow);
    public Dictionary<string,string> Titulos()=>Ler<Dictionary<string,string>>(ChaveTitulos)??[];
    public IReadOnlyList<ChamadaMesa> Chamadas()=>Ler<List<ChamadaMesa>>(ChaveChamadas)??[];
    private T? Ler<T>(string chave){var json=repositorio.ObterValor(chave);return json is null?default:JsonSerializer.Deserialize<T>(json);}
    public void SalvarTitulos(IReadOnlyDictionary<string,string> titulos)=>repositorio.Definir(ChaveTitulos,JsonSerializer.Serialize(titulos),DateTime.UtcNow);
    public void SalvarChamadas(IEnumerable<ChamadaMesa> chamadas)=>repositorio.Definir(ChaveChamadas,JsonSerializer.Serialize(chamadas),DateTime.UtcNow);
}
