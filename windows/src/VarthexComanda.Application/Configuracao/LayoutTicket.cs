using System.Text.Json;

namespace VarthexComanda.Application.Configuracao;

public sealed class LayoutTicket
{
    public bool ImprimirLogo {get;set;}
    public string LogoBase64 {get;set;}="";
    public bool NomeEstabelecimento {get;set;}=true;
    public bool NumeroMesa {get;set;}=true;
    public bool NomeCliente {get;set;}=true;
    public bool DataHora {get;set;}=true;
    public bool ObservacoesItem {get;set;}=true;
    public bool ObservacoesPedido {get;set;}=true;
    public bool CodigoPedido {get;set;}=true;
    public int PapelMm {get;set;}=80;
}

public sealed class LayoutTicketService(IConfiguracaoRepository repositorio)
{
    public const string Chave="impressoras.layout-ticket.v1";
    public LayoutTicket? Carregar()
    {
        if(repositorio.ObterValor(Chave) is not string json)return null;
        var layout=JsonSerializer.Deserialize<LayoutTicket>(json);
        if(layout is not null)layout.ObservacoesItem=Preferencias.ExibirObservacoesCupom;
        return layout;
    }
    public string NomeEstabelecimento=>repositorio.ObterValor("estabelecimento.nome")??"";
    public PreferenciasSistema Preferencias=>new ConfiguracoesSistemaService(repositorio).Carregar();
    public void Salvar(LayoutTicket layout,PreferenciasSistema? cupom=null)
    {
        Validar(layout);
        var prefs=Preferencias;
        if(cupom is not null)CopiarCupom(cupom,prefs);
        prefs.ExibirObservacoesCupom=layout.ObservacoesItem;
        ConfiguracoesSistemaService.Validar(prefs);
        var impressoras=new ConfiguracoesImpressaoService(repositorio).Carregar();impressoras.Caixa.LarguraPapelMm=layout.PapelMm;
        var valores=new Dictionary<string,string> {[Chave]=JsonSerializer.Serialize(layout),[ConfiguracoesSistemaService.Chave]=JsonSerializer.Serialize(prefs)};
        if(repositorio.ObterValor(ConfiguracoesImpressaoService.Chave) is not null)valores[ConfiguracoesImpressaoService.Chave]=JsonSerializer.Serialize(impressoras);
        repositorio.DefinirValores(valores,DateTime.UtcNow);
    }
    public static void CopiarCupom(PreferenciasSistema origem,PreferenciasSistema destino)
    {
        destino.AgruparItensCupom=origem.AgruparItensCupom;destino.ExibirObservacoesCupom=origem.ExibirObservacoesCupom;
        destino.ExibirDocumentoCupom=origem.ExibirDocumentoCupom;destino.ExibirEnderecoCupom=origem.ExibirEnderecoCupom;
        destino.CabecalhoCupom=origem.CabecalhoCupom;destino.RodapeCupom=origem.RodapeCupom;
    }
    public static void Validar(LayoutTicket layout)
    {
        if(layout.PapelMm is not (58 or 80))throw new ArgumentException("Selecione papel de 58 ou 80 mm.");
        if(layout.LogoBase64.Length>2800000)throw new ArgumentException("Use uma imagem de até 2 MB.");
        if(layout.ImprimirLogo&&string.IsNullOrWhiteSpace(layout.LogoBase64))throw new ArgumentException("Escolha a logo antes de habilitar sua impressão.");
        if(layout.LogoBase64.Length>0)try{Convert.FromBase64String(layout.LogoBase64);}catch(FormatException){throw new ArgumentException("A imagem da logo é inválida.");}
    }
}
