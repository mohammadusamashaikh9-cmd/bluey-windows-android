using System.Net;
using System.Net.Sockets;
using Xunit;
namespace Bluey.Link.Tests;

public sealed class DiagnosticTests
{
    private static string Text(LinkServer server)=>server.Diagnostics.Text();
    [Fact] public async Task AcceptedSocketIsObservableBeforeAnyTlsOrEnrollmentMessage()
    {
        using var store=new MemoryStore();await using var server=new LinkServer(store);server.Start(IPAddress.Loopback);
        using var tcp=new TcpClient();await tcp.ConnectAsync(IPAddress.Loopback,server.Port);
        var text=Text(server);
        for(var i=0;i<100 && !text.Contains("TlsStarted");i++){await Task.Delay(10);text=Text(server);}
        Assert.Contains("TcpAccepted",text);Assert.Contains("TlsStarted",text);
        Assert.Contains("port="+((IPEndPoint)tcp.Client.LocalEndPoint!).Port,text);
        Assert.Empty(store.Peers);Assert.Null(server.Pending);
    }
    [Fact] public async Task MalformedPeerIsCategorizedWithoutEchoingBytes()
    {
        using var store=new MemoryStore();await using var server=new LinkServer(store);server.Start(IPAddress.Loopback);
        const string forbidden="bluey-secure://pair?secret=NEVER_LOG_THIS";
        using var tcp=new TcpClient();await tcp.ConnectAsync(IPAddress.Loopback,server.Port);
        await tcp.GetStream().WriteAsync(System.Text.Encoding.UTF8.GetBytes(forbidden));tcp.Client.Shutdown(SocketShutdown.Send);
        for(var i=0;i<100 && !Text(server).Contains("SessionClosed");i++)await Task.Delay(10);
        var text=Text(server);Assert.Contains("SessionClosed",text);Assert.DoesNotContain(forbidden,text);Assert.DoesNotContain("NEVER_LOG_THIS",text);
        Assert.Contains(server.Diagnostics.Snapshot(),e=>e.Failure!=ConnectionFailure.None);
    }
    [Fact] public void DiagnosticsRetainABoundedTail()
    {
        var log=new ConnectionDiagnostics();
        for(var i=0;i<1000;i++)log.Record(log.Begin(),ConnectionStage.TlsStarted,1234);
        Assert.Equal(ConnectionDiagnostics.Capacity,log.Snapshot().Length);Assert.Equal(ConnectionDiagnostics.Capacity+1,log.Text().Split('\n').Length);
    }
}
