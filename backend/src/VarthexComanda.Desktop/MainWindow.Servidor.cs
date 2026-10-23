using System.Windows;
using VarthexComanda.Desktop.Rede;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private ServidorLocal? _servidorLocal;
    private JanelaServidor? _janelaServidor;
    private void InicializarServidor(ServidorLocal? servidor)
    {
        _servidorLocal=servidor;
        if(servidor is null)return;
        servidor.InicioPermitido=()=>!_restauracao && !_backupView.ViewModel.EmAndamento;
        _backupView.ViewModel.RestauracaoPermitida=()=>!servidor.Ativo && !servidor.Iniciando;
        servidor.ComandaAlterada+=NotificarLancamentoRemoto;
        Closed+=(_,_)=>{ servidor.ComandaAlterada-=NotificarLancamentoRemoto; _janelaServidor?.Close(); servidor.Dispose(); };
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
    private void AbrirServidorLocal()
    {
        if(!PodeGerir())return;
        if(_servidorLocal is null){JanelaAviso.Mostrar("Servidor","O servidor não está disponível nesta sessão.",TipoAviso.Informacao);return;}
        if(_janelaServidor is not null){_janelaServidor.Activate();return;}
        _janelaServidor=new JanelaServidor(_servidorLocal){Owner=this};
        _janelaServidor.Closed+=(_,_)=>_janelaServidor=null;
        _janelaServidor.Show();
    }
}
