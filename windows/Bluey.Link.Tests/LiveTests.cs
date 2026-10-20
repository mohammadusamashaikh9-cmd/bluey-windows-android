using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
namespace Bluey.Link.Tests;
public sealed class LiveTests
{
    private sealed class Issuer: IClientSecretIssuer {
        public int Calls;public TaskCompletionSource<ClientCredential>? Pending;
        public Task<ClientCredential> Mint(string model,LiveLimits limits,CancellationToken stop){Calls++;return Pending?.Task??Task.FromResult(new ClientCredential("offline-test-secret-not-real",1030));}
    }
    private static Message M(string type,object p)=>Wire.Decode(Wire.Encode(type,Guid.NewGuid().ToString(),0,p)[..^1]);
    private static readonly string Consent=Guid.NewGuid().ToString();
    private static void ConsentTo(LiveAuthority a,string lease="tls",bool cloud=true)=>a.Handle(lease,M("liveConsent",new{consentId=Consent,microphone=true,cloud,continuous=false}));
    private static async Task<LivePacket> Next(LiveAuthority a,string lease="tls") {using var c=new CancellationTokenSource(2000);return await a.Reader(lease).ReadAsync(c.Token);}
    private static void Request(LiveAuthority a,string auth,string request,string lease="tls")=>a.Handle(lease,M("liveRequest",new{authorizationId=auth,consentId=Consent,requestId=request}));
    [Fact] public async Task NoIssuanceWithoutCurrentDesktopAuthorizationAndAndroidConsent() {
        var i=new Issuer();var a=new LiveAuthority(i,()=>0,()=>1000);
        Assert.Throws<ProtocolException>(()=>ConsentTo(a));a.Open("tls","pin","device");ConsentTo(a,cloud:false);
        Assert.Throws<ProtocolException>(()=>a.Authorize("tls",Consent,LiveProtocol.Models[0],new()));
        ConsentTo(a);Request(a,Guid.NewGuid().ToString(),Guid.NewGuid().ToString());Assert.Equal("liveStop",(await Next(a)).Type);Assert.Equal(0,i.Calls);
    }
    [Fact] public async Task CurrentGrantMintsBoundedShortLivedMetadataWithoutMasterKey() {
        long time=0;var i=new Issuer();var a=new LiveAuthority(i,()=>time,()=>1000);a.Open("tls","pin","device");ConsentTo(a);
        var auth=a.Authorize("tls",Consent,LiveProtocol.Models[0],new());Assert.Equal("liveGrant",(await Next(a)).Type);
        var id=Guid.NewGuid().ToString();Request(a,auth,id);var token=await Next(a);Assert.Equal("liveToken",token.Type);
        var wire=Wire.Decode(Wire.Encode(token.Type,Guid.NewGuid().ToString(),0,token.Payload)[..^1]);Assert.Equal(id,Wire.Text(wire.Payload,"requestId"));Assert.Equal(1030,Wire.Integer(wire.Payload,"expiresAt"));
        Assert.DoesNotContain("offline-test-secret",token.ToString());Assert.DoesNotContain("offline-test-secret",new ClientCredential("offline-test-secret",1030).ToString());
        Request(a,auth,Guid.NewGuid().ToString());Assert.Equal("liveStop",(await Next(a)).Type);Assert.Equal(1,i.Calls);
    }
    [Fact] public async Task WithdrawalDisconnectConsentReplacementExpiryAndRevocationDiscardInFlightReply() {
        for(int mode=0;mode<4;mode++) {
            long time=0;var i=new Issuer{Pending=new(TaskCreationOptions.RunContinuationsAsynchronously)};var a=new LiveAuthority(i,()=>time,()=>1000);a.Open("tls","pin","device");ConsentTo(a);
            var auth=a.Authorize("tls",Consent,LiveProtocol.Models[0],new());await Next(a);Request(a,auth,Guid.NewGuid().ToString());
            for(int n=0;n<100&&i.Calls==0;n++)await Task.Delay(1);
            if(mode==0)a.StopAll();if(mode==1)a.Close("tls");if(mode==2)ConsentTo(a,cloud:false);if(mode==3){time=300001;a.Expire();}
            i.Pending.SetResult(new("late-offline-test-secret",1030));await Task.Delay(20);
            if(mode!=1)while(a.Reader("tls").TryRead(out var p))Assert.NotEqual("liveToken",p.Type);
        }
    }
    [Fact] public async Task AtMostTwoMintsAndOneAuthorizedPhonePerGrant() {
        long time=0;var i=new Issuer();var a=new LiveAuthority(i,()=>time,()=>1000);a.Open("tls","pin","device");ConsentTo(a);
        var auth=a.Authorize("tls",Consent,LiveProtocol.Models[0],new());await Next(a);Request(a,auth,Guid.NewGuid().ToString());await Next(a);
        time=30000;Request(a,auth,Guid.NewGuid().ToString());await Next(a);time=60000;Request(a,auth,Guid.NewGuid().ToString());Assert.Equal("liveStop",(await Next(a)).Type);Assert.Equal(2,i.Calls);
        a.Authorize("tls",Consent,LiveProtocol.Models[0],new());a.Open("other","pin2","device2");ConsentTo(a,"other");a.Authorize("other",Consent,LiveProtocol.Models[1],new());Assert.Single(a.Peers,p=>p.Authorized);
    }
    private sealed class Key:IMasterKey {public byte[] Read()=>Encoding.UTF8.GetBytes("offline-master-placeholder-never-real");}
    private sealed class Handler:HttpMessageHandler {
        public string? Body;public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken stop){Calls++;Assert.Equal("https://api.openai.com/v1/realtime/client_secrets",request.RequestUri!.AbsoluteUri);Assert.Equal("Bearer",request.Headers.Authorization!.Scheme);Body=await request.Content!.ReadAsStringAsync(stop);
            return new(HttpStatusCode.OK){Content=new StringContent("""{"value":"offline-client-placeholder","expires_at":1030}""")};}
    }
    [Fact] public async Task ActualIssuerSerializesCurrentGaContractUsingOnlyFakeHttp() {
        var h=new Handler();var issuer=new OpenAiIssuer(new Key(),new HttpClient(h),()=>1000);Assert.Equal(0,h.Calls);
        await issuer.Mint(LiveProtocol.Models[0],new(),default);using var d=JsonDocument.Parse(h.Body!);var p=d.RootElement;
        Assert.Equal(30,p.GetProperty("expires_after").GetProperty("seconds").GetInt32());var s=p.GetProperty("session");Assert.Equal("text",s.GetProperty("output_modalities")[0].GetString());
        Assert.Equal(24000,s.GetProperty("audio").GetProperty("input").GetProperty("format").GetProperty("rate").GetInt32());Assert.Equal(JsonValueKind.Null,s.GetProperty("audio").GetProperty("input").GetProperty("turn_detection").ValueKind);
        Assert.Empty(s.GetProperty("tools").EnumerateArray());Assert.DoesNotContain("offline-master",h.Body!);Assert.DoesNotContain("offline-client",h.Body!);
    }
}
