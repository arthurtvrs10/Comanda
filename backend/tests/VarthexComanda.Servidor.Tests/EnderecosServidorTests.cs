using System.Net.NetworkInformation;
using VarthexComanda.Servidor;
namespace VarthexComanda.Servidor.Tests;
public class EnderecosServidorTests
{
    private static InterfaceAcessoServidor Rede(string nome,string descricao,string ip,bool gateway=false,NetworkInterfaceType tipo=NetworkInterfaceType.Ethernet,bool ativa=true)=>new(nome,descricao,tipo,ativa,gateway,[ip]);
    [Fact] public void EthernetRealVemAntesDoHyperVSemEscolherIPFixo()
    {var result=EnderecosServidor.Selecionar([Rede("vEthernet (Default Switch)","Hyper-V Virtual Ethernet Adapter","172.22.64.1"),Rede("Ethernet","Intel Ethernet","192.168.15.115",true),Rede("Ethernet 2","VirtualBox Host-Only Ethernet Adapter","192.168.56.1")]);Assert.Equal(new[]{"192.168.15.115","127.0.0.1"},result);}
    [Theory][InlineData("vEthernet (WSL)","Hyper-V")][InlineData("VPN","WireGuard Tunnel")][InlineData("Ethernet 2","VMware Virtual Ethernet Adapter")][InlineData("Docker","Virtual adapter")]
    public void ApenasRedeVirtualNaoGeraQrInacessivel(string nome,string descricao)=>Assert.Equal(new[]{"127.0.0.1"},EnderecosServidor.Selecionar([Rede(nome,descricao,"172.19.0.1",true)]));
    [Fact] public void GatewayRealTemPrioridadeSobreWifiSemRota()
    {Assert.Equal("10.20.30.40",EnderecosServidor.Selecionar([Rede("Wi-Fi","Intel wireless","192.168.50.2",false,NetworkInterfaceType.Wireless80211),Rede("Ethernet","Realtek","10.20.30.40",true)])[0]);}
    [Fact] public void WifiTemPrioridadeSeAmbosTiveremGateway()
    {Assert.Equal("192.168.1.99",EnderecosServidor.Selecionar([Rede("Ethernet","Intel","192.168.1.50",true),Rede("Wi-Fi","Intel wireless","192.168.1.99",true,NetworkInterfaceType.Wireless80211)])[0]);}
    [Fact] public void PreservaRedeLocalSemGatewayMasIgnoraApipaPublicoInativoIpv6EDuplicados()
    {var result=EnderecosServidor.Selecionar([Rede("A","Intel","10.1.2.3"),Rede("B","Realtek","10.1.2.3"),Rede("C","Intel","169.254.1.1"),Rede("D","Intel","8.8.8.8"),Rede("E","Intel","192.168.0.7",false,NetworkInterfaceType.Ethernet,false),Rede("F","Intel","::1")]);Assert.Equal(new[]{"10.1.2.3","127.0.0.1"},result);}
    [Fact] public void DetectaRedeDesteComputadorSemPriorizarAdaptadorVirtual()
    {var ips=EnderecosServidor.Ler();Assert.NotEmpty(ips);Assert.Equal("127.0.0.1",ips[^1]);Assert.DoesNotContain("172.22.64.1",ips);}
}
