using System.Windows;
using System.Windows.Threading;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao;

public sealed class GerenciadorPainelSenhas : IDisposable
{
    private readonly PainelSenhasService _service;
    private readonly Func<bool> _autorizado;
    private readonly Func<EstadoPainelSenhas,PreferenciasPainelSenhas,JanelaExibicaoSenhas> _criarExibicao;
    private readonly AudioPainelSenhas _audio=new();
    private readonly DispatcherTimer _timer=new() { Interval=TimeSpan.FromSeconds(1) };
    private JanelaExibicaoSenhas? _exibicao;
    private JanelaChamadorSenhas? _chamador;
    public PreferenciasPainelSenhas Preferencias { get; }
    public EstadoPainelSenhas Estado { get; }=new();
    public bool PainelVisivel=>_exibicao?.IsVisible==true;
    public GerenciadorPainelSenhas(PainelSenhasService service,Func<bool> autorizado,
        Func<EstadoPainelSenhas,PreferenciasPainelSenhas,JanelaExibicaoSenhas>? criarExibicao=null)
    {
        _service=service;_autorizado=autorizado;Preferencias=service.Carregar();
        _criarExibicao=criarExibicao??((estado,preferencias)=>new JanelaExibicaoSenhas(estado,preferencias));
        _timer.Tick+=VerificarTempo;_timer.Start();
    }
    private void VerificarTempo(object? sender,EventArgs e)=>Estado.VerificarExpiracao(Preferencias.LimparAposUmMinuto);
    private void ConferirAcesso()
    {
        if(!_autorizado())throw new InvalidOperationException("Entre no sistema para operar o painel de senhas.");
    }
    public void AbrirChamador(Window owner)
    {
        ConferirAcesso();
        if(_chamador is null)
        {
            _chamador=new JanelaChamadorSenhas(this) { Owner=owner };
            _chamador.Closed+=(_,_)=>_chamador=null;
        }
        _chamador.Show();_chamador.Activate();
    }
    public void Salvar()
    {
        ConferirAcesso();_service.Salvar(Preferencias);_exibicao?.Aplicar(Preferencias);
    }
    public string Chamar(string senha)
    {
        ConferirAcesso();
        if(!EstadoPainelSenhas.SenhaValida(senha.Trim()))throw new ArgumentException("Digite uma senha com 1 a 9 números.");
        Salvar();
        // Posiciona a tela antes de mudar o número: uma falha de monitor não confirma a chamada.
        var aviso=Exibir();Estado.Chamar(senha);
        var audio=_audio.Anunciar(senha.Trim(),Preferencias.Beep,Preferencias.Voz,Preferencias.VozFeminina);
        return string.Join(" ",new[]{aviso,audio}.Where(x=>!string.IsNullOrWhiteSpace(x)));
    }
    public string Exibir()
    {
        ConferirAcesso();
        var monitores=MonitoresPainel.Listar();
        var monitor=monitores.FirstOrDefault(m=>m.Id==Preferencias.Monitor);
        bool ausente=monitor is null&&!string.IsNullOrEmpty(Preferencias.Monitor);
        monitor??=monitores.FirstOrDefault(m=>!m.Principal)??monitores[0];
        if(_exibicao is null)
        {
            _exibicao=_criarExibicao(Estado,Preferencias);
            _exibicao.Closed+=(_,_)=>_exibicao=null;
        }
        _exibicao.Aplicar(Preferencias);_exibicao.Show();_exibicao.ConfigurarMonitor(monitor,Preferencias.TelaCheia);
        _chamador?.Activate();
        return ausente?"O monitor salvo está desconectado. O painel foi aberto no monitor disponível.":monitores.Count==1?"Há um único monitor conectado. O painel está neste monitor.":"";
    }
    public void Ocultar() { ConferirAcesso();_exibicao?.Hide(); }
    public void Limpar() { ConferirAcesso();_audio.Parar();Estado.Limpar(); }
    public void FecharChamador()=>_chamador?.Close();
    public void Encerrar()
    {
        ConferirAcesso();_audio.Parar();_exibicao?.Close();Estado.Limpar();FecharChamador();
    }
    public void Dispose()
    {
        _timer.Stop();_timer.Tick-=VerificarTempo;_chamador?.Close();_exibicao?.Close();_audio.Dispose();
    }
}
