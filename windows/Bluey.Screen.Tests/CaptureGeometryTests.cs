using Xunit;
namespace Bluey.Screen.Tests;
public sealed class CaptureGeometryTests
{
    private static readonly Box Primary=new(0,0,3000,1800),Visible=new(100,100,1000,600),Frame=new(92,100,1016,608);
    [Fact] public void ExactVisibleSizeRetainsVisibleOriginAndVerifiedMapping() {
        var plan=CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,Frame,1000,600,96);
        Assert.Equal(CaptureMapping.VisibleBounds,plan.Mapping);Assert.Equal(Visible,plan.Physical);Assert.True(plan.Verified);
    }
    [Fact] public void MeasuredBorderDifferenceUsesWindowRectangleWithoutStretchingVisibleBounds() {
        var plan=CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,Frame,1016,608,96);
        Assert.True(plan.Verified);Assert.Equal(CaptureMapping.WindowFrame,plan.Mapping);Assert.Equal(Frame,plan.Physical);
        var surface=plan.ToSurface(1);Assert.Equal(Visible,surface.SelectionBounds);
        Assert.Equal(new Box(100,110,100,50),surface.ImageToPhysical(new(8,10,100,50)));
        Assert.True(plan.Matches(Visible,Frame,1016,608));Assert.False(plan.Matches(Visible with {X=101},Frame,1016,608));
        Assert.False(plan.Matches(Visible,Frame with {Width=1018},1016,608));Assert.False(plan.Matches(Visible,Frame,1018,608));
    }
    [Theory][InlineData(96)][InlineData(144)][InlineData(192)] public void PhysicalFrameMapsToPrimaryLocalDipsAtEachDpi(double dpi) {
        var plan=CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,Frame,1016,608,dpi);var surface=plan.ToSurface(1);
        var b=surface.ImageToDip(new(8,10,100,50));Assert.Equal(100*96/dpi,b.X,8);Assert.Equal(110*96/dpi,b.Y,8);
        Assert.Equal(100*96/dpi,b.Width,8);var p=surface.GridToDip(0,0);Assert.Equal(92*96/dpi,p.X,8);Assert.Equal(100*96/dpi,p.Y,8);
        Assert.Equal(new Box(0,0,1000,1000),surface.Normalized(surface.Image));
    }
    [Fact] public void UnknownScaleOrAspectCanPreviewButNeverInventCoordinatesOrConsent() {
        foreach(var size in new[]{(1500,900),(1200,650)}) {
            var plan=CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,Frame,size.Item1,size.Item2,144);Assert.False(plan.Verified);
            var s=plan.ToSurface(1);Assert.False(s.MappingVerified);Assert.Throws<ScreenException>(()=>s.GridToDip(500,500));
            Assert.Throws<ScreenException>(()=>s.ImageToPhysical(new(0,0,20,20)));Assert.Throws<ScreenException>(()=>s.PhysicalToImage(Visible));
            using var state=new ScreenSession(()=>0);var shot=new Snapshot(s,[255,216,0,255,217],0,DateTimeOffset.UnixEpoch);state.CompleteCapture(state.BeginCapture(true),shot);
            using var preview=state.CopyCurrent();Assert.NotEmpty(preview.Jpeg.ToArray());state.MarkPreviewReady(shot.Id);
            Assert.Throws<ScreenException>(()=>state.SetAnalysis(shot.Id,new([],[],false,OcrStatus.Ready)));
            Assert.Throws<ScreenException>(()=>state.PointAtSpot(shot.Id,500,500));Assert.Throws<ScreenException>(()=>state.Approve(shot.Id,"tls","c","a",true));
            state.Invalidate();Assert.Null(state.Info);Assert.True(shot.Disposed);
        }
    }
    [Fact] public void PaddingOutsidePrimaryOrNotContainingDwmNeverBecomesVerified() {
        var edge=new Box(0,0,1000,600);var outside=new Box(-8,0,1016,608);
        Assert.False(CaptureGeometry.Resolve(SurfaceKind.Window,edge,Primary,outside,1016,608,96).Verified);
        Assert.False(CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,new(101,100,1016,608),1016,608,96).Verified);
    }
    [Theory][InlineData(0,600)][InlineData(-1,600)][InlineData(9000,9000)][InlineData(100,600)][InlineData(100000,1)]
    public void InvalidOversizedOrInconsistentFramesRejectBeforeAllocation(int w,int h) {
        Assert.Throws<ScreenException>(()=>CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,Frame,w,h,96));
    }
    [Fact] public void VerifiedFrameHasExactNormalizedAndDownscaledPhysicalTransform() {
        var visible=new Box(300,150,1500,900);var raw=new Box(288,150,1524,912);
        var p=CaptureGeometry.Resolve(SurfaceKind.Window,visible,Primary,raw,1524,912,144);var s=p.ToSurface(1);
        Assert.Equal(1100,s.ImageWidth);Assert.Equal(658,s.ImageHeight);Assert.True(s.MappingVerified);
        var center=s.ImageToPhysical(new(0,0,s.ImageWidth,s.ImageHeight)).Center;
        Assert.Equal(1050,center.X,8);Assert.Equal(606,center.Y,8);
    }
    [Theory][InlineData(true)][InlineData(false)] public void MovedOrResizedMeasuredBoundsInvalidatePreviouslyApprovedTargets(bool moved) {
        var plan=CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,Frame,1016,608,96);var currentVisible=Visible;Box? currentFrame=Frame;
        using var state=new ScreenSession(()=>0,s=>s.SelectionBounds==currentVisible&&s.WindowFrameBounds==currentFrame);
        var shot=new Snapshot(plan.ToSurface(1),[255,216,0,255,217],0,DateTimeOffset.UnixEpoch);state.CompleteCapture(state.BeginCapture(true),shot);
        state.SetAnalysis(shot.Id,new([new("PUBLIC",new(8,10,100,20),[])],[],false,OcrStatus.Ready));state.MarkPreviewReady(shot.Id);state.Approve(shot.Id,"tls","c","a",true);
        using var approved=state.TakeForQuestion("tls","c","a","t")!;Assert.NotNull(state.PointAtTarget(shot.Id,"L1"));
        if(moved)currentVisible=Visible with {X=101};else currentFrame=Frame with {Width=1020};
        Assert.False(state.IsValid(approved));Assert.Throws<ScreenException>(()=>state.PointAtTarget(shot.Id,"L1"));Assert.Throws<ScreenException>(()=>state.PointAtSpot(shot.Id,500,500));
        state.Invalidate();Assert.True(approved.Stop.IsCancellationRequested);Assert.All(approved.Jpeg.ToArray(),b=>Assert.Equal(0,b));
    }
    [Fact] public void SurfaceCannotClaimVerifiedFrameMappingWithoutMatchingMeasuredBounds() {
        Assert.Throws<ScreenException>(()=>new Surface(SurfaceKind.Window,1,Frame,Primary,96,96,1016,608,CaptureMapping.WindowFrame,Visible,null));
        Assert.Throws<ScreenException>(()=>new Surface(SurfaceKind.Window,1,Frame,Primary,96,96,1016,608,CaptureMapping.VisibleBounds,Visible,Frame));
        Assert.Throws<ScreenException>(()=>new Surface(SurfaceKind.Window,1,Frame,Primary,96,96,1016,608,CaptureMapping.WindowFrame,new(2000,100,100,100),Frame));
    }
}
