using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Atendimento;
using VarthexComanda.Application.Tests.Catalogo;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Atendimento;

namespace VarthexComanda.Desktop.Tests.Atendimento;
public class NumeracaoGradeComandasTests
{
    [Fact]
    public void GradeComecaEm100PreservaAbertasForaDaFaixaEAberturaEMovimentoUsamFaixa()
    {
        var repo=new FakeConfiguracaoRepository();repo.Definir("comandas.quantidade_maxima","20",DateTime.UtcNow);
        var comandas=new FakeComandaRepository();var relogio=new FakeClock();comandas.AbrirComanda(3,relogio.UtcNow);
        new NumeracaoComandasService(repo).Salvar(new(){Ativa=true,Inicio=100},DateTime.UtcNow);
        var obter=new ObterConfiguracao(repo);var produtos=new FakeProdutoRepository();var categorias=new FakeCategoriaRepository();
        var vm=new AtendimentoViewModel(new AbrirComanda(comandas,relogio,obter),new AdicionarItem(comandas,produtos,relogio),new AlterarQuantidade(comandas,relogio),new RemoverItem(comandas,relogio),new CancelarComanda(comandas,relogio),comandas,new ListarCategoriasAtivas(categorias),new PesquisarProdutos(produtos),new FakeConfirmador(),new FakeEncerramentoDialog(comandas,relogio),obter,relogio);
        Assert.Equal(Enumerable.Range(100,20),vm.Slots.Take(20).Select(s=>s.Numero));Assert.True(vm.Slots.Single(s=>s.Numero==3).Aberta);
        vm.AbrirOuSelecionarSlotCommand.Execute(vm.Slots.Single(s=>s.Numero==100));Assert.Equal(100,vm.ComandaAtual!.Numero);
        Assert.True(vm.SalvarDadosComanda("mover","119"));Assert.Equal(119,vm.ComandaAtual!.Numero);Assert.False(vm.SalvarDadosComanda("mover","120"));Assert.False(vm.SalvarDadosComanda("mover","99"));
    }
}
