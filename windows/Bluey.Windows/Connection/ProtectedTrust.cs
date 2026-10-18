using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Bluey.Link;
namespace Bluey.Windows.Connection;

// All access is off the renderer thread. DPAPI binds the ledger AND private PFX to this Windows user.
internal sealed class ProtectedTrust : ITrustStore
{
    private sealed record Ledger(string HostId, string Pfx, List<Peer> Peers);
    private readonly object gate = new(); private readonly string file; private Ledger ledger;
    public string HostId => ledger.HostId;
    public X509Certificate2 Certificate { get; }
    public IReadOnlyList<Peer> Peers => Volatile.Read(ref ledger).Peers.ToArray();
    internal ProtectedTrust(string? directory = null)
    {
        directory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Bluey", "Link");
        Directory.CreateDirectory(directory); file = Path.Combine(directory, "trust.dpapi");
        if (File.Exists(file))
        {
            var bytes = ProtectedData.Unprotect(File.ReadAllBytes(file), null, DataProtectionScope.CurrentUser);
            try { ledger = JsonSerializer.Deserialize<Ledger>(bytes) ?? throw new IOException("Invalid trust store"); }
            finally { CryptographicOperations.ZeroMemory(bytes); }
            if(!Wire.Uuid(ledger.HostId)) throw new IOException("Invalid host identity");
            if (ledger.Peers.Count > 8 || ledger.Peers.Select(p=>p.Pin).Distinct().Count()!=ledger.Peers.Count) throw new IOException("Invalid trust store");
            foreach(var p in ledger.Peers) { if(!Wire.Uuid(p.DeviceId)||!Wire.Pin(p.Pin)) throw new IOException("Invalid peer"); }
            Certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(ledger.Pfx), null);
            if (!Identity.Valid(Certificate) || !Certificate.HasPrivateKey) throw new IOException("Host identity expired or unavailable");
        }
        else
        {
            var pfx=Identity.CreatePfx("Bluey secure desktop", true);
            try {
                Certificate=X509CertificateLoader.LoadPkcs12(pfx,null);
                ledger = new(Guid.NewGuid().ToString(), Convert.ToBase64String(pfx), []); Save(ledger);
            } finally { CryptographicOperations.ZeroMemory(pfx); }
        }
    }
    private void Save(Ledger value)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        try { File.WriteAllBytes(file + ".new", ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser)); File.Move(file+".new", file, true); }
        finally { CryptographicOperations.ZeroMemory(bytes); }
    }
    public void Add(Peer peer)
    {
        lock(gate) {
            var peers = ledger.Peers.Where(p=>p.Pin != peer.Pin && p.DeviceId != peer.DeviceId).Append(peer).ToList();
            if(peers.Count>8) throw new IOException("Trust limit");
            var next = ledger with { Peers = peers }; Save(next); ledger = next;
        }
    }
    public void Remove(string pin) { lock(gate) { var next=ledger with {Peers=ledger.Peers.Where(p=>p.Pin!=pin).ToList()}; Save(next); ledger=next; } }
}
