using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Xunit;
namespace Bluey.Link.Tests;
public sealed class MemoryStore : ITrustStore, IDisposable
{
    private readonly object gate=new();private readonly List<Peer> peers=[];
    public string HostId {get;}=Guid.NewGuid().ToString();public X509Certificate2 Certificate{get;}=Identity.Create("Bluey-Test",true);
    public IReadOnlyList<Peer> Peers {get{lock(gate)return peers.ToArray();}}
    public void Add(Peer p){lock(gate){peers.RemoveAll(x=>x.Pin==p.Pin);peers.Add(p);}}
    public void Remove(string p){lock(gate)peers.RemoveAll(x=>x.Pin==p);}
    public void Dispose()=>Certificate.Dispose();
}
public sealed class TlsTests
{
    private sealed class Client(TcpClient socket,SslStream tls,WireConnection wire,Message hello) : IDisposable
    { public WireConnection Wire=>wire;public Message Hello=>hello;public void Dispose(){tls.Dispose();socket.Dispose();} }
    private static async Task<Client> Connect(LinkServer server,X509Certificate2? cert,string? pin=null,SslProtocols protocols=SslProtocols.Tls12|SslProtocols.Tls13)
    {
        var tcp=new TcpClient();await tcp.ConnectAsync(IPAddress.Loopback,server.Port);
        var ssl=new SslStream(tcp.GetStream(),false,(_,c,_,_)=>c is X509Certificate2 x && Identity.Fingerprint(x)==(pin??server.Pin));
        try
        {
            using var deadline=new CancellationTokenSource(5000);
            await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions{TargetHost="Bluey",ClientCertificates=cert==null?[]:new X509CertificateCollection{cert},EnabledSslProtocols=protocols,CertificateRevocationCheckMode=X509RevocationMode.NoCheck},deadline.Token);
            var bytes=new List<byte>();var one=new byte[1];
            while(true){var n=await ssl.ReadAsync(one,deadline.Token);if(n==0)throw new EndOfStreamException();if(one[0]==10)break;bytes.Add(one[0]);if(bytes.Count>4096)throw new IOException();}
            var hello=Wire.Decode(bytes.ToArray());Assert.Equal("hello",hello.Type);
            var wire=new WireConnection(ssl,hello.Session);wire.SeedIncoming(hello);return new(tcp,ssl,wire,hello);
        }
        catch{ssl.Dispose();tcp.Dispose();throw;}
    }
    private static async Task Denied(Func<Task> operation)
    {
        var e=await Record.ExceptionAsync(operation);Assert.True(e is AuthenticationException or IOException,$"Expected TLS/authentication denial, got {e?.GetType().Name??"success"}");
    }
    [Fact] public async Task RealMutualTlsEnrollmentRequiresBothApprovalsThenStreamsAndReconnects()
    {
        using var store=new MemoryStore();await using var server=new LinkServer(store);server.Start(IPAddress.Loopback,0);
        using var cert=Identity.Create("phone",false);var id=Guid.NewGuid().ToString();var secret=server.StartPairing();
        using(var c=await Connect(server,cert))
        {
            var request=await c.Wire.Send("enroll",new{deviceId=id,name="Test phone",secret},default);
            for(var i=0;i<100 && server.Pending==null;i++)await Task.Delay(10);
            Assert.NotNull(server.Pending);Assert.Empty(store.Peers);Assert.Equal(0,server.Connections);
            var trace=server.Diagnostics.Text();Assert.Contains("MutualTlsCompleted",trace);Assert.Contains("EnrollmentValidated",trace);Assert.Contains("ApprovalPending",trace);
            Assert.DoesNotContain(secret,trace);Assert.DoesNotContain("Test phone",trace);
            server.Decide(Identity.Fingerprint(cert),true,Guid.NewGuid().ToString());
            Assert.False(server.Pending!.Decision.Task.IsCompleted); // A different reviewed request cannot approve this one.
            server.Decide(Identity.Fingerprint(cert),true,server.Pending.RequestId);
            var approve=await c.Wire.Read(default);Assert.Equal("approve",approve.Type);Assert.Empty(store.Peers);
            await c.Wire.Send("confirm",new{requestId=request},default);
            Assert.Equal("ready",(await c.Wire.Read(default)).Type);Assert.Single(store.Peers);
            server.Publish(.3,-.6,"talking",.75);
            var face=Wire.Face(await c.Wire.Read(default));Assert.Equal(.3,face.GazeX);Assert.Equal("talking",face.Mood);Assert.Equal(.75,face.Talk);
        }
        using(var c=await Connect(server,cert))
        {
            await c.Wire.Send("connect",new{hostId=server.HostId,deviceId=id},default);Assert.Equal("ready",(await c.Wire.Read(default)).Type);
            server.Revoke(Identity.Fingerprint(cert));Assert.Empty(store.Peers);
            // Any in-flight face may precede closure; no reconnection after revocation.
        }
        await Denied(async()=>{using var c=await Connect(server,cert);});
    }
    [Theory]
    [InlineData(SslProtocols.Tls12)]
    [InlineData(SslProtocols.Tls13)]
    public async Task WrongPinMissingCertificateUnknownAndPlaintextPeersFailClosed(SslProtocols protocol)
    {
        using var store=new MemoryStore();await using var s=new LinkServer(store);s.Start(IPAddress.Loopback,0);using var cert=Identity.Create("phone",false);
        s.StartPairing();await Denied(async()=>{using var c=await Connect(s,cert,new string('0',64),protocol);});
        await Denied(async()=>{using var c=await Connect(s,null,protocols:protocol);});s.StopPairing();
        await Denied(async()=>{using var c=await Connect(s,cert,protocols:protocol);});
        using var tcp=new TcpClient();await tcp.ConnectAsync(IPAddress.Loopback,s.Port);
        var raw=tcp.GetStream();await raw.WriteAsync("{}\n"u8.ToArray());tcp.Client.Shutdown(SocketShutdown.Send);
        using var deadline=new CancellationTokenSource(5000);var b=new byte[1024];var n=await raw.ReadAsync(b,deadline.Token);
        Assert.True(n==0 || b[0]!=123);Assert.Empty(store.Peers);
    }
    [Fact] public async Task WrongSecretAndRejectedEnrollmentNeverPersistTrust()
    {
        using var store=new MemoryStore();await using var s=new LinkServer(store);s.Start(IPAddress.Loopback,0);using var cert=Identity.Create("phone",false);s.StartPairing();
        using(var c=await Connect(s,cert))
        {
            await c.Wire.Send("enroll",new{deviceId=Guid.NewGuid().ToString(),name="wrong",secret=new string('x',43)},default);
            await Denied(async()=>{await c.Wire.Read(default);});Assert.Empty(store.Peers);
        }
        var secret=s.StartPairing();using(var c=await Connect(s,cert))
        {
            await c.Wire.Send("enroll",new{deviceId=Guid.NewGuid().ToString(),name="reject",secret},default);
            for(var i=0;i<100 && s.Pending==null;i++)await Task.Delay(10);
            s.Decide(Identity.Fingerprint(cert),false,s.Pending!.RequestId);await Denied(async()=>{await c.Wire.Read(default);});Assert.Empty(store.Peers);
        }
    }
    [Fact] public async Task PendingApprovalCancelAndSlotTimeoutCleanup()
    {
        using var store=new MemoryStore();await using var s=new LinkServer(store);s.Start(IPAddress.Loopback,0);using var cert=Identity.Create("phone",false);
        var secret=s.StartPairing();using var c=await Connect(s,cert);
        await c.Wire.Send("enroll",new{deviceId=Guid.NewGuid().ToString(),name="cancel",secret},default);
        for(var i=0;i<100 && s.Pending==null;i++)await Task.Delay(10);
        Assert.NotNull(s.Pending);s.StopPairing();await Denied(async()=>{await c.Wire.Read(default);});Assert.Empty(store.Peers);
        // A fragmented unauthenticated TLS handshake cannot occupy all four slots forever.
        var sockets=new List<TcpClient>();
        try {
            for(var i=0;i<4;i++){var tcp=new TcpClient();await tcp.ConnectAsync(IPAddress.Loopback,s.Port);sockets.Add(tcp);}
            await Task.Delay(5300);
            s.StartPairing();using var fresh=await Connect(s,cert);Assert.Equal(0,s.Connections);
        } finally {foreach(var tcp in sockets)tcp.Dispose();}
    }
    [Fact] public async Task LatestStateIsBoundedAndMalformedSessionCloses()
    {
        using var store=new MemoryStore();await using var s=new LinkServer(store);s.Start(IPAddress.Loopback,0);using var cert=Identity.Create("phone",false);
        var deviceId=Guid.NewGuid().ToString();store.Add(new(deviceId,"test",Identity.Fingerprint(cert)));
        using var c=await Connect(s,cert);await c.Wire.Send("connect",new{hostId=s.HostId,deviceId},default);Assert.Equal("ready",(await c.Wire.Read(default)).Type);
        var clock=System.Diagnostics.Stopwatch.StartNew();for(var i=0;i<100000;i++)s.Publish(.5,-.2,"happy",0);
        Assert.True(clock.Elapsed<TimeSpan.FromSeconds(2),"Publishing must not wait on the network");
        var face=await c.Wire.Read(default);Assert.Equal("face",face.Type);Assert.Equal(.5,Wire.Face(face).GazeX);
        // A valid envelope carrying a role-inappropriate command must close the session.
        await c.Wire.Send("ready",new{hostId=s.HostId,deviceId},default);
        await Denied(async()=>{while(true)await c.Wire.Read(default);});
    }
    [Fact] public async Task PhoneDisconnectReleasesPendingEnrollmentWithoutWaitingForApprovalTimeout()
    {
        using var store=new MemoryStore();await using var s=new LinkServer(store);s.Start(IPAddress.Loopback,0);using var cert=Identity.Create("phone",false);
        var secret=s.StartPairing();using(var c=await Connect(s,cert)) {
            await c.Wire.Send("enroll",new{deviceId=Guid.NewGuid().ToString(),name="background",secret},default);
            for(var n=0;n<100 && s.Pending==null;n++)await Task.Delay(10);Assert.NotNull(s.Pending);
        }
        for(var n=0;n<100 && s.Pending!=null;n++)await Task.Delay(10);
        Assert.Null(s.Pending);Assert.Empty(store.Peers);Assert.True(s.Invitation.Active);
    }
    [Fact] public async Task RealTlsBackpressureCancelsWriterInsteadOfGrowingAnApplicationQueue()
    {
        using var serverCert=Identity.Create("slow-reader",true);using var clientCert=Identity.Create("writer",false);
        using var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();
        using var tcp=new TcpClient{SendBufferSize=1024};await tcp.ConnectAsync((IPEndPoint)listener.LocalEndpoint);
        using var peer=await listener.AcceptTcpClientAsync();peer.ReceiveBufferSize=1024;
        using var serverTls=new SslStream(peer.GetStream(),false,(_,c,_,_)=>c is X509Certificate2 x && Identity.Fingerprint(x)==Identity.Fingerprint(clientCert));
        using var clientTls=new SslStream(tcp.GetStream(),false,(_,c,_,_)=>c is X509Certificate2 x && Identity.Fingerprint(x)==Identity.Fingerprint(serverCert));
        using var deadline=new CancellationTokenSource(5000);
        await Task.WhenAll(serverTls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions{
            ServerCertificate=serverCert,ClientCertificateRequired=true,EnabledSslProtocols=SslProtocols.Tls12|SslProtocols.Tls13,
            CertificateRevocationCheckMode=X509RevocationMode.NoCheck},deadline.Token),
            clientTls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions{
                TargetHost="slow-reader",ClientCertificates=new X509CertificateCollection{clientCert},EnabledSslProtocols=SslProtocols.Tls12|SslProtocols.Tls13,
                CertificateRevocationCheckMode=X509RevocationMode.NoCheck},deadline.Token));
        Assert.True(serverTls.IsMutuallyAuthenticated);
        var wire=new WireConnection(clientTls,Guid.NewGuid().ToString());
        // The authenticated peer deliberately never reads application bytes; actual TCP/TLS buffers fill.
        var clock=System.Diagnostics.Stopwatch.StartNew();var sent=0;
        var failure=await Record.ExceptionAsync(async()=>{
            for(;sent<100000;sent++)await wire.Send("ping",new{},default);
        });
        Assert.True(failure is OperationCanceledException or IOException,$"Expected bounded write cancellation, got {failure?.GetType().Name}");
        Assert.InRange(sent,1,99999);Assert.True(clock.Elapsed<TimeSpan.FromSeconds(8));
    }
    [Fact] public async Task AuthenticatedCertificateCannotClaimAnotherDeviceIdentityOnReconnect()
    {
        using var store=new MemoryStore();await using var s=new LinkServer(store);s.Start(IPAddress.Loopback,0);using var cert=Identity.Create("phone",false);
        var device=Guid.NewGuid().ToString();store.Add(new(device,"phone",Identity.Fingerprint(cert)));
        using var c=await Connect(s,cert);await c.Wire.Send("connect",new{hostId=s.HostId,deviceId=Guid.NewGuid().ToString()},default);
        await Denied(async()=>{await c.Wire.Read(default);});Assert.Equal(device,store.Peers.Single().DeviceId);Assert.Equal(0,s.Connections);
    }
    [Theory]
    [InlineData(SslProtocols.Tls12)]
    [InlineData(SslProtocols.Tls13)]
    public async Task AndroidShapedP256CertificateWithoutEkuCanReachApproval(SslProtocols protocol)
    {
        using var key=System.Security.Cryptography.ECDsa.Create(System.Security.Cryptography.ECCurve.NamedCurves.nistP256);
        var request=new CertificateRequest("CN=Android shaped test",key,System.Security.Cryptography.HashAlgorithmName.SHA256);
        // Android KeyGenParameterSpec creates a self-signed leaf without explicit KU/EKU by default.
        using var leaf=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow.AddDays(1));
        using var cert=X509CertificateLoader.LoadPkcs12(leaf.Export(X509ContentType.Pkcs12),null);
        Assert.Empty(cert.Extensions.OfType<X509EnhancedKeyUsageExtension>());
        using var store=new MemoryStore();await using var server=new LinkServer(store);server.Start(IPAddress.Loopback,0);var secret=server.StartPairing();
        using var client=await Connect(server,cert,protocols:protocol);
        await client.Wire.Send("enroll",new{deviceId=Guid.NewGuid().ToString(),name="Android shaped",secret},default);
        for(var i=0;i<100 && server.Pending==null;i++)await Task.Delay(10);
        Assert.NotNull(server.Pending);Assert.Empty(store.Peers);
        Assert.Contains(server.Diagnostics.Snapshot(),e=>e.Stage==ConnectionStage.MutualTlsCompleted && e.Protocol==protocol);
        var trace=server.Diagnostics.Snapshot();
        Assert.Contains(trace,e=>e.Stage==ConnectionStage.ServerCertificateSelected);
        Assert.Contains(trace,e=>e.Stage==ConnectionStage.ClientCertificatePresent);
        Assert.Contains(trace,e=>e.Stage==ConnectionStage.EnrollmentValidated);
        Assert.Contains(trace,e=>e.Stage==ConnectionStage.ApprovalPending);
        var pending=server.Pending!;
        server.Decide(pending.Pin,true,pending.RequestId);
        var approve=await client.Wire.Read(default);Assert.Equal("approve",approve.Type);
        await client.Wire.Send("confirm",new{requestId=pending.RequestId},default);
        Assert.Equal("ready",(await client.Wire.Read(default)).Type);Assert.Single(store.Peers);
        server.Publish(.25,-.5,"talking",.5);
        Assert.Equal("talking",Wire.Face(await client.Wire.Read(default)).Mood);
        server.Revoke(pending.Pin);server.StopPairing();
        await Denied(async()=>{using var rejected=await Connect(server,cert,protocols:protocol);});
    }
    [Fact] public async Task DefaultPortConflictFailsWithoutSilentlySelectingAnEphemeralEndpoint()
    {
        using var occupied=new TcpListener(IPAddress.Loopback,LinkServer.DefaultPort);
        occupied.ExclusiveAddressUse=true;occupied.Start();
        using var store=new MemoryStore();await using var server=new LinkServer(store);
        var error=Assert.Throws<SocketException>(()=>server.Start(IPAddress.Loopback));
        Assert.Contains(error.SocketErrorCode,new[]{SocketError.AddressAlreadyInUse,SocketError.AccessDenied});
    }

}
