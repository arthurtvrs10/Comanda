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

public sealed class PagamentoFluxoTests
{
    [Fact]
    public void ConferenciaImpressaoEConfirmacaoFuncionamNosDoisTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var repo=new FakeComandaRepository();var relogio=new FakeClock();
            var produto=new FakeProdutoRepository().Salvar(new Produto { Id=0,CategoriaId=1,Nome="Hambúrguer da casa",PrecoCentavos=2500,Ativo=true,CriadoEm=relogio.UtcNow,AtualizadoEm=relogio.UtcNow });
            var comanda=repo.AbrirComanda(7,relogio.UtcNow);repo.AdicionarItem(comanda.Id,produto,2,relogio.UtcNow);
            var vm=new EncerramentoViewModel(new EncerrarComanda(repo,relogio),repo);vm.Carregar(comanda.Id);
            var impressos=0;var janela=new EncerramentoView(vm,_=>impressos++);
            try
            {
                var raiz=(FrameworkElement)janela.Content;
                foreach(var escuro in new[]{false,true})
                foreach(var largura in new[]{1120,800})
                {
                    TemaInterface.Aplicar(escuro,false);
                    raiz.Measure(new Size(largura,700));raiz.Arrange(new Rect(0,0,largura,700));raiz.UpdateLayout();
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ContextIdle);
                    raiz.UpdateLayout();
                    var destino=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                    if(destino is not null)
                    {
                        System.IO.Directory.CreateDirectory(destino);
                        var bitmap=new RenderTargetBitmap(largura,700,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);
                        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));
                        using var arquivo=System.IO.File.Create(System.IO.Path.Combine(destino,$"pagamento-{escuro}-{largura}.png"));png.Save(arquivo);
                    }
                }
                var conferir=(CheckBox)janela.FindName("ConferirItens");
                Assert.False(conferir.IsChecked);
                conferir.IsChecked=false;Assert.Equal(Visibility.Collapsed,((FrameworkElement)janela.FindName("PainelItens")).Visibility);
                conferir.IsChecked=true;Assert.Equal(Visibility.Visible,((FrameworkElement)janela.FindName("PainelItens")).Visibility);
                ((CheckBox)janela.FindName("SelecionarItens")).IsChecked=true;
                vm.ItensParaDividir[0].Selecionado=true;
                Assert.Equal(5000,vm.TotalSelecionadoCentavos);
                raiz.Measure(new Size(1120,700));raiz.Arrange(new Rect(0,0,1120,700));raiz.UpdateLayout();
                var previa=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
                if(previa is not null)
                {
                    var bitmap=new RenderTargetBitmap(1120,700,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);
                    var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));
                    using var arquivo=System.IO.File.Create(System.IO.Path.Combine(previa,"pagamento-selecao-itens.png"));png.Save(arquivo);
                }
                conferir.IsChecked=false;Assert.False(vm.SelecionarParaDividir);
                Assert.Equal(0,vm.TotalSelecionadoCentavos);
                ((Button)janela.FindName("BotaoImprimir")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.Equal(1,impressos);
                vm.QuantidadePessoasTexto="3";Assert.Contains("16,67",vm.DivisaoDescricao);Assert.Contains("16,66",vm.DivisaoDescricao);
                Assert.False(vm.ConfirmarEncerrarCommand.CanExecute(null));
                ((RadioButton)janela.FindName("FormaPix")).IsChecked=true;Assert.Equal(FormaPagamento.Pix,vm.FormaSelecionada);
                vm.PagamentoRecebido=true;Assert.True(vm.ConfirmarEncerrarCommand.CanExecute(null));Assert.Equal("Pronto para concluir",vm.SituacaoPagamento);
                ((RadioButton)janela.FindName("FormaCredito")).IsChecked=true;Assert.False(vm.PagamentoRecebido);Assert.False(vm.ConfirmarEncerrarCommand.CanExecute(null));
                Assert.Equal(StatusComanda.Aberta,repo.BuscarComItens(comanda.Id)!.Comanda.Status);
            }
            finally{janela.Close();TemaInterface.Aplicar(false,false);}
        });
    }
}
