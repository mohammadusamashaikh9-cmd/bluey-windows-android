using Bluey.Screen;
namespace Bluey.Link;
public sealed partial class LiveAuthority
{
    public ScreenSession Screens{get;}=screenSession??new(milliseconds);
    public bool ScreenAvailable=>answers is IScreenAudioAnswers {ScreenContractVerified:true};
    public event Action<PointDirective>? PointRequested;
    public void ApproveScreen(string lease,string reviewedConsent,string snapshotId,bool reviewed)
    {
        lock(gate) {
            if(Provider!=VoiceProvider.GeminiFree||!ScreenAvailable||!sessions.TryGetValue(lease,out var s)||s.Consent!=reviewedConsent||!s.Microphone||!s.Cloud||s.Continuous||s.AudioUsed||s.Issue!=null)throw new ProtocolException("screen approval denied");
            Screens.Approve(snapshotId,lease,reviewedConsent,s.Authorization,reviewed);
        }
    }
    public bool CanPresent(PointDirective p)
    {
        lock(gate)return Screens.IsValid(p)&&sessions.TryGetValue(p.Lease,out var s)&&s.Authorization==p.Auth&&s.AudioId==p.Turn&&s.Microphone&&s.Cloud&&mono()<s.Deadline;
    }
    public void ObservePresentation(string lease,Message message)
    {
        PointDirective? point=null;
        lock(gate) {
            if(message.Type!="presentation"||!sessions.TryGetValue(lease,out var s))return;
            PresentationCodec.Validate(message.Payload);var p=message.Payload;var turn=Wire.Text(p,"turnId");var kind=Wire.Text(p,"event");
            if(kind=="cancel") {Screens.Invalidate();if(s.AudioId==turn)Cancel(s,"stopped",true);return;}
            if(kind=="delta"&&Wire.Text(p,"state")=="speaking"&&s.Authorization!=null&&s.AudioId==turn&&s.Cloud&&s.Microphone&&mono()<s.Deadline)
                point=Screens.TakePoint(lease,turn);
        }
        if(point!=null&&CanPresent(point))PointRequested?.Invoke(point);
    }
}
