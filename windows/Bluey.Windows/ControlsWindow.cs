using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using Bluey.Core;

namespace Bluey.Windows;
internal sealed class ControlsWindow : Window
{
    private readonly CursorEngine engine;
    private readonly OverlayWindow overlay;
    private readonly VisualState visuals;
    private readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, Margin = new(0, 12, 0, 0) };
    private readonly TextBlock warning = new() { TextWrapping = TextWrapping.Wrap };
    private readonly CheckBox shown;
    private readonly Dictionary<int, Action> hotkeys = [];
    private nint hwnd;
    private double lastStatus;
    internal ControlsWindow(CursorEngine engine, OverlayWindow overlay, VisualState visuals, Action? openConnection = null, Action? voicePreview = null)
    {
        this.engine = engine; this.overlay = overlay; this.visuals = visuals;
        Title = "Bluey — offline Windows demo"; Width = 430; Height = 700; MinWidth = 350; MinHeight = 420;
        var panel = new StackPanel { Margin = new(20) }; Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        panel.Children.Add(new TextBlock { Text = "Bluey desktop character", FontSize = 23, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock { Text = "Primary monitor · offline demo · close this window to exit", TextWrapping = TextWrapping.Wrap, Margin = new(0, 5, 0, 14) });
        var row = new WrapPanel(); panel.Children.Add(row);
        Button(row, "Point at mouse (Ctrl+Alt+P)", () => overlay.PointAtMouse());
        Button(row, "Home (Ctrl+Alt+D)", engine.GoHome);
        Button(row, "Dock", () => { engine.Settings.FollowMouse = false; engine.SetMode(CursorMode.Docked); Save(); });
        Button(row, "Follow (Ctrl+Alt+F)", () => { engine.Settings.FollowMouse = true; engine.GoHome(); Save(); });
        var location = new WrapPanel(); panel.Children.Add(location);
        var x = Field("X in primary-monitor DIPs", "400"); var y = Field("Y in primary-monitor DIPs", "300"); location.Children.Add(x); location.Children.Add(y);
        Button(location, "Point at X/Y", () =>
        {
            if (double.TryParse(x.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var px) && double.TryParse(y.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var py) && double.IsFinite(px + py))
                overlay.Point(new(px, py));
            else warning.Text = "Enter finite X and Y coordinates in DIPs.";
        });
        shown = new CheckBox { Content = "Show overlay (Ctrl+Alt+H)", IsChecked = engine.Settings.ShowCursor, Margin = new(0, 10, 0, 5) };
        shown.Click += (_, _) => { engine.Settings.ShowCursor = shown.IsChecked == true; Save(); }; panel.Children.Add(shown);
        var glow = new CheckBox { Content = "Cursor glow", IsChecked = engine.Settings.Glow, Margin = new(0, 4, 0, 5) };
        glow.Click += (_, _) => { engine.Settings.Glow = glow.IsChecked == true; Save(); }; panel.Children.Add(glow);
        Combo(panel, "Size (DIPs)", new double[] { 48, 72, 96, 120 }, engine.Settings.Size, n => engine.Settings.Size = n);
        Combo(panel, "Trail", Enum.GetValues<PointerTrail>(), engine.Settings.Trail, t => engine.Settings.Trail = t);
        Combo(panel, "Phone position", new[] { "Left", "Center", "Right" }, engine.Settings.PhonePosition < .35 ? "Left" : engine.Settings.PhonePosition > .65 ? "Right" : "Center",
            p => engine.Settings.PhonePosition = p == "Left" ? .2 : p == "Right" ? .8 : .5);
        Combo(panel, "Demo phone mood output", Enum.GetValues<Mood>(), engine.Settings.Mood, m => engine.Settings.Mood = m);
        var caption = Field("Example speech bubble text", "Hello! I'm Bluey, pointing at this spot."); caption.Width = double.NaN; caption.MaxLength = 2000;
        panel.Children.Add(caption);
        var demos = new WrapPanel(); panel.Children.Add(demos);
        Button(demos, "Show speech", () => overlay.Art.Caption(caption.Text));
        Button(demos, "Clear speech", () => overlay.Art.Caption(null));
        Button(demos, "Temporary label", () => overlay.Art.Label("Hello there!"));
        Button(demos, "Visual burst", visuals.DemoBurst);
        Button(demos, "Talk output (Ctrl+Alt+T)", () => engine.TalkUntil = engine.Now + 3);
        if (openConnection != null) Button(demos, "Secure device connection", openConnection);
        if (voicePreview != null) Button(demos, "Offline voice caption preview", voicePreview);
        panel.Children.Add(new TextBlock { Text = "Offline voice simulation: no audio or cloud. Planned permissions — microphone: OFF; cloud processing: OFF; screen capture: OFF. Future live awake sessions may stream audio continuously, even without holding. No API key needed here.", TextWrapping = TextWrapping.Wrap, Margin = new(0, 12, 0, 0) });
        Button(demos, "Exit", Close);
        panel.Children.Add(status); panel.Children.Add(warning);
        if (SettingsStore.Warning is { } w) warning.Text = w;
        SourceInitialized += (_, _) => RegisterShortcuts();
        overlay.Frame += UpdateStatus;
        Closed += (_, _) => { overlay.Frame -= UpdateStatus; foreach (var id in hotkeys.Keys) Native.UnregisterHotKey(hwnd, id); overlay.Close(); Application.Current.Shutdown(); };
    }
    private static TextBox Field(string name, string text)
    {
        var box = new TextBox { Text = text, Width = 80, Margin = new(0, 6, 6, 6), Padding = new(5), ToolTip = name };
        AutomationProperties.SetName(box, name); return box;
    }
    private static void Button(Panel panel, string title, Action action)
    {
        var b = new Button { Content = title, Margin = new(0, 4, 6, 4), Padding = new(8, 5, 8, 5) };
        b.Click += (_, _) => action(); panel.Children.Add(b);
    }
    private void Combo<T>(Panel panel, string name, T[] values, T value, Action<T> action)
    {
        panel.Children.Add(new TextBlock { Text = name, Margin = new(0, 6, 0, 2) });
        var c = new ComboBox { ItemsSource = values, SelectedItem = value, MinHeight = 28 };
        AutomationProperties.SetName(c, name); c.SelectionChanged += (_, _) => { if (c.SelectedItem is T selected) { action(selected); Save(); } };
        panel.Children.Add(c);
    }
    private void Save() { if (!SettingsStore.Save(engine.Settings)) warning.Text = "Settings could not be saved; changes apply for this session."; }
    private void RegisterShortcuts()
    {
        hwnd = new WindowInteropHelper(this).Handle; var errors = new List<string>();
        var bindings = new (char Key, Action Action)[] {
            ('P', overlay.PointAtMouse), ('D', engine.GoHome), ('F', () => { engine.Settings.FollowMouse = !engine.Settings.FollowMouse; engine.GoHome(); Save(); }),
            ('H', () => { engine.Settings.ShowCursor = !engine.Settings.ShowCursor; shown.IsChecked = engine.Settings.ShowCursor; Save(); }),
            ('T', () => engine.TalkUntil = engine.Now + 3) };
        foreach (var (key, action) in bindings)
        {
            var id = (int)key;
            if (Native.RegisterHotKey(hwnd, id, 0x4003, key)) hotkeys.Add(id, action); // MOD_NOREPEAT | CTRL | ALT
            else errors.Add($"Ctrl+Alt+{key}");
        }
        HwndSource.FromHwnd(hwnd)!.AddHook((nint h, int msg, nint w, nint l, ref bool handled) =>
        {
            if (msg == Native.WmHotkey && hotkeys.TryGetValue((int)w, out var action)) { action(); handled = true; }
            return 0;
        });
        if (errors.Count > 0) warning.Text = $"Shortcut unavailable: {string.Join(", ", errors)}. Use the buttons here.";
    }
    private void UpdateStatus()
    {
        if (engine.Now - lastStatus < .2) return; lastStatus = engine.Now;
        var face = engine.Face(engine.Now);
        status.Text = $"{engine.Mode} · {overlay.ActualWidth:0} × {overlay.ActualHeight:0} DIPs · {overlay.Coordinates.DpiX:0} DPI\nPhone output: {face.Mood}, gaze ({face.GazeX:0.00}, {face.GazeY:0.00}), talk {face.Talk:0.00}\nMinimize this control window while comparing animation. Restore it from the taskbar to recover a hidden overlay or exit.";
    }
}
