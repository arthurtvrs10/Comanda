using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

public sealed partial class PrimeiraConfiguracaoService
{
    private static string TipoDocumento(PreferenciasSistema p)=>p.TipoDocumento is "CPF" or "CNPJ"?p.TipoDocumento:Mascaras.Digitos(p.Documento).Length==11?"CPF":"CNPJ";
    private static readonly (string Chave,Func<EstadoConfiguracao,string> Ler,Action<EstadoConfiguracao,string> Escrever)[] CamposGerais=
    [
        ("Nome",e=>e.Nome,(e,v)=>e.Nome=v),
        ("TipoDocumento",e=>e.TipoDocumento,(e,v)=>{e.TipoDocumento=v;e.Cupom.TipoDocumento=v;}),
        ("RazaoSocial",e=>e.Cupom.RazaoSocial,(e,v)=>e.Cupom.RazaoSocial=v),
        ("Documento",e=>e.Cupom.Documento,(e,v)=>e.Cupom.Documento=v),
        ("InscricaoEstadual",e=>e.Cupom.InscricaoEstadual,(e,v)=>e.Cupom.InscricaoEstadual=v),
        ("PaisTelefone",e=>e.Cupom.PaisTelefone,(e,v)=>e.Cupom.PaisTelefone=v),
        ("Telefone",e=>e.Cupom.Telefone,(e,v)=>e.Cupom.Telefone=v),
        ("Endereco",e=>e.Cupom.Endereco,(e,v)=>e.Cupom.Endereco=v),
        ("Numero",e=>e.Cupom.Numero,(e,v)=>e.Cupom.Numero=v),
        ("Complemento",e=>e.Cupom.Complemento,(e,v)=>e.Cupom.Complemento=v),
        ("Bairro",e=>e.Cupom.Bairro,(e,v)=>e.Cupom.Bairro=v),
        ("Cep",e=>e.Cupom.Cep,(e,v)=>e.Cupom.Cep=v),
        ("Cidade",e=>e.Cupom.Cidade,(e,v)=>e.Cupom.Cidade=v),
        ("Uf",e=>e.Cupom.Uf,(e,v)=>e.Cupom.Uf=v),
      ("AgruparItensCupom",e=>e.Cupom.AgruparItensCupom.ToString(),(e,v)=>e.Cupom.AgruparItensCupom=bool.Parse(v)),
      ("ExibirObservacoesCupom",e=>e.Cupom.ExibirObservacoesCupom.ToString(),(e,v)=>e.Cupom.ExibirObservacoesCupom=bool.Parse(v)),
      ("ExibirDocumentoCupom",e=>e.Cupom.ExibirDocumentoCupom.ToString(),(e,v)=>e.Cupom.ExibirDocumentoCupom=bool.Parse(v)),
      ("ExibirEnderecoCupom",e=>e.Cupom.ExibirEnderecoCupom.ToString(),(e,v)=>e.Cupom.ExibirEnderecoCupom=bool.Parse(v)),
      ("CabecalhoCupom",e=>e.Cupom.CabecalhoCupom,(e,v)=>e.Cupom.CabecalhoCupom=v),
      ("RodapeCupom",e=>e.Cupom.RodapeCupom,(e,v)=>e.Cupom.RodapeCupom=v)
    ];
    private static Dictionary<string,string> DadosEstabelecimento(EstadoConfiguracao estado)=>CamposGerais.ToDictionary(c=>c.Chave,c=>c.Ler(estado)??"");
    private EstadoConfiguracao LerEstabelecimentoAtual()
    {
        var p=new ConfiguracoesSistemaService(configuracoes).Carregar();
        return new(){Nome=new ObterConfiguracao(configuracoes).Executar().NomeEstabelecimento,Cupom=p,TipoDocumento=TipoDocumento(p)};
    }
    private void SincronizarEstabelecimento(EstadoConfiguracao estado)
    {
        var atual=LerEstabelecimentoAtual();var baseAnterior=estado.BaseEstabelecimento;var dados=DadosEstabelecimento(atual);
        var temCadastro=!string.IsNullOrWhiteSpace(atual.Nome)||!string.IsNullOrWhiteSpace(atual.Cupom.Documento);
        foreach(var campo in CamposGerais)
        {
            var salvo=dados[campo.Chave];
            // Campos alterados em Geral depois do rascunho prevalecem. Os demais
            // mantêm a edição ainda não confirmada do assistente.
            if(baseAnterior is not null?baseAnterior.GetValueOrDefault(campo.Chave)!=salvo:temCadastro)
                campo.Escrever(estado,salvo);
        }
        estado.BaseEstabelecimento=dados;
    }
    public void SalvarEstabelecimento(EstadoConfiguracao estado)
    {
        if(estado.BaseEstabelecimento is not null)SincronizarEstabelecimento(estado);
        if(estado.Erro(0) is {Length:>0} erro)throw new ArgumentException(erro);
        var atual=LerEstabelecimentoAtual();
        foreach(var campo in CamposGerais)campo.Escrever(atual,campo.Ler(estado));
        var quantidade=new ObterConfiguracao(configuracoes).Executar().QuantidadeMaximaComandas;
        new ConfiguracoesSistemaService(configuracoes).Salvar(atual.Cupom,atual.Nome,quantidade,DateTime.UtcNow);
        estado.Nome=atual.Nome.Trim();estado.BaseEstabelecimento=DadosEstabelecimento(atual);
    }
}
