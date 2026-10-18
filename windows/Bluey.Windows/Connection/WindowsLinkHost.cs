using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Bluey.Link;
namespace Bluey.Windows.Connection;
internal sealed class WindowsLinkHost
{
    private ProtectedTrust? store; private LinkServer? server; private Advertisement? advertisement; private IPAddress? address;
    internal LinkServer? Server => Volatile.Read(ref server);
    internal string Status => Server is {} s ? $"TLS host {address}:{s.Port} · {s.Connections} authenticated connection(s)\n{advertisement?.Status}" : "Secure host stopped";
    internal static bool Private(IPAddress ip) { var b=ip.GetAddressBytes(); return b.Length==4 && (b[0]==10 || b[0]==172 && b[1]>=16 && b[1]<=31 || b[0]==192 && b[1]==168); }
    internal static string[] Addresses() => NetworkInterface.GetAllNetworkInterfaces().Where(n=>n.OperationalStatus==OperationalStatus.Up && n.NetworkInterfaceType!=NetworkInterfaceType.Loopback)
        .SelectMany(n=>n.GetIPProperties().UnicastAddresses).Select(a=>a.Address).Where(Private).Select(a=>a.ToString()).Distinct().ToArray();
    internal void Start(string selected)
    {
        if(Server!=null) return;
        if(!IPAddress.TryParse(selected,out var ip)||!Private(ip)||!Addresses().Contains(selected)) throw new InvalidOperationException("Select this laptop's private IPv4 Wi-Fi/LAN address.");
        store ??= new ProtectedTrust(); var next=new LinkServer(store); next.Start(ip); address=ip;
        Volatile.Write(ref server,next); advertisement=new(ip,next.Port,next.HostId);
    }
    internal string StartPairing()
    {
        var s=Server??throw new InvalidOperationException("Start the secure host first."); var secret=s.StartPairing();
        return $"bluey-secure://pair?host={address}&port={s.Port}&id={s.HostId}&pin={s.Pin}&secret={secret}&exp={DateTimeOffset.UtcNow.ToUnixTimeSeconds()+300}";
    }
    internal async Task Stop()
    {
        var s=Interlocked.Exchange(ref server,null); advertisement?.Stop(); advertisement=null;
        if(s!=null) await s.DisposeAsync();
    }
    internal void Publish(Bluey.Core.FaceState f) => Server?.Publish(f.GazeX,f.GazeY,f.Mood.ToString().ToLowerInvariant(),f.Talk);
}
