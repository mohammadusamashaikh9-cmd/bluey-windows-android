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
            Check(Directory.GetFiles(output, "*.png").Length == 12, "Twelve current offscreen renders produced by actual WPF renderer");
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
        art.UpdateLayout(); var bitmap = new RenderTargetBitmap(1000, 700, 96, 96, PixelFormats.Pbgra32); bitmap.Render(art);
        var pixels = new byte[1000 * 700 * 4]; bitmap.CopyPixels(pixels, 4000, 0);
        if (pixels[3] != 0 || !pixels.Where((_, i) => i % 4 == 3).Any(a => a > 200)) throw new InvalidOperationException("Offscreen renderer lost transparency or character.");
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(output, name + ".png")); encoder.Save(stream);
    }
}
