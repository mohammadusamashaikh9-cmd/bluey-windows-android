using System.Net.Security;
namespace Bluey.Link;
public sealed class WireConnection(SslStream stream, string session)
{
    private readonly Frames frames = new(); private readonly MessageRate rate=new(); private readonly System.Diagnostics.Stopwatch clock=System.Diagnostics.Stopwatch.StartNew(); private readonly Queue<byte[]> waiting = new();
    private readonly SemaphoreSlim sendGate = new(1); private readonly SequenceGuard incoming = new(session); private long sequence;
    public async Task<Message> Read(CancellationToken stop, int timeoutMs = 5000)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop); deadline.CancelAfter(timeoutMs);
        while (waiting.Count == 0)
        {
            var buffer = new byte[1024]; var n = await stream.ReadAsync(buffer, deadline.Token);
            if (n == 0) { frames.End(); throw new EndOfStreamException(); }
            foreach (var f in frames.Feed(buffer.AsSpan(0, n))) waiting.Enqueue(f);
        }
        rate.Accept(clock.ElapsedMilliseconds); var message = Wire.Decode(waiting.Dequeue()); incoming.Accept(message); return message;
    }
    public void SeedIncoming(Message hello) => incoming.Accept(hello);
    public async Task<string> Send(string type, object payload, CancellationToken stop)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop); deadline.CancelAfter(2000);
        await sendGate.WaitAsync(deadline.Token);
        try { var id = Guid.NewGuid().ToString(); await stream.WriteAsync(Wire.Encode(type, session, sequence++, payload, id), deadline.Token); await stream.FlushAsync(deadline.Token); return id; }
        finally { sendGate.Release(); }
    }
}
