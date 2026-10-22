using System.Globalization;
namespace Bluey.Screen;
public enum OcrStatus { Ready,UnavailableIdentity,UnavailableLanguage,Failed,SyntheticAnnotations }
public sealed record TextWord(string Text,Box Bounds);
public sealed record TextLine(string Text,Box Bounds,TextWord[] Words);
public sealed record Control(string Role,string Label,Box PhysicalBounds,bool Visible,bool Secure);
public sealed record Analysis(TextLine[] Lines,Control[] Controls,bool TrustWordBounds,OcrStatus Ocr,bool ControlsTimedOut=false);
public sealed record Target(string Id,string Text,string Role,Box ImageBounds,Box PhysicalBounds,Box DipBounds,Box NormalizedBounds)
{
    public override string ToString()=>Id+" [label redacted]";
    public VisualPoint Point(Surface surface) {
        var below=new Spot(DipBounds.Center.X,DipBounds.Bottom+3);
        return new(surface.DipCapture.Contains(below)?below:DipBounds.Center,DipBounds);
    }
}
public static class Targets
{
    public static string Clean(string text,int max=256) {
        if(max is <1 or >256)throw new ScreenException("text-bound");
        var output=new System.Text.StringBuilder(Math.Min(max,256));var space=false;
        foreach(var c in text.AsSpan(0,Math.Min(text.Length,4096))) {
            if(char.IsWhiteSpace(c)){space=output.Length>0;continue;}
            if(char.IsControl(c)||char.IsSurrogate(c)||char.GetUnicodeCategory(c)==UnicodeCategory.Format)continue;
            if(space&&output.Length<max)output.Append(' ');space=false;if(output.Length>=max)break;output.Append(c);
        }
        return output.ToString().Trim();
    }
    public static Target[] Extract(Surface surface,Analysis analysis) {
        var found=new List<Target>(1140);int l=0,w=0,c=0;
        void Add(string id,string text,string role,Box box)=>found.Add(new(id,text,role,box,surface.ImageToPhysical(box),surface.ImageToDip(box),surface.Normalized(box)));
        foreach(var line in analysis.Lines.Take(400).Where(t=>surface.Image.Contains(t.Bounds)).OrderBy(t=>t.Bounds.Y).ThenBy(t=>t.Bounds.X)) {
            var label=Clean(line.Text);if(label.Length==0)continue;Add("L"+(++l),label,"line",line.Bounds);
            if(!analysis.TrustWordBounds)continue;
            foreach(var word in line.Words.Take(600-w)) {
                var text=Clean(word.Text,96);if(text.Length==0||!line.Bounds.Contains(word.Bounds))continue;
                Add("W"+(++w),text,"word",word.Bounds);
            }
        }
        foreach(var control in analysis.Controls.Take(5000).Where(t=>t.Visible&&!t.Secure&&surface.Physical.Contains(t.PhysicalBounds)).OrderBy(t=>t.PhysicalBounds.Y).ThenBy(t=>t.PhysicalBounds.X).Take(140)) {
            var text=Clean(control.Label,128);var role=Clean(control.Role,32);if(text.Length==0||role.Length==0)continue;Add("C"+(++c),text,role,surface.PhysicalToImage(control.PhysicalBounds));
        }
        return found.ToArray();
    }
}
public sealed record VisualPoint(Spot Position,Box Showing);
// Source RealtimeHost: 1s flight allowance +2.2s hold, then home after >5s quiet.
public sealed class Pointing
{
    private readonly Queue<VisualPoint> queue=new();private long holdUntil,quietSince;private bool active,speaking;
    public void Enqueue(VisualPoint point,long now){if(queue.Count==4)throw new ScreenException("point-queue");queue.Enqueue(point);if(!active)quietSince=now;active=true;}
    public (VisualPoint? Point,bool Home) Tick(long now,bool talking) {
        if(speaking&&!talking)quietSince=now;speaking=talking;
        if(!active)return(null,false);
        if(now>=holdUntil&&queue.Count>0){var p=queue.Dequeue();holdUntil=now+3200;if(!speaking)quietSince=now;return(p,false);}
        if(queue.Count==0&&now>=holdUntil&&!speaking&&now-quietSince>5000)return Stop();
        return(null,false);
    }
    public (VisualPoint? Point,bool Home) Stop(){queue.Clear();holdUntil=0;speaking=false;var home=active;active=false;return(null,home);}
}
