using System.Reflection;
using VarthexComanda.Application.Atendimento;
using VarthexComanda.Desktop.Atendimento;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence.Atendimento;
using Xunit;

namespace VarthexComanda.Desktop.Tests.Homologacao;

// CT13 — criterio de docs/docs/09-testes-aceitacao.md: cadastro, comanda, total, encerramento e
// historico funcionam sem rede.
//
// Cobertura estrutural do núcleo: Domain e Application não dependem de APIs de rede.
// Desktop e Infrastructure agora oferecem backup opcional no Google Drive, autorizado pelo usuário.
// O ensaio com a rede fisicamente desligada continua descrito em docs/homologacao.
public class OperacaoOfflinePorConstrucaoTests
{
    // Assemblies de framework que dao acesso a rede (HTTP, sockets, WebSocket, WebClient/WebRequest,
    // e-mail, ping, servidor HTTP, DNS, TLS). System.Net.Primitives/NetworkInformation nao entram:
    // so tipos de dado (IPAddress etc.) ou consulta de interfaces, sem trafego.
    private static readonly string[] AssembliesDeRedeProibidos =
    {
        "System.Net.Http",
        "System.Net.Http.Json",
        "System.Net.Sockets",
        "System.Net.WebClient",
        "System.Net.WebSockets",
        "System.Net.WebSockets.Client",
        "System.Net.Requests",
        "System.Net.HttpListener",
        "System.Net.Mail",
        "System.Net.Ping",
        "System.Net.Security",
        "System.Net.NameResolution",
        "System.Net.ServicePoint",
        "System.Net.WebProxy",
        "System.Net.WebHeaderCollection",
    };

    // O assembly do projeto Desktop chama-se "VarthexComanda" (VarthexComanda.dll).
    private static IReadOnlyDictionary<string, Assembly> AssembliesDoProduto() => new Dictionary<string, Assembly>
    {
        ["VarthexComanda.Domain"] = typeof(Comanda).Assembly,
        ["VarthexComanda.Application"] = typeof(AbrirComanda).Assembly,
    };

    private static List<string> ReferenciasDeRede(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(r => r.Name!)
            .Where(r => AssembliesDeRedeProibidos.Contains(r, StringComparer.OrdinalIgnoreCase))
            .ToList();

    [Fact]
    [Trait("Caso", "CT13")]
    public void CT13_NucleoDePedidosECatalogo_NaoReferenciaApisDeRede()
    {
        foreach (var (nome, assembly) in AssembliesDoProduto())
        {
            Assert.Equal(nome, assembly.GetName().Name);
            Assert.NotEmpty(assembly.GetReferencedAssemblies()); // controle: a leitura das referencias funcionou

            var proibidos = ReferenciasDeRede(assembly);
            Assert.True(proibidos.Count == 0, $"{nome} referencia API de rede: {string.Join(", ", proibidos)}");
        }
    }

    // Controle do proprio detector (nao e caso de aceitacao): este assembly de teste USA HttpClient,
    // entao tem de ser flagrado; senao o teste acima poderia passar sem enxergar nada.
    [Fact]
    public void Detector_FlagraAssemblyQueUsaHttpClient()
    {
        Assert.Equal("HttpClient", typeof(HttpClient).Name); // uso real do tipo: o compilador emite a referencia

        Assert.Contains("System.Net.Http", ReferenciasDeRede(typeof(OperacaoOfflinePorConstrucaoTests).Assembly));
    }
}
