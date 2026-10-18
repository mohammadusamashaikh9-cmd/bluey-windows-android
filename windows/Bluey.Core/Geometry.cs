namespace Bluey.Core;

public enum PathVerb { Move, Line, Arc, Cubic, Close }
public readonly record struct PathPart(PathVerb Verb, V2 End = default, V2 C1 = default, V2 C2 = default, double Radius = 0);
public static class CursorGeometry
{
    // Exact circular tangent arcs from Swift, not cubic approximations.
    public static PathPart[] Teardrop(double s)
    {
        var r = 6 * s / 96;
        return [new(PathVerb.Move, new(r, 0)), new(PathVerb.Line, new(s / 2, 0)),
            new(PathVerb.Arc, new(s, s / 2), Radius: s / 2), new(PathVerb.Arc, new(s / 2, s), Radius: s / 2),
            new(PathVerb.Arc, new(0, s / 2), Radius: s / 2), new(PathVerb.Line, new(0, r)),
            new(PathVerb.Arc, new(r, 0), Radius: r), new(PathVerb.Close)];
    }
    public static V2[] Star(double radius) => Enumerable.Range(0, 8).Select(i =>
    {
        var a = i * Math.PI / 4 - Math.PI / 2;
        return new V2(Math.Cos(a), Math.Sin(a)) * (i % 2 == 0 ? radius : radius * .4);
    }).ToArray();
    public static V2 EyeCenter(int eye, double side) => new V2(43 + eye * 24, 49) * (side / 96);
    public static V2[] Comet(IReadOnlyList<V2> samples, double side)
    {
        if (samples.Count < 3) return [];
        var left = new List<V2>(); var right = new List<V2>();
        for (var i = 0; i < samples.Count; i++)
        {
            var d = samples[Math.Min(samples.Count - 1, i + 1)] - samples[Math.Max(0, i - 1)];
            var n = new V2(-d.Y, d.X) / Math.Max(d.Length, .001);
            var w = side * .42 * Math.Pow((double)i / (samples.Count - 1), 1.3) / 2;
            left.Add(samples[i] + n * w); right.Add(samples[i] - n * w);
        }
        right.Reverse(); return left.Concat(right).ToArray();
    }
}
public readonly record struct SpeechPlacement(Rect2 Box, V2 Target, bool TailUp, double BaseX, double Lean);
public static class CaptionLayout
{
    public static double FontSize(int length) => length <= 50 ? 21 : length <= 110 ? 19 : 17;
    public static double MaxTextWidth(int length, Size2 b) => Math.Min(Math.Min(b.Width * .5, 580), Math.Max(240, Math.Sqrt(length) * 50));
    public static SpeechPlacement Place(Size2 size, Size2 b, V2 tip, double cursorSize, bool pointing,
        double phonePosition, Rect2? target, double age)
    {
        const double tail = 18, margin = 14;
        V2 aim; V2 origin; var up = false;
        if (pointing)
        {
            var thing = target ?? new Rect2(tip.X - 12, tip.Y - 30, 24, 24);
            aim = new(thing.MidX, thing.Y - 10); origin = new(aim.X - size.Width / 2, aim.Y - tail - size.Height);
            if (origin.Y < margin)
            {
                aim = new(tip.X + cursorSize * .5, tip.Y + cursorSize * 1.35);
                origin = new(aim.X - size.Width / 2, aim.Y + tail); up = true;
            }
        }
        else { aim = new(b.Width * phonePosition, b.Height - cursorSize * .9); origin = new(aim.X - size.Width / 2, aim.Y - tail - size.Height); }
        var bob = Math.Sin(age * 2.2) * 2;
        origin = new(Math.Clamp(origin.X, margin, Math.Max(margin, b.Width - size.Width - margin)),
            Math.Clamp(origin.Y + bob, margin, Math.Max(margin, b.Height - size.Height - margin)));
        aim += new V2(0, bob * .5);
        var r = Math.Min(24, size.Height / 2); var local = aim.X - origin.X;
        var baseX = Math.Clamp(local, r + 15, Math.Max(r + 15, size.Width - r - 15));
        return new(new(origin.X, origin.Y, size.Width, size.Height), aim, up, baseX, Math.Clamp(local - baseX, -tail * 1.4, tail * 1.4));
    }
    public static Rect2 Label(Size2 size, Size2 b, V2 tip, double side, double opacity, double phonePosition, double age)
    {
        var a = opacity > .1 ? new V2(tip.X + side * 1.05, tip.Y - side * .1) : new V2(b.Width * phonePosition, b.Height - side * 1.3);
        if (a.X + size.Width > b.Width - 12) a = a with { X = tip.X - size.Width - side * .3 };
        return new(Math.Clamp(a.X, 12, Math.Max(12, b.Width - size.Width - 12)),
            Math.Clamp(a.Y - size.Height / 2 - Math.Min(age, .3) * 20, 12, Math.Max(12, b.Height - size.Height - 12)), size.Width, size.Height);
    }
}
