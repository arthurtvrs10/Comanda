using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Application.Catalogo;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Desktop.Tests.Atendimento;
using VarthexComanda.Domain;

namespace VarthexComanda.Desktop.Tests.Xaml;

public class CozinhaKdsXamlTests
{
    [Fact]
    public void PainelRealMostraEstadosObservacoesAcoesERolagemNosDoisTemas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var dados=new[]{
                CozinhaViewModelTests.Pedido(1,2,CozinhaViewModelTests.Item(1,"Batata rústica com cheddar e bacon",minutos:24),CozinhaViewModelTests.Item(2,"Hambúrguer artesanal",StatusPreparo.EmPreparo,minutos:24)),
                CozinhaViewModelTests.Pedido(2,3,CozinhaViewModelTests.Item(3,"Filé ao molho de gorgonzola",StatusPreparo.Pronto,minutos:14),CozinhaViewModelTests.Item(4,"Suco natural de laranja",StatusPreparo.Pronto,minutos:14)),
                CozinhaViewModelTests.Pedido(3,null,Enumerable.Range(5,12).Select(i=>CozinhaViewModelTests.Item(i,$"Porção para compartilhar {i}",minutos:7)).ToArray()),
                CozinhaViewModelTests.Pedido(4,8,CozinhaViewModelTests.Item(20,"Almoço por quilo",minutos:2))};
            dados[0].Comanda.NomeCliente="Arthur";dados[0].Itens[0].Observacao="Sem cebola. Entregar junto com os lanches.";
            dados[0].Comanda.Observacao="Cliente aguardando no balcão. Conferir todos os itens antes de entregar.";
            dados[1].Comanda.NomeCliente="Ana";dados[1].Comanda.Status=StatusComanda.Fechada;
            dados[3].Itens[0].Quantidade=.350m;dados[3].Itens[0].UnidadeVenda=UnidadeVenda.Quilo;
            try
            {
                foreach(var escuro in new[]{false,true})
                foreach(var tamanho in new[]{new Size(1280,780),new Size(800,600),new Size(1900,900)})
                {
                    TemaInterface.Aplicar(escuro,false);
                    var view=new CozinhaView(()=>dados,(_,_)=>Resultado<ConfirmacaoPreparo>.Ok(new(1)));view.ViewModel.Agora=()=>CozinhaViewModelTests.Agora;view.Atualizar();
                    Medir(view,tamanho);
                    var textos=AlinhamentoInterfaceTests.Filhos(view).OfType<TextBlock>().Select(t=>t.Text).ToArray();
                    Assert.Contains("Cozinha (KDS)",textos);Assert.Contains("Pendente",textos);Assert.Contains("Em preparo",textos);Assert.Contains("Pronto",textos);
                    Assert.Contains("Sem cebola. Entregar junto com os lanches.",textos);Assert.DoesNotContain(textos,t=>t.Contains("R$"));
                    Assert.Contains(dados[0].Comanda.Observacao,textos);
                    var notas=AlinhamentoInterfaceTests.Filhos(view).OfType<Border>().Where(b=>b.Visibility==Visibility.Visible&&b.Style==view.FindResource("ObservacaoDestacada")).ToArray();
                    Assert.Equal(2,notas.Length);Assert.All(notas,b=>Assert.Equal(((SolidColorBrush)view.FindResource("KdsObservacaoSurface")).Color,((SolidColorBrush)b.Background).Color));
                    var selos=AlinhamentoInterfaceTests.Filhos(view).OfType<Border>().Where(b=>b.Style==view.FindResource("SeloEstado")).ToArray();
                    Assert.Contains(selos,b=>((SolidColorBrush)b.Background).Color==((SolidColorBrush)view.FindResource("KdsPreparoSurface")).Color);
                    Assert.Contains(selos,b=>((SolidColorBrush)b.Background).Color==((SolidColorBrush)view.FindResource("KdsProntoSurface")).Color);
                    Assert.Contains("Comanda encerrada · entrega pendente",textos);
                    var botoes=AlinhamentoInterfaceTests.Filhos(view).OfType<Button>().Where(b=>Equals(b.Content,"Entregue")).ToArray();
                    Assert.Equal(4,botoes.Length);Assert.Single(botoes,b=>b.IsEnabled);
                    Assert.Contains(AlinhamentoInterfaceTests.Filhos(view).OfType<ScrollViewer>(),s=>s.ScrollableHeight>0);
                    Capturar(view,$"kds-{(escuro?"escuro":"claro")}-{tamanho.Width}");
                }
                foreach(var escuro in new[]{false,true})
                {
                    TemaInterface.Aplicar(escuro,false);var view=new CozinhaView(()=>Array.Empty<ComandaComItens>());view.Atualizar();Medir(view,new Size(800,600));
                    Assert.True(view.ViewModel.Vazio);Capturar(view,$"kds-vazio-{(escuro?"escuro":"claro")}");
                    var erro=new CozinhaView(()=>throw new IOException());erro.Atualizar();Medir(erro,new Size(800,600));
                    Assert.Equal("Cozinha indisponível",erro.ViewModel.TituloVazio);Capturar(erro,$"kds-erro-{(escuro?"escuro":"claro")}");
                }
            }
            finally { TemaInterface.Aplicar(false,false); }
        });
    }

    [Fact]
    public void CoresSemanticasTemContrasteNosDoisTemasENotasVaziasNaoGeramBlocos()
    {
        static double Luz(Color cor)
        {
            static double Canal(byte c){var v=c/255d;return v<=.04045?v/12.92:Math.Pow((v+.055)/1.055,2.4);}
            return .2126*Canal(cor.R)+.7152*Canal(cor.G)+.0722*Canal(cor.B);
        }
        static double Contraste(Color a,Color b)=>(Math.Max(Luz(a),Luz(b))+.05)/(Math.Min(Luz(a),Luz(b))+.05);
        foreach(var escuro in new[]{false,true})
        foreach(var estado in new[]{"Pendente","Preparo","Pronto","Atraso","Observacao"})
        {
            var fundo=PaletaComanda.Cor("Kds"+estado+"Surface",escuro);var texto=PaletaComanda.Cor("Kds"+estado+"Ink",escuro);
            Assert.True(Contraste(texto,fundo)>=4.5,$"{estado}, escuro={escuro}: contraste do texto insuficiente.");
            if(estado!="Atraso")Assert.True(Contraste(PaletaComanda.Cor("Kds"+estado+"Line",escuro),fundo)>=3);
        }
        var item=new ItemKds(1,1,1,"Suco",1,UnidadeVenda.Unidade,"   ",StatusPreparo.Pendente,DateTime.UtcNow);
        Assert.False(item.TemObservacao);Assert.DoesNotContain("Observação",item.NomeAcessivel);
    }
    private static void Medir(FrameworkElement view,Size tamanho){view.Measure(tamanho);view.Arrange(new Rect(new Point(),tamanho));view.UpdateLayout();}
    private static void Capturar(FrameworkElement view,string nome)
    {
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrEmpty(pasta))return;Directory.CreateDirectory(pasta);
        var bitmap=new RenderTargetBitmap((int)view.ActualWidth,(int)view.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
}
