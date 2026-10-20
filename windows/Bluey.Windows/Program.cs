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
        var host = new Connection.WindowsLinkHost();
        var voice = new VoicePresentation(overlay.Art, host, engine);
        Connection.ConnectionWindow? connection = null;
        var controls = new ControlsWindow(engine, overlay, visuals, () => {
            if (connection == null) { connection = new(host); connection.Closed += (_,_) => connection = null; connection.Show(); }
            else connection.Activate();
        }, () => voice.Preview(engine.Now));
        overlay.Frame += () => { voice.Tick(engine.Now); host.Publish(engine.Face(engine.Now)); };
        app.Exit += (_,_) => { connection?.Close(); Task.Run(host.Stop).GetAwaiter().GetResult(); };
        app.MainWindow = controls;
        engine.GoHome(); overlay.Show(); controls.Show();
        return app.Run();
    }
}
