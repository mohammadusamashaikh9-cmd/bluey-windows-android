namespace Bluey.Core;

public enum EffectKind { Ring, Star }
public sealed record Effect(EffectKind Kind, V2 Point, double Radius, V2 Travel, uint Color, double Alpha, double Born, double Duration, double Spin = 0);
// Pure transient state: injected random draws and the owner's monotonic time.
public sealed class VisualState
{
    private readonly Func<double> random;
    private readonly CursorEngine engine;
    private readonly List<(V2 Point, double Time)> samples = [];
    private V2? stringMid;
    private V2 stringVelocity;
    private double lastTwinkle;
    public V2 Look { get; private set; } = new(-.7, -.7);
    public List<Effect> Effects { get; } = [];
    public V2[] Comet { get; private set; } = [];
    public double CometOpacity { get; private set; }
    public (V2 From, V2 Control, V2 To)? String { get; private set; }
    public VisualState(CursorEngine engine, Func<double> random)
    {
        this.engine = engine; this.random = random;
        engine.Land += p => { engine.Press(.05); Ring(p, 30, Palette.Berry2, .55, .7); };
        engine.Launch += Puff;
    }
    private double Between(double a, double b) => a + random() * (b - a);
    private void Ring(V2 p, double r, uint c, double a, double d) => Effects.Add(new(EffectKind.Ring, p, r, default, c, a, engine.Now, d));
    private void Star(V2 p, double r, V2 travel, uint c, double d) => Effects.Add(new(EffectKind.Star, p, r, travel, c, 1, engine.Now, d, Between(-2.5, 2.5)));
    private void Puff(V2 p)
    {
        if (!engine.Settings.ShowCursor) return;
        Ring(p, 26, Palette.Berry1, .6, .55);
        for (var i = 0; i < 3; i++) { var dx = (i - 1) * 14; Star(p + new V2(dx, -4), 4, new(dx * .6, -28), Palette.Berry1, .6); }
    }
    public void DemoBurst()
    {
        // Source clickEffect appearance only; never an OS input action.
        engine.Press(.14); Ring(engine.Tip, 38, Palette.Berry2, .85, .65);
        for (var i = 0; i < 6; i++)
        {
            var a = i / 6.0 * 2 * Math.PI + Between(-.25, .25); var d = Between(34, 58);
            Star(engine.Tip, Between(4, 7), new V2(Math.Cos(a), Math.Sin(a)) * d, Palette.Gradient[(int)(random() * 3)], .55);
        }
    }
    public void Step()
    {
        var now = engine.Now; var dt = engine.DeltaTime; var speed = engine.Velocity.Length;
        var want = speed > 90 ? engine.Velocity.Rotate(-engine.Angle) / Math.Max(speed, 1) : new V2(-.7, -.7);
        Look += (want - Look) * Math.Min(1, dt * 12);
        samples.Add((engine.BodyCenter, now)); samples.RemoveAll(p => now - p.Time > .2);
        if (samples.Count > 256) samples.RemoveAt(0);
        var visible = engine.Settings.ShowCursor;
        Comet = visible && engine.Settings.Trail == PointerTrail.Comet && speed > 260 ? CursorGeometry.Comet(samples.Select(p => p.Point).ToArray(), engine.Settings.Size) : [];
        CometOpacity = Math.Min(1, (speed - 260) / 900) * .26 * engine.Opacity;
        if (Comet.Length > 0 && speed > 900 && now - lastTwinkle > .05)
        {
            lastTwinkle = now;
            Star(engine.BodyCenter + new V2(Between(-10, 10), Between(-10, 10)), Between(3, 5.5), new(Between(-8, 8), Between(6, 18)), random() < .5 ? Palette.Berry1 : Palette.Berry2, .5);
        }
        String = null;
        if (visible && engine.Settings.Trail == PointerTrail.String && engine.Opacity > .02)
        {
            var from = new V2(engine.Bounds.Width * engine.Settings.PhonePosition, engine.Bounds.Height + 4);
            var to = engine.BodyCenter; var distance = (to - from).Length;
            var goal = (from + to) / 2 + new V2(Math.Sin(now * 1.3) * 6, Math.Max(0, 160 - distance * .12));
            var mid = stringMid ?? goal;
            stringVelocity += ((goal - mid) * 22 - stringVelocity * (2 * Math.Sqrt(22) * .55)) * dt;
            mid += stringVelocity * dt; stringMid = mid;
            String = (from, mid * 2 - (from + to) / 2, to);
        }
        else stringMid = null;
        Effects.RemoveAll(e => now - e.Born >= e.Duration);
    }
}
