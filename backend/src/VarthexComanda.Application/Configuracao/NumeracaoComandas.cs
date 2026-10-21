using System.Globalization;

namespace VarthexComanda.Application.Configuracao;

public sealed class ConfiguracaoNumeracaoComandas
{
    public bool Ativa { get; set; }
    public int Inicio { get; set; } = 100;
    public bool UsarComoSenhaAtendimento { get; set; }
    public int PrimeiroNumero => Ativa ? Inicio : 1;
}

public sealed class NumeracaoComandasService(IConfiguracaoRepository repositorio)
{
    public const string ChaveAtiva="comandas.numeracao.ativa";
    public const string ChaveInicio="comandas.numeracao.inicio";
    public const string ChaveSenha="comandas.numeracao.senha-atendimento";
    public ConfiguracaoNumeracaoComandas Carregar()
    {
        // Compatibilidade com a preferência já preenchida na janela anterior.
        var ativa=repositorio.ObterValor(ChaveAtiva)??repositorio.ObterValor(NumeracaoPedidosService.ChaveAtiva);
        var inicio=NumeracaoPedidosService.LerNumero(repositorio.ObterValor(ChaveInicio)??repositorio.ObterValor(NumeracaoPedidosService.ChaveInicio),100);
        if(inicio<1)throw new InvalidOperationException("Informe um número inicial de comanda maior que zero.");
        return new() { Ativa=ativa=="true",Inicio=(int)inicio,UsarComoSenhaAtendimento=(repositorio.ObterValor(ChaveSenha)??repositorio.ObterValor(NumeracaoPedidosService.ChaveSenhaAtendimento))=="true" };
    }
    public void Salvar(ConfiguracaoNumeracaoComandas configuracao,DateTime agora)
    {
        if(configuracao.Inicio is <1 or >999_999_999)throw new ArgumentException("Informe um número inicial de 1 a 999.999.999.");
        var quantidade=int.TryParse(repositorio.ObterValor("comandas.quantidade_maxima"),out var q)?q:20;
        ValidarFaixa(configuracao.PrimeiroNumero,quantidade);
        repositorio.DefinirValores(new Dictionary<string,string>
        {
            [ChaveAtiva]=configuracao.Ativa?"true":"false",[ChaveInicio]=configuracao.Inicio.ToString(CultureInfo.InvariantCulture),
            [ChaveSenha]=configuracao.UsarComoSenhaAtendimento?"true":"false",
            // Este menu controla a grade; não aloca um segundo contador para novos pedidos.
            [NumeracaoPedidosService.ChaveAtiva]="false"
        },agora);
    }
    public static void ValidarFaixa(int inicio,int quantidade)
    {
        if(inicio<1||quantidade<1||(long)inicio+quantidade-1>999_999_999)
            throw new ArgumentException("A faixa de comandas deve ficar entre 1 e 999.999.999. Reduza o início ou a quantidade.");
    }
}
