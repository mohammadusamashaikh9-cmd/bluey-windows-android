using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Bluey.Core;

namespace Bluey.Windows;
internal static class VectorDrawing
{
    internal static Point P(V2 v) => new(v.X, v.Y);
    internal static Color Color(uint hex, double alpha = 1) => System.Windows.Media.Color.FromArgb((byte)Math.Round(Math.Clamp(alpha, 0, 1) * 255), (byte)(hex >> 16), (byte)(hex >> 8), (byte)hex);
    internal static SolidColorBrush Brush(uint hex, double alpha = 1) { var b = new SolidColorBrush(Color(hex, alpha)); b.Freeze(); return b; }
    internal static StreamGeometry Path(IEnumerable<PathPart> parts)
    {
        var g = new StreamGeometry();
        using (var c = g.Open()) foreach (var p in parts)
        {
            switch (p.Verb)
            {
                case PathVerb.Move: c.BeginFigure(P(p.End), true, true); break;
                case PathVerb.Line: c.LineTo(P(p.End), true, false); break;
                case PathVerb.Arc: c.ArcTo(P(p.End), new(p.Radius, p.Radius), 0, false, SweepDirection.Clockwise, true, false); break;
                case PathVerb.Cubic: c.BezierTo(P(p.C1), P(p.C2), P(p.End), true, false); break;
            }
        }
        g.Freeze(); return g;
    }
    internal static StreamGeometry Polygon(IReadOnlyList<V2> points)
    {
        var g = new StreamGeometry();
        using (var c = g.Open()) if (points.Count > 0) { c.BeginFigure(P(points[0]), true, true); foreach (var p in points.Skip(1)) c.LineTo(P(p), true, false); }
        g.Freeze(); return g;
    }
    internal static readonly FontFamily DisplayFont = new(new Uri("pack://application:,,,/"), "./Fonts/#Fredoka");
    internal static FormattedText Text(string text, double size, Brush color, double dpi, double width = 1000, double height = 800)
    {
        var f = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(DisplayFont, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), size, color, dpi)
        { MaxTextWidth = Math.Max(1, width), MaxTextHeight = Math.Max(1, height), TextAlignment = TextAlignment.Center, Trimming = TextTrimming.CharacterEllipsis };
        return f;
    }
    internal static DrawingImage Body(double side)
    {
        var path = Path(CursorGeometry.Teardrop(side)); var group = new DrawingGroup();
        using (var dc = group.Open())
        {
            var gradient = new LinearGradientBrush { StartPoint = new(.25, .067), EndPoint = new(.75, .933) };
            for (var i = 0; i < 4; i++) gradient.GradientStops.Add(new(Color(Palette.Gradient[i]), Palette.Stops[i]));
            gradient.Freeze(); dc.DrawGeometry(gradient, null, path);
            dc.PushClip(path);
            var shine = new RadialGradientBrush { Center = new(.3, .24), GradientOrigin = new(.3, .24), RadiusX = .38, RadiusY = .38 };
            shine.GradientStops.Add(new(System.Windows.Media.Color.FromArgb(140, 255, 255, 255), 0));
            shine.GradientStops.Add(new(Colors.Transparent, 1)); shine.Freeze();
            dc.DrawRectangle(shine, null, new(0, 0, side, side));
            dc.DrawGeometry(null, new Pen(Brushes.White, 8 * side / 96), path); dc.Pop();
        }
        group.Freeze(); var image = new DrawingImage(group); image.Freeze(); return image;
    }
    internal static Geometry Speech(SpeechPlacement p) => Path(CursorGeometry.Speech(p));
}
