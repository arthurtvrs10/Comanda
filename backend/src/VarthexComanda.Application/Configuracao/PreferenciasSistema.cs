using System.Text.Json;

namespace VarthexComanda.Application.Configuracao;

public sealed class PreferenciasSistema
{
    public bool SolicitarSenha { get; set; }
    public string RazaoSocial { get; set; } = "";
    public string Documento { get; set; } = "";
    public string InscricaoEstadual { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Endereco { get; set; } = "";
    public string Numero { get; set; } = "";
    public string Complemento { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Cep { get; set; } = "";
    public string Cidade { get; set; } = "";
    public string Uf { get; set; } = "";
    public bool AgruparItensCupom { get; set; } = true;
    public bool ExibirObservacoesCupom { get; set; } = true;
    public bool ExibirEnderecoCupom { get; set; } = true;
    public bool ExibirDocumentoCupom { get; set; } = true;
    public string CabecalhoCupom { get; set; } = "CONFERÊNCIA DA CONTA\nCOMPROVANTE NÃO FISCAL";
    public string RodapeCupom { get; set; } = "Obrigado pela preferência!";
    public const string CorFundoPadrao = "#F2F2F2";
    public string CorFundo { get; set; } = CorFundoPadrao;
    public string ImagemFundo { get; set; } = "";
    public string PosicaoImagem { get; set; } = "Centralizar";
    public List<string> OrigensCliente { get; set; } = ["Indefinido", "Indicação de amigo", "Google", "Instagram"];
    public List<LocalCozinha> Cozinhas { get; set; } = [new()];
    public string PrefixoCodigoPeso { get; set; } = "2";
    public bool CodigoPesoAtivo { get; set; }
    public int DigitosProdutoPeso { get; set; } = 5;
}
public sealed class LocalCozinha
{
    public string Nome { get; set; } = "Cozinha principal";
    public bool Imprimir { get; set; }
    public string Impressora { get; set; } = "Selecionar ao imprimir";
    [System.Text.Json.Serialization.JsonIgnore]
    public string SaidaDescricao => Imprimir ? Impressora : "Somente monitor";
    public List<int> Produtos { get; set; } = [];
    public override string ToString() => Nome;
}
public sealed class ConfiguracoesSistemaService(IConfiguracaoRepository repositorio)
{
    public const string Chave = "sistema.preferencias.v1";
    public PreferenciasSistema Carregar()
    {
        var json = repositorio.ObterValor(Chave);
        if (string.IsNullOrWhiteSpace(json)) return new();
        var valor = JsonSerializer.Deserialize<PreferenciasSistema>(json) ?? throw new InvalidOperationException("Preferências do sistema inválidas.");
        Validar(valor);
        return valor;
    }
    public void Salvar(PreferenciasSistema preferencias, string nome, int? quantidade, DateTime agora)
    {
        Validar(preferencias);
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 120) throw new ArgumentException("Informe o nome do estabelecimento com até 120 caracteres.");
        if (quantidade is <= 0 or > 9999) throw new ArgumentException("Informe de 1 a 9999 comandas ou deixe vazio.");
        NumeracaoComandasService.ValidarFaixa(new NumeracaoComandasService(repositorio).Carregar().PrimeiroNumero,quantidade ?? 20);
        repositorio.DefinirValores(new Dictionary<string,string>
        {
            [Chave]=JsonSerializer.Serialize(preferencias),
            ["estabelecimento.nome"]=nome.Trim(),
            ["comandas.quantidade_maxima"]=quantidade?.ToString() ?? ""
        },agora);
    }
    public static void Validar(PreferenciasSistema p)
    {
        if (p.OrigensCliente is null || p.Cozinhas is null || p.Cozinhas.Count==0) throw new ArgumentException("Cadastre pelo menos uma cozinha.");
        if (p.OrigensCliente.Any(s=>string.IsNullOrWhiteSpace(s)||s.Length>80) || p.OrigensCliente.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=p.OrigensCliente.Count) throw new ArgumentException("As origens devem ter nomes únicos de até 80 caracteres.");
        if (p.Cozinhas.Any(c=>string.IsNullOrWhiteSpace(c.Nome)||c.Nome.Length>80||c.Produtos is null) || p.Cozinhas.Select(c=>c.Nome).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=p.Cozinhas.Count) throw new ArgumentException("As cozinhas devem ter nomes únicos de até 80 caracteres.");
        if (p.DigitosProdutoPeso is <4 or >6 || p.PrefixoCodigoPeso.Length!=1 || !char.IsAsciiDigit(p.PrefixoCodigoPeso[0])) throw new ArgumentException("Código por Kg: informe um prefixo numérico e 4, 5 ou 6 dígitos de produto.");
        if (p.CabecalhoCupom.Length>500 || p.RodapeCupom.Length>500) throw new ArgumentException("Mensagens do cupom devem ter até 500 caracteres.");
        if (p.PosicaoImagem is not ("Centralizar" or "Preencher" or "Zoom" or "Repetir")) throw new ArgumentException("Selecione uma posição de imagem válida.");
        if (p.CorFundo.Length!=7 || p.CorFundo[0]!='#' || !p.CorFundo[1..].All(char.IsAsciiHexDigit)) throw new ArgumentException("Use uma cor no formato #RRGGBB.");
    }
    public static (int Produto, long ValorCentavos) InterpretarEtiqueta(string codigo, PreferenciasSistema p)
    {
        Validar(p);
        if(codigo.Length!=13 || !codigo.All(char.IsAsciiDigit) || !codigo.StartsWith(p.PrefixoCodigoPeso)) throw new ArgumentException("Informe uma etiqueta EAN-13 compatível com o prefixo configurado.");
        int soma=0;
        for(int i=0;i<12;i++)soma+=(codigo[i]-'0')*(i%2==0?1:3);
        if((10-soma%10)%10!=codigo[12]-'0')throw new ArgumentException("Dígito verificador inválido.");
        return (int.Parse(codigo.Substring(1,p.DigitosProdutoPeso)),long.Parse(codigo.Substring(1+p.DigitosProdutoPeso,11-p.DigitosProdutoPeso)));
    }
}
