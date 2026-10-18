namespace Bluey.Core;

public readonly record struct EffectFrame(V2 Point, double Radius, double Opacity, double Rotation);
public static class Presentation
{
    private static readonly TimingCurve RingEase = new(.2, .7, .3, 1);
    private static readonly TimingCurve StarEase = new(.1, .8, .3, 1);
    private static readonly TimingCurve ClearEase = new(.42, 0, 1, 1);
    // Unit-mass underdamped spring. Initial velocity is in animation-distances/second,
    // as in CASpringAnimation, rather than raw scale units/second.
    public static double Pop(double age, double from, double stiffness, double damping, double initialVelocity)
    {
        var t = Math.Max(0, age); var a = damping / 2; var w = Math.Sqrt(stiffness - a * a); var offset = from - 1;
        var velocity = initialVelocity * (1 - from);
        return 1 + Math.Exp(-a * t) * (offset * Math.Cos(w * t) + (velocity + a * offset) / w * Math.Sin(w * t));
    }
    public static (double Scale, double Opacity) Label(double age, double life) =>
        (Pop(age, .4, 100, 11, 6), Math.Clamp(Math.Min(age / .12, (life - age) / .3), 0, 1));
    public static (double Scale, double Opacity) Speech(double age, double? clearAge)
    {
        var opacity = Math.Clamp(age / .12, 0, 1);
        if (clearAge is { } cleared)
        {
            var ease = ClearEase.Value(Math.Clamp(cleared / .22, 0, 1));
            return (1 - .15 * ease, opacity * (1 - ease));
        }
        return (Pop(age, .55, 260, 12, 4), opacity);
    }
    public static EffectFrame Sample(Effect effect, double now)
    {
        var p = Math.Clamp((now - effect.Born) / effect.Duration, 0, 1);
        if (effect.Kind == EffectKind.Ring)
        {
            var eased = RingEase.Value(p);
            return new(effect.Point, effect.Radius * (.3 + .7 * eased), (1 - eased) * effect.Alpha, 0);
        }
        return new(effect.Point + effect.Travel * StarEase.Value(p), effect.Radius * (1 - .6 * p),
            p < .45 ? 1 : (1 - p) / .55, effect.Spin * p);
    }
}
