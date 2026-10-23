namespace Bluey.Screen;
public enum CaptureStage { Selection,Support,ActivationFactory,CreateWindowItem,CreateMonitorItem,ItemProjection,ItemSize,D3DDevice,DxgiDevice,DeviceProjection,FramePool,CaptureSession,StartSession,FirstFrame,SurfaceCopy,JpegEncode,JpegDecode,GeometryValidation,BlankValidation,Complete }
public enum CaptureOutcome { Succeeded,Canceled,TimedOut,Unsupported,InvalidSelection,Changed,BlankOrProtected,NativeFailure }
public sealed record CaptureReport(long Attempt,SurfaceKind Kind,CaptureStage Stage,CaptureOutcome Outcome,long ElapsedMs,int? NativeHresult)
{
    public string SafeText=>$"capture#{Attempt} {Kind} {Stage} {Outcome} HR={(NativeHresult is {} hr?"0x"+hr.ToString("X8"):"none")} {ElapsedMs}ms";
    public string UserMessage=>Outcome switch {
        CaptureOutcome.Succeeded=>"Windows returned a bounded local image. Review the current preview before sharing.",
        CaptureOutcome.Canceled=>"Capture canceled or its outer deadline was reached. No snapshot retained.",
        CaptureOutcome.TimedOut=>"Capture timed out waiting for Windows. Check the selected surface and try manually again.",
        CaptureOutcome.Unsupported=>"Windows Graphics Capture is unavailable on this desktop.",
        CaptureOutcome.InvalidSelection=>"Capture selection is unavailable or outside supported bounds. Refresh and select again.",
        CaptureOutcome.Changed=>"The selected window/display changed geometry. Refresh and select it again.",
        CaptureOutcome.BlankOrProtected=>"Windows returned an empty or protected image. No snapshot retained.",
        _=>NativeHresult==unchecked((int)0x80070005)?"Windows denied capture access. Check screen-capture privacy settings and selection.":"Native Windows capture failed. Share only the safe stage/HRESULT below; no image or title is included."
    };
    public override string ToString()=>SafeText;
}
public sealed class CaptureFailure(CaptureReport report):IOException(report.UserMessage)
{
    public CaptureReport Report{get;}=report;
    public override string ToString()=>Report.SafeText;
}
// Eight immutable numeric/enum summaries only. Never retains exceptions, titles, HWNDs or pixels.
public sealed class CaptureDiagnostics
{
    private readonly object gate=new();private readonly Queue<CaptureReport> reports=new();private long next;
    public CaptureReport[] Reports{get{lock(gate)return reports.ToArray();}}
    public string Text=>string.Join(Environment.NewLine,Reports.Select(r=>r.SafeText));
    public CaptureReport Record(SurfaceKind kind,CaptureStage stage,CaptureOutcome outcome,long elapsed,Exception? cause=null) {
        lock(gate){var report=new CaptureReport(++next,kind,stage,outcome,Math.Clamp(elapsed,0,60000),cause?.HResult);if(reports.Count==8)reports.Dequeue();reports.Enqueue(report);return report;}
    }
}
