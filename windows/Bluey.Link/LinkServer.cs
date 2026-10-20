using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
namespace Bluey.Link;

public sealed record Enrollment(string DeviceId, string Name, string Pin, string RequestId, TaskCompletionSource<bool> Decision, CancellationTokenSource Stop);
public sealed class LinkServer : IAsyncDisposable
{
    public const int DefaultPort=47654;
    private readonly ITrustStore store;
    private readonly object trustGate = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly CancellationTokenSource lifetime = new();
    private readonly SemaphoreSlim slots = new(4);
    private readonly ConcurrentDictionary<int, Task> tasks = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> sessions = new();
    private TcpListener? listener; private Task? accepting; private int taskId;
    private LinkFace? latest; private Enrollment? pending;
    public Invitation Invitation { get; }
    public PresentationHub Presentations {get;}=new();
    public PresentationSnapshot Presentation=>Presentations.Sample(clock.ElapsedMilliseconds);
    public ConnectionDiagnostics Diagnostics { get; }
    public Enrollment? Pending => Volatile.Read(ref pending);
    public string HostId => store.HostId;
    public string Pin => Identity.Fingerprint(store.Certificate);
    public IReadOnlyList<Peer> Peers => store.Peers;
    public int Connections => sessions.Count;
    public int Port => listener?.LocalEndpoint is IPEndPoint endpoint?endpoint.Port:throw new InvalidOperationException("Secure host not started");
    public LinkServer(ITrustStore store, ConnectionDiagnostics? diagnostics=null) { this.store = store; Diagnostics=diagnostics??new(); Invitation = new(() => clock.ElapsedMilliseconds); }
    public void Start(IPAddress address,int port=DefaultPort)
    {
        if (listener != null) throw new InvalidOperationException("already started");
        var next=new TcpListener(address,port){ExclusiveAddressUse=true};
        try {next.Start(8);} catch {next.Stop();throw;}
        listener=next;accepting=AcceptLoop();
    }
    public string StartPairing() { StopPairing(); return Invitation.Start(); }
    public void StopPairing() { Invitation.Stop(); Pending?.Decision.TrySetResult(false); }
    public void Decide(string pin, bool approved, string requestId) { var p = Pending; if (p?.Pin == pin && p.RequestId==requestId) p.Decision.TrySetResult(approved); }
    public void Revoke(string pin) { lock(trustGate) {
        store.Remove(pin); if(Pending is {} p && p.Pin==pin) { try {p.Stop.Cancel();} catch(ObjectDisposedException) { } }
        if (sessions.TryGetValue(pin, out var session)) { try { session.Cancel(); } catch(ObjectDisposedException) { } }
    } }
    // O(1), no serialization, crypto, storage, socket work or waiting on the animation thread.
    public void Publish(double x, double y, string mood, double talk)
    {
        if (!double.IsFinite(x+y+talk) || x is < -1 or > 1 || y is < -1 or > 1 || talk is < 0 or > 1 || !Wire.Moods.Contains(mood)) return;
        Volatile.Write(ref latest, new(x, y, mood, talk, clock.ElapsedMilliseconds));
    }
    private async Task AcceptLoop()
    {
        try
        {
            while (!lifetime.IsCancellationRequested)
            {
                var client = await listener!.AcceptTcpClientAsync(lifetime.Token);
                var attempt=Diagnostics.Begin();
                var port=0;
                try {port=(client.Client.RemoteEndPoint as IPEndPoint)?.Port??0;}
                catch(Exception e) when(e is SocketException or ObjectDisposedException) { /* Optional diagnostic metadata. */ }
                Diagnostics.Record(attempt,ConnectionStage.TcpAccepted,port);
                if (!slots.Wait(0)) { Diagnostics.Record(attempt,ConnectionStage.TcpRejectedBusy,port);client.Dispose(); continue; }
                var id = Interlocked.Increment(ref taskId);
                var task = Handle(client,attempt,port); tasks[id] = task;
                _ = task.ContinueWith(_ => { tasks.TryRemove(id, out var unused); }, TaskScheduler.Default);
            }
        }
        catch (Exception e) when (e is OperationCanceledException or SocketException or ObjectDisposedException) { }
    }
    private async Task Handle(TcpClient client,string attempt,int port)
    {
        string? pin = null; WireConnection? wire = null; Enrollment? request = null;
        var stage=ConnectionStage.TlsStarted;var failure=ConnectionFailure.None;var validationReached=false;
        void Stage(ConnectionStage value) {stage=value;Diagnostics.Record(attempt,value,port);}
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        try
        {
            using (client)
            using (var tls = new SslStream(client.GetStream(), false, (_, certificate, _, _) =>
            {
                validationReached=true;
                if(certificate==null) {failure=ConnectionFailure.ClientCertificateAbsent;Stage(ConnectionStage.ClientCertificateAbsent);return false;}
                Stage(ConnectionStage.ClientCertificatePresent);
                if (certificate is not X509Certificate2 c || !Identity.Valid(c)) {
                    failure=ConnectionFailure.ClientCertificateInvalid;Stage(ConnectionStage.ClientCertificateRejected);return false;
                }
                pin = Identity.Fingerprint(c);
                if(store.Peers.Any(p=>p.Pin==pin)){Stage(ConnectionStage.ClientCertificateTrusted);return true;}
                if(Invitation.Active){Stage(ConnectionStage.ClientCertificateEnrollmentCandidate);return true;}
                failure=ConnectionFailure.EnrollmentRejectedOrExpired;Stage(ConnectionStage.ClientCertificateRejected);return false;
            }))
            {
                client.NoDelay = true;Stage(ConnectionStage.TlsStarted);
                using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
                {
                    deadline.CancelAfter(5000);
                    await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions {
                        ServerCertificateSelectionCallback = (_, _) => {
                            Diagnostics.Record(attempt,ConnectionStage.ServerCertificateSelected,port);
                            return store.Certificate;
                        }, ClientCertificateRequired = true,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck }, deadline.Token);
                }
                if(pin==null || tls.RemoteCertificate==null) {
                    failure=ConnectionFailure.ClientCertificateAbsent;Stage(ConnectionStage.ClientCertificateAbsent);throw new AuthenticationException();
                }
                if(!tls.IsMutuallyAuthenticated){failure=ConnectionFailure.MutualAuthenticationFailed;throw new AuthenticationException();}
                failure=ConnectionFailure.None;stage=ConnectionStage.MutualTlsCompleted;
                Diagnostics.Record(attempt,stage,port,protocol:tls.SslProtocol);
                var session = Guid.NewGuid().ToString(); wire = new(tls, session);
                await wire.Send("hello", new { hostId = HostId, pin = Pin }, stop.Token);Stage(ConnectionStage.HelloSent);
                var first = await wire.Read(stop.Token); var peer = store.Peers.SingleOrDefault(p => p.Pin == pin);
                if (first.Type == "enroll")
                {
                    Stage(ConnectionStage.EnrollmentReceived);
                    var generation = Invitation.Generation;
                    if (store.Peers.Count >= 8 || !Invitation.Check(Wire.Text(first.Payload, "secret"))) {
                        failure=ConnectionFailure.EnrollmentRejectedOrExpired;throw new AuthenticationException();
                    }
                    Stage(ConnectionStage.EnrollmentValidated);
                    request = new(Wire.Text(first.Payload,"deviceId"), Wire.Text(first.Payload,"name"), pin, first.Id, new(TaskCreationOptions.RunContinuationsAsynchronously), stop);
                    if (Interlocked.CompareExchange(ref pending, request, null) != null) {failure=ConnectionFailure.EnrollmentRejectedOrExpired;throw new AuthenticationException();}
                    Stage(ConnectionStage.ApprovalPending);
                    // Keep one TLS read outstanding during approval: disconnect/background releases
                    // the pending slot immediately. The same read receives confirm after approval.
                    using(var approvalStop=CancellationTokenSource.CreateLinkedTokenSource(stop.Token)) {
                        var confirmation=wire.Read(approvalStop.Token,90000);
                        try {
                            var decision=request.Decision.Task.WaitAsync(TimeSpan.FromSeconds(90),stop.Token);
                            if(await Task.WhenAny(decision,confirmation)==confirmation) { await confirmation; throw new AuthenticationException(); }
                            if(!await decision) {failure=ConnectionFailure.ApprovalRejected;Stage(ConnectionStage.HostRejected);throw new AuthenticationException();}
                            Stage(ConnectionStage.HostApproved);
                            await wire.Send("approve",new {requestId=first.Id,clientPin=pin},stop.Token);
                            approvalStop.CancelAfter(5000);
                            var confirm=await confirmation;
                            if(confirm.Type!="confirm" || Wire.Text(confirm.Payload,"requestId")!=first.Id || !Invitation.Consume(generation))throw new AuthenticationException();
                        } finally {
                            approvalStop.Cancel();request.Decision.TrySetResult(false);
                            try {await confirmation;} catch(Exception e) when(e is IOException or OperationCanceledException) { }
                        }
                    }
                    peer = new(request.DeviceId, request.Name, pin);
                    Stage(ConnectionStage.TrustPersisting);
                    lock(trustGate) { stop.Token.ThrowIfCancellationRequested(); if(store.Peers.Any(p=>p.DeviceId==peer.DeviceId && p.Pin!=peer.Pin))throw new AuthenticationException(); store.Add(peer); }
                    Interlocked.CompareExchange(ref pending, null, request); request = null;
                }
                else if (first.Type != "connect" || peer == null || Wire.Text(first.Payload,"hostId") != HostId || Wire.Text(first.Payload,"deviceId") != peer.DeviceId)
                    throw new AuthenticationException();
                if (peer == null) throw new AuthenticationException();
                if (sessions.TryGetValue(pin, out var old)) old.Cancel(); sessions[pin] = stop;
                await wire.Send("ready", new { hostId = HostId, deviceId = peer.DeviceId }, stop.Token);Stage(ConnectionStage.SessionReady);
                var writer = SendFaces(wire, pin, stop.Token);
                var reader = ReadHeartbeats(wire, pin, stop.Token,attempt);
                await Task.WhenAny(writer, reader); stop.Cancel();
                await Task.WhenAll(writer, reader);
            }
        }
        catch (Exception e) when (e is IOException or AuthenticationException or OperationCanceledException or TimeoutException or SocketException or InvalidOperationException or System.Security.Cryptography.CryptographicException)
        {
            if(failure==ConnectionFailure.None)failure=e switch {
                ProtocolException=>ConnectionFailure.ProtocolRejected,
                OperationCanceledException=>stop.IsCancellationRequested?ConnectionFailure.Cancelled:ConnectionFailure.TimedOut,
                TimeoutException=>ConnectionFailure.TimedOut,
                AuthenticationException=>stage<=ConnectionStage.MutualTlsCompleted?ConnectionFailure.TlsHandshakeFailed:ConnectionFailure.ProtocolRejected,
                SocketException=>ConnectionFailure.SocketFailure,
                IOException=>stage==ConnectionStage.TrustPersisting?ConnectionFailure.StorageFailure:stage<ConnectionStage.MutualTlsCompleted?ConnectionFailure.TlsPeerClosed:ConnectionFailure.PeerClosed,
                System.Security.Cryptography.CryptographicException=>ConnectionFailure.StorageFailure,
                _=>ConnectionFailure.Unexpected
            };
            var duringTls=stage<ConnectionStage.MutualTlsCompleted;
            if(duringTls&&!validationReached)Diagnostics.Record(attempt,ConnectionStage.ClientValidationNotReached,port);
            Diagnostics.Record(attempt,stage,port,failure,ConnectionDiagnostics.NativeError(e),
                errorHResult:e.HResult,tlsDetail:duringTls?ConnectionDiagnostics.TlsDetailFor(e):TlsFailureDetail.None);
        }
        finally
        {
            stop.Cancel(); client.Dispose();Presentations.Release(attempt);Diagnostics.Record(attempt,ConnectionStage.SessionClosed,port,failure);
            if (request != null) Interlocked.CompareExchange(ref pending, null, request);
            if (pin != null) ((ICollection<KeyValuePair<string,CancellationTokenSource>>)sessions).Remove(new(pin, stop));
            slots.Release();
        }
    }
    private async Task ReadHeartbeats(WireConnection wire, string pin, CancellationToken stop,string lease)
    {
        while (true)
        {
            var m = await wire.Read(stop);
            if (!store.Peers.Any(p => p.Pin == pin)) throw new ProtocolException("untrusted/command");
            if(m.Type=="presentation")Presentations.Apply(lease,m,clock.ElapsedMilliseconds);
            else if(m.Type=="ping")await wire.Send("pong", new { }, stop);
            else throw new ProtocolException("command");
        }
    }
    private async Task SendFaces(WireConnection wire, string pin, CancellationToken stop)
    {
        LinkFace? last = null; long sentAt = -1000;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));
        while (await timer.WaitForNextTickAsync(stop))
        {
            var face = Volatile.Read(ref latest); var now = clock.ElapsedMilliseconds;
            if (!store.Peers.Any(p => p.Pin == pin)) throw new AuthenticationException();
            if (face == null || now - face.AtMs > 2500 || !FaceFilter.Significant(last, face, sentAt, now)) continue;
            await wire.Send("face", new { gazeX=face.GazeX, gazeY=face.GazeY, mood=face.Mood, talk=face.Talk, atMs=face.AtMs }, stop);
            last = face; sentAt = now;
        }
    }
    public async ValueTask DisposeAsync()
    {
        StopPairing(); lifetime.Cancel(); listener?.Stop(); foreach (var s in sessions.Values) { try {s.Cancel();} catch(ObjectDisposedException) { } }
        if (accepting != null) await accepting;
        try { await Task.WhenAll(tasks.Values).WaitAsync(TimeSpan.FromSeconds(6)); } catch (TimeoutException) { }
    }
}
