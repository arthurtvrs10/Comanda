using Microsoft.Data.Sqlite;
using VarthexComanda.Desktop.Ajuda;
using VarthexComanda.Desktop.Configuracao;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Desktop.Tests.Configuracao;
public class DiagnosticoServicosTests
{
    [Fact]
    public void DiagnosticoTestaBancoEPastasSemAlterarBancoEReportaFalhasIndependentes()
    {
        var pasta=Path.Combine(Path.GetTempPath(),"varthex-diagnostico-"+Guid.NewGuid());var paths=new AppPaths(pasta);paths.EnsureCreated();
        try
        {
            using(var db=new SqliteConnection($"Data Source={paths.DatabasePath};Pooling=False")){db.Open();using var c=db.CreateCommand();c.CommandText="CREATE TABLE comanda (id INTEGER); CREATE TABLE produto (id INTEGER); CREATE TABLE item_comanda (id INTEGER); CREATE TABLE venda (id INTEGER); CREATE TABLE configuracao (id INTEGER);";c.ExecuteNonQuery();}
            var antes=File.ReadAllBytes(paths.DatabasePath);
            var resultados=DiagnosticoServicos.Avaliar(paths,()=>true,()=>[new ImpressoraWindows("Teste","Teste",false)]);
            Assert.Equal(8,resultados.Count);Assert.DoesNotContain(resultados,r=>r.Estado=="Falha");Assert.Equal("Funcionando",resultados.Single(r=>r.Servico=="Banco de dados").Estado);Assert.Equal(antes,File.ReadAllBytes(paths.DatabasePath));
            Assert.Empty(Directory.GetFiles(pasta,".verificacao-*",SearchOption.AllDirectories));
            var falhas=DiagnosticoServicos.Avaliar(paths,()=>false,()=>throw new InvalidOperationException());
            Assert.Equal("Atenção",falhas.Single(r=>r.Servico=="Assinatura").Estado);Assert.Equal("Falha",falhas.Single(r=>r.Servico=="Impressão do Windows").Estado);Assert.Equal("Funcionando",falhas.Single(r=>r.Servico=="Banco de dados").Estado);
        }
        finally{SqliteConnection.ClearAllPools();Directory.Delete(pasta,true);}
    }
    [Fact]
    public void AusenciaDeBancoOuPastaNaoEApresentadaComoFuncionando()
    {
        var paths=new AppPaths(Path.Combine(Path.GetTempPath(),"ausente-"+Guid.NewGuid()));var resultado=DiagnosticoServicos.Avaliar(paths,()=>false,()=>[]);
        Assert.Equal("Falha",resultado.Single(r=>r.Servico=="Banco de dados").Estado);Assert.Equal(5,resultado.Count(r=>r.Estado=="Falha"));Assert.False(Directory.Exists(paths.Root));
    }
}
