using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MdReel.Api.Features.Auth;
using MdReel.Api.Features.Instrumentation;
using MdReel.Api.Features.Payments;
using MdReel.Api.Features.Webhooks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MdReel.Tests.Integration;

/// <summary>
/// The Phase 4 funnel against the REAL Postgres stores — the METRICS.md §6.2 source of truth.
/// <see cref="Phase4BackendTests"/> proves the flows against the in-memory stores; this class
/// proves the Postgres implementations persist the same facts. Requires docker compose
/// (TESTING.md): a fresh database is created per test run and dropped afterwards, so nothing
/// leaks into the dev `vectorreel` database.
/// </summary>
[Trait("Category", "RequiresDocker")]
public sealed class PostgresBackedApiTests : IClassFixture<PostgresBackedApiTests.PostgresDatabaseFixture>
{
    private readonly PostgresDatabaseFixture _fixture;

    public PostgresBackedApiTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Signup_attribution_survives_to_payment_in_postgres()
    {
        await using var factory = _fixture.CreateFactory();
        using var client = CreateAuthedClient(factory);

        var email = UniqueEmail("buyer");
        var signup = await SignupAsync(client, email, "google", "https://first.example", "A");

        using var checkout = await client.PostAsJsonAsync("/api/v1/checkout", new { plan = "pro", tenant_id = signup.TenantId });
        Assert.Equal(HttpStatusCode.Created, checkout.StatusCode);

        using var webhook = await PostWebhookAsync(client, signup.TenantId, "pro");
        Assert.Equal(HttpStatusCode.OK, webhook.StatusCode);

        var paymentStore = factory.Services.GetRequiredService<IPaymentStore>();
        Assert.IsType<PostgresPaymentStore>(paymentStore);
        var payment = Assert.Single(
            await paymentStore.ListAsync(CancellationToken.None), x => x.TenantId == signup.TenantId);
        Assert.Equal("google", payment.FirstUtmSource);
        Assert.Equal("https://first.example", payment.FirstReferrer);
        Assert.Equal("A", payment.AbArm);

        var tenantStore = factory.Services.GetRequiredService<ITenantStore>();
        Assert.IsType<PostgresTenantStore>(tenantStore);
        var tenant = await tenantStore.GetAsync(signup.TenantId, CancellationToken.None);
        Assert.NotNull(tenant);
        Assert.Equal("pro", tenant.Plan);
        Assert.Equal("google", tenant.FirstUtmSource);

        var subscriptionStore = factory.Services.GetRequiredService<ISubscriptionStore>();
        Assert.IsType<PostgresSubscriptionStore>(subscriptionStore);
        var subscription = await subscriptionStore.GetByTenantAsync(signup.TenantId, CancellationToken.None);
        Assert.NotNull(subscription);
        Assert.Equal("pro", subscription.Plan);
        Assert.Equal("active", subscription.Status);
    }

    [Fact]
    public async Task Events_and_signup_rows_land_in_postgres_tables()
    {
        await using var factory = _fixture.CreateFactory();
        using var client = factory.CreateClient();

        var sessionId = $"sess_{Guid.NewGuid():N}";
        using var pageView = await client.PostAsJsonAsync("/api/v1/events", new
        {
            name = "page_view",
            session_id = sessionId,
            referrer = "https://example.test",
            path = "/",
        });
        Assert.Equal(HttpStatusCode.Accepted, pageView.StatusCode);

        var email = UniqueEmail("events");
        var first = await SignupAsync(client, email, "linkedin", "https://li.example", "B");
        var second = await SignupAsync(client, email, "google", "https://second.example", "A");
        Assert.Equal(first.TenantId, second.TenantId);

        // Assert against the tables themselves, not the store abstraction — this is the check the
        // E2E runbook (TESTING.md) teaches an agent to run by hand.
        await using var dataSource = NpgsqlDataSource.Create(_fixture.ConnectionString);
        await using (var command = dataSource.CreateCommand("select count(*) from events where session_id = @s and name = 'page_view'"))
        {
            command.Parameters.AddWithValue("s", sessionId);
            Assert.Equal(1L, (long)(await command.ExecuteScalarAsync())!);
        }

        await using (var command = dataSource.CreateCommand("select first_utm_source, first_referrer, ab_arm from tenants where id = @id"))
        {
            command.Parameters.AddWithValue("id", first.TenantId);
            await using var reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal("linkedin", reader.GetString(0));
            Assert.Equal("https://li.example", reader.GetString(1));
            Assert.Equal("B", reader.GetString(2));
        }
    }

    [Fact]
    public async Task Ad_spend_and_cohorts_round_trip_through_postgres()
    {
        await using var factory = _fixture.CreateFactory();
        using var client = factory.CreateClient();

        var ledger = factory.Services.GetRequiredService<IAdSpendLedger>();
        Assert.IsType<PostgresAdSpendLedger>(ledger);
        var entry = await ledger.RecordAsync(
            new AdSpendEntry(string.Empty, new DateOnly(2026, 7, 17), "google_ads", "launch", "video to markdown", 2222, 21, 543),
            CancellationToken.None);
        Assert.Contains(await ledger.ListAsync(CancellationToken.None), x => x.Id == entry.Id && x.CostCents == 2222);

        var email = UniqueEmail("cohort");
        var signup = await SignupAsync(client, email, "google", "https://first.example", "A", "2026-07-01T00:00:00Z");
        await client.PostAsJsonAsync("/api/v1/events", new { name = "job_completed", tenant_id = signup.TenantId, occurred_at = "2026-07-02T00:00:00Z", duration_sec = 5400 });

        var cohorts = factory.Services.GetRequiredService<ICohortAnalytics>();
        Assert.IsType<PostgresCohortAnalytics>(cohorts);
        var row = Assert.Single(
            await cohorts.GetTenantHoursByWeekAsync(CancellationToken.None), x => x.TenantId == signup.TenantId);
        Assert.Equal(0, row.WeekIndex);
        Assert.Equal(1.5, row.Hours);
    }

    [Fact]
    public async Task Webhook_deliveries_round_trip_through_postgres()
    {
        await using var factory = _fixture.CreateFactory();

        var store = factory.Services.GetRequiredService<IWebhookDeliveryStore>();
        Assert.IsType<PostgresWebhookDeliveryStore>(store);
        var created = await store.EnqueueAsync(new WebhookDeliveryRecord(
            string.Empty,
            "tenant_arbitrary",
            "job.completed",
            "https://customer.example/webhook",
            "{\"jobId\":\"job_1\"}",
            "secret"), CancellationToken.None);

        var loaded = await store.GetAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal("pending", loaded.Status);
        Assert.Equal(0, loaded.Attempts);
        Assert.Equal("tenant_arbitrary", loaded.TenantId);
        Assert.Equal("job.completed", loaded.EventType);
    }

    [Fact]
    public async Task Upload_records_persist_and_mark_stored_in_postgres()
    {
        await using var factory = _fixture.CreateFactory();
        using var client = CreateAuthedClient(factory);

        using var create = await client.PostAsJsonAsync("/api/v1/uploads", new { contentType = "video/mp4" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createPayload = await create.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(createPayload);
        var uploadId = createPayload.RootElement.GetProperty("uploadId").GetString();
        var uploadUrl = createPayload.RootElement.GetProperty("uploadUrl").GetString();
        Assert.Equal("api", createPayload.RootElement.GetProperty("storage").GetString());
        Assert.False(string.IsNullOrWhiteSpace(uploadId));
        Assert.False(string.IsNullOrWhiteSpace(uploadUrl));

        using var bytes = new ByteArrayContent(Encoding.UTF8.GetBytes("not a real video"));
        using var put = await client.PutAsync(uploadUrl, bytes);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        await using var dataSource = NpgsqlDataSource.Create(_fixture.ConnectionString);
        await using var command = dataSource.CreateCommand("""
            select storage_mode, content_type, stored, signature is not null, local_path is not null
            from private_uploads
            where id = @id
            """);
        command.Parameters.AddWithValue("id", uploadId!);
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("api", reader.GetString(0));
        Assert.Equal("video/mp4", reader.GetString(1));
        Assert.True(reader.GetBoolean(2));
        Assert.True(reader.GetBoolean(3));
        Assert.True(reader.GetBoolean(4));
    }

    [Fact]
    public async Task Register_provisions_tenant_and_trial_credit_then_me_returns_session()
    {
        IdentitySchema.ResetForTests();
        await using var factory = _fixture.CreateFactory();
        using var client = factory.CreateClient();

        var email = UniqueEmail("member");
        using var register = await client.PostAsJsonAsync("/api/v1/auth/signup", new
        {
            email,
            password = "Str0ng-Passw0rd!",
            archiveHours = 12,
            monthlyHours = 4,
            utmSource = "google",
            firstReferrer = "https://first.example",
            abArm = "A",
        });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        using var registerBody = await register.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(registerBody);
        var tenantId = registerBody.RootElement.GetProperty("tenant_id").GetString()!;
        Assert.Equal(1, registerBody.RootElement.GetProperty("trial_credit_hours").GetDouble());

        // The auth cookie set by /signup authorizes /me (no bearer header).
        using var me = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        using var meBody = await me.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(meBody);
        Assert.Equal(email, meBody.RootElement.GetProperty("email").GetString());
        Assert.Equal(tenantId, meBody.RootElement.GetProperty("tenant_id").GetString());

        // Tenant creation flowed through ITenantStore — the single source of truth — so the tenant,
        // its N33 trial credit, and the identity user all land in Postgres.
        await using var dataSource = NpgsqlDataSource.Create(_fixture.ConnectionString);
        await using (var command = dataSource.CreateCommand("select \"UserName\", \"TenantId\" from \"AspNetUsers\" where \"Email\" = @e"))
        {
            command.Parameters.AddWithValue("e", email);
            await using var reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(email, reader.GetString(0));
            Assert.Equal(tenantId, reader.GetString(1));
        }

        await using (var command = dataSource.CreateCommand("select trial_credit_hours, first_utm_source from tenants where id = @id"))
        {
            command.Parameters.AddWithValue("id", tenantId);
            await using var reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(1d, reader.GetDouble(0));
            Assert.Equal("google", reader.GetString(1));
        }

        await using (var command = dataSource.CreateCommand("select count(*) from usage_ledger where tenant_id = @id and step = 'trial_credit'"))
        {
            command.Parameters.AddWithValue("id", tenantId);
            Assert.Equal(1L, (long)(await command.ExecuteScalarAsync())!);
        }
    }

    [Fact]
    public async Task Login_after_logout_restores_the_session_cookie()
    {
        IdentitySchema.ResetForTests();
        await using var factory = _fixture.CreateFactory();
        using var client = factory.CreateClient();

        var email = UniqueEmail("returning");
        const string password = "Str0ng-Passw0rd!";
        using (var register = await client.PostAsJsonAsync("/api/v1/auth/signup", new { email, password }))
        {
            Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        }

        using (var logout = await client.PostAsync("/api/v1/auth/logout", content: null))
        {
            Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        }

        using (var meAfterLogout = await client.GetAsync("/api/v1/auth/me"))
        {
            Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
        }

        using (var login = await client.PostAsJsonAsync("/api/v1/auth/login?useCookies=true", new { email, password }))
        {
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        }

        using var me = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Duplicate_registration_is_rejected()
    {
        IdentitySchema.ResetForTests();
        await using var factory = _fixture.CreateFactory();
        using var client = factory.CreateClient();

        var email = UniqueEmail("dupe");
        using (var first = await client.PostAsJsonAsync("/api/v1/auth/signup", new { email, password = "Str0ng-Passw0rd!" }))
        {
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        }

        using var second = await client.PostAsJsonAsync("/api/v1/auth/signup", new { email, password = "An0ther-Passw0rd!" });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    private static string UniqueEmail(string prefix) => $"{prefix}+{Guid.NewGuid():N}@example.test";

    private static HttpClient CreateAuthedClient(WebApplicationFactory<MdReel.Api.Program> factory)
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
            payload.RootElement.GetProperty("user_id").GetString()!);
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

    private sealed record SignupResponse(string TenantId, string UserId);

    /// <summary>
    /// Creates one throwaway database on the compose Postgres for the whole test class and drops
    /// it afterwards. Fails with an actionable message when compose is down. One database per
    /// process on purpose: <see cref="PostgresSchema"/> ensures the schema once per process.
    /// </summary>
    public sealed class PostgresDatabaseFixture : IAsyncLifetime
    {
        private const string AdminConnectionString =
            "Host=localhost;Port=5432;Database=vectorreel;Username=dev;Password=dev;Timeout=3";

        private readonly string _databaseName = $"mdreel_it_{Guid.NewGuid():N}";

        public string ConnectionString =>
            $"Host=localhost;Port=5432;Database={_databaseName};Username=dev;Password=dev";

        public WebApplicationFactory<MdReel.Api.Program> CreateFactory() =>
            new PostgresApiFactory(ConnectionString);

        public async Task InitializeAsync()
        {
            try
            {
                await using var dataSource = NpgsqlDataSource.Create(AdminConnectionString);
                await using var command = dataSource.CreateCommand($"create database \"{_databaseName}\"");
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex) when (ex is NpgsqlException or TimeoutException)
            {
                throw new InvalidOperationException(
                    "Postgres is not reachable on localhost:5432 — run `docker compose up -d` first (TESTING.md).", ex);
            }
        }

        public async Task DisposeAsync()
        {
            await using var dataSource = NpgsqlDataSource.Create(AdminConnectionString);
            await using var command = dataSource.CreateCommand($"drop database if exists \"{_databaseName}\" with (force)");
            await command.ExecuteNonQueryAsync();
        }

        private sealed class PostgresApiFactory(string connectionString) : WebApplicationFactory<MdReel.Api.Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseSetting("POSTGRES_CONNECTION", connectionString);
            }
        }
    }
}
