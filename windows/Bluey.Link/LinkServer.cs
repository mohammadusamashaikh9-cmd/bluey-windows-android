using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
namespace Bluey.Link;

public sealed record Enrollment(string DeviceId, string Name, string Pin, string RequestId, TaskCompletionSource<bool> Decision);
public sealed class LinkServer : IAsyncDisposable
{
    private readonly ITrustStore store;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly CancellationTokenSource lifetime = new();
    private readonly SemaphoreSlim slots = new(4);
    private readonly ConcurrentDictionary<int, Task> tasks = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> sessions = new();
    private TcpListener? listener; private Task? accepting; private int taskId;
    private LinkFace? latest; private Enrollment? pending;
    public Invitation Invitation { get; }
    public Enrollment? Pending => Volatile.Read(ref pending);
    public string HostId => store.HostId;
    public string Pin => Identity.Fingerprint(store.Certificate);
    public IReadOnlyList<Peer> Peers => store.Peers;
    public int Connections => sessions.Count;
    public int Port => ((IPEndPoint)listener!.LocalEndpoint).Port;
    public LinkServer(ITrustStore store) { this.store = store; Invitation = new(() => clock.ElapsedMilliseconds); }
    public void Start(IPAddress address)
    {
        if (listener != null) throw new InvalidOperationException("already started");
        listener = new(address, 0); listener.Start(8); accepting = AcceptLoop();
    }
    public string StartPairing() { StopPairing(); return Invitation.Start(); }
    public void StopPairing() { Invitation.Stop(); Pending?.Decision.TrySetResult(false); }
    public void Decide(string pin, bool approved) { var p = Pending; if (p?.Pin == pin) p.Decision.TrySetResult(approved); }
    public void Revoke(string pin) { store.Remove(pin); if (sessions.TryGetValue(pin, out var session)) session.Cancel(); }
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
                if (!slots.Wait(0)) { client.Dispose(); continue; }
                var id = Interlocked.Increment(ref taskId);
                var task = Handle(client); tasks[id] = task;
                _ = task.ContinueWith(_ => { tasks.TryRemove(id, out var unused); }, TaskScheduler.Default);
            }
        }
        catch (Exception e) when (e is OperationCanceledException or SocketException or ObjectDisposedException) { }
    }
    private async Task Handle(TcpClient client)
    {
        string? pin = null; WireConnection? wire = null; Enrollment? request = null;
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        try
        {
            using (client)
            using (var tls = new SslStream(client.GetStream(), false, (_, certificate, _, _) =>
            {
                if (certificate is not X509Certificate2 c || !Identity.Valid(c)) return false;
                pin = Identity.Fingerprint(c);
                return store.Peers.Any(p => p.Pin == pin) || Invitation.Active;
            }))
            {
                client.NoDelay = true;
                using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
                {
                    deadline.CancelAfter(5000);
                    await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions {
                        ServerCertificate = store.Certificate, ClientCertificateRequired = true,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck }, deadline.Token);
                }
                if (pin == null || tls.RemoteCertificate == null || !tls.IsMutuallyAuthenticated) throw new AuthenticationException();
                var session = Guid.NewGuid().ToString(); wire = new(tls, session);
                await wire.Send("hello", new { hostId = HostId, pin = Pin }, stop.Token);
                var first = await wire.Read(stop.Token); var peer = store.Peers.SingleOrDefault(p => p.Pin == pin);
                if (first.Type == "enroll")
                {
                    var generation = Invitation.Generation;
                    if (store.Peers.Count >= 8 || !Invitation.Check(Wire.Text(first.Payload, "secret"))) throw new AuthenticationException();
                    request = new(Wire.Text(first.Payload,"deviceId"), Wire.Text(first.Payload,"name"), pin, first.Id, new(TaskCreationOptions.RunContinuationsAsynchronously));
                    if (Interlocked.CompareExchange(ref pending, request, null) != null) throw new AuthenticationException();
                    if (!await request.Decision.Task.WaitAsync(TimeSpan.FromSeconds(90), stop.Token)) throw new AuthenticationException();
                    await wire.Send("approve", new { requestId = first.Id, clientPin = pin }, stop.Token);
                    var confirm = await wire.Read(stop.Token);
                    if (confirm.Type != "confirm" || Wire.Text(confirm.Payload,"requestId") != first.Id || !Invitation.Consume(generation)) throw new AuthenticationException();
                    peer = new(request.DeviceId, request.Name, pin); store.Add(peer);
                    Interlocked.CompareExchange(ref pending, null, request); request = null;
                }
                else if (first.Type != "connect" || peer == null || Wire.Text(first.Payload,"hostId") != HostId || Wire.Text(first.Payload,"deviceId") != peer.DeviceId)
                    throw new AuthenticationException();
                if (peer == null) throw new AuthenticationException();
                if (sessions.TryGetValue(pin, out var old)) old.Cancel(); sessions[pin] = stop;
                await wire.Send("ready", new { hostId = HostId, deviceId = peer.DeviceId }, stop.Token);
                var writer = SendFaces(wire, pin, stop.Token);
                var reader = ReadHeartbeats(wire, pin, stop.Token);
                await Task.WhenAny(writer, reader); stop.Cancel();
                await Task.WhenAll(writer, reader);
            }
        }
        catch (Exception e) when (e is IOException or AuthenticationException or OperationCanceledException or TimeoutException or SocketException or InvalidOperationException or System.Security.Cryptography.CryptographicException)
        { /* Fail closed. Never log packet, invitation or private-key material. */ }
        finally
        {
            stop.Cancel(); client.Dispose();
            if (request != null) Interlocked.CompareExchange(ref pending, null, request);
            if (pin != null) ((ICollection<KeyValuePair<string,CancellationTokenSource>>)sessions).Remove(new(pin, stop));
            slots.Release();
        }
    }
    private async Task ReadHeartbeats(WireConnection wire, string pin, CancellationToken stop)
    {
        while (true)
        {
            var m = await wire.Read(stop);
            if (m.Type != "ping" || !store.Peers.Any(p => p.Pin == pin)) throw new ProtocolException("untrusted/command");
            await wire.Send("pong", new { }, stop);
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
        StopPairing(); lifetime.Cancel(); listener?.Stop(); foreach (var s in sessions.Values) s.Cancel();
        if (accepting != null) await accepting;
        try { await Task.WhenAll(tasks.Values).WaitAsync(TimeSpan.FromSeconds(6)); } catch (TimeoutException) { }
    }
}
