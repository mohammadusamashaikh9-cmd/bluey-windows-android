using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
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
    internal CaptureDiagnostics Diagnostics{get;}=new();
    internal const long MaxPixels=CaptureGeometry.MaxPixels;
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
        return surface.Kind!=SurfaceKind.Window||TryBounds((nint)surface.NativeId,out var b)&&b==surface.SelectionBounds&&(surface.WindowFrameBounds is not {} frame||TryWindowBounds((nint)surface.NativeId,out var now)&&now==frame)&&Native.IsWindowVisible((nint)surface.NativeId)&&!IsIconic((nint)surface.NativeId)&&DwmGetWindowAttribute((nint)surface.NativeId,14,out int cloaked,4)==0&&cloaked==0;
    }
    internal async Task<Snapshot> Capture(SurfaceChoice choice,CancellationToken stop)
    {
        if(Interlocked.CompareExchange(ref busy,1,0)!=0)throw new ScreenException("capture-busy");
        var started=Environment.TickCount64;var kind=choice.Monitor?SurfaceKind.PrimaryDisplay:SurfaceKind.Window;
        int stage=(int)CaptureStage.Selection;bool succeeded=false;CaptureDimensions? dimensions=null;CaptureFrames? acquisition=null;
        void At(CaptureStage value)=>Volatile.Write(ref stage,(int)value);
        CaptureFailure Fail(CaptureOutcome outcome,Exception? cause=null,CaptureStage? at=null) {
            succeeded=false;return new(Diagnostics.Record(kind,at??(CaptureStage)Volatile.Read(ref stage),outcome,Environment.TickCount64-started,cause,dimensions,acquisition?.Trace));
        }
        using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(8));
        try {
            if(choice.Physical.Width*choice.Physical.Height>MaxPixels)throw new ScreenException("image-size");
            if(!choice.Monitor){GetWindowThreadProcessId(choice.Handle,out var pid);if(pid==Environment.ProcessId)throw new ScreenException("self-capture-denied");}
            At(CaptureStage.Support);if(!Supported)throw Fail(CaptureOutcome.Unsupported);
            using var linked=CancellationTokenSource.CreateLinkedTokenSource(stop,deadline.Token);var cancel=linked.Token;
            At(choice.Monitor?CaptureStage.CreateMonitorItem:CaptureStage.CreateWindowItem);
            var item=CreateItem(choice.Handle,choice.Monitor);At(CaptureStage.ItemSize);var size=item.Size;
            Box? windowFrame=null;
            if(!choice.Monitor&&TryWindowBounds(choice.Handle,out var measured))windowFrame=measured;
            dimensions=new((int)choice.Physical.Width,(int)choice.Physical.Height,size.Width,size.Height,windowFrame is {} f?(int)f.Width:null,windowFrame is {} g?(int)g.Height:null,choice.Dpi,null);
            if(!choice.Monitor&&(!TryBounds(choice.Handle,out var visible)||visible!=choice.Physical))throw new ScreenException("capture-changed");
            var geometry=CaptureGeometry.Resolve(kind,choice.Physical,choice.Primary,windowFrame,size.Width,size.Height,choice.Dpi);
            dimensions=dimensions with {Mapping=geometry.Mapping};
            acquisition=new(geometry);var selectedSurface=geometry.ToSurface((long)choice.Handle);
            At(CaptureStage.D3DDevice);using var device=CreateDevice();
            At(CaptureStage.FramePool);using var pool=Direct3D11CaptureFramePool.CreateFreeThreaded(device,DirectXPixelFormat.B8G8R8A8UIntNormalized,1,size);
            bool captureActive=true;long capturedAt=0;DateTimeOffset capturedUtc=default;int closed=0;
            // The event never owns a frame or performs async work. One consumer owns every
            // acquisition/copy/Recreate and finishes before pool/session/device disposal.
            var arrivals=Channel.CreateBounded<bool>(new BoundedChannelOptions(1){FullMode=BoundedChannelFullMode.DropWrite,SingleReader=true,SingleWriter=false});
            void Frame(Direct3D11CaptureFramePool sender,object args)=>arrivals.Writer.TryWrite(true);
            void Closed(GraphicsCaptureItem _,object args){Interlocked.Exchange(ref closed,1);arrivals.Writer.TryComplete();}
            bool frameSubscribed=false,closedSubscribed=false;SoftwareBitmap? image=null;byte[]? jpeg=null;
            At(CaptureStage.CaptureSession);var session=pool.CreateCaptureSession(item);
            try {
                session.IsCursorCaptureEnabled=false;
                pool.FrameArrived+=Frame;frameSubscribed=true;item.Closed+=Closed;closedSubscribed=true;
                cancel.ThrowIfCancellationRequested();At(CaptureStage.StartSession);session.StartCapture();
                while(image==null) {
                    At(CaptureStage.FirstFrame);
                    try {await arrivals.Reader.ReadAsync(cancel).ConfigureAwait(false);}
                    catch(ChannelClosedException){throw new ScreenException("window-closed");}
                    cancel.ThrowIfCancellationRequested();if(Volatile.Read(ref closed)!=0)throw new ScreenException("window-closed");
                    FrameDisposition decision;
                    using(var frame=pool.TryGetNextFrame()) {
                        var itemNow=item.Size;FrameSize? content=null,buffer=null;
                        if(frame!=null) {var c=frame.ContentSize;content=new(c.Width,c.Height);At(CaptureStage.FrameSurface);var d=frame.Surface.Description;buffer=new(d.Width,d.Height);At(CaptureStage.FirstFrame);}
                        Box? currentFrame=null;var currentVisible=choice.Physical;
                        if(!choice.Monitor) {
                            if(!TryBounds(choice.Handle,out currentVisible))throw new ScreenException("capture-changed");
                            if(TryWindowBounds(choice.Handle,out var measuredNow))currentFrame=measuredNow;
                        }
                        // Record the actual frame before failing a changed display/window.
                        decision=acquisition.Observe(content,buffer,new(itemNow.Width,itemNow.Height),currentVisible,currentFrame,Environment.TickCount64-started);
                        if(!Matches(selectedSurface))throw new ScreenException("capture-changed");
                        if(decision==FrameDisposition.Accept) {
                            geometry=acquisition.Accepted!;dimensions=dimensions with {WgcWidth=geometry.Width,WgcHeight=geometry.Height,Mapping=geometry.Mapping};
                            capturedAt=Environment.TickCount64;capturedUtc=DateTimeOffset.UtcNow;At(CaptureStage.SurfaceCopy);
                            // Await native completion even if Stop arrives. This keeps the frame,
                            // pool and device alive until the copy releases their resources.
                            image=await SoftwareBitmap.CreateCopyFromSurfaceAsync(frame!.Surface,BitmapAlphaMode.Ignore).AsTask().ConfigureAwait(false);
                            cancel.ThrowIfCancellationRequested();
                        }
                    } // Return the checked-out frame BEFORE Recreate, as Microsoft specifies.
                    if(decision==FrameDisposition.Recreate) {
                        cancel.ThrowIfCancellationRequested();At(CaptureStage.FramePoolResize);
                        while(arrivals.Reader.TryRead(out _)){} // Retire notifications for discarded buffers.
                        var extent=acquisition.PoolSize;
                        pool.Recreate(device,DirectXPixelFormat.B8G8R8A8UIntNormalized,1,new(){Width=extent.Width,Height=extent.Height});
                    }
                }
                session.Dispose();captureActive=false;pool.FrameArrived-=Frame;frameSubscribed=false;
                At(CaptureStage.GeometryValidation);var surface=geometry.ToSurface((long)choice.Handle);
                if(image.PixelWidth!=geometry.Width||image.PixelHeight!=geometry.Height||!Matches(surface)||Volatile.Read(ref closed)!=0)throw new ScreenException("capture-changed");
                At(CaptureStage.JpegEncode);jpeg=await Encode(image,cancel).ConfigureAwait(false);
                At(CaptureStage.JpegDecode);using(var decoded=await Decode(jpeg,cancel).ConfigureAwait(false)) {if(decoded.PixelWidth!=surface.ImageWidth||decoded.PixelHeight!=surface.ImageHeight)throw new ScreenException("capture-changed");At(CaptureStage.BlankValidation);EnsureNonblank(decoded);}
                At(CaptureStage.GeometryValidation);if(!Matches(surface)||Volatile.Read(ref closed)!=0)throw new ScreenException("capture-changed");
                cancel.ThrowIfCancellationRequested();var shot=new Snapshot(surface,jpeg,capturedAt,capturedUtc);jpeg=null;succeeded=true;return shot;
            } finally {
                try {if(frameSubscribed)pool.FrameArrived-=Frame;}
                finally {
                    try {if(closedSubscribed)item.Closed-=Closed;}
                    finally {
                        arrivals.Writer.TryComplete();
                        try {if(captureActive)session.Dispose();}
                        finally {try{image?.Dispose();}finally{if(jpeg!=null)System.Security.Cryptography.CryptographicOperations.ZeroMemory(jpeg);}}
                    }
                }
            }
        } catch(CaptureFailure){succeeded=false;throw;}
        catch(OperationCanceledException) {
            var failure=Fail(deadline.IsCancellationRequested?CaptureOutcome.TimedOut:CaptureOutcome.Canceled);
            if(deadline.IsCancellationRequested)throw failure;throw;
        }
        catch(NativeStep e){throw Fail(CaptureOutcome.NativeFailure,e,e.Stage);}
        catch(ScreenException e){throw Fail(e.Code switch {"blank-or-protected"=>CaptureOutcome.BlankOrProtected,"capture-unstable"=>CaptureOutcome.FramesUnstable,"capture-timeout"=>CaptureOutcome.TimedOut,"capture-geometry" or "capture-changed" or "window-closed" or "geometry"=>CaptureOutcome.Changed,_=>CaptureOutcome.InvalidSelection});}
        catch(Exception e){throw Fail(CaptureOutcome.NativeFailure,e);}
        finally {
            if(succeeded)Diagnostics.Record(kind,CaptureStage.Complete,CaptureOutcome.Succeeded,Environment.TickCount64-started,dimensions:dimensions,frames:acquisition?.Trace);
            Volatile.Write(ref busy,0);
        }
    }
    internal static async Task<byte[]> Encode(SoftwareBitmap image,CancellationToken stop)
    {
        if(image.PixelWidth<1||image.PixelHeight<1||image.PixelWidth*(long)image.PixelHeight>MaxPixels)throw new ScreenException("image-size");
        using var stream=new InMemoryRandomAccessStream();
        // ImageQuality is an encoder option, not writable frame metadata.
        var options=new BitmapPropertySet{{"ImageQuality",new BitmapTypedValue(.72f,global::Windows.Foundation.PropertyType.Single)}};
        var encoder=await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId,stream,options).AsTask(stop);
        var scale=Math.Min(1,1100.0/Math.Max(image.PixelWidth,image.PixelHeight));
        encoder.BitmapTransform.ScaledWidth=(uint)Math.Round(image.PixelWidth*scale);encoder.BitmapTransform.ScaledHeight=(uint)Math.Round(image.PixelHeight*scale);
        encoder.BitmapTransform.InterpolationMode=BitmapInterpolationMode.Fant;encoder.SetSoftwareBitmap(image);
        await encoder.FlushAsync().AsTask(stop);
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
        const string name="Windows.Graphics.Capture.GraphicsCaptureItem";nint text=0,factory=0,item=0;var stage=CaptureStage.ActivationFactory;
        try {
            Marshal.ThrowExceptionForHR(WindowsCreateString(name,name.Length,out text));var iid=typeof(ICaptureItemInterop).GUID;
            Marshal.ThrowExceptionForHR(RoGetActivationFactory(text,ref iid,out factory));var interop=(ICaptureItemInterop)Marshal.GetObjectForIUnknown(factory);
            var itemId=new Guid("79c3f95b-31f7-4ec2-a464-632ef5d30760");
            stage=monitor?CaptureStage.CreateMonitorItem:CaptureStage.CreateWindowItem;
            Marshal.ThrowExceptionForHR(monitor?interop.CreateForMonitor(handle,ref itemId,out item):interop.CreateForWindow(handle,ref itemId,out item));
            stage=CaptureStage.ItemProjection;return WinRT.MarshalInspectable<GraphicsCaptureItem>.FromAbi(item);
        } catch(Exception e){throw new NativeStep(stage,e.HResult);}
        finally {if(item!=0)Marshal.Release(item);if(factory!=0)Marshal.Release(factory);if(text!=0)WindowsDeleteString(text);}
    }
    private static IDirect3DDevice CreateDevice()
    {
        nint device=0,context=0,dxgi=0,inspectable=0;var stage=CaptureStage.D3DDevice;
        try {
            var hr=D3D11CreateDevice(0,1,0,0x20,0,0,7,out device,out _,out context);
            if(hr<0)Marshal.ThrowExceptionForHR(D3D11CreateDevice(0,5,0,0x20,0,0,7,out device,out _,out context)); // WARP, bounded single frame.
            stage=CaptureStage.DxgiDevice;var iid=new Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c");Marshal.ThrowExceptionForHR(Marshal.QueryInterface(device,in iid,out dxgi));
            stage=CaptureStage.DeviceProjection;Marshal.ThrowExceptionForHR(CreateDirect3D11DeviceFromDXGIDevice(dxgi,out inspectable));return WinRT.MarshalInterface<IDirect3DDevice>.FromAbi(inspectable);
        } catch(Exception e){throw new NativeStep(stage,e.HResult);}
        finally {if(inspectable!=0)Marshal.Release(inspectable);if(dxgi!=0)Marshal.Release(dxgi);if(context!=0)Marshal.Release(context);if(device!=0)Marshal.Release(device);}
    }
    private sealed class NativeStep:IOException {
        internal CaptureStage Stage{get;}
        internal NativeStep(CaptureStage stage,int hresult):base("Native capture step failed"){Stage=stage;HResult=hresult;}
    }
    // GetWindowRect is DPI-virtualized. Scope only this synchronous read, then restore the caller context.
    internal static bool TryWindowBounds(nint hwnd,out Box bounds) {
        bounds=default;var previous=SetThreadDpiAwarenessContext((nint)(-4));if(previous==0)return false;
        try {if(!GetWindowRect(hwnd,out var rect))return false;bounds=BoxOf(rect);return bounds.Valid;}
        finally {SetThreadDpiAwarenessContext(previous);}
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
    [DllImport("user32.dll",CharSet=CharSet.Unicode,ExactSpelling=true)]private static extern int GetWindowTextW(nint hwnd,StringBuilder text,int count);
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
    [DllImport("user32.dll")]private static extern nint SetThreadDpiAwarenessContext(nint context);
    [DllImport("user32.dll")]private static extern bool GetWindowRect(nint hwnd,out Native.Rect rect);
    [DllImport("d3d11.dll")]private static extern int D3D11CreateDevice(nint adapter,int driver,nint software,uint flags,nint levels,uint count,uint version,out nint device,out uint level,out nint context);
    [DllImport("d3d11.dll")]private static extern int CreateDirect3D11DeviceFromDXGIDevice(nint dxgi,out nint inspectable);
}
