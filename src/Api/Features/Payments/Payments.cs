using System.Text.Json;
using System.Text.Json.Serialization;
using MdReel.Api.Features.Instrumentation;
using Npgsql;
using Stripe;
using Stripe.Checkout;

namespace MdReel.Api.Features.Payments;

public sealed record PaymentRecord(
    string Id,
    string TenantId,
    string SessionId,
    string Plan,
    int AmountCents,
    string Currency,
    string? FirstUtmSource,
    string? FirstUtmMedium,
    string? FirstUtmCampaign,
    string? FirstUtmTerm,
    string? FirstReferrer,
    string? AbArm);

public sealed record CheckoutRequest(
    string? Plan,
    [property: JsonPropertyName("tenant_id")] string? TenantId);

public sealed record CheckoutSessionResult(string Url, string SessionId);

public sealed record PaymentWebhookEvent(
    string Type,
    string TenantId,
    string Plan,
    string SessionId,
    int AmountCents,
    string Currency,
    string? SubscriptionId = null,
    string? CustomerId = null,
    string? Status = null);

public interface IPaymentStore
{
    Task<PaymentRecord> RecordPaymentAsync(PaymentRecord payment, CancellationToken cancellationToken);

    Task<IReadOnlyList<PaymentRecord>> ListAsync(CancellationToken cancellationToken);
}

public interface IPaymentGateway
{
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(string plan, string tenantId, CancellationToken cancellationToken);

    Task<string> CreatePortalSessionAsync(string tenantId, string? customerId, CancellationToken cancellationToken);

    PaymentWebhookEvent ConstructWebhookEvent(string payload, string? signature);
}

public sealed class PaymentOptions
{
    public string? StripeSecretKey { get; init; }

    public string? StripeWebhookSecret { get; init; }

    public string? ProPriceId { get; init; }

    public string? BusinessPriceId { get; init; }

    public string? StarterPriceId { get; init; }

    public string AppBaseUrl { get; init; } = "http://localhost";

    public bool ShowStarterPlan { get; init; }

    // fake (deterministic, local/CI) | stripe (auto when a secret key is present) | disabled (503).
    public string Mode { get; init; } = "fake";
}

public sealed class InMemoryPaymentStore : IPaymentStore
{
    private readonly List<PaymentRecord> _payments = [];
    private readonly object _gate = new();

    public Task<PaymentRecord> RecordPaymentAsync(PaymentRecord payment, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stored = string.IsNullOrWhiteSpace(payment.Id) ? payment with { Id = $"pay_{Guid.NewGuid():N}" } : payment;
        lock (_gate)
        {
            _payments.Add(stored);
        }

        return Task.FromResult(stored);
    }

    public Task<IReadOnlyList<PaymentRecord>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<PaymentRecord>>(_payments.ToArray());
        }
    }
}

public sealed class PostgresPaymentStore(NpgsqlDataSource dataSource) : IPaymentStore
{
    public async Task<PaymentRecord> RecordPaymentAsync(PaymentRecord payment, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        var stored = string.IsNullOrWhiteSpace(payment.Id) ? payment with { Id = $"pay_{Guid.NewGuid():N}" } : payment;
        await using var command = dataSource.CreateCommand("""
            insert into payments (id, tenant_id, session_id, plan, amount_cents, currency, first_utm_source, first_utm_medium, first_utm_campaign, first_utm_term, first_referrer, ab_arm)
            values (@id, @tenant_id, @session_id, @plan, @amount_cents, @currency, @source, @medium, @campaign, @term, @referrer, @ab_arm)
            """);
        AddPaymentParams(command, stored);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return stored;
    }

    public async Task<IReadOnlyList<PaymentRecord>> ListAsync(CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            select id, tenant_id, session_id, plan, amount_cents, currency, first_utm_source, first_utm_medium, first_utm_campaign, first_utm_term, first_referrer, ab_arm
            from payments
            order by created_at, id
            """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var payments = new List<PaymentRecord>();
        while (await reader.ReadAsync(cancellationToken))
        {
            payments.Add(ReadPayment(reader));
        }

        return payments;
    }

    private static PaymentRecord ReadPayment(NpgsqlDataReader reader) => new(
        reader.GetString(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.GetInt32(4),
        reader.GetString(5),
        reader.IsDBNull(6) ? null : reader.GetString(6),
        reader.IsDBNull(7) ? null : reader.GetString(7),
        reader.IsDBNull(8) ? null : reader.GetString(8),
        reader.IsDBNull(9) ? null : reader.GetString(9),
        reader.IsDBNull(10) ? null : reader.GetString(10),
        reader.IsDBNull(11) ? null : reader.GetString(11));

    private static void AddPaymentParams(NpgsqlCommand command, PaymentRecord payment)
    {
        command.Parameters.AddWithValue("id", payment.Id);
        command.Parameters.AddWithValue("tenant_id", payment.TenantId);
        command.Parameters.AddWithValue("session_id", payment.SessionId);
        command.Parameters.AddWithValue("plan", payment.Plan);
        command.Parameters.AddWithValue("amount_cents", payment.AmountCents);
        command.Parameters.AddWithValue("currency", payment.Currency);
        command.Parameters.AddWithValue("source", (object?)payment.FirstUtmSource ?? DBNull.Value);
        command.Parameters.AddWithValue("medium", (object?)payment.FirstUtmMedium ?? DBNull.Value);
        command.Parameters.AddWithValue("campaign", (object?)payment.FirstUtmCampaign ?? DBNull.Value);
        command.Parameters.AddWithValue("term", (object?)payment.FirstUtmTerm ?? DBNull.Value);
        command.Parameters.AddWithValue("referrer", (object?)payment.FirstReferrer ?? DBNull.Value);
        command.Parameters.AddWithValue("ab_arm", (object?)payment.AbArm ?? DBNull.Value);
    }
}

public sealed class FakePaymentGateway : IPaymentGateway
{
    public const string ValidSignature = "test-signature";

    public Task<CheckoutSessionResult> CreateCheckoutSessionAsync(string plan, string tenantId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sessionId = $"cs_test_{tenantId}_{plan}";
        return Task.FromResult(new CheckoutSessionResult($"https://checkout.test/{sessionId}", sessionId));
    }

    public Task<string> CreatePortalSessionAsync(string tenantId, string? customerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"https://billing.test/portal/{tenantId}");
    }

    public PaymentWebhookEvent ConstructWebhookEvent(string payload, string? signature)
    {
        if (!string.Equals(signature, ValidSignature, StringComparison.Ordinal))
        {
            throw new PaymentWebhookSignatureException("Invalid test signature.");
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var type = GetString(root, "type") ?? "checkout.session.completed";
        var tenantId = GetString(root, "tenant_id") ?? throw new PaymentWebhookSignatureException("Missing tenant_id.");
        var plan = GetString(root, "plan") ?? "pro";
        var sessionId = GetString(root, "sessionId") ?? GetString(root, "session_id") ?? $"cs_test_{tenantId}_{plan}";
        var amount = GetInt(root, "amount_cents") ?? 0;
        var currency = GetString(root, "currency") ?? "eur";
        var subscriptionId = GetString(root, "subscription_id") ?? $"sub_test_{tenantId}";
        var customerId = GetString(root, "customer_id") ?? $"cus_test_{tenantId}";
        var status = GetString(root, "status");
        return new PaymentWebhookEvent(type, tenantId, plan, sessionId, amount, currency, subscriptionId, customerId, status);
    }

    public static object CreateWebhookPayload(string tenantId, string plan, int amountCents = 2900) => new
    {
        type = "checkout.session.completed",
        tenant_id = tenantId,
        plan,
        sessionId = $"cs_test_{tenantId}_{plan}",
        amount_cents = amountCents,
        currency = "eur",
    };

    private static string? GetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

    private static int? GetInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
}

public sealed class StripePaymentGateway(PaymentOptions options) : IPaymentGateway
{
    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(string plan, string tenantId, CancellationToken cancellationToken)
    {
        var priceId = PriceIdFor(plan);
        if (string.IsNullOrWhiteSpace(priceId) || string.IsNullOrWhiteSpace(options.StripeSecretKey))
        {
            throw new InvalidOperationException("Stripe checkout is not configured.");
        }

        var service = new SessionService(new StripeClient(options.StripeSecretKey));
        var session = await service.CreateAsync(new SessionCreateOptions
        {
            Mode = "subscription",
            // Always bill in the price's currency (EUR); never let Stripe localise to the
            // visitor's currency with its ~3.75% conversion fee.
            AdaptivePricing = new SessionAdaptivePricingOptions { Enabled = false },
            SuccessUrl = $"{options.AppBaseUrl.TrimEnd('/')}/billing/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{options.AppBaseUrl.TrimEnd('/')}/billing/cancelled",
            ClientReferenceId = tenantId,
            Metadata = new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId,
                ["plan"] = plan,
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = tenantId,
                    ["plan"] = plan,
                },
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                },
            ],
        }, cancellationToken: cancellationToken);

        return new CheckoutSessionResult(session.Url, session.Id);
    }

    public async Task<string> CreatePortalSessionAsync(string tenantId, string? customerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.StripeSecretKey))
        {
            throw new InvalidOperationException("Stripe portal is not configured.");
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new PaymentCustomerNotFoundException("No Stripe customer is on file for this tenant.");
        }

        var service = new Stripe.BillingPortal.SessionService(new StripeClient(options.StripeSecretKey));
        var session = await service.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = $"{options.AppBaseUrl.TrimEnd('/')}/app",
        }, cancellationToken: cancellationToken);
        return session.Url;
    }

    public PaymentWebhookEvent ConstructWebhookEvent(string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(options.StripeWebhookSecret))
        {
            throw new PaymentWebhookSignatureException("Stripe webhook secret is not configured.");
        }

        var stripeEvent = EventUtility.ConstructEvent(payload, signature, options.StripeWebhookSecret);
        if (stripeEvent.Data.Object is Subscription subscription)
        {
            var subTenant = subscription.Metadata.TryGetValue("tenant_id", out var metadataSubTenant) ? metadataSubTenant : string.Empty;
            var subPlan = subscription.Metadata.TryGetValue("plan", out var metadataSubPlan) ? metadataSubPlan : "pro";
            return new PaymentWebhookEvent(stripeEvent.Type, subTenant, subPlan, subscription.Id, 0, subscription.Currency ?? "eur", subscription.Id, subscription.CustomerId, subscription.Status);
        }

        if (stripeEvent.Data.Object is not Session session)
        {
            return new PaymentWebhookEvent(stripeEvent.Type, string.Empty, string.Empty, string.Empty, 0, "eur");
        }

        var tenantId = session.Metadata.TryGetValue("tenant_id", out var metadataTenant) ? metadataTenant : session.ClientReferenceId;
        var plan = session.Metadata.TryGetValue("plan", out var metadataPlan) ? metadataPlan : "pro";
        var amount = checked((int)(session.AmountTotal ?? 0));
        return new PaymentWebhookEvent(stripeEvent.Type, tenantId, plan, session.Id, amount, session.Currency ?? "eur", session.SubscriptionId, session.CustomerId, "active");
    }

    private string? PriceIdFor(string plan) => plan switch
    {
        "pro" => options.ProPriceId,
        "business" => options.BusinessPriceId,
        "starter" => options.StarterPriceId,
        _ => null,
    };
}

public sealed class DisabledPaymentGateway : IPaymentGateway
{
    public Task<CheckoutSessionResult> CreateCheckoutSessionAsync(string plan, string tenantId, CancellationToken cancellationToken) =>
        throw new PaymentsUnavailableException();

    public Task<string> CreatePortalSessionAsync(string tenantId, string? customerId, CancellationToken cancellationToken) =>
        throw new PaymentsUnavailableException();

    public PaymentWebhookEvent ConstructWebhookEvent(string payload, string? signature) =>
        throw new PaymentsUnavailableException();
}

public sealed class PaymentWebhookSignatureException(string message) : Exception(message);

public sealed class PaymentCustomerNotFoundException(string message) : Exception(message);

public sealed class PaymentsUnavailableException() : Exception("Payments are not configured.");
