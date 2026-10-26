using Xunit;
namespace Bluey.Screen.Tests;
public sealed class CaptureFramesTests
{
    private static readonly Box Visible=new(100,100,578,557),Raw=new(92,100,594,565),Primary=new(0,0,1920,1080);
    private static CaptureFrames Begin()=>new(CaptureGeometry.Resolve(SurfaceKind.Window,Visible,Primary,Raw,594,565,120));
    private static FrameDisposition See(CaptureFrames f,FrameSize? content,FrameSize? buffer,Box? visible=null,long elapsed=100)=>f.Observe(content,buffer,new(594,565),visible??Visible,Raw,elapsed);
    [Fact] public void NullFirstFrameDoesNotClaimSnapshotAndNextValidFrameCanComplete() {
        var f=Begin();Assert.Equal(FrameDisposition.Await,See(f,null,null));Assert.Null(f.Accepted);
        Assert.Equal(FrameDisposition.Accept,See(f,new(594,565),new(594,565)));Assert.Equal(CaptureMapping.WindowFrame,f.Accepted!.Mapping);
    }
    [Fact] public void FirstContentMismatchRecreatesThenAcceptsStableCompleteFrame() {
        var f=Begin();Assert.Equal(FrameDisposition.Recreate,See(f,new(578,557),new(594,565)));
        Assert.Equal(new FrameSize(578,557),f.PoolSize);Assert.Null(f.Accepted);
        Assert.Equal(FrameDisposition.Accept,See(f,new(578,557),new(578,557)));
        Assert.Equal(CaptureMapping.VisibleBounds,f.Accepted!.Mapping);
    }
    [Fact] public void TransientContentMismatchCanReturnToInitialSizeWithinBound() {
        var f=Begin();Assert.Equal(FrameDisposition.Recreate,See(f,new(578,557),new(594,565)));
        Assert.Equal(FrameDisposition.Recreate,See(f,new(594,565),new(578,557)));
        Assert.Equal(FrameDisposition.Accept,See(f,new(594,565),new(594,565)));
    }
    [Fact] public void UndefinedBufferPixelsAreNeverAcceptedAndPoolRecreationIsBounded() {
        var f=Begin();for(int n=0;n<CaptureFrames.MaxRecreates;n++)Assert.Equal(FrameDisposition.Recreate,See(f,new(594,565),new(600,600)));
        Assert.Equal("capture-unstable",Assert.Throws<ScreenException>(()=>See(f,new(594,565),new(600,600))).Code);Assert.Null(f.Accepted);
    }
    [Fact] public void RepeatedUnavailableFramesFailWithinFiniteAcquisitionCount() {
        var f=Begin();for(int n=1;n<CaptureFrames.MaxFrames;n++)Assert.Equal(FrameDisposition.Await,See(f,null,null));
        Assert.Equal("capture-unstable",Assert.Throws<ScreenException>(()=>See(f,null,null)).Code);Assert.Null(f.Accepted);
    }
    [Fact] public void ActualMovementFailsEvenWhenContentAndBufferHaveMatchingSizes() {
        var f=Begin();Assert.Equal("capture-changed",Assert.Throws<ScreenException>(()=>See(f,new(594,565),new(594,565),Visible with {X=101})).Code);Assert.Null(f.Accepted);
    }
    [Fact] public void BoundedUnknownFinalGeometryRemainsPreviewOnlyAndCannotAuthorizeSharing() {
        var f=Begin();Assert.Equal(FrameDisposition.Recreate,See(f,new(600,570),new(594,565)));
        Assert.Equal(FrameDisposition.Accept,See(f,new(600,570),new(600,570)));var s=f.Accepted!.ToSurface(1);Assert.False(s.MappingVerified);
        using var state=new ScreenSession(()=>0);using var shot=new Snapshot(s,[255,216,0,255,217],0,DateTimeOffset.UnixEpoch);
        Assert.True(state.CompleteCapture(state.BeginCapture(true),shot));using var preview=state.CopyCurrent();Assert.NotEmpty(preview.Jpeg.ToArray());state.MarkPreviewReady(shot.Id);
        Assert.Throws<ScreenException>(()=>state.PointAtSpot(shot.Id,500,500));Assert.Throws<ScreenException>(()=>state.Approve(shot.Id,"tls","c","a",true));
    }
    [Fact] public void VerifiedFinalFrameProducesCorrectCenterPreviewAndCloudRemainsUnapproved() {
        var f=Begin();Assert.Equal(FrameDisposition.Accept,See(f,new(594,565),new(594,565)));var s=f.Accepted!.ToSurface(1);
        using var state=new ScreenSession(()=>0);using var shot=new Snapshot(s,[255,216,0,255,217],0,DateTimeOffset.UnixEpoch);
        state.CompleteCapture(state.BeginCapture(true),shot);using var preview=state.CopyCurrent();state.MarkPreviewReady(shot.Id);
        var p=state.PointAtSpot(shot.Id,500,500);Assert.Equal((92+593*.5)/1.25,p.Position.X,8);Assert.Equal((100+564*.5)/1.25,p.Position.Y,8);
        Assert.Null(state.TakeForQuestion("tls","c","a","turn"));
    }
    [Theory][InlineData(0,0)][InlineData(9000,9000)] public void InvalidOrOversizedFramesCannotBeCopied(int w,int h) {
        var f=Begin();if(w==0)Assert.Equal(FrameDisposition.Await,See(f,new(w,h),new(594,565)));
        else Assert.Throws<ScreenException>(()=>See(f,new(w,h),new(w,h)));Assert.Null(f.Accepted);
    }
    [Fact] public void CaptureDeadlineRemainsEightSecondsRegardlessOfFrameCount() {
        var f=Begin();Assert.Equal("capture-timeout",Assert.Throws<ScreenException>(()=>See(f,new(594,565),new(594,565),elapsed:8000)).Code);Assert.Null(f.Accepted);
    }
    [Fact] public void ClippedSurfaceIsRecreatedBeforeCopyingLargerContent() {
        var f=Begin();Assert.Equal(FrameDisposition.Recreate,See(f,new(594,565),new(578,557)));Assert.Null(f.Accepted);
        Assert.Equal(FrameDisposition.Accept,See(f,new(594,565),new(594,565)));
    }
    [Fact] public void ActualWin32ResizeFailsEvenIfVisibleBoundsHaveNotUpdatedYet() {
        var f=Begin();Assert.Equal("capture-changed",Assert.Throws<ScreenException>(()=>f.Observe(new(594,565),new(594,565),new(594,565),Visible,Raw with {Width=595},100)).Code);
        Assert.Null(f.Accepted);
    }
    [Fact] public void PersistentAlternatingSizesFailAfterTwoRecreates() {
        var f=Begin();See(f,new(578,557),new(594,565));See(f,new(594,565),new(578,557));
        Assert.Equal("capture-unstable",Assert.Throws<ScreenException>(()=>See(f,new(578,557),new(594,565))).Code);Assert.Null(f.Accepted);
    }
    [Fact] public void DiagnosticHistoryIsImmutableAndShowsNullAndResizeBeforeAcceptedFrame() {
        var f=Begin();See(f,null,null);var first=f.Trace;See(f,new(578,557),new(594,565));See(f,new(578,557),new(578,557));
        Assert.Single(first.Observations);Assert.True(first.Observations[0].Null);Assert.Equal(3,f.Trace.Observations.Count);Assert.Equal(1,f.Trace.Recreates);
        Assert.Contains("initial=594x565 frames=3",f.Trace.SafeText);Assert.Contains("content=578x557 buffer=594x565",f.Trace.SafeText);
        Assert.Equal(FrameState.Accepted,f.Trace.Observations[^1].State);Assert.Throws<ScreenException>(()=>See(f,new(578,557),new(578,557)));
    }
}
