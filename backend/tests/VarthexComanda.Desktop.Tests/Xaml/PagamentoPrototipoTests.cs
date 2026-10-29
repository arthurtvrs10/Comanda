using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Tests.Catalogo;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Atendimento.Prototipos;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class PagamentoPrototipoTests
{
    private static (EfComandaRepository Repo,int Id,PagamentoFluxoPrototipoViewModel Modelo) Preparar(BancoDeHomologacao banco,long total=3200)
    {
        var repo=new EfComandaRepository(banco.Fabrica);var comanda=repo.AbrirComanda(102,DateTime.UtcNow);
        var itens=total==3200?new[] { banco.CriarProduto("Lanche da casa",2000),banco.CriarProduto("Suco natural",800),banco.CriarProduto("Água",400) }:new[] { banco.CriarProduto("Conta para dividir",total) };
        foreach(var item in itens)repo.AdicionarItem(comanda.Id,item,1,DateTime.UtcNow);
        return(repo,comanda.Id,Abrir(repo,comanda.Id));
    }
    private static PagamentoFluxoPrototipoViewModel Abrir(EfComandaRepository repo,int id)
    { var core=new EncerramentoViewModel(new EncerrarComanda(repo,new FakeClock()),repo);core.Carregar(id);return new(core); }
    private static void Receber(PagamentoFluxoPrototipoViewModel vm,FormaPagamento forma,string valor,string recebido="")
    {
        vm.SelecionarFormaCommand.Execute(forma);vm.Pagamento.ValorPagamentoTexto=valor;vm.Pagamento.ValorRecebidoTexto=recebido;
        Assert.True(vm.RegistrarCommand.CanExecute(null),vm.OrientacaoEntrada);vm.RegistrarCommand.Execute(null);
    }
    [Fact]
    public void PagamentoParcialExigeNovaEscolhaEFinalizacaoNaoDuplicaVenda()
    {
        using var banco=new BancoDeHomologacao();var (repo,id,vm)=Preparar(banco);using var modelo=vm;
        Assert.False(vm.RegistrarCommand.CanExecute(null));Assert.False(vm.FinalizarCommand.CanExecute(null));
        vm.SelecionarFormaCommand.Execute(FormaPagamento.Pix);Assert.Empty(vm.Pagamento.Pagamentos);
        Receber(vm,FormaPagamento.Pix,"10,00");Assert.Equal(2200,vm.Pagamento.FaltaPagarCentavos);
        vm.RegistrarCommand.Execute(null);Assert.Single(vm.Pagamento.Pagamentos);Assert.Null(vm.Pagamento.FormaSelecionada);
        using(var reaberto=Abrir(repo,id)){Assert.Equal(1000,reaberto.Pagamento.TotalPagoCentavos);Assert.Equal(2200,reaberto.Pagamento.FaltaPagarCentavos);}
        Receber(vm,FormaPagamento.Dinheiro,"22,00","50,00");Assert.Equal(2800,vm.Pagamento.Pagamentos.Last().TrocoCentavos);
        Assert.True(vm.FinalizarCommand.CanExecute(null));vm.FinalizarCommand.Execute(null);vm.FinalizarCommand.Execute(null);
        Assert.True(vm.Finalizado);Assert.False(vm.RegistrarCommand.CanExecute(null));
        using var db=banco.Fabrica.CreateDbContext();var venda=Assert.Single(db.Vendas.ToArray());Assert.Equal(3200,venda.TotalCentavos);Assert.Equal(2,PagamentosConta.Ler(venda.PagamentosJson).Count);
    }
    [Theory]
    [InlineData("")][InlineData("0")][InlineData("-1")][InlineData("32,001")][InlineData("33,00")][InlineData("abc")]
    public void ValorInvalidoNaoPodeSerRegistrado(string valor)
    {
        using var banco=new BancoDeHomologacao();var (repo,id,vm)=Preparar(banco);using var modelo=vm;
        vm.SelecionarFormaCommand.Execute(FormaPagamento.Pix);vm.Pagamento.ValorPagamentoTexto=valor;
        Assert.False(vm.RegistrarCommand.CanExecute(null));vm.RegistrarCommand.Execute(null);
        Assert.Empty(PagamentosConta.Ler(repo.BuscarComItens(id)!.Comanda.PagamentosJson));Assert.False(vm.FinalizarCommand.CanExecute(null));
    }
    [Fact]
    public void DinheiroInsuficienteBloqueiaETrocoEValorExatoSaoDistintos()
    {
        using var banco=new BancoDeHomologacao();var (_,_,vm)=Preparar(banco);using var modelo=vm;
        vm.SelecionarFormaCommand.Execute(FormaPagamento.Dinheiro);vm.Pagamento.ValorRecebidoTexto="20,00";
        Assert.False(vm.RegistrarCommand.CanExecute(null));Assert.Contains("dinheiro recebido",vm.OrientacaoEntrada);
        vm.Pagamento.ValorRecebidoTexto="50,00";Assert.True(vm.RegistrarCommand.CanExecute(null));Assert.Equal(1800,vm.Pagamento.TrocoCentavos);
        vm.Pagamento.ValorRecebidoTexto="";vm.RegistrarCommand.Execute(null);var registro=Assert.Single(vm.Pagamento.Pagamentos);Assert.Equal(3200,registro.RecebidoCentavos);Assert.Equal(0,registro.TrocoCentavos);
    }
    [Fact]
    public void RateioDistribuiCentavosSemPerderTotal()
    {
        using var banco=new BancoDeHomologacao();var (_,_,vm)=Preparar(banco,10001);using var modelo=vm;
        vm.PorPessoasCommand.Execute(null);vm.Pessoas="3";Assert.Contains("33,34",vm.DivisaoDescricao);Assert.Contains("33,33",vm.DivisaoDescricao);
        vm.AplicarDivisaoCommand.Execute(null);Assert.Equal("33,34",vm.Pagamento.ValorPagamentoTexto);Receber(vm,FormaPagamento.Pix,"33,34");
        vm.Pessoas="2";vm.AplicarDivisaoCommand.Execute(null);Assert.Equal("33,34",vm.Pagamento.ValorPagamentoTexto);Receber(vm,FormaPagamento.Credito,"33,34");
        Receber(vm,FormaPagamento.Debito,"33,33");Assert.Equal(10001,vm.Pagamento.TotalPagoCentavos);Assert.Equal(0,vm.Pagamento.FaltaPagarCentavos);
    }
    [Fact]
    public void CorrecaoExigeConfirmacaoEPreservaAuditoria()
    {
        using var banco=new BancoDeHomologacao();var (repo,id,vm)=Preparar(banco);using var modelo=vm;Receber(vm,FormaPagamento.Pix,"32,00");
        var registro=Assert.Single(vm.Pagamento.Pagamentos);vm.PedirRemocaoCommand.Execute(registro);
        Assert.True(vm.TemRemocao);Assert.False(vm.FinalizarCommand.CanExecute(null));Assert.Equal(0,vm.Pagamento.FaltaPagarCentavos);
        vm.ManterRegistroCommand.Execute(null);Assert.False(vm.TemRemocao);Assert.Single(vm.Pagamento.Pagamentos);
        vm.PedirRemocaoCommand.Execute(registro);vm.ConfirmarRemocaoCommand.Execute(null);Assert.Empty(vm.Pagamento.Pagamentos);Assert.Equal(3200,vm.Pagamento.FaltaPagarCentavos);
        Assert.True(Assert.Single(PagamentosConta.Ler(repo.BuscarComItens(id)!.Comanda.PagamentosJson)).Cancelado);Assert.Equal(3,repo.BuscarComItens(id)!.Itens.Count);
    }
    [Fact]
    public void ItensPagosNaoSaoSelecionadosNovamente()
    {
        using var banco=new BancoDeHomologacao();var (repo,id,vm)=Preparar(banco);using var modelo=vm;
        vm.PorItensCommand.Execute(null);vm.Pagamento.ItensParaDividir[0].Selecionado=true;
        vm.SelecionarFormaCommand.Execute(FormaPagamento.Pix);vm.RegistrarCommand.Execute(null);Assert.Equal(2000,vm.Pagamento.TotalPagoCentavos);
        using var novo=Abrir(repo,id);novo.PorItensCommand.Execute(null);var pago=novo.Pagamento.ItensParaDividir.Single(i=>!i.PodeSelecionar);
        pago.Selecionado=true;Assert.False(pago.Selecionado);Assert.Equal(1200,novo.Pagamento.FaltaPagarCentavos);
    }
    [Fact]
    public void SemPermissaoNaoGravaEConflitoDeSaldoReconsultaSemReenviar()
    {
        using var banco=new BancoDeHomologacao();var (repo,id,vm)=Preparar(banco);using var modelo=vm;
        vm.Pagamento.Autorizar=_=>false;vm.SelecionarFormaCommand.Execute(FormaPagamento.Pix);vm.RegistrarCommand.Execute(null);
        Assert.Empty(vm.Pagamento.Pagamentos);Assert.Contains("autorizado",vm.Pagamento.Mensagem);
        vm.Pagamento.Autorizar=null;repo.RegistrarPagamento(id,new() { Forma=FormaPagamento.Debito,ValorCentavos=1000,RecebidoCentavos=1000 });
        vm.Pagamento.ValorPagamentoTexto="32,00";vm.RegistrarCommand.Execute(null);
        Assert.Single(vm.Pagamento.Pagamentos);Assert.Equal(2200,vm.Pagamento.FaltaPagarCentavos);Assert.Contains("Saldo atualizado",vm.Pagamento.Mensagem);Assert.False(vm.RegistrarCommand.CanExecute(null));
    }
    [Fact]
    public void TelaDoPrototipoMantemAcoesVisiveisNosDoisTemasESemJanelaDeLancamento()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach(var escuro in new[] {false,true})foreach(var largura in new[] {820d,1120d})
            {
                using var banco=new BancoDeHomologacao();var (_,_,vm)=Preparar(banco);using var modelo=vm;TemaInterface.Aplicar(escuro,false);
                var view=new PagamentoFluxoPrototipoView(vm);var altura=largura==820?560d:650d;
                Capturar(view,largura,altura,$"inicial-{escuro}-{largura}");
                var finalizar=(Button)view.FindName("FinalizarPedido");var confirmar=(Button)view.FindName("ConfirmarRecebimento");
                Assert.False(finalizar.IsEnabled);Assert.False(confirmar.IsEnabled);Assert.False(confirmar.IsDefault);
                vm.SelecionarFormaCommand.Execute(FormaPagamento.Dinheiro);vm.Pagamento.ValorRecebidoTexto="50,00";
                Capturar(view,largura,altura,$"dinheiro-{escuro}-{largura}");Assert.True(confirmar.IsEnabled);
                var posicao=finalizar.TransformToAncestor(view).Transform(new Point(0,0));Assert.InRange(posicao.Y+finalizar.ActualHeight,1,altura);
                Assert.DoesNotContain(System.Windows.Application.Current.Windows.OfType<Window>(),w=>w.IsVisible);
                var confirmarPosicao=confirmar.TransformToAncestor(view).Transform(new Point(0,0));Assert.InRange(confirmarPosicao.Y+confirmar.ActualHeight,1,altura);
                Receber(vm,FormaPagamento.Pix,"10,00");Capturar(view,largura,altura,$"parcial-{escuro}-{largura}");
                Receber(vm,FormaPagamento.Debito,"22,00");Capturar(view,largura,altura,$"quitado-{escuro}-{largura}");Assert.True(finalizar.IsEnabled);
            }
            TemaInterface.Aplicar(false,false);
        });
    }
    [Fact]
    public void VariacaoGuiadaConfereAntesDeGravarEPermitePagamentoMisto()
    {
        using var banco=new BancoDeHomologacao();var (repo,id,vm)=Preparar(banco);using var fluxo=vm;using var guiado=new PagamentoGuiadoPrototipoViewModel(vm);
        Assert.True(guiado.EscolhendoForma);Assert.False(guiado.AcaoCommand.CanExecute(null));
        guiado.SelecionarFormaCommand.Execute(FormaPagamento.Pix);Assert.True(guiado.InformandoValor);Assert.Empty(vm.Pagamento.Pagamentos);
        vm.Pagamento.ValorPagamentoTexto="33,00";Assert.False(guiado.AcaoCommand.CanExecute(null));
        vm.Pagamento.ValorPagamentoTexto="10,00";guiado.AcaoCommand.Execute(null);Assert.True(guiado.Conferindo);Assert.Empty(vm.Pagamento.Pagamentos);
        guiado.VoltarEtapaCommand.Execute(null);Assert.True(guiado.InformandoValor);guiado.AcaoCommand.Execute(null);guiado.AcaoCommand.Execute(null);
        Assert.True(guiado.ParcelaConcluida);Assert.Single(vm.Pagamento.Pagamentos);Assert.Equal(2200,vm.Pagamento.FaltaPagarCentavos);
        guiado.AcaoCommand.Execute(null);Assert.True(guiado.EscolhendoForma);Assert.Single(vm.Pagamento.Pagamentos);
        guiado.SelecionarFormaCommand.Execute(FormaPagamento.Dinheiro);vm.Pagamento.ValorRecebidoTexto="50,00";
        guiado.AcaoCommand.Execute(null);Assert.True(guiado.Conferindo);Assert.Equal(2800,vm.Pagamento.TrocoCentavos);
        guiado.AcaoCommand.Execute(null);Assert.True(guiado.Quitado);Assert.Equal(2,vm.Pagamento.Pagamentos.Count);
        guiado.AcaoCommand.Execute(null);guiado.AcaoCommand.Execute(null);Assert.True(guiado.Encerrado);
        using var db=banco.Fabrica.CreateDbContext();Assert.Single(db.Vendas.ToArray());
    }
    [Fact]
    public void AlternarModelosPreservaRecebimentosECorrecaoRetornaAoInicioGuiado()
    {
        using var banco=new BancoDeHomologacao();var (_,_,vm)=Preparar(banco);using var fluxo=vm;
        Receber(vm,FormaPagamento.Pix,"32,00");using var guiado=new PagamentoGuiadoPrototipoViewModel(vm);Assert.True(guiado.Quitado);
        var registro=Assert.Single(vm.Pagamento.Pagamentos);vm.PedirRemocaoCommand.Execute(registro);Assert.False(guiado.AcaoCommand.CanExecute(null));
        vm.ConfirmarRemocaoCommand.Execute(null);Assert.True(guiado.EscolhendoForma);Assert.Equal(3200,vm.Pagamento.FaltaPagarCentavos);
    }
    [Fact]
    public void VariacaoGuiadaRenderizaEtapasNosDoisTemasETamanhos()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            foreach(var escuro in new[] {false,true})foreach(var largura in new[] {820d,1120d})
            {
                using var banco=new BancoDeHomologacao();var (_,_,vm)=Preparar(banco);using var fluxo=vm;using var guiado=new PagamentoGuiadoPrototipoViewModel(vm);
                TemaInterface.Aplicar(escuro,false);var view=new PagamentoGuiadoPrototipoView(guiado);var altura=largura==820?560d:650d;
                Capturar(view,largura,altura,$"b-forma-{escuro}-{largura}");
                guiado.SelecionarFormaCommand.Execute(FormaPagamento.Dinheiro);vm.Pagamento.ValorRecebidoTexto="50,00";Capturar(view,largura,altura,$"b-valor-{escuro}-{largura}");
                guiado.AcaoCommand.Execute(null);Capturar(view,largura,altura,$"b-conferencia-{escuro}-{largura}");
                var acao=(Button)view.FindName("AcaoPrincipal");var posicao=acao.TransformToAncestor(view).Transform(new Point(0,0));Assert.InRange(posicao.Y+acao.ActualHeight,1,altura);Assert.True(acao.IsEnabled);Assert.False(acao.IsDefault);
                guiado.AcaoCommand.Execute(null);Capturar(view,largura,altura,$"b-quitado-{escuro}-{largura}");
                guiado.AcaoCommand.Execute(null);Capturar(view,largura,altura,$"b-finalizado-{escuro}-{largura}");Assert.False(acao.IsEnabled);
            }
            TemaInterface.Aplicar(false,false);
        });
    }

    private static void Capturar(FrameworkElement view,double largura,double altura,string nome)
    {
        view.Measure(new Size(largura,altura));view.Arrange(new Rect(0,0,largura,altura));view.UpdateLayout();
        var pasta=Path.Combine(AppContext.BaseDirectory,"previas-pagamento-prototipo");Directory.CreateDirectory(pasta);
        var imagem=new RenderTargetBitmap((int)largura,(int)altura,96,96,PixelFormats.Pbgra32);imagem.Render(view);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(imagem));
        using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));png.Save(arquivo);
    }
}
