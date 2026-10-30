using System.Globalization;
using System.Text.Json;
using VarthexComanda.Application.Configuracao;

namespace VarthexComanda.Desktop.Configuracao.PrimeiraConfiguracao;

public sealed record ProdutoInicial(string Nome,string Unidade,long Centavos)
{
    public string Preco=>(Centavos/100m).ToString("C",CultureInfo.GetCultureInfo("pt-BR"));
}
public sealed record ColaboradorInicial(string Nome,string Funcao);
public sealed class EstadoConfiguracao
{
    public string Nome { get;set; }="";
    public Dictionary<string,string>? BaseEstabelecimento { get;set; }
    public string TipoDocumento { get;set; }="CNPJ";
    public string DocumentoCpf { get;set; }="";
    public string DocumentoCnpj { get;set; }="";
    public PreferenciasSistema Cupom { get;set; }=new(){ExibirDocumentoCupom=false,ExibirEnderecoCupom=false};
    public bool Balcao { get;set; }=true;
    public bool Mesas { get;set; }
    public int QuantidadeMesas { get;set; }=10;
    public int QuantidadeComandas { get;set; }=30;
    public int Papel { get;set; }=80;
    public string Responsavel { get;set; }="";
    public List<ProdutoInicial> Produtos { get;set; }=[];
    public List<ColaboradorInicial> Colaboradores { get;set; }=[];
    public List<string> Pagamentos { get;set; }=["Dinheiro","Pix","Débito","Crédito"];
    public HashSet<int> Concluidas { get;set; }=[];
    public int Etapa { get;set; }
    public bool Finalizado { get;set; }
    public string Erro(int etapa)
    {
        if(etapa==0)
        {
            if(string.IsNullOrWhiteSpace(Nome))return "Informe o nome fantasia do estabelecimento.";
            var documento=new string((Cupom.Documento??"").Where(char.IsDigit).ToArray());
            if(documento.Length!=0 && documento.Length!=(TipoDocumento=="CPF"?11:14))return TipoDocumento=="CPF"?"Informe os 11 números do CPF.":"Informe os 14 números do CNPJ.";
            if(Cupom.ExibirDocumentoCupom && documento.Length==0)return "Informe o CNPJ / CPF ou desmarque sua exibição na etapa Comanda impressa.";
            if(Cupom.ExibirEnderecoCupom && string.IsNullOrWhiteSpace(Cupom.Endereco))return "Informe o endereço ou desmarque sua exibição na etapa Comanda impressa.";
        }
        if(etapa==1)
        {
            if(Cupom.ExibirDocumentoCupom && string.IsNullOrWhiteSpace(Cupom.Documento))return "Preencha o CNPJ / CPF em Estabelecimento para exibi-lo no cupom.";
            if(Cupom.ExibirEnderecoCupom && string.IsNullOrWhiteSpace(Cupom.Endereco))return "Preencha o endereço em Estabelecimento para exibi-lo no cupom.";
        }
        if(etapa==2)
        {
            if(!Balcao&&!Mesas)return "Selecione atendimento em balcão ou mesas.";
            if(QuantidadeComandas is <1 or >500)return "Informe de 1 a 500 comandas.";
            if(Mesas && QuantidadeMesas is <1 or >300)return "Informe de 1 a 300 mesas.";
        }
        if(etapa==4&&Pagamentos.Count==0)return "Selecione pelo menos uma forma de pagamento.";
        return "";
    }
    public int Progresso=>Finalizado?100:(int)Math.Round(Concluidas.Count(i=>i is >=0 and <6 && Erro(i)=="")/7d*100);
    public int PrimeiraPendente { get {for(var i=0;i<6;i++)if(!Concluidas.Contains(i)||Erro(i)!="")return i;return 6;} }
    public bool PodeAcessar(int etapa)=>etapa is >=0 and <=6 && (Finalizado||etapa<=PrimeiraPendente);
    public static EstadoConfiguracao Exemplo()=>new() { Nome="Restaurante de demonstração",Cupom=new(){Documento="00.000.000/0000-00",Endereco="Rua de exemplo",Numero="100",Cidade="Cidade de teste",Uf="SP",ExibirDocumentoCupom=true,ExibirEnderecoCupom=true},Mesas=true,Produtos=[new("Refeição no quilo","KG",4990),new("Suco natural","UN",800)],Responsavel="Responsável de demonstração" };
    public static long? Preco(string texto)=>decimal.TryParse(texto,NumberStyles.AllowDecimalPoint,CultureInfo.GetCultureInfo("pt-BR"),out var valor)&&valor>0&&valor<=1000000&&decimal.Round(valor,2)==valor?(long)(valor*100):null;
    public string Json()=>JsonSerializer.Serialize(this,new JsonSerializerOptions { WriteIndented=true });
}
