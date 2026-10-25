using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Bluey.Core;
using Bluey.Screen;
namespace Bluey.Windows.Screen;
// Explicit developer test only. Generated public pixels, real OCR; no WGC, user data or network.
internal static class OcrSmoke
{
    internal static int Run(string output)
    {
        Directory.CreateDirectory(output);var checks=new List<string>();int lines=0,words=0;string stage="Identity";
        int Report(int exit,string status,int? hr=null) {
            File.WriteAllText(Path.Combine(output,"ocr-native-evidence.json"),JsonSerializer.Serialize(new{status,stage,hresult=hr is {} h?"0x"+h.ToString("X8"):null,lines,words,checks,fixture="Generated public four-line image; no capture",physicalNotepad="UNVERIFIED",provider="NONE; no listener/key/cloud work"},new JsonSerializerOptions{WriteIndented=true}));return exit;
        }
        void Check(bool condition,string code){if(!condition)throw new ScreenException(code);checks.Add(code);}
        try {
            if(!NativeCapture.HasPackageIdentity)return Report(3,"UnavailableIdentity");
            OcrIdentity.Require();checks.Add("Expected identity and original LocalAppData");stage="GeneratePublicPixels";
            string[] expected=["BLUEY SCREEN TEST","ALPHA BETA","12345","SCREEN CAPTURE WORKING"];
            var visual=new DrawingVisual();using(var dc=visual.RenderOpen()) {
                dc.DrawRectangle(Brushes.White,null,new Rect(0,0,900,360));
                for(int i=0;i<expected.Length;i++)dc.DrawText(new FormattedText(expected[i],CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),36,Brushes.Black,1),new(40,30+i*75));
            }
            var bitmap=new RenderTargetBitmap(900,360,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
            var encoder=new JpegBitmapEncoder{QualityLevel=72};encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream=new MemoryStream();encoder.Save(stream);
            var surface=new Surface(SurfaceKind.Synthetic,0,new(80,80,900,360),new(0,0,1920,1080),120,120,900,360);
            long now=0;using var state=new ScreenSession(()=>now);using var shot=new Snapshot(surface,stream.ToArray(),0,DateTimeOffset.UnixEpoch);
            state.CompleteCapture(state.BeginCapture(true),shot);using var media=state.CopyCurrent();stage="NativeOcr";
            var analysis=Pump(Task.Run(()=>new NativeAnalysis().Analyze(media,default)));
            if(analysis.Ocr!=OcrStatus.Ready)return Report(3,analysis.Ocr.ToString());
            state.SetAnalysis(shot.Id,analysis);var targets=state.Info!.Targets;
            var actual=targets.Where(t=>t.Role=="line").ToArray();lines=actual.Length;words=targets.Count(t=>t.Role=="word");
            Check(actual.Select(t=>t.Text).SequenceEqual(expected),"Four expected native OCR lines in reading order");
            Check(actual.Select(t=>t.Id).SequenceEqual(new[]{"L1","L2","L3","L4"}),"Stable L1-L4 IDs");
            Check(words==9,"Nine actual OCR word targets");
            for(int i=0;i<actual.Length;i++)Check(new Box(30,20+i*75,800,65).Contains(actual[i].ImageBounds),"Line rectangle within generated text band "+i);
            Check(targets.All(t=>surface.Image.Contains(t.ImageBounds)&&surface.Physical.Contains(t.PhysicalBounds)&&t.DipBounds.Valid),"OCR rectangles survive image/physical/DIP conversion");
            var p=state.PointAtTarget(shot.Id,"W1");var engine=new CursorEngine(new(),()=>0,()=>.5);engine.Step(0,default,new(1920,1080));engine.SetMode(CursorMode.Pinned,new(p.Position.X,p.Position.Y));engine.Step(.01,default,new(1920,1080));
            Check(engine.CurrentFlight!=null,"Native OCR word triggers existing pure animated flight");engine.GoHome();Check(engine.IsHome,"Return Home without OS input");
            Check(state.TakeForQuestion("tls","c","a","turn")==null,"No unapproved screen sharing");
            state.Invalidate();Check(state.Info==null&&shot.Disposed,"Stop discards native OCR targets and pixels");
            using var expiring=new Snapshot(surface,stream.ToArray(),0,DateTimeOffset.UnixEpoch);state.CompleteCapture(state.BeginCapture(true),expiring);state.SetAnalysis(expiring.Id,analysis);now=Snapshot.MaxAgeMs;state.Expire();
            Check(state.Info==null&&expiring.Disposed,"Expiry discards native OCR targets and pixels");stage="Complete";return Report(0,"Ready");
        } catch(ScreenException e){return Report(1,e.Code);}
        catch(Exception e){return Report(1,"NativeFailure",e.HResult);}
    }
    private static T Pump<T>(Task<T> task) {
        var frame=new DispatcherFrame();var dispatcher=Dispatcher.CurrentDispatcher;
        _=task.ContinueWith(_=>dispatcher.BeginInvoke(()=>frame.Continue=false),TaskScheduler.Default);Dispatcher.PushFrame(frame);return task.GetAwaiter().GetResult();
    }
}
