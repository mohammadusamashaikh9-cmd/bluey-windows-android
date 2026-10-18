using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Bluey.Core;

namespace Bluey.Windows;
internal sealed class OverlayWindow : Window
{
    private nint hwnd;
    private readonly CursorEngine engine;
    private readonly OverlayArt art;
    private DisplayTransform coordinates;
    private bool resizing;
    internal nint Handle => hwnd;
    internal DisplayTransform Coordinates => coordinates;
    internal OverlayArt Art => art;
    internal event Action? Frame;
    internal OverlayWindow(CursorEngine engine, VisualState visuals)
    {
        this.engine = engine;
        WindowStyle = WindowStyle.None; ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true; Background = Brushes.Transparent;
        Topmost = true; ShowInTaskbar = false; ShowActivated = false; Focusable = false;
        IsHitTestVisible = false;
        art = new(engine, visuals); Content = art;
        SourceInitialized += (_, _) =>
        {
            hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)!.AddHook(Hook);
            Native.ConfigureOverlay(hwnd); FitPrimary();
        };
        CompositionTarget.Rendering += Render;
        Closed += (_, _) => CompositionTarget.Rendering -= Render;
    }
    private nint Hook(nint window, int msg, nint wparam, nint lparam, ref bool handled)
    {
        if (msg == Native.WmNcHitTest) { handled = true; return -1; } // HTTRANSPARENT, plus layered WS_EX_TRANSPARENT across threads.
        if (msg == Native.WmMouseActivate) { handled = true; return 3; } // MA_NOACTIVATE
        if (msg is Native.WmDisplayChange or Native.WmDpiChanged)
            Dispatcher.BeginInvoke(FitPrimary, DispatcherPriority.Loaded);
        return 0;
    }
    internal void FitPrimary()
    {
        if (resizing || hwnd == 0) return;
        resizing = true;
        try
        {
            var b = Native.PrimaryBounds();
            if (!Native.SetWindowPos(hwnd, -1, b.Left, b.Top, b.Right - b.Left, b.Bottom - b.Top, Native.NoActivatePosition))
                throw new System.ComponentModel.Win32Exception();
            var dpi = Native.GetDpiForWindow(hwnd);
            coordinates = new(new(b.Left, b.Top), dpi, dpi);
            var size = coordinates.ToLogical(new(b.Right, b.Bottom));
            Width = size.X; Height = size.Y;
            if (engine.Mode == CursorMode.Pinned) Point(engine.PinnedPoint);
        }
        finally { resizing = false; }
    }
    internal void Point(V2 point)
    {
        // Keep demo targets on the primary display; no unsupported multi-monitor routing.
        engine.SetMode(CursorMode.Pinned, new(Math.Clamp(point.X, 0, Math.Max(0, Width - 1)), Math.Clamp(point.Y, 0, Math.Max(0, Height - 1))));
    }
    internal void PointAtMouse() => Point(coordinates.ToLogical(Native.Mouse()));
    private TimeSpan lastRendering = TimeSpan.MinValue;
    private void Render(object? sender, EventArgs args)
    {
        if (args is RenderingEventArgs frame && frame.RenderingTime == lastRendering) return;
        if (args is RenderingEventArgs rendered) lastRendering = rendered.RenderingTime;
        if (hwnd == 0 || ActualWidth <= 0 || ActualHeight <= 0) return;
        engine.Tick(coordinates.ToLogical(Native.Mouse()), new(ActualWidth, ActualHeight));
        art.Advance(); Frame?.Invoke();
    }
}
