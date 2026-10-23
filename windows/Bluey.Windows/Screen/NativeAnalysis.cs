using System.Diagnostics;
using System.Windows.Automation;
using Bluey.Screen;
using Windows.Media.Ocr;
namespace Bluey.Windows.Screen;
// One potentially slow native UIA provider can occupy one background worker, never the render loop.
internal sealed class NativeControls
{
    private int busy;
    internal async Task<(Control[] Controls,bool TimedOut)> Read(Surface surface,CancellationToken stop)
    {
        if(surface.Kind!=SurfaceKind.Window||Interlocked.CompareExchange(ref busy,1,0)!=0)return([],true);
        var task=Task.Run(()=>{try{return Traverse((nint)surface.NativeId,surface.Physical,stop);}finally{Volatile.Write(ref busy,0);}},CancellationToken.None);
        try{var result=await task.WaitAsync(TimeSpan.FromMilliseconds(450),stop).ConfigureAwait(false);return(result.Controls,result.TimedOut);}
        catch(OperationCanceledException){throw;}
        catch {return([],true);}
    }
    private static ControlReadout Traverse(nint handle,Box selected,CancellationToken stop)
    {
        var elapsed=Stopwatch.StartNew();return ControlTree.Read(new Node(AutomationElement.FromHandle(handle)),selected,()=>elapsed.ElapsedMilliseconds,stop);
    }
    private sealed class Node(AutomationElement element):IControlNode {
        public bool Secure=>element.Current.IsPassword;public bool Offscreen=>element.Current.IsOffscreen;
        public Box Bounds{get{var b=element.Current.BoundingRectangle;return new(b.X,b.Y,b.Width,b.Height);}}
        public string? Role=>NativeControls.Role(element.Current.ControlType);
        public string Name=>element.Current.Name??"";
        public IEnumerable<IControlNode> Children{get{var child=TreeWalker.ControlViewWalker.GetFirstChild(element);while(child!=null){yield return new Node(child);child=TreeWalker.ControlViewWalker.GetNextSibling(child);}}}
    }
    private static string? Role(ControlType t)=>t==ControlType.Button?"button":t==ControlType.CheckBox?"checkbox":t==ControlType.RadioButton?"option":t==ControlType.Hyperlink?"link":t==ControlType.TabItem?"tab":t==ControlType.ComboBox?"combo box":t==ControlType.Edit?"text field":t==ControlType.Slider?"slider":t==ControlType.Text?"text":null;
}
internal sealed class NativeAnalysis
{
    private readonly NativeControls controls=new();private int busy;
    internal async Task<Analysis> Analyze(ScreenMedia image,CancellationToken stop)
    {
        if(Interlocked.CompareExchange(ref busy,1,0)!=0)throw new ScreenException("analysis-busy");
        try {
            using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(5));using var linked=CancellationTokenSource.CreateLinkedTokenSource(stop,image.Stop,deadline.Token);var cancel=linked.Token;
            var controlTask=controls.Read(image.Info.Surface,cancel);var lines=new List<TextLine>();var status=OcrStatus.UnavailableIdentity;
            if(NativeCapture.HasPackageIdentity) {
                using var bitmap=await NativeCapture.Decode(image.Jpeg,cancel).ConfigureAwait(false);
                var engine=OcrEngine.TryCreateFromUserProfileLanguages();status=engine==null?OcrStatus.UnavailableLanguage:OcrStatus.Ready;
                if(engine!=null) {
                    if(bitmap.PixelWidth>OcrEngine.MaxImageDimension||bitmap.PixelHeight>OcrEngine.MaxImageDimension)throw new ScreenException("ocr-dimensions");
                    var result=await engine.RecognizeAsync(bitmap).AsTask(cancel).ConfigureAwait(false);int words=0;
                    foreach(var line in result.Lines.Take(400)) {
                        cancel.ThrowIfCancellationRequested();
                        var all=line.Words.Take(600).Select(w=>new TextWord(Targets.Clean(w.Text,96),new(w.BoundingRect.X,w.BoundingRect.Y,w.BoundingRect.Width,w.BoundingRect.Height))).Where(w=>image.Info.Surface.Image.Contains(w.Bounds)).ToArray();
                        if(all.Length==0)continue;var actual=all.Take(600-words).ToArray();words+=actual.Length;
                        var bounds=all.Select(w=>w.Bounds).Aggregate(Box.Union);lines.Add(new(Targets.Clean(line.Text),bounds,actual));
                    }
                }
            }
            var c=await controlTask.ConfigureAwait(false);cancel.ThrowIfCancellationRequested();
            return new(lines.ToArray(),c.Controls,true,status,c.TimedOut);
        } catch(OperationCanceledException){throw;}
        catch(ScreenException){throw;}
        catch {throw new ScreenException("analysis-unavailable");}
        finally {Volatile.Write(ref busy,0);}
    }
}
