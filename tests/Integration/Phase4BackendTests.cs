using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MdReel.Api.Features.Instrumentation;
using MdReel.Api.Features.Payments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MdReel.Tests.Integration;

public sealed class Phase4BackendTests
{
    [Fact]
    public async Task Events_persist_and_signup_keeps_first_touch_attribution()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        using var pageView = await client.PostAsJsonAsync("/api/v1/events", new
        {
            name = "page_view",
            session_id = "sess_1",
            referrer = "https://example.test",
            path = "/",
        });
        Assert.Equal(HttpStatusCode.Accepted, pageView.StatusCode);

        var firstSignup = await SignupAsync(client, "founder@example.test", "google", "https://first.example", "A");
        var secondSignup = await SignupAsync(client, "founder@example.test", "linkedin", "https://second.example", "B");
        Assert.Equal(firstSignup.TenantId, secondSignup.TenantId);
        Assert.Equal(1, firstSignup.TrialCreditHours);
        Assert.Equal(1, secondSignup.TrialCreditHours);

        var tenants = factory.Services.GetRequiredService<ITenantStore>();
        var tenant = await tenants.GetAsync(firstSignup.TenantId, CancellationToken.None);
        Assert.NotNull(tenant);
        Assert.Equal("google", tenant.FirstUtmSource);
        Assert.Equal("https://first.example", tenant.FirstReferrer);
        Assert.Equal("A", tenant.AbArm);
        Assert.Equal(12, tenant.ArchiveHours);
        Assert.Equal(4, tenant.MonthlyHours);

        var events = await factory.Services.GetRequiredService<IEventStore>().ListAsync(CancellationToken.None);
        Assert.Contains(events, x => x.Name == "page_view");
        Assert.Equal(2, events.Count(x => x.Name == "signup"));
    }

    [Fact]
    public async Task Payment_attribution_survives_signup_checkout_and_webhook()
    {
        await using var factory = new ApiFactory();
        using var client = CreateAuthedClient(factory);

        var signup = await SignupAsync(client, "buyer@example.test", "google", "https://first.example", "A");
        using var checkout = await client.PostAsJsonAsync("/api/v1/checkout", new { plan = "pro", tenant_id = signup.TenantId });
        Assert.Equal(HttpStatusCode.Created, checkout.StatusCode);

        using var webhook = await PostWebhookAsync(client, signup.TenantId, "pro");
        Assert.Equal(HttpStatusCode.OK, webhook.StatusCode);

        var payments = await factory.Services.GetRequiredService<IPaymentStore>().ListAsync(CancellationToken.None);
        var payment = Assert.Single(payments);
        Assert.Equal(signup.TenantId, payment.TenantId);
        Assert.Equal("google", payment.FirstUtmSource);
        Assert.Equal("https://first.example", payment.FirstReferrer);
        Assert.Equal("A", payment.AbArm);

        var tenant = await factory.Services.GetRequiredService<ITenantStore>().GetAsync(signup.TenantId, CancellationToken.None);
        Assert.NotNull(tenant);
        Assert.Equal("pro", tenant.Plan);
    }

    [Fact]
    public async Task Checkout_validates_plans_and_emits_clicked_event()
    {
        await using var factory = new ApiFactory();
        using var client = CreateAuthedClient(factory);
        var signup = await SignupAsync(client, "checkout@example.test", "google", "https://first.example", "A");

        foreach (var plan in new[] { "pro", "business" })
        {
            using var response = await client.PostAsJsonAsync("/api/v1/checkout", new { plan, tenant_id = signup.TenantId });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            using var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            Assert.NotNull(json);
            Assert.StartsWith("https://checkout.test/", json.RootElement.GetProperty("url").GetString(), StringComparison.Ordinal);
            Assert.StartsWith("cs_test_", json.RootElement.GetProperty("sessionId").GetString(), StringComparison.Ordinal);
        }

        using var unknown = await client.PostAsJsonAsync("/api/v1/checkout", new { plan = "enterprise", tenant_id = signup.TenantId });
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal("application/problem+json", unknown.Content.Headers.ContentType?.MediaType);

        using var darkStarter = await client.PostAsJsonAsync("/api/v1/checkout", new { plan = "starter", tenant_id = signup.TenantId });
        Assert.Equal(HttpStatusCode.NotFound, darkStarter.StatusCode);

        var events = await factory.Services.GetRequiredService<IEventStore>().ListAsync(CancellationToken.None);
        Assert.Equal(2, events.Count(x => x.Name == "checkout_clicked"));
    }

    [Fact]
    public async Task Starter_checkout_is_allowed_when_flag_is_on()
    {
        await using var factory = new ApiFactory(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<PaymentOptions>();
                services.AddSingleton(new PaymentOptions { ShowStarterPlan = true });
            }));
        using var client = CreateAuthedClient(factory);
        var signup = await SignupAsync(client, "starter@example.test", "google", "https://first.example", "A");

        using var response = await client.PostAsJsonAsync("/api/v1/checkout", new { plan = "starter", tenant_id = signup.TenantId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Stripe_webhook_valid_records_payment_invalid_signature_returns_problem()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var signup = await SignupAsync(client, "webhook@example.test", "google", "https://first.example", "A");

        using var invalid = await client.PostAsJsonAsync("/api/v1/webhooks/stripe", FakePaymentGateway.CreateWebhookPayload(signup.TenantId, "business"));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType?.MediaType);

        using var valid = await PostWebhookAsync(client, signup.TenantId, "business");
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);

        var payment = Assert.Single(await factory.Services.GetRequiredService<IPaymentStore>().ListAsync(CancellationToken.None));
        Assert.Equal("business", payment.Plan);
        var tenant = await factory.Services.GetRequiredService<ITenantStore>().GetAsync(signup.TenantId, CancellationToken.None);
        Assert.NotNull(tenant);
        Assert.Equal("business", tenant.Plan);
    }

    [Fact]
    public async Task Cohort_hour_decay_groups_job_completed_hours_by_signup_week()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var signup = await SignupAsync(client, "cohort@example.test", "google", "https://first.example", "A", "2026-07-01T00:00:00Z");

        await client.PostAsJsonAsync("/api/v1/events", new { name = "job_completed", tenant_id = signup.TenantId, occurred_at = "2026-07-03T00:00:00Z", hours = 1.5 });
        await client.PostAsJsonAsync("/api/v1/events", new { name = "job_completed", tenant_id = signup.TenantId, occurred_at = "2026-07-10T00:00:00Z", duration_sec = 7200 });

        var cohorts = await factory.Services.GetRequiredService<ICohortAnalytics>().GetTenantHoursByWeekAsync(CancellationToken.None);
        Assert.Collection(
            cohorts,
            first =>
            {
                Assert.Equal(signup.TenantId, first.TenantId);
                Assert.Equal(0, first.WeekIndex);
                Assert.Equal(1.5, first.Hours);
            },
            second =>
            {
                Assert.Equal(signup.TenantId, second.TenantId);
                Assert.Equal(1, second.WeekIndex);
                Assert.Equal(2, second.Hours);
            });
    }

    [Fact]
    public async Task Ad_spend_round_trips_through_ledger()
    {
        await using var factory = new ApiFactory();
        var ledger = factory.Services.GetRequiredService<IAdSpendLedger>();
        var entry = await ledger.RecordAsync(new AdSpendEntry(string.Empty, new DateOnly(2026, 7, 16), "google_ads", "launch", "video to markdown", 1234, 12, 345), CancellationToken.None);

        var roundTrip = Assert.Single(await ledger.ListAsync(CancellationToken.None));
        Assert.Equal(entry.Id, roundTrip.Id);
        Assert.Equal("google_ads", roundTrip.Channel);
        Assert.Equal(1234, roundTrip.CostCents);
        Assert.Equal(12, roundTrip.Clicks);
        Assert.Equal(345, roundTrip.Impressions);
    }

    private static HttpClient CreateAuthedClient(ApiFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        return client;
    }

    private static async Task<SignupResponse> SignupAsync(HttpClient client, string email, string utmSource, string referrer, string abArm, string occurredAt = "2026-07-01T00:00:00Z")
    {
        using var response = await client.PostAsJsonAsync("/api/v1/events", new
        {
            name = "signup",
            session_id = $"sess_{email}",
            occurred_at = occurredAt,
            email,
            archive_hours = 12,
            monthly_hours = 4,
            utm_source = utmSource,
            utm_medium = "cpc",
            utm_campaign = "launch",
            utm_term = "video to markdown",
            first_referrer = referrer,
            ab_arm = abArm,
        });
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        return new SignupResponse(
            payload.RootElement.GetProperty("tenant_id").GetString()!,
            payload.RootElement.GetProperty("user_id").GetString()!,
            payload.RootElement.GetProperty("trial_credit_hours").GetDouble());
    }

    private static async Task<HttpResponseMessage> PostWebhookAsync(HttpClient client, string tenantId, string plan)
    {
        var json = JsonSerializer.Serialize(FakePaymentGateway.CreateWebhookPayload(tenantId, plan));
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/webhooks/stripe")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Stripe-Signature", FakePaymentGateway.ValidSignature);
        return await client.SendAsync(request);
    }

    private sealed record SignupResponse(string TenantId, string UserId, double TrialCreditHours);

    private sealed class ApiFactory(Action<IWebHostBuilder>? configure = null) : WebApplicationFactory<MdReel.Api.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            configure?.Invoke(builder);
        }
    }
}
