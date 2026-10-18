using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Bluey.Core;
using static Bluey.Windows.VectorDrawing;

namespace Bluey.Windows;
internal sealed class DrawSurface(Action<DrawingContext> draw) : FrameworkElement
{
    protected override void OnRender(DrawingContext dc) => draw(dc);
}
internal sealed class OverlayArt : Canvas
{
    private readonly CursorEngine engine;
    private readonly VisualState state;
    private readonly Canvas character = new() { IsHitTestVisible = false };
    private readonly Image body = new() { IsHitTestVisible = false, Stretch = Stretch.None };
    private readonly DrawSurface back, front, eyes;
    private readonly TransformGroup transform = new();
    private readonly ScaleTransform squish = new();
    private readonly RotateTransform rotation = new();
    private readonly TranslateTransform position = new();
    private (double Side, bool Glow)? builtArt;
    private string? caption, label, fadingCaption;
    private double speechBorn, labelBorn, labelLife, fadeBorn;
    private FormattedText? captionText, labelText;
    private Size2 captionSize;
    private (string? Text, double Width, double Height, double Dpi)? textKey;
    internal Rect2? SpeechTarget { get; set; }
    internal OverlayArt(CursorEngine engine, VisualState state)
    {
        this.engine = engine; this.state = state;
        ClipToBounds = true; IsHitTestVisible = false;
        back = new(DrawBack); front = new(DrawFront); eyes = new(DrawEyes);
        transform.Children.Add(squish); transform.Children.Add(rotation); transform.Children.Add(position);
        character.RenderTransform = transform;
        character.Children.Add(body); character.Children.Add(eyes);
        Children.Add(back); Children.Add(character); Children.Add(front);
        SizeChanged += (_, _) => { back.Width = front.Width = ActualWidth; back.Height = front.Height = ActualHeight; textKey = null; };
    }
    internal void Caption(string? text)
    {
        text = string.IsNullOrWhiteSpace(text) ? null : text;
        if (text == caption) return;
        if (text == null) { fadingCaption = caption; fadeBorn = engine.Now; }
        else if (caption == null) { speechBorn = engine.Now; fadingCaption = null; }
        caption = text; textKey = null;
    }
    internal void Label(string text, double life = 1.4)
    {
        label = text; labelBorn = engine.Now; labelLife = life;
        labelText = Text(text, 19, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip, Math.Max(1, ActualWidth - 52), 80);
    }
    internal void Advance()
    {
        state.Step();
        if (builtArt != (engine.Settings.Size, engine.Settings.Glow))
        {
            var s = engine.Settings.Size; var k = s / 96;
            body.Source = Body(s); body.Width = body.Height = s;
            body.Effect = new DropShadowEffect { Color = Color(engine.Settings.Glow ? Palette.Berry2 : Palette.Berry4),
                BlurRadius = (engine.Settings.Glow ? 30 : 16) * k, Opacity = engine.Settings.Glow ? .7 : .45, ShadowDepth = 0, RenderingBias = RenderingBias.Quality };
            eyes.Width = eyes.Height = character.Width = character.Height = s;
            builtArt = (s, engine.Settings.Glow);
        }
        character.Opacity = engine.Settings.ShowCursor ? engine.Opacity : 0;
        squish.ScaleX = squish.ScaleY = engine.PressScale(engine.Now);
        rotation.Angle = engine.Angle * 180 / Math.PI;
        position.X = engine.Tip.X; position.Y = engine.Tip.Y + engine.Hover(engine.Now);
        eyes.InvalidateVisual(); back.InvalidateVisual(); front.InvalidateVisual();
    }
    private void DrawEyes(DrawingContext dc)
    {
        var k = engine.Settings.Size / 96;
        for (var i = 0; i < 2; i++)
        {
            var center = CursorGeometry.EyeCenter(i, engine.Settings.Size);
            dc.PushTransform(new ScaleTransform(1, engine.Now < engine.BlinkUntil ? .12 : 1, center.X, center.Y));
            dc.DrawEllipse(Brushes.White, null, P(center), 9 * k, 9 * k);
            dc.DrawEllipse(Brush(Palette.Ink), null, P(center + state.Look * (3.4 * k)), 4.5 * k, 4.5 * k);
            dc.Pop();
        }
    }
    private void DrawBack(DrawingContext dc)
    {
        if (!engine.Settings.ShowCursor) return;
        if (state.Comet.Length > 0)
        {
            dc.PushOpacity(Math.Clamp(state.CometOpacity, 0, 1)); dc.DrawGeometry(Brush(Palette.Berry2), null, Polygon(state.Comet)); dc.Pop();
        }
        if (state.String is { } str)
        {
            var g = new StreamGeometry(); using (var c = g.Open()) { c.BeginFigure(P(str.From), false, false); c.QuadraticBezierTo(P(str.Control), P(str.To), true, false); }
            var pen = new Pen(Brush(Palette.Berry1, .7), 2.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            dc.PushOpacity(engine.Opacity); dc.DrawGeometry(null, pen, g); dc.Pop();
        }
        foreach (var e in state.Effects)
        {
            var sample = Presentation.Sample(e, engine.Now);
            if (e.Kind == EffectKind.Ring)
            {
                dc.PushOpacity(sample.Opacity); dc.DrawEllipse(null, new(Brush(e.Color), 3.5), P(sample.Point), sample.Radius, sample.Radius); dc.Pop();
            }
            else
            {
                var at = sample.Point;
                dc.PushOpacity(sample.Opacity);
                dc.PushTransform(new TranslateTransform(at.X, at.Y));
                // Core Animation spin is y-up, so negate for WPF.
                dc.PushTransform(new RotateTransform(-sample.Rotation * 180 / Math.PI));
                dc.DrawGeometry(Brush(e.Color), null, Polygon(CursorGeometry.Star(sample.Radius)));
                dc.Pop(); dc.Pop(); dc.Pop();
            }
        }
    }
    private void DrawFront(DrawingContext dc)
    {
        if (!engine.Settings.ShowCursor) return;
        DrawLabel(dc); DrawSpeech(dc);
    }
    private void DrawLabel(DrawingContext dc)
    {
        var age = engine.Now - labelBorn;
        if (label == null || age > labelLife || labelText == null) return;
        var size = new Size2(Math.Ceiling(labelText.Width) + 28, Math.Ceiling(labelText.Height) + 14);
        var box = CaptionLayout.Label(size, engine.Bounds, engine.Tip, engine.Settings.Size, engine.Opacity, engine.Settings.PhonePosition, age);
        var (pop, opacity) = Presentation.Label(age, labelLife);
        dc.PushOpacity(opacity);
        dc.PushTransform(new ScaleTransform(pop, pop, box.MidX, box.Y + box.Height / 2));
        dc.DrawRoundedRectangle(Brush(Palette.Ink, .92), new(Brush(Palette.Berry2, .9), 2), new(box.X, box.Y, box.Width, box.Height), 14, 14);
        dc.DrawText(labelText, new(box.X + 14, box.Y + 7)); dc.Pop(); dc.Pop();
    }
    private void DrawSpeech(DrawingContext dc)
    {
        var text = caption ?? fadingCaption;
        if (text == null || (caption == null && engine.Now - fadeBorn >= .22)) return;
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var key = (text, ActualWidth, ActualHeight, dpi);
        if (textKey != key)
        {
            var width = Math.Max(1, Math.Min(CaptionLayout.MaxTextWidth(text.Length, engine.Bounds), ActualWidth - 76));
            captionText = Text(text, CaptionLayout.FontSize(text.Length), Brush(Palette.Ink), dpi, width, Math.Max(1, Math.Min(800, ActualHeight - 130)));
            // FormattedText uses MaxTextWidth for alignment; keep its centered text box inside the padding.
            captionSize = new(Math.Min(ActualWidth - 28, Math.Max(70, Math.Ceiling(captionText.Width) + 48)), Math.Ceiling(captionText.Height) + 30);
            captionText.MaxTextWidth = Math.Max(1, captionSize.Width - 48);
            textKey = key;
        }
        var age = engine.Now - speechBorn;
        var p = CaptionLayout.Place(captionSize, engine.Bounds, engine.Tip, engine.Settings.Size, engine.Opacity > .1 && !engine.IsHome, engine.Settings.PhonePosition, SpeechTarget, age);
        var (pop, opacity) = Presentation.Speech(age, caption == null ? engine.Now - fadeBorn : null);
        dc.PushOpacity(Math.Clamp(opacity, 0, 1));
        dc.PushTransform(new ScaleTransform(pop, pop, p.Box.MidX, p.Box.Y + p.Box.Height / 2));
        dc.PushTransform(new TranslateTransform(p.Box.X, p.Box.Y));
        var g = Speech(p); var fill = new LinearGradientBrush(Colors.White, Color(0xEEF0FF), 90);
        dc.DrawGeometry(fill, new(Brush(Palette.Berry2), 2.5) { LineJoin = PenLineJoin.Round }, g);
        dc.DrawText(captionText!, new(24, 14)); dc.Pop(); dc.Pop(); dc.Pop();
    }
}
