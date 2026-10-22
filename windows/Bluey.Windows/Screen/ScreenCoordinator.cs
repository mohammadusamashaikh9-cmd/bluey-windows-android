using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Bluey.Core;
using Bluey.Screen;
namespace Bluey.Windows.Screen;
internal sealed class ScreenCoordinator : IDisposable
{
    private readonly OverlayWindow overlay;private readonly CursorEngine engine;private readonly Connection.WindowsLinkHost host;
    private readonly NativeCapture capture=new();private readonly NativeAnalysis analysis=new();private readonly ScreenWorkflow workflow;
    private readonly Pointing pointing=new();private Surface? pointSurface;private PointDirective? remotePoint;private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(100)};
    internal ScreenSession State=>host.Live.Screens;internal bool Busy=>workflow.Busy;
    internal string Status{get;private set;}="Screen capture: OFF · cloud screen sharing: OFF";
    internal ScreenCoordinator(OverlayWindow overlay,CursorEngine engine,Connection.WindowsLinkHost host) {
        this.overlay=overlay;this.engine=engine;this.host=host;workflow=new(State);host.Live.PointRequested+=Receive;
        overlay.DisplayChanged+=DisplayChanged;timer.Tick+=(_,_)=>Tick();timer.Start();
    }
    private void DisplayChanged(){Stop();Status="Display/DPI changed — screenshot discarded; capture again.";}
    private void Receive(PointDirective directive)=>overlay.Dispatcher.BeginInvoke(()=>{
        if(!host.Live.CanPresent(directive))return;remotePoint=directive;pointSurface=directive.Surface;pointing.Enqueue(directive.Point,Environment.TickCount64);
    });
    internal SurfaceChoice[] Choices()=>NativeCapture.Choices(overlay.Coordinates.DpiX);
    internal async Task Capture(SurfaceChoice selected,Window progress)
    {
        var hidden=Application.Current.Windows.Cast<Window>().Where(w=>w!=progress&&w.IsVisible).ToArray();
        try {
            // Keep the nonsensitive cancel/status window reachable; exclusion is best effort.
            foreach(var window in hidden)window.Hide();
            Status="Screen capture: Capturing ONE snapshot · Cancel remains available";
            var completed=await workflow.Capture(true,new Source(capture,selected));
            Status=completed?"Screen capture: Preview ready · cloud sharing OFF · review the actual pixels":"Screen capture: OFF — canceled or superseded";
        } catch {Status="Screen capture: OFF — canceled/denied/unavailable; no snapshot";throw;}
        finally {foreach(var window in hidden)try{window.Show();}catch(InvalidOperationException){}}
    }
    private sealed class Source(NativeCapture native,SurfaceChoice selection):ISnapshotSource {public Task<Snapshot> Capture(CancellationToken stop)=>Task.Run(()=>native.Capture(selection,stop),stop);}
    private sealed class Reader(NativeAnalysis native):ILocalReader {public Task<Analysis> Analyze(ScreenMedia media,CancellationToken stop)=>Task.Run(()=>native.Analyze(media,stop),stop);}
    internal async Task Analyze() {
        try{await workflow.Analyze(new Reader(analysis));}catch{Status="Local OCR: Failed/canceled — no recognition result published";if(State.Info is {} failed)State.SetAnalysis(failed.Id,new([],[],false,OcrStatus.Failed));throw;}
        Status=State.Info?.Ocr switch {
            OcrStatus.Ready=>"Local OCR: Ready · trusted word rectangles · UIA labels only",
            OcrStatus.UnavailableIdentity=>"Local OCR: Unavailable — Microsoft requires package identity. Portable build has none. UIA targets, if available, are listed separately.",
            OcrStatus.UnavailableLanguage=>"Local OCR: No installed OCR language available. No language/model download was started.",
            _=>"Local OCR: Failed/unavailable — no fabricated recognition results"
        };
    }
    internal BitmapSource Preview() {
        using var media=State.CopyCurrent();var bytes=media.Jpeg.ToArray();
        try {using var stream=new MemoryStream(bytes,false);var bitmap=new BitmapImage();bitmap.BeginInit();bitmap.CacheOption=BitmapCacheOption.OnLoad;bitmap.StreamSource=stream;bitmap.EndInit();bitmap.Freeze();State.MarkPreviewReady(media.Info.Id);return bitmap;}
        finally{System.Security.Cryptography.CryptographicOperations.ZeroMemory(bytes);}
    }
    internal void Point(string id,string target) {var info=State.Info??throw new ScreenException("stale-snapshot");var p=State.PointAtTarget(id,target);pointSurface=info.Surface;remotePoint=null;pointing.Enqueue(p,Environment.TickCount64);Tick();}
    internal void PointSpot(string id,double x,double y){var info=State.Info??throw new ScreenException("stale-snapshot");var p=State.PointAtSpot(id,x,y);pointSurface=info.Surface;remotePoint=null;pointing.Enqueue(p,Environment.TickCount64);Tick();}
    internal void Home(){pointing.Stop();remotePoint=null;pointSurface=null;engine.GoHome();overlay.Art.SpeechTarget=null;}
    internal void Stop(){State.Invalidate();host.StopLive();Home();Status="Screen capture: OFF · discarded · cloud sharing OFF";}
    private void Tick() {
        State.Expire();
        if(pointSurface is {} geometry&&(!NativeCapture.Matches(geometry)||remotePoint is {} d&&!host.Live.CanPresent(d))){State.Invalidate();Home();}
        if(State.Info is {} info&&!NativeCapture.Matches(info.Surface)){Stop();Status="Selected window moved/resized or display changed — screenshot discarded.";}
        var step=pointing.Tick(Environment.TickCount64,host.Server?.Presentation.State=="speaking");
        if(step.Point is {} p&&pointSurface is {} s&&NativeCapture.Matches(s)) {
            overlay.Point(new(p.Position.X,p.Position.Y));overlay.Art.SpeechTarget=new(p.Showing.X,p.Showing.Y,p.Showing.Width,p.Showing.Height);
        }
        if(step.Home)Home();
    }
    internal void SyntheticDemo() {
        var snapshot=SyntheticFixture.Create(overlay.Coordinates.DpiX);State.CompleteCapture(State.BeginCapture(true),snapshot);
        State.SetAnalysis(snapshot.Id,SyntheticFixture.Annotations(snapshot.Surface));Status="Synthetic fixture — public generated pixels and declared target annotations, NOT a real capture or OCR result. Cloud sharing disabled.";
    }
    public void Dispose(){timer.Stop();host.Live.PointRequested-=Receive;overlay.DisplayChanged-=DisplayChanged;Stop();}
}
internal static class OwnPrivacy
{
    internal static void Protect(Window window){window.SourceInitialized+=(_,_)=>NativeCapture.Exclude(new System.Windows.Interop.WindowInteropHelper(window).Handle);}
}
internal static class SyntheticFixture
{
    internal static Snapshot Create(double dpi) {
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen()) {
            dc.DrawRectangle(Brushes.White,null,new Rect(0,0,800,450));
            dc.DrawText(new FormattedText("ALPHA BETA",CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),40,Brushes.Black,1),new(50,50));
            dc.DrawRectangle(Brushes.LightBlue,new Pen(Brushes.Black,2),new(50,160,150,50));
            dc.DrawText(new FormattedText("Save",CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),28,Brushes.Black,1),new(85,166));
        }
        var bitmap=new RenderTargetBitmap(800,450,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var encoder=new JpegBitmapEncoder{QualityLevel=72};encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream=new MemoryStream();encoder.Save(stream);var b=Native.PrimaryBounds();var primary=new Box(b.Left,b.Top,b.Right-b.Left,b.Bottom-b.Top);
        var width=Math.Min(800,primary.Width*.75);var height=width*450/800;if(height>primary.Height*.75){height=primary.Height*.75;width=height*800/450;}
        return new(new(SurfaceKind.Synthetic,0,new(primary.X+10,primary.Y+10,width,height),primary,dpi,dpi,800,450),stream.ToArray(),Environment.TickCount64,DateTimeOffset.UtcNow);
    }
    internal static Analysis Annotations(Surface surface)=>new([new("ALPHA BETA",new(50,50,270,50),[new("ALPHA",new(50,50,130,50)),new("BETA",new(190,50,130,50))])],[new("button","Save",surface.ImageToPhysical(new(50,160,150,50)),true,false)],true,OcrStatus.SyntheticAnnotations);
}
