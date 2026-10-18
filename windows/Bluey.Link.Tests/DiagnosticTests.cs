using System.Net;
using System.Net.Sockets;
using Xunit;
namespace Bluey.Link.Tests;

public sealed class DiagnosticTests
{
    private static string Text(LinkServer server)
    {
        var property=typeof(LinkServer).GetProperty("Diagnostics");
        Assert.NotNull(property);
        var log=property.GetValue(server);Assert.NotNull(log);
        var method=log.GetType().GetMethod("Text");Assert.NotNull(method);
        return Assert.IsType<string>(method.Invoke(log,null));
    }
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
}
