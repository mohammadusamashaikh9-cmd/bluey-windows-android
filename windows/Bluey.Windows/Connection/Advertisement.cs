using System.Net;
using System.Runtime.InteropServices;
namespace Bluey.Windows.Connection;

// Native Windows DNS-SD; retain native request/instance/delegate until async deregistration completes.
internal sealed class Advertisement
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void Complete(uint status, nint context, nint instance);
    [StructLayout(LayoutKind.Sequential)] private struct Request
    {
        public uint Version, InterfaceIndex; public nint Instance, Callback, Context, Credentials;
        [MarshalAs(UnmanagedType.Bool)] public bool Unicast;
    }
    [DllImport("dnsapi.dll", CharSet=CharSet.Unicode)] private static extern nint DnsServiceConstructInstance(string service, string host, nint ip4, nint ip6, ushort port, ushort priority, ushort weight, uint count, nint keys, nint values);
    [DllImport("dnsapi.dll")] private static extern uint DnsServiceRegister(nint request, nint cancel);
    [DllImport("dnsapi.dll")] private static extern uint DnsServiceDeRegister(nint request, nint cancel);
    [DllImport("dnsapi.dll")] private static extern void DnsServiceFreeInstance(nint instance);
    private readonly Complete callback; private nint request, instance; private bool stopping; private bool registered;
    private readonly object gate = new(); private static readonly HashSet<Advertisement> roots = [];
    public string Status { get; private set; } = "Submitting DNS-SD registration";
    internal Advertisement(IPAddress address, int port, string hostId)
    {
        callback = Completed;
        var ip = Marshal.AllocHGlobal(4);
        try { Marshal.Copy(address.GetAddressBytes(),0,ip,4); instance = DnsServiceConstructInstance("Bluey-"+hostId[..8]+"._bluey-sec._tcp.local", "Bluey-"+hostId[..8]+".local", ip,0,(ushort)port,0,0,0,0,0); }
        finally { Marshal.FreeHGlobal(ip); }
        if(instance==0) { Status="DNS-SD instance unavailable; use pairing QR/URI";return; }
        request=Marshal.AllocHGlobal(Marshal.SizeOf<Request>());
        Marshal.StructureToPtr(new Request {Version=1, Instance=instance, Callback=Marshal.GetFunctionPointerForDelegate(callback)},request,false);
        lock(roots) roots.Add(this);
        var result=DnsServiceRegister(request,0);
        if(result!=9506) { Status=$"DNS-SD submission failed ({result}); use QR/URI"; Free(); }
    }
    private void Completed(uint status,nint context,nint returned)
    {
        lock(gate) {
            if(returned!=0 && returned!=instance) DnsServiceFreeInstance(returned);
            if(stopping) { Free(); return; }
            registered=status==0; Status=registered?"DNS-SD advertised (_bluey-sec._tcp)":$"DNS-SD failed ({status}); use QR/URI";
            if(!registered) Free();
            else if(stoppingRequested) Stop();
        }
    }
    internal void Stop()
    {
        lock(gate) {
            if(request==0 || stopping) return;
            // If registration is still pending, its completion will initiate deregistration.
            if(!registered) { stoppingRequested=true; return; }
            stopping=true; var result=DnsServiceDeRegister(request,0);
            if(result!=9506) Free();
        }
    }
    private bool stoppingRequested;
    private void Free()
    {
        if(instance!=0) DnsServiceFreeInstance(instance); instance=0;
        if(request!=0) Marshal.FreeHGlobal(request); request=0;
        lock(roots) roots.Remove(this);
    }
}
