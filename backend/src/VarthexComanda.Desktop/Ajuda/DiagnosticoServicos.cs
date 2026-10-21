using System.IO;
using Microsoft.Data.Sqlite;
using VarthexComanda.Infrastructure.Storage;
using VarthexComanda.Desktop.Configuracao;

namespace VarthexComanda.Desktop.Ajuda;
public sealed record VerificacaoServico(string Servico,string Estado,string Detalhe);
public static class DiagnosticoServicos
{
    public static IReadOnlyList<VerificacaoServico> Avaliar(AppPaths paths,Func<bool> verificarLicenca,Func<IReadOnlyList<ImpressoraWindows>>? consultarImpressoras=null)
    {
        var resultados=new List<VerificacaoServico>();
        void Testar(string nome,Func<VerificacaoServico> teste){try{resultados.Add(teste());}catch{resultados.Add(new(nome,"Falha","Não foi possível concluir a verificação. Confira os dados, as permissões ou a configuração deste serviço."));}}
        Testar("Banco de dados",()=>
        {
            if(!File.Exists(paths.DatabasePath))return new("Banco de dados","Falha","O arquivo do banco não foi encontrado. Use Backup e restauração.");
            using var conexao=new SqliteConnection(new SqliteConnectionStringBuilder { DataSource=paths.DatabasePath,Mode=SqliteOpenMode.ReadOnly,DefaultTimeout=5,Pooling=false }.ToString());conexao.Open();
            using var comando=conexao.CreateCommand();comando.CommandText="PRAGMA quick_check(1)";
            if(!string.Equals(comando.ExecuteScalar()?.ToString(),"ok",StringComparison.OrdinalIgnoreCase))return new("Banco de dados","Falha","A verificação de integridade encontrou um problema. Preserve o arquivo e procure o suporte.");
            comando.CommandText="SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('comanda','produto','item_comanda','venda','configuracao')";
            return Convert.ToInt32(comando.ExecuteScalar())==5?new("Banco de dados","Funcionando","Conexão, integridade e tabelas principais verificadas em modo somente leitura."):new("Banco de dados","Falha","As tabelas principais não foram encontradas. Confira a instalação ou restaure um backup.");
        });
        foreach(var (nome,pasta) in new[]{("Dados locais",paths.DataDirectory),("Backups locais",paths.BackupsDirectory),("Fotos de produtos",paths.FotosDirectory),("Registros do sistema",paths.LogsDirectory)})
            Testar(nome,()=>
            {
                if(!Directory.Exists(pasta))return new(nome,"Falha","A pasta necessária não foi encontrada.");
                var temporario=Path.Combine(pasta,".verificacao-"+Guid.NewGuid().ToString("N")+".tmp");
                using(var arquivo=new FileStream(temporario,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose)){arquivo.WriteByte(1);arquivo.Flush();arquivo.Position=0;if(arquivo.ReadByte()!=1)throw new IOException();}
                return new(nome,"Funcionando","Leitura e gravação verificadas com arquivo temporário removido ao concluir.");
            });
        Testar("Assinatura",()=>verificarLicenca()?new("Assinatura","Funcionando","Assinatura validada neste computador."):new("Assinatura","Atenção","Não foi possível validar a assinatura. Consulte Minha assinatura."));
        Testar("Impressão do Windows",()=>
        {
            var filas=(consultarImpressoras??ImpressaoWindows.Consultar)();
            return filas.Count>0?new("Impressão do Windows","Funcionando",$"{filas.Count} fila(s) acessível(is). Isso confirma o serviço do Windows; use Testar impressora para conferir a saída física."):new("Impressão do Windows","Atenção","Nenhuma fila encontrada. Instale ou configure uma impressora para imprimir.");
        });
        resultados.Add(new("Serviços online","Não utilizado","A operação local do Varthex não depende de um servidor online. WhatsApp e download do RustDesk precisam de internet."));
        return resultados;
    }
}
