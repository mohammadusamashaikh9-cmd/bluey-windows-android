using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Text;
using System.Security.Cryptography;
using Bluey.Link;
namespace Bluey.Windows.AI;
internal sealed class AiWindow : Window
{
    private readonly PasswordBox input=new(){MaxLength=512};private readonly TextBlock status=new(){TextWrapping=TextWrapping.Wrap};
    private readonly ListBox phones=new(){MinHeight=80};
    private readonly ComboBox provider=new(){ItemsSource=Enum.GetValues<VoiceProvider>(),SelectedIndex=0};
    private readonly ComboBox credential=new(){ItemsSource=new[]{"Gemini","OpenAI (dormant)"},SelectedIndex=0};
    private readonly CheckBox cloud=new(){Content="I authorize Google cloud processing for the selected current phone/consent"};
    private readonly CheckBox free=new(){Content="I checked Google AI Studio: this project is Free Tier and billing is NOT enabled for this feature"};
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(500)};private bool busy;
    internal AiWindow(Connection.WindowsLinkHost host)
    {
        Title="Bluey — provider settings (OFF by default)";Width=680;Height=850;
        var p=new StackPanel{Margin=new(20)};Content=new ScrollViewer{Content=p,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        void Text(string text,int size=14)=>p.Children.Add(new TextBlock{Text=text,FontSize=size,TextWrapping=TextWrapping.Wrap,Margin=new(0,5,0,5)});
        Text("Offline simulation / Gemini free-tier foundation",23);
        Text("Gemini live requests are BLOCKED in this build: official model/audio/free-tier documentation could not be verified in the development environment. No connectivity test or request happens when saving a key. Offline simulation remains available. OpenAI code is retained but disabled; there is no paid fallback.");
        provider.SelectedItem=host.Live.Provider;p.Children.Add(provider);
        provider.SelectionChanged+=(_,_)=>{host.StopLive();cloud.IsChecked=free.IsChecked=false;
            if(provider.SelectedItem is VoiceProvider mode){if(mode==VoiceProvider.OpenAIRealtime){status.Text="OpenAI Realtime is disabled for this free-tier amendment";provider.SelectedItem=VoiceProvider.OfflineSimulation;}else host.Live.SelectProvider(mode);}};
        Text("Credential store (separate Windows CurrentUser DPAPI files)");p.Children.Add(credential);p.Children.Add(input);
        ProtectedApiKey Key()=>credential.SelectedIndex==1?host.ApiKey:host.GeminiKey;
        void Button(string label,Func<Task> action){var b=new Button{Content=label,Margin=new(0,4,0,4),Padding=new(8)};b.Click+=async(_,_)=>{if(busy)return;busy=true;try{await action();}catch(Exception){status.Text="Operation denied or failed safely. No key, audio or provider error logged.";}finally{busy=false;}};p.Children.Add(b);}
        async Task Save(bool replace){var bytes=Encoding.UTF8.GetBytes(input.Password);input.Clear();host.StopLive();cloud.IsChecked=free.IsChecked=false;var key=Key();try{await Task.Run(()=>key.Set(bytes,replace));}finally{CryptographicOperations.ZeroMemory(bytes);}}
        Button("Set selected key (never redisplay)",()=>Save(false));Button("Replace selected key / stop",()=>Save(true));Button("Remove selected key / stop",async()=>{host.StopLive();cloud.IsChecked=free.IsChecked=false;var key=Key();await Task.Run(key.Remove);});
        Text("GeminiFree: hold-only, one response per approval, maximum5 seconds PCM input,128 output tokens planned,20-second request timeout. No automatic retries, reconnect requests, continuous audio, tools, grounding or speech output. Candidate gemini-3.8-flash remains UNVERIFIED; no silent model substitution.");
        Text("Google free-tier content may be used to improve its products under applicable terms. Do not share sensitive conversations. The app cannot guarantee zero cost for an arbitrary key/project. If Free Tier eligibility is uncertain, do not authorize. Quota exhaustion must stop and offer offline simulation; no billing upgrade.");
        Text("Current authenticated phones (select exact device and current consent)");p.Children.Add(phones);p.Children.Add(cloud);p.Children.Add(free);
        Button("Authorize ONE Gemini hold-only question (blocked pending contract verification)",async()=>{
            if(provider.SelectedItem is not VoiceProvider.GeminiFree||cloud.IsChecked!=true||free.IsChecked!=true||!host.Live.GeminiAvailable||phones.SelectedItem is not Row row||row.Peer.ConsentId==null)throw new InvalidOperationException();
            await Task.Run(()=>{var key=host.GeminiKey.Read();CryptographicOperations.ZeroMemory(key);host.Live.AuthorizeAudio(row.Peer.Lease,row.Peer.ConsentId,true);});cloud.IsChecked=free.IsChecked=false;
        });
        Button("STOP / withdraw cloud authorization",()=>{host.StopLive();cloud.IsChecked=free.IsChecked=false;return Task.CompletedTask;});p.Children.Add(status);
        timer.Tick+=async(_,_)=>{if(busy)return;var key=Key();var stored=await Task.Run(()=>key.Status);status.Text=stored+" · "+host.Live.Provider+" · Gemini contract UNVERIFIED / requests blocked";var selected=(phones.SelectedItem as Row)?.Peer.Lease;var rows=host.Live.Peers.Select(x=>new Row(x)).ToArray();if(!rows.SequenceEqual(phones.Items.Cast<Row>())){phones.ItemsSource=rows;phones.SelectedItem=rows.FirstOrDefault(x=>x.Peer.Lease==selected);}};
        timer.Start();Closed+=(_,_)=>{timer.Stop();input.Clear();};
    }
    private sealed record Row(LivePeer Peer){public override string ToString()=>$"Device {Peer.DeviceId} · pin {Peer.Pin}\nMic={Peer.Microphone}; cloud={Peer.Cloud}; continuous={Peer.Continuous}; authorized={Peer.Authorized}";}
}
