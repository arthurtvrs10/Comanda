using System.Windows;
using VarthexComanda.Desktop.Rede;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private ServidorLocal? _servidorLocal;
    private JanelaServidor? _janelaServidor;
    private ServidorViewModel? _controleServidor;
    private Task _inicioAutomaticoServidor=Task.CompletedTask;
    private void InicializarServidor(ServidorLocal? servidor)
    {
        _servidorLocal=servidor;
        if(servidor is null)return;
        servidor.InicioPermitido=()=>!_restauracao && !_backupView.ViewModel.EmAndamento;
        _backupView.ViewModel.RestauracaoPermitida=()=>!servidor.Ativo && !servidor.Iniciando && !servidor.Parando;
        _controleServidor=new ServidorViewModel(servidor,_pathsSistema.DatabasePath,_repositorioSistema is null?null:new VarthexComanda.Application.Configuracao.ConfiguracaoServidorService(_repositorioSistema));
        Loaded+=(_,_)=>
        {
            if(!_restauracao)_inicioAutomaticoServidor=_controleServidor.IniciarAutomaticamenteAsync();
        };
        servidor.ComandaAlterada+=NotificarLancamentoRemoto;
        Closed+=(_,_)=>{ servidor.ComandaAlterada-=NotificarLancamentoRemoto; _janelaServidor?.Close(); _controleServidor.Encerrar(); };
    }
    private void NotificarLancamentoRemoto(int id)
    {
        if(Dispatcher.HasShutdownStarted)return;
        Dispatcher.BeginInvoke(new Action(()=>
        {
            try { _atendimentoView.ViewModel.AtualizarLancamentoRemoto(id); if(ReferenceEquals(ConteudoPrincipal.JanelaAtiva?.Conteudo,_cozinhaView))_cozinhaView.Atualizar(); }
            catch { /* A confirmação remota já foi persistida; a próxima atualização recupera os dados. */ }
        }));
    }
    private void AbrirGuiaServidor() => new JanelaGuiaServidor(()=>ExecutarDestino(_destinosRibbon.Single(d=>d.Nome=="Servidor / acesso pelo celular"))) { Owner=this }.ShowDialog();

    private void AbrirServidorLocal()
    {
        if(!PodeGerir() || !PermiteColaborador("servidor"))return;
        if(_servidorLocal is null){JanelaAviso.Mostrar("Servidor","O servidor não está disponível nesta sessão.",TipoAviso.Informacao);return;}
        if(_janelaServidor is not null){_janelaServidor.Activate();return;}
        _janelaServidor=new JanelaServidor(_controleServidor!){Owner=this};
        ProtegerTela(_janelaServidor,"servidor");
        _janelaServidor.Closed+=(_,_)=>_janelaServidor=null;
        _janelaServidor.Show();
    }
}
