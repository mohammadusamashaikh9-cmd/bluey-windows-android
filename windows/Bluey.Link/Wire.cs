using System.Text;
using System.Text.Json;
namespace Bluey.Link;

public sealed class ProtocolException(string message) : IOException(message);
public sealed record LinkFace(double GazeX, double GazeY, string Mood, double Talk, long AtMs);
public sealed record Message(string Type, string Id, string Session, long Seq, JsonElement Payload);
public static class Wire
{
    public const int MaxFrame = 4096;
    public const string Service = "_bluey-sec._tcp";
    public static readonly string[] Moods = ["listening", "resting", "thinking", "talking", "pointing", "happy", "sleepy"];
    public static bool Uuid(string s) => Guid.TryParseExact(s, "D", out var g) && g.ToString("D") == s;
    public static bool Pin(string s) => s.Length == 64 && s.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
    public static void Keys(JsonElement e, params string[] keys)
    {
        if (e.ValueKind != JsonValueKind.Object || !e.EnumerateObject().Select(p => p.Name).Order().SequenceEqual(keys.Order())) throw new ProtocolException("fields");
    }
    public static string Text(JsonElement p, string key, int max = 128)
    {
        var e = p.GetProperty(key);
        if (e.ValueKind != JsonValueKind.String || e.GetString() is not { } s || s.Length == 0 || s.Length > max || s.Any(char.IsControl)) throw new ProtocolException("text");
        return s;
    }
    public static long Integer(JsonElement p, string key)
    {
        if (p.GetProperty(key).GetRawText().Any(c => c < '0' || c > '9') || !p.GetProperty(key).TryGetInt64(out var n) || n < 0) throw new ProtocolException("integer"); return n;
    }
    private static void Id(JsonElement p, string key) { if (!Uuid(Text(p, key, 36))) throw new ProtocolException("identity"); }
    public static LinkFace Face(Message m)
    {
        var p = m.Payload;
        double Number(string k, double lo, double hi)
        { if (!p.GetProperty(k).TryGetDouble(out var n) || !double.IsFinite(n) || n < lo || n > hi) throw new ProtocolException("range"); return n; }
        var mood = Text(p, "mood", 16); if (!Moods.Contains(mood)) throw new ProtocolException("mood");
        return new(Number("gazeX", -1, 1), Number("gazeY", -1, 1), mood, Number("talk", 0, 1), Integer(p, "atMs"));
    }
    public static Message Decode(byte[] bytes)
    {
        try
        {
            if (bytes.Length is 0 or > MaxFrame) throw new ProtocolException("size");
            _ = new UTF8Encoding(false, true).GetString(bytes);
            var reader = new Utf8JsonReader(bytes, new JsonReaderOptions { MaxDepth = 4 }); var sets = new Stack<HashSet<string>>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.StartObject) sets.Push([]);
                if (reader.TokenType == JsonTokenType.EndObject) sets.Pop();
                if (reader.TokenType == JsonTokenType.PropertyName && !sets.Peek().Add(reader.GetString()!)) throw new ProtocolException("duplicate");
            }
            using var doc = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 4 }); var e = doc.RootElement;
            Keys(e, "v", "type", "id", "session", "seq", "payload");
            if (Integer(e, "v") != 1) throw new ProtocolException("version"); Id(e, "id"); Id(e, "session");
            var type = Text(e, "type", 16); var p = e.GetProperty("payload");
            var m = new Message(type, Text(e, "id"), Text(e, "session"), Integer(e, "seq"), p.Clone());
            switch (type)
            {
                case "hello": Keys(p, "hostId", "pin"); Id(p, "hostId"); if (!Pin(Text(p, "pin"))) throw new ProtocolException("pin"); break;
                case "connect": Keys(p, "hostId", "deviceId"); Id(p, "hostId"); Id(p, "deviceId"); break;
                case "enroll": Keys(p, "deviceId", "name", "secret"); Id(p, "deviceId"); Text(p, "name", 64); if (!Invitation.IsSecret(Text(p, "secret", 43))) throw new ProtocolException("secret"); break;
                case "approve": Keys(p, "requestId", "clientPin"); Id(p, "requestId"); if (!Pin(Text(p, "clientPin"))) throw new ProtocolException("pin"); break;
                case "confirm": Keys(p, "requestId"); Id(p, "requestId"); break;
                case "ready": Keys(p, "hostId", "deviceId"); Id(p, "hostId"); Id(p, "deviceId"); break;
                case "presentation": PresentationCodec.Validate(p); break;
                case "face": Keys(p, "gazeX", "gazeY", "mood", "talk", "atMs"); Face(m); break;
                case "ping": case "pong": Keys(p); break;
                case "error": Keys(p, "code"); if (!new[] { "denied", "expired", "revoked", "protocol", "busy" }.Contains(Text(p, "code"))) throw new ProtocolException("error"); break;
                default: throw new ProtocolException("command");
            }
            return m;
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        { throw new ProtocolException("encoding/schema"); }
    }
    public static byte[] Encode(string type, string session, long seq, object payload, string? id = null)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new { v = 1, type, id = id ?? Guid.NewGuid().ToString(), session, seq, payload });
        Decode(bytes); return [.. bytes, 10];
    }
}
public sealed class Frames
{
    private readonly byte[] buffer = new byte[Wire.MaxFrame]; private int count;
    public IReadOnlyList<byte[]> Feed(ReadOnlySpan<byte> bytes)
    {
        var frames = new List<byte[]>();
        foreach (var b in bytes)
        {
            if (b == 10) { if (count == 0) throw new ProtocolException("empty"); frames.Add(buffer[..count]); count = 0; }
            else { if (count == buffer.Length) throw new ProtocolException("size"); buffer[count++] = b; }
        }
        return frames;
    }
    public void End() { if (count != 0) throw new ProtocolException("incomplete"); }
}
public sealed class SequenceGuard(string session)
{
    private long next; private readonly HashSet<string> ids = []; private readonly Queue<string> recent = [];
    private long? firstAt, firstLocal, lastAt;
    public void Accept(Message m)
    {
        if (m.Session != session || m.Seq != next || !ids.Add(m.Id)) throw new ProtocolException("sequence");
        next++; recent.Enqueue(m.Id); if (recent.Count > 256) ids.Remove(recent.Dequeue());
    }
    public void Fresh(LinkFace f, long localMs)
    {
        if (lastAt is { } last && f.AtMs < last) throw new ProtocolException("stale"); lastAt = f.AtMs;
        firstAt ??= f.AtMs; firstLocal ??= localMs;
        var lag = (localMs - firstLocal.Value) - (f.AtMs - firstAt.Value);
        if (lag > 2500 || lag < -5000) throw new ProtocolException("stale");
    }
}
