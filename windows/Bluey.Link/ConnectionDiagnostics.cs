using System.ComponentModel;
using System.Globalization;
using System.Security.Authentication;
namespace Bluey.Link;

public enum ConnectionStage
{
    TcpAccepted, TcpRejectedBusy, TlsStarted, ServerCertificateSelected, ClientValidationNotReached, ClientCertificatePresent, ClientCertificateAbsent,
    ClientCertificateRejected, ClientCertificateEnrollmentCandidate, ClientCertificateTrusted,
    MutualTlsCompleted, HelloSent, EnrollmentReceived, EnrollmentValidated, ApprovalPending,
    HostApproved, HostRejected, TrustPersisting, SessionReady, SessionClosed
}
public enum ConnectionFailure
{
    None, TlsHandshakeFailed, TlsPeerClosed, ClientCertificateAbsent, ClientCertificateInvalid, MutualAuthenticationFailed,
    EnrollmentRejectedOrExpired, ApprovalRejected, ProtocolRejected, TimedOut, Cancelled, SocketFailure,
    PeerClosed, StorageFailure, Unexpected
}
public enum TlsFailureDetail
{
    None, AlgorithmMismatch, InvalidTlsMessage, CertificateRejected, InvalidSignature,
    CredentialsUnavailable, UntrustedRoot, OtherNativeError, AuthenticationFailed, PeerClosed
}
public sealed record ConnectionDiagnostic(DateTimeOffset At, string Attempt, ConnectionStage Stage,
    ConnectionFailure Failure, int Port, int NativeError, SslProtocols Protocol, int ErrorHResult, TlsFailureDetail TlsDetail);

// Fixed vocabulary only: never accept exception messages, packet text, certificate subjects or URIs.
public sealed class ConnectionDiagnostics
{
    public const int Capacity=96;
    private readonly object gate=new(); private readonly Queue<ConnectionDiagnostic> entries=new(); private int attempts;
    public string Begin() => "W"+Interlocked.Increment(ref attempts).ToString("x8",CultureInfo.InvariantCulture);
    public void Record(string attempt, ConnectionStage stage, int port, ConnectionFailure failure=ConnectionFailure.None,
        int nativeError=0, SslProtocols protocol=SslProtocols.None, int errorHResult=0, TlsFailureDetail tlsDetail=TlsFailureDetail.None)
    {
        lock(gate) {
            entries.Enqueue(new(DateTimeOffset.UtcNow,attempt,stage,failure,port,nativeError,protocol,errorHResult,tlsDetail));
            while(entries.Count>Capacity) entries.Dequeue();
        }
    }
    public ConnectionDiagnostic[] Snapshot() { lock(gate)return entries.ToArray(); }
    public string Text() => "Bluey safe connection diagnostics v2 (UTC; ids local; port=client TCP source port)\n"+
        string.Join("\n",Snapshot().Select(e=>$"{e.At:O} {e.Attempt} port={e.Port} {e.Stage} {e.Failure}"+
            (e.NativeError!=0?$" os=0x{unchecked((uint)e.NativeError):x8}":"")+
            (e.ErrorHResult!=0?$" hr=0x{unchecked((uint)e.ErrorHResult):x8}":"")+
            (e.TlsDetail!=TlsFailureDetail.None?$" detail={e.TlsDetail}":"")+
            (e.Protocol!=SslProtocols.None?$" tls={e.Protocol}":"")));
    public static TlsFailureDetail TlsDetailFor(Exception error) => unchecked((uint)NativeError(error)) switch {
        0x80090331 => TlsFailureDetail.AlgorithmMismatch,
        0x80090326 => TlsFailureDetail.InvalidTlsMessage,
        0x80090327 or 0x80090328 => TlsFailureDetail.CertificateRejected,
        0x8009030f => TlsFailureDetail.InvalidSignature,
        0x8009030e => TlsFailureDetail.CredentialsUnavailable,
        0x80090325 => TlsFailureDetail.UntrustedRoot,
        0 => error is AuthenticationException?TlsFailureDetail.AuthenticationFailed:TlsFailureDetail.PeerClosed,
        _ => TlsFailureDetail.OtherNativeError
    };
    internal static int NativeError(Exception error)
    {
        Exception? next=error;
        for(var i=0;i<10 && next!=null;i++,next=next.InnerException)
            if(next is Win32Exception native)return native.NativeErrorCode;
        return 0;
    }
}
