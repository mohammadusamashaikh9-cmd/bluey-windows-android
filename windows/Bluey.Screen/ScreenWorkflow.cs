namespace Bluey.Screen;
public interface ISnapshotSource {Task<Snapshot> Capture(CancellationToken stop);}
public interface ILocalReader {Task<Analysis> Analyze(ScreenMedia media,CancellationToken stop);}
// The platform adapters are injected; no OS calls/provider requests occur in this coordinator.
public sealed class ScreenWorkflow(ScreenSession state,TimeProvider? time=null)
{
    private readonly TimeProvider clock=time??TimeProvider.System;private int busy;
    public bool Busy=>Volatile.Read(ref busy)!=0;
    public async Task<bool> Capture(bool userAction,ISnapshotSource source)
    {
        if(Interlocked.CompareExchange(ref busy,1,0)!=0)throw new ScreenException("capture-busy");
        CaptureWork? work=null;Task<Snapshot>? pending=null;bool committed=false;
        try {
            work=state.BeginCapture(userAction);using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(8),clock);
            using var linked=CancellationTokenSource.CreateLinkedTokenSource(work.Stop,deadline.Token);
            pending=source.Capture(linked.Token);var snapshot=await pending.WaitAsync(linked.Token).ConfigureAwait(false);
            return committed=state.CompleteCapture(work,snapshot);
        } catch {if(work!=null)state.CancelCapture(work);throw;}
        finally {
            if(pending is {IsCompleted:false}) {
                _=pending.ContinueWith(t=>{if(t.IsCompletedSuccessfully)t.Result.Dispose();Volatile.Write(ref busy,0);},CancellationToken.None,TaskContinuationOptions.ExecuteSynchronously,TaskScheduler.Default);
            } else {if(pending is {IsCompletedSuccessfully:true}&&!committed)pending.Result.Dispose();Volatile.Write(ref busy,0);}
        }
    }
    public async Task Analyze(ILocalReader reader)
    {
        if(Interlocked.CompareExchange(ref busy,1,0)!=0)throw new ScreenException("analysis-busy");
        ScreenMedia? media=null;Task<Analysis>? pending=null;
        try {
            media=state.CopyCurrent();using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(5),clock);
            using var linked=CancellationTokenSource.CreateLinkedTokenSource(media.Stop,deadline.Token);
            pending=reader.Analyze(media,linked.Token);var result=await pending.WaitAsync(linked.Token).ConfigureAwait(false);state.SetAnalysis(media.Info.Id,result);
        } finally {
            media?.Dispose();
            if(pending is {IsCompleted:false})_=pending.ContinueWith(_=>Volatile.Write(ref busy,0),CancellationToken.None,TaskContinuationOptions.ExecuteSynchronously,TaskScheduler.Default);
            else Volatile.Write(ref busy,0);
        }
    }
}
