using System.Threading.Channels;
namespace Bluey.Link;
public sealed record LivePeer(string Lease,string Pin,string DeviceId,string? ConsentId,bool Microphone,bool Cloud,bool Continuous,bool Authorized);
// Only ready, trusted TLS sessions are registered. HTTP work never blocks LAN reads/rendering.
public sealed partial class LiveAuthority(IClientSecretIssuer issuer,Func<long>? milliseconds=null,Func<long>? unixSeconds=null,IAudioAnswers? audioAnswers=null,bool openAiEnabled=false,Bluey.Screen.ScreenSession? screenSession=null)
{
    private sealed class Session(string pin,string device){public long AudioRequestAt;public AudioTurn? Audio;public string? AudioId;public string? AudioStopCode;public bool AudioUsed;public string Pin=pin,Device=device;public string? Consent,Authorization;public bool Microphone,Cloud,Continuous;public long Deadline,LastMint=-30000;public int Mints;public string Model=LiveProtocol.Models[0];public LiveLimits Limits=new();public CancellationTokenSource? Issue;public HashSet<string> Requests=[];public Channel<LivePacket> Out=Channel.CreateBounded<LivePacket>(8);}
    private readonly IAudioAnswers answers=audioAnswers??new UnavailableAudioAnswers();
    public VoiceProvider Provider {get;private set;}=VoiceProvider.OfflineSimulation;
    public bool GeminiAvailable=>answers.ContractVerified;
    public void SelectProvider(VoiceProvider selected){lock(gate){Screens.Invalidate();if(!Enum.IsDefined(selected))throw new ProtocolException("provider");if(selected==VoiceProvider.OpenAIRealtime&&!openAiEnabled)throw new ProtocolException("OpenAI disabled");foreach(var s in sessions.Values)Cancel(s,"stopped",true);Provider=selected;}}
    private readonly object gate=new();private readonly Dictionary<string,Session> sessions=[];
    private readonly Func<long> mono=milliseconds??(()=>Environment.TickCount64),unix=unixSeconds??(()=>DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    public LivePeer[] Peers {get{lock(gate)return sessions.Select(k=>new LivePeer(k.Key,k.Value.Pin,k.Value.Device,k.Value.Consent,k.Value.Microphone,k.Value.Cloud,k.Value.Continuous,k.Value.Authorization!=null)).ToArray();}}
    public void Open(string lease,string pin,string device){lock(gate){if(sessions.Count>=4)throw new ProtocolException("live slots");sessions.Add(lease,new(pin,device));}}
    public ChannelReader<LivePacket> Reader(string lease){lock(gate)return sessions.TryGetValue(lease,out var s)?s.Out.Reader:throw new ProtocolException("live closed");}
    private void Cancel(Session s,string code,bool force=false){if(force||s.Authorization!=null||code!="stopped")Screens.Invalidate();var notify=force||s.Authorization!=null||code!="stopped";if(s.Issue!=null)s.AudioStopCode=code;s.Authorization=null;s.Audio?.Dispose();s.Audio=null;s.AudioId=null;s.AudioUsed=false;s.Issue?.Cancel();s.Issue=null;while(s.Out.Reader.TryRead(out _)){}if(notify&&!s.Out.Writer.TryWrite(new("liveStop",new{code})))s.Out.Writer.TryComplete();}
    public void Close(string lease){lock(gate){Screens.Invalidate();if(sessions.Remove(lease,out var s)){Cancel(s,"disconnected");s.Out.Writer.TryComplete();}}}
    public void StopAll(){lock(gate){Screens.Invalidate();foreach(var s in sessions.Values)Cancel(s,"stopped",true);}}
    public string Authorize(string lease,string reviewedConsent,string model,LiveLimits limits)
    {
        if(!openAiEnabled)throw new ProtocolException("OpenAI disabled");
        limits.Validate();if(!LiveProtocol.Models.Contains(model))throw new ProtocolException("model");
        lock(gate){var s=sessions[lease];if(s.Consent!=reviewedConsent||!s.Microphone||!s.Cloud)throw new ProtocolException("consent changed");
            foreach(var other in sessions.Values)Cancel(other,"stopped");
            Provider=VoiceProvider.OpenAIRealtime;s.Authorization=Guid.NewGuid().ToString();s.Deadline=mono()+limits.SessionSeconds*1000L;s.Limits=limits;s.Model=model;s.Mints=0;s.LastMint=-30000;s.Requests.Clear();
            s.Out.Writer.TryWrite(new("liveGrant",new{authorizationId=s.Authorization,consentId=s.Consent}));return s.Authorization;}
    }
    public void Handle(string lease,Message m)
    {
        lock(gate){if(!sessions.TryGetValue(lease,out var s))throw new ProtocolException("live closed");LiveProtocol.Validate(m.Type,m.Payload);var p=m.Payload;
            if(m.Type=="liveConsent"){if(s.Consent!=null||!LiveProtocol.Flag(p,"microphone")||!LiveProtocol.Flag(p,"cloud"))Screens.Invalidate();Cancel(s,"stopped");s.Consent=Wire.Text(p,"consentId");s.Microphone=LiveProtocol.Flag(p,"microphone");s.Cloud=LiveProtocol.Flag(p,"cloud");s.Continuous=LiveProtocol.Flag(p,"continuous");return;}
            if(m.Type=="liveStop"){Screens.Invalidate();Cancel(s,"stopped");s.Consent=null;s.Microphone=s.Cloud=false;return;}
            if(Provider!=VoiceProvider.OpenAIRealtime){Cancel(s,"denied");return;}
            if(m.Type!="liveRequest")throw new ProtocolException("live direction");
            var request=Wire.Text(p,"requestId");var auth=Wire.Text(p,"authorizationId");
            if(s.Authorization!=auth||s.Consent!=Wire.Text(p,"consentId")||!s.Cloud||!s.Microphone||mono()>=s.Deadline){Cancel(s,"denied");return;}
            if(s.Issue!=null||s.Mints>=2||mono()-s.LastMint<30000||!s.Requests.Add(request)){Cancel(s,"limit");return;}
            s.Mints++;s.LastMint=mono();var issue=new CancellationTokenSource(TimeSpan.FromSeconds(10));s.Issue=issue;
            _=Issue(lease,s,auth,request,issue);
        }
    }
    private async Task Issue(string lease,Session s,string auth,string request,CancellationTokenSource issue)
    {
        // Yield before provider code: Handle holds only a short state lock.
        await Task.Yield();ClientCredential? result=null;try{result=await issuer.Mint(s.Model,s.Limits,issue.Token);}catch(Exception){/* Never expose provider bodies, keys, tokens or exception text. */}
        lock(gate){if(!sessions.TryGetValue(lease,out var current)||current!=s||s.Authorization!=auth||issue.IsCancellationRequested){issue.Dispose();return;}
            s.Issue=null;
            if(result==null||result.ExpiresAt<=unix()||result.ExpiresAt>unix()+60||mono()>=s.Deadline){Cancel(s,"issuer");}
            else {
                var remaining=(int)Math.Min(s.Limits.SessionSeconds,(s.Deadline-mono())/1000);
                if(remaining<1)Cancel(s,"expired");
                else if(!s.Out.Writer.TryWrite(new("liveToken",new{authorizationId=auth,requestId=request,secret=result.Value,expiresAt=result.ExpiresAt,model=s.Model,sessionSeconds=remaining,responses=s.Limits.Responses,audioSeconds=s.Limits.AudioSeconds,maxTokens=s.Limits.MaxTokens})))Cancel(s,"limit");
            }
        }issue.Dispose();
    }
    public void Expire(){Screens.Expire();lock(gate)foreach(var s in sessions.Values)if(s.Authorization!=null&&(mono()>=s.Deadline||s.Audio?.Expired(mono())==true||Provider==VoiceProvider.GeminiFree&&s.Issue!=null&&mono()-s.AudioRequestAt>=20000))Cancel(s,Provider==VoiceProvider.GeminiFree&&s.Issue!=null&&mono()<s.Deadline?"timeout":"expired");}
}
