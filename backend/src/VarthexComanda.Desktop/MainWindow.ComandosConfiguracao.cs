using System.Windows;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private bool PodeAlterarSenha() => !_bloqueado && !_restauracao && (_acesso.Autenticado || _colaboradores.SessaoAtual is not null);

    private void AbrirNumeracaoPedidos()
    {
        if (!PodeGerir() || _repositorioSistema is null) return;
        AbrirComandoConfiguracao(() => new JanelaNumeracaoPedidos(new NumeracaoPedidosService(
            _repositorioSistema, _atendimentoView.ViewModel.ConsultarMaiorCodigoPedido)) { Owner=this }.ShowDialog());
    }

    private void AbrirImpressoras()
    {
        if (!PodeGerir() || _repositorioSistema is null) return;
        AbrirComandoConfiguracao(() => new JanelaImpressoras(new ConfiguracoesImpressaoService(_repositorioSistema),
            () => _cozinhaView.Atualizar()) { Owner=this }.ShowDialog());
    }

    private void AbrirColaboradores()
    {
        if (!PodeGerir()) return;
        AbrirComandoConfiguracao(() => new JanelaColaboradores(_colaboradores,_acesso) { Owner=this }.ShowDialog());
        AtualizarPerfil();
    }
    private void AbrirMinhaAssinatura()
    {
        if(!PodeGerir())return;
        AbrirComandoConfiguracao(()=>
        {
            var janela=new Licenciamento.JanelaMinhaAssinatura(()=>new Licenciamento.ServicoLicenca().Verificar()) { Owner=this };
            var trocar=janela.ShowDialog();
            if(trocar==true)AcaoConfiguracao("licenca");
            return trocar;
        });
    }

    private void AbrirAlteracaoAcesso(bool pin)
    {
        if (!PodeAlterarSenha()) return;
        if (pin && !_acesso.Autenticado) return;
        AbrirComandoConfiguracao(() =>
        {
            var usuario=_colaboradores.SessaoAtual;
            Action<string,string>? alterar=usuario is null ? null : (atual,nova) => _colaboradores.AlterarSenha(usuario.Id,atual,nova);
            return new JanelaAlterarAcesso(_acesso,_acessoAtendimento,pin,alterar,usuario?.Nome)
                { Owner=_configuracoesModal??(Window)this }.ShowDialog();
        });
    }

    private static void AbrirComandoConfiguracao(Func<bool?> abrir)
    {
        try { abrir(); }
        catch { JanelaAviso.Mostrar("Configurações","Não foi possível abrir essa opção. Verifique os dados e tente novamente.",TipoAviso.Erro); }
    }
}
