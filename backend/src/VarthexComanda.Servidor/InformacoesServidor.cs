using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace VarthexComanda.Servidor;

public sealed record AcessoServidor(string Endereco,DateTime UltimoAcesso,int Requisicoes);
public sealed record RedeServidor(string Nome,string Tipo,string Endereco,string Situacao);

public static class InformacoesServidor
{
    public static IReadOnlyList<RedeServidor> LerRedes()
    {
        var redes=new List<RedeServidor>();
        foreach(var rede in NetworkInterface.GetAllNetworkInterfaces().Where(r=>r.NetworkInterfaceType!=NetworkInterfaceType.Loopback))
        {
            try
            {
                var ips=rede.GetIPProperties().UnicastAddresses.Where(a=>a.Address.AddressFamily==AddressFamily.InterNetwork).Select(a=>a.Address.ToString()).ToArray();
                var tipo=rede.NetworkInterfaceType==NetworkInterfaceType.Wireless80211?"Wi-Fi":rede.NetworkInterfaceType==NetworkInterfaceType.Ethernet?"Ethernet":"Outra rede";
                var situacao=rede.OperationalStatus!=OperationalStatus.Up?"Sem conexão":ips.Length==0?"Sem IPv4":"Conectada";
                redes.Add(new(rede.Name,tipo,ips.Length==0?"—":string.Join(", ",ips),situacao));
            }
            catch(NetworkInformationException){redes.Add(new(rede.Name,"Rede","—","Indisponível"));}
        }
        return redes.OrderByDescending(r=>r.Situacao=="Conectada").ThenBy(r=>r.Tipo!="Wi-Fi").ThenBy(r=>r.Nome).ToArray();
    }
}
