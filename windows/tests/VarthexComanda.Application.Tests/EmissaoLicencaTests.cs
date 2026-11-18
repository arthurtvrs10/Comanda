using System.Security.Cryptography;
using VarthexComanda.Application.Licenciamento;
namespace VarthexComanda.Application.Tests;
public class EmissaoLicencaTests
{
    private static readonly DateOnly Hoje=new(2028,2,28);
    [Theory][InlineData(1,"2028-03-28")][InlineData(2,"2028-04-28")][InlineData(3,"2028-05-28")]
    public void MesesComecamHojeETerminamEmDataDeCalendario(int plano,string esperado)
    {
        var p=EmissaoLicenca.Calcular((PlanoLicenca)plano,Hoje);Assert.Equal("2028-02-28",p.Inicio.ToString("yyyy-MM-dd"));Assert.Equal(TimeSpan.FromHours(-3),p.Inicio.Offset);Assert.Equal(esperado,p.Vencimento!.Value.ToString("yyyy-MM-dd"));
    }
    [Fact] public void RenovacaoAntecipadaSomaAoPrazoEJaPodeSerUsada()
    {
        var atual=new DateOnly(2028,3,10);var p=EmissaoLicenca.Calcular(PlanoLicenca.Dias,Hoje,15,vencimentoAtual:atual);Assert.Equal(new DateTimeOffset(2028,3,25,0,0,0,TimeSpan.FromHours(-3)),p.Vencimento);
        using var rsa=RSA.Create(2048);var d=new DadosLicenca(1,"sintetica",new string('A',64),"Teste",p.Inicio,p.Vencimento);var token=LicencaOffline.Emitir(d,rsa.ExportPkcs8PrivateKeyPem());Assert.Equal(d,LicencaOffline.Validar(token,rsa.ExportSubjectPublicKeyInfoPem(),d.Computador,p.Inicio));Assert.Throws<InvalidOperationException>(()=>LicencaOffline.Validar(token,rsa.ExportSubjectPublicKeyInfoPem(),d.Computador,p.Vencimento!.Value));
    }
    [Fact] public void DataEscolhidaIncluiODiaInteiroEDiasRespeitamAnoBissexto()
    {
        Assert.Equal("2028-03-01",EmissaoLicenca.Calcular(PlanoLicenca.Dias,Hoje,2).Vencimento!.Value.ToString("yyyy-MM-dd"));
        Assert.Equal("2028-02-29",EmissaoLicenca.Calcular(PlanoLicenca.Data,Hoje,dataFinal:Hoje).Vencimento!.Value.ToString("yyyy-MM-dd"));
        Assert.Equal("2028-02-29",EmissaoLicenca.Calcular(PlanoLicenca.Dias,Hoje,1,vencimentoAtual:Hoje.AddDays(-1)).Vencimento!.Value.ToString("yyyy-MM-dd"));Assert.Null(EmissaoLicenca.Calcular(PlanoLicenca.Vitalicia,Hoje).Vencimento);
    }
    [Theory][InlineData(0)][InlineData(-1)][InlineData(36501)]public void DiasInvalidosSaoRecusados(int dias)=>Assert.Throws<ArgumentException>(()=>EmissaoLicenca.Calcular(PlanoLicenca.Dias,Hoje,dias));
    [Fact] public void FimDeMesEDataMaximaNaoProduzemLicencasInvalidas()
    {
        Assert.Equal("2028-02-29",EmissaoLicenca.Calcular(PlanoLicenca.UmMes,new(2028,1,31)).Vencimento!.Value.ToString("yyyy-MM-dd"));
        Assert.Throws<ArgumentException>(()=>EmissaoLicenca.Calcular(PlanoLicenca.Data,Hoje,dataFinal:Hoje.AddDays(-1)));Assert.Throws<ArgumentException>(()=>EmissaoLicenca.Calcular(PlanoLicenca.Data,Hoje,dataFinal:DateOnly.MaxValue));Assert.Throws<ArgumentException>(()=>EmissaoLicenca.Calcular((PlanoLicenca)99,Hoje));Assert.Throws<ArgumentException>(()=>EmissaoLicenca.Calcular(PlanoLicenca.UmMes,new(1999,12,31)));
    }
    [Fact] public void LeCodigoManualOuQrEstritoSemAceitarParametrosExtras()
    {
        var pc=new string('A',64);var qr="varthex://ativacao?v=1&maquina="+pc;Assert.Equal(pc,EmissaoLicenca.LerComputador(pc.ToLowerInvariant()));Assert.Equal(pc,EmissaoLicenca.LerComputador(" "+qr+" "));
        foreach(var invalido in new[]{qr+"&extra=1",qr.Replace("v=1","v=2"),qr.ToLowerInvariant(),"https://exemplo/"+pc,"ABC",""})Assert.Throws<ArgumentException>(()=>EmissaoLicenca.LerComputador(invalido));
    }
    [Fact] public void ConfereParDeChavesSemModificarChavePublicaDoProduto()
    {
        using var a=RSA.Create(2048);using var b=RSA.Create(2048);EmissaoLicenca.ConferirChave(a.ExportPkcs8PrivateKeyPem(),a.ExportSubjectPublicKeyInfoPem());Assert.Throws<ArgumentException>(()=>EmissaoLicenca.ConferirChave(b.ExportPkcs8PrivateKeyPem(),a.ExportSubjectPublicKeyInfoPem()));
    }
}
