namespace Bluey.Core;

// All engine coordinates: top-left origin, y down, logical DIPs; time: monotonic seconds.
public readonly record struct V2(double X, double Y)
{
    public double Length => Math.Sqrt(X * X + Y * Y);
    public static V2 operator +(V2 a, V2 b) => new(a.X + b.X, a.Y + b.Y);
    public static V2 operator -(V2 a, V2 b) => new(a.X - b.X, a.Y - b.Y);
    public static V2 operator *(V2 a, double s) => new(a.X * s, a.Y * s);
    public static V2 operator /(V2 a, double s) => new(a.X / s, a.Y / s);
    public V2 Rotate(double a) => new(X * Math.Cos(a) - Y * Math.Sin(a), X * Math.Sin(a) + Y * Math.Cos(a));
}
public readonly record struct Size2(double Width, double Height);
public readonly record struct Rect2(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;
    public double MidX => X + Width / 2;
}
public enum CursorMode { Following, Docked, Pinned }
public enum PointerTrail { Comet, String, None }
public enum Mood { Listening, Resting, Thinking, Talking, Pointing, Happy, Sleepy }
public readonly record struct FaceState(double GazeX, double GazeY, Mood Mood, double Talk);
public sealed class CursorSettings
{
    public double Size { get; set; } = 72;
    public double PhonePosition { get; set; } = .5;
    public bool Glow { get; set; }
    public bool ShowCursor { get; set; } = true;
    public bool FollowMouse { get; set; } = true;
    public PointerTrail Trail { get; set; } = PointerTrail.Comet;
    public Mood Mood { get; set; } = Mood.Listening;
}
public readonly record struct DisplayTransform(V2 PhysicalOrigin, double DpiX, double DpiY)
{
    public V2 ToLogical(V2 physical) => new((physical.X - PhysicalOrigin.X) * 96 / DpiX, (physical.Y - PhysicalOrigin.Y) * 96 / DpiY);
    public V2 ToPhysical(V2 logical) => new(logical.X * DpiX / 96 + PhysicalOrigin.X, logical.Y * DpiY / 96 + PhysicalOrigin.Y);
}
public sealed class FrameClock
{
    private double? previous;
    public double Advance(double now)
    {
        if (!double.IsFinite(now) || now < 0 || now < previous) throw new ArgumentOutOfRangeException(nameof(now));
        var dt = previous is { } last ? Math.Clamp(now - last, 1.0 / 480, 1.0 / 20) : 1.0 / 60;
        previous = now;
        return dt;
    }
}
public static class Palette
{
    public const uint Berry1 = 0xA9BCFF, Berry2 = 0x6C86F5, Berry3 = 0x4254D6, Berry4 = 0x2B2F8F;
    public const uint Ink = 0x17151F, Nose = 0x1C1F66, InkSoft = 0xB9B2CC, Panel = 0x1E1B29;
    public static ReadOnlySpan<uint> Gradient => [Berry1, Berry2, Berry3, Berry4];
    public static ReadOnlySpan<double> Stops => [0, .34, .68, 1];
}
