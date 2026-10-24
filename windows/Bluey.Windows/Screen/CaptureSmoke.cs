using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Bluey.Screen;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
namespace Bluey.Windows.Screen;
// Native contract checks on a public fixture. Never starts a capture session or reads desktop pixels.
internal static class CaptureSmoke
{
    internal sealed record Evidence(string[] Checks,CaptureDimensions? Geometry);
    internal static Evidence Run()
    {
        CaptureDimensions? geometryEvidence=null;var checks=new List<string>();var failures=new List<string>();
        void Probe(string code,Action body){try{body();checks.Add(code);}catch(Exception e){var native=e.GetBaseException();failures.Add(code+" HR=0x"+native.HResult.ToString("X8"));}}
        var fixture=new Window{Title="*Untitled - Notepad — 蓝莓",Width=320,Height=210,Left=40,Top=40};
        try {
            fixture.Show();fixture.UpdateLayout();var hwnd=new WindowInteropHelper(fixture).Handle;
            Probe("UNICODE_WINDOW_TITLE",()=>{
                var text=new StringBuilder(257);Private("GetWindowTextW").Invoke(null,[hwnd,text,257]);
                if(text.ToString()!="*Untitled - Notepad — 蓝莓")throw new InvalidOperationException("Unicode title marshalling failed");
                fixture.Title="*Untitled - Notepad";text.Clear();Private("GetWindowTextW").Invoke(null,[hwnd,text,257]);
                if(text.ToString()!="*Untitled - Notepad")throw new InvalidOperationException("Notepad-style title marshalling failed");
            });
            var task=Task.Run(()=>{
                Probe("WGC_SUPPORT",()=>{if(!NativeCapture.Supported)throw new PlatformNotSupportedException("WGC unavailable on runner");});
                Probe("WGC_WINDOW_ITEM",()=>{var item=(GraphicsCaptureItem)Private("CreateItem").Invoke(null,[hwnd,false])!;if(item.Size.Width<1||item.Size.Height<1)throw new InvalidOperationException("Empty public window item");});
                Probe("WGC_WINDOW_GEOMETRY",()=>{
                    var item=(GraphicsCaptureItem)Private("CreateItem").Invoke(null,[hwnd,false])!;object[] args=[hwnd,default(Box)];
                    if(!(bool)Private("TryBounds").Invoke(null,args)!||!NativeCapture.TryWindowBounds(hwnd,out var raw))throw new InvalidOperationException("Measured geometry unavailable");
                    var visible=(Box)args[1];var r=Native.PrimaryBounds();var primary=new Box(r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top);
                    var plan=CaptureGeometry.Resolve(SurfaceKind.Window,visible,primary,raw,item.Size.Width,item.Size.Height,Native.GetDpiForWindow(hwnd));
                    if(plan.Width!=item.Size.Width||plan.Height!=item.Size.Height||!plan.Matches(visible,raw,item.Size.Width,item.Size.Height))throw new InvalidOperationException("Invalid capture plan");
                    geometryEvidence=new((int)visible.Width,(int)visible.Height,plan.Width,plan.Height,(int)raw.Width,(int)raw.Height,plan.Dpi,plan.Mapping);
                });
                Probe("PUBLIC_LARGE_JPEG_GEOMETRY",()=>{
                    var pixels=new byte[1920*1080*4];for(int i=0;i<pixels.Length;i+=4){pixels[i]=255;pixels[i+3]=255;}
                    using var writer=new global::Windows.Storage.Streams.DataWriter();writer.WriteBytes(pixels);
                    using var bitmap=new global::Windows.Graphics.Imaging.SoftwareBitmap(global::Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,1920,1080,global::Windows.Graphics.Imaging.BitmapAlphaMode.Ignore);bitmap.CopyFromBuffer(writer.DetachBuffer());
                    var bytes=NativeCapture.Encode(bitmap,default).GetAwaiter().GetResult();try {using var decoded=NativeCapture.Decode(bytes,default).GetAwaiter().GetResult();if(decoded.PixelWidth!=1100||decoded.PixelHeight!=619)throw new InvalidOperationException("JPEG scale differs from validated transform");}finally{System.Security.Cryptography.CryptographicOperations.ZeroMemory(bytes);System.Security.Cryptography.CryptographicOperations.ZeroMemory(pixels);}
                });
                Probe("WGC_PRIMARY_ITEM",()=>{var monitor=MonitorFromPoint(default,1);var item=(GraphicsCaptureItem)Private("CreateItem").Invoke(null,[monitor,true])!;if(item.Size.Width<1||item.Size.Height<1)throw new InvalidOperationException("Empty primary item");});
                Probe("INVALID_WINDOW_STAGE_AND_HRESULT",()=>{
                    var native=new NativeCapture();var primary=Native.PrimaryBounds();
                    var bounds=new Box(primary.Left,primary.Top,primary.Right-primary.Left,primary.Bottom-primary.Top);
                    try {native.Capture(new((nint)(-1),false,"PRIVATE_TITLE_MUST_NOT_APPEAR",new(bounds.X,bounds.Y,320,210),bounds,96),default).GetAwaiter().GetResult();throw new InvalidOperationException("Invalid handle must fail");}
                    catch(CaptureFailure e) {
                        if(e.Report.Stage!=CaptureStage.CreateWindowItem||e.Report.NativeHresult==null||native.Diagnostics.Text.Contains("PRIVATE"))throw new InvalidOperationException("Stage or HRESULT privacy failed");
                    }
                });
                Probe("SELF_WINDOW_FILTER",()=>{if(NativeCapture.Choices(96).Any(c=>c.Handle==hwnd))throw new InvalidOperationException("Self window must not be selectable");});
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
        return new(checks.ToArray(),geometryEvidence);
    }
    private static MethodInfo Private(string name)=>typeof(NativeCapture).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Static)??throw new InvalidOperationException("Native boundary unavailable");
    [DllImport("user32.dll")]private static extern nint MonitorFromPoint(Native.Point point,uint flags);
}
