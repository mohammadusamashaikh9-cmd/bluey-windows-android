using System.Diagnostics;
using System.Windows;
using Bluey.Core;

namespace Bluey.Windows;
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--self-test")
        {
            System.IO.Directory.CreateDirectory(args[1]);
            AppDomain.CurrentDomain.UnhandledException += (_, failure) =>
                System.IO.File.WriteAllText(System.IO.Path.Combine(args[1], "native-smoke-fatal.txt"), failure.ExceptionObject.ToString());
            _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            return NativeSmoke.Run(args[1]);
        }
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
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
