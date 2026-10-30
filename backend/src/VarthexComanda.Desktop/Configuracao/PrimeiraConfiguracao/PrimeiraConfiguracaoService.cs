using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VarthexComanda.Application.Configuracao;
using VarthexComanda.Domain;
using VarthexComanda.Infrastructure.Persistence;
using VarthexComanda.Infrastructure.Storage;

namespace VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

public sealed partial class PrimeiraConfiguracaoService(IDbContextFactory<VarthexComandaDbContext> fabrica,IConfiguracaoRepository configuracoes,AppPaths paths)
{
    public const string ChaveConclusao="primeira-configuracao.concluida.v1";
    public const string ChavePagamentos="primeira-configuracao.pagamentos.v1";
    public string ArquivoRascunho=>Path.Combine(paths.Root,"primeira-configuracao.rascunho.json");
    public bool ConfiguracaoPendente=>configuracoes.ObterValor(ChaveConclusao)!="1";
    public bool DeveAbrir()
    {
        if(configuracoes.ObterValor(ChaveConclusao)=="1")return false;
        if(File.Exists(ArquivoRascunho))return true;
        using var db=fabrica.CreateDbContext();
        return string.IsNullOrWhiteSpace(configuracoes.ObterValor("estabelecimento.nome"))&&!db.Produtos.Any()&&!db.Comandas.Any();
    }
    public EstadoConfiguracao Carregar()
    {
        if(configuracoes.ObterValor(ChaveConclusao)!="1"&&File.Exists(ArquivoRascunho))
        {
            var rascunho=JsonSerializer.Deserialize<EstadoConfiguracao>(File.ReadAllText(ArquivoRascunho))??throw new InvalidOperationException("Não foi possível ler o rascunho da configuração inicial.");
            SincronizarEstabelecimento(rascunho);
            return rascunho;
        }
        var existente=new ObterConfiguracao(configuracoes).Executar();
        var prefs=new ConfiguracoesSistemaService(configuracoes).Carregar();
        if(string.IsNullOrWhiteSpace(prefs.Documento))prefs.ExibirDocumentoCupom=false;
        if(string.IsNullOrWhiteSpace(prefs.Endereco))prefs.ExibirEnderecoCupom=false;
        var pagamentos=configuracoes.ObterValor(ChavePagamentos);
        var estado=new EstadoConfiguracao {Nome=existente.NomeEstabelecimento,Cupom=prefs,TipoDocumento=TipoDocumento(prefs),QuantidadeComandas=existente.QuantidadeMaximaComandas??30,Pagamentos=pagamentos is null?["Dinheiro","Pix","Débito","Crédito"]:JsonSerializer.Deserialize<List<string>>(pagamentos)??[]};
        estado.Papel=new LayoutTicketService(configuracoes).Carregar()?.PapelMm??80;
        estado.BaseEstabelecimento=DadosEstabelecimento(LerEstabelecimentoAtual());
        return estado;
    }
    public void SalvarRascunho(EstadoConfiguracao estado)
    {
        Directory.CreateDirectory(paths.Root);var temporario=ArquivoRascunho+".tmp";File.WriteAllText(temporario,estado.Json());File.Move(temporario,ArquivoRascunho,true);
    }
    public void Concluir(EstadoConfiguracao estado)
    {
        if(estado.BaseEstabelecimento is not null)SincronizarEstabelecimento(estado);
        for(var i=0;i<6;i++)if(estado.Erro(i) is {Length:>0} erro)throw new ArgumentException(erro);
        if(estado.Nome.Trim().Length>120)throw new ArgumentException("O nome do estabelecimento deve ter até 120 caracteres.");
        ConfiguracoesSistemaService.Validar(estado.Cupom);
        var formasValidas=new[]{"Dinheiro","Pix","Débito","Crédito"};
        if(estado.Pagamentos.Count==0||estado.Pagamentos.Any(f=>!formasValidas.Contains(f)))throw new ArgumentException("Selecione formas de pagamento válidas.");
        // O assistente edita somente estabelecimento e cupom. Um rascunho antigo
        // não pode sobrescrever segurança, cozinha ou aparência alteradas depois.
        var preferencias=new ConfiguracoesSistemaService(configuracoes).Carregar();var editadas=estado.Cupom;
        preferencias.TipoDocumento=estado.TipoDocumento;preferencias.PaisTelefone=editadas.PaisTelefone;preferencias.RazaoSocial=editadas.RazaoSocial;preferencias.Documento=editadas.Documento;preferencias.InscricaoEstadual=editadas.InscricaoEstadual;preferencias.Telefone=editadas.Telefone;
        preferencias.Endereco=editadas.Endereco;preferencias.Numero=editadas.Numero;preferencias.Complemento=editadas.Complemento;preferencias.Bairro=editadas.Bairro;preferencias.Cep=editadas.Cep;preferencias.Cidade=editadas.Cidade;preferencias.Uf=editadas.Uf;
        preferencias.ExibirDocumentoCupom=editadas.ExibirDocumentoCupom;preferencias.ExibirEnderecoCupom=editadas.ExibirEnderecoCupom;preferencias.ExibirObservacoesCupom=editadas.ExibirObservacoesCupom;preferencias.AgruparItensCupom=editadas.AgruparItensCupom;preferencias.CabecalhoCupom=editadas.CabecalhoCupom;preferencias.RodapeCupom=editadas.RodapeCupom;
        var inicio=new NumeracaoComandasService(configuracoes).Carregar().PrimeiroNumero;NumeracaoComandasService.ValidarFaixa(inicio,estado.QuantidadeComandas);
        using var db=fabrica.CreateDbContext();using var tx=db.Database.BeginTransaction();
        if(db.Comandas.Any(c=>c.Status==StatusComanda.Aberta&&c.Numero>inicio+estado.QuantidadeComandas-1))throw new InvalidOperationException("Há uma comanda aberta fora da quantidade escolhida. Mantenha a faixa atual ou encerre esse atendimento.");
        var agora=DateTime.UtcNow;
        var produtos=db.Produtos.ToList();
        foreach(var item in estado.Produtos)
        {
            if(string.IsNullOrWhiteSpace(item.Nome)||item.Nome.Length>120||item.Centavos<=0||item.Unidade is not ("UN" or "KG"))throw new ArgumentException("Confira o nome, a unidade e o preço dos produtos.");
            var unidade=item.Unidade=="KG"?UnidadeVenda.Quilo:UnidadeVenda.Unidade;
            var existente=produtos.FirstOrDefault(p=>p.Nome.Equals(item.Nome.Trim(),StringComparison.OrdinalIgnoreCase));
            if(existente is not null){if(existente.PrecoCentavos!=item.Centavos||existente.UnidadeVenda!=unidade)throw new InvalidOperationException($"O produto {item.Nome} já existe com outro preço ou unidade. Revise no catálogo antes de continuar.");continue;}
            var categoria=db.Categorias.Local.FirstOrDefault(c=>c.Nome=="Produtos")??db.Categorias.FirstOrDefault(c=>c.Nome=="Produtos");
            if(categoria is null){categoria=new Categoria {Id=0,Nome="Produtos",Ativo=true,CriadoEm=agora,AtualizadoEm=agora};db.Categorias.Add(categoria);db.SaveChanges();}
            if(!categoria.Ativo)throw new InvalidOperationException("A categoria Produtos está inativa. Ative-a ou cadastre seus produtos pelo catálogo.");
            var produto=new Produto {Id=0,Nome=item.Nome.Trim(),CategoriaId=categoria.Id,PrecoCentavos=item.Centavos,UnidadeVenda=unidade,Ativo=true,CriadoEm=agora,AtualizadoEm=agora};db.Produtos.Add(produto);produtos.Add(produto);
        }
        void Definir(string chave,string valor){var atual=db.Configuracoes.SingleOrDefault(c=>c.Chave==chave);if(atual is null)db.Configuracoes.Add(new Domain.Configuracao {Chave=chave,Valor=valor,AtualizadoEm=agora});else{atual.Valor=valor;atual.AtualizadoEm=agora;}}
        Definir(ConfiguracoesSistemaService.Chave,JsonSerializer.Serialize(preferencias));Definir("estabelecimento.nome",estado.Nome.Trim());Definir("comandas.quantidade_maxima",estado.QuantidadeComandas.ToString());Definir(ChavePagamentos,JsonSerializer.Serialize(estado.Pagamentos.Distinct()));Definir("primeira-configuracao.responsavel",estado.Responsavel.Trim());Definir(ChaveConclusao,"1");
        var layout=new LayoutTicketService(configuracoes).Carregar()??new();layout.ObservacoesItem=preferencias.ExibirObservacoesCupom;layout.PapelMm=estado.Papel;
        LayoutTicketService.Validar(layout);Definir(LayoutTicketService.Chave,JsonSerializer.Serialize(layout));
        var impressoras=new ConfiguracoesImpressaoService(configuracoes).Carregar();impressoras.Caixa.LarguraPapelMm=estado.Papel;if(configuracoes.ObterValor(ConfiguracoesImpressaoService.Chave) is not null)Definir(ConfiguracoesImpressaoService.Chave,JsonSerializer.Serialize(impressoras));
        db.SaveChanges();tx.Commit();
        try{if(File.Exists(ArquivoRascunho))File.Delete(ArquivoRascunho);}catch(IOException){}catch(UnauthorizedAccessException){}
    }
}
