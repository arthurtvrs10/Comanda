using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private GerenciadorPainelSenhas? _painelSenhas;
    private bool PodeChamarSenha()=>!_bloqueado&&!_cozinhaAtiva&&!_restauracao;
    private void AbrirPainelSenhas()
    {
        if(!PodeChamarSenha()||_repositorioSistema is null)return;
        try
        {
            _painelSenhas??=new GerenciadorPainelSenhas(new PainelSenhasService(_repositorioSistema),PodeChamarSenha);
            _painelSenhas.AbrirChamador(this);
        }
        catch { JanelaAviso.Mostrar("Painel de senhas","Não foi possível abrir o painel. Verifique as configurações e os monitores do Windows.",TipoAviso.Erro); }
    }
}
