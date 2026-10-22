using Xunit;
namespace Bluey.Screen.Tests;
public sealed class ScreenTests
{
    private static Surface Surface(double dpi=144)=>new(SurfaceKind.Window,42,new(120,90,900,600),new(-100,0,1920,1080),dpi,dpi,900,600);
    private static Snapshot Shot(long at=0,Surface? surface=null)=>new(surface??Surface(),[255,216,0,255,217],at,DateTimeOffset.UnixEpoch);
    private static readonly TextLine Line=new("ALPHA BETA",new(10,20,200,30),[new("ALPHA",new(10,20,80,30)),new("BETA",new(110,20,100,30))]);
    [Fact] public void DefaultDeniedAndCaptureNeedsExplicitAction() {
        var s=new ScreenSession(()=>0);Assert.Null(s.Info);Assert.Throws<ScreenException>(()=>s.BeginCapture(false));
        Assert.Throws<ScreenException>(()=>s.PointAtSpot("missing",20,20));Assert.False(s.CloudApproved);
    }
    [Fact] public void OldCaptureCannotReplaceNewOrSurviveStop() {
        var s=new ScreenSession(()=>0);var old=s.BeginCapture(true);var next=s.BeginCapture(true);var a=Shot();var b=Shot();
        Assert.False(s.CompleteCapture(old,a));Assert.True(a.Disposed);Assert.True(s.CompleteCapture(next,b));s.Invalidate();Assert.True(b.Disposed);Assert.Null(s.Info);Assert.True(next.Stop.IsCancellationRequested);
    }
    [Theory][InlineData(96)][InlineData(144)][InlineData(192)] public void PhysicalOriginAndDpiAreAppliedBeforePointing(double dpi) {
        var g=Surface(dpi);var p=g.GridToDip(0,0);Assert.Equal(220*96/dpi,p.X,7);Assert.Equal(90*96/dpi,p.Y,7);
        var q=g.GridToDip(1000,1000);Assert.Equal(1119*96/dpi,q.X,7);Assert.Equal(689*96/dpi,q.Y,7);
        var r=g.ImageToDip(new(10,20,200,30));Assert.Equal(230*96/dpi,r.X,7);Assert.Equal(110*96/dpi,r.Y,7);
    }
    [Theory][InlineData(double.NaN,0)][InlineData(double.PositiveInfinity,0)][InlineData(-1,10)][InlineData(1001,10)][InlineData(10,1001)]
    public void BadGridCoordinatesFailClosed(double x,double y)=>Assert.Throws<ScreenException>(()=>Surface().GridToDip(x,y));
    [Fact] public void InvalidSurfaceImagesAndOffPrimaryWindowsFailClosed() {
        Assert.Throws<ScreenException>(()=>new Surface(SurfaceKind.Window,1,new(1900,0,100,100),new(0,0,1920,1080),96,96,100,100));
        Assert.Throws<ScreenException>(()=>new Surface(SurfaceKind.Window,1,new(0,0,100,100),new(0,0,1920,1080),double.NaN,96,100,100));
        Assert.Throws<ScreenException>(()=>new Surface(SurfaceKind.Window,1,new(0,0,100,100),new(0,0,1920,1080),96,96,5000,5000));
        Assert.Throws<ScreenException>(()=>new Snapshot(Surface(),[0,1,2],0,DateTimeOffset.UnixEpoch));
        Assert.Throws<ScreenException>(()=>new Snapshot(Surface(),new byte[Snapshot.MaxJpegBytes+1],0,DateTimeOffset.UnixEpoch));
    }
    [Fact] public void ExtractionHasReadingOrderRealWordBoundsAndNoSecureOrHiddenLabels() {
        var g=Surface();var lines=new[]{Line with{Text="lower",Bounds=new(10,100,200,30)},Line};
        var controls=new[]{new Control("button","Save",new(200,200,80,30),true,false),new Control("edit","private password",new(200,300,80,30),true,true),new Control("button","hidden",new(200,400,80,30),false,false)};
        var targets=Targets.Extract(g,new(lines,controls,true,OcrStatus.Ready));
        Assert.Equal("ALPHA BETA",targets.First(t=>t.Id=="L1").Text);Assert.Equal("ALPHA",targets.First(t=>t.Id=="W1").Text);
        Assert.Equal(new Box(10,20,80,30),targets.First(t=>t.Id=="W1").ImageBounds);
        Assert.Single(targets,t=>t.Id.StartsWith('C'));Assert.DoesNotContain(targets,t=>t.Text.Contains("private")||t.Text=="hidden");
        Assert.DoesNotContain("ALPHA",targets[0].ToString());
        Assert.DoesNotContain(Targets.Extract(g,new([Line],[],false,OcrStatus.Ready)),t=>t.Id.StartsWith('W'));
    }
    [Fact] public void LimitsAndTextAreHardCapsAndOutsideGeometryIsDiscarded() {
        var lines=Enumerable.Repeat(Line with{Text=new('x',2000)},600).ToArray();
        var controls=Enumerable.Range(0,300).Select(i=>new Control("button","test",new(200,200,80,30),true,false)).ToArray();
        var targets=Targets.Extract(Surface(),new(lines,controls,true,OcrStatus.Ready));
        Assert.Equal(400,targets.Count(t=>t.Id.StartsWith('L')));Assert.Equal(600,targets.Count(t=>t.Id.StartsWith('W')));Assert.Equal(140,targets.Count(t=>t.Id.StartsWith('C')));Assert.All(targets,t=>Assert.InRange(t.Text.Length,1,256));
        Assert.Empty(Targets.Extract(Surface(),new([Line with{Bounds=new(-10,0,100,10)}],[],false,OcrStatus.Ready)));
    }
    [Fact] public void ApprovalIsSeparateReviewedExactLeaseConsentAuthorizationAndOneShot() {
        long now=0;var s=new ScreenSession(()=>now);var shot=Shot();s.CompleteCapture(s.BeginCapture(true),shot);var id=shot.Id;
        Assert.Throws<ScreenException>(()=>s.Approve(id,"tls","consent","auth",true));s.MarkPreviewReady(id);
        Assert.Throws<ScreenException>(()=>s.Approve(id,"tls","consent","auth",false));s.Approve(id,"tls","consent","auth",true);
        Assert.Null(s.TakeForQuestion("other","consent","auth","turn"));Assert.Null(s.TakeForQuestion("tls","other","auth","turn"));
        using var approved=s.TakeForQuestion("tls","consent","auth","turn");Assert.NotNull(approved);Assert.False(s.CloudApproved);Assert.Null(s.TakeForQuestion("tls","consent","auth","turn"));
        now=60000;Assert.False(s.IsValid(approved!));s.Expire();Assert.Null(s.Info);Assert.True(shot.Disposed);
    }
    [Fact] public void CompleteAnswerResolvesOnlySameSnapshotAndWipesMedia() {
        long now=0;var s=new ScreenSession(()=>now);var shot=Shot();s.CompleteCapture(s.BeginCapture(true),shot);s.SetAnalysis(shot.Id,new([Line],[],true,OcrStatus.Ready));s.MarkPreviewReady(shot.Id);s.Approve(shot.Id,"tls","c","a",true);
        using var approval=s.TakeForQuestion("tls","c","a","t")!;var borrowed=approval.Jpeg;
        s.CompleteQuestion(approval,"W1");Assert.True(shot.Disposed);Assert.Null(s.Info);Assert.All(borrowed.ToArray(),b=>Assert.Equal(0,b));
        Assert.Null(s.TakePoint("other","t"));var point=s.TakePoint("tls","t");Assert.NotNull(point);Assert.Null(s.TakePoint("tls","t"));Assert.True(s.IsValid(point!));s.Invalidate();Assert.False(s.IsValid(point!));
    }
    [Theory][InlineData("W999")][InlineData("L1;click()")][InlineData("http://example.com")][InlineData(null)]
    public void InvalidOptionalModelTargetDoesNotProduceAPoint(string? target) {
        var s=new ScreenSession(()=>0);var shot=Shot();s.CompleteCapture(s.BeginCapture(true),shot);s.MarkPreviewReady(shot.Id);s.Approve(shot.Id,"tls","c","a",true);
        using var approval=s.TakeForQuestion("tls","c","a","t")!;s.CompleteQuestion(approval,target);Assert.Null(s.TakePoint("tls","t"));Assert.Null(s.Info);
    }
    [Fact] public void ExpiryReplacementAndWithdrawalInvalidateEveryTargetAndPendingMedia() {
        long now=0;var s=new ScreenSession(()=>now);var shot=Shot();s.CompleteCapture(s.BeginCapture(true),shot);s.SetAnalysis(shot.Id,new([Line],[],true,OcrStatus.Ready));
        Assert.NotNull(s.PointAtTarget(shot.Id,"L1"));now=60000;Assert.Throws<ScreenException>(()=>s.PointAtTarget(shot.Id,"L1"));s.Expire();Assert.True(shot.Disposed);
        shot=Shot(now);s.CompleteCapture(s.BeginCapture(true),shot);s.MarkPreviewReady(shot.Id);s.Approve(shot.Id,"tls","c","a",true);using var approval=s.TakeForQuestion("tls","c","a","t")!;
        s.Invalidate();Assert.False(s.IsValid(approval));Assert.True(approval.Stop.IsCancellationRequested);Assert.Throws<ScreenException>(()=>s.PointAtTarget(shot.Id,"L1"));
    }
    [Fact] public void ChoreographyHoldsThenReturnsHomeAndStopIsImmediate() {
        var q=new Pointing();var p=new VisualPoint(new(100,200),new(90,180,20,20));q.Enqueue(p,0);
        Assert.Equal(p,q.Tick(0,false).Point);Assert.False(q.Tick(3199,false).Home);Assert.False(q.Tick(5000,false).Home);Assert.True(q.Tick(5001,false).Home);
        q.Enqueue(p,6000);q.Tick(6000,true);Assert.False(q.Tick(13000,true).Home);q.Tick(14000,false);Assert.False(q.Tick(19000,false).Home);Assert.True(q.Tick(19001,false).Home);q.Enqueue(p,20000);Assert.True(q.Stop().Home);Assert.Null(q.Tick(30000,false).Point);
    }
}
