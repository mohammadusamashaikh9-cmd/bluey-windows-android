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
}
