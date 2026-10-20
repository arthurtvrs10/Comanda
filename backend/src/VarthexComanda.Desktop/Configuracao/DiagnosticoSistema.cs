using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Data.Sqlite;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Desktop.Configuracao;

public static class DiagnosticoSistema
{
    public static string Gerar(AppPaths paths,string nome,string backup)
    {
        var texto=new StringBuilder();
        void Secao(string nomeSecao){texto.AppendLine();texto.AppendLine(new string('=',52));texto.AppendLine(nomeSecao);texto.AppendLine(new string('=',52));}
        Secao("VARTHEX COMANDA — REGISTRO E AMBIENTE");
        texto.AppendLine($"Versão: {VersaoDoAplicativo.Atual}\nAplicação: Windows · Operação local\nEstabelecimento: {nome}");
        try
        {
            var servico=new Licenciamento.ServicoLicenca();var dados=servico.Verificar();
            texto.AppendLine($"Registrada para: {dados.Cliente}\nLicença ID: {dados.Id}\nInício: {dados.Inicio.ToLocalTime():dd/MM/yyyy}\nValidade: {(dados.Vencimento is null?"Vitalícia":dados.Vencimento.Value.ToLocalTime().ToString("dd/MM/yyyy"))}\nComputador: {servico.Computador}");
        }
        catch { texto.AppendLine("Assinatura: não foi possível verificar. Consulte a ativação."); }
        Secao("BANCO DE DADOS");texto.AppendLine("Servidor: SQLite local (sem servidor Firebird)");
        try
        {
            if(!File.Exists(paths.DatabasePath))throw new IOException();
            texto.AppendLine($"Tamanho do arquivo: {new FileInfo(paths.DatabasePath).Length/1024d/1024d:0.00} MB");
            using var conexao=new SqliteConnection(new SqliteConnectionStringBuilder { DataSource=paths.DatabasePath,Mode=SqliteOpenMode.ReadOnly }.ToString());conexao.Open();
            foreach(var (nomeTabela,tabela) in new[]{("Produtos","produto"),("Comandas","comanda"),("Itens das comandas","item_comanda"),("Vendas","venda")})
            {
                using var cmd=conexao.CreateCommand();cmd.CommandText=$"SELECT COUNT(*) FROM {tabela}";texto.AppendLine($"Registros de {nomeTabela}: {cmd.ExecuteScalar()}");
            }
            using var migracao=conexao.CreateCommand();migracao.CommandText="SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 1";texto.AppendLine("Última migração: "+migracao.ExecuteScalar());
        }
        catch { texto.AppendLine("Estatísticas do banco indisponíveis."); }
        texto.AppendLine("Último backup: "+backup);
        Secao("WINDOWS E RUNTIME");texto.AppendLine($"Sistema: {RuntimeInformation.OSDescription}\nArquitetura: {RuntimeInformation.OSArchitecture}\nRuntime: {RuntimeInformation.FrameworkDescription}\nUsuário Windows: {Environment.UserName}\nWindows iniciou há: {Environment.TickCount64/60000} minutos\nRelógio: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss zzz}");
        Secao("MÓDULOS E CONECTIVIDADE");texto.AppendLine("Catálogo, comandas, consulta da cozinha, histórico, backup e licença offline.\nNão há IIS, API de integração ou Access Token nesta instalação.\nRelatório sem senhas, tokens ou chave de ativação.");return texto.ToString();
    }
}
