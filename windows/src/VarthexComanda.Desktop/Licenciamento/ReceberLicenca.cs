using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VarthexComanda.Desktop.Licenciamento;

internal static class ReceberLicenca
{
    internal static readonly Uri Endpoint=new("https://vcomanda-licencas-transferencia.comandasoftwarebyvarthex.workers.dev/v1/receive");
    internal static string NormalizarCodigo(string valor)
    {
        var codigo=valor.Replace(" ","").Replace("-","");
        if(!Regex.IsMatch(codigo,@"\A[0-9]{8}\z"))throw new InvalidOperationException("Digite os 8 dígitos mostrados no celular.");
        return codigo;
    }
    internal static async Task<string> Buscar(string codigo,string computador,CancellationToken cancellation=default)
    {
        using var handler=new HttpClientHandler { AllowAutoRedirect=false };
        using var client=new HttpClient(handler) { Timeout=TimeSpan.FromSeconds(20),MaxResponseContentBufferSize=16384 };
        return await BuscarComCliente(client,Endpoint,codigo,computador,cancellation);
    }
    internal static async Task<string> BuscarComCliente(HttpClient client,Uri endpoint,string codigo,string computador,CancellationToken cancellation=default)
    {
        try
        {
            using var response=await client.PostAsJsonAsync(endpoint,new {code=NormalizarCodigo(codigo),machine=computador},cancellation);
            using var json=JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync(cancellation));
            if(!response.IsSuccessStatusCode)throw new InvalidOperationException(json.RootElement.ValueKind==JsonValueKind.Object && json.RootElement.TryGetProperty("error",out var error) && error.ValueKind==JsonValueKind.String?error.GetString():"Não foi possível receber a licença.");
            if(json.RootElement.ValueKind!=JsonValueKind.Object||!json.RootElement.TryGetProperty("license",out var property)||property.ValueKind!=JsonValueKind.String)throw new InvalidOperationException("Resposta inválida do serviço.");
            var token=property.GetString();
            if(string.IsNullOrWhiteSpace(token)||token.Length>12000)throw new InvalidOperationException("Resposta inválida do serviço.");
            return token;
        }
        catch(Exception e) when(e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException)
        {throw new InvalidOperationException("Não foi possível receber. Confira a internet. Se o código já foi consumido, envie novamente pelo celular. Também é possível colar a licença manualmente.");}
    }
}
