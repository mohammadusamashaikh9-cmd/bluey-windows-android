using System.Security.Cryptography;
namespace Bluey.Link;
// Shares the current authenticated lease, consent, cancellation and outbox with D011.
public sealed partial class LiveAuthority
{
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
                s.AudioUsed=true;s.AudioId=id;s.Audio=new(id,mono());return;
            }
            if(s.Audio==null||s.AudioId!=id)throw new ProtocolException("audio turn");
            try {
                if(message.Type=="audioChunk"){s.Audio.Append(p,mono());return;}
                if(message.Type!="audioEnd")throw new ProtocolException("audio direction");
                var data=s.Audio.Complete(p,mono());s.Audio=null;
                var issue=new CancellationTokenSource(TimeSpan.FromSeconds(20));s.Issue=issue;s.AudioRequestAt=mono();
                _=AnswerAudio(lease,s,s.Authorization!,id,data,issue);
            } catch {Cancel(s,"denied");throw;}
        }
    }
    private async Task AnswerAudio(string lease,Session s,string auth,string turn,byte[] data,CancellationTokenSource issue)
    {
        await Task.Yield();
        lock(gate) {
            if(!sessions.TryGetValue(lease,out var active)||active!=s||s.Authorization!=auth||s.AudioId!=turn){CryptographicOperations.ZeroMemory(data);issue.Dispose();return;}
            if(!s.Cloud||!s.Microphone||s.Continuous||Provider!=VoiceProvider.GeminiFree||mono()>=s.Deadline||issue.IsCancellationRequested){s.Issue=null;Cancel(s,"expired");CryptographicOperations.ZeroMemory(data);issue.Dispose();return;}
        }
        string? result=null;
        try {result=await answers.Answer(data,issue.Token).WaitAsync(issue.Token);}catch(Exception){/* No provider errors, transcripts or audio in diagnostics. */}
        finally {CryptographicOperations.ZeroMemory(data);}
        lock(gate) {
            if(!sessions.TryGetValue(lease,out var current)||current!=s||s.Authorization!=auth||s.AudioId!=turn){issue.Dispose();return;}
            s.Issue=null;
            if(issue.IsCancellationRequested||mono()>=s.Deadline){Cancel(s,"expired");issue.Dispose();return;}
            // Nonstreamed completed answer. No model text ever becomes a command.
            var text=result==null||result.Length>4096?"":string.Join(" ",result.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries));
            text=new string(text.Where(c=>!char.IsControl(c)&&!char.IsSurrogate(c)).Take(256).ToArray());
            if(text.Length==0)Cancel(s,"issuer");
            else if(!s.Out.Writer.TryWrite(new("audioReply",new{authorizationId=auth,turnId=turn,text}))||!s.Out.Writer.TryWrite(new("audioDone",new{authorizationId=auth,turnId=turn})))Cancel(s,"limit");
            // Keep authority only to deliver/clear this completed reply; AudioUsed refuses a second request.
        }
        issue.Dispose();
    }
}
