using System.IO;
using System.Text.Json;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Tests.Catalogo;
using VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Storage;
using VarthexComanda.Infrastructure.Tests.Homologacao;

namespace VarthexComanda.Desktop.Tests.Configuracao;
public sealed class PrimeiraConfiguracaoTests
{
    private static PrimeiraConfiguracaoService Servico(BancoDeHomologacao banco)=>new(banco.Fabrica,new EfConfiguracaoRepository(banco.Fabrica),banco.Paths);
    private static EstadoConfiguracao Pronto()=>EstadoConfiguracao.Exemplo();
    [Fact] public void InstalacaoVaziaAbreMasBaseExistenteNaoEhForcada()
    {
        using var banco=new BancoDeHomologacao();var s=Servico(banco);Assert.True(s.DeveAbrir());Assert.True(s.ConfiguracaoPendente);new EfConfiguracaoRepository(banco.Fabrica).Definir("estabelecimento.nome","Existente",DateTime.UtcNow);Assert.False(s.DeveAbrir());
    }
    [Fact] public void RetomaRascunhoSemGravarConfiguracoesOperacionais()
    {
        using var banco=new BancoDeHomologacao();var s=Servico(banco);var e=Pronto();e.Etapa=3;s.SalvarRascunho(e);var lido=s.Carregar();Assert.Equal(3,lido.Etapa);Assert.Equal(e.Nome,lido.Nome);Assert.Null(new EfConfiguracaoRepository(banco.Fabrica).ObterValor("estabelecimento.nome"));Assert.DoesNotContain("\"Senha\":",File.ReadAllText(s.ArquivoRascunho));
    }
    [Fact] public void ConclusaoPersisteProdutosUnidadesCupomQuantidadeEFormasSemDuplicar()
    {
        using var banco=new BancoDeHomologacao();var s=Servico(banco);var e=Pronto();s.SalvarRascunho(e);s.Concluir(e);s.Concluir(e);
        using var db=banco.Fabrica.CreateDbContext();Assert.Equal(2,db.Produtos.Count());Assert.Single(db.Categorias);Assert.Equal(UnidadeVenda.Quilo,db.Produtos.Single(p=>p.Nome=="Refeição no quilo").UnidadeVenda);var r=new EfConfiguracaoRepository(banco.Fabrica);Assert.Equal(e.Nome,r.ObterValor("estabelecimento.nome"));Assert.Equal("30",r.ObterValor("comandas.quantidade_maxima"));Assert.Equal("1",r.ObterValor(PrimeiraConfiguracaoService.ChaveConclusao));Assert.False(s.DeveAbrir());Assert.False(s.ConfiguracaoPendente);Assert.False(File.Exists(s.ArquivoRascunho));Assert.Equal(e.Cupom.Documento,new ConfiguracoesSistemaService(r).Carregar().Documento);Assert.Equal(e.Pagamentos,JsonSerializer.Deserialize<List<string>>(r.ObterValor(PrimeiraConfiguracaoService.ChavePagamentos)!));
    }
    [Fact] public void ConflitoReverteProdutosCategoriaEConfiguracoesDaTentativa()
    {
        using var banco=new BancoDeHomologacao();banco.CriarProduto("Suco natural",999);var s=Servico(banco);var e=Pronto();Assert.Throws<InvalidOperationException>(()=>s.Concluir(e));using var db=banco.Fabrica.CreateDbContext();Assert.Single(db.Produtos);Assert.DoesNotContain(db.Categorias,c=>c.Nome=="Produtos");Assert.Null(new EfConfiguracaoRepository(banco.Fabrica).ObterValor(PrimeiraConfiguracaoService.ChaveConclusao));Assert.Null(new EfConfiguracaoRepository(banco.Fabrica).ObterValor("estabelecimento.nome"));
    }
    [Fact] public void NaoReduzFaixaComPedidoAberto()
    {
        using var banco=new BancoDeHomologacao();new EfComandaRepository(banco.Fabrica).AbrirComanda(100,DateTime.UtcNow);var e=Pronto();Assert.Throws<InvalidOperationException>(()=>Servico(banco).Concluir(e));
    }
    [Fact] public void PreservaSegurancaEOutrasPreferenciasAoReabrir()
    {
        using var banco=new BancoDeHomologacao();var r=new EfConfiguracaoRepository(banco.Fabrica);new ConfiguracoesSistemaService(r).Salvar(new(){SolicitarSenha=true,CorFundo="#123456"},"Anterior",30,DateTime.UtcNow);var s=Servico(banco);var e=s.Carregar();Assert.True(e.Cupom.SolicitarSenha);e.Nome="Atualizado";s.Concluir(e);Assert.True(new ConfiguracoesSistemaService(r).Carregar().SolicitarSenha);Assert.Equal("#123456",new ConfiguracoesSistemaService(r).Carregar().CorFundo);
    }
    [Fact] public void DocumentoIncompletoNaoMarcaConclusao()
    {
        using var banco=new BancoDeHomologacao();var e=Pronto();e.Cupom.Documento="123";Assert.Throws<ArgumentException>(()=>Servico(banco).Concluir(e));Assert.Null(new EfConfiguracaoRepository(banco.Fabrica).ObterValor(PrimeiraConfiguracaoService.ChaveConclusao));
    }
    [Fact] public void RascunhoAntigoNaoDesativaSegurancaNemMudaAparencia()
    {
        using var banco=new BancoDeHomologacao();var s=Servico(banco);var e=Pronto();e.Cupom.SolicitarSenha=false;e.Cupom.CorFundo="#FFFFFF";var r=new EfConfiguracaoRepository(banco.Fabrica);new ConfiguracoesSistemaService(r).Salvar(new(){SolicitarSenha=true,CorFundo="#123456"},"Atual",30,DateTime.UtcNow);s.Concluir(e);var salvo=new ConfiguracoesSistemaService(r).Carregar();Assert.True(salvo.SolicitarSenha);Assert.Equal("#123456",salvo.CorFundo);
    }
    [Fact] public void ComandoDePagamentoRespeitaFormasHabilitadas()
    {
        using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var c=repo.AbrirComanda(1,DateTime.UtcNow);repo.AdicionarItem(c.Id,banco.CriarProduto("Teste",1000),1,DateTime.UtcNow);var vm=new EncerramentoViewModel(new EncerrarComanda(repo,new FakeClock()),repo);vm.Carregar(c.Id);vm.DefinirFormasPermitidas([FormaPagamento.Pix]);vm.SelecionarFormaCommand.Execute(FormaPagamento.Dinheiro);Assert.Null(vm.FormaSelecionada);vm.SelecionarFormaCommand.Execute(FormaPagamento.Pix);Assert.Equal(FormaPagamento.Pix,vm.FormaSelecionada);
    }
}
