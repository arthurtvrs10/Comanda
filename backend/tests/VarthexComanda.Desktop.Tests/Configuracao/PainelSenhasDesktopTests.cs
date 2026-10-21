using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Application.Tests.Configuracao;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Desktop.Tests.Xaml;

namespace VarthexComanda.Desktop.Tests.Configuracao;

public class PainelSenhasDesktopTests
{
    [Fact]
    public void ChamarAbreTelaConfiguradaReutilizaReabreOcultaEProtegeSessao()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var repo=new FakeConfiguracaoRepository();var service=new PainelSenhasService(repo);
            var monitores=MonitoresPainel.Listar();Assert.NotEmpty(monitores);Assert.All(monitores,m=>Assert.True(m.Largura>0&&m.Altura>0));
            var destino=monitores.Last();service.Salvar(new(){Monitor=destino.Id,TelaCheia=true,Beep=false,Voz=false});
            bool autorizado=true;var criadas=new List<JanelaExibicaoSenhas>();
            using var painel=new GerenciadorPainelSenhas(service,()=>autorizado,(estado,p)=>{var tela=new JanelaExibicaoSenhas(estado,p){Opacity=0,ShowInTaskbar=false};criadas.Add(tela);return tela;});
            painel.Chamar("001");Assert.True(painel.PainelVisivel);Assert.Single(criadas);
            Assert.True(GetWindowRect(new System.Windows.Interop.WindowInteropHelper(criadas[0]).Handle,out var posicao));
            Assert.Equal(destino.X,posicao.X);Assert.Equal(destino.Y,posicao.Y);Assert.Equal(destino.Largura,posicao.Direita-posicao.X);Assert.Equal(WindowStyle.None,criadas[0].WindowStyle);
            painel.Preferencias.TelaCheia=false;painel.Chamar("002");Assert.Equal(WindowStyle.SingleBorderWindow,criadas[0].WindowStyle);Assert.Single(criadas);Assert.Equal("002",painel.Estado.Atual);Assert.Equal(new[]{"001"},painel.Estado.Anteriores);
            Assert.Contains(Filhos((FrameworkElement)criadas[0].Content).OfType<TextBlock>(),t=>t.Text=="002");
            painel.Ocultar();Assert.False(painel.PainelVisivel);Assert.Equal("002",painel.Estado.Atual);
            painel.Chamar("003");Assert.True(painel.PainelVisivel);Assert.Single(criadas);Assert.Equal(new[]{"002","001"},painel.Estado.Anteriores);
            criadas[0].Close();painel.Chamar("004");Assert.Equal(2,criadas.Count);Assert.Equal(new[]{"003","002","001"},painel.Estado.Anteriores);
            autorizado=false;Assert.Throws<InvalidOperationException>(()=>painel.Chamar("005"));Assert.Equal("004",painel.Estado.Atual);
            autorizado=true;painel.Limpar();Assert.Null(painel.Estado.Atual);Assert.Empty(painel.Estado.Anteriores);
            painel.Encerrar();Assert.False(painel.PainelVisivel);
        });
    }
    [Fact]
    public void ChamadorETelaPublicaRenderizamDoisTemasESenhasLongas()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();
            var service=new PainelSenhasService(new FakeConfiguracaoRepository());service.Salvar(new(){Beep=false,Voz=false});
            using var painel=new GerenciadorPainelSenhas(service,()=>true);
            for(int i=100;i<=105;i++)painel.Estado.Chamar(i.ToString());
            using var janela=new JanelaParaTeste(new JanelaChamadorSenhas(painel));
            foreach(var escuro in new[]{false,true})
            {
                TemaInterface.Aplicar(escuro,false);
                var raiz=(FrameworkElement)janela.Valor.Content;Renderizar(raiz,690,620,$"chamador-{(escuro?"escuro":"claro")}");
                var abas=Filhos(raiz).OfType<TabControl>().Single();abas.SelectedIndex=1;Renderizar(raiz,690,620,$"cores-painel-{(escuro?"escuro":"claro")}");abas.SelectedIndex=0;
            }
            var raizCores=(FrameworkElement)janela.Valor.Content;
            var abasCores=Filhos(raizCores).OfType<TabControl>().Single();abasCores.SelectedIndex=1;
            Renderizar(raizCores,660,590,"cores-selecao");
            var seletores=Filhos(raizCores).OfType<ComboBox>().Where(c=>System.Windows.Automation.AutomationProperties.GetName(c) is "Cor de fundo:" or "Para criar degradê, selecione outra cor:").ToArray();
            Assert.Equal(2,seletores.Length);
            seletores[0].SelectedItem=seletores[0].Items.Cast<JanelaChamadorSenhas.CorPainel>().Single(i=>Equals(i.Hex,"#000000"));
            seletores[1].SelectedItem=seletores[1].Items.Cast<JanelaChamadorSenhas.CorPainel>().Single(i=>Equals(i.Hex,"#191970"));
            Filhos(raizCores).OfType<RadioButton>().Single(r=>Equals(r.Content,"Branco")).IsChecked=true;
            Filhos(raizCores).OfType<Button>().Single(b=>Equals(b.Content,"Salvar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("#000000",service.Carregar().CorInicial);Assert.Equal("#191970",service.Carregar().CorFinal);Assert.Equal("#FFFFFF",service.Carregar().CorNumeros);
            Renderizar(raizCores,660,590,"cores-preto-midnightblue");
            Filhos(raizCores).OfType<Button>().Single(b=>System.Windows.Automation.AutomationProperties.GetName(b)=="Voltar à configuração padrão").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("#DCDCDC",painel.Preferencias.CorInicial);Assert.Equal("#1E90FF",painel.Preferencias.CorFinal);Assert.Equal("#000000",painel.Preferencias.CorNumeros);
            seletores[0].SelectedItem=seletores[0].Items.Cast<JanelaChamadorSenhas.CorPainel>().Single(i=>Equals(i.Hex,"#00FFFFFF"));
            Filhos(raizCores).OfType<Button>().Single(b=>Equals(b.Content,"Salvar")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("#00FFFFFF",service.Carregar().CorInicial);
            abasCores.SelectedIndex=0;
            var estado=new EstadoPainelSenhas();for(int i=100;i<=105;i++)estado.Chamar(i.ToString());
            var tela=new JanelaExibicaoSenhas(estado,service.Carregar());var conteudo=(FrameworkElement)tela.Content;
            Renderizar(conteudo,1280,720,"painel-publico");Assert.Contains(Filhos(conteudo).OfType<TextBlock>(),t=>t.Text=="105");
            estado.Chamar("999999999");Renderizar(conteudo,800,450,"painel-publico-menor");
            var atual=(FrameworkElement)janela.Valor.Content;
            var limpar=Filhos(atual).OfType<CheckBox>().Single(c=>Equals(c.Content,"Limpar número após 1 minuto"));
            limpar.IsChecked=true;limpar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Assert.True(service.Carregar().LimparAposUmMinuto);
            Assert.Equal(Visibility.Collapsed,Filhos(atual).OfType<Button>().Single(b=>Equals(b.Content,"Salvar")).Visibility);
            tela.Close();TemaInterface.Aplicar(false,false);
        });
    }
    private static void Renderizar(FrameworkElement raiz,int largura,int altura,string nome)
    {
        raiz.Measure(new Size(largura,altura));raiz.Arrange(new Rect(0,0,largura,altura));raiz.UpdateLayout();
        var pasta=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(string.IsNullOrWhiteSpace(pasta))return;
        Directory.CreateDirectory(pasta);var bitmap=new RenderTargetBitmap(largura,altura,96,96,PixelFormats.Pbgra32);bitmap.Render(raiz);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var arquivo=File.Create(Path.Combine(pasta,nome+".png"));encoder.Save(arquivo);
    }
    private static IEnumerable<DependencyObject> Filhos(DependencyObject raiz)
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(raiz);i++){var filho=VisualTreeHelper.GetChild(raiz,i);yield return filho;foreach(var neto in Filhos(filho))yield return neto;}
    }
    private sealed class JanelaParaTeste(Window valor):IDisposable
    {
        public Window Valor { get; }=valor;
        public void Dispose()=>Valor.Close();
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct Retangulo { public int X,Y,Direita,Inferior; }
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return:System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr handle,out Retangulo retangulo);
}
