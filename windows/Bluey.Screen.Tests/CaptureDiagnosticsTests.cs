using System.Runtime.InteropServices;
using Xunit;
namespace Bluey.Screen.Tests;
public sealed class CaptureDiagnosticsTests
{
    [Fact] public void NativeFailurePreservesStageAndHresultWithoutRetainingPrivateExceptionDetails() {
        var diagnostics=new CaptureDiagnostics();
        var report=diagnostics.Record(SurfaceKind.Window,CaptureStage.DxgiDevice,CaptureOutcome.NativeFailure,125,new COMException("PRIVATE WINDOW TITLE; PRIVATE API KEY",unchecked((int)0x80004002)));
        var error=new CaptureFailure(report);
        Assert.Equal(unchecked((int)0x80004002),report.NativeHresult);Assert.Contains("DxgiDevice",diagnostics.Text);Assert.Contains("0x80004002",diagnostics.Text);
        Assert.DoesNotContain("PRIVATE",diagnostics.Text+error+error.Message);Assert.Null(error.InnerException);
        Assert.NotEqual(report.SafeText,diagnostics.Record(SurfaceKind.PrimaryDisplay,CaptureStage.SurfaceCopy,CaptureOutcome.NativeFailure,126).SafeText);
    }
    [Fact] public void DiagnosticsKeepOnlyEightCompletedAttemptsAndBoundElapsedMetadata() {
        var diagnostics=new CaptureDiagnostics();
        for(int n=0;n<20;n++)diagnostics.Record(SurfaceKind.PrimaryDisplay,CaptureStage.FirstFrame,CaptureOutcome.TimedOut,n==19?long.MaxValue:-1);
        Assert.Equal(8,diagnostics.Reports.Length);Assert.Equal(13,diagnostics.Reports[0].Attempt);Assert.Equal(20,diagnostics.Reports[^1].Attempt);
        Assert.Equal(0,diagnostics.Reports[0].ElapsedMs);Assert.Equal(60000,diagnostics.Reports[^1].ElapsedMs);Assert.True(diagnostics.Text.Length<2400);
    }
    [Fact] public void CancellationTimeoutAndChangedGeometryGiveDistinctSafeGuidance() {
        var d=new CaptureDiagnostics();
        var canceled=d.Record(SurfaceKind.Window,CaptureStage.FirstFrame,CaptureOutcome.Canceled,10);
        var timeout=d.Record(SurfaceKind.Window,CaptureStage.FirstFrame,CaptureOutcome.TimedOut,8000);
        var changed=d.Record(SurfaceKind.Window,CaptureStage.GeometryValidation,CaptureOutcome.Changed,100);
        Assert.Null(canceled.NativeHresult);Assert.Contains("canceled",canceled.UserMessage,StringComparison.OrdinalIgnoreCase);
        Assert.Contains("timed out",timeout.UserMessage,StringComparison.OrdinalIgnoreCase);Assert.Contains("select",changed.UserMessage,StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(timeout.UserMessage,canceled.UserMessage);
    }
    [Fact] public void DimensionSummaryShowsMeasuredExtentsAndDeltasWithNoPositionOrPrivateContent() {
        var d=new CaptureDiagnostics();var extent=new CaptureDimensions(1000,600,1016,608,1016,608,144,CaptureMapping.WindowFrame);
        for(int i=0;i<20;i++)d.Record(SurfaceKind.Window,CaptureStage.ItemSize,CaptureOutcome.Succeeded,10,dimensions:extent);
        Assert.Contains("DWM=1000x600",d.Text);Assert.Contains("WGC=1016x608",d.Text);Assert.Contains("delta=16x8",d.Text);
        Assert.Contains("dpiScale=1.5",d.Text);Assert.Contains("map=WindowFrame",d.Text);Assert.Equal(8,d.Reports.Length);Assert.True(d.Text.Length<4096);
    }
    [Fact] public void SafeFrameDiagnosticsDistinguishNullFromSizeMismatchAndRemainBounded() {
        var plan=CaptureGeometry.Resolve(SurfaceKind.Window,new(100,100,578,557),new(0,0,1920,1080),new(92,100,594,565),594,565,120);
        var f=new CaptureFrames(plan);var d=new CaptureDiagnostics();
        for(int n=1;n<CaptureFrames.MaxFrames;n++)f.Observe(null,null,new(594,565),plan.Selection,plan.WindowFrame,n);
        Assert.Throws<ScreenException>(()=>f.Observe(null,null,new(594,565),plan.Selection,plan.WindowFrame,6));
        for(int n=0;n<20;n++)d.Record(SurfaceKind.Window,CaptureStage.FirstFrame,CaptureOutcome.FramesUnstable,7,frames:f.Trace);
        Assert.Contains("frames=6",d.Text);Assert.Contains("null=True content=none buffer=none",d.Text);
        Assert.Equal(8,d.Reports.Length);Assert.True(d.Text.Length<12000);Assert.Contains("stable complete frame",d.Reports[^1].UserMessage);
    }
}
