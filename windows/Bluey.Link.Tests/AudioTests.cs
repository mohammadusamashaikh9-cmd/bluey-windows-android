using System.Text.Json;
using Xunit;
namespace Bluey.Link.Tests;
public sealed class AudioTests
{
    private sealed class Answers : IAudioAnswers {
        public bool ContractVerified {get;set;}=true;public int Calls;public ReadOnlyMemory<byte> Borrowed;
        public TaskCompletionSource Entered=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<string>? Pending;public string Text="A completed synthetic answer.";
        public Task<string> Answer(ReadOnlyMemory<byte> pcm,CancellationToken stop){Calls++;Borrowed=pcm;Entered.TrySetResult();return Pending?.Task??Task.FromResult(Text);}
    }
    private sealed class Setup {
        public long Time;public Answers Fake=new();public LiveAuthority Authority;public string Consent=Guid.NewGuid().ToString(),Turn=Guid.NewGuid().ToString(),Auth="";
        public Setup(){Authority=new(new DeniedIssuer(),()=>Time,()=>1000,Fake,openAiEnabled:false);Authority.Open("tls","pin","device");Authority.SelectProvider(VoiceProvider.GeminiFree);ConsentTo();}
        public void ConsentTo(bool cloud=true,bool continuous=false)=>Authority.Handle("tls",M("liveConsent",new{consentId=Consent,microphone=true,cloud,continuous}));
        public void Authorize(){Auth=Authority.AuthorizeAudio("tls",Consent,true);Assert.Equal("audioGrant",Authority.Reader("tls").ReadAsync().AsTask().GetAwaiter().GetResult().Type);}
        public void Begin()=>Send("audioBegin",new{authorizationId=Auth,turnId=Turn,format=AudioProtocol.Format});
        public void Chunk(int index,byte value=31)=>Send("audioChunk",new{authorizationId=Auth,turnId=Turn,index,data=Convert.ToBase64String(Enumerable.Repeat(value,960).ToArray())});
        public void End(int count=5)=>Send("audioEnd",new{authorizationId=Auth,turnId=Turn,chunks=count,bytes=count*960});
        public void Send(string type,object payload)=>Authority.HandleAudio("tls",M(type,payload));
        public void Drain(){while(Authority.Reader("tls").TryRead(out _)){}}
    }
    private static Message M(string t,object p)=>Wire.Decode(Wire.Encode(t,Guid.NewGuid().ToString(),0,p)[..^1]);
    private static async Task<LivePacket> Next(LiveAuthority a){using var c=new CancellationTokenSource(2000);return await a.Reader("tls").ReadAsync(c.Token);}
    private static async Task WaitForCleanup(Setup x) {
        // Await the observable cleanup, not a scheduler-dependent 20 ms grace period.
        using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(5));
        // A terminal outcome is published after buffer clearing, including stale-result cancellation.
        while(x.Authority.AudioOutcomes.Length==0)await Task.Delay(1,deadline.Token);
    }
    [Fact] public void DefaultsAndUnverifiedContractFailClosedWithoutCallingAnyProvider() {
        var a=new LiveAuthority(new DeniedIssuer(),openAiEnabled:false);Assert.Equal(VoiceProvider.OfflineSimulation,a.Provider);Assert.False(a.GeminiAvailable);
        Assert.Throws<ProtocolException>(()=>a.SelectProvider(VoiceProvider.OpenAIRealtime));
        a.Open("tls","pin","device");var consent=Guid.NewGuid().ToString();a.Handle("tls",M("liveConsent",new{consentId=consent,microphone=true,cloud=true,continuous=false}));
        a.SelectProvider(VoiceProvider.GeminiFree);Assert.Throws<ProtocolException>(()=>a.AuthorizeAudio("tls",consent,true));
        Assert.Throws<ProtocolException>(()=>a.Authorize("tls",consent,LiveProtocol.Models[0],new()));
    }
    [Fact] public void EveryConsentAndWindowsFreeTierDecisionIsRequired() {
        var x=new Setup();Assert.Throws<ProtocolException>(()=>x.Authority.AuthorizeAudio("tls",x.Consent,false));
        x.ConsentTo(cloud:false);Assert.Throws<ProtocolException>(()=>x.Authority.AuthorizeAudio("tls",x.Consent,true));
        x.ConsentTo(continuous:true);Assert.Throws<ProtocolException>(()=>x.Authority.AuthorizeAudio("tls",x.Consent,true));
        x.ConsentTo();Assert.Throws<ProtocolException>(()=>x.Authority.AuthorizeAudio("tls",Guid.NewGuid().ToString(),true));
        x.Authority.SelectProvider(VoiceProvider.OfflineSimulation);Assert.Throws<ProtocolException>(()=>x.Authority.AuthorizeAudio("tls",x.Consent,true));Assert.Equal(0,x.Fake.Calls);
    }
    [Fact] public async Task AuthorizedCompletedTurnProducesOneCompletedTextAndWipesAudio() {
        var x=new Setup();x.Authorize();x.Begin();for(int i=0;i<5;i++)x.Chunk(i);Assert.Equal(0,x.Fake.Calls);x.End();
        var reply=await Next(x.Authority);Assert.Equal("audioReply",reply.Type);var m=M(reply.Type,reply.Payload);Assert.Equal(x.Fake.Text,Wire.Text(m.Payload,"text",256));
        Assert.Equal("audioDone",(await Next(x.Authority)).Type);Assert.Equal(1,x.Fake.Calls);Assert.All(x.Fake.Borrowed.ToArray(),b=>Assert.Equal(0,b));
        x.Turn=Guid.NewGuid().ToString();x.Begin();Assert.Equal("liveStop",(await Next(x.Authority)).Type);Assert.Equal(1,x.Fake.Calls);
    }
    [Theory][InlineData(0)][InlineData(2)] public void DuplicateOrReorderedChunkCancelsIncompleteTurn(int index) {
        var x=new Setup();x.Authorize();x.Begin();x.Chunk(0);Assert.Throws<ProtocolException>(()=>x.Chunk(index));Assert.False(x.Authority.Peers.Single().Authorized);Assert.Equal(0,x.Fake.Calls);
    }
    [Fact] public void SizeCountTimeoutAndWrongTurnAreRejectedBeforeInference() {
        var x=new Setup();x.Authorize();x.Begin();x.Chunk(0);Assert.Throws<ProtocolException>(()=>x.End());Assert.Equal(0,x.Fake.Calls);
        x.Authorize();x.Begin();x.Time=8001;x.Authority.Expire();Assert.False(x.Authority.Peers.Single().Authorized);
        x.Authorize();x.Begin();x.Turn=Guid.NewGuid().ToString();Assert.Throws<ProtocolException>(()=>x.Chunk(0));Assert.Equal(0,x.Fake.Calls);
    }
    [Fact] public async Task FullFiveSecondTurnUses250SmallFramesAndNoGlobalFrameIncrease() {
        var x=new Setup();x.Authorize();x.Begin();for(int i=0;i<250;i++){x.Chunk(i);var frame=Wire.Encode("audioChunk",Guid.NewGuid().ToString(),i,new{authorizationId=x.Auth,turnId=x.Turn,index=i,data=Convert.ToBase64String(new byte[960])});Assert.InRange(frame.Length,1000,2048);}
        x.End(250);Assert.Equal("audioReply",(await Next(x.Authority)).Type);Assert.Equal(240000,x.Fake.Borrowed.Length);Assert.Equal(4096,Wire.MaxFrame);
    }
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public async Task CancelDisconnectConsentWithdrawalAndProviderChangeDropPendingReply(int mode) {
        var x=new Setup();x.Fake.Pending=new(TaskCreationOptions.RunContinuationsAsynchronously);x.Authorize();x.Begin();for(int i=0;i<5;i++)x.Chunk(i);x.End();
        await x.Fake.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));Assert.Equal(1,x.Fake.Calls);
        if(mode==0)x.Send("audioCancel",new{authorizationId=x.Auth,turnId=x.Turn});if(mode==1)x.Authority.Close("tls");if(mode==2)x.ConsentTo(cloud:false);if(mode==3)x.Authority.SelectProvider(VoiceProvider.OfflineSimulation);
        x.Fake.Pending.SetResult("Stale answer must never appear");await WaitForCleanup(x);
        if(mode!=1)while(x.Authority.Reader("tls").TryRead(out var packet))Assert.DoesNotContain(packet.Type,new[]{"audioReply","audioDone"});Assert.All(x.Fake.Borrowed.ToArray(),b=>Assert.Equal(0,b));
    }
    [Theory][InlineData("")][InlineData("\n\t")]
    public async Task EmptyOrBlockedAnswerStopsWithoutRetryOrFallback(string text) {
        var x=new Setup();x.Fake.Text=text;x.Authorize();x.Begin();for(int i=0;i<5;i++)x.Chunk(i);x.End();Assert.Equal("liveStop",(await Next(x.Authority)).Type);Assert.Equal(1,x.Fake.Calls);
    }
    [Fact] public async Task InferenceFailureHasNoRetryAndNoProviderDetails() {
        var x=new Setup();x.Fake.Pending=new(TaskCreationOptions.RunContinuationsAsynchronously);x.Authorize();x.Begin();for(int i=0;i<5;i++)x.Chunk(i);x.End();x.Fake.Pending.SetException(new IOException("synthetic private provider details"));var stop=await Next(x.Authority);Assert.Equal("liveStop",stop.Type);Assert.DoesNotContain("private",stop.ToString());Assert.Equal(1,x.Fake.Calls);
    }
    [Fact] public async Task DeterministicRequestTimeoutCancelsWipesAndNeverRetries() {
        var x=new Setup();x.Fake.Pending=new(TaskCreationOptions.RunContinuationsAsynchronously);x.Authorize();x.Begin();for(int i=0;i<5;i++)x.Chunk(i);x.End();
        await x.Fake.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));x.Time=20000;x.Authority.Expire();var timeout=await Next(x.Authority);Assert.Equal("liveStop",timeout.Type);Assert.Equal("timeout",Wire.Text(M(timeout.Type,timeout.Payload).Payload,"code"));
        x.Fake.Pending.SetResult("late");await WaitForCleanup(x);Assert.All(x.Fake.Borrowed.ToArray(),b=>Assert.Equal(0,b));Assert.False(x.Authority.Peers.Single().Authorized);Assert.Equal(1,x.Fake.Calls);Assert.Equal("timeout",Assert.Single(x.Authority.AudioOutcomes).Code);
    }

}
