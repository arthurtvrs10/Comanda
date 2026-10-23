using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Tests.Catalogo;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Homologacao;

public sealed class PagamentosMistosTests
{
    [Fact]
    public void RecebimentosPersistemEntreAberturasETotalMistoFechaUmaVenda()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var relogio=new FakeClock();
        var produto=banco.CriarProduto("Lanche",5000);var c=repo.AbrirComanda(7,relogio.UtcNow);repo.AdicionarItem(c.Id,produto,1,relogio.UtcNow);
        EncerramentoViewModel Abrir(){var vm=new EncerramentoViewModel(new EncerrarComanda(repo,relogio),repo);vm.Carregar(c.Id);return vm;}
        var vm=Abrir();vm.FormaSelecionada=FormaPagamento.Pix;vm.ValorPagamentoTexto="20,00";vm.ObservacaoPagamento="Primeira pessoa";vm.PagamentoRecebido=true;
        vm.RegistrarPagamentoCommand.Execute(null);Assert.Equal(2000,vm.TotalPagoCentavos);Assert.False(vm.ConfirmarEncerrarCommand.CanExecute(null));
        Assert.Throws<InvalidOperationException>(()=>repo.EncerrarComanda(c.Id,relogio.UtcNow));
        vm=Abrir();Assert.Equal(2000,vm.TotalPagoCentavos);Assert.Equal(3000,vm.FaltaPagarCentavos);
        vm.FormaSelecionada=FormaPagamento.Dinheiro;vm.ValorPagamentoTexto="30,00";vm.ValorRecebidoTexto="50,00";vm.PagamentoRecebido=true;
        Assert.Equal(2000,vm.TrocoCentavos);vm.RegistrarPagamentoCommand.Execute(null);
        Assert.Equal(0,vm.FaltaPagarCentavos);Assert.True(vm.ConfirmarEncerrarCommand.CanExecute(null));
        vm.ConfirmarEncerrarCommand.Execute(null);
        using var db=banco.Fabrica.CreateDbContext();var venda=Assert.Single(db.Vendas.ToArray());
        Assert.Equal(5000,venda.TotalCentavos);Assert.Equal(2,PagamentosConta.Ler(venda.PagamentosJson).Count);
        Assert.Equal("Pix + Dinheiro",PagamentosConta.Descrever(venda.PagamentosJson,venda.FormaPagamento));
        Assert.Contains(PagamentosConta.Ler(venda.PagamentosJson),p=>p.Observacao=="Primeira pessoa");
    }
    [Fact]
    public void RemocaoPreservaAuditoriaEPermiteCorrigirSemAlterarItens()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var p=banco.CriarProduto("Lanche",1000);var c=repo.AbrirComanda(1,DateTime.UtcNow);var item=Assert.Single(repo.AdicionarItem(c.Id,p,1,DateTime.UtcNow).Itens);
        var pagamento=new PagamentoRegistrado { Forma=FormaPagamento.Pix,ValorCentavos=500,RecebidoCentavos=500 };
        repo.RegistrarPagamento(c.Id,pagamento);repo.RegistrarPagamento(c.Id,pagamento);
        Assert.Equal(500,PagamentosConta.Total(repo.BuscarComItens(c.Id)!.Comanda.PagamentosJson));
        Assert.Throws<ArgumentException>(()=>repo.RegistrarPagamento(c.Id,new(){Forma=FormaPagamento.Pix,ValorCentavos=600,RecebidoCentavos=600}));
        Assert.Throws<InvalidOperationException>(()=>repo.RemoverItem(item.Id,DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(()=>repo.CancelarComanda(c.Id,DateTime.UtcNow));
        repo.RemoverRegistroPagamento(c.Id,pagamento.Id);
        var dados=repo.BuscarComItens(c.Id)!;Assert.Equal(1000,dados.Comanda.TotalCentavos);Assert.True(Assert.Single(PagamentosConta.Ler(dados.Comanda.PagamentosJson)).Cancelado);
        Assert.Equal(0,PagamentosConta.Total(dados.Comanda.PagamentosJson));
    }

    [Fact]
    public void DivisaoPorItensPersisteImpedeDuplicidadeELiberaAposRemocao()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var relogio=new FakeClock();
        var p=banco.CriarProduto("Lanche",1000);var bebida=banco.CriarProduto("Suco",600);
        var c=repo.AbrirComanda(3,relogio.UtcNow);repo.AdicionarItem(c.Id,p,1,relogio.UtcNow);repo.AdicionarItem(c.Id,bebida,1,relogio.UtcNow);
        EncerramentoViewModel Abrir(){var vm=new EncerramentoViewModel(new EncerrarComanda(repo,relogio),repo);vm.Carregar(c.Id);return vm;}
        var vm=Abrir();vm.SelecionarParaDividir=true;
        var selecionado=vm.ItensParaDividir.Single(i=>i.Item.ProdutoId==p.Id);selecionado.Selecionado=true;
        Assert.Equal(1000,vm.TotalSelecionadoCentavos);Assert.Equal("10,00",vm.ValorPagamentoTexto);
        vm.FormaSelecionada=FormaPagamento.Pix;vm.PagamentoRecebido=true;
        Assert.True(vm.RegistrarPagamentoCommand.CanExecute(null));Assert.False(vm.ConfirmarEncerrarCommand.CanExecute(null));
        vm.RegistrarPagamentoCommand.Execute(null);
        Assert.Equal(selecionado.Item.Id,Assert.Single(Assert.Single(vm.Pagamentos).ItensIds));
        vm=Abrir();var pago=vm.ItensParaDividir.Single(i=>i.Item.Id==selecionado.Item.Id);
        Assert.False(pago.PodeSelecionar);pago.Selecionado=true;Assert.False(pago.Selecionado);
        Assert.Throws<ArgumentException>(()=>repo.RegistrarPagamento(c.Id,new(){Forma=FormaPagamento.Pix,ValorCentavos=1000,RecebidoCentavos=1000,ItensIds=[pago.Item.Id]}));
        var registro=Assert.Single(vm.Pagamentos);vm.RemoverRegistro(registro);
        Assert.True(vm.ItensParaDividir.Single(i=>i.Item.Id==pago.Item.Id).PodeSelecionar);
        vm.SelecionarParaDividir=true;foreach(var item in vm.ItensParaDividir)item.Selecionado=true;
        Assert.Equal(1600,vm.TotalSelecionadoCentavos);
        vm.FormaSelecionada=FormaPagamento.Dinheiro;vm.PagamentoRecebido=true;vm.RegistrarPagamentoCommand.Execute(null);
        Assert.Equal(0,vm.FaltaPagarCentavos);Assert.True(vm.ConfirmarEncerrarCommand.CanExecute(null));
    }
    [Fact]
    public void PagamentoPorItensRejeitaValorIncorretoOuItemDeOutraConta()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var p=banco.CriarProduto("Lanche",1000);
        var c=repo.AbrirComanda(1,DateTime.UtcNow);var item=Assert.Single(repo.AdicionarItem(c.Id,p,1,DateTime.UtcNow).Itens);
        var outra=repo.AbrirComanda(2,DateTime.UtcNow);var alheio=Assert.Single(repo.AdicionarItem(outra.Id,p,1,DateTime.UtcNow).Itens);
        Assert.Throws<ArgumentException>(()=>repo.RegistrarPagamento(c.Id,new(){Forma=FormaPagamento.Pix,ValorCentavos=500,RecebidoCentavos=500,ItensIds=[item.Id]}));
        Assert.Throws<ArgumentException>(()=>repo.RegistrarPagamento(c.Id,new(){Forma=FormaPagamento.Pix,ValorCentavos=1000,RecebidoCentavos=1000,ItensIds=[alheio.Id]}));
        Assert.Equal(0,PagamentosConta.Total(repo.BuscarComItens(c.Id)!.Comanda.PagamentosJson));
    }
}
