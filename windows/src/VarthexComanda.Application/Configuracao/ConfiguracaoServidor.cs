namespace VarthexComanda.Application.Configuracao;

public sealed record ConfiguracaoServidor(bool IniciarAutomaticamente = true, int Porta = 3001);

public sealed class ConfiguracaoServidorService(IConfiguracaoRepository repositorio)
{
    public const string ChaveAutomatica="servidor.iniciar_automaticamente";
    public const string ChavePorta="servidor.porta";
    public ConfiguracaoServidor Carregar()
    {
        var automatico=repositorio.ObterValor(ChaveAutomatica);
        var porta=repositorio.ObterValor(ChavePorta);
        if(automatico is not (null or "true" or "false"))throw new InvalidOperationException("A configuração de início do servidor é inválida.");
        if(porta is not null && (!int.TryParse(porta,out var valor)||valor is <1024 or >65535))throw new InvalidOperationException("A porta salva para o servidor é inválida.");
        return new(automatico!="false",porta is null?3001:int.Parse(porta));
    }
    public void Salvar(ConfiguracaoServidor configuracao,DateTime agora)
    {
        if(configuracao.Porta is <1024 or >65535)throw new ArgumentException("Informe uma porta entre 1024 e 65535.");
        repositorio.DefinirValores(new Dictionary<string,string>
        {
            [ChaveAutomatica]=configuracao.IniciarAutomaticamente?"true":"false",
            [ChavePorta]=configuracao.Porta.ToString(System.Globalization.CultureInfo.InvariantCulture)
        },agora);
    }
}
