namespace Bluey.Core;

// CursorOverlay.swift TimingCurve: invert x before sampling y. No framework easing preset.
public readonly record struct TimingCurve(double X1, double Y1, double X2, double Y2)
{
    public static readonly TimingCurve Launch = new(.42, 0, .22, 1);
    public static readonly TimingCurve Redirect = new(.25, .25, .22, 1);
    public static readonly TimingCurve Steady = new(.42, 0, .38, 1);
    private static double Coordinate(double t, double a, double b)
    {
        var u = 1 - t;
        return 3 * u * u * t * a + 3 * u * t * t * b + t * t * t;
    }
    private static double Derivative(double t, double a, double b)
    {
        var u = 1 - t;
        return 3 * u * u * a + 6 * u * t * (b - a) + 3 * t * t * (1 - b);
    }
    private double Parameter(double x)
    {
        var t = x;
        for (var i = 0; i < 8; i++)
        {
            var error = Coordinate(t, X1, X2) - x;
            if (Math.Abs(error) < 1e-7) return t;
            var d = Derivative(t, X1, X2);
            if (Math.Abs(d) < 1e-7) break;
            t -= error / d;
        }
        double low = 0, high = 1;
        t = x;
        for (var i = 0; i < 40; i++)
        {
            var value = Coordinate(t, X1, X2);
            if (Math.Abs(value - x) < 1e-7) break;
            if (value < x) low = t; else high = t;
            t = (low + high) / 2;
        }
        return t;
    }
    public double Value(double x) => x <= 0 ? 0 : x >= 1 ? 1 : Coordinate(Parameter(x), Y1, Y2);
    public double Rate(double x)
    {
        var t = Parameter(Math.Clamp(x, 0, 1));
        var dx = Derivative(t, X1, X2);
        return dx > 1e-7 ? Derivative(t, Y1, Y2) / dx : 0;
    }
}
public sealed record Flight(V2 From, V2 C1, V2 C2, V2 To, double Start, double Duration, TimingCurve Timing, bool Pointing)
{
    public bool Landed { get; set; }
    public double Progress(double now) => Duration <= 0 ? 1 : Math.Clamp((now - Start) / Duration, 0, 1);
    public V2 Position(double e)
    {
        var u = 1 - e;
        return From * (u * u * u) + C1 * (3 * u * u * e) + C2 * (3 * u * e * e) + To * (e * e * e);
    }
    public V2 Tangent(double e)
    {
        var u = 1 - e;
        return (C1 - From) * (3 * u * u) + (C2 - C1) * (6 * u * e) + (To - C2) * (3 * e * e);
    }
    public V2 Velocity(double now)
    {
        var p = Progress(now);
        return p >= 1 ? default : Tangent(Timing.Value(p)) * (Timing.Rate(p) / Duration);
    }
}
