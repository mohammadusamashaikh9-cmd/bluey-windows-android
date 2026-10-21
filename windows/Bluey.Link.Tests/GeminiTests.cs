using System.Buffers.Binary;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
namespace Bluey.Link.Tests;

public sealed class GeminiTests
{
    private const string Good="""{"candidates":[{"finishReason":"STOP","content":{"role":"model","parts":[{"text":"A friendly short answer."}]}}]}""";
    private sealed class Key:IMasterKey {
        public int Reads;public byte[]? Borrowed;public bool Missing,Invalid;
        public byte[] Read(){Reads++;if(Missing)throw new FileNotFoundException("synthetic private path");return Borrowed=Invalid?[0]:Encoding.ASCII.GetBytes("synthetic-gemini-key-never-valid");}
    }
    private sealed class Handler:HttpMessageHandler {
        public int Calls;public byte[]? Body;public HttpRequestMessage? Request;
        public Func<HttpRequestMessage,CancellationToken,Task<HttpResponseMessage>>? Respond;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken stop){Calls++;Request=request;Body=await request.Content!.ReadAsByteArrayAsync(stop);return Respond==null?Json(Good):await Respond(request,stop);}
    }
    private static HttpResponseMessage Json(string text,HttpStatusCode status=HttpStatusCode.OK)=>new(status){Content=new StringContent(text,Encoding.UTF8,"application/json")};
    private static byte[] Pcm(int chunks=5)=>Enumerable.Range(0,960*chunks).Select(i=>(byte)i).ToArray();
    private static async Task Failure(GeminiAnswers client,AudioAnswerFailure reason) {
        var error=await Assert.ThrowsAsync<AudioAnswerException>(()=>client.Answer(Pcm(),default));Assert.Equal(reason,error.Reason);
        Assert.DoesNotContain("synthetic private",error.ToString());Assert.Null(error.InnerException);
    }
    [Theory][InlineData(5)][InlineData(250)] public void WaveHeaderAndPcmAreExact(int chunks) {
        var pcm=Pcm(chunks);var wav=PcmWave.Encode(pcm);Assert.Equal(pcm.Length+44,wav.Length);
        Assert.Equal("RIFF",Encoding.ASCII.GetString(wav,0,4));Assert.Equal(wav.Length-8,BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(4)));
        Assert.Equal("WAVEfmt ",Encoding.ASCII.GetString(wav,8,8));Assert.Equal(16,BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(16)));
        Assert.Equal(1,BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(20)));Assert.Equal(1,BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(22)));
        Assert.Equal(24000,BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(24)));Assert.Equal(48000,BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(28)));
        Assert.Equal(2,BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(32)));Assert.Equal(16,BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(34)));
        Assert.Equal("data",Encoding.ASCII.GetString(wav,36,4));Assert.Equal(pcm.Length,BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(40)));Assert.Equal(pcm,wav[44..]);
    }
    [Fact] public async Task FixedInlineWavRequestAndTextOnlyReplyUseFakeHttpExclusively() {
        var key=new Key();var h=new Handler();using var http=new HttpClient(h);var client=new GeminiAnswers(key,http);
        Assert.True(client.ContractVerified);Assert.Equal(0,key.Reads);Assert.Equal(0,h.Calls);
        Assert.Equal("A friendly short answer.",await client.Answer(Pcm(),default));Assert.Equal(1,h.Calls);var request=h.Request!;
        Assert.Equal(HttpMethod.Post,request.Method);Assert.Equal("https://generativelanguage.googleapis.com/v1beta/models/gemini-3.8-flash:generateContent",request.RequestUri!.AbsoluteUri);Assert.Equal("",request.RequestUri.Query);
        Assert.Equal("synthetic-gemini-key-never-valid",Assert.Single(request.Headers.GetValues("x-goog-api-key")));Assert.Null(request.Headers.Authorization);Assert.Empty(http.DefaultRequestHeaders);
        using var doc=JsonDocument.Parse(h.Body!);var root=doc.RootElement;
        Assert.Contains("Do not transcribe",root.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString());
        var parts=root.GetProperty("contents")[0].GetProperty("parts");Assert.Equal("audio/wav",parts[0].GetProperty("inlineData").GetProperty("mimeType").GetString());
        Assert.Equal(PcmWave.Encode(Pcm()),parts[0].GetProperty("inlineData").GetProperty("data").GetBytesFromBase64());
        var config=root.GetProperty("generationConfig");Assert.Equal(512,config.GetProperty("maxOutputTokens").GetInt32());Assert.False(config.TryGetProperty("candidateCount",out _));
        Assert.Equal("low",config.GetProperty("thinkingConfig").GetProperty("thinkingLevel").GetString());
        Assert.Equal("TEXT",config.GetProperty("responseModalities")[0].GetString());Assert.Equal("text/plain",config.GetProperty("responseMimeType").GetString());Assert.Empty(root.GetProperty("tools").EnumerateArray());
        Assert.DoesNotContain("synthetic-gemini-key",Encoding.UTF8.GetString(h.Body!));Assert.All(key.Borrowed!,b=>Assert.Equal(0,b));
    }
    [Fact] public async Task MaximumInputIncludingBase64PlusFitsFixedRequestBound() {
        var h=new Handler();using var http=new HttpClient(h);var client=new GeminiAnswers(new Key(),http);
        await client.Answer(Enumerable.Repeat((byte)251,240000).ToArray(),default);Assert.InRange(h.Body!.Length,320000,GeminiAnswers.MaxRequestBytes);
    }
    [Fact] public void WrongModelCannotConstructAnyRequest() {
        var key=new Key();var h=new Handler();using var http=new HttpClient(h);
        Assert.Equal(AudioAnswerFailure.Model,Assert.Throws<AudioAnswerException>(()=>new GeminiAnswers(key,http,"another-model")).Reason);Assert.Equal(0,key.Reads);Assert.Equal(0,h.Calls);
    }
    [Theory][InlineData(0)][InlineData(4799)][InlineData(4801)][InlineData(240960)]
    public async Task InvalidAudioIsRejectedBeforeCredentialOrHttp(int bytes) {
        var key=new Key();var h=new Handler();using var http=new HttpClient(h);var client=new GeminiAnswers(key,http);
        var error=await Assert.ThrowsAsync<AudioAnswerException>(()=>client.Answer(new byte[bytes],default));Assert.Equal(AudioAnswerFailure.InvalidAudio,error.Reason);Assert.Equal(0,key.Reads);Assert.Equal(0,h.Calls);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task MissingOrInvalidCredentialNeverSends(bool invalid) {
        var key=new Key{Missing=!invalid,Invalid=invalid};var h=new Handler();using var http=new HttpClient(h);
        await Failure(new(key,http),AudioAnswerFailure.Credential);Assert.Equal(0,h.Calls);if(key.Borrowed!=null)Assert.All(key.Borrowed,b=>Assert.Equal(0,b));
    }
    [Theory][InlineData(429,AudioAnswerFailure.Quota)][InlineData(401,AudioAnswerFailure.Credential)][InlineData(403,AudioAnswerFailure.Credential)]
    [InlineData(400,AudioAnswerFailure.Configuration)][InlineData(500,AudioAnswerFailure.Provider)][InlineData(503,AudioAnswerFailure.Unavailable)][InlineData(302,AudioAnswerFailure.Provider)]
    public async Task HttpFailuresNeverRetryFollowRedirectOrExposeBodies(int status,AudioAnswerFailure reason) {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json("synthetic private provider body",(HttpStatusCode)status))};using var http=new HttpClient(h);
        await Failure(new(new Key(),http),reason);Assert.Equal(1,h.Calls);
    }
    [Theory][InlineData(503,"Unavailable")][InlineData(400,"Configuration")]
    public async Task ConfigurationAndTemporaryFailuresHaveDistinctSafeCategories(int status,string expected) {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json("private synthetic body",(HttpStatusCode)status))};using var http=new HttpClient(h);
        var e=await Assert.ThrowsAsync<AudioAnswerException>(()=>new GeminiAnswers(new Key(),http).Answer(Pcm(),default));
        Assert.Equal(expected,e.Reason.ToString());Assert.DoesNotContain("private",e.ToString());Assert.Equal(1,h.Calls);
    }
    [Theory]
    [InlineData("{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\"}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\",\"content\":{\"parts\":[]}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\",\"content\":{\"parts\":[{\"thought\":true,\"text\":\"synthetic reasoning\"}]}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\",\"content\":{\"parts\":[{\"text\":\"   \"}]}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\",\"content\":{\"role\":\"model\"}}]}")]
    public async Task ExhaustedBudgetWithoutVisibleAnswerIsDistinctFromEmpty(string json) {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(json))};using var http=new HttpClient(h);
        var e=await Assert.ThrowsAsync<AudioAnswerException>(()=>new GeminiAnswers(new Key(),http).Answer(Pcm(),default));
        Assert.Equal("Budget",e.Reason.ToString());Assert.Equal(1,h.Calls);
    }
    [Theory]
    [InlineData("{\"promptFeedback\":{\"blockReason\":\"SAFETY\"}}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"SAFETY\"}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"RECITATION\"}]}")]
    [InlineData("{\"candidates\":[{\"safetyRatings\":[{\"blocked\":true}],\"finishReason\":\"STOP\"}]}")]
    public async Task SafetyAndRefusalResponsesNeverBecomeCaptions(string json) {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(json))};using var http=new HttpClient(h);await Failure(new(new Key(),http),AudioAnswerFailure.Blocked);Assert.Equal(1,h.Calls);
    }
    [Theory]
    [InlineData("{")][InlineData("[]")][InlineData("{\"candidates\":true}")]
    [InlineData("{\"candidates\":[],\"candidates\":[]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"text\":42}]}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"functionCall\":{\"name\":\"action\"}}]}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"inlineData\":{}}]}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"role\":\"user\",\"parts\":[{\"text\":\"wrong role\"}]}}]}")]
    public async Task MalformedOrNonTextResponsesAreRejected(string json) {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(json))};using var http=new HttpClient(h);await Failure(new(new Key(),http),AudioAnswerFailure.Malformed);
    }
    [Theory][InlineData("{}")][InlineData("{\"candidates\":[]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\"}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"text\":\"  \\n\"}]}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"role\":\"model\"}}]}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":null}}]}")]
    public async Task EmptyAnswersDoNotTriggerAnotherRequest(string json) {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(json))};using var http=new HttpClient(h);await Failure(new(new Key(),http),AudioAnswerFailure.Empty);Assert.Equal(1,h.Calls);
    }
    [Fact] public async Task SuccessfulHttpErrorEnvelopeStillFailsClosed() {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json("{\"error\":{\"code\":429,\"message\":\"synthetic private message\"}}"))};using var http=new HttpClient(h);await Failure(new(new Key(),http),AudioAnswerFailure.Quota);
    }
    [Fact] public async Task NetworkErrorsAreSanitizedAndNeverRetried() {
        var h=new Handler{Respond=(_,_)=>throw new HttpRequestException("synthetic private network details")};using var http=new HttpClient(h);await Failure(new(new Key(),http),AudioAnswerFailure.Network);Assert.Equal(1,h.Calls);
    }
    [Fact] public async Task InvalidUtf8AndChunkedOversizeBodiesAreRejected() {
        foreach(var bytes in new[]{new byte[]{0xff,0xfe},Encoding.UTF8.GetBytes(new string(' ',GeminiAnswers.MaxResponseBytes+1))}) {
            var h=new Handler{Respond=(_,_)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new UnknownContent(bytes)})};using var http=new HttpClient(h);await Failure(new(new Key(),http),AudioAnswerFailure.Malformed);
        }
    }
    [Fact] public async Task TextIsBoundedNormalizedAndThoughtsAreNeverDisplayed() {
        var text=" Hello\n\tworld\u202e "+new string('x',300);
        var json=JsonSerializer.Serialize(new{candidates=new[]{new{finishReason="MAX_TOKENS",content=new{role="model",parts=new object[]{new{thought=true,text="private reasoning"},new{text}}}}}});
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(json))};using var http=new HttpClient(h);var answer=await new GeminiAnswers(new Key(),http).Answer(Pcm(),default);
        Assert.Equal(256,answer.Length);Assert.StartsWith("Hello world ",answer);Assert.DoesNotContain("reasoning",answer);Assert.DoesNotContain('\u202e',answer);Assert.DoesNotContain('\n',answer);
    }
    [Fact] public async Task AlreadyCanceledRequestDoesNotReadKeyOrUseHttp() {
        var key=new Key();var h=new Handler();using var http=new HttpClient(h);using var stop=new CancellationTokenSource();stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>new GeminiAnswers(key,http).Answer(Pcm(),stop.Token));Assert.Equal(0,key.Reads);Assert.Equal(0,h.Calls);
    }
    private sealed class ManualTime:TimeProvider {
        private Timer? timer;
        public override ITimer CreateTimer(TimerCallback callback,object? state,TimeSpan dueTime,TimeSpan period)=>timer=new Timer(callback,state,dueTime);
        public void Advance(TimeSpan by)=>timer!.Advance(by);
        private sealed class Timer(TimerCallback callback,object? state,TimeSpan due):ITimer {
            private bool disposed;
            public bool Change(TimeSpan dueTime,TimeSpan period){due=dueTime;return !disposed;}
            public void Advance(TimeSpan by){if(!disposed&&by>=due){disposed=true;callback(state);}}
            public void Dispose()=>disposed=true;public ValueTask DisposeAsync(){Dispose();return ValueTask.CompletedTask;}
        }
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task TwentySecondDeadlineCoversHeadersAndBody(bool body) {
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var time=new ManualTime();var key=new Key();
        var h=new Handler{Respond=async(_,stop)=>{if(body)return new(HttpStatusCode.OK){Content=new SlowContent(entered)};entered.SetResult();await Task.Delay(Timeout.Infinite,stop);throw new InvalidOperationException();}};
        using var http=new HttpClient(h);var task=new GeminiAnswers(key,http,clock:time).Answer(Pcm(),default);await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));time.Advance(TimeSpan.FromSeconds(20));
        var error=await Assert.ThrowsAsync<AudioAnswerException>(()=>task.WaitAsync(TimeSpan.FromSeconds(2)));Assert.Equal(AudioAnswerFailure.Timeout,error.Reason);Assert.Equal(1,h.Calls);Assert.All(key.Borrowed!,b=>Assert.Equal(0,b));
    }
    [Fact] public async Task CancellationWhileHttpIsPendingCleansUpAndNeverRetries() {
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var key=new Key();var h=new Handler{Respond=async(_,stop)=>{entered.SetResult();await Task.Delay(Timeout.Infinite,stop);throw new InvalidOperationException();}};
        using var http=new HttpClient(h);using var stop=new CancellationTokenSource();var task=new GeminiAnswers(key,http).Answer(Pcm(),stop.Token);await entered.Task;stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>task);Assert.Equal(1,h.Calls);Assert.All(key.Borrowed!,b=>Assert.Equal(0,b));
    }
    private static Message M(string type,object payload)=>Wire.Decode(Wire.Encode(type,Guid.NewGuid().ToString(),0,payload)[..^1]);
    private static async Task<LivePacket> Next(LiveAuthority authority){using var stop=new CancellationTokenSource(2000);return await authority.Reader("tls").ReadAsync(stop.Token);}
    [Theory][InlineData(200,"audioReply")][InlineData(429,"quota")][InlineData(403,"credential")][InlineData(500,"provider")][InlineData(503,"unavailable")][InlineData(400,"configuration")]
    public async Task ActualAdapterRunsOnlyAfterCurrentConsentFreeTierApprovalAndCompletedTurn(int status,string expected) {
        var key=new Key();var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(status==200?Good:"synthetic private provider error",(HttpStatusCode)status))};using var http=new HttpClient(h);
        var authority=new LiveAuthority(new DeniedIssuer(),audioAnswers:new GeminiAnswers(key,http));Assert.Equal(VoiceProvider.OfflineSimulation,authority.Provider);Assert.Equal(0,h.Calls);
        var consent=Guid.NewGuid().ToString();var turn=Guid.NewGuid().ToString();
        Assert.Throws<ProtocolException>(()=>authority.HandleAudio("untrusted",M("audioBegin",new{authorizationId=Guid.NewGuid().ToString(),turnId=turn,format=AudioProtocol.Format})));
        authority.Open("tls","pinned-phone","device");authority.Handle("tls",M("liveConsent",new{consentId=consent,microphone=true,cloud=true,continuous=false}));
        Assert.Throws<ProtocolException>(()=>authority.AuthorizeAudio("tls",consent,true));authority.SelectProvider(VoiceProvider.GeminiFree);
        // Selecting a provider sends stop; it cannot grant permission or send audio.
        while(authority.Reader("tls").TryRead(out _)){}
        Assert.Throws<ProtocolException>(()=>authority.AuthorizeAudio("tls",consent,false));Assert.Equal(0,key.Reads);Assert.Equal(0,h.Calls);
        var auth=authority.AuthorizeAudio("tls",consent,true);Assert.Equal("audioGrant",(await Next(authority)).Type);Assert.Equal(0,h.Calls);
        authority.HandleAudio("tls",M("audioBegin",new{authorizationId=auth,turnId=turn,format=AudioProtocol.Format}));
        for(int n=0;n<5;n++)authority.HandleAudio("tls",M("audioChunk",new{authorizationId=auth,turnId=turn,index=n,data=Convert.ToBase64String(new byte[960])}));Assert.Equal(0,h.Calls);
        authority.HandleAudio("tls",M("audioEnd",new{authorizationId=auth,turnId=turn,chunks=5,bytes=4800}));var packet=await Next(authority);
        if(status==200){Assert.Equal(expected,packet.Type);Assert.Equal("audioDone",(await Next(authority)).Type);}else{Assert.Equal("liveStop",packet.Type);Assert.Equal(expected,Wire.Text(M(packet.Type,packet.Payload).Payload,"code"));Assert.False(authority.Peers.Single().Authorized);}
        Assert.Equal(1,h.Calls);Assert.Equal(1,key.Reads);
        var outcome=Assert.Single(authority.AudioOutcomes);Assert.Equal(status==200?"answer":expected,outcome.Code);
        Assert.Equal(4800,outcome.InputBytes);Assert.InRange(outcome.ElapsedMs,0,60000);Assert.Equal(status==200?24:0,outcome.Characters);
        Assert.DoesNotContain("friendly",outcome.ToString());Assert.DoesNotContain("synthetic",outcome.ToString());
        authority.HandleAudio("tls",M("audioBegin",new{authorizationId=auth,turnId=Guid.NewGuid().ToString(),format=AudioProtocol.Format}));Assert.Equal("liveStop",(await Next(authority)).Type);Assert.Equal(1,h.Calls);
    }
    [Theory][InlineData("{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\"}]}","budget")]
    [InlineData("{}","empty")][InlineData("{\"promptFeedback\":{\"blockReason\":\"SAFETY\"}}","safety")]
    public async Task EmptyBudgetAndSafetyOutcomesSurviveAuthorityWithoutFabricatedReplies(string body,string code) {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(body))};using var http=new HttpClient(h);
        var a=new LiveAuthority(new DeniedIssuer(),audioAnswers:new GeminiAnswers(new Key(),http));
        a.Open("tls","pin","device");a.SelectProvider(VoiceProvider.GeminiFree);while(a.Reader("tls").TryRead(out _)){}
        var consent=Guid.NewGuid().ToString();var turn=Guid.NewGuid().ToString();
        a.Handle("tls",M("liveConsent",new{consentId=consent,microphone=true,cloud=true,continuous=false}));
        var auth=a.AuthorizeAudio("tls",consent,true);await Next(a);
        a.HandleAudio("tls",M("audioBegin",new{authorizationId=auth,turnId=turn,format=AudioProtocol.Format}));
        for(int n=0;n<5;n++)a.HandleAudio("tls",M("audioChunk",new{authorizationId=auth,turnId=turn,index=n,data=Convert.ToBase64String(new byte[960])}));
        a.HandleAudio("tls",M("audioEnd",new{authorizationId=auth,turnId=turn,chunks=5,bytes=4800}));
        var packet=await Next(a);Assert.Equal("liveStop",packet.Type);Assert.Equal(code,Wire.Text(M(packet.Type,packet.Payload).Payload,"code"));
        Assert.False(a.Reader("tls").TryRead(out _));Assert.False(a.Peers.Single().Authorized);Assert.Equal(1,h.Calls);
    }
    [Fact] public async Task FakeHttpCompletedAnswerMatchesSharedAndroidPresentationTrace() {
        using var fixture=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"gemini-response-trace.json")));var trace=fixture.RootElement;
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json(trace.GetProperty("provider").GetRawText()))};using var http=new HttpClient(h);
        var a=new LiveAuthority(new DeniedIssuer(),audioAnswers:new GeminiAnswers(new Key(),http));a.Open("tls","pin","device");a.SelectProvider(VoiceProvider.GeminiFree);while(a.Reader("tls").TryRead(out _)){}
        var consent=Guid.NewGuid().ToString();var turn=trace.GetProperty("ids")[2].GetString()!;
        a.Handle("tls",M("liveConsent",new{consentId=consent,microphone=true,cloud=true,continuous=false}));var auth=a.AuthorizeAudio("tls",consent,true);await Next(a);
        a.HandleAudio("tls",M("audioBegin",new{authorizationId=auth,turnId=turn,format=AudioProtocol.Format}));
        for(int n=0;n<5;n++)a.HandleAudio("tls",M("audioChunk",new{authorizationId=auth,turnId=turn,index=n,data=Convert.ToBase64String(new byte[960])}));
        a.HandleAudio("tls",M("audioEnd",new{authorizationId=auth,turnId=turn,chunks=5,bytes=4800}));
        foreach(var expected in trace.GetProperty("hostPackets").EnumerateArray()) {
            var packet=await Next(a);Assert.Equal(expected.GetProperty("type").GetString(),packet.Type);var payload=M(packet.Type,packet.Payload).Payload;
            Assert.Equal(auth,Wire.Text(payload,"authorizationId"));Assert.Equal(turn,Wire.Text(payload,"turnId"));
            if(packet.Type=="audioReply")Assert.Equal(expected.GetProperty("text").GetString(),Wire.Text(payload,"text",256));
        }
        Assert.Equal(1,h.Calls);Assert.False(a.Reader("tls").TryRead(out _));Assert.True(a.Peers.Single().Authorized);
        var outcome=Assert.Single(a.AudioOutcomes);Assert.Equal("answer",outcome.Code);Assert.Equal(4800,outcome.InputBytes);Assert.Equal(trace.GetProperty("answer").GetString()!.Length,outcome.Characters);
        Assert.DoesNotContain("synthetic",outcome.ToString());
        // Kotlin's companion test asserts this exact trace is emitted by its real coordinator/controller.
        var hub=new PresentationHub();int captions=0;string? previous=null;
        foreach(var row in trace.GetProperty("presentation").EnumerateArray()) {
            var now=row.GetProperty("at").GetInt64();hub.Apply("tls",M("presentation",row.GetProperty("payload")),now);var caption=hub.Sample(now).Caption;
            if(caption!=null&&caption!=previous)captions++;previous=caption;
        }
        Assert.Equal(1,captions);Assert.Null(hub.Sample(6520).Caption);Assert.Equal(5800,CaptionSession.ReadMs(trace.GetProperty("answer").GetString()!));
    }
    [Fact] public async Task RevocationCancelsRealAdapterFakeHttpAndDiscardsCaption() {
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var canceled=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var key=new Key();var h=new Handler{Respond=async(_,stop)=>{entered.SetResult();try{await Task.Delay(Timeout.Infinite,stop);throw new InvalidOperationException();}finally{canceled.SetResult();}}};using var http=new HttpClient(h);
        var authority=new LiveAuthority(new DeniedIssuer(),audioAnswers:new GeminiAnswers(key,http));authority.Open("tls","pin","device");authority.SelectProvider(VoiceProvider.GeminiFree);
        var consent=Guid.NewGuid().ToString();var turn=Guid.NewGuid().ToString();authority.Handle("tls",M("liveConsent",new{consentId=consent,microphone=true,cloud=true,continuous=false}));
        var auth=authority.AuthorizeAudio("tls",consent,true);await Next(authority);authority.HandleAudio("tls",M("audioBegin",new{authorizationId=auth,turnId=turn,format=AudioProtocol.Format}));
        for(int n=0;n<5;n++)authority.HandleAudio("tls",M("audioChunk",new{authorizationId=auth,turnId=turn,index=n,data=Convert.ToBase64String(new byte[960])}));
        authority.HandleAudio("tls",M("audioEnd",new{authorizationId=auth,turnId=turn,chunks=5,bytes=4800}));await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));authority.Close("tls");await canceled.Task.WaitAsync(TimeSpan.FromSeconds(2));Assert.Empty(authority.Peers);Assert.Equal(1,h.Calls);
    }
    [Fact] public async Task OutcomeMetadataIsBoundedAcrossRepeatedExplicitApprovals() {
        var h=new Handler{Respond=(_,_)=>Task.FromResult(Json("private synthetic provider detail",HttpStatusCode.ServiceUnavailable))};using var http=new HttpClient(h);
        var a=new LiveAuthority(new DeniedIssuer(),audioAnswers:new GeminiAnswers(new Key(),http));a.Open("tls","pin","device");a.SelectProvider(VoiceProvider.GeminiFree);while(a.Reader("tls").TryRead(out _)){}
        var consent=Guid.NewGuid().ToString();a.Handle("tls",M("liveConsent",new{consentId=consent,microphone=true,cloud=true,continuous=false}));
        for(int i=0;i<12;i++) {
            var auth=a.AuthorizeAudio("tls",consent,true);await Next(a);var turn=Guid.NewGuid().ToString();
            a.HandleAudio("tls",M("audioBegin",new{authorizationId=auth,turnId=turn,format=AudioProtocol.Format}));
            for(int n=0;n<5;n++)a.HandleAudio("tls",M("audioChunk",new{authorizationId=auth,turnId=turn,index=n,data=Convert.ToBase64String(new byte[960])}));
            a.HandleAudio("tls",M("audioEnd",new{authorizationId=auth,turnId=turn,chunks=5,bytes=4800}));Assert.Equal("liveStop",(await Next(a)).Type);
        }
        Assert.Equal(12,h.Calls);Assert.Equal(8,a.AudioOutcomes.Length);Assert.Equal(5,a.AudioOutcomes[0].Attempt);Assert.Equal(12,a.AudioOutcomes[^1].Attempt);
        Assert.All(a.AudioOutcomes,x=>{Assert.Equal("unavailable",x.Code);Assert.Equal(0,x.Characters);Assert.DoesNotContain("private",x.ToString());});
        Assert.Contains("503",AudioOutcome.Message("unavailable"));Assert.Contains("no visible text",AudioOutcome.Message("budget"));
    }
    private sealed class UnknownContent:HttpContent {
        private readonly byte[] bytes;public UnknownContent(byte[] bytes){this.bytes=bytes;Headers.ContentType=new("application/json");}
        protected override bool TryComputeLength(out long length){length=0;return false;}
        protected override Task SerializeToStreamAsync(Stream stream,TransportContext? context)=>stream.WriteAsync(bytes).AsTask();
        protected override Task<Stream> CreateContentReadStreamAsync()=>Task.FromResult<Stream>(new MemoryStream(bytes));
    }
    private sealed class SlowContent:HttpContent {
        private readonly TaskCompletionSource entered;public SlowContent(TaskCompletionSource entered){this.entered=entered;Headers.ContentType=new("application/json");}
        protected override bool TryComputeLength(out long length){length=0;return false;}
        protected override Task SerializeToStreamAsync(Stream stream,TransportContext? context)=>throw new InvalidOperationException();
        protected override Task<Stream> CreateContentReadStreamAsync()=>Task.FromResult<Stream>(new SlowStream(entered));
        private sealed class SlowStream(TaskCompletionSource entered):MemoryStream {
            public override async ValueTask<int> ReadAsync(Memory<byte> buffer,CancellationToken stop=default){entered.TrySetResult();await Task.Delay(Timeout.Infinite,stop);return 0;}
        }
    }
}
