using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Documents;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Xaml;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Configuracao;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using VarthexComanda.Infrastructure.Persistence.Catalogo;
using VarthexComanda.Infrastructure.Tests.Homologacao;
using VarthexComanda.Servidor;

namespace VarthexComanda.Desktop.Tests.Homologacao;
public class FluxoOperacionalCompletoTests
{
    private static void Esperar(Task tarefa)
    {
        var dispatcher=System.Windows.Threading.Dispatcher.CurrentDispatcher;
        var frame=new System.Windows.Threading.DispatcherFrame();
        tarefa.ContinueWith(_=>dispatcher.BeginInvoke(new Action(()=>frame.Continue=false)));
        System.Windows.Threading.Dispatcher.PushFrame(frame);tarefa.GetAwaiter().GetResult();
    }
    [Fact]
    public void CelularKdsChamadasEntregaPagamentoMistoHistoricoEImpressosUsamMesmoBanco()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            using var banco=new BancoDeHomologacao();var repo=new EfComandaRepository(banco.Fabrica);var config=new EfConfiguracaoRepository(banco.Fabrica);var produtos=new EfProdutoRepository(banco.Fabrica);var clock=new RelogioFixo(DateTime.UtcNow);var obter=new ObterConfiguracao(config);var produto=banco.CriarProduto("Lanche de teste",2500);
            var remoto=new AtendimentoRemoto(repo,produtos,new EfCategoriaRepository(banco.Fabrica),new(repo,clock,obter),new(repo,produtos,clock),obter,configuracoes:config);using var servidor=new ServidorLocal(remoto);servidor.IniciarAsync(0,true).GetAwaiter().GetResult();
            using var celular=new HttpClient {BaseAddress=new Uri($"http://127.0.0.1:{servidor.Porta}")};celular.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",servidor.LinkAcesso(celular.BaseAddress.ToString().TrimEnd('/')).Split("#chave=")[1]);
            HttpResponseMessage Enviar(string rota,object body){var r=new HttpRequestMessage(HttpMethod.Post,rota){Content=JsonContent.Create(body)};r.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());return celular.SendAsync(r).GetAwaiter().GetResult();}
            var abertura=Enviar("/api/comandas",new AberturaRemota(1));Assert.Equal(HttpStatusCode.Created,abertura.StatusCode);var id=JsonDocument.Parse(abertura.Content.ReadAsStringAsync().GetAwaiter().GetResult()).RootElement.GetProperty("id").GetInt32();repo.DefinirMesa(id,12);
            Assert.Equal(HttpStatusCode.OK,Enviar($"/api/comandas/{id}/itens",new LancamentoRemoto(produto.Id,2,"Sem cebola","Atendente de teste")).StatusCode);
            var cozinhaRepo=new EfCozinhaRepository(banco.Fabrica);var consultar=new ConsultarFilaCozinha(cozinhaRepo);var preparo=new AtualizarPreparo(cozinhaRepo,clock);var kds=new CozinhaViewModel(consultar.Quadro,preparo.Executar){PreferenciasQuadro=new(config)};kds.Atualizar();Assert.Single(kds.Colunas[1].Pedidos);
            Assert.Equal(HttpStatusCode.OK,Enviar($"/api/comandas/{id}/chamadas",new PedidoChamada("Trazer guardanapos",Guid.NewGuid())).StatusCode);kds.Atualizar();var chamada=Assert.Single(kds.Colunas[0].Chamadas);Assert.Equal(12,chamada.Mesa);Assert.Equal("Trazer guardanapos",chamada.Solicitacao);kds.AtenderChamada(chamada);Assert.Empty(new ChamadasMesasService(config).Listar());
            foreach(var destino in new[]{2,3,4}){var cartao=kds.Colunas[destino-1].Pedidos.Single();Esperar(kds.MoverAsync(cartao,kds.Colunas[destino]));Assert.False(kds.Falha);Assert.Single(kds.Colunas[destino].Pedidos);}
            Assert.Equal(StatusComanda.Aberta,repo.BuscarComItens(id)!.Comanda.Status);
            EncerramentoViewModel AbrirPagamento(){var vm=new EncerramentoViewModel(new(repo,clock),repo);vm.Carregar(id);return vm;}
            var pagamento=AbrirPagamento();pagamento.FormaSelecionada=FormaPagamento.Pix;pagamento.ValorPagamentoTexto="20,00";pagamento.PagamentoRecebido=true;pagamento.RegistrarPagamentoCommand.Execute(null);Assert.Equal(3000,pagamento.FaltaPagarCentavos);Assert.False(pagamento.ConfirmarEncerrarCommand.CanExecute(null));
            pagamento=AbrirPagamento();Assert.Equal(2000,pagamento.TotalPagoCentavos);pagamento.FormaSelecionada=FormaPagamento.Dinheiro;pagamento.ValorPagamentoTexto="30,00";pagamento.ValorRecebidoTexto="50,00";pagamento.PagamentoRecebido=true;Assert.Equal(2000,pagamento.TrocoCentavos);pagamento.RegistrarPagamentoCommand.Execute(null);Assert.True(pagamento.ConfirmarEncerrarCommand.CanExecute(null));pagamento.ConfirmarEncerrarCommand.Execute(null);
            using var db=banco.Fabrica.CreateDbContext();var venda=Assert.Single(db.Vendas.ToArray());Assert.Equal(5000,venda.TotalCentavos);Assert.Equal(2,PagamentosConta.Ler(venda.PagamentosJson).Count);Assert.Empty(new EfComandaRepository(banco.Fabrica).ListarAbertas());
            var detalhe=repo.BuscarComItens(id)!;foreach(var mm in new[]{58,80}){var doc=CupomTermico.Conta(new(),"Restaurante de teste",detalhe.Comanda,detalhe.Itens,mm,layout:new());var texto=new TextRange(doc.ContentStart,doc.ContentEnd).Text;Assert.Contains("MESA 12",texto);Assert.Contains("R$ 50,00",texto);Assert.Contains("Pix",texto);Assert.Contains("Dinheiro",texto);Assert.Contains("R$ 20,00",texto);Assert.Equal(CupomTermico.Largura(mm),doc.PageWidth);}
        });
    }
}
