using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Tests.Atendimento;
using VarthexComanda.Application.Tests.Catalogo;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class RecebimentoViewTests
{
    [Fact]
    public void LancamentoExigeRecebimentoSalvaTrocoObservacaoEAtualizaResumo()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var repo=new FakeComandaRepository();var relogio=new FakeClock();
            var produto=new FakeProdutoRepository().Salvar(new Produto {Id=0,CategoriaId=1,Nome="Lanche",PrecoCentavos=5000,Ativo=true,CriadoEm=relogio.UtcNow,AtualizadoEm=relogio.UtcNow});
            var c=repo.AbrirComanda(7,relogio.UtcNow);repo.AdicionarItem(c.Id,produto,1,relogio.UtcNow);
            var vm=new EncerramentoViewModel(new EncerrarComanda(repo,relogio),repo);vm.Carregar(c.Id);vm.FormaSelecionada=FormaPagamento.Dinheiro;
            using var tema=new RestaurarTema();
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);var editor=new RecebimentoView(vm);
                try
                {
                    var salvar=(Button)editor.FindName("Salvar");Assert.False(salvar.IsEnabled);
                    vm.ValorPagamentoTexto="25,00";vm.ValorRecebidoTexto="50,00";vm.ObservacaoPagamento="Primeira pessoa";vm.PagamentoRecebido=true;
                    Assert.Equal(2500,vm.TrocoCentavos);Assert.True(salvar.IsEnabled);
                    Capturar((FrameworkElement)editor.Content,$"recebimento-{escuro}",450,660);
                    salvar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.Equal(2500,vm.TotalPagoCentavos);Assert.Equal(50,vm.ProgressoPagamento);Assert.False(vm.ContaQuitada);
                    Assert.Equal("Primeira pessoa",Assert.Single(vm.Pagamentos).Observacao);
                    var janela=new EncerramentoView(vm,_=>{});
                    try{Capturar((FrameworkElement)janela.Content,$"pagamento-parcial-{escuro}",1120,700);}
                    finally{janela.Close();}
                    vm.RemoverRegistro(Assert.Single(vm.Pagamentos));
                }
                finally{editor.Close();}
            }
            var cancelado=new RecebimentoView(vm);vm.ValorPagamentoTexto="50,00";vm.PagamentoRecebido=true;cancelado.Close();
            Assert.Empty(vm.Pagamentos);Assert.False(vm.PagamentoRecebido);
        });
    }
    private static void Capturar(FrameworkElement raiz,string nome,int largura,int altura)
    {
        raiz.Measure(new Size(largura,altura));raiz.Arrange(new Rect(0,0,largura,altura));raiz.UpdateLayout();
        var destino=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(destino is null)return;
        System.IO.Directory.CreateDirectory(destino);var bitmap=new RenderTargetBitmap(largura,altura,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);
        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=System.IO.File.Create(System.IO.Path.Combine(destino,nome+".png"));png.Save(arquivo);
    }
    private sealed class RestaurarTema:IDisposable{public void Dispose()=>TemaInterface.Aplicar(false,false);}
}
