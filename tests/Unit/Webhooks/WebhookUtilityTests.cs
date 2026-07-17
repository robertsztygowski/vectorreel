using MdReel.Api.Features.Webhooks;

namespace MdReel.Tests.Unit.Webhooks;

public sealed class WebhookUtilityTests
{
    [Fact]
    public void Signature_compute_matches_known_hmac_vector()
    {
        const string payload = "{\"jobId\":\"job_1\",\"status\":\"completed\"}";

        var signature = WebhookSignature.Compute("secret", payload);

        Assert.Equal("sha256=f43cca1a9377df297c25658262a5ab7efc518cb4e7535d03af03f82404a475a0", signature);
    }

    [Fact]
    public void Backoff_grows_exponentially_and_is_capped()
    {
        Assert.Equal(TimeSpan.FromSeconds(10), WebhookBackoff.NextDelay(1));
        Assert.Equal(TimeSpan.FromSeconds(20), WebhookBackoff.NextDelay(2));
        Assert.Equal(TimeSpan.FromSeconds(40), WebhookBackoff.NextDelay(3));
        Assert.Equal(TimeSpan.FromHours(1), WebhookBackoff.NextDelay(20));
    }
}
