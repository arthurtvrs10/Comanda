using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.Atendimento;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class MenuComandaTests
{
    [Fact]
    public void BloqueioImpedeLancamentosPorComandoEPagamentoContinuaDisponivel()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (vm, _) = CarregamentoDeXamlTests.CriarAtendimento();
            vm.ComandaAtual!.Bloqueada = true;
            vm.AdicionarProdutoAoItemCommand.Execute(vm.ProdutosCatalogo.First());
            vm.AumentarQuantidadeCommand.Execute(vm.Itens.First());
            vm.DiminuirQuantidadeCommand.Execute(vm.Itens.First());
            vm.RemoverCommand.Execute(vm.Itens.First());
            Assert.Equal(2, vm.Itens.Single().Quantidade);
            Assert.Contains("bloqueado", vm.Mensagem);
            Assert.True(vm.VerTotalCommand.CanExecute(null));
        });
    }

    [Fact]
    public void MenusOferecemAsAcoesEPendenciasSemMarcarImpressaoAoAbrir()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var (vm, _) = CarregamentoDeXamlTests.CriarAtendimento();
            var view = new AtendimentoView(vm);
            var botao = (Button)view.FindName("BotaoMaisOpcoes");
            var mais = view.CriarMenuMaisOpcoes(botao);
            Assert.Contains(mais.Items.OfType<MenuItem>(), i => i.Header.ToString()!.StartsWith("Transferir / copiar"));
            Assert.Contains(mais.Items.OfType<MenuItem>(), i => i.Header.ToString()!.StartsWith("Imprimir fichas"));
            Assert.Contains(mais.Items.OfType<MenuItem>(), i => i.Header.Equals("Enviar para WhatsApp"));
            var imprimir = view.CriarMenuImprimir(botao);
            Assert.Contains(imprimir.Items.OfType<MenuItem>(), i => i.Header.Equals("Imprimir conta e bloquear"));
            Assert.Contains(imprimir.Items.OfType<MenuItem>(), i => i.Header.ToString()!.StartsWith("Imprimir na cozinha (2"));
            Assert.Equal(0, vm.Itens.Single().QuantidadeCozinha);
            vm.Itens.Single().QuantidadeCozinha = 2;
            imprimir = view.CriarMenuImprimir(botao);
            Assert.False(Assert.Single(imprimir.Items.OfType<MenuItem>(), i => i.Header.ToString()!.StartsWith("Não há itens novos")).IsEnabled);
            vm.ComandaAtual!.Bloqueada = true;
            mais = view.CriarMenuMaisOpcoes(botao);
            Assert.False(Assert.Single(mais.Items.OfType<MenuItem>(), i => i.Header.ToString()!.StartsWith("Transferir / copiar")).IsEnabled);
        });
    }

    [Fact]
    public void TelaEMenusMantemLayoutNosTemasETamanhosComVazioBloqueadoECatalogo()
    {
        ThreadingHelper.EmSta(() =>
        {
            CarregamentoDeXamlTests.GarantirApp();
            try
            {
                foreach (var escuro in new[] { false, true })
                {
                    TemaInterface.Aplicar(escuro, false);
                    foreach (var largura in new[] { 800d, 1180d })
                    {
                        var (vm, _) = CarregamentoDeXamlTests.CriarAtendimento();
                        vm.ComandaAtual!.NomeCliente = "Cliente de exemplo";
                        vm.ComandaAtual.CriadoPor = "Ana";
                        vm.ComandaAtual.Responsavel = "Ana";
                        vm.ComandaAtual.NumeroMesa = 12;
                        vm.ComandaAtual.Observacao = "Pedido de exemplo: entregar os itens juntos.";
                        vm.Itens[0].NomeProduto = "Almoço executivo com acompanhamento e salada da casa";
                        vm.Itens[0].LancadoPor = "Ana";
                        vm.Itens[0].Observacao = "Sem gelo. Entregar junto com os lanches.";
                        var view = new AtendimentoView(vm);
                        var nome = $"comanda-{(escuro ? "escuro" : "claro")}-{largura}";
                        var altura = largura == 800 ? 548 : 688;
                        Renderizar(view, largura, altura, nome);
                        var pagar = AlinhamentoInterfaceTests.Filhos(view).OfType<Button>().Single(b => b.Command == vm.VerTotalCommand);
                        Assert.True(pagar.ActualWidth >= 170);
                        Assert.True(pagar.TranslatePoint(new Point(0,pagar.ActualHeight),view).Y <= altura);
                        Assert.Equal(Visibility.Collapsed, ((FrameworkElement)view.FindName("PainelMenu")).Visibility);
                        if (largura == 800)
                        {
                            ((Button)view.FindName("BotaoProdutosPedido")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            Renderizar(view, largura, altura, nome + "-produtos");
                            ((FrameworkElement)view.FindName("PainelMenu")).Visibility = Visibility.Collapsed;
                            vm.ComandaAtual = new VarthexComanda.Domain.Comanda { Id=vm.ComandaAtual.Id,Numero=vm.ComandaAtual.Numero,Status=vm.ComandaAtual.Status,AbertaEm=vm.ComandaAtual.AbertaEm,TotalCentavos=1000,Bloqueada=true };
                            Renderizar(view, largura, altura, nome + "-bloqueado");
                            var botao = (Button)view.FindName("BotaoMaisOpcoes");
                            CapturarMenu(view.CriarMenuMaisOpcoes(botao), $"menu-opcoes-{(escuro ? "escuro" : "claro")}");
                            CapturarMenu(view.CriarMenuImprimir(botao), $"menu-imprimir-{(escuro ? "escuro" : "claro")}");
                            vm.Itens.Clear();vm.ComandaAtual.TotalCentavos=0;vm.VerTotalCommand.NotifyCanExecuteChanged();vm.NotificarItensPedido();
                            Renderizar(view, largura, altura, nome + "-vazio");
                        }
                    }
                }
            }
            finally { TemaInterface.Aplicar(false, false); }
        });
    }

    private static void CapturarMenu(ContextMenu menu, string nome)
    {
        menu.ApplyTemplate();menu.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
        Renderizar(menu, Math.Ceiling(menu.DesiredSize.Width), Math.Ceiling(menu.DesiredSize.Height), nome);
    }
    private static void Renderizar(FrameworkElement view, double largura, double altura, string nome)
    {
        view.Measure(new Size(largura,altura));view.Arrange(new Rect(0,0,largura,altura));view.UpdateLayout();
        var pasta = Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");
        if (string.IsNullOrWhiteSpace(pasta)) return;
        Directory.CreateDirectory(pasta);
        var imagem = new RenderTargetBitmap((int)largura,(int)altura,96,96,PixelFormats.Pbgra32);imagem.Render(view);
        var png = new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(imagem));
        using var arquivo = File.Create(Path.Combine(pasta,nome+".png"));png.Save(arquivo);
    }
}
