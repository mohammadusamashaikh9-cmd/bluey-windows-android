using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
namespace Bluey.Link;
public static class Identity
{
    public static string Fingerprint(X509Certificate2 c) => Convert.ToHexStringLower(SHA256.HashData(c.RawData));
    public static bool Valid(X509Certificate2? c) => c != null && c.NotBefore.ToUniversalTime() <= DateTime.UtcNow && c.NotAfter.ToUniversalTime() > DateTime.UtcNow && c.GetECDsaPublicKey() != null;
    public static X509Certificate2 Create(string name, bool server)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=" + name, key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(server ? "1.3.6.1.5.5.7.3.1" : "1.3.6.1.5.5.7.3.2") }, false));
        using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
        return X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pkcs12), null);
    }
}
public sealed record Peer(string DeviceId, string Name, string Pin);
public interface ITrustStore
{
    string HostId { get; }
    X509Certificate2 Certificate { get; }
    IReadOnlyList<Peer> Peers { get; }
    void Add(Peer peer);
    void Remove(string pin);
}
public sealed class Invitation(Func<long> clock)
{
    private readonly object gate = new(); private string? secret; private long until; private int attempts; private long generation;
    public long Generation { get { lock (gate) return generation; } }
    public bool Active { get { lock (gate) return secret != null && clock() < until && attempts < 8; } }
    public string Start() { lock (gate) { generation++; secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+','-').Replace('/','_'); until = clock() + 300_000; attempts = 0; return secret; } }
    public void Stop() { lock (gate) { generation++; secret = null; } }
    public static bool IsSecret(string s) => s.Length == 43 && s.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
    public bool Check(string candidate)
    {
        lock (gate)
        {
            if (!Active) return false; attempts++;
            return IsSecret(candidate) && CryptographicOperations.FixedTimeEquals(System.Text.Encoding.ASCII.GetBytes(candidate), System.Text.Encoding.ASCII.GetBytes(secret!));
        }
    }
    public bool Consume(long expectedGeneration) { lock (gate) { if (generation != expectedGeneration || !Active) return false; secret = null; return true; } }
}
public static class FaceFilter
{
    public static bool Significant(LinkFace? old, LinkFace current, long lastSent, long now) => old == null || now - lastSent >= 1000 || old.Mood != current.Mood ||
        Math.Abs(old.GazeX-current.GazeX) >= .004 || Math.Abs(old.GazeY-current.GazeY) >= .004 || Math.Abs(old.Talk-current.Talk) >= .02;
}
