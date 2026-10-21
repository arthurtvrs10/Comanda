using System.Windows.Controls;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Atendimento;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Tests.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class OperacoesComandaRegressaoTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LancamentoSelecionaAExataLinhaIncrementadaAposTransferirOuCopiar(bool copiar)
    {
        using var banco = new BancoDeHomologacao();
        var produto = banco.CriarProduto("Refrigerante", 500);
        var repo = new EfComandaRepository(banco.Fabrica);
        var origem = repo.AbrirComanda(1, DateTime.UtcNow);
        var destino = repo.AbrirComanda(2, DateTime.UtcNow);
        var original = repo.AdicionarItem(destino.Id, produto, 1, DateTime.UtcNow, null, "Ana").Itens.Single();
        var outro = repo.AdicionarItem(origem.Id, produto, 2, DateTime.UtcNow, null, "Ana").Itens.Single();
        repo.TransferirItens(origem.Id, destino.Id, [outro.Id], copiar, DateTime.UtcNow);
        repo.AdicionarItem(destino.Id, produto, 1, DateTime.UtcNow, null, "Bruno");
        var vm = Criar(banco, repo);
        vm.ObterOperador = () => "Ana";
        vm.SelecionarComandaCommand.Execute(destino);
        vm.AdicionarProdutoAoItemCommand.Execute(produto);
        Assert.Equal(original.Id, vm.ItemSelecionadoId);
        Assert.Equal(2, vm.Itens.Single(i => i.Id == original.Id).Quantidade);
        vm.AumentarSelecionadoCommand.Execute(null);
        Assert.Equal(3, repo.BuscarComItens(destino.Id)!.Itens.Single(i => i.Id == original.Id).Quantidade);
        Assert.Equal(2, vm.Itens.Single(i => i.Id != original.Id && i.LancadoPor == "Ana").Quantidade);
        Assert.Equal(1, vm.Itens.Single(i => i.LancadoPor == "Bruno").Quantidade);
    }

    [Fact]
    public void FalhaAoSalvarObservacaoPreservaTextoPedidoEImpedeVoltarEPagar()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            // O repositório de teste legado recusa persistir dados do atendimento.
            var (vm, comanda) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(vm);
            var campo = (TextBox)view.FindName("ObservacaoPedido");
            campo.Text = "Observação que precisa ser preservada";
            var encerramentos = 0;
            vm.AtendimentoConcluido += () => encerramentos++;
            vm.FecharEdicaoCommand.Execute(null);
            Assert.Equal(comanda.Id, vm.ComandaAtual?.Id);
            Assert.Equal("Observação que precisa ser preservada", campo.Text);
            Assert.NotEmpty(vm.Mensagem);
            Assert.Single(vm.Itens);
            vm.VerTotalCommand.Execute(null);
            Assert.Equal(comanda.Id, vm.ComandaAtual?.Id);
            Assert.Equal(0, encerramentos);
            // O operador pode corrigir o texto e sair; a falha não desativa a navegação.
            campo.Text = comanda.Observacao ?? "";
            vm.FecharEdicaoCommand.Execute(null);
            Assert.Null(vm.ComandaAtual);
            Assert.Equal(1, encerramentos);
        });
    }

    [Fact]
    public void VoltarSalvaObservacaoPendenteAntesDeLimparPedido()
    {
        using var banco = new BancoDeHomologacao();
        var repo = new EfComandaRepository(banco.Fabrica);
        var comanda = repo.AbrirComanda(1, DateTime.UtcNow);
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var vm = Criar(banco, repo);
            vm.SelecionarComandaCommand.Execute(comanda);
            var view = new AtendimentoView(vm);
            ((TextBox)view.FindName("ObservacaoPedido")).Text = "Entregar os itens juntos";
            vm.FecharEdicaoCommand.Execute(null);
            Assert.Null(vm.ComandaAtual);
            Assert.Equal("Entregar os itens juntos", new EfComandaRepository(banco.Fabrica).BuscarComItens(comanda.Id)!.Comanda.Observacao);
        });
    }

    internal static AtendimentoViewModel Criar(BancoDeHomologacao banco, IComandaRepository repo)
    {
        var clock = new RelogioFixo();
        var produtos = new EfProdutoRepository(banco.Fabrica);
        var config = new ObterConfiguracao(new FakeConfiguracaoRepository());
        return new(new AbrirComanda(repo, clock, config), new AdicionarItem(repo, produtos, clock),
            new AlterarQuantidade(repo, clock), new RemoverItem(repo, clock), new CancelarComanda(repo, clock),
            repo, new ListarCategoriasAtivas(new EfCategoriaRepository(banco.Fabrica)), new PesquisarProdutos(produtos),
            new FakeConfirmador(), new FakeEncerramentoDialog(repo, clock), config, clock);
    }
}
