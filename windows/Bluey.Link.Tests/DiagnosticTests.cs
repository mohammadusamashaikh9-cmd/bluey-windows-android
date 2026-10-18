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
    [Theory]
    [InlineData(unchecked((int)0x80090331),TlsFailureDetail.AlgorithmMismatch)]
    [InlineData(unchecked((int)0x80090326),TlsFailureDetail.InvalidTlsMessage)]
    [InlineData(unchecked((int)0x80090327),TlsFailureDetail.CertificateRejected)]
    [InlineData(unchecked((int)0x8009030f),TlsFailureDetail.InvalidSignature)]
    [InlineData(unchecked((int)0x8009030e),TlsFailureDetail.CredentialsUnavailable)]
    public void NativeTlsErrorsKeepFixedCategoriesWithoutProviderMessages(int code,TlsFailureDetail expected)
    {
        var error=new IOException("NEVER_LOG",new System.Security.Authentication.AuthenticationException("SECRET",
            new System.ComponentModel.Win32Exception(code,"PRIVATE_PROVIDER_TEXT")));
        Assert.Equal(expected,ConnectionDiagnostics.TlsDetailFor(error));
        var log=new ConnectionDiagnostics();log.Record(log.Begin(),ConnectionStage.TlsStarted,1234,
            tlsDetail:ConnectionDiagnostics.TlsDetailFor(error),errorHResult:error.HResult);
        Assert.Contains("hr=0x",log.Text());Assert.DoesNotContain("NEVER_LOG",log.Text());
        Assert.DoesNotContain("SECRET",log.Text());Assert.DoesNotContain("PRIVATE_PROVIDER_TEXT",log.Text());
    }

    [Fact] public async Task ClosedPeerBeforeCertificateValidationIsExplicitWithoutAssumingRejectionCause()
    {
        using var store=new MemoryStore();await using var server=new LinkServer(store);server.Start(IPAddress.Loopback);
        using(var tcp=new TcpClient()) {await tcp.ConnectAsync(IPAddress.Loopback,server.Port);}
        for(var i=0;i<100 && !Text(server).Contains("SessionClosed");i++)await Task.Delay(10);
        var trace=server.Diagnostics.Snapshot();
        Assert.Contains(trace,e=>e.Stage==ConnectionStage.ClientValidationNotReached);
        Assert.DoesNotContain(trace,e=>e.Stage==ConnectionStage.ClientCertificatePresent || e.Stage==ConnectionStage.MutualTlsCompleted);
        Assert.Empty(store.Peers);Assert.Null(server.Pending);
    }

    [Fact] public void CancellationDoesNotInventAPeerAlertOrNativeTlsError()
    {
        Assert.Equal(TlsFailureDetail.None,ConnectionDiagnostics.TlsDetailFor(new OperationCanceledException()));
    }

}
