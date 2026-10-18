using System.ComponentModel;
using System.Globalization;
using System.Security.Authentication;
namespace Bluey.Link;

public enum ConnectionStage
{
    TcpAccepted, TcpRejectedBusy, TlsStarted, ClientCertificatePresent, ClientCertificateAbsent,
    ClientCertificateRejected, ClientCertificateEnrollmentCandidate, ClientCertificateTrusted,
    MutualTlsCompleted, HelloSent, EnrollmentReceived, EnrollmentValidated, ApprovalPending,
    HostApproved, HostRejected, TrustPersisting, SessionReady, SessionClosed
}
public enum ConnectionFailure
{
    None, TlsHandshakeFailed, ClientCertificateAbsent, ClientCertificateInvalid, MutualAuthenticationFailed,
    EnrollmentRejectedOrExpired, ApprovalRejected, ProtocolRejected, TimedOut, Cancelled, SocketFailure,
    PeerClosed, StorageFailure, Unexpected
}
public sealed record ConnectionDiagnostic(DateTimeOffset At, string Attempt, ConnectionStage Stage,
    ConnectionFailure Failure, int Port, int NativeError, SslProtocols Protocol);

// Fixed vocabulary only: never accept exception messages, packet text, certificate subjects or URIs.
public sealed class ConnectionDiagnostics
{
    public const int Capacity=96;
    private readonly object gate=new(); private readonly Queue<ConnectionDiagnostic> entries=new(); private int attempts;
    public string Begin() => "W"+Interlocked.Increment(ref attempts).ToString("x8",CultureInfo.InvariantCulture);
    public void Record(string attempt, ConnectionStage stage, int port, ConnectionFailure failure=ConnectionFailure.None,
        int nativeError=0, SslProtocols protocol=SslProtocols.None)
    {
        lock(gate) {
            entries.Enqueue(new(DateTimeOffset.UtcNow,attempt,stage,failure,port,nativeError,protocol));
            while(entries.Count>Capacity) entries.Dequeue();
        }
    }
    public ConnectionDiagnostic[] Snapshot() { lock(gate)return entries.ToArray(); }
    public string Text() => "Bluey safe connection diagnostics v1 (UTC; ids local; port=client TCP source port)\n"+
        string.Join("\n",Snapshot().Select(e=>$"{e.At:O} {e.Attempt} port={e.Port} {e.Stage} {e.Failure}"+
            (e.NativeError!=0?$" os=0x{unchecked((uint)e.NativeError):x8}":"")+
            (e.Protocol!=SslProtocols.None?$" tls={e.Protocol}":"")));
    internal static int NativeError(Exception error)
    {
        Exception? next=error;
        for(var i=0;i<5 && next!=null;i++,next=next.InnerException)
            if(next is Win32Exception native)return native.NativeErrorCode;
        return 0;
    }
}
