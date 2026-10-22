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
        var screenState=new Bluey.Screen.ScreenSession(surfaceCurrent:s=>Screen.NativeCapture.Matches(s)&&Math.Abs(Native.GetDpiForWindow(overlay.Handle)-s.DpiX)<.1);
        var host = new Connection.WindowsLinkHost(screenState);
        var screen=new Screen.ScreenCoordinator(overlay,engine,host);Screen.ScreenWindow? screenWindow=null;
        var voice = new VoicePresentation(overlay.Art, host, engine);
        Connection.ConnectionWindow? connection = null; AI.AiWindow? ai=null;
        var controls = new ControlsWindow(engine, overlay, visuals, () => {
            if (connection == null) { connection = new(host); Screen.OwnPrivacy.Protect(connection);connection.Closed += (_,_) => connection = null; connection.Show(); }
            else connection.Activate();
        }, () => voice.Preview(engine.Now), () => {if(ai==null){ai=new(host);Screen.OwnPrivacy.Protect(ai);ai.Closed+=(_,_)=>ai=null;ai.Show();}else ai.Activate();},()=>{if(screenWindow==null){screenWindow=new(screen,host);screenWindow.Closed+=(_,_)=>screenWindow=null;screenWindow.Show();}else screenWindow.Activate();},screen.Stop);
        overlay.Frame += () => { voice.Tick(engine.Now); host.Publish(engine.Face(engine.Now)); };
        app.Exit += (_,_) => { screenWindow?.Close();screen.Dispose();ai?.Close(); connection?.Close(); Task.Run(host.Stop).GetAwaiter().GetResult(); };
        app.MainWindow = controls;
        engine.GoHome(); overlay.Show(); controls.Show();
        return app.Run();
    }
}
