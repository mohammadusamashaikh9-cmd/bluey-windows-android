using System.Diagnostics;
using System.Windows;
using Bluey.Core;

namespace Bluey.Windows;
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        if (args.Length == 2 && args[0] == "--self-test") return NativeSmoke.Run(args[1]);
        var timer = Stopwatch.StartNew(); var settings = SettingsStore.Load(); var random = new Random();
        var engine = new CursorEngine(settings, () => timer.Elapsed.TotalSeconds, random.NextDouble);
        var visuals = new VisualState(engine, random.NextDouble);
        var overlay = new OverlayWindow(engine, visuals);
        var controls = new ControlsWindow(engine, overlay, visuals);
        app.MainWindow = controls;
        engine.GoHome(); overlay.Show(); controls.Show();
        return app.Run();
    }
}
