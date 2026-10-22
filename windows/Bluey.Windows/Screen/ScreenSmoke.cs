using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Bluey.Core;
using Bluey.Screen;
namespace Bluey.Windows.Screen;
internal static class ScreenSmoke
{
    internal static string[] Run(string output) {
        var checks=new List<string>();void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);checks.Add(name);}
        using var state=new ScreenSession();using var shot=SyntheticFixture.Create(96);var id=shot.Id;state.CompleteCapture(state.BeginCapture(true),shot);state.SetAnalysis(id,SyntheticFixture.Annotations(shot.Surface));
        using var media=state.CopyCurrent();
        var encoded=Pump(Task.Run(async()=>{using var bitmap=await NativeCapture.Decode(media.Jpeg,default);NativeCapture.EnsureNonblank(bitmap);using var blank=new global::Windows.Graphics.Imaging.SoftwareBitmap(global::Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,32,32,global::Windows.Graphics.Imaging.BitmapAlphaMode.Ignore);try{NativeCapture.EnsureNonblank(blank);throw new InvalidOperationException("uniform bitmap must fail");}catch(ScreenException){}return await NativeCapture.Encode(bitmap,default);}));
        Check(encoded.Length is >100 and <=Snapshot.MaxJpegBytes,"Native WinRT JPEG decode/encode on public synthetic pixels respects bounded media");
        System.Security.Cryptography.CryptographicOperations.ZeroMemory(encoded);
        var analysis=Pump(Task.Run(()=>new NativeAnalysis().Analyze(media,default)));
        if(!NativeCapture.HasPackageIdentity)Check(analysis.Ocr==OcrStatus.UnavailableIdentity&&analysis.Lines.Length==0,"Portable OCR fails explicitly on documented package-identity requirement; no fabricated lines");
        else Check(analysis.Ocr is OcrStatus.Ready or OcrStatus.UnavailableLanguage,"Packaged native OCR reports actual installed-language availability");
        var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.StreamSource=new MemoryStream(media.Jpeg.ToArray());image.EndInit();image.Freeze();
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using(var file=File.Create(Path.Combine(output,"screen-SYNTHETIC-only.png")))encoder.Save(file);
        var known=state.PointAtTarget(id,"W1");var engine=new CursorEngine(new(),()=>0,()=>.5);engine.Step(0,default,new(1920,1080));engine.SetMode(CursorMode.Pinned,new(known.Position.X,known.Position.Y));engine.Step(.01,default,new(1920,1080));
        Check(engine.CurrentFlight!=null&&engine.Mode==CursorMode.Pinned,"Validated synthetic OCR target starts existing Bezier flight without an OS input call");
        engine.GoHome();Check(engine.IsHome,"Return Home uses existing pure engine mode");
        var password=new PasswordBox{Password="public-synthetic-password"};AutomationProperties.SetName(password,"SECURE-FIELD-MUST-NOT-BE-READ");
        var panel=new StackPanel();panel.Children.Add(new Button{Content="PUBLIC SAVE"});panel.Children.Add(password);panel.Children.Add(new TextBlock{Text="ALPHA BETA"});
        var window=new Window{Title="Bluey CI public UIA fixture",Width=300,Height=200,Left=40,Top=40,Content=panel};
        try {
            window.Show();window.UpdateLayout();var handle=new WindowInteropHelper(window).Handle;GetWindowRect(handle,out var r);var primary=Native.PrimaryBounds();
            var rect=new Box(r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top);var surface=new Surface(SurfaceKind.Window,(long)handle,rect,new(primary.Left,primary.Top,primary.Right-primary.Left,primary.Bottom-primary.Top),Native.GetDpiForWindow(handle),Native.GetDpiForWindow(handle),(int)rect.Width,(int)rect.Height);
            var result=Pump(new NativeControls().Read(surface,default));
            Check(!result.TimedOut&&result.Controls.Any(c=>c.Label=="PUBLIC SAVE"),"Actual Windows UIA reads public fixture button label/rectangle");
            Check(!result.Controls.Any(c=>c.Label.Contains("SECURE-FIELD")),"Actual Windows UIA omits password field before reading its accessible name");
        } finally {window.Close();}
        File.WriteAllText(Path.Combine(output,"screen-native-evidence.json"),JsonSerializer.Serialize(new{checks,packageIdentity=NativeCapture.HasPackageIdentity,ocr=analysis.Ocr.ToString(),capture="NOT EXECUTED: no desktop/private screenshots in CI; native WGC adapter compiled",provider="FAKE ONLY; no Gemini request",physicalAcceptance="UNVERIFIED"},new JsonSerializerOptions{WriteIndented=true}));
        return checks.ToArray();
    }
    private static T Pump<T>(Task<T> task) {
        var frame=new DispatcherFrame();var dispatcher=Dispatcher.CurrentDispatcher;
        _=task.ContinueWith(_=>dispatcher.BeginInvoke(()=>frame.Continue=false),TaskScheduler.Default);
        Dispatcher.PushFrame(frame);return task.GetAwaiter().GetResult();
    }
    [DllImport("user32.dll")]private static extern bool GetWindowRect(nint hwnd,out Native.Rect rect);
}
