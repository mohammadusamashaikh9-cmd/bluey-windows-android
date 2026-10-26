using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Bluey.Link;
namespace Bluey.Windows.Connection;
internal sealed class WindowsLinkHost(Bluey.Screen.ScreenSession? screenSession=null)
{
    private ProtectedTrust? store; private LinkServer? server; private Advertisement? advertisement; private IPAddress? address;
    internal AI.ProtectedApiKey ApiKey {get;}=new();
    internal AI.ProtectedApiKey GeminiKey {get;}=new(provider:"gemini");
    private Bluey.Link.LiveAuthority? live;private readonly object liveGate=new();
    internal Bluey.Link.LiveAuthority Live {get{lock(liveGate)return live??=new(new Bluey.Link.DeniedIssuer(),audioAnswers:new Bluey.Link.GeminiAnswers(GeminiKey,Bluey.Link.GeminiAnswers.CreateHttpClient()),openAiEnabled:false,screenSession:screenSession);}}
    internal ConnectionDiagnostics Diagnostics {get;}=new();
    internal LinkServer? Server => Volatile.Read(ref server);
    internal string Status => Server is {} s ? $"TLS host {address}:{s.Port} · {s.Connections} authenticated connection(s)\n{advertisement?.Status}\nHost identity: {s.HostId}\nServer SHA-256: {s.Pin}" : "Secure host stopped";
    internal static bool Private(IPAddress ip) { var b=ip.GetAddressBytes(); return b.Length==4 && (b[0]==10 || b[0]==172 && b[1]>=16 && b[1]<=31 || b[0]==192 && b[1]==168); }
    internal static string[] Addresses() => NetworkInterface.GetAllNetworkInterfaces().Where(n=>n.OperationalStatus==OperationalStatus.Up && n.NetworkInterfaceType!=NetworkInterfaceType.Loopback)
        .SelectMany(n=>n.GetIPProperties().UnicastAddresses).Select(a=>a.Address).Where(Private).Select(a=>a.ToString()).Distinct().ToArray();
    internal async Task Start(string selected)
    {
        if(Server!=null) return;
        if(!IPAddress.TryParse(selected,out var ip)||!Private(ip)||!Addresses().Contains(selected)) throw new InvalidOperationException("Select this laptop's private IPv4 Wi-Fi/LAN address.");
        store ??= new ProtectedTrust(); var next=new LinkServer(store,Diagnostics,Live);
        try {next.Start(ip);} catch {await next.DisposeAsync();throw;}
        address=ip;
        Volatile.Write(ref server,next); advertisement=new(ip,next.Port,next.HostId);
    }
    internal string DiagnosticText() => "Bluey host status: "+(Server is {} s?$"listening={address}:{s.Port}":"stopped")+"\n"+Diagnostics.Text();
    internal string StartPairing()
    {
        var s=Server??throw new InvalidOperationException("Start the secure host first."); var secret=s.StartPairing();
        return $"bluey-secure://pair?host={address}&port={s.Port}&id={s.HostId}&pin={s.Pin}&secret={secret}&exp={DateTimeOffset.UtcNow.ToUnixTimeSeconds()+300}";
    }
    internal void StopLive(){foreach(var peer in Live.Peers.Where(p=>p.Authorized))Server?.Presentations.Release(peer.Lease);Live.StopAll();}
    internal async Task Stop()
    {
        live?.StopAll();var s=Interlocked.Exchange(ref server,null); advertisement?.Stop(); advertisement=null;
        if(s!=null) await s.DisposeAsync();
    }
    internal void Publish(Bluey.Core.FaceState f) => Server?.Publish(f.GazeX,f.GazeY,f.Mood.ToString().ToLowerInvariant(),f.Talk);
}
