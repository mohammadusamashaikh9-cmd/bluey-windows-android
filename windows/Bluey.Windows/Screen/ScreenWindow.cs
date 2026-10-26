using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Bluey.Screen;
namespace Bluey.Windows.Screen;
internal sealed class ScreenWindow:Window
{
    private readonly ScreenCoordinator coordinator;private readonly Connection.WindowsLinkHost host;
    private readonly ComboBox choices=new(){MinWidth=300};private readonly ListBox targets=new(){Height=140};private readonly ListBox phones=new(){Height=85};
    private readonly Image preview=new(){MaxHeight=380,Stretch=Stretch.Uniform};private readonly TextBlock status=new(){TextWrapping=TextWrapping.Wrap};
    private readonly TextBox diagnostics=new(){IsReadOnly=true,Height=115,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
    private readonly CheckBox reviewed=new(){Content="I reviewed THIS preview and approve sending its image/visible text to Google for ONE question from the selected current phone."};
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(200)};private string? previewId,operationFailure;private bool busy;private readonly List<Button> mappingButtons=new();
    internal ScreenWindow(ScreenCoordinator coordinator,Connection.WindowsLinkHost host) {
        this.coordinator=coordinator;this.host=host;Title="Bluey — Screen Understanding (OFF by default)";Width=850;Height=940;MinWidth=650;MinHeight=550;OwnPrivacy.Protect(this);
        var panel=new StackPanel{Margin=new(18)};Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        void Text(string s)=>panel.Children.Add(new TextBlock{Text=s,TextWrapping=TextWrapping.Wrap,Margin=new(0,6,0,6)});
        void Button(string text,Func<Task> action,bool needsMapping=false) {var b=new Button{Content=text,Margin=new(0,3,0,3),Padding=new(6)};if(needsMapping)mappingButtons.Add(b);b.Click+=async(_,_)=>{if(busy)return;busy=true;operationFailure=null;try{await action();Refresh();}catch(Exception e){operationFailure=e is CaptureFailure failure?failure.Report.UserMessage+"\n"+failure.Report.SafeText:e is OperationCanceledException?"Capture/analysis canceled or deadline reached. See the last safe capture diagnostic.":e is ScreenException se?"Screen operation stopped: "+se.Code:"Screen operation canceled/failed safely. No private error details logged.";}finally{busy=false;}};panel.Children.Add(b);}
        Text("Screen Understanding · primary display and windows wholly inside it · one snapshot, no background capture");panel.Children.Add(choices);
        Button("Refresh visible window/display choices",()=>{choices.ItemsSource=coordinator.Choices();choices.SelectedIndex=0;return Task.CompletedTask;});
        Button("Capture selected window/display — local only",async()=>{if(choices.SelectedItem is not SurfaceChoice selected)throw new ScreenException("select-surface");preview.Source=null;previewId=null;reviewed.IsChecked=false;await coordinator.Capture(selected,this);});
        // Cancellation must remain usable while a capture/analysis is awaiting native work.
        var stop=new Button{Content="STOP / cancel / discard screenshot (Ctrl+Alt+S)",Padding=new(8),Margin=new(0,4,0,4)};stop.Click+=(_,_)=>{coordinator.Stop();operationFailure=null;ClearPreview();Refresh();};panel.Children.Add(stop);
        panel.Children.Add(preview);
        Text("Review exactly what is visible. Screenshots may expose credentials, financial details or personal messages. Exclusion is best effort, not a privacy guarantee. No automatic redaction/crop is provided: discard and select a harmless smaller window if anything is sensitive. Pixels stay in Windows memory; no screenshot file or Android image transfer.");
        Button("Analyze locally — Windows OCR + visible UIA labels",async()=>{if(coordinator.State.Info?.Surface.Kind==SurfaceKind.Synthetic)throw new ScreenException("synthetic-annotations-not-ocr");await coordinator.Analyze();},true);
        Text("Native OCR requires Windows package identity (Microsoft requirement). This portable build has none; it reports OCR unavailable. Visible read-only UIA targets may still be available. Synthetic annotations below are clearly marked and do not prove OCR.");
        panel.Children.Add(targets);Button("Point Bluey at selected target (never move the real mouse)",()=>{if(targets.SelectedItem is not TargetRow row||previewId==null)throw new ScreenException("select-target");coordinator.Point(previewId,row.Target.Id);return Task.CompletedTask;},true);
        var xy=new WrapPanel();var x=new TextBox{Text="500",Width=90,ToolTip="Normalized X,0..1000"};var y=new TextBox{Text="500",Width=90,ToolTip="Normalized Y,0..1000"};xy.Children.Add(x);xy.Children.Add(y);panel.Children.Add(xy);
        Button("Point at spot X/Y (0–1000 in THIS snapshot)",()=>{if(previewId==null||!double.TryParse(x.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out var gx)||!double.TryParse(y.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out var gy))throw new ScreenException("grid-range");coordinator.PointSpot(previewId,gx,gy);return Task.CompletedTask;},true);
        Button("Stop pointing / Return Home",()=>{coordinator.Home();return Task.CompletedTask;});
        Button("Load SYNTHETIC fixture — not screen capture / not OCR",()=>{coordinator.SyntheticDemo();previewId=null;reviewed.IsChecked=false;return Task.CompletedTask;});
        Text("Optional Gemini screen context — OFF by default. First select GeminiFree and Start current Android hold-only consent. Select the exact phone below; preview/review and approve this snapshot separately. Then use existing Live AI settings to authorize ONE Free Tier question. No call occurs here. Screen context expires after60s and is erased after the attempt/Stop/disconnect. Audio permission/key alone cannot enable screen sharing.");
        Text("Google free-tier content may improve its products. Avoid sensitive conversations/screens. Project Free Tier eligibility is not verified by the app. No paid fallback, second request, tools, grounding or input actions.");
        panel.Children.Add(phones);panel.Children.Add(reviewed);
        Button("Share THIS reviewed snapshot for ONE authorized phone question",()=>{if(previewId==null||reviewed.IsChecked!=true||phones.SelectedItem is not PhoneRow row||row.Peer.ConsentId==null)throw new ScreenException("review-and-select-current-phone");host.Live.ApproveScreen(row.Peer.Lease,row.Peer.ConsentId,previewId,true);reviewed.IsChecked=false;return Task.CompletedTask;},true);
        panel.Children.Add(status);Text("Safe local capture diagnostics — last8 attempts; stage/HRESULT, null frames, content/buffer sizes, frame count, DPI and timing. Select/copy this box when reporting a failure. No titles or pixels.");panel.Children.Add(diagnostics);choices.ItemsSource=coordinator.Choices();choices.SelectedIndex=0;
        timer.Tick+=(_,_)=>Refresh();timer.Start();Closed+=(_,_)=>{timer.Stop();coordinator.Stop();ClearPreview();};Refresh();
    }
    private void ClearPreview(){preview.Source=null;previewId=null;targets.ItemsSource=null;reviewed.IsChecked=false;}
    private void Refresh() {
        diagnostics.Text=coordinator.CaptureDiagnostics;
        var info=coordinator.State.Info;
        foreach(var button in mappingButtons)button.IsEnabled=info?.Surface.MappingVerified==true;reviewed.IsEnabled=info?.Surface.MappingVerified==true;
        if(info==null)ClearPreview();
        else {
            if(previewId!=info.Id){preview.Source=coordinator.Preview();previewId=info.Id;reviewed.IsChecked=false;}
            var selected=(targets.SelectedItem as TargetRow)?.Target.Id;var rows=info.Targets.Select(t=>new TargetRow(t)).ToArray();
            if(!rows.SequenceEqual(targets.Items.Cast<TargetRow>())){targets.ItemsSource=rows;targets.SelectedItem=rows.FirstOrDefault(r=>r.Target.Id==selected);}
        }
        var lease=(phones.SelectedItem as PhoneRow)?.Peer.Lease;var peers=host.Live.Peers.Select(p=>new PhoneRow(p)).ToArray();if(!peers.SequenceEqual(phones.Items.Cast<PhoneRow>())){phones.ItemsSource=peers;phones.SelectedItem=peers.FirstOrDefault(p=>p.Peer.Lease==lease);}
        status.Text=(operationFailure==null?"":operationFailure+"\n")+"Screen capture: "+(coordinator.Busy&&coordinator.Status.StartsWith("Screen capture: Capturing")?"Capturing":info==null?"OFF":"Preview ready")+"\nLast operation: "+coordinator.Status+"\nCloud screen sharing: "+(coordinator.State.CloudApproved?"ONE QUESTION APPROVED":"OFF")+(info==null?"\nSnapshot: none":$"\nSnapshot {info.Id} · age {info.AgeMs/1000}s /60s · {info.Surface.Kind} · {info.Surface.ImageWidth}×{info.Surface.ImageHeight} · {info.Targets.Length} targets · OCR {info.Ocr} · Mapping {info.Surface.Mapping}{(info.Surface.MappingVerified?"":" — PREVIEW ONLY; pointing/cloud disabled")}");
    }
    private sealed record TargetRow(Target Target){public override string ToString()=>$"{Target.Id} [{Target.Role}] {Target.Text} @{Target.NormalizedBounds.Center.X:0},{Target.NormalizedBounds.Center.Y:0}";}
    private sealed record PhoneRow(Bluey.Link.LivePeer Peer){public override string ToString()=>$"Device {Peer.DeviceId} · pin {Peer.Pin} · mic={Peer.Microphone}, cloud={Peer.Cloud}, approved={Peer.Authorized}";}
}
