using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace VarthexComanda.Servidor;

public sealed record InterfaceAcessoServidor(string Nome,string Descricao,NetworkInterfaceType Tipo,bool Ativa,bool TemGateway,IReadOnlyList<string> Enderecos);

/// <summary>Advertises LAN addresses rather than Hyper-V/WSL/VPN addresses to phones.</summary>
public static class EnderecosServidor
{
    public static IReadOnlyList<string> Ler()
    {
        var redes=new List<InterfaceAcessoServidor>();
        foreach(var rede in NetworkInterface.GetAllNetworkInterfaces())
        {
            if(rede.OperationalStatus!=OperationalStatus.Up)continue;
            try
            {
                var ip=rede.GetIPProperties();
                redes.Add(new(rede.Name,rede.Description,rede.NetworkInterfaceType,true,
                    ip.GatewayAddresses.Any(g=>g.Address.AddressFamily==AddressFamily.InterNetwork&&!g.Address.Equals(IPAddress.Any)),
                    ip.UnicastAddresses.Where(u=>u.Address.AddressFamily==AddressFamily.InterNetwork).Select(u=>u.Address.ToString()).ToArray()));
            }
            catch(NetworkInformationException){ /* A changing/disconnected adapter must not hide other networks. */ }
        }
        return Selecionar(redes);
    }
    public static IReadOnlyList<string> Selecionar(IEnumerable<InterfaceAcessoServidor> redes)=>redes
        .Where(r=>r.Ativa&&r.Tipo is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)&&!Virtual(r.Nome,r.Descricao))
        .OrderByDescending(r=>r.TemGateway).ThenBy(r=>r.Tipo==NetworkInterfaceType.Wireless80211?0:1).ThenBy(r=>r.Nome,StringComparer.OrdinalIgnoreCase)
        .SelectMany(r=>r.Enderecos).Where(Local).Distinct().Append("127.0.0.1").ToArray();
    public static bool Virtual(string nome,string descricao)
    {
        var texto=nome+" "+descricao;
        return new[]{"vethernet","hyper-v","hyper v","wsl","virtualbox","vmware","docker","virtual ethernet","virtual adapter","vpn","wireguard","tailscale","zerotier","tap-windows","tunnel"}.Any(p=>texto.Contains(p,StringComparison.OrdinalIgnoreCase));
    }
    private static bool Local(string valor)
    {
        if(!IPAddress.TryParse(valor,out var ip)||ip.AddressFamily!=AddressFamily.InterNetwork)return false;
        var b=ip.GetAddressBytes();return b[0]==10||b[0]==192&&b[1]==168||b[0]==172&&b[1] is >=16 and <=31;
    }
}
