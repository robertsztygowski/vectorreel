using MdReel.Core.Domain;

namespace MdReel.Tests.Unit.Domain;

/// <summary>
/// Pricing is what turns the ledger from a call counter into a measurement. These tests pin the
/// two properties that decide whether a batch's reported €/video-hour means anything: input is
/// priced per modality, and many small calls sum without losing the bill to rounding.
/// </summary>
public sealed class LlmPricingTests
{
    [Fact]
    public void Audio_input_costs_more_than_the_same_number_of_video_tokens()
    {
        var video = LlmPricing.Microcents(new LlmTokenUsage(VideoInputTokens: 1_000_000));
        var audio = LlmPricing.Microcents(new LlmTokenUsage(AudioInputTokens: 1_000_000));

        // The whole reason the split exists. At low media resolution video tokens collapse and
        // audio does not, so a blended input rate would misprice production runs specifically.
        Assert.True(audio > video, $"audio {audio} should exceed video {video}");
    }

    [Fact]
    public void Thinking_tokens_are_billed_as_output()
    {
        var withoutThinking = LlmPricing.Microcents(new LlmTokenUsage(OutputTokens: 1_000));
        var withThinking = LlmPricing.Microcents(new LlmTokenUsage(OutputTokens: 1_000, ThinkingTokens: 1_000));

        Assert.Equal(withoutThinking * 2, withThinking);
    }

    [Fact]
    public void A_single_small_call_rounds_to_zero_cents_but_not_to_zero_microcents()
    {
        // This is the bug the microcent unit exists to prevent: one Stage B call is worth a
        // fraction of a cent, and a 25-video batch is hundreds of them.
        var usage = new LlmTokenUsage(VideoInputTokens: 4_000, OutputTokens: 500);

        Assert.Equal(0, LlmPricing.Cents(usage));
        Assert.True(LlmPricing.Microcents(usage) > 0);
    }

    [Fact]
    public void Summing_microcents_across_many_calls_keeps_the_bill()
    {
        var usage = new LlmTokenUsage(VideoInputTokens: 4_000, OutputTokens: 500);

        var summedFromMicrocents = Enumerable.Range(0, 400).Sum(_ => LlmPricing.Microcents(usage));
        var summedFromRoundedCents = Enumerable.Range(0, 400).Sum(_ => LlmPricing.Cents(usage));

        Assert.Equal(0, summedFromRoundedCents);
        Assert.True(summedFromMicrocents / 10_000m > 1m, "400 calls of this size are worth more than a cent");
    }

    [Fact]
    public void Zero_usage_costs_nothing()
    {
        Assert.Equal(0, LlmPricing.Microcents(new LlmTokenUsage()));
        Assert.Equal(0, LlmPricing.Cents(new LlmTokenUsage()));
    }

    [Fact]
    public void A_realistic_low_resolution_segment_prices_in_the_expected_order_of_magnitude()
    {
        // Ten minutes at the measured low-resolution video rate (~66 tok/s) plus its audio track
        // and a typical structured-output response. This is a sanity rail, not a target: it exists
        // so a pricing change that is wrong by an order of magnitude fails here rather than in a
        // batch report.
        var usage = new LlmTokenUsage(
            VideoInputTokens: 600 * 66,
            AudioInputTokens: 600 * 32,
            TextInputTokens: 800,
            OutputTokens: 6_000,
            ThinkingTokens: 2_000);

        var cents = LlmPricing.Microcents(usage) / 10_000m;

        Assert.InRange(cents, 1m, 20m);
    }
}
