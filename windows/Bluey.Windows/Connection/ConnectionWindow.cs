using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using QRCoder;
namespace Bluey.Windows.Connection;
internal sealed class ConnectionWindow : Window
{
    private readonly WindowsLinkHost host;
    private readonly TextBlock status=new(){TextWrapping=TextWrapping.Wrap};
    private readonly TextBlock error=new(){TextWrapping=TextWrapping.Wrap};
    private readonly TextBlock pending=new(){TextWrapping=TextWrapping.Wrap};
    private readonly TextBox invite=new(){IsReadOnly=true,TextWrapping=TextWrapping.Wrap,MinHeight=70};
    private readonly TextBox diagnostics=new(){IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Height=180,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
    private readonly ListBox peers=new(){MinHeight=80};
    private readonly Image qr=new(){Width=260,Height=260,HorizontalAlignment=HorizontalAlignment.Left};
    private readonly ComboBox address=new(){MinWidth=160};
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(500)};
    private bool busy; private string? uri; private Bluey.Link.Enrollment? displayed;
    internal ConnectionWindow(WindowsLinkHost host)
    {
        this.host=host; Title="Bluey — secure local connection"; Width=620; Height=850;
        var panel=new StackPanel{Margin=new(20)}; Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        panel.Children.Add(new TextBlock{Text="Windows ↔ Android",FontSize=24});
        panel.Children.Add(new TextBlock{Text="Same private Wi-Fi/LAN. Allow Bluey through Windows Firewall on private networks only. Never share the pairing QR or URI online.",TextWrapping=TextWrapping.Wrap,Margin=new(0,8,0,8)});
        panel.Children.Add(address);
        Add(panel,"Refresh local addresses",async()=>{var list=await Task.Run(WindowsLinkHost.Addresses);address.ItemsSource=list;address.SelectedIndex=0;});
        Add(panel,"Start secure host",async()=>{var ip=address.SelectedItem as string??"";await Task.Run(()=>host.Start(ip));});
        Add(panel,"Stop secure host",()=>Task.Run(host.Stop));
        panel.Children.Add(status);panel.Children.Add(error);
        panel.Children.Add(new TextBlock{Text="Safe connection diagnostics — UTC/local attempt IDs/client source ports. Review before sharing; never share the QR/URI.",TextWrapping=TextWrapping.Wrap});
        panel.Children.Add(diagnostics);
        Add(panel,"Copy safe diagnostics",()=>{Clipboard.SetText(host.Diagnostics.Text());return Task.CompletedTask;});
        Add(panel,"Start pairing (expires in 5 minutes)",async()=>{uri=await Task.Run(host.StartPairing);invite.Text=uri;qr.Source=Qr(uri);});
        Add(panel,"Stop pairing / clear invitation",async()=>{await Task.Run(()=>host.Server?.StopPairing());ClearInvite();});
        panel.Children.Add(qr);panel.Children.Add(invite);
        panel.Children.Add(new TextBlock{Text="Scan with the phone's system QR scanner, or copy the complete URI privately and paste it in Android Connection settings. Verify the host fingerprint there, then choose Pair. Approve the requesting device below.",TextWrapping=TextWrapping.Wrap});
        panel.Children.Add(pending);
        Add(panel,"Approve requesting device",()=>Decide(true));
        Add(panel,"Reject requesting device",()=>Decide(false));
        panel.Children.Add(new TextBlock{Text="Trusted devices (select to revoke)",Margin=new(0,12,0,3)});panel.Children.Add(peers);
        Add(panel,"Revoke selected device",async()=>{var peer=peers.SelectedItem as DeviceRow;if(peer!=null)await Task.Run(()=>host.Server?.Revoke(peer.Pin));});
        timer.Tick+=(_,_)=>Refresh();timer.Start();
        Closed+=(_,_)=>{timer.Stop();ClearInvite();};
        Loaded+=async(_,_)=>{address.ItemsSource=await Task.Run(WindowsLinkHost.Addresses);address.SelectedIndex=0;Refresh();};
    }
    private Task Decide(bool approved) {
        // Bind the action to the identity/request actually displayed, across refresh/network races.
        var reviewed=displayed;
        return Task.Run(()=>{if(reviewed!=null)host.Server?.Decide(reviewed.Pin,approved,reviewed.RequestId);});
    }
    private sealed record DeviceRow(string Pin,string Label) { public override string ToString()=>Label; }
    private void ClearInvite(){uri=null;invite.Clear();qr.Source=null;}
    private void Refresh()
    {
        if(!busy)status.Text=host.Status;
        var log=host.Diagnostics.Text();if(diagnostics.Text!=log)diagnostics.Text=log;
        var s=host.Server;
        if(s?.Invitation.Active!=true && uri!=null)ClearInvite();
        displayed=s?.Pending;
        pending.Text=displayed is {} p?$"REQUEST: {p.Name}\nClient certificate SHA-256: {p.Pin}\nCompare this fingerprint with Connection on the phone before approving.":"No enrollment awaiting approval.";
        var selected=(peers.SelectedItem as DeviceRow)?.Pin;
        var rows=s?.Peers.Select(p=>new DeviceRow(p.Pin,$"{p.Name} · {p.DeviceId}\n{p.Pin}")).ToArray()??[];
        if(!rows.SequenceEqual(peers.Items.Cast<DeviceRow>())) {peers.ItemsSource=rows;peers.SelectedItem=rows.FirstOrDefault(p=>p.Pin==selected);}
    }
    private void Add(Panel panel,string label,Func<Task> action)
    {
        var button=new Button{Content=label,Padding=new(8),Margin=new(0,4,0,4),HorizontalAlignment=HorizontalAlignment.Left};
        button.Click+=async(_,_)=>{if(busy)return;busy=true;button.IsEnabled=false;error.Text="";try{await action();status.Text=host.Status;}catch(Exception e) when(e is System.IO.IOException or System.Security.Cryptography.CryptographicException or InvalidOperationException or System.Net.Sockets.SocketException or System.Runtime.InteropServices.ExternalException){error.Text="Connection operation failed: "+e.GetType().Name+". Check the selected address and protected storage.";}finally{busy=false;button.IsEnabled=true;}};
        panel.Children.Add(button);
    }
    private static ImageSource Qr(string value)
    {
        using var generator=new QRCodeGenerator();using var data=generator.CreateQrCode(value,QRCodeGenerator.ECCLevel.M);
        var n=data.ModuleMatrix.Count;var drawing=new DrawingGroup();using(var dc=drawing.Open()) {
            dc.DrawRectangle(Brushes.White,null,new Rect(0,0,n,n));
            for(var y=0;y<n;y++)for(var x=0;x<n;x++)if(data.ModuleMatrix[y][x])dc.DrawRectangle(Brushes.Black,null,new Rect(x,y,1,1));
        }
        drawing.Freeze();var result=new DrawingImage(drawing);result.Freeze();return result;
    }
}
