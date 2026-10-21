using System.Windows;
using System.Windows.Media;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Desktop;

public partial class MainWindow
{
    private AppPaths _pathsSistema=null!;
    private IConfiguracaoRepository? _repositorioSistema;
    private JanelaConfiguracoesSistema? _configuracoesModal;
    private PreferenciasSistema LerPreferencias()
    {
        try{return _repositorioSistema is null?new():new ConfiguracoesSistemaService(_repositorioSistema).Carregar();}
        catch{return new();}
    }
    private void AbrirSistemaConfiguracoes()
    {
        if(_bloqueado||!_acesso.Autenticado||_restauracao)return;
        if(_configuracoesModal!=null){_configuracoesModal.Activate();return;}
        if(_repositorioSistema is null)
        {
            JanelaAviso.Mostrar("Configurações","O repositório de configurações não está disponível nesta sessão.",TipoAviso.Erro);return;
        }
        try
        {
            _configuracoesModal=new JanelaConfiguracoesSistema(new ConfiguracoesSistemaService(_repositorioSistema),new ObterConfiguracao(_repositorioSistema).Executar(),
                ()=>{_atendimentoView.ViewModel.AtualizarComandasAbertas();return _atendimentoView.ViewModel.ComandasAbertas.Select(c=>c.Numero).DefaultIfEmpty(0).Max();},
                ()=>_atendimentoView.ViewModel.PesquisarProdutosGlobais(""),
                ()=>DiagnosticoSistema.Gerar(_pathsSistema,_configuracaoView.ViewModel.NomeEstabelecimento,_configuracaoView.ConsultarBackup?.Invoke()??"Não disponível"),
                AcaoConfiguracao,()=>
                {
                    _configuracaoView.ViewModel.Carregar();AplicarFundoSistema();_cozinhaView.Atualizar();_atendimentoView.ViewModel.AtualizarComandasAbertas();
                }) { Owner=this };
            _configuracoesModal.ShowDialog();
        }
        catch{JanelaAviso.Mostrar("Configurações","Não foi possível abrir as configurações. Verifique os dados e tente novamente.",TipoAviso.Erro);}
        finally{_configuracoesModal=null;}
    }
    private void AplicarFundoSistema()
    {
        var preferencias=LerPreferencias();
        try
        {
            FundoTrabalho.Background=(Brush)new BrushConverter().ConvertFromString(preferencias.CorFundo)!;
            var imagem=JanelaConfiguracoesSistema.CriarImagem(preferencias.ImagemFundo);
            if(imagem is null){ImagemTrabalho.Background=null;return;}
            var brush=new ImageBrush(imagem)
            {
                Stretch=preferencias.PosicaoImagem switch {"Preencher"=>Stretch.Fill,"Zoom"=>Stretch.UniformToFill,_=>Stretch.None},
                AlignmentX=AlignmentX.Center,AlignmentY=AlignmentY.Center
            };
            if(preferencias.PosicaoImagem=="Repetir")
            {brush.TileMode=TileMode.Tile;brush.ViewportUnits=BrushMappingMode.Absolute;brush.Viewport=new Rect(0,0,imagem.Width,imagem.Height);}
            ImagemTrabalho.Background=brush;
        }
        catch{ImagemTrabalho.Background=null;FundoTrabalho.SetResourceReference(System.Windows.Controls.Border.BackgroundProperty,"Canvas");}
    }
    private void ImprimirConta()
    {
        var comanda=_atendimentoView.ViewModel.ComandaAtual;
        if(comanda is null||_bloqueado)return;
        var owner=System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w=>w.IsActive)??this;
        var numeracao=_repositorioSistema is null ? null : new NumeracaoComandasService(_repositorioSistema).Carregar();
        var impressora=_repositorioSistema is null ? null : new ConfiguracoesImpressaoService(_repositorioSistema).ObterCaixaConfigurado();
        ComprovanteConta.Visualizar(owner,ComprovanteConta.MontarTexto(LerPreferencias(),_configuracaoView.ViewModel.NomeEstabelecimento,comanda.Numero,_atendimentoView.ViewModel.Itens,comanda.CodigoPedido,false,numeracao?.UsarComoSenhaAtendimento==true?comanda.Numero:null),impressora);
    }
}
