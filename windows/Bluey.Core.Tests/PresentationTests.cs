using Xunit;
namespace Bluey.Core.Tests;
public sealed class PresentationTests
{
    [Fact] public void PopHasNormalizedInitialVelocityAndSettles()
    {
        Assert.Equal(.55, Presentation.Pop(0, .55, 260, 12, 4), 10);
        const double h = 1e-6;
        var rate = (Presentation.Pop(h, .55, 260, 12, 4) - .55) / h;
        Assert.InRange(rate, 1.7999, 1.8001); // 4 animation-distances/sec * .45 scale-distance.
        Assert.Equal(1, Presentation.Pop(4, .55, 260, 12, 4), 8);
        Assert.Equal(0, Presentation.Speech(1, .22).Opacity);
        Assert.Equal(.85, Presentation.Speech(1, .22).Scale);
        Assert.Equal(0, Presentation.Label(1.4, 1.4).Opacity);
    }
    [Fact] public void EffectsHaveOriginalEndpointsAndFadeDurations()
    {
        var ring = new Effect(EffectKind.Ring, new(10, 20), 30, default, Palette.Berry2, .55, 1, .7);
        var start = Presentation.Sample(ring, 1); Assert.Equal(9, start.Radius); Assert.Equal(.55, start.Opacity);
        var end = Presentation.Sample(ring, 1.7); Assert.Equal(30, end.Radius, 8); Assert.Equal(0, end.Opacity, 8);
        var star = new Effect(EffectKind.Star, default, 5, new(20, -30), Palette.Berry1, 1, 0, .5, 2);
        var last = Presentation.Sample(star, .5); Assert.Equal(new(20, -30), last.Point); Assert.Equal(2, last.Radius); Assert.Equal(0, last.Opacity); Assert.Equal(2, last.Rotation);
    }
    [Theory] [InlineData(false, 98)] [InlineData(true, -18)]
    public void SpeechPathRetainsFourArcsAndCurvedTail(bool up, double tailY)
    {
        var path = CursorGeometry.Speech(new(new(0, 0, 300, 80), default, up, 150, 10));
        Assert.Equal(4, path.Count(p => p.Verb == PathVerb.Arc));
        Assert.Equal(2, path.Count(p => p.Verb == PathVerb.Cubic));
        Assert.Equal(new(160, tailY), path.First(p => p.Verb == PathVerb.Cubic).End);
        Assert.Equal(PathVerb.Close, path[^1].Verb);
    }
}
