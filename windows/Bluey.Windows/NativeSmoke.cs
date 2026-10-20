using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Bluey.Core;

namespace Bluey.Windows;
// CI-only real HWND/API checks and offscreen vector renders. No desktop capture/input injection.
internal static class NativeSmoke
{
    internal static int Run(string output)
    {
        Directory.CreateDirectory(output);
        var checks = new List<string>();
        OverlayWindow? window = null;
        void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException(name); checks.Add(name); }
        try
        {
            var trustDirectory=Path.Combine(Path.GetTempPath(),"bluey-link-check-"+Guid.NewGuid());
            try {
                var first=new Connection.ProtectedTrust(trustDirectory);
                var peer=new Bluey.Link.Peer(Guid.NewGuid().ToString(),"CI synthetic device",new string('a',64));
                first.Add(peer);var second=new Connection.ProtectedTrust(trustDirectory);
                Check(first.HostId==second.HostId && Bluey.Link.Identity.Fingerprint(first.Certificate)==Bluey.Link.Identity.Fingerprint(second.Certificate) && second.Certificate.HasPrivateKey && second.Peers.Single()==peer,
                    "Actual CurrentUser DPAPI identity/trust roundtrip retains key and peer");
                second.Remove(peer.Pin);var third=new Connection.ProtectedTrust(trustDirectory);
                Check(third.Peers.Count==0,"DPAPI revocation persists across store reload");
                var protectedBytes=File.ReadAllBytes(Path.Combine(trustDirectory,"trust.dpapi"));protectedBytes[0]^=255;
                File.WriteAllBytes(Path.Combine(trustDirectory,"trust.dpapi"),protectedBytes);
                var rejected=false;try{_ = new Connection.ProtectedTrust(trustDirectory);}catch(System.Security.Cryptography.CryptographicException){rejected=true;}
                Check(rejected,"Corrupted protected trust fails closed without identity replacement");
                first.Certificate.Dispose();second.Certificate.Dispose();third.Certificate.Dispose();
            } finally { if(Directory.Exists(trustDirectory))Directory.Delete(trustDirectory,true); }
            var keyDirectory=Path.Combine(Path.GetTempPath(),"bluey-ai-check-"+Guid.NewGuid());
            try {
                var key=new AI.ProtectedApiKey(keyDirectory);var firstKey=System.Text.Encoding.UTF8.GetBytes("offline-test-master-placeholder");
                Check(key.Status=="Not configured","Live master key starts unconfigured");key.Set(firstKey,false);
                var read=key.Read();Check(read.SequenceEqual(firstKey)&&key.Status.StartsWith("Configured"),"Actual CurrentUser DPAPI key roundtrip without redisplay");System.Security.Cryptography.CryptographicOperations.ZeroMemory(read);
                var encrypted=File.ReadAllBytes(Path.Combine(keyDirectory,"openai.dpapi"));Check(!System.Text.Encoding.UTF8.GetString(encrypted).Contains("offline-test-master"),"Credential file contains DPAPI ciphertext only");
                var replaced=false;try{key.Set(firstKey,false);}catch(IOException){replaced=true;}Check(replaced,"Configured key requires explicit Replace");
                var secondKey=System.Text.Encoding.UTF8.GetBytes("offline-replacement-placeholder");key.Set(secondKey,true);read=key.Read();Check(read.SequenceEqual(secondKey),"Protected key replacement is atomic and readable");System.Security.Cryptography.CryptographicOperations.ZeroMemory(read);
                encrypted=File.ReadAllBytes(Path.Combine(keyDirectory,"openai.dpapi"));encrypted[0]^=255;File.WriteAllBytes(Path.Combine(keyDirectory,"openai.dpapi"),encrypted);Check(key.Status.StartsWith("Unavailable"),"Corrupted master key fails closed without regeneration");
                key.Remove();Check(key.Status=="Not configured"&&!File.Exists(Path.Combine(keyDirectory,"openai.dpapi")),"Explicit key Remove removes protected credential");
                System.Security.Cryptography.CryptographicOperations.ZeroMemory(firstKey);System.Security.Cryptography.CryptographicOperations.ZeroMemory(secondKey);
            }finally{if(Directory.Exists(keyDirectory))Directory.Delete(keyDirectory,true);}
            var e = new CursorEngine(new(), () => 0, () => .5); var v = new VisualState(e, () => .5);
            window = new(e, v); var before = Native.GetForegroundWindow(); window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            var style = (long)Native.GetWindowLongPtrW(window.Handle, Native.GwlExStyle);
            var required = Native.Transparent | Native.NoActivate | Native.ToolWindow | Native.Layered;
            Check((style & required) == required, "Actual HWND has layered, transparent, noactivate and toolwindow styles");
            Check(Native.IsWindowVisible(window.Handle), "Overlay HWND is visible");
            Check(Native.GetForegroundWindow() == before, "Show did not change foreground HWND in CI session");
            Check(Native.SendMessageW(window.Handle, Native.WmNcHitTest, 0, 0) == -1, "Actual WM_NCHITTEST handler returns HTTRANSPARENT");
            Check(Native.SendMessageW(window.Handle, Native.WmMouseActivate, 0, 0) == 3, "Actual WM_MOUSEACTIVATE handler returns MA_NOACTIVATE");
            window.FitPrimary(); // Exercise a refit with a live HWND and compositor callbacks.
            var b = Native.PrimaryBounds(); var dpi = Native.GetDpiForWindow(window.Handle);
            Check(Math.Abs(window.Width - (b.Right - b.Left) * 96.0 / dpi) < .1, "Primary HWND logical width follows physical monitor/DPI");
            window.Hide(); window.Show(); Check(Native.IsWindowVisible(window.Handle), "Hidden native overlay can be shown again");
            window.Close(); window = null;
            var face = new Typeface(VectorDrawing.DisplayFont, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            Check(face.TryGetGlyphTypeface(out var glyph) && glyph.FamilyNames.Values.Any(n => n.Contains("Fredoka", StringComparison.OrdinalIgnoreCase)), "Bundled Fredoka resolves without system installation");
            foreach (var size in new double[] { 48, 72, 96, 120 })
                foreach (var glow in new[] { false, true }) Render(output, $"cursor-{size}-{glow}", size, glow, PointerTrail.None, false);
            Render(output, "comet-flight", 72, true, PointerTrail.Comet, false, true);
            Render(output, "string-flight", 72, true, PointerTrail.String, false, true);
            Render(output, "speech-top", 72, false, PointerTrail.None, true);
            Render(output, "speech-center", 72, false, PointerTrail.None, false);
            Render(output, "voice-preview", 72, false, PointerTrail.None, false);
            Check(Directory.GetFiles(output, "*.png").Length == 13, "Thirteen current offscreen renders including offline voice caption");
            var previewArt=new OverlayArt(e,v);var previewHost=new Connection.WindowsLinkHost();var preview=new VoicePresentation(previewArt,previewHost,e);
            preview.Preview(0);preview.Tick(.69);Check(previewArt.CurrentCaption==null,"Offline preview has deterministic thinking delay");
            preview.Tick(.7);Check(previewArt.CurrentCaption=="This is an offline ","Offline preview starts with a caption delta");
            preview.Tick(1.3);Check(previewArt.CurrentCaption!.EndsWith("No audio or cloud is active."),"Offline preview streams complete bounded mock caption");
            preview.Tick(30);Check(previewArt.CurrentCaption==null && previewHost.Server==null,"Preview clears after source reading time without starting a listener");
            File.WriteAllText(Path.Combine(output, "native-smoke.json"), JsonSerializer.Serialize(new { passed = checks.Count, failed = 0, checks,
                platform = Environment.OSVersion.ToString(), desktopAcceptance = "UNVERIFIED: physical click routing, focus, DPI changes, capture, fidelity, pacing, CPU" }, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        catch (Exception error)
        {
            window?.Close(); File.WriteAllText(Path.Combine(output, "native-smoke-failure.txt"), error.ToString()); return 1;
        }
    }
    private static void Render(string output, string name, double size, bool glow, PointerTrail trail, bool top, bool flying = false)
    {
        var settings = new CursorSettings { Size = size, Glow = glow, Trail = trail };
        var e = new CursorEngine(settings, () => 0, () => .5); var v = new VisualState(e, () => .5);
        var art = new OverlayArt(e, v) { Width = 1000, Height = 700 };
        art.Measure(new(1000, 700)); art.Arrange(new(0, 0, 1000, 700)); art.UpdateLayout();
        e.Step(0, default, new(1000, 700)); art.Advance(); e.SetMode(CursorMode.Pinned, new(400, top ? 20 : 320));
        var end = flying ? .3 : 2;
        for (var i = 1; i <= end * 60; i++) { e.Step(i / 60.0, default, new(1000, 700)); art.Advance(); }
        if (name.StartsWith("speech", StringComparison.Ordinal))
        {
            art.Caption("Hello! I'm Bluey. This is a native vector speech bubble."); art.Label("Offline demo");
            for (var i = 1; i <= 60; i++) { e.Step(end + i / 60.0, default, new(1000, 700)); art.Advance(); }
        }
        if(name=="voice-preview") {
            var adapter=new VoicePresentation(art,new Connection.WindowsLinkHost(),e);adapter.Preview(e.Now);
            for(var i=1;i<=100;i++){e.Step(end+i/60.0,default,new(1000,700));adapter.Tick(e.Now);art.Advance();}
        }
        art.UpdateLayout(); var bitmap = new RenderTargetBitmap(1000, 700, 96, 96, PixelFormats.Pbgra32); bitmap.Render(art);
        var pixels = new byte[1000 * 700 * 4]; bitmap.CopyPixels(pixels, 4000, 0);
        if (pixels[3] != 0 || !pixels.Where((_, i) => i % 4 == 3).Any(a => a > 200)) throw new InvalidOperationException("Offscreen renderer lost transparency or character.");
        var interior = new V2(size * .5, size * .8).Rotate(e.Angle) * e.PressScale(e.Now) + e.Tip + new V2(0, e.Hover(e.Now));
        var offset = ((int)Math.Round(interior.Y) * 1000 + (int)Math.Round(interior.X)) * 4;
        if (pixels[offset + 3] < 240 || pixels[offset] <= pixels[offset + 2] + 5)
            throw new InvalidOperationException("WPF vector body interior must be opaque blueberry, not just eyes/shadow.");
        // Fixed speech fixtures place body, speech and label entirely left of x=800.
        // This catches a FormattedText alignment region larger than its measured box.
        if (name.StartsWith("speech", StringComparison.Ordinal))
            for (var y = 0; y < 700; y++)
                for (var x = 800; x < 1000; x++)
                    if (pixels[(y * 1000 + x) * 4 + 3] > 0)
                        throw new InvalidOperationException("Speech/label glyphs escaped their measured fixture boxes.");
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(output, name + ".png")); encoder.Save(stream);
    }
}
