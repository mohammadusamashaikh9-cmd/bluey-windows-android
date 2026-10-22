using System.Runtime.InteropServices;
using System.Text;
using Bluey.Screen;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
namespace Bluey.Windows.Screen;
internal sealed record SurfaceChoice(nint Handle,bool Monitor,string Label,Box Physical,Box Primary,double Dpi)
{
    public override string ToString()=>Label;
}
// Supported WGC desktop interop. No polling, disk file, legacy screen BitBlt or input API.
internal sealed class NativeCapture
{
    private int busy;
    internal const long MaxPixels=16*1024*1024;
    internal static bool Supported=>GraphicsCaptureSession.IsSupported();
    internal static bool Exclude(nint hwnd)=>SetWindowDisplayAffinity(hwnd,0x11); // Best effort, not a security guarantee.
    internal static bool HasPackageIdentity {get{uint n=0;return GetCurrentPackageFullName(ref n,null)==122;}}
    internal static SurfaceChoice[] Choices(double primaryDpi)
    {
        var b=Native.PrimaryBounds();var primary=BoxOf(b);var list=new List<SurfaceChoice>();
        list.Add(new(MonitorFromPoint(default,1),true,"Primary display (review preview before sharing)",primary,primary,primaryDpi));
        EnumWindows((hwnd,_)=>{
            if(list.Count>=80)return true;
            try {
                GetWindowThreadProcessId(hwnd,out var pid);if(pid==Environment.ProcessId||!Native.IsWindowVisible(hwnd)||IsIconic(hwnd)||GetWindow(hwnd,4)!=0)return true;
                if(DwmGetWindowAttribute(hwnd,14,out int cloaked,4)!=0||cloaked!=0)return true;
                var name=new StringBuilder(257);GetWindowTextW(hwnd,name,257);if(name.Length==0)return true;
                if(TryBounds(hwnd,out var rect)&&primary.Contains(rect)&&rect.Width>=32&&rect.Height>=32&&rect.Width*rect.Height<=MaxPixels)
                    list.Add(new(hwnd,false,Targets.Clean(name.ToString(),128),rect,primary,primaryDpi));
            } catch { /* Unavailable window is omitted; no title/error logging. */ }
            return true;
        },0);
        return list.ToArray();
    }
    internal static bool Matches(Surface surface)
    {
        var primary=BoxOf(Native.PrimaryBounds());if(primary!=surface.Primary)return false;
        return surface.Kind!=SurfaceKind.Window||TryBounds((nint)surface.NativeId,out var b)&&b==surface.Physical&&Native.IsWindowVisible((nint)surface.NativeId)&&!IsIconic((nint)surface.NativeId)&&DwmGetWindowAttribute((nint)surface.NativeId,14,out int cloaked,4)==0&&cloaked==0;
    }
    internal async Task<Snapshot> Capture(SurfaceChoice choice,CancellationToken stop)
    {
        if(Interlocked.CompareExchange(ref busy,1,0)!=0)throw new ScreenException("capture-busy");
        try {
            if(!Supported||choice.Physical.Width*choice.Physical.Height>MaxPixels)throw new ScreenException("capture-unavailable");
            if(!choice.Monitor){GetWindowThreadProcessId(choice.Handle,out var pid);if(pid==Environment.ProcessId)throw new ScreenException("self-capture-denied");}
            using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(8));using var linked=CancellationTokenSource.CreateLinkedTokenSource(stop,deadline.Token);var cancel=linked.Token;
            var item=CreateItem(choice.Handle,choice.Monitor);var size=item.Size;
            if(size.Width!=(int)choice.Physical.Width||size.Height!=(int)choice.Physical.Height||size.Width<1||size.Height<1||size.Width*(long)size.Height>MaxPixels)throw new ScreenException("capture-geometry");
            using var device=CreateDevice();using var pool=Direct3D11CaptureFramePool.CreateFreeThreaded(device,DirectXPixelFormat.B8G8R8A8UIntNormalized,1,size);
            var session=pool.CreateCaptureSession(item);session.IsCursorCaptureEnabled=false;bool captureActive=true;long capturedAt=0;DateTimeOffset capturedUtc=default;
            var frameResult=new TaskCompletionSource<SoftwareBitmap>(TaskCreationOptions.RunContinuationsAsynchronously);int taken=0;
            async void Frame(Direct3D11CaptureFramePool sender,object args) {
                if(Interlocked.Exchange(ref taken,1)!=0)return;
                try {
                    using var frame=sender.TryGetNextFrame();if(frame==null||frame.ContentSize.Width!=size.Width||frame.ContentSize.Height!=size.Height)throw new ScreenException("capture-changed");
                    capturedAt=Environment.TickCount64;capturedUtc=DateTimeOffset.UtcNow;
                    var bitmap=await SoftwareBitmap.CreateCopyFromSurfaceAsync(frame.Surface,BitmapAlphaMode.Ignore).AsTask(cancel).ConfigureAwait(false);
                    if(!frameResult.TrySetResult(bitmap))bitmap.Dispose();
                } catch {frameResult.TrySetException(new ScreenException("capture-denied-or-unavailable"));}
            }
            void Closed(GraphicsCaptureItem _,object args)=>frameResult.TrySetException(new ScreenException("window-closed"));
            pool.FrameArrived+=Frame;item.Closed+=Closed;
            SoftwareBitmap? image=null;byte[]? jpeg=null;
            try {
                cancel.ThrowIfCancellationRequested();session.StartCapture();
                image=await frameResult.Task.WaitAsync(cancel).ConfigureAwait(false);
                session.Dispose();captureActive=false;pool.FrameArrived-=Frame;
                jpeg=await Encode(image,cancel).ConfigureAwait(false);
                var scale=Math.Min(1,1100.0/Math.Max(image.PixelWidth,image.PixelHeight));
                var surface=new Surface(choice.Monitor?SurfaceKind.PrimaryDisplay:SurfaceKind.Window,(long)choice.Handle,choice.Physical,choice.Primary,choice.Dpi,choice.Dpi,(int)Math.Round(image.PixelWidth*scale),(int)Math.Round(image.PixelHeight*scale));
                if(!Matches(surface)){System.Security.Cryptography.CryptographicOperations.ZeroMemory(jpeg);throw new ScreenException("capture-changed");}
                using(var decoded=await Decode(jpeg,cancel).ConfigureAwait(false)) {
                    EnsureNonblank(decoded);
                }
                cancel.ThrowIfCancellationRequested();var shot=new Snapshot(surface,jpeg,capturedAt,capturedUtc);jpeg=null;return shot;
            } finally {
                pool.FrameArrived-=Frame;item.Closed-=Closed;if(captureActive)session.Dispose();frameResult.TrySetCanceled();
                if(image!=null)image.Dispose();else if(frameResult.Task.IsCompletedSuccessfully)frameResult.Task.Result.Dispose();
                if(jpeg!=null)System.Security.Cryptography.CryptographicOperations.ZeroMemory(jpeg);
            }
        } catch(OperationCanceledException){throw;}
        catch(ScreenException){throw;}
        catch {throw new ScreenException("capture-denied-or-unavailable");}
        finally {Volatile.Write(ref busy,0);}
    }
    internal static async Task<byte[]> Encode(SoftwareBitmap image,CancellationToken stop)
    {
        if(image.PixelWidth<1||image.PixelHeight<1||image.PixelWidth*(long)image.PixelHeight>MaxPixels)throw new ScreenException("image-size");
        using var stream=new InMemoryRandomAccessStream();var encoder=await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId,stream).AsTask(stop);
        var scale=Math.Min(1,1100.0/Math.Max(image.PixelWidth,image.PixelHeight));
        encoder.BitmapTransform.ScaledWidth=(uint)Math.Round(image.PixelWidth*scale);encoder.BitmapTransform.ScaledHeight=(uint)Math.Round(image.PixelHeight*scale);
        encoder.BitmapTransform.InterpolationMode=BitmapInterpolationMode.Fant;encoder.SetSoftwareBitmap(image);
        var properties=new BitmapPropertySet{{"ImageQuality",new BitmapTypedValue(.72f,global::Windows.Foundation.PropertyType.Single)}};
        await encoder.BitmapProperties.SetPropertiesAsync(properties).AsTask(stop);await encoder.FlushAsync().AsTask(stop);
        if(stream.Size is <5 or >Snapshot.MaxJpegBytes)throw new ScreenException("jpeg-size");
        var bytes=new byte[(int)stream.Size];stream.Seek(0);using var reader=new DataReader(stream.GetInputStreamAt(0));await reader.LoadAsync((uint)bytes.Length).AsTask(stop);reader.ReadBytes(bytes);return bytes;
    }
    internal static async Task<SoftwareBitmap> Decode(ReadOnlyMemory<byte> bytes,CancellationToken stop)
    {
        if(bytes.Length is <5 or >Snapshot.MaxJpegBytes)throw new ScreenException("jpeg-size");
        using var stream=new InMemoryRandomAccessStream();using(var writer=new DataWriter(stream.GetOutputStreamAt(0))){var copy=bytes.ToArray();try{writer.WriteBytes(copy);await writer.StoreAsync().AsTask(stop);}finally{System.Security.Cryptography.CryptographicOperations.ZeroMemory(copy);}}
        stream.Seek(0);var decoder=await BitmapDecoder.CreateAsync(stream).AsTask(stop);
        if(decoder.PixelWidth is <1 or >1100||decoder.PixelHeight is <1 or >1100)throw new ScreenException("decoded-size");
        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,BitmapAlphaMode.Ignore).AsTask(stop);
    }
    internal static void EnsureNonblank(SoftwareBitmap decoded)
    {
        if(decoded.PixelWidth>1100||decoded.PixelHeight>1100||decoded.BitmapPixelFormat!=BitmapPixelFormat.Bgra8)throw new ScreenException("decoded-size");
        var bytes=new byte[checked(decoded.PixelWidth*decoded.PixelHeight*4)];
        try {
            var buffer=new global::Windows.Storage.Streams.Buffer((uint)bytes.Length){Length=(uint)bytes.Length};decoded.CopyToBuffer(buffer);
            using var reader=DataReader.FromBuffer(buffer);reader.ReadBytes(bytes);
            var different=false;for(int n=4;n<bytes.Length;n+=4)if(bytes[n]!=bytes[0]||bytes[n+1]!=bytes[1]||bytes[n+2]!=bytes[2]){different=true;break;}
            if(!different)throw new ScreenException("blank-or-protected");
        } finally {System.Security.Cryptography.CryptographicOperations.ZeroMemory(bytes);}
    }
    private static GraphicsCaptureItem CreateItem(nint handle,bool monitor)
    {
        const string name="Windows.Graphics.Capture.GraphicsCaptureItem";nint text=0,factory=0,item=0;
        try {
            Marshal.ThrowExceptionForHR(WindowsCreateString(name,name.Length,out text));var iid=typeof(ICaptureItemInterop).GUID;
            Marshal.ThrowExceptionForHR(RoGetActivationFactory(text,ref iid,out factory));var interop=(ICaptureItemInterop)Marshal.GetObjectForIUnknown(factory);
            var itemId=new Guid("79c3f95b-31f7-4ec2-a464-632ef5d30760");
            Marshal.ThrowExceptionForHR(monitor?interop.CreateForMonitor(handle,ref itemId,out item):interop.CreateForWindow(handle,ref itemId,out item));
            return WinRT.MarshalInspectable<GraphicsCaptureItem>.FromAbi(item);
        } finally {if(item!=0)Marshal.Release(item);if(factory!=0)Marshal.Release(factory);if(text!=0)WindowsDeleteString(text);}
    }
    private static IDirect3DDevice CreateDevice()
    {
        nint device=0,context=0,dxgi=0,inspectable=0;
        try {
            var hr=D3D11CreateDevice(0,1,0,0x20,0,0,7,out device,out _,out context);
            if(hr<0)Marshal.ThrowExceptionForHR(D3D11CreateDevice(0,5,0,0x20,0,0,7,out device,out _,out context)); // WARP, bounded single frame.
            var iid=new Guid("54ec77fa-1377-44e6-8c32-388fd5f44c84");Marshal.ThrowExceptionForHR(Marshal.QueryInterface(device,in iid,out dxgi));
            Marshal.ThrowExceptionForHR(CreateDirect3D11DeviceFromDXGIDevice(dxgi,out inspectable));return WinRT.MarshalInterface<IDirect3DDevice>.FromAbi(inspectable);
        } finally {if(inspectable!=0)Marshal.Release(inspectable);if(dxgi!=0)Marshal.Release(dxgi);if(context!=0)Marshal.Release(context);if(device!=0)Marshal.Release(device);}
    }
    private static Box BoxOf(Native.Rect r)=>new(r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top);
    private static bool TryBounds(nint hwnd,out Box box){var hr=DwmGetWindowAttribute(hwnd,9,out Native.Rect rect,Marshal.SizeOf<Native.Rect>());box=BoxOf(rect);return hr==0&&box.Valid;}
    [ComImport,Guid("3628e81b-3cac-4c60-b7f4-23ce0e0c3356"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ICaptureItemInterop {
        [PreserveSig]int CreateForWindow(nint window,ref Guid iid,out nint item);
        [PreserveSig]int CreateForMonitor(nint monitor,ref Guid iid,out nint item);
    }
    private delegate bool EnumProc(nint hwnd,nint param);
    [DllImport("user32.dll")]private static extern bool EnumWindows(EnumProc proc,nint param);
    [DllImport("user32.dll")]private static extern int GetWindowTextW(nint hwnd,StringBuilder text,int count);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(nint hwnd,out uint pid);
    [DllImport("user32.dll")]private static extern bool IsIconic(nint hwnd);
    [DllImport("user32.dll")]private static extern nint GetWindow(nint hwnd,uint which);
    [DllImport("user32.dll")]private static extern nint MonitorFromPoint(Native.Point point,uint flags);
    [DllImport("user32.dll",SetLastError=true)]private static extern bool SetWindowDisplayAffinity(nint hwnd,uint affinity);
    [DllImport("dwmapi.dll")]private static extern int DwmGetWindowAttribute(nint hwnd,uint attribute,out Native.Rect value,int size);
    [DllImport("dwmapi.dll")]private static extern int DwmGetWindowAttribute(nint hwnd,uint attribute,out int value,int size);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]private static extern int GetCurrentPackageFullName(ref uint length,StringBuilder? name);
    [DllImport("combase.dll",CharSet=CharSet.Unicode)]private static extern int WindowsCreateString(string text,int length,out nint handle);
    [DllImport("combase.dll")]private static extern int WindowsDeleteString(nint handle);
    [DllImport("combase.dll")]private static extern int RoGetActivationFactory(nint name,ref Guid iid,out nint factory);
    [DllImport("d3d11.dll")]private static extern int D3D11CreateDevice(nint adapter,int driver,nint software,uint flags,nint levels,uint count,uint version,out nint device,out uint level,out nint context);
    [DllImport("d3d11.dll")]private static extern int CreateDirect3D11DeviceFromDXGIDevice(nint dxgi,out nint inspectable);
}
