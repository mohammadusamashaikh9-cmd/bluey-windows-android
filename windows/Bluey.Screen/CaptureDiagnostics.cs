namespace Bluey.Screen;
public enum CaptureStage { Selection,Support,ActivationFactory,CreateWindowItem,CreateMonitorItem,ItemProjection,ItemSize,D3DDevice,DxgiDevice,DeviceProjection,FramePool,CaptureSession,StartSession,FirstFrame,FrameSurface,FramePoolResize,SurfaceCopy,JpegEncode,JpegDecode,GeometryValidation,BlankValidation,Complete }
public enum CaptureOutcome { Succeeded,Canceled,TimedOut,Unsupported,InvalidSelection,Changed,BlankOrProtected,FramesUnstable,NativeFailure }
public sealed record CaptureDimensions(int DwmWidth,int DwmHeight,int WgcWidth,int WgcHeight,int? FrameWidth,int? FrameHeight,double Dpi,CaptureMapping? Mapping)
{
    public string SafeText=>$"DWM={DwmWidth}x{DwmHeight} WGC={WgcWidth}x{WgcHeight} delta={Math.Abs((long)WgcWidth-DwmWidth)}x{Math.Abs((long)WgcHeight-DwmHeight)} Win32={FrameWidth?.ToString()??"?"}x{FrameHeight?.ToString()??"?"} dpiScale={(Dpi/96).ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)} map={Mapping?.ToString()??"unresolved"}";
}
public sealed record CaptureReport(long Attempt,SurfaceKind Kind,CaptureStage Stage,CaptureOutcome Outcome,long ElapsedMs,int? NativeHresult,CaptureDimensions? Dimensions=null,CaptureFrameTrace? Frames=null)
{
    public string SafeText=>$"capture#{Attempt} {Kind} {Stage} {Outcome} HR={(NativeHresult is {} hr?"0x"+hr.ToString("X8"):"none")} {ElapsedMs}ms"+(Dimensions is {} d?" "+d.SafeText:"")+(Frames is {} f?" "+f.SafeText:"");
    public string UserMessage=>Outcome switch {
        CaptureOutcome.Succeeded when Dimensions?.Mapping==CaptureMapping.PreviewOnly=>"Preview only: coordinate mapping is unverified. Pointing and screen sharing are disabled.",
        CaptureOutcome.Succeeded=>"Windows returned a bounded local image. Review the current preview before sharing.",
        CaptureOutcome.Canceled=>"Capture canceled or its outer deadline was reached. No snapshot retained.",
        CaptureOutcome.TimedOut=>"Capture timed out waiting for Windows. Check the selected surface and try manually again.",
        CaptureOutcome.Unsupported=>"Windows Graphics Capture is unavailable on this desktop.",
        CaptureOutcome.InvalidSelection=>"Capture selection is unavailable or outside supported bounds. Refresh and select again.",
        CaptureOutcome.Changed=>"The selected window/display changed geometry. Refresh and select it again.",
        CaptureOutcome.BlankOrProtected=>"Windows returned an empty or protected image. No snapshot retained.",
        CaptureOutcome.FramesUnstable=>"Windows did not return a stable complete frame within the capture limits. Keep the window still and try manually again. Share only the safe frame diagnostics.",
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
    public CaptureReport Record(SurfaceKind kind,CaptureStage stage,CaptureOutcome outcome,long elapsed,Exception? cause=null,CaptureDimensions? dimensions=null,CaptureFrameTrace? frames=null) {
        lock(gate){var report=new CaptureReport(++next,kind,stage,outcome,Math.Clamp(elapsed,0,60000),cause?.HResult,dimensions,frames);if(reports.Count==8)reports.Dequeue();reports.Enqueue(report);return report;}
    }
}
