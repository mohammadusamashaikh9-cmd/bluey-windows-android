using Xunit;
namespace Bluey.Screen.Tests;
public sealed class WorkflowTests
{
    private static Snapshot Shot()=>new(new(SurfaceKind.Window,1,new(0,0,400,300),new(0,0,800,600),96,96,400,300),[255,216,0,255,217],0,DateTimeOffset.UnixEpoch);
    private sealed class Source:ISnapshotSource {
        public int Calls;public TaskCompletionSource<Snapshot>? Pending;
        public Task<Snapshot> Capture(CancellationToken stop){Calls++;return Pending?.Task??Task.FromResult(Shot());}
    }
    private sealed class Reader:ILocalReader {
        public TaskCompletionSource<Analysis>? Pending;public ReadOnlyMemory<byte> Borrowed;public int Calls;
        public Task<Analysis> Analyze(ScreenMedia media,CancellationToken stop){Calls++;Borrowed=media.Jpeg;return Pending?.Task??Task.FromResult(new Analysis([new("known",new(10,10,100,20),[])],[],false,OcrStatus.Ready));}
    }
    private sealed class ManualTime:TimeProvider {
        private TickTimer? timer;
        public override ITimer CreateTimer(TimerCallback callback,object? state,TimeSpan dueTime,TimeSpan period)=>timer=new(callback,state,dueTime);
        public void Advance(TimeSpan by)=>timer!.Advance(by);
        private sealed class TickTimer(TimerCallback callback,object? state,TimeSpan due):ITimer {
            private bool disposed;public bool Change(TimeSpan time,TimeSpan period){due=time;return !disposed;}
            public void Advance(TimeSpan by){if(!disposed&&by>=due){disposed=true;callback(state);}}
            public void Dispose()=>disposed=true;public ValueTask DisposeAsync(){Dispose();return ValueTask.CompletedTask;}
        }
    }
    [Fact] public async Task NoCaptureWithoutUserActionAndDeniedCaptureLeavesNoSnapshot() {
        var state=new ScreenSession(()=>0);var workflow=new ScreenWorkflow(state);var source=new Source();
        await Assert.ThrowsAsync<ScreenException>(()=>workflow.Capture(false,source));Assert.Equal(0,source.Calls);
        source.Pending=new();source.Pending.SetException(new ScreenException("denied"));await Assert.ThrowsAsync<ScreenException>(()=>workflow.Capture(true,source));Assert.Null(state.Info);
    }
    [Fact] public async Task CaptureTimeoutAndLateCompletionAreBoundedAndDiscarded() {
        var time=new ManualTime();var state=new ScreenSession(()=>0);var workflow=new ScreenWorkflow(state,time);var source=new Source{Pending=new(TaskCreationOptions.RunContinuationsAsynchronously)};
        var capture=workflow.Capture(true,source);time.Advance(TimeSpan.FromSeconds(8));await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>capture);
        Assert.Null(state.Info);Assert.True(workflow.Busy);await Assert.ThrowsAsync<ScreenException>(()=>workflow.Capture(true,source));
        var late=Shot();source.Pending.SetResult(late);using(var timeout=new CancellationTokenSource(5000))while(!late.Disposed||workflow.Busy)await Task.Delay(1,timeout.Token);Assert.True(late.Disposed);Assert.False(workflow.Busy);
    }
    [Fact] public async Task AnalysisCancellationAndLateResultsCannotRestoreTextOrPixels() {
        var state=new ScreenSession(()=>0);var workflow=new ScreenWorkflow(state);await workflow.Capture(true,new Source());var reader=new Reader{Pending=new(TaskCreationOptions.RunContinuationsAsynchronously)};
        var analyze=workflow.Analyze(reader);state.Invalidate();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>analyze);Assert.All(reader.Borrowed.ToArray(),b=>Assert.Equal(0,b));
        reader.Pending.SetResult(new([new("stale private text",new(0,0,10,10),[])],[],false,OcrStatus.Ready));Assert.Null(state.Info);
    }
    [Fact] public async Task ValidSyntheticCaptureAndAnalysisHaveDeterministicTarget() {
        var state=new ScreenSession(()=>0);var workflow=new ScreenWorkflow(state);await workflow.Capture(true,new Source());var reader=new Reader();await workflow.Analyze(reader);
        Assert.Equal("known",Assert.Single(state.Info!.Targets).Text);Assert.Equal("L1",state.Info.Targets[0].Id);Assert.Equal(1,reader.Calls);Assert.All(reader.Borrowed.ToArray(),b=>Assert.Equal(0,b));
    }
}
