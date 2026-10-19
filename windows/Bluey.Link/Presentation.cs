using System.Text.Json;
namespace Bluey.Link;

public sealed record PresentationSnapshot(string State,string? Caption,long Revision);
public static class PresentationCodec
{
    public static readonly string[] States=["asleep","waking","listening","asking","thinking","speaking"];
    public static void Validate(JsonElement p)
    {
        Wire.Keys(p,"turnId","state","event","text");if(!Wire.Uuid(Wire.Text(p,"turnId")))throw new ProtocolException("presentation identity");
        var state=Wire.Text(p,"state",16);var kind=Wire.Text(p,"event",16);
        if(!States.Contains(state)||!new[]{"state","delta","done","clear","cancel"}.Contains(kind))throw new ProtocolException("presentation enum");
        var e=p.GetProperty("text");if(e.ValueKind!=JsonValueKind.String)throw new ProtocolException("presentation text");
        var text=e.GetString()!;if(text.Length>256||text.Any(char.IsControl))throw new ProtocolException("presentation text");
        for(var i=0;i<text.Length;i++) {if(char.IsHighSurrogate(text[i])){if(++i==text.Length||!char.IsLowSurrogate(text[i]))throw new ProtocolException("unicode");}else if(char.IsLowSurrogate(text[i]))throw new ProtocolException("unicode");}
        if(kind=="delta"&&(state!="speaking"||text.Length==0)||kind!="delta"&&text.Length!=0||kind=="done"&&state!="speaking"||kind=="cancel"&&state!="asleep")throw new ProtocolException("presentation role");
    }
}
// One reducer per authenticated TLS session; its turn/caption never survives disconnect.
public sealed class CaptionSession
{
    private string? turn;private string state="asleep",text="";private bool done;
    private long revision,clearAt=long.MaxValue,last=-1;
    private readonly HashSet<string> retired=[];private readonly Queue<string> recent=[];
    private void Retire(string id){if(retired.Add(id)){recent.Enqueue(id);if(recent.Count>64)retired.Remove(recent.Dequeue());}}
    public static long ReadMs(string text)=> (long)((Math.Clamp(1.2+text.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries).Length*.32,2.8,12)+3)*1000);
    public PresentationSnapshot Sample(long now)=>new(state,now>=clearAt||text.Length==0?null:text,revision);
    public PresentationSnapshot Apply(Message message,long now)
    {
        if(now<last)throw new InvalidOperationException("monotonic clock");last=now;
        PresentationCodec.Validate(message.Payload);var p=message.Payload;
        var id=Wire.Text(p,"turnId");var next=Wire.Text(p,"state");var kind=Wire.Text(p,"event");
        if(kind=="cancel") {
            if(turn!=null&&id!=turn)return Sample(now);
            if(turn!=null)Retire(turn);else Retire(id);
            turn=null;text="";state="asleep";done=true;clearAt=0;revision++;return Sample(now);
        }
        if(retired.Contains(id))throw new ProtocolException("retired turn");
        if(id!=turn) {
            if(kind!="state"||next is not ("waking" or "asking"))throw new ProtocolException("new turn");
            if(turn!=null)Retire(turn);turn=id;text="";done=false;clearAt=long.MaxValue;
            if(next=="asking"&&state is not ("listening" or "speaking"))throw new ProtocolException("ask state");
            if(next=="waking"&&state!="asleep")throw new ProtocolException("wake state");state=next;
        } else if(kind=="state") {
            var allowed=state switch {
                "waking"=>new[]{"listening","asking","thinking","asleep"},
                "listening"=>new[]{"asking","asleep"},"asking"=>new[]{"thinking","listening","asleep"},
                "thinking"=>new[]{"speaking","asleep"},"speaking"=>new[]{"listening","asking","asleep"},_=>Array.Empty<string>()};
            if(!allowed.Contains(next))throw new ProtocolException("state transition");state=next;
        }
        if(kind=="cancel") {text="";state="asleep";done=true;clearAt=0;if(turn!=null)Retire(turn);turn=null;}
        else if(kind=="clear") {if(next!=state)throw new ProtocolException("clear state");text="";clearAt=0;}
        else if(kind=="delta") {
            if(state!="speaking"||done)throw new ProtocolException("caption state");
            var delta=p.GetProperty("text").GetString()!;if(text.Length+delta.Length>2048)throw new ProtocolException("caption size");
            text+=delta;clearAt=now+15000;
        } else if(kind=="done") {
            if(state!="speaking"||done)throw new ProtocolException("caption completion");done=true;clearAt=now+ReadMs(text);
        }
        if(state=="asleep") {text="";clearAt=0;}
        revision++;return Sample(now);
    }
}
public sealed class PresentationHub
{
    private readonly object gate=new();private string? owner;private CaptionSession? reducer;
    private PresentationSnapshot snapshot=new("asleep",null,0);private long lastEvent;
    public void Apply(string lease,Message m,long now) {
        lock(gate) {
            if(owner!=null&&owner!=lease)throw new ProtocolException("presentation busy");
            owner=lease;reducer??=new();snapshot=reducer.Apply(m,now);lastEvent=now;
        }
    }
    public PresentationSnapshot Sample(long now) {
        lock(gate) {
            if(reducer==null||now-lastEvent>30000)return new("asleep",null,snapshot.Revision);
            return reducer.Sample(now);
        }
    }
    public void Release(string lease){lock(gate){if(owner==lease){owner=null;reducer=null;snapshot=new("asleep",null,snapshot.Revision+1);}}}
}
