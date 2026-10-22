using System.Text.Json;
using Bluey.Screen;
using Xunit;
namespace Bluey.Link.Tests;
public sealed class ScreenAuthorityTests
{
    private sealed class Answers:IAudioAnswers,IScreenAudioAnswers {
        public bool ContractVerified=>true;public bool ScreenContractVerified=>true;public int AudioCalls,ScreenCalls;public ApprovedScreen? Media;public TaskCompletionSource Entered=new(TaskCreationOptions.RunContinuationsAsynchronously);public TaskCompletionSource<ScreenReply>? Pending;
        public Task<string> Answer(ReadOnlyMemory<byte> pcm,CancellationToken stop){AudioCalls++;return Task.FromResult("Audio-only synthetic answer.");}
        public Task<ScreenReply> AnswerWithScreen(ReadOnlyMemory<byte> pcm,ApprovedScreen screen,CancellationToken stop){ScreenCalls++;Media=screen;Entered.TrySetResult();return Pending?.Task??Task.FromResult(new ScreenReply("The word is ALPHA.","W1"));}
    }
    private static Message M(string type,object p)=>Wire.Decode(Wire.Encode(type,Guid.NewGuid().ToString(),0,p)[..^1]);
    private sealed class Setup {
        public long Time;public readonly Answers Fake=new();public readonly LiveAuthority Authority;
        public string Consent=Guid.NewGuid().ToString(),Turn=Guid.NewGuid().ToString(),Auth="",Id="";
        public Setup(){Authority=new(new DeniedIssuer(),()=>Time,audioAnswers:Fake);Authority.Open("tls","pin","phone");Authority.SelectProvider(VoiceProvider.GeminiFree);while(Authority.Reader("tls").TryRead(out _)){}Authority.Handle("tls",M("liveConsent",new{consentId=Consent,microphone=true,cloud=true,continuous=false}));}
        public void Capture(){var shot=new Snapshot(new(SurfaceKind.Window,1,new(0,0,800,600),new(0,0,1920,1080),96,96,800,600),[255,216,0,255,217],Time,DateTimeOffset.UnixEpoch);Id=shot.Id;Authority.Screens.CompleteCapture(Authority.Screens.BeginCapture(true),shot);Authority.Screens.SetAnalysis(Id,new([new("ALPHA",new(10,10,80,20),[new("ALPHA",new(10,10,80,20))])],[],true,OcrStatus.Ready));}
        public void Approve(){Authority.Screens.MarkPreviewReady(Id);Authority.ApproveScreen("tls",Consent,Id,true);}
        public void Grant(){Auth=Authority.AuthorizeAudio("tls",Consent,true);while(Authority.Reader("tls").TryRead(out _)){}}
        public void SendTurn(){Authority.HandleAudio("tls",M("audioBegin",new{authorizationId=Auth,turnId=Turn,format=AudioProtocol.Format}));for(int n=0;n<5;n++)Authority.HandleAudio("tls",M("audioChunk",new{authorizationId=Auth,turnId=Turn,index=n,data=Convert.ToBase64String(new byte[960])}));Authority.HandleAudio("tls",M("audioEnd",new{authorizationId=Auth,turnId=Turn,chunks=5,bytes=4800}));}
        public async Task<LivePacket> Next()=>await Authority.Reader("tls").ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(3));
    }
    [Fact] public async Task ScreenConsentIsSeparateAndAudioOnlyPathRemainsUnchanged() {
        var s=new Setup();s.Capture();s.Grant();s.SendTurn();Assert.Equal("audioReply",(await s.Next()).Type);Assert.Equal("audioDone",(await s.Next()).Type);Assert.Equal(1,s.Fake.AudioCalls);Assert.Equal(0,s.Fake.ScreenCalls);
    }
    [Fact] public async Task ReviewedCurrentPhoneScreenIsConsumedBySameOneQuestionThenPointsOnPresentationOnce() {
        var s=new Setup();s.Capture();s.Approve();s.Grant();PointDirective? directed=null;int points=0;s.Authority.PointRequested+=p=>{directed=p;points++;};s.SendTurn();
        var reply=await s.Next();Assert.Equal("audioReply",reply.Type);Assert.Equal("The word is ALPHA.",Wire.Text(M(reply.Type,reply.Payload).Payload,"text"));Assert.Equal("audioDone",(await s.Next()).Type);
        Assert.Equal(1,s.Fake.ScreenCalls);Assert.Equal(0,s.Fake.AudioCalls);Assert.Null(s.Authority.Screens.Info);Assert.All(s.Fake.Media!.Jpeg.ToArray(),b=>Assert.Equal(0,b));Assert.Null(directed);
        s.Authority.ObservePresentation("tls",M("presentation",new{turnId=s.Turn,state="speaking",@event="delta",text="The word is ALPHA."}));
        Assert.NotNull(directed);Assert.True(s.Authority.CanPresent(directed!));Assert.Equal(33,directed!.Point.Position.Y); // Original point_at: target bottom+3 DIP.
        s.Authority.ObservePresentation("tls",M("presentation",new{turnId=s.Turn,state="speaking",@event="delta",text="again"}));Assert.Equal(1,points);
        s.Authority.StopAll();Assert.False(s.Authority.CanPresent(directed));
    }
    [Fact] public void MissingTrustWrongConsentUnreviewedAndExpiredSnapshotNeverApprove() {
        var s=new Setup();s.Capture();Assert.Throws<ProtocolException>(()=>s.Authority.ApproveScreen("untrusted",s.Consent,s.Id,true));Assert.Throws<ProtocolException>(()=>s.Authority.ApproveScreen("tls",Guid.NewGuid().ToString(),s.Id,true));
        Assert.Throws<ScreenException>(()=>s.Authority.ApproveScreen("tls",s.Consent,s.Id,true));s.Authority.Screens.MarkPreviewReady(s.Id);Assert.Throws<ScreenException>(()=>s.Authority.ApproveScreen("tls",s.Consent,s.Id,false));s.Time=60000;Assert.Throws<ScreenException>(()=>s.Approve());Assert.Equal(0,s.Fake.ScreenCalls);
    }
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public async Task StopDisconnectSleepAndScreenWithdrawalDiscardPendingProviderResult(int mode) {
        var s=new Setup();s.Fake.Pending=new(TaskCreationOptions.RunContinuationsAsynchronously);s.Capture();s.Approve();s.Grant();s.SendTurn();
        await s.Fake.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));Assert.Equal(1,s.Fake.ScreenCalls);
        if(mode==0)s.Authority.StopAll();if(mode==1)s.Authority.Close("tls");if(mode==2)s.Authority.ObservePresentation("tls",M("presentation",new{turnId=s.Turn,state="asleep",@event="cancel",text=""}));if(mode==3)s.Authority.Screens.Invalidate();
        s.Fake.Pending.SetResult(new("Stale answer", "W1"));
        using(var timeout=new CancellationTokenSource(5000))while(s.Authority.AudioOutcomes.Length==0)await Task.Delay(1,timeout.Token);
        Assert.Null(s.Authority.Screens.Info);Assert.All(s.Fake.Media!.Jpeg.ToArray(),b=>Assert.Equal(0,b));
        if(mode!=1)while(s.Authority.Reader("tls").TryRead(out var p))Assert.DoesNotContain(p.Type,new[]{"audioReply","audioDone"});
    }
}
