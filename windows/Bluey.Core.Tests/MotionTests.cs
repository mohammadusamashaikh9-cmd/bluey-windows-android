using Xunit;
namespace Bluey.Core.Tests;
public sealed class MotionTests
{
    private static readonly Size2 Bounds = new(1440, 900);
    private static void Near(double e, double a, double t = 1e-5) => Assert.InRange(a, e - t, e + t);
    private static void Near(V2 e, V2 a, double t = 1e-5) { Near(e.X, a.X, t); Near(e.Y, a.Y, t); }
    private static CursorEngine Engine() => new(new(), () => 0, () => .5);
    private static void Settle(CursorEngine e, double from, double to) { for (var t = from; t <= to; t += 1.0 / 60) e.Step(t, default, Bounds); }
    [Theory] [InlineData(.42, 0, .22, 1)] [InlineData(.25, .25, .22, 1)] [InlineData(.42, 0, .38, 1)]
    public void TimingMatchesIndependentBisectionAndNumericalDerivative(double x1, double y1, double x2, double y2)
    {
        var curve = new TimingCurve(x1, y1, x2, y2);
        static double B(double t, double a, double b) => 3 * a * t * Math.Pow(1 - t, 2) + 3 * b * t * t * (1 - t) + t * t * t;
        for (var i = 1; i < 100; i++)
        {
            var x = i / 100.0; double lo = 0, hi = 1;
            for (var n = 0; n < 60; n++) { var m = (lo + hi) / 2; if (B(m, x1, x2) < x) lo = m; else hi = m; }
            Near(B((lo + hi) / 2, y1, y2), curve.Value(x), 1e-6);
            const double h = .0001;
            Near((curve.Value(x + h) - curve.Value(x - h)) / (2 * h), curve.Rate(x), .004);
        }
        Near(0, curve.Value(-1)); Near(1, curve.Value(2)); Near(y1 / x1, curve.Rate(0)); Near(0, curve.Rate(1));
    }
    [Fact] public void LaunchIsNotRawYPolynomial() => Assert.InRange(TimingCurve.Launch.Value(.5), .75857, .75858);
    [Fact] public void FlightHasBezierPositionsAndAnalyticVelocity()
    {
        var f = new Flight(new(0, 0), new(100, -50), new(200, 50), new(300, 0), 2, 1, TimingCurve.Redirect, true);
        Near(new(150, 0), f.Position(.5)); Near(new(300, 0), f.Position(1));
        const double h = 1e-5; var t = 2.4;
        var a = f.Position(f.Timing.Value(f.Progress(t - h))); var b = f.Position(f.Timing.Value(f.Progress(t + h)));
        Near((b - a) / (2 * h), f.Velocity(t), .01);
        Near(default(V2), f.Velocity(3)); Near(0, f.Progress(0)); Near(1, f.Progress(4));
    }
    [Theory] [InlineData(0, .45)] [InlineData(24, .52)] [InlineData(100000, 1.2)]
    public void DurationPreservesClamps(double d, double expected) => Near(expected, CursorEngine.DurationFor(d));
    [Fact] public void DragUsesSteadyStraightPath()
    {
        var e = Engine(); e.Step(0, default, Bounds); e.Dragging = true; e.SetMode(CursorMode.Pinned, new(900, 400)); e.Step(.02, default, Bounds);
        var f = e.CurrentFlight!; Assert.Equal(TimingCurve.Steady, f.Timing);
        var d = f.To - f.From; var c = f.C1 - f.From; Near(0, d.X * c.Y - d.Y * c.X, 1e-6);
        Near(CursorEngine.DurationFor(d.Length, true), f.Duration);
    }
    [Fact] public void RedirectPreservesPositionAndVelocityBelowCarryCap()
    {
        var e = Engine(); e.Step(0, default, Bounds); e.SetMode(CursorMode.Pinned, new(400, 200)); e.Step(.01, default, Bounds); e.Step(.21, default, Bounds);
        var tip = e.Tip; var v = e.Velocity; Assert.True(v.Length > 60);
        e.SetMode(CursorMode.Pinned, new(1400, 100)); e.Step(.22, default, Bounds);
        Near(tip, e.Tip); Near(v, e.Velocity); Assert.Equal(TimingCurve.Redirect, e.CurrentFlight!.Timing);
    }
    [Fact] public void RedirectCapsCarryWithoutChangingDirection()
    {
        var e = Engine(); e.Step(0, default, Bounds); e.SetMode(CursorMode.Pinned, new(400, 200)); e.Step(.01, default, Bounds); e.Step(.3, default, Bounds);
        var v = e.Velocity; e.SetMode(CursorMode.Pinned, e.Tip + new V2(10, 0)); e.Step(.31, default, Bounds);
        var c = e.CurrentFlight!.C1 - e.CurrentFlight.From; Near(6, c.Length); Near(v / v.Length, c / c.Length);
    }
    [Fact] public void InterruptedTripOnlyLandsAtNewTargetOnce()
    {
        var e = Engine(); var landed = new List<V2>(); var launches = 0; e.Land += landed.Add; e.Launch += _ => launches++;
        e.Step(0, default, Bounds); e.SetMode(CursorMode.Pinned, new(100, 100)); Settle(e, .01, .2);
        e.SetMode(CursorMode.Pinned, new(900, 300)); Settle(e, .21, 3);
        Assert.Single(landed); Near(new(900, 300), landed[0]); Assert.Equal(2, launches); // Source emits another puff when redirecting before leaving home.
    }
    [Fact] public void ModesHaveDistinctTargetsAndFadeOnlyAfterReturn()
    {
        var e = Engine(); e.Step(0, default, Bounds); Near(0, e.Opacity);
        e.SetMode(CursorMode.Docked); Settle(e, .01, 2); Near(e.DockPoint(Bounds), e.Tip); Near(1, e.Opacity, .0001);
        e.SetMode(CursorMode.Pinned, new(400, 300)); Settle(e, 2.01, 4); Assert.False(e.IsHome);
        e.GoHome(); e.Step(4.01, default, Bounds); Assert.True(e.Opacity > .9);
        Settle(e, 4.02, 8); Near(e.TuckedPoint(Bounds), e.Tip); Near(0, e.Opacity, .0001);
    }
    [Fact] public void PressHoverAndAngleUseSourceTiming()
    {
        var e = Engine(); e.Step(0, default, Bounds); e.Press(.14); Near(.86, e.PressScale(.12)); Near(1, e.PressScale(.42)); Near(0, e.Hover(2));
        e.SetMode(CursorMode.Pinned, new(400, 300)); Settle(e, .01, 3); Near(0, e.Angle, .01);
        Near(0, e.Hover(e.LandedAt + .3)); Near(Math.Sin(2.4) * 1.6, e.Hover(e.LandedAt + 1));
    }
    [Fact] public void GazeIdleAndMoodPrecedenceMatchSource()
    {
        var e = Engine(); e.Step(0, new(720, 450), Bounds); Near(0, e.Face(0).GazeX); Near(-612.0 / 990, e.Face(0).GazeY);
        Assert.Equal(Mood.Sleepy, e.Face(6).Mood); Assert.Equal(Mood.Resting, e.Face(16).Mood);
        e.Awake = true; Assert.Equal(Mood.Listening, e.Face(16).Mood);
        e.SetMode(CursorMode.Pinned, new(100, 100)); Assert.Equal(Mood.Pointing, e.Face(16).Mood);
        e.TalkUntil = 20; Assert.Equal(Mood.Talking, e.Face(16).Mood); e.BrainMood = Mood.Thinking; Assert.Equal(Mood.Thinking, e.Face(16).Mood);
        e.GazeOverride = new(.3, -.4); Near(.3, e.Face(16).GazeX); Near(-.4, e.Face(16).GazeY);
        e.TalkUntil = 0; e.TalkLevel = .7; Near(.7, e.Face(16).Talk);
    }
    [Fact] public void ClockClampsAndRejectsInvalidTime()
    {
        var f = new FrameClock(); Near(1.0 / 60, f.Advance(0)); Near(1.0 / 480, f.Advance(0)); Near(.05, f.Advance(10));
        Assert.Throws<ArgumentOutOfRangeException>(() => f.Advance(9)); Assert.Throws<ArgumentOutOfRangeException>(() => f.Advance(double.NaN));
    }
    [Fact] public void InjectedClockAndRandomReproduceBlinkTrace()
    {
        double now = 0; var e = new CursorEngine(new(), () => now, () => 0); e.Tick(default, Bounds);
        now = 3.01; e.Tick(default, Bounds); Near(3.14, e.BlinkUntil); now = 5.5; e.Tick(default, Bounds); Near(3.14, e.BlinkUntil);
        now = 5.52; e.Tick(default, Bounds); Near(5.65, e.BlinkUntil);
    }
    [Theory] [InlineData(96)] [InlineData(144)] [InlineData(192)]
    public void CoordinatesRoundTripAcrossDpiAndOrigin(double dpi)
    {
        var t = new DisplayTransform(new(-1920, 120), dpi, dpi);
        Near(new(200, 100), t.ToLogical(new(-1920 + 200 * dpi / 96, 120 + 100 * dpi / 96)));
        Near(new(-1833, 412), t.ToPhysical(t.ToLogical(new(-1833, 412))));
    }
    [Fact] public void GeometryRetainsExactArcsPaletteAndEyePositions()
    {
        var p = CursorGeometry.Teardrop(96); Assert.Equal(8, p.Length); Near(new(6, 0), p[0].End);
        Near(48, p[2].Radius); Near(new(96, 48), p[2].End); Near(new(0, 6), p[5].End); Near(6, p[6].Radius);
        Near(new(43, 49), CursorGeometry.EyeCenter(0, 96)); Near(new(67, 49), CursorGeometry.EyeCenter(1, 96));
        Assert.Equal(new uint[] { 0xA9BCFF, 0x6C86F5, 0x4254D6, 0x2B2F8F }, Palette.Gradient.ToArray());
        Assert.Equal(new double[] { 0, .34, .68, 1 }, Palette.Stops.ToArray());
        var star = CursorGeometry.Star(10); Assert.Equal(8, star.Length); Near(new(0, -10), star[0]); Near(4, star[1].Length);
    }
    [Fact] public void CaptionFlipsAtTopAndLabelsStayInBounds()
    {
        var b = new Size2(800, 600); var size = new Size2(300, 80);
        var top = CaptionLayout.Place(size, b, new(20, 20), 72, true, .5, null, 0);
        Assert.True(top.TailUp); Assert.InRange(top.Box.X, 14, 486); Assert.InRange(top.Box.Y, 14, 506);
        var normal = CaptionLayout.Place(size, b, new(400, 400), 72, true, .5, null, 0); Assert.False(normal.TailUp); Near(262, normal.Box.Y);
        var home = CaptionLayout.Place(size, b, default, 72, false, .5, null, 0); Near(250, home.Box.X); Near(437.2, home.Box.Y);
        var label = CaptionLayout.Label(new(200, 40), b, new(790, 2), 72, 1, .5, 0); Assert.True(label.X >= 12 && label.Right <= 788 && label.Y >= 12);
    }
    [Fact] public void TrailsAndEffectsExpireDeterministically()
    {
        var e = Engine(); var v = new VisualState(e, () => .5); e.Step(0, default, Bounds); e.SetMode(CursorMode.Pinned, new(400, 100));
        for (var i = 1; i < 16; i++) { e.Step(i / 60.0, default, Bounds); v.Step(); }
        Assert.NotEmpty(v.Comet); Assert.True(v.CometOpacity > 0);
        e.Settings.Trail = PointerTrail.String; e.Step(.3, default, Bounds); v.Step(); Assert.Empty(v.Comet); Assert.NotNull(v.String);
        e.Settings.Trail = PointerTrail.None; e.Step(.31, default, Bounds); v.Step(); Assert.Null(v.String);
        v.DemoBurst(); Assert.True(v.Effects.Count >= 7);
        e.Step(3, default, Bounds); v.Step(); e.Step(4, default, Bounds); v.Step(); Assert.Empty(v.Effects);
    }
}
