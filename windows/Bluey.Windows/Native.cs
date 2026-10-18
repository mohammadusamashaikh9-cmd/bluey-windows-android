using System.ComponentModel;
using System.Runtime.InteropServices;
using Bluey.Core;

namespace Bluey.Windows;
internal static class Native
{
    public const int GwlExStyle = -20, WmMouseActivate = 0x21, WmNcHitTest = 0x84, WmHotkey = 0x312;
    public const int WmDisplayChange = 0x7e, WmDpiChanged = 0x2e0;
    public const long Transparent = 0x20, ToolWindow = 0x80, Layered = 0x80000, NoActivate = 0x08000000;
    public const uint NoSize = 1, NoMove = 2, NoZOrder = 4, NoActivatePosition = 0x10, FrameChanged = 0x20;
    [StructLayout(LayoutKind.Sequential)] internal struct Point { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] internal struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] internal struct MonitorInfo { public int Size; public Rect Monitor, Work; public uint Flags; }
    [DllImport("user32.dll")] internal static extern nint GetWindowLongPtrW(nint hwnd, int index);
    [DllImport("user32.dll", SetLastError = true)] private static extern nint SetWindowLongPtrW(nint hwnd, int index, nint value);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern nint MonitorFromPoint(Point point, uint flags);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetMonitorInfoW(nint monitor, ref MonitorInfo info);
    [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(nint hwnd);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool RegisterHotKey(nint hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool UnregisterHotKey(nint hwnd, int id);
    [DllImport("user32.dll")] internal static extern nint SendMessageW(nint hwnd, uint msg, nint wparam, nint lparam);
    [DllImport("user32.dll")] internal static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern nint GetWindow(nint hwnd, uint command);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindowVisible(nint hwnd);
    internal static void ConfigureOverlay(nint hwnd)
    {
        var style = (long)GetWindowLongPtrW(hwnd, GwlExStyle);
        Marshal.SetLastPInvokeError(0);
        var old = SetWindowLongPtrW(hwnd, GwlExStyle, (nint)(style | Transparent | ToolWindow | NoActivate | Layered));
        if (old == 0 && Marshal.GetLastPInvokeError() != 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
        if (!SetWindowPos(hwnd, -1, 0, 0, 0, 0, NoMove | NoSize | NoActivatePosition | FrameChanged)) throw new Win32Exception();
    }
    internal static Rect PrimaryBounds()
    {
        var monitor = MonitorFromPoint(default, 1); // MONITOR_DEFAULTTOPRIMARY; (0,0) belongs to primary.
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfoW(monitor, ref info)) throw new Win32Exception();
        return info.Monitor;
    }
    internal static V2 Mouse()
    {
        if (!GetCursorPos(out var p)) throw new Win32Exception();
        return new(p.X, p.Y);
    }
}
