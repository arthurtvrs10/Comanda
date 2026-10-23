using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Atendimento;

public class EncerramentoDialog : IEncerramentoDialog
{
    private readonly Func<EncerramentoViewModel> _fabricaViewModel;
    private readonly IConfiguracaoRepository? _configuracoes;

    public EncerramentoDialog(Func<EncerramentoViewModel> fabricaViewModel,IConfiguracaoRepository? configuracoes=null)
    {
        _fabricaViewModel = fabricaViewModel;_configuracoes=configuracoes;
    }

    public bool Abrir(int comandaId)
    {
        var viewModel = _fabricaViewModel();
        viewModel.Carregar(comandaId);
        Action<System.Windows.Window>? imprimir=_configuracoes is null?null:owner=>
        {
            var prefs=new ConfiguracoesSistemaService(_configuracoes).Carregar();
            var nome=new ObterConfiguracao(_configuracoes).Executar().NomeEstabelecimento;
            var impressora=new ConfiguracoesImpressaoService(_configuracoes).Carregar().Caixa;
            var texto=ComprovanteConta.MontarTexto(prefs,nome,viewModel.NumeroComanda,viewModel.Itens,viewModel.ComandaAtual?.CodigoPedido);
            ComprovanteConta.Visualizar(owner,texto,impressora,criarDocumento:mm=>CupomTermico.Conta(prefs,nome,viewModel.ComandaAtual!,viewModel.Itens,mm));
        };
        var view = new EncerramentoView(viewModel,imprimir) { Owner = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(w => w.IsActive)
            ?? System.Windows.Application.Current.MainWindow };
        var concluido = view.ShowDialog() == true;
        if(concluido&&view.ImprimirAposFinalizar&&imprimir is not null)
        {
            try { imprimir(view.Owner??System.Windows.Application.Current.MainWindow); }
            catch { JanelaAviso.Mostrar("Impressão","O pedido foi finalizado, mas não foi possível abrir a impressão. Reimprima pelo histórico de vendas."); }
        }
        return concluido;
    }
}
