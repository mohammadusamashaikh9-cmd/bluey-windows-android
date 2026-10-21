using Bluey.Link;
namespace Bluey.Windows;
// UI adapter only. Secure host reducer owns remote captions; no socket work on frames.
internal sealed class VoicePresentation(OverlayArt art, Func<PresentationSnapshot?> read, Bluey.Core.CursorEngine engine)
{
    internal VoicePresentation(OverlayArt art,Connection.WindowsLinkHost host,Bluey.Core.CursorEngine engine):this(art,()=>host.Server?.Presentation,engine){}
    private bool voiceOwned; private double? preview; private bool owned; private string? shown;
    internal void Preview(double now) { preview=now; owned=true; }
    internal void Tick(double now)
    {
        var snapshot=read();
        var remote=snapshot?.Caption;
        var active=snapshot?.State is not (null or "asleep") || preview!=null;
        if(active) { voiceOwned=true; engine.Awake=true;engine.BrainMood=snapshot?.State switch {"thinking"=>Bluey.Core.Mood.Thinking,"asking"=>Bluey.Core.Mood.Listening,"waking"=>Bluey.Core.Mood.Happy,_=>null}; }
        else if(voiceOwned){engine.Awake=false;engine.BrainMood=null;voiceOwned=false;}
        string? caption=remote;
        if(remote!=null)preview=null;
        if(preview is {} start) {
            var age=now-start;
            caption=age<.7?null:age<1?"This is an offline ":age<1.3?"This is an offline Bluey preview. ":"This is an offline Bluey preview. No audio or cloud is active.";
            if(age>1.6+CaptionSession.ReadMs(caption??"")/1000.0){preview=null;caption=null;}
        }
        if(caption!=null){owned=true;engine.TalkUntil=now+.2;engine.TalkLevel=.5+.5*Math.Sin(now*19)*Math.Sin(now*7.3);}
        if(owned&&caption!=shown){art.Caption(caption);shown=caption;}
        if(caption==null&&preview==null)owned=false;
    }
}
