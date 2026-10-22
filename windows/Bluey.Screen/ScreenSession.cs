using System.Security.Cryptography;
namespace Bluey.Screen;
public sealed class Snapshot : IDisposable
{
    public const int MaxJpegBytes=768*1024,MaxAgeMs=60000;
    private readonly byte[] jpeg;
    public string Id{get;}=Guid.NewGuid().ToString();public Surface Surface{get;}public long At{get;}public DateTimeOffset CapturedUtc{get;}public bool Disposed{get;private set;}
    public Target[] Targets{get;internal set;}=[];public OcrStatus? Ocr{get;internal set;}
    public Snapshot(Surface surface,byte[] jpeg,long at,DateTimeOffset utc) {
        if(jpeg.Length is <5 or >MaxJpegBytes||jpeg[0]!=255||jpeg[1]!=216||jpeg[^2]!=255||jpeg[^1]!=217)throw new ScreenException("image-invalid");
        Surface=surface;this.jpeg=jpeg;At=at;CapturedUtc=utc;
    }
    internal byte[] CopyJpeg(){if(Disposed)throw new ScreenException("discarded");return jpeg.ToArray();}
    public void Dispose(){if(Disposed)return;Disposed=true;CryptographicOperations.ZeroMemory(jpeg);Targets=[];}
    public override string ToString()=>"[snapshot pixels/text redacted]";
}
public sealed record SnapshotInfo(string Id,Surface Surface,long AgeMs,DateTimeOffset CapturedUtc,Target[] Targets,OcrStatus? Ocr);
public sealed record CaptureWork(long Generation,CancellationToken Stop);
public sealed class ScreenMedia:IDisposable
{
    private readonly byte[] jpeg;public ReadOnlyMemory<byte> Jpeg=>jpeg;
    public SnapshotInfo Info{get;}public CancellationToken Stop{get;}
    internal ScreenMedia(SnapshotInfo info,byte[] jpeg,CancellationToken stop){Info=info;this.jpeg=jpeg;Stop=stop;}
    public void Dispose()=>CryptographicOperations.ZeroMemory(jpeg);
    public override string ToString()=>"[screen media redacted]";
}
public sealed class ApprovedScreen:IDisposable
{
    private readonly ScreenMedia media;internal readonly long Generation;internal readonly string Lease,Consent,Auth,Turn;internal readonly long Expires;
    public ReadOnlyMemory<byte> Jpeg=>media.Jpeg;public SnapshotInfo Info=>media.Info;public CancellationToken Stop=>media.Stop;
    internal ApprovedScreen(ScreenMedia media,long generation,string lease,string consent,string auth,string turn,long expires){this.media=media;Generation=generation;Lease=lease;Consent=consent;Auth=auth;Turn=turn;Expires=expires;}
    public void Dispose()=>media.Dispose();public override string ToString()=>"[approved screen redacted]";
}
public sealed record PointDirective(long Generation,long Expires,Surface Surface,VisualPoint Point,string Lease,string Auth,string Turn);
public sealed class ScreenSession(Func<long>? clock=null,Func<Surface,bool>? surfaceCurrent=null):IDisposable
{
    private readonly object gate=new();private readonly Func<long> now=clock??(()=>Environment.TickCount64);
    private readonly Func<Surface,bool> matches=surfaceCurrent??(_=>true);
    private long generation;private Snapshot? snapshot;private string? previewed;private CancellationTokenSource work=new();private ApprovedScreen? active;private PointDirective? point;
    private (string Snapshot,string Lease,string Consent,string? Auth)? approval;
    public SnapshotInfo? Info {get{lock(gate)return snapshot is {} s&&!s.Disposed&&now()-s.At<Snapshot.MaxAgeMs?InfoOf(s):null;}}
    public bool CloudApproved {get{lock(gate)return approval!=null&&Info!=null;}}
    private SnapshotInfo InfoOf(Snapshot s)=>new(s.Id,s.Surface,Math.Max(0,now()-s.At),s.CapturedUtc,s.Targets.ToArray(),s.Ocr);
    private Snapshot Current(string id){if(snapshot is not {} s||s.Id!=id||s.Disposed||!matches(s.Surface)||now()<s.At||now()-s.At>=Snapshot.MaxAgeMs)throw new ScreenException("stale-snapshot");return s;}
    public CaptureWork BeginCapture(bool userAction){lock(gate){if(!userAction)throw new ScreenException("capture-denied");Invalidate();return new(generation,work.Token);}}
    public bool CompleteCapture(CaptureWork capture,Snapshot image){lock(gate){if(capture.Generation!=generation||capture.Stop.IsCancellationRequested||now()<image.At||now()-image.At>=Snapshot.MaxAgeMs){image.Dispose();return false;}snapshot=image;return true;}}
    public void CancelCapture(CaptureWork capture){lock(gate){if(capture.Generation==generation)Invalidate();}}
    public ScreenMedia CopyCurrent(){lock(gate){var s=Current(snapshot?.Id??"");return new(InfoOf(s),s.CopyJpeg(),work.Token);}}
    public void SetAnalysis(string id,Analysis analysis){lock(gate){var s=Current(id);s.Targets=Targets.Extract(s.Surface,analysis);s.Ocr=analysis.Ocr;}}
    public void MarkPreviewReady(string id){lock(gate){Current(id);previewed=id;}}
    public void Approve(string id,string lease,string consent,string? auth,bool reviewed){lock(gate){var s=Current(id);if(!reviewed||previewed!=id||s.Surface.Kind==SurfaceKind.Synthetic||string.IsNullOrEmpty(lease)||string.IsNullOrEmpty(consent))throw new ScreenException("screen-consent");approval=(id,lease,consent,auth);}}
    public void BindAuthorization(string lease,string consent,string auth){lock(gate){if(approval is {} a&&a.Lease==lease&&a.Consent==consent)approval=(a.Snapshot,lease,consent,auth);}}
    public ApprovedScreen? TakeForQuestion(string lease,string consent,string auth,string turn){lock(gate){if(approval is not {} a||a.Lease!=lease||a.Consent!=consent)return null;if(a.Auth!=auth)throw new ScreenException("screen-authorization");var s=Current(a.Snapshot);approval=null;return active=new(new(InfoOf(s),s.CopyJpeg(),work.Token),generation,lease,consent,auth,turn,s.At+Snapshot.MaxAgeMs);}}
    public bool IsValid(ApprovedScreen screen){lock(gate)return active==screen&&screen.Generation==generation&&!screen.Stop.IsCancellationRequested&&now()<screen.Expires&&matches(screen.Info.Surface);}
    public bool IsValid(PointDirective directive){lock(gate)return directive.Generation==generation&&!work.IsCancellationRequested&&now()<directive.Expires&&matches(directive.Surface);}
    public void CompleteQuestion(ApprovedScreen screen,string? target){lock(gate){if(!IsValid(screen))throw new ScreenException("stale-reply");var found=screen.Info.Targets.FirstOrDefault(t=>t.Id==target);if(found!=null)point=new(generation,screen.Expires,screen.Info.Surface,found.Point(screen.Info.Surface),screen.Lease,screen.Auth,screen.Turn);snapshot?.Dispose();snapshot=null;previewed=null;active=null;screen.Dispose();}}
    public PointDirective? TakePoint(string lease,string turn){lock(gate){if(point is not {} p||p.Lease!=lease||p.Turn!=turn||!IsValid(p))return null;point=null;return p;}}
    public VisualPoint PointAtTarget(string id,string target){lock(gate){var s=Current(id);var t=s.Targets.FirstOrDefault(t=>t.Id==target)??throw new ScreenException("missing-target");return t.Point(s.Surface);}}
    public VisualPoint PointAtSpot(string id,double x,double y){lock(gate){var g=Current(id).Surface;var p=g.GridToDip(x,y);return new(p,new(p.X-16,p.Y-16,32,32));}}
    public void Expire(){lock(gate){if(snapshot is {} s&&now()-s.At>=Snapshot.MaxAgeMs||point is {} p&&now()>=p.Expires)Invalidate();}}
    public void Invalidate(){lock(gate){generation++;work.Cancel();work.Dispose();work=new();snapshot?.Dispose();snapshot=null;active?.Dispose();active=null;approval=null;previewed=null;point=null;}}
    public void Dispose(){Invalidate();lock(gate)work.Dispose();}
}
