namespace Bluey.Screen;
public readonly record struct FrameSize(int Width,int Height);
public enum FrameDisposition { Await,Recreate,Accept }
public enum FrameState { Null,Empty,Recreate,Accepted,Rejected }
public sealed record FrameObservation(int Number,bool Null,FrameSize? Content,FrameSize? Buffer,FrameSize Pool,FrameSize Item,long ElapsedMs,FrameState State)
{
    private static string Extent(FrameSize? s)=>s is {} v?$"{v.Width}x{v.Height}":"none";
    public string SafeText=>$"f{Number} null={Null} content={Extent(Content)} buffer={Extent(Buffer)} pool={Extent(Pool)} item={Extent(Item)} {State} {ElapsedMs}ms";
}
public sealed record CaptureFrameTrace(FrameSize Initial,int Recreates,IReadOnlyList<FrameObservation> Observations)
{
    public string SafeText=>$"initial={Initial.Width}x{Initial.Height} frames={Observations.Count} recreates={Recreates} ["+string.Join("; ",Observations.Select(f=>f.SafeText))+"]";
}
// Frame acquisition policy is independent of WinRT. Native pixels are never retained here.
public sealed class CaptureFrames(CaptureGeometry initial)
{
    public const int MaxFrames=6,MaxRecreates=2;
    private readonly List<FrameObservation> observations=new();private int recreates;private bool finished;private long lastElapsed=-1;
    public FrameSize PoolSize{get;private set;}=new(initial.Width,initial.Height);
    public CaptureGeometry? Accepted{get;private set;}
    public CaptureFrameTrace Trace=>new(new(initial.Width,initial.Height),recreates,Array.AsReadOnly(observations.ToArray()));
    public FrameDisposition Observe(FrameSize? content,FrameSize? buffer,FrameSize item,Box visible,Box? windowFrame,long elapsed)
    {
        if(finished||observations.Count>=MaxFrames)throw new ScreenException("capture-unstable");
        var state=FrameState.Rejected;var pool=PoolSize;
        try {
            if(elapsed>=8000)throw new ScreenException("capture-timeout");
            if(elapsed<0||elapsed<lastElapsed)throw new ScreenException("capture-clock");lastElapsed=elapsed;
            if(visible!=initial.Selection||windowFrame!=initial.WindowFrame)throw new ScreenException("capture-changed");
            Validate(item);if(buffer is {} b)Validate(b);
            if(content==null){state=FrameState.Null;return Again();}
            var size=content.Value;
            if(size.Width>=0&&size.Height>=0&&(size.Width==0||size.Height==0)){state=FrameState.Empty;return Again();}
            Validate(size);
            var geometry=CaptureGeometry.Resolve(initial.Kind,visible,initial.Primary,windowFrame,size.Width,size.Height,initial.Dpi);
            if(size!=PoolSize||buffer!=size) {
                if(recreates>=MaxRecreates||observations.Count+1>=MaxFrames)throw new ScreenException("capture-unstable");
                recreates++;PoolSize=size;state=FrameState.Recreate;return FrameDisposition.Recreate;
            }
            Accepted=geometry;finished=true;state=FrameState.Accepted;return FrameDisposition.Accept;
        } catch {finished=true;throw;}
        finally {observations.Add(new(observations.Count+1,content==null,content,buffer,pool,item,Math.Clamp(elapsed,0,60000),state));}
    }
    private FrameDisposition Again() {
        if(observations.Count+1>=MaxFrames)throw new ScreenException("capture-unstable");return FrameDisposition.Await;
    }
    private static void Validate(FrameSize s) {
        if(s.Width is <1 or >16384||s.Height is <1 or >16384||s.Width*(long)s.Height>CaptureGeometry.MaxPixels)throw new ScreenException("capture-size");
    }
}
