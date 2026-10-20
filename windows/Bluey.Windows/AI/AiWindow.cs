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
    private readonly ListBox phones=new(){MinHeight=80};private readonly CheckBox cloud=new(){Content="I reviewed current OpenAI pricing/budgets and authorize billable cloud audio for this selected phone/session"};
    private readonly ComboBox model=new(){ItemsSource=LiveProtocol.Models,SelectedIndex=0};
    private readonly TextBox duration=new(){Text="300"},responses=new(){Text="5"},audio=new(){Text="60"},tokens=new(){Text="256"};
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(500)};private bool busy;
    internal AiWindow(Connection.WindowsLinkHost host)
    {
        Title="Bluey — live AI (OFF until authorized)";Width=650;Height=800;
        var p=new StackPanel{Margin=new(20)};Content=new ScrollViewer{Content=p,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        p.Children.Add(new TextBlock{Text="Live AI — billable, OFF by default",FontSize=23});
        p.Children.Add(new TextBlock{Text="No API request happens when saving a key. The master key stays DPAPI-protected under this Windows user. Microphone/cloud consent on Android and current Windows authorization are both required. No screen capture or tools. Stop ends authorization; restarting does not resume it.",TextWrapping=TextWrapping.Wrap});
        p.Children.Add(input);
        void Button(string label,Func<Task> action){var b=new Button{Content=label,Margin=new(0,4,0,4),Padding=new(8)};b.Click+=async(_,_)=>{if(busy)return;busy=true;try{await action();}catch(Exception){status.Text="Operation failed safely. Check credential/configuration; no provider details logged.";}finally{busy=false;}};p.Children.Add(b);}
        async Task Save(bool replace){var bytes=Encoding.UTF8.GetBytes(input.Password);input.Clear();host.StopLive();cloud.IsChecked=false;try{await Task.Run(()=>host.ApiKey.Set(bytes,replace));}finally{CryptographicOperations.ZeroMemory(bytes);}}
        Button("Set key (never redisplay)",()=>Save(false));Button("Replace key / stop live sessions",()=>Save(true));Button("Remove key / stop live sessions",async()=>{host.StopLive();cloud.IsChecked=false;await Task.Run(host.ApiKey.Remove);});
        p.Children.Add(model);
        foreach(var pair in new[]{("Maximum session seconds (30–600)",duration),("Maximum responses (1–20)",responses),("Total input audio seconds (1–120)",audio),("Maximum response tokens (16–1024)",tokens)}){p.Children.Add(new TextBlock{Text=pair.Item1});p.Children.Add(pair.Item2);}
        p.Children.Add(new TextBlock{Text="Active authenticated phones with their CURRENT Android consent. Select the exact phone; continuous mode is shown explicitly. This is a local safeguard, not a hard financial cap. Configure OpenAI project budgets/usage notifications separately.",TextWrapping=TextWrapping.Wrap});p.Children.Add(phones);p.Children.Add(cloud);
        Button("Authorize selected live session (billable)",async()=>{
            if(cloud.IsChecked!=true||phones.SelectedItem is not Row row||row.Peer.ConsentId==null)throw new InvalidOperationException();
            var limits=new LiveLimits(int.Parse(duration.Text),int.Parse(responses.Text),int.Parse(audio.Text),int.Parse(tokens.Text));var selectedModel=(string)model.SelectedItem;
            await Task.Run(()=>{var key=host.ApiKey.Read();CryptographicOperations.ZeroMemory(key);host.Live.Authorize(row.Peer.Lease,row.Peer.ConsentId,selectedModel,limits);});cloud.IsChecked=false;
        });
        Button("STOP all live sessions / withdraw cloud authorization",()=>{host.StopLive();cloud.IsChecked=false;return Task.CompletedTask;});
        p.Children.Add(status);
        timer.Tick+=async(_,_)=>{if(busy)return;var credential=await Task.Run(()=>host.ApiKey.Status);status.Text=credential+" · "+(host.Live.Peers.Any(x=>x.Authorized)?"LIVE AUTHORIZED":"Live OFF")+" · no key or tokens displayed";var selected=(phones.SelectedItem as Row)?.Peer.Lease;var rows=host.Live.Peers.Select(x=>new Row(x)).ToArray();if(!rows.SequenceEqual(phones.Items.Cast<Row>())){phones.ItemsSource=rows;phones.SelectedItem=rows.FirstOrDefault(x=>x.Peer.Lease==selected);}};
        timer.Start();Closed+=(_,_)=>{timer.Stop();input.Clear();};
    }
    private sealed record Row(LivePeer Peer){public override string ToString()=>$"Device {Peer.DeviceId} · pin {Peer.Pin}\nMic={Peer.Microphone}; cloud={Peer.Cloud}; continuous={Peer.Continuous}; authorized={Peer.Authorized}";}
}
