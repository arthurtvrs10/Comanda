using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using VarthexComanda.Desktop.Licenciamento;

namespace VarthexComanda.Desktop.Tests;
public class ReceberLicencaTests
{
    [Theory]
    [InlineData("1234 5678","12345678")]
    [InlineData("1234-5678","12345678")]
    [InlineData("00001234","00001234")]
    public void AceitaApenasOitoDigitosComSeparador(string entrada,string esperado)=>Assert.Equal(esperado,ReceberLicenca.NormalizarCodigo(entrada));
    [Theory]
    [InlineData("123456")][InlineData("123456789")][InlineData("1234abcd")][InlineData("１２３４５６７８")]
    public void RejeitaCodigoInvalido(string entrada)=>Assert.Throws<InvalidOperationException>(()=>ReceberLicenca.NormalizarCodigo(entrada));
    [Fact]
    public async Task EnviaCodigoNoCorpoComMaquinaESemSegredos()
    {
        var handler=new Handler(async request=>{Assert.Equal(HttpMethod.Post,request.Method);Assert.Equal("/v1/receive",request.RequestUri!.AbsolutePath);using var json=JsonDocument.Parse(await request.Content!.ReadAsStringAsync());Assert.Equal("12345678",json.RootElement.GetProperty("code").GetString());Assert.Equal(new string('A',64),json.RootElement.GetProperty("machine").GetString());Assert.Null(request.Headers.Authorization);return Response("{\"license\":\"signed-token\"}");});
        using var client=new HttpClient(handler);
        Assert.Equal("signed-token",await ReceberLicenca.BuscarComCliente(client,ReceberLicenca.Endpoint,"1234 5678",new string('A',64)));
    }
    [Fact]
    public async Task MostraErroDeExpiracaoSemTentarNovamente()
    {
        int calls=0;using var client=new HttpClient(new Handler(_=>{calls++;return Task.FromResult(Response("{\"error\":\"Código expirado\"}",HttpStatusCode.BadRequest));}));
        var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>ReceberLicenca.BuscarComCliente(client,ReceberLicenca.Endpoint,"12345678",new string('A',64)));Assert.Contains("expirado",error.Message);Assert.Equal(1,calls);
    }
    [Theory][InlineData("{}")] [InlineData("{\"license\":123}")] [InlineData("{\"license\":\"\"}")]
    public async Task RejeitaRespostaInvalida(string response)
    {using var client=new HttpClient(new Handler(_=>Task.FromResult(Response(response))));await Assert.ThrowsAsync<InvalidOperationException>(()=>ReceberLicenca.BuscarComCliente(client,ReceberLicenca.Endpoint,"12345678",new string('A',64)));}
    [Fact]
    public async Task FalhaDeRedeOrientaReenvioSemAtivar()
    {using var client=new HttpClient(new Handler(_=>throw new HttpRequestException()));var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>ReceberLicenca.BuscarComCliente(client,ReceberLicenca.Endpoint,"12345678",new string('A',64)));Assert.Contains("internet",error.Message);}
    private static HttpResponseMessage Response(string body,HttpStatusCode status=HttpStatusCode.OK)=>new(status){Content=new StringContent(body,Encoding.UTF8,"application/json")};
    private sealed class Handler(Func<HttpRequestMessage,Task<HttpResponseMessage>> action):HttpMessageHandler{protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>action(request);}
}
