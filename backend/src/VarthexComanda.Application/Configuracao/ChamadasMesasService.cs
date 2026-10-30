using System.Text.Json;

namespace VarthexComanda.Application.Configuracao;

public sealed record ChamadaMesa(Guid Id,int Mesa,string Solicitacao,DateTime CriadaEm);
public sealed record ResultadoChamada(ChamadaMesa? Chamada,bool JaAtendida);

/// <summary>Fila persistida e compartilhada entre o servidor local e o KDS deste computador.</summary>
public sealed class ChamadasMesasService(IConfiguracaoRepository repositorio)
{
    public const string Chave="cozinha.kanban.chamadas.v1";
    private const string ChaveConfirmadas="cozinha.kanban.chamadas-confirmadas.v1";
    private static readonly object Trava=new();
    private sealed record Confirmacao(Guid Id,int ComandaId,Guid ChamadaId,DateTime CriadaEm);
    private T Ler<T>(string chave,T padrao)=>repositorio.ObterValor(chave) is string json?JsonSerializer.Deserialize<T>(json)??padrao:padrao;
    public IReadOnlyList<ChamadaMesa> Listar(){lock(Trava)return Ler<List<ChamadaMesa>>(Chave,[]).OrderBy(c=>c.CriadaEm).ToArray();}
    public ResultadoChamada Registrar(int comandaId,int mesa,string? solicitacao,Guid identificacao)
    {
        solicitacao=string.IsNullOrWhiteSpace(solicitacao)?"Solicita atendimento":solicitacao.Trim();
        if(mesa is <1 or >9999||identificacao==Guid.Empty||solicitacao.Length>120||solicitacao.Any(char.IsControl))throw new ArgumentException("Informe uma mesa válida e uma solicitação de até 120 caracteres.");
        lock(Trava)
        {
            var chamadas=Ler<List<ChamadaMesa>>(Chave,[]);
            var confirmadas=Ler<List<Confirmacao>>(ChaveConfirmadas,[]);
            var anterior=confirmadas.FirstOrDefault(c=>c.Id==identificacao);
            if(anterior is not null)
            {
                if(anterior.ComandaId!=comandaId)throw new ArgumentException("A identificação já foi usada para outra comanda.");
                var ativa=chamadas.FirstOrDefault(c=>c.Id==anterior.ChamadaId);return new(ativa,ativa is null);
            }
            if(confirmadas.Count>=10000)throw new InvalidOperationException("Limite de solicitações atingido. Confira as chamadas no computador.");
            var chamada=chamadas.FirstOrDefault(c=>c.Mesa==mesa);
            if(chamada is null)
            {
                if(chamadas.Count>=300)throw new InvalidOperationException("Atenda as chamadas pendentes antes de registrar novas.");
                chamada=new(Guid.NewGuid(),mesa,solicitacao,DateTime.UtcNow);chamadas.Add(chamada);
            }
            confirmadas.Add(new(identificacao,comandaId,chamada.Id,DateTime.UtcNow));
            repositorio.DefinirValores(new Dictionary<string,string>{{Chave,JsonSerializer.Serialize(chamadas)},{ChaveConfirmadas,JsonSerializer.Serialize(confirmadas)}},DateTime.UtcNow);
            return new(chamada,false);
        }
    }
    public void Atender(Guid id)
    {
        lock(Trava){var chamadas=Ler<List<ChamadaMesa>>(Chave,[]);chamadas.RemoveAll(c=>c.Id==id);repositorio.Definir(Chave,JsonSerializer.Serialize(chamadas),DateTime.UtcNow);}
    }
}
