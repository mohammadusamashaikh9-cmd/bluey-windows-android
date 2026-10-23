using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
namespace Bluey.Windows.Screen;
// Native contract checks on a public fixture. Never starts a capture session or reads desktop pixels.
internal static class CaptureSmoke
{
    internal static string[] Run()
    {
        var checks=new List<string>();var failures=new List<string>();
        void Probe(string code,Action body){try{body();checks.Add(code);}catch(Exception e){var native=e.GetBaseException();failures.Add(code+" HR=0x"+native.HResult.ToString("X8"));}}
        var fixture=new Window{Title="*Untitled - Notepad — 蓝莓",Width=320,Height=210,Left=40,Top=40};
        try {
            fixture.Show();fixture.UpdateLayout();var hwnd=new WindowInteropHelper(fixture).Handle;
            Probe("UNICODE_WINDOW_TITLE",()=>{
                var text=new StringBuilder(257);Private("GetWindowTextW").Invoke(null,[hwnd,text,257]);
                if(text.ToString()!="*Untitled - Notepad — 蓝莓")throw new InvalidOperationException("Unicode title marshalling failed");
            });
            var task=Task.Run(()=>{
                Probe("WGC_SUPPORT",()=>{if(!NativeCapture.Supported)throw new PlatformNotSupportedException("WGC unavailable on runner");});
                Probe("WGC_WINDOW_ITEM",()=>{var item=(GraphicsCaptureItem)Private("CreateItem").Invoke(null,[hwnd,false])!;if(item.Size.Width<1||item.Size.Height<1)throw new InvalidOperationException("Empty public window item");});
                Probe("WGC_PRIMARY_ITEM",()=>{var monitor=MonitorFromPoint(default,1);var item=(GraphicsCaptureItem)Private("CreateItem").Invoke(null,[monitor,true])!;if(item.Size.Width<1||item.Size.Height<1)throw new InvalidOperationException("Empty primary item");});
                Probe("WGC_D3D_POOL_SESSION",()=>{
                    using var device=(IDirect3DDevice)Private("CreateDevice").Invoke(null,null)!;
                    using var pool=Direct3D11CaptureFramePool.CreateFreeThreaded(device,DirectXPixelFormat.B8G8R8A8UIntNormalized,1,new(){Width=320,Height=210});
                    var item=(GraphicsCaptureItem)Private("CreateItem").Invoke(null,[hwnd,false])!;
                    using var session=pool.CreateCaptureSession(item);session.IsCursorCaptureEnabled=false;
                    // No StartCapture: metadata/interop proof only, not screenshot/first-frame proof.
                });
            });
            var frame=new DispatcherFrame();var dispatcher=Dispatcher.CurrentDispatcher;
            _=task.ContinueWith(_=>dispatcher.BeginInvoke(()=>frame.Continue=false),TaskScheduler.Default);Dispatcher.PushFrame(frame);task.GetAwaiter().GetResult();
        } finally {fixture.Close();}
        if(failures.Count>0)throw new InvalidOperationException("Capture contracts: "+string.Join("; ",failures));
        return checks.ToArray();
    }
    private static MethodInfo Private(string name)=>typeof(NativeCapture).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Static)??throw new InvalidOperationException("Native boundary unavailable");
    [DllImport("user32.dll")]private static extern nint MonitorFromPoint(Native.Point point,uint flags);
}
