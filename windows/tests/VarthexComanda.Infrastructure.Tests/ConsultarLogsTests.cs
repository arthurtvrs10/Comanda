using VarthexComanda.Infrastructure.Logging;

namespace VarthexComanda.Infrastructure.Tests;

public class ConsultarLogsTests
{
    [Fact]
    public void LeArquivoAtivoComMascaraExcecoesERetencaoSemAlterarOriginais()
    {
        var pasta = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")); Directory.CreateDirectory(pasta);
        try
        {
            var logger = LoggingConfigurator.CreateLogger(pasta, perfilUsuario: @"C:\Users\Pessoa");
            using (logger as IDisposable)
            {
                logger.Information("Iniciando teste em {Pasta}", @"C:\Users\Pessoa\Dados");
                logger.Error(new IOException("Arquivo indisponível"), "Falha ao gravar");
                var linhas = new ConsultarLogs(pasta).Executar();
                Assert.Equal(2, linhas.Count); Assert.Equal("ERR", linhas[0].Nivel);
                Assert.Contains("Arquivo indisponível", linhas[0].Mensagem);
                Assert.Contains("%USERPROFILE%", linhas[1].Mensagem); Assert.DoesNotContain("Pessoa", linhas[1].Mensagem);
            }
            var arquivo = Directory.GetFiles(pasta).Single();
            File.WriteAllLines(arquivo, Enumerable.Range(1, 900).Select(i => $"2026-10-24 12:00:00.000 [INF] Evento {i}"));
            var dados = new ConsultarLogs(pasta).Executar();
            Assert.Equal(500, dados.Count); Assert.Equal("Evento 900", dados[0].Mensagem); Assert.Equal("Evento 401", dados[^1].Mensagem);
            Assert.Equal(900, File.ReadAllLines(arquivo).Length);
        }
        finally { Directory.Delete(pasta, true); }
    }
    [Fact]
    public void PastaInexistenteEArquivoGrandeNaoImpedemConsulta()
    {
        var pasta = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")); Assert.Empty(new ConsultarLogs(pasta).Executar());
        Directory.CreateDirectory(pasta);
        try
        {
            File.WriteAllText(Path.Combine(pasta, "varthex-comanda-20261024.log"), new string('x', 600_000) + "\n2026-10-24 12:00:00.000 [WRN] Final\n");
            Assert.Equal("Final", Assert.Single(new ConsultarLogs(pasta).Executar()).Mensagem);
        }
        finally { Directory.Delete(pasta, true); }
    }
}
