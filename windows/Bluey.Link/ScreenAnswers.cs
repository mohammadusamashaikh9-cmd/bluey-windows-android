using Bluey.Screen;
namespace Bluey.Link;
// Optional application-boundary capability, implemented by the same Gemini adapter, not a second service.
public sealed record ScreenReply(string Answer,string? TargetId);
public interface IScreenAudioAnswers {
    bool ScreenContractVerified{get;}
    Task<ScreenReply> AnswerWithScreen(ReadOnlyMemory<byte> pcm,ApprovedScreen screen,CancellationToken stop);
}
