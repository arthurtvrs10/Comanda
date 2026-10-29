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
    public LayoutTicket? Carregar()=>repositorio.ObterValor(Chave) is string json?JsonSerializer.Deserialize<LayoutTicket>(json):null;
    public string NomeEstabelecimento=>repositorio.ObterValor("estabelecimento.nome")??"";
    public PreferenciasSistema Preferencias=>new ConfiguracoesSistemaService(repositorio).Carregar();
    public void Salvar(LayoutTicket layout)
    {
        Validar(layout);
        repositorio.Definir(Chave,JsonSerializer.Serialize(layout),DateTime.UtcNow);
    }
    public static void Validar(LayoutTicket layout)
    {
        if(layout.PapelMm is not (58 or 80))throw new ArgumentException("Selecione papel de 58 ou 80 mm.");
        if(layout.LogoBase64.Length>2800000)throw new ArgumentException("Use uma imagem de até 2 MB.");
        if(layout.ImprimirLogo&&string.IsNullOrWhiteSpace(layout.LogoBase64))throw new ArgumentException("Escolha a logo antes de habilitar sua impressão.");
        if(layout.LogoBase64.Length>0)try{Convert.FromBase64String(layout.LogoBase64);}catch(FormatException){throw new ArgumentException("A imagem da logo é inválida.");}
    }
}
