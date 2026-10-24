namespace Bluey.Screen;
public sealed class ScreenException(string code):IOException("Screen operation stopped: "+code) {public string Code{get;}=code;public override string ToString()=>"Screen ["+Code+"]";}
public readonly record struct Spot(double X,double Y);
public readonly record struct Box(double X,double Y,double Width,double Height)
{
    public double Right=>X+Width;public double Bottom=>Y+Height;public Spot Center=>new(X+Width/2,Y+Height/2);
    public bool Valid=>double.IsFinite(X+Y+Width+Height+Right+Bottom)&&Width>0&&Height>0;
    public bool Contains(Box b)=>Valid&&b.Valid&&b.X>=X&&b.Y>=Y&&b.Right<=Right&&b.Bottom<=Bottom;
    public bool Contains(Spot p)=>double.IsFinite(p.X+p.Y)&&p.X>=X&&p.Y>=Y&&p.X<Right&&p.Y<Bottom;
    public static Box Union(Box a,Box b)=>new(Math.Min(a.X,b.X),Math.Min(a.Y,b.Y),Math.Max(a.Right,b.Right)-Math.Min(a.X,b.X),Math.Max(a.Bottom,b.Bottom)-Math.Min(a.Y,b.Y));
}
public enum SurfaceKind { PrimaryDisplay,Window,Synthetic }
// Physical desktop pixels, encoded image pixels, then primary-overlay-local DIPs. Never interchangeable.
public sealed record Surface
{
    public CaptureMapping Mapping{get;}public bool MappingVerified=>Mapping!=CaptureMapping.PreviewOnly;
    public Box SelectionBounds{get;}public Box? WindowFrameBounds{get;}
    public SurfaceKind Kind{get;}public long NativeId{get;}public Box Physical{get;}public Box Primary{get;}
    public double DpiX{get;}public double DpiY{get;}public int ImageWidth{get;}public int ImageHeight{get;}
    public Surface(SurfaceKind kind,long nativeId,Box physical,Box primary,double dpiX,double dpiY,int imageWidth,int imageHeight,CaptureMapping mapping=CaptureMapping.VisibleBounds,Box? selectionBounds=null,Box? windowFrameBounds=null) {
        var selected=selectionBounds??physical;var verified=mapping!=CaptureMapping.PreviewOnly;
        if(!Enum.IsDefined(kind)||!Enum.IsDefined(mapping)||!primary.Contains(selected)||!physical.Valid||verified&&!primary.Contains(physical)||!verified&&physical!=selected||windowFrameBounds is {} frame&&!frame.Valid||!double.IsFinite(dpiX+dpiY)||dpiX is <48 or >768||dpiY is <48 or >768||imageWidth is <1 or >1100||imageHeight is <1 or >1100||verified&&Math.Abs(physical.Width/physical.Height-(double)imageWidth/imageHeight)>.02||!verified&&(double)imageWidth/imageHeight is <.03125 or >32)throw new ScreenException("geometry");
        Mapping=mapping;SelectionBounds=selected;WindowFrameBounds=windowFrameBounds;Kind=kind;NativeId=nativeId;Physical=physical;Primary=primary;DpiX=dpiX;DpiY=dpiY;ImageWidth=imageWidth;ImageHeight=imageHeight;
    }
    public void RequireMapping(){if(!MappingVerified)throw new ScreenException("mapping-unverified");}
    public Box Image=>new(0,0,ImageWidth,ImageHeight);
    public Box DipCapture=>PhysicalToDip(Physical);
    public Spot PhysicalToDip(Spot p) {RequireMapping();if(!Primary.Contains(p))throw new ScreenException("outside-primary");return new((p.X-Primary.X)*96/DpiX,(p.Y-Primary.Y)*96/DpiY);}
    public Box PhysicalToDip(Box b) {RequireMapping();if(!Primary.Contains(b))throw new ScreenException("outside-primary");return new((b.X-Primary.X)*96/DpiX,(b.Y-Primary.Y)*96/DpiY,b.Width*96/DpiX,b.Height*96/DpiY);}
    public Box ImageToPhysical(Box b) {RequireMapping();if(!Image.Contains(b))throw new ScreenException("image-bounds");return new(Physical.X+b.X/ImageWidth*Physical.Width,Physical.Y+b.Y/ImageHeight*Physical.Height,b.Width/ImageWidth*Physical.Width,b.Height/ImageHeight*Physical.Height);}
    public Box PhysicalToImage(Box b) {RequireMapping();if(!Physical.Contains(b))throw new ScreenException("capture-bounds");return new((b.X-Physical.X)/Physical.Width*ImageWidth,(b.Y-Physical.Y)/Physical.Height*ImageHeight,b.Width/Physical.Width*ImageWidth,b.Height/Physical.Height*ImageHeight);}
    public Box ImageToDip(Box b)=>PhysicalToDip(ImageToPhysical(b));
    public Box Normalized(Box b) {RequireMapping();if(!Image.Contains(b))throw new ScreenException("image-bounds");return new(b.X/ImageWidth*1000,b.Y/ImageHeight*1000,b.Width/ImageWidth*1000,b.Height/ImageHeight*1000);}
    // Inclusive 1000 means the last physical pixel, never a point beyond the display edge.
    public Spot GridToDip(double x,double y) {RequireMapping();if(!double.IsFinite(x+y)||x is <0 or >1000||y is <0 or >1000)throw new ScreenException("grid-range");return PhysicalToDip(new Spot(Physical.X+x/1000*(Physical.Width-1),Physical.Y+y/1000*(Physical.Height-1)));}
}
