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
    public async Task FechamentoLentoNaoBloqueiaRetornoNaoRecriaItensEReverteEmFalha(bool falhar)
    {
        using var banco=new BancoDeHomologacao();var produto=banco.CriarProduto("Refrigerante",500);
        var real=new EfComandaRepository(banco.Fabrica);var comanda=real.AbrirComanda(1,DateTime.UtcNow);
        real.AdicionarItem(comanda.Id,produto,2,DateTime.UtcNow);
        var repo=System.Reflection.DispatchProxy.Create<IComandaRepository,RepositorioFechamentoControlado>();
        var controle=(RepositorioFechamentoControlado)(object)repo;controle.Real=real;controle.Falhar=falhar;
        using var iniciou=controle.Iniciou;using var liberar=controle.Liberar;
        var vm=Criar(banco,repo);vm.SelecionarComandaCommand.Execute(comanda);
        var item=vm.Itens.Single();var slot=vm.Slots.Single(s=>s.ComandaId==comanda.Id);
        controle.Consultas=0;
        var operacao=vm.DefinirFechamentoAsync(true);
        try
        {
            Assert.True(iniciou.Wait(TimeSpan.FromSeconds(3)));
            Assert.False(operacao.IsCompleted);Assert.True(vm.SalvandoFechamento);Assert.False(vm.PodeInteragirPedido);
            Assert.False(vm.VerTotalCommand.CanExecute(null));Assert.False(await vm.DefinirFechamentoAsync(false));
            vm.FecharEdicaoCommand.Execute(null);Assert.NotNull(vm.ComandaAtual);
            Assert.False(vm.AdicionarProduto(produto,1,null));Assert.Equal(1,controle.Gravacoes);
        }
        finally { liberar.Set(); }
        Assert.Equal(!falhar,await operacao);Assert.False(vm.SalvandoFechamento);Assert.True(vm.PodeInteragirPedido);
        Assert.Same(item,vm.Itens.Single());Assert.Same(slot,vm.Slots.Single(s=>s.ComandaId==comanda.Id));
        Assert.Equal(0,controle.Consultas);Assert.Equal(!falhar,real.BuscarComItens(comanda.Id)!.Comanda.Bloqueada);
        Assert.Equal(falhar?"Em aberto":"Em fechamento",vm.SituacaoPedido);
        Assert.True(vm.VerTotalCommand.CanExecute(null));
        if(falhar)Assert.Contains("Tente novamente",vm.Mensagem);
    }

    public class RepositorioFechamentoControlado : System.Reflection.DispatchProxy
    {
        public IComandaRepository Real { get; set; } = null!;
        public bool Falhar { get; set; }
        public int Consultas,Gravacoes;
        public ManualResetEventSlim Iniciou { get; }=new();
        public ManualResetEventSlim Liberar { get; }=new();
        protected override object? Invoke(System.Reflection.MethodInfo? metodo,object?[]? argumentos)
        {
            if(metodo!.Name is nameof(IComandaRepository.BuscarComItens) or nameof(IComandaRepository.ListarAbertas))Interlocked.Increment(ref Consultas);
            if(metodo.Name==nameof(IComandaRepository.DefinirBloqueio))
            {
                Interlocked.Increment(ref Gravacoes);Iniciou.Set();
                if(!Liberar.Wait(TimeSpan.FromSeconds(5)))throw new TimeoutException();
                if(Falhar)throw new InvalidOperationException("Falha controlada de gravação.");
            }
            return metodo.Invoke(Real,argumentos);
        }
    }

    [Fact]
    public void FechamentoPersistidoAtualizaSituacaoEGradeEPermiteRetomarAlteracoes()
    {
        using var banco=new BancoDeHomologacao();var produto=banco.CriarProduto("Refrigerante",500);
        var repo=new EfComandaRepository(banco.Fabrica);var comanda=repo.AbrirComanda(1,DateTime.UtcNow);
        var vm=Criar(banco,repo);vm.SelecionarComandaCommand.Execute(comanda);
        Assert.True(vm.DefinirBloqueio(true));Assert.Equal("Em fechamento",vm.SituacaoPedido);
        Assert.True(repo.BuscarComItens(comanda.Id)!.Comanda.Bloqueada);
        var slot=vm.Slots.Single(s=>s.ComandaId==comanda.Id);Assert.True(slot.EmFechamento);Assert.Equal("Em fechamento",slot.SituacaoTexto);
        Assert.False(vm.AdicionarProduto(produto,1,null));Assert.Empty(repo.BuscarComItens(comanda.Id)!.Itens);
        Assert.True(vm.DefinirBloqueio(false));Assert.Equal("Em aberto",vm.SituacaoPedido);
        Assert.False(vm.Slots.Single(s=>s.ComandaId==comanda.Id).EmFechamento);
        Assert.True(vm.AdicionarProduto(produto,1,null));Assert.Single(repo.BuscarComItens(comanda.Id)!.Itens);
    }

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
