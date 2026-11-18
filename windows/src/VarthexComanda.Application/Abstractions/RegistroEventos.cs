using VarthexComanda.Application.Catalogo;

namespace VarthexComanda.Application.Abstractions;

public enum OrigemEvento { Atendimento, Cozinha, Servidor }
public enum AcaoEvento { AbrirComanda, AdicionarItem, AtualizarPreparo, ReceberPedido, ReceberItem }
public enum EstadoEvento { Aguardando, Processando, Concluido, Recusado, Falha, Cancelado }

public sealed record EventoSistema(long Id, DateTime CriadoEm, DateTime AtualizadoEm, OrigemEvento Origem, AcaoEvento Acao, EstadoEvento Estado)
{
    public string Operacao => Acao switch
    {
        AcaoEvento.AbrirComanda => "Abrir comanda", AcaoEvento.AdicionarItem => "Adicionar item",
        AcaoEvento.AtualizarPreparo => "Atualizar preparo", AcaoEvento.ReceberPedido => "Receber pedido web", _ => "Receber item web"
    };
    public string Situacao => Estado switch
    {
        EstadoEvento.Concluido => "Concluído", _ => Estado.ToString()
    };
    public bool EmCurso => Estado is EstadoEvento.Aguardando or EstadoEvento.Processando;
}

/// <summary>Observação das operações reais. Não reenvia nem repete operações financeiras ou pedidos.</summary>
public sealed class RegistroEventos(Action<EventoSistema>? registrar = null)
{
    public const int Capacidade = 500;
    private readonly object _trava = new();
    private readonly List<EventoSistema> _eventos = [];
    private long _sequencia, _omitidos;
    public long Omitidos { get { lock (_trava) return _omitidos; } }
    public IReadOnlyList<EventoSistema> Consultar() { lock (_trava) return _eventos.OrderByDescending(e => e.Id).ToArray(); }
    public Operacao Iniciar(OrigemEvento origem, AcaoEvento acao, bool aguarda = false)
    {
        lock (_trava)
        {
            if (_eventos.Count >= Capacidade)
            {
                var indice = _eventos.FindIndex(e => !e.EmCurso);
                if (indice < 0) { _omitidos++; return new(this, 0); }
                _eventos.RemoveAt(indice);
            }
            var id = ++_sequencia; var agora = DateTime.UtcNow;
            _eventos.Add(new(id, agora, agora, origem, acao, aguarda ? EstadoEvento.Aguardando : EstadoEvento.Processando));
            return new(this, id);
        }
    }
    private void Alterar(long id, EstadoEvento estado)
    {
        EventoSistema? alterado;
        lock (_trava)
        {
            var indice = _eventos.FindIndex(e => e.Id == id);
            if (indice < 0 || !_eventos[indice].EmCurso) return;
            alterado = _eventos[indice] with { Estado = estado, AtualizadoEm = DateTime.UtcNow };
            _eventos[indice] = alterado;
        }
        // O diagnóstico nunca pode invalidar uma gravação já confirmada.
        if (!alterado.EmCurso) try { registrar?.Invoke(alterado); } catch { }
    }
    public Resultado<T> Executar<T>(OrigemEvento origem, AcaoEvento acao, Func<Resultado<T>> executar) where T : class
    {
        using var operacao = Iniciar(origem, acao);
        var resultado = executar();
        operacao.Finalizar(resultado.Sucesso ? EstadoEvento.Concluido : EstadoEvento.Recusado);
        return resultado;
    }
    public sealed class Operacao(RegistroEventos registro, long id) : IDisposable
    {
        public void Processar() => registro.Alterar(id, EstadoEvento.Processando);
        public void Finalizar(EstadoEvento estado) => registro.Alterar(id, estado);
        public void Dispose() => registro.Alterar(id, EstadoEvento.Falha);
    }
}
