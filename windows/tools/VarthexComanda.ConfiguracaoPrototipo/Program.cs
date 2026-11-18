using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VarthexComanda.Desktop;
using VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

namespace VarthexComanda.ConfiguracaoPrototipo;
internal static class Program
{
    [STAThread] private static void Main(string[] args)
    {
        try
        {
            if(args.Contains("--verificar")){Verificar();return;}
            var app=new App(false);app.InitializeComponent();TemaInterface.Aplicar(false,false);
            var window=new JanelaConfiguracao();
            if(args.Contains("--verificar-interface")){window.VerificarEdicaoMascarada();window.VerificarFluxoLista();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"verificacao-interface.txt"),"Digitação sequencial de CPF, CNPJ e telefone aprovada. Lista, abertura de formulário, liberação da próxima etapa e retorno aprovados.");return;}
            if(args.Contains("--capturar"))window.Loaded+=(_,_)=>window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,new Action(()=>
            {
                var pasta=Path.Combine(AppContext.BaseDirectory,"capturas");Directory.CreateDirectory(pasta);
                foreach(var escuro in new[]{false,true})foreach(var largura in new[]{1000,1280})
                {
                    TemaInterface.Aplicar(escuro,false);window.Width=largura;window.Height=800;
                    for(var i=0;i<7;i++)
                    {
                        window.CarregarExemplo(i);window.UpdateLayout();
                        var root=(FrameworkElement)window.Content;root.UpdateLayout();
                        var bitmap=new RenderTargetBitmap((int)root.ActualWidth,(int)root.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(root);
                        var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var output=File.Create(Path.Combine(pasta,$"etapa-{i+1}-{(escuro?"escuro":"claro")}-{largura}.png"));png.Save(output);
                    }
                }
                window.Close();
            }));
            app.Run(window);
        }
        catch(Exception ex){File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"erro-prototipo.log"),ex.ToString());Environment.ExitCode=1;}
    }
    private static void Verificar()
    {
        var count=0;void Check(bool ok){if(!ok)throw new InvalidOperationException($"Verificação {count+1} falhou.");count++;}
        var e=new EstadoConfiguracao();Check(e.Erro(0)!="");e.Nome="Teste";e.Cupom.ExibirDocumentoCupom=false;e.Cupom.ExibirEnderecoCupom=false;Check(e.Erro(0)=="");e.Cupom.Documento="123";Check(e.Erro(0)!="");e.Cupom.Documento="";e.Cupom.ExibirDocumentoCupom=true;Check(e.Erro(1)!="");e.Cupom.ExibirDocumentoCupom=false;e.Balcao=false;Check(e.Erro(2)!="");e.Balcao=true;e.QuantidadeComandas=0;Check(e.Erro(2)!="");e.QuantidadeComandas=30;e.Pagamentos.Clear();Check(e.Erro(4)!="");Check(EstadoConfiguracao.Preco("49,90")==4990);Check(EstadoConfiguracao.Preco("1,001")==null);Check(EstadoConfiguracao.Preco("-1")==null);e.Pagamentos.Add("Pix");e.Concluidas=[0,1,2,3,4,5];Check(e.Progresso==86);e.Finalizado=true;Check(e.Progresso==100);Check(!e.Json().Contains("\"Senha\":",StringComparison.OrdinalIgnoreCase));
        var novo=new EstadoConfiguracao();Check(novo.PodeAcessar(0));Check(!novo.PodeAcessar(1));novo.Nome="Teste";novo.Concluidas.Add(0);Check(novo.PodeAcessar(1));Check(!novo.PodeAcessar(2));novo.Concluidas.Add(1);Check(novo.PodeAcessar(2));novo.Nome="";Check(!novo.PodeAcessar(2));novo.Nome="Teste";novo.TipoDocumento="CPF";novo.Cupom.Documento="12345678901";Check(novo.Erro(0)=="");novo.TipoDocumento="CNPJ";Check(novo.Erro(0)!="");
        Check(Mascaras.Documento("12345678901","CPF")=="123.456.789-01");Check(Mascaras.Documento("12345678000199","CNPJ")=="12.345.678/0001-99");Check(Mascaras.Documento("123.456.789-01","CPF")=="123.456.789-01");Check(Mascaras.Telefone("11987654321")=="(11) 98765-4321");Check(Mascaras.Telefone("1134567890")=="(11) 3456-7890");Check(Mascaras.Telefone("abc11987654321")=="(11) 98765-4321");Check(Mascaras.Documento("123456789012345678","CPF")=="123.456.789-01");Check(Mascaras.Documento("1234","CPF")=="123.4");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"verificacao.txt"),$"{count} verificações aprovadas.");
    }
}
