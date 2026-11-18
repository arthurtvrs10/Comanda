using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VarthexComanda.Desktop.RecuperacaoEmail;

namespace VarthexComanda.Desktop.Tests.Xaml;

public sealed class RecuperacaoEmailXamlTests
{
    private sealed class Handler(RSA rsa):HttpMessageHandler
    {
        private string _nonce="",_digest="",_install="";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            using var doc=JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));var b=doc.RootElement;
            object result;
            if(request.RequestUri!.AbsolutePath.EndsWith("request"))
            {
                _nonce=b.GetProperty("nonce").GetString()!;_digest=b.GetProperty("credentialHash").GetString()!;_install=b.GetProperty("installationId").GetString()!;
                result=new{challengeId=new string('a',32),maskedEmail="ge***@example.com",expiresIn=600,retryAfter=60};
            }
            else
            {
                static string B(byte[] data)=>Convert.ToBase64String(data).TrimEnd('=').Replace('+','-').Replace('/','_');
                var now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();var p="v1."+B(JsonSerializer.SerializeToUtf8Bytes(new{aud="vcomanda-gestor-reset",installationId=_install,nonce=_nonce,credentialHash=_digest,challengeId=new string('a',32),iat=now,exp=now+600}));
                result=new{grant=p+"."+B(rsa.SignData(Encoding.UTF8.GetBytes(p),HashAlgorithmName.SHA256,RSASignaturePadding.Pss))};
            }
            return new(System.Net.HttpStatusCode.OK){Content=JsonContent.Create(result)};
        }
    }
    [Fact] public void CadastroEIndisponibilidadeRespeitamTemaECamposAcessiveis()
    {
        ThreadingHelper.EmSta(()=>
        {
            CarregamentoDeXamlTests.GarantirApp();var dir=Path.Combine(Path.GetTempPath(),"email-ui-"+Guid.NewGuid());
            var gestor=new AcessoGestor(Path.Combine(dir,"gestor"));gestor.CadastrarComSenhaRecuperacao("antiga","offline");
            using var rsa=RSA.Create(2048);using var http=new HttpClient(new Handler(rsa));
            var owner=new Window {Width=1,Height=1,Left=-20000,Top=-20000,ShowInTaskbar=false};owner.Show();
            try
            {
                foreach(var dark in new[]{false,true})
                {
                    TemaInterface.Aplicar(dark,false);
                    foreach(var scenario in new[]{"cadastro","codigo","nova-senha","indisponivel"})
                    {
                        var cadastrar=scenario=="cadastro";var state=Path.Combine(dir,scenario+dark);
                        File.WriteAllBytes(state,ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(new ClienteRecuperacaoEmail.Estado(new string('a',32),new string('b',64),"ge***@example.com")),null,DataProtectionScope.CurrentUser));
                        var cliente=new ClienteRecuperacaoEmail(gestor,state,new("https://example.com",rsa.ExportSubjectPublicKeyInfoPem()),http);
                        var w=new JanelaRecuperacaoEmail(gestor,cadastrar,owner,()=>scenario=="indisponivel"?null:cliente);
                        try
                        {
                            w.WindowStartupLocation=WindowStartupLocation.Manual;w.Left=-20000;w.Top=-20000;w.Show();
                            var content=(FrameworkElement)w.Content;w.UpdateLayout();
                            var controls=AlinhamentoInterfaceTests.Filhos(content).OfType<Control>().Where(c=>c.IsVisible||c.Visibility==Visibility.Visible).ToArray();
                            if(scenario is "codigo" or "nova-senha")
                            {
                                var send=controls.OfType<Button>().Single(b=>Equals(b.Content,"Enviar código"));send.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                var code=controls.OfType<TextBox>().Single(c=>System.Windows.Automation.AutomationProperties.GetName(c)=="Código de seis números");
                                Pump(()=>((FrameworkElement)code.Parent).Visibility==Visibility.Visible&&code.IsEnabled);
                                if(scenario=="nova-senha")
                                {
                                    code.Text="123456";controls.OfType<Button>().Single(b=>Equals(b.Content,"Verificar código")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                    var password=controls.OfType<PasswordBox>().Single(c=>System.Windows.Automation.AutomationProperties.GetName(c)=="Nova senha (4 a 128 caracteres)");
                                    Pump(()=>((FrameworkElement)password.Parent).Visibility==Visibility.Visible&&password.IsEnabled);
                                }
                                w.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ApplicationIdle);w.UpdateLayout();
                            }
                            if(cadastrar)
                            {
                                var email=controls.OfType<TextBox>().Single(x=>System.Windows.Automation.AutomationProperties.GetName(x)=="E-mail de recuperação");
                                Assert.True(System.Windows.Automation.AutomationProperties.GetIsRequiredForForm(email));
                                Assert.Contains(controls.OfType<Button>(),b=>Equals(b.Content,"Enviar código"));
                            }
                            else if(scenario=="indisponivel")
                            {
                                Assert.Contains(controls.OfType<Button>(),b=>Equals(b.Content,"Usar senha de recuperação offline"));
                                Assert.Contains(AlinhamentoInterfaceTests.Filhos(content).OfType<TextBlock>(),t=>t.Text.Contains("ainda não foi habilitada"));
                            }
                            var output=Environment.GetEnvironmentVariable("VARTHEX_PREVIAS_TESTE");if(output is null)continue;Directory.CreateDirectory(output);
                            var visibleButtons=controls.OfType<Button>().Where(b=>b.IsVisible).ToArray();
                            Assert.All(visibleButtons,b=>Assert.True(b.TransformToAncestor(content).TransformBounds(new Rect(b.RenderSize)).Bottom<=content.ActualHeight+1,"Ação cortada na janela de recuperação."));
                            var bitmap=new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth),(int)Math.Ceiling(content.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(content);
                            var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var file=File.Create(Path.Combine(output,$"recuperacao-{scenario}-{(dark?"escuro":"claro")}.png"));png.Save(file);
                        }
                        finally{w.Close();}
                    }
                }
            }
            finally{owner.Close();TemaInterface.Aplicar(false,false);Directory.Delete(dir,true);}
        });
    }
    private static void Pump(Func<bool> predicate)
    {
        if(predicate())return;var frame=new System.Windows.Threading.DispatcherFrame();var deadline=DateTime.UtcNow.AddSeconds(3);
        var timer=new System.Windows.Threading.DispatcherTimer {Interval=TimeSpan.FromMilliseconds(10)};
        timer.Tick+=(_,_)=>{if(predicate()||DateTime.UtcNow>deadline)frame.Continue=false;};
        timer.Start();System.Windows.Threading.Dispatcher.PushFrame(frame);timer.Stop();Assert.True(predicate());
    }
}
