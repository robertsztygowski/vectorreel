using MdReel.Core.Domain;

namespace MdReel.Infrastructure.Vertex;

/// <summary>
/// Maps Vertex's <c>usageMetadata</c> onto the billing-shaped <see cref="LlmTokenUsage"/>, so both
/// Stage B and Stage C price their calls the same way (CLAUDE.md rule 6).
/// </summary>
internal static class VertexUsage
{
    public static LlmTokenUsage? ToTokenUsage(VertexUsageMetadata? usage)
    {
        if (usage is null)
        {
            return null;
        }

        var text = 0;
        var video = 0;
        var audio = 0;

        foreach (var detail in usage.PromptTokensDetails ?? [])
        {
            switch (detail.Modality?.ToUpperInvariant())
            {
                case "VIDEO":
                    video += detail.TokenCount;
                    break;
                case "AUDIO":
                    audio += detail.TokenCount;
                    break;
                default:
                    // TEXT, IMAGE and DOCUMENT share the text price.
                    text += detail.TokenCount;
                    break;
            }
        }

        // No breakdown reported: fall back to the total at the text rate. That under-prices
        // audio-heavy calls, so the resulting figure is a floor, not a measurement.
        if (text + video + audio == 0)
        {
            text = usage.PromptTokenCount;
        }

        return new LlmTokenUsage(
            TextInputTokens: text,
            VideoInputTokens: video,
            AudioInputTokens: audio,
            OutputTokens: usage.CandidatesTokenCount,
            ThinkingTokens: usage.ThoughtsTokenCount);
    }
}
