using System.Windows;
using VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

namespace VarthexComanda.Desktop;
public partial class MainWindow
{
    private readonly PrimeiraConfiguracaoService? _primeiraConfiguracao;
    private JanelaConfiguracao? _janelaPrimeirosPassos;
    private void AtualizarFaixaConfiguracao()
    {
        var exibir=_primeiraConfiguracao?.ConfiguracaoPendente==true&&!FaixaConfiguracao.OcultadaNestaSessao&&_acesso.Autenticado&&!_bloqueado&&!_restauracao&&PermiteColaborador("configuracoes")&&PermiteColaborador("produtos");
        FaixaConfiguracao.Visibility=exibir?Visibility.Visible:Visibility.Collapsed;
        if(exibir)
        {
            try
            {
                var estado=_primeiraConfiguracao!.Carregar();
                FaixaConfiguracao.AtualizarProgresso(estado.Finalizado?7:estado.Concluidas.Count(i=>i is >=0 and <6&&estado.Erro(i)==""));
                FaixaConfiguracao.ToolTip=null;
            }
            catch(Exception ex) when(ex is System.IO.IOException or System.UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException)
            {
                FaixaConfiguracao.AtualizarProgresso(0);
                FaixaConfiguracao.ToolTip="Não foi possível ler o progresso salvo. Abra a configuração para conferir.";
            }
        }
    }
    private void ConfigurarFaixa_Click(object? sender,EventArgs e)=>AbrirPrimeirosPassos();
    private void AbrirPrimeirosPassos()
    {
        if(_primeiraConfiguracao is null||!PodeGerir()||!PermiteColaborador("configuracoes")||!PermiteColaborador("produtos"))return;
        if(_janelaPrimeirosPassos is not null){_janelaPrimeirosPassos.Activate();return;}
        try
        {
            _janelaPrimeirosPassos=new JanelaConfiguracao(_primeiraConfiguracao,
                PermiteColaborador("colaboradores")?owner=>{if(!PodeGerir()||!PermiteColaborador("colaboradores"))return;new Configuracao.JanelaColaboradores(_colaboradores,_acesso){Owner=owner}.ShowDialog();}:null,
                ()=>_colaboradores.Listar()){Owner=this};
            _janelaPrimeirosPassos.ShowDialog();
            _configuracaoView.ViewModel.Carregar();AplicarFundoSistema();_atendimentoView.ViewModel.AtualizarCategorias();_atendimentoView.ViewModel.AtualizarComandasAbertas();_cozinhaView.Atualizar();AtualizarPerfil();
        }
        catch(Exception ex){JanelaAviso.Mostrar("Primeiros passos",ex.Message,TipoAviso.Erro);}
        finally{_janelaPrimeirosPassos=null;AtualizarFaixaConfiguracao();}
    }
}
