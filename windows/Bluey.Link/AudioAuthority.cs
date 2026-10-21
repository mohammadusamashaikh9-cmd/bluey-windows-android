using System.Security.Cryptography;
namespace Bluey.Link;
// Shares the current authenticated lease, consent, cancellation and outbox with D011.
public sealed partial class LiveAuthority
{
    private long audioAttempt;
    private readonly Queue<AudioOutcome> audioOutcomes=new();
    public AudioOutcome[] AudioOutcomes { get {lock(gate)return audioOutcomes.ToArray();} }
    private void RecordAudio(long attempt,string code,long started,int bytes,int characters=0) {
        if(audioOutcomes.Count==8)audioOutcomes.Dequeue();
        audioOutcomes.Enqueue(new(attempt,code,Math.Clamp(mono()-started,0,60000),bytes,characters));
    }
    private static string FailureCode(AudioAnswerFailure? reason)=>reason switch {
        AudioAnswerFailure.Unavailable=>"unavailable",AudioAnswerFailure.Configuration=>"configuration",
        AudioAnswerFailure.Quota=>"quota",AudioAnswerFailure.Credential=>"credential",
        AudioAnswerFailure.Timeout=>"timeout",AudioAnswerFailure.Network=>"network",
        AudioAnswerFailure.Empty=>"empty",AudioAnswerFailure.Budget=>"budget",
        AudioAnswerFailure.Blocked=>"safety",AudioAnswerFailure.Malformed=>"malformed",_=>"provider"
    };
    public string AuthorizeAudio(string lease,string reviewedConsent,bool freeTierConfirmed)
    {
        lock(gate) {
            if(Provider!=VoiceProvider.GeminiFree||!answers.ContractVerified||!freeTierConfirmed)throw new ProtocolException("Gemini unavailable or Free Tier unconfirmed");
            if(!sessions.TryGetValue(lease,out var s)||s.Consent!=reviewedConsent||!s.Microphone||!s.Cloud||s.Continuous)throw new ProtocolException("hold-only consent required");
            foreach(var other in sessions.Values)Cancel(other,"stopped");
            s.Authorization=Guid.NewGuid().ToString();s.Deadline=mono()+60000;s.AudioUsed=false;
            if(!s.Out.Writer.TryWrite(new("audioGrant",new{authorizationId=s.Authorization,consentId=s.Consent,provider="GeminiFree",sessionSeconds=60})))Cancel(s,"limit");return s.Authorization!;
        }
    }
    public void HandleAudio(string lease,Message message)
    {
        lock(gate) {
            if(!sessions.TryGetValue(lease,out var s))throw new ProtocolException("audio closed");
            AudioProtocol.Validate(message.Type,message.Payload);var p=message.Payload;
            if(Provider!=VoiceProvider.GeminiFree||!answers.ContractVerified||s.Authorization!=Wire.Text(p,"authorizationId")||!s.Cloud||!s.Microphone||s.Continuous||mono()>=s.Deadline){Cancel(s,"denied");return;}
            var id=Wire.Text(p,"turnId");
            if(message.Type=="audioCancel") {
                if(s.AudioId!=id)throw new ProtocolException("audio stale cancel");Cancel(s,"stopped",true);return;
            }
            if(message.Type=="audioBegin") {
                if(s.AudioUsed||s.Audio!=null||s.Issue!=null){Cancel(s,"limit");return;}
                s.AudioUsed=true;s.AudioStopCode=null;s.AudioId=id;s.Audio=new(id,mono());return;
            }
            if(s.Audio==null||s.AudioId!=id)throw new ProtocolException("audio turn");
            try {
                if(message.Type=="audioChunk"){s.Audio.Append(p,mono());return;}
                if(message.Type!="audioEnd")throw new ProtocolException("audio direction");
                var data=s.Audio.Complete(p,mono());s.Audio=null;
                var issue=new CancellationTokenSource(TimeSpan.FromSeconds(20));s.Issue=issue;s.AudioRequestAt=mono();
                _=AnswerAudio(lease,s,s.Authorization!,id,data,issue,++audioAttempt,s.AudioRequestAt);
            } catch {Cancel(s,"denied");throw;}
        }
    }
    private async Task AnswerAudio(string lease,Session s,string auth,string turn,byte[] data,CancellationTokenSource issue,long attempt,long started)
    {
        await Task.Yield();
        lock(gate) {
            if(!sessions.TryGetValue(lease,out var active)||active!=s||s.Authorization!=auth||s.AudioId!=turn){CryptographicOperations.ZeroMemory(data);issue.Dispose();return;}
            if(!s.Cloud||!s.Microphone||s.Continuous||Provider!=VoiceProvider.GeminiFree||mono()>=s.Deadline||issue.IsCancellationRequested){s.Issue=null;Cancel(s,"expired");CryptographicOperations.ZeroMemory(data);issue.Dispose();return;}
        }
        string? result=null;AudioAnswerFailure? failure=null;
        try {result=await answers.Answer(data,issue.Token).WaitAsync(issue.Token);}catch(AudioAnswerException e){failure=e.Reason;}
        catch(Exception){/* No provider errors, transcripts or audio in diagnostics. */}
        finally {CryptographicOperations.ZeroMemory(data);}
        lock(gate) {
            if(!sessions.TryGetValue(lease,out var current)||current!=s||s.Authorization!=auth||s.AudioId!=turn){RecordAudio(attempt,s.AudioStopCode=="timeout"?"timeout":"canceled",started,data.Length);issue.Dispose();return;}
            s.Issue=null;
            if(issue.IsCancellationRequested||mono()>=s.Deadline){var code=mono()>=s.Deadline?"expired":"timeout";RecordAudio(attempt,code,started,data.Length);Cancel(s,code);issue.Dispose();return;}
            // Nonstreamed completed answer. No model text ever becomes a command.
            var text=result==null||result.Length>4096?"":string.Join(" ",result.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries));
            text=new string(text.Where(c=>!char.IsControl(c)&&!char.IsSurrogate(c)).Take(256).ToArray());
            if(text.Length==0){var code=FailureCode(failure);RecordAudio(attempt,code,started,data.Length);Cancel(s,code);}
            else if(!s.Out.Writer.TryWrite(new("audioReply",new{authorizationId=auth,turnId=turn,text}))||!s.Out.Writer.TryWrite(new("audioDone",new{authorizationId=auth,turnId=turn})))Cancel(s,"limit");
            else RecordAudio(attempt,"answer",started,data.Length,text.Length);
            // Keep authority only to deliver/clear this completed reply; AudioUsed refuses a second request.
        }
        issue.Dispose();
    }
}
