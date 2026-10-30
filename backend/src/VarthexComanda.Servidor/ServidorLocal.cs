using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VarthexComanda.Application.Abstractions;

namespace VarthexComanda.Servidor;

/// <summary>Servidor no mesmo processo do desktop. Nunca abre o SQLite pela rede.</summary>
public sealed class ServidorLocal(AtendimentoRemoto atendimento, Func<bool>? podeOperar = null, RegistroEventos? eventos = null) : IDisposable, IAsyncDisposable
{
    public RegistroEventos Eventos { get; } = eventos ?? new();
    private readonly SemaphoreSlim _ciclo = new(1,1), _escritas = new(1,1);
    private readonly Dictionary<Guid,(string Assinatura,RespostaRemota Resposta)> _confirmadas = new();
    private readonly object _travaAcessos=new();
    private readonly Dictionary<string,AcessoServidor> _acessos=new();
    private WebApplication? _app;
    private string _chave = "";
    private bool _somenteNesteComputador;
    public bool Ativo => _app is not null;
    public bool Iniciando { get; private set; }
    public bool Parando { get; private set; }
    public DateTime? IniciadoEm { get; private set; }
    public IReadOnlyList<AcessoServidor> Acessos { get { lock(_travaAcessos)return _acessos.Values.OrderByDescending(a=>a.UltimoAcesso).ToArray(); } }
    public Func<bool>? InicioPermitido { get; set; }
    public int Porta { get; private set; }
    public event Action<int>? ComandaAlterada;
    public IReadOnlyList<string> Enderecos
    {
        get
        {
            if(!Ativo)return [];
            try{return (_somenteNesteComputador?new[]{"127.0.0.1"}:ObterEnderecos()).Select(ip=>$"http://{ip}:{Porta}").ToArray();}
            catch(NetworkInformationException){return [$"http://127.0.0.1:{Porta}"];}
        }
    }
    public string LinkAcesso(string endereco) => Ativo && Enderecos.Contains(endereco)
        ? endereco + "/#chave=" + _chave : throw new InvalidOperationException("Inicie o servidor e selecione um endereço disponível.");

    public async Task IniciarAsync(int porta = 3001, bool somenteNesteComputador = false)
    {
        if (porta is < 0 or > 65535 || (porta != 0 && porta < 1024)) throw new ArgumentOutOfRangeException(nameof(porta));
        await _ciclo.WaitAsync().ConfigureAwait(false);
        try
        {
            if (Ativo) return;
            if (InicioPermitido?.Invoke() == false) throw new InvalidOperationException("Aguarde o backup ou a restauração terminar.");
            Iniciando=true;
            if (podeOperar?.Invoke() == false) throw new InvalidOperationException("A licença precisa estar válida para iniciar o servidor.");
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args=[], ApplicationName=typeof(ServidorLocal).Assembly.FullName, ContentRootPath=AppContext.BaseDirectory });
            builder.Configuration.Sources.Clear();
            builder.Logging.ClearProviders(); // Não registrar cabeçalhos, chaves, observações ou dados do atendimento.
            builder.WebHost.ConfigureKestrel(o => { o.AddServerHeader=false; o.Limits.MaxRequestBodySize=16*1024; o.Listen(somenteNesteComputador?IPAddress.Loopback:IPAddress.Any,porta); });
            var app = builder.Build();
            var chave = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            app.Use(async (contexto, proximo) =>
            {
                contexto.Response.Headers.CacheControl="no-store";
                contexto.Response.Headers["X-Content-Type-Options"]="nosniff";
                contexto.Response.Headers["Referrer-Policy"]="no-referrer";
                contexto.Response.Headers["Content-Security-Policy"]="default-src 'self'; script-src 'self'; style-src 'self'; connect-src 'self'; img-src 'self' blob:; base-uri 'none'; frame-ancestors 'none'; form-action 'self'";
                if (!EnderecoLocal(contexto.Connection.RemoteIpAddress)) { contexto.Response.StatusCode=403; return; }
                if (contexto.Request.Path.StartsWithSegments("/api"))
                {
                    if(Parando){contexto.Response.StatusCode=503;return;}
                    var esperado = Encoding.UTF8.GetBytes("Bearer "+chave);
                    var recebido = Encoding.UTF8.GetBytes(contexto.Request.Headers.Authorization.ToString());
                    if (!CryptographicOperations.FixedTimeEquals(esperado,recebido)) { contexto.Response.StatusCode=401; return; }
                    if (podeOperar?.Invoke() == false) { contexto.Response.StatusCode=403; await contexto.Response.WriteAsJsonAsync(new { Erro="Acesso interrompido. Confira a licença no computador." }); return; }
                    if (contexto.Request.Headers.Origin.Count > 0 && contexto.Request.Headers.Origin.ToString() != $"http://{contexto.Request.Host}") { contexto.Response.StatusCode=403; return; }
                }
                try
                {
                    await proximo(contexto);
                    if(contexto.Request.Path.StartsWithSegments("/api") && contexto.Response.StatusCode<400 && contexto.Connection.RemoteIpAddress is { } ip)
                        RegistrarAcesso(ip.IsIPv4MappedToIPv6?ip.MapToIPv4().ToString():ip.ToString());
                }
                catch (Exception ex) when (ex is JsonException or BadHttpRequestException or OverflowException or ArgumentException)
                { if (!contexto.Response.HasStarted) { contexto.Response.StatusCode=400; await contexto.Response.WriteAsJsonAsync(new { Erro="Dados inválidos. Confira os campos e tente novamente." }); } }
                catch (OperationCanceledException) when (contexto.RequestAborted.IsCancellationRequested) { }
                catch (Exception)
                { if (!contexto.Response.HasStarted) { contexto.Response.StatusCode=503; await contexto.Response.WriteAsJsonAsync(new { Erro="Não foi possível concluir. Atualize a comanda e tente novamente." }); } }
            });
            app.MapGet("/",() => Arquivo("index.html","text/html; charset=utf-8"));
            app.MapGet("/app.css",() => Arquivo("app.css","text/css; charset=utf-8"));
            app.MapGet("/app.js",() => Arquivo("app.js","text/javascript; charset=utf-8"));
            app.MapGet("/logo.svg",() => Arquivo("logo.svg","image/svg+xml"));
            app.MapGet("/api/catalogo",() => Results.Json(atendimento.Catalogo()));
            app.MapGet("/api/produtos/{id:int}/foto",(int id) => atendimento.Foto(id) is { } foto
                ? Results.Bytes(foto.Conteudo,foto.Tipo) : Results.NotFound());
            app.MapGet("/api/comandas",() => Results.Json(atendimento.Comandas()));
            app.MapGet("/api/comandas/{id:int}",(int id) => Resultado(atendimento.Consultar(id)));
            app.MapPost("/api/comandas",(Func<HttpContext,Task<IResult>>)(c => Escrever(c,null)));
            app.MapPost("/api/comandas/{id:int}/itens",async (HttpContext c,int id) => await Escrever(c,id));
            try
            {
                await app.StartAsync().ConfigureAwait(false);
                var urls = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses;
                Porta=new Uri(urls.First()).Port;
                _chave=chave; _confirmadas.Clear(); _somenteNesteComputador=somenteNesteComputador;
                lock(_travaAcessos)_acessos.Clear();
                IniciadoEm=DateTime.UtcNow;_app=app;
            }
            catch { await app.DisposeAsync().ConfigureAwait(false); throw; }
        }
        finally { Iniciando=false; _ciclo.Release(); }
    }

    private void RegistrarAcesso(string endereco)
    {
        lock(_travaAcessos)
        {
            if(!_acessos.ContainsKey(endereco) && _acessos.Count>=256)_acessos.Remove(_acessos.Values.MinBy(a=>a.UltimoAcesso)!.Endereco);
            _acessos.TryGetValue(endereco,out var anterior);
            _acessos[endereco]=new(endereco,DateTime.UtcNow,Math.Min(int.MaxValue-1,anterior?.Requisicoes??0)+1);
        }
    }

    private async Task<IResult> Escrever(HttpContext contexto,int? comandaId)
    {
        if (!contexto.Request.HasJsonContentType() || !Guid.TryParse(contexto.Request.Headers["Idempotency-Key"],out var chave))
            return Resultado(AtendimentoRemoto.Erro(400,"Envie JSON com uma identificação única do lançamento."));
        using var texto = new StreamReader(contexto.Request.Body,Encoding.UTF8);
        var corpo = await texto.ReadToEndAsync(contexto.RequestAborted);
        var assinatura = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(contexto.Request.Path + "\n" + corpo)));
        using var evento = Eventos.Iniciar(OrigemEvento.Servidor, comandaId is null ? AcaoEvento.ReceberPedido : AcaoEvento.ReceberItem, aguarda: true);
        try { await _escritas.WaitAsync(contexto.RequestAborted).ConfigureAwait(false); }
        catch (OperationCanceledException) { evento.Finalizar(EstadoEvento.Cancelado); throw; }
        evento.Processar();
        IResult Concluir(RespostaRemota resposta)
        {
            evento.Finalizar(resposta.Status < 300 ? EstadoEvento.Concluido : resposta.Status < 500 ? EstadoEvento.Recusado : EstadoEvento.Falha);
            return Resultado(resposta);
        }
        try
        {
            if (_confirmadas.TryGetValue(chave,out var anterior))
                return anterior.Assinatura==assinatura ? Concluir(anterior.Resposta) : Concluir(AtendimentoRemoto.Erro(409,"Esta identificação já foi usada em outro lançamento."));
            if (_confirmadas.Count>=20000) return Concluir(AtendimentoRemoto.Erro(503,"Limite da sessão atingido. Confira os lançamentos e reinicie o servidor no computador."));
            var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web) { UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow };
            var resposta = comandaId is int id
                ? atendimento.Adicionar(id,JsonSerializer.Deserialize<LancamentoRemoto>(corpo,opcoes) ?? throw new JsonException())
                : atendimento.Abrir(JsonSerializer.Deserialize<AberturaRemota>(corpo,opcoes) ?? throw new JsonException());
            if (resposta.Status < 300)
            {
                // Não descartar confirmações de uma sessão ativa: a repetição nunca vira uma nova inclusão.
                var alterada = (ComandaRemota)resposta.Dados;
                resposta=new(resposta.Status,new { Id=alterada.Id });
                _confirmadas[chave]=(assinatura,resposta);
                try { ComandaAlterada?.Invoke(alterada.Id); } catch { /* O commit confirmado não depende da apresentação. */ }
            }
            return Concluir(resposta);
        }
        catch (Exception ex) when (ex is JsonException or OverflowException or ArgumentException)
        { evento.Finalizar(EstadoEvento.Recusado); throw; }
        catch (OperationCanceledException) when (contexto.RequestAborted.IsCancellationRequested)
        { evento.Finalizar(EstadoEvento.Cancelado); throw; }
        finally { _escritas.Release(); }
    }
    private static IResult Resultado(RespostaRemota resposta) => Results.Json(resposta.Dados,statusCode:resposta.Status);
    private static IResult Arquivo(string nome,string tipo)
    {
        using var stream=typeof(ServidorLocal).Assembly.GetManifestResourceStream("VarthexComanda.Servidor.Web."+nome)!;
        using var leitor=new StreamReader(stream,Encoding.UTF8);
        return Results.Content(leitor.ReadToEnd(),tipo,Encoding.UTF8);
    }
    public async Task PararAsync()
    {
        await _ciclo.WaitAsync().ConfigureAwait(false);
        try
        {
            var app=_app; if(app is null)return;
            Parando=true;
            using var prazo=new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try { await app.StopAsync(prazo.Token).ConfigureAwait(false); }
            finally
            {
                try{await app.DisposeAsync().ConfigureAwait(false);}
                finally{_app=null;_chave="";Porta=0;IniciadoEm=null;}
            }
        }
        finally { Parando=false;_ciclo.Release(); }
    }
    // O host também é encerrado pelo Closed do WPF. Não executar o começo da
    // parada no contexto STA que aguarda o resultado: callbacks do host podem
    // capturar esse contexto e esperar uma Dispatcher já bloqueada.
    public void Dispose() => Task.Run(PararAsync).GetAwaiter().GetResult();
    public async ValueTask DisposeAsync() => await PararAsync().ConfigureAwait(false);
    private static bool EnderecoLocal(IPAddress? endereco)
    {
        if(endereco is null)return false;
        if(endereco.IsIPv4MappedToIPv6)endereco=endereco.MapToIPv4();
        if(IPAddress.IsLoopback(endereco))return true;
        var b=endereco.GetAddressBytes();
        return b.Length==4 && (b[0]==10 || b[0]==192&&b[1]==168 || b[0]==172&&b[1] is >=16 and <=31 || b[0]==169&&b[1]==254);
    }
    private static IEnumerable<string> ObterEnderecos() => EnderecosServidor.Ler();
}
