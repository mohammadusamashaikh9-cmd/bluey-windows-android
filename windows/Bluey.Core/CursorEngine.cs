namespace Bluey.Core;

// Literal motion/state translation of CursorOverlay.swift. Own this object on one thread.
public sealed class CursorEngine
{
    private readonly Func<double> clock;
    private readonly Func<double> random;
    private readonly FrameClock frameClock = new();
    private bool placed;
    private double angleVelocity, pressStart = -10, pressDepth, nextBlink, lastMouseMove;
    private V2 mouse;
    public CursorSettings Settings { get; }
    public CursorMode Mode { get; private set; } = CursorMode.Following;
    public V2 PinnedPoint { get; private set; }
    public V2 Tip { get; private set; } = new(-1000, -1000);
    public V2 Velocity { get; private set; }
    public double Angle { get; private set; } = Math.PI / 4;
    public double Opacity { get; private set; }
    public Size2 Bounds { get; private set; } = new(1440, 900);
    public Flight? CurrentFlight { get; private set; }
    public double LandedAt { get; private set; } = -10;
    public double BlinkUntil { get; private set; }
    public double TalkUntil { get; set; }
    public double TalkLevel { get; set; }
    public Mood? BrainMood { get; set; }
    public V2? GazeOverride { get; set; }
    public bool Awake { get; set; }
    public bool Dragging { get; set; }
    public double Now { get; private set; }
    public double DeltaTime { get; private set; }
    public bool IsHome => Mode != CursorMode.Pinned;
    public event Action<V2>? Land;
    public event Action<V2>? Launch;
    public CursorEngine(CursorSettings settings, Func<double> clock, Func<double> random)
    {
        Settings = settings; this.clock = clock; this.random = random;
        lastMouseMove = clock(); nextBlink = lastMouseMove + 3;
    }
    public void SetMode(CursorMode mode, V2 point = default) { Mode = mode; PinnedPoint = point; }
    public void GoHome() => SetMode(Settings.FollowMouse ? CursorMode.Following : CursorMode.Docked);
    public V2 PhonePoint(Size2 b) => new(b.Width * Settings.PhonePosition, b.Height * 1.18);
    public V2 DockPoint(Size2 b) => new(b.Width * Settings.PhonePosition, b.Height - Settings.Size * .42);
    public V2 TuckedPoint(Size2 b) => new(b.Width * Settings.PhonePosition, b.Height + Settings.Size * .35);
    public double TimeToArrive => Math.Max(0, (CurrentFlight?.Start ?? 0) + (CurrentFlight?.Duration ?? 0) - Now);
    public void Press(double depth) { pressStart = Now; pressDepth = depth; }
    public void Tick(V2 mouse, Size2 bounds) => Step(clock(), mouse, bounds);
    public void Step(double now, V2 mouse, Size2 bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 || !double.IsFinite(bounds.Width + bounds.Height + mouse.X + mouse.Y))
            throw new ArgumentOutOfRangeException(nameof(bounds));
        var dt = frameClock.Advance(now); DeltaTime = dt; Now = now; Bounds = bounds;
        if ((mouse - this.mouse).Length > 1.5) lastMouseMove = now;
        this.mouse = mouse;
        if (!placed) { Tip = TuckedPoint(bounds); placed = true; }
        var target = Mode switch { CursorMode.Docked => DockPoint(bounds), CursorMode.Following => TuckedPoint(bounds), _ => PinnedPoint };
        if (((CurrentFlight?.To ?? Tip) - target).Length > .5) Plan(target, now);
        Advance(now);
        var settled = (CurrentFlight?.Progress(now) ?? 1) >= 1;
        var want = Mode == CursorMode.Following && settled ? 0 : 1;
        Opacity += (want - Opacity) * Math.Min(1, dt * (want > Opacity ? 12 : 6));
        var lean = Math.Clamp(Velocity.X / 2800, -.2, .2);
        var targetAngle = (IsHome ? Math.PI / 4 : 0) + lean;
        angleVelocity += (24 * (targetAngle - Angle) - 2 * Math.Sqrt(24) * angleVelocity) * dt;
        Angle += angleVelocity * dt;
        if (now > nextBlink) { BlinkUntil = now + .13; nextBlink = now + 2.5 + random() * 3.5; }
    }
    public static double DurationFor(double distance, bool dragging = false)
    {
        var duration = Math.Clamp(.4 + .12 * Math.Log2(1 + distance / 24), .45, 1.2);
        return dragging ? Math.Clamp(duration * 1.45, .6, 1.6) : duration;
    }
    private void Plan(V2 goal, double now)
    {
        var from = Tip; var delta = goal - from; var distance = delta.Length;
        var dock = DockPoint(Bounds);
        var fromHome = (from - dock).Length < Settings.Size * 1.2 || from.Y > Bounds.Height - 2;
        var duration = DurationFor(distance, Dragging);
        var direction = delta / distance;
        var normal = new V2(-direction.Y, direction.X);
        if (Math.Abs(direction.X) > .4) { if (normal.Y > 0) normal *= -1; }
        else
        {
            var towardMiddle = Bounds.Width / 2 - (from.X + goal.X) / 2 >= 0 ? 1 : -1;
            if (normal.X * towardMiddle < 0) normal *= -1;
        }
        var bow = Dragging ? 0 : Math.Min(distance * .1, 70) * .75;
        var reach = distance * .3;
        var c2 = goal - direction * reach + normal * bow;
        V2 c1; TimingCurve timing;
        if (Velocity.Length > 60)
        {
            var carry = Velocity * (duration / 3);
            if (carry.Length > distance * .6) carry = carry / carry.Length * (distance * .6);
            c1 = from + carry; timing = TimingCurve.Redirect;
        }
        else { c1 = from + direction * reach + normal * bow; timing = Dragging ? TimingCurve.Steady : TimingCurve.Launch; }
        CurrentFlight = new(from, c1, c2, goal, now, duration, timing, !IsHome);
        if (!IsHome && fromHome) Launch?.Invoke(new(dock.X, Bounds.Height));
    }
    private void Advance(double now)
    {
        if (CurrentFlight is not { } flight) { Velocity = default; return; }
        var p = flight.Progress(now);
        Tip = flight.Position(flight.Timing.Value(p)); Velocity = flight.Velocity(now);
        if (p >= 1 && !flight.Landed)
        {
            flight.Landed = true;
            if (flight.Pointing) { LandedAt = now; Land?.Invoke(Tip); }
            else if (Mode == CursorMode.Following) Launch?.Invoke(new(Tip.X, Bounds.Height));
        }
    }
    public double PressScale(double now)
    {
        var t = now - pressStart;
        if (t < 0 || t >= .42) return 1;
        if (t < .12) return 1 - pressDepth * Math.Sin(t / .12 * Math.PI / 2);
        var r = (t - .12) / .3;
        return 1 - pressDepth * Math.Cos(r * Math.PI * 1.5) * Math.Exp(-r * 3.2);
    }
    public double Hover(double now)
    {
        if (IsHome || (CurrentFlight?.Progress(now) ?? 1) < 1) return 0;
        var since = now - LandedAt;
        return Math.Sin(since * 2.4) * 1.6 * Math.Clamp((since - .35) / .6, 0, 1);
    }
    public V2 BodyCenter => Tip + new V2(Math.Cos(Angle + Math.PI / 4), Math.Sin(Angle + Math.PI / 4)) * (Settings.Size * .5 * Math.Sqrt(2));
    public FaceState Face(double now)
    {
        var phone = PhonePoint(Bounds); var c = Mode == CursorMode.Following ? mouse : BodyCenter;
        var gx = Math.Clamp((c.X - phone.X) / (Bounds.Width * .5), -1, 1);
        var gy = -Math.Clamp((phone.Y - c.Y) / (Bounds.Height * 1.1), .15, 1);
        if (GazeOverride is { } g) { gx = g.X; gy = g.Y; }
        var testing = now < TalkUntil;
        var talk = testing ? .5 + .5 * Math.Sin(now * 19) * Math.Sin(now * 7.3) : TalkLevel;
        var mood = Settings.Mood;
        if (BrainMood is { } brain) mood = brain;
        else if (testing) mood = Mood.Talking;
        else if (Mode == CursorMode.Pinned && mood == Mood.Listening) mood = Mood.Pointing;
        else if (Mode == CursorMode.Following && !Awake && mood == Mood.Listening)
        {
            var idle = now - lastMouseMove;
            if (idle > 15) mood = Mood.Resting; else if (idle > 5) mood = Mood.Sleepy;
        }
        return new(gx, gy, mood, talk);
    }
}
