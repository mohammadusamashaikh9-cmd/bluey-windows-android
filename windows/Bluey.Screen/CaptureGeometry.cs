namespace Bluey.Screen;
public enum CaptureMapping { VisibleBounds,WindowFrame,PreviewOnly }
// A measured transform, never a guessed scale based on similar aspect ratios.
public sealed record CaptureGeometry(SurfaceKind Kind,Box Selection,Box Primary,Box? WindowFrame,int Width,int Height,double Dpi,CaptureMapping Mapping,Box Physical)
{
    public const long MaxPixels=16*1024*1024;
    public bool Verified=>Mapping!=CaptureMapping.PreviewOnly;
    public static CaptureGeometry Resolve(SurfaceKind kind,Box selected,Box primary,Box? frame,int width,int height,double dpi) {
        static bool Pixels(Box b)=>b.Valid&&new[]{b.X,b.Y,b.Width,b.Height}.All(v=>v==Math.Truncate(v));
        if(kind is not (SurfaceKind.Window or SurfaceKind.PrimaryDisplay)||!Pixels(selected)||!Pixels(primary)||!primary.Contains(selected)||!double.IsFinite(dpi)||dpi is <48 or >768||frame is {} raw&&!Pixels(raw))throw new ScreenException("capture-bounds");
        if(width is <1 or >16384||height is <1 or >16384||width*(long)height>MaxPixels||width/(double)height is <.03125 or >32||width/selected.Width is <.25 or >4||height/selected.Height is <.25 or >4)throw new ScreenException("capture-size");
        var mode=CaptureMapping.PreviewOnly;var physical=selected;
        if(width==selected.Width&&height==selected.Height&&(frame==null||frame.Value.Contains(selected)))mode=CaptureMapping.VisibleBounds;
        else if(kind==SurfaceKind.Window&&frame is {} f&&f.Width==width&&f.Height==height&&f.Contains(selected)&&primary.Contains(f)) {
            var maxPadding=32*dpi/96;
            if(selected.X-f.X<=maxPadding&&selected.Y-f.Y<=maxPadding&&f.Right-selected.Right<=maxPadding&&f.Bottom-selected.Bottom<=maxPadding){mode=CaptureMapping.WindowFrame;physical=f;}
        }
        return new(kind,selected,primary,frame,width,height,dpi,mode,physical);
    }
    public Surface ToSurface(long id) {
        var scale=Math.Min(1,1100.0/Math.Max(Width,Height));
        return new(Kind,id,Physical,Primary,Dpi,Dpi,Math.Max(1,(int)Math.Round(Width*scale)),Math.Max(1,(int)Math.Round(Height*scale)),Mapping,Selection,WindowFrame);
    }
    public bool Matches(Box selected,Box? frame,int width,int height)=>Selection==selected&&WindowFrame==frame&&Width==width&&Height==height;
}
