using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using MdReel.Api.Features.Instrumentation;
using MdReel.Core.Providers;
using Npgsql;

namespace MdReel.Api.Features.Webhooks;

public sealed record WebhookDeliveryRecord(
    string Id,
    string TenantId,
    string EventType,
    string TargetUrl,
    string Payload,
    string Secret,
    string Status = "pending",
    int Attempts = 0,
    int MaxAttempts = 5,
    DateTimeOffset? NextAttemptAt = null,
    string? LastError = null,
    int? ResponseStatus = null,
    DateTimeOffset? CreatedAt = null,
    DateTimeOffset? UpdatedAt = null);

public interface IWebhookDeliveryStore
{
    Task<WebhookDeliveryRecord> EnqueueAsync(WebhookDeliveryRecord draft, CancellationToken cancellationToken);

    Task<WebhookDeliveryRecord?> GetAsync(string id, CancellationToken cancellationToken);

    Task MarkDeliveredAsync(string id, int responseStatus, CancellationToken cancellationToken);

    Task MarkAttemptFailedAsync(
        string id,
        int attempts,
        DateTimeOffset nextAttemptAt,
        string status,
        string errorMessage,
        int? responseStatus,
        CancellationToken cancellationToken);
}

public sealed class InMemoryWebhookDeliveryStore : IWebhookDeliveryStore
{
    private readonly Dictionary<string, WebhookDeliveryRecord> _deliveries = [];
    private readonly object _gate = new();

    public Task<WebhookDeliveryRecord> EnqueueAsync(WebhookDeliveryRecord draft, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        var record = draft with
        {
            Id = string.IsNullOrWhiteSpace(draft.Id) ? $"wd_{Guid.NewGuid():N}" : draft.Id,
            Status = string.IsNullOrWhiteSpace(draft.Status) ? "pending" : draft.Status,
            NextAttemptAt = draft.NextAttemptAt ?? now,
            CreatedAt = draft.CreatedAt ?? now,
            UpdatedAt = draft.UpdatedAt ?? now,
        };

        lock (_gate)
        {
            _deliveries[record.Id] = record;
        }

        return Task.FromResult(record);
    }

    public Task<WebhookDeliveryRecord?> GetAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            return Task.FromResult(_deliveries.TryGetValue(id, out var record) ? record : null);
        }
    }

    public Task MarkDeliveredAsync(string id, int responseStatus, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_deliveries.TryGetValue(id, out var record))
            {
                _deliveries[id] = record with
                {
                    Status = "delivered",
                    ResponseStatus = responseStatus,
                    LastError = null,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };
            }
        }

        return Task.CompletedTask;
    }

    public Task MarkAttemptFailedAsync(
        string id,
        int attempts,
        DateTimeOffset nextAttemptAt,
        string status,
        string errorMessage,
        int? responseStatus,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_deliveries.TryGetValue(id, out var record))
            {
                _deliveries[id] = record with
                {
                    Attempts = attempts,
                    NextAttemptAt = nextAttemptAt,
                    Status = status,
                    LastError = errorMessage,
                    ResponseStatus = responseStatus,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };
            }
        }

        return Task.CompletedTask;
    }
}

public sealed class PostgresWebhookDeliveryStore(NpgsqlDataSource dataSource) : IWebhookDeliveryStore
{
    public async Task<WebhookDeliveryRecord> EnqueueAsync(WebhookDeliveryRecord draft, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var record = draft with
        {
            Id = string.IsNullOrWhiteSpace(draft.Id) ? $"wd_{Guid.NewGuid():N}" : draft.Id,
            Status = string.IsNullOrWhiteSpace(draft.Status) ? "pending" : draft.Status,
            NextAttemptAt = draft.NextAttemptAt ?? now,
            CreatedAt = draft.CreatedAt ?? now,
            UpdatedAt = draft.UpdatedAt ?? now,
        };

        await using var command = dataSource.CreateCommand("""
            insert into webhook_deliveries (
                id, tenant_id, event_type, target_url, payload, secret, status, attempts,
                max_attempts, next_attempt_at, last_error, response_status, created_at, updated_at)
            values (
                @id, @tenant_id, @event_type, @target_url, @payload, @secret, @status, @attempts,
                @max_attempts, @next_attempt_at, @last_error, @response_status, @created_at, @updated_at)
            """);
        AddRecordParameters(command, record);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return record;
    }

    public async Task<WebhookDeliveryRecord?> GetAsync(string id, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            select id, tenant_id, event_type, target_url, payload, secret, status, attempts,
                max_attempts, next_attempt_at, last_error, response_status, created_at, updated_at
            from webhook_deliveries
            where id = @id
            """);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadRecord(reader);
    }

    public async Task MarkDeliveredAsync(string id, int responseStatus, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            update webhook_deliveries
            set status = 'delivered', response_status = @response_status, last_error = null, updated_at = now()
            where id = @id
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("response_status", responseStatus);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkAttemptFailedAsync(
        string id,
        int attempts,
        DateTimeOffset nextAttemptAt,
        string status,
        string errorMessage,
        int? responseStatus,
        CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            update webhook_deliveries
            set attempts = @attempts,
                next_attempt_at = @next_attempt_at,
                status = @status,
                last_error = @last_error,
                response_status = @response_status,
                updated_at = now()
            where id = @id
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("attempts", attempts);
        command.Parameters.AddWithValue("next_attempt_at", nextAttemptAt);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("last_error", errorMessage);
        command.Parameters.AddWithValue("response_status", (object?)responseStatus ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddRecordParameters(NpgsqlCommand command, WebhookDeliveryRecord record)
    {
        command.Parameters.AddWithValue("id", record.Id);
        command.Parameters.AddWithValue("tenant_id", record.TenantId);
        command.Parameters.AddWithValue("event_type", record.EventType);
        command.Parameters.AddWithValue("target_url", record.TargetUrl);
        command.Parameters.AddWithValue("payload", record.Payload);
        command.Parameters.AddWithValue("secret", record.Secret);
        command.Parameters.AddWithValue("status", record.Status);
        command.Parameters.AddWithValue("attempts", record.Attempts);
        command.Parameters.AddWithValue("max_attempts", record.MaxAttempts);
        command.Parameters.AddWithValue("next_attempt_at", record.NextAttemptAt!.Value);
        command.Parameters.AddWithValue("last_error", (object?)record.LastError ?? DBNull.Value);
        command.Parameters.AddWithValue("response_status", (object?)record.ResponseStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("created_at", record.CreatedAt!.Value);
        command.Parameters.AddWithValue("updated_at", record.UpdatedAt!.Value);
    }

    private static WebhookDeliveryRecord ReadRecord(NpgsqlDataReader reader) => new(
        reader.GetString(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.GetString(4),
        reader.GetString(5),
        reader.GetString(6),
        reader.GetInt32(7),
        reader.GetInt32(8),
        reader.GetFieldValue<DateTimeOffset>(9),
        reader.IsDBNull(10) ? null : reader.GetString(10),
        reader.IsDBNull(11) ? null : reader.GetInt32(11),
        reader.GetFieldValue<DateTimeOffset>(12),
        reader.GetFieldValue<DateTimeOffset>(13));
}

public static class WebhookSignature
{
    public static string Compute(string secret, string payload)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var bytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(key, bytes);
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public static class WebhookBackoff
{
    private static readonly TimeSpan MaximumDelay = TimeSpan.FromHours(1);

    public static TimeSpan NextDelay(int attempt)
    {
        if (attempt <= 0)
        {
            return TimeSpan.FromSeconds(10);
        }

        var multiplier = Math.Pow(2, attempt - 1);
        var seconds = Math.Min(10 * multiplier, MaximumDelay.TotalSeconds);
        return TimeSpan.FromSeconds(seconds);
    }
}

public sealed record WebhookSendResult(bool Success, int? StatusCode, string? Error);

public interface IWebhookSender
{
    Task<WebhookSendResult> SendAsync(WebhookDeliveryRecord delivery, CancellationToken cancellationToken);
}

public sealed class HttpWebhookSender(HttpClient http) : IWebhookSender
{
    public async Task<WebhookSendResult> SendAsync(WebhookDeliveryRecord delivery, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, delivery.TargetUrl)
        {
            Content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Mdreel-Signature", WebhookSignature.Compute(delivery.Secret, delivery.Payload));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            var status = (int)response.StatusCode;
            return response.IsSuccessStatusCode
                ? new WebhookSendResult(true, status, null)
                : new WebhookSendResult(false, status, $"HTTP {status.ToString(CultureInfo.InvariantCulture)}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new WebhookSendResult(false, null, ex.Message);
        }
    }
}

public sealed class WebhookDeliveryService(IWebhookDeliveryStore store, IWebhookSender sender)
{
    public Task<WebhookDeliveryRecord> EnqueueAsync(
        string tenantId,
        string eventType,
        string targetUrl,
        string secret,
        string payload,
        CancellationToken cancellationToken) =>
        store.EnqueueAsync(new WebhookDeliveryRecord(
            string.Empty,
            tenantId,
            eventType,
            targetUrl,
            payload,
            secret,
            "pending",
            0,
            5,
            DateTimeOffset.UtcNow), cancellationToken);

    public async Task<WebhookDeliveryRecord?> AttemptAsync(string deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await store.GetAsync(deliveryId, cancellationToken);
        if (delivery is null || delivery.Status is "delivered" or "failed")
        {
            return delivery;
        }

        var result = await sender.SendAsync(delivery, cancellationToken);
        if (result.Success)
        {
            await store.MarkDeliveredAsync(delivery.Id, result.StatusCode ?? StatusCodes.Status200OK, cancellationToken);
            return await store.GetAsync(delivery.Id, cancellationToken);
        }

        var attempts = delivery.Attempts + 1;
        var status = attempts >= delivery.MaxAttempts ? "failed" : "pending";
        var nextAttemptAt = DateTimeOffset.UtcNow + WebhookBackoff.NextDelay(attempts);
        await store.MarkAttemptFailedAsync(
            delivery.Id,
            attempts,
            nextAttemptAt,
            status,
            result.Error ?? "Webhook delivery failed.",
            result.StatusCode,
            cancellationToken);
        return await store.GetAsync(delivery.Id, cancellationToken);
    }
}

public sealed class WebhookDeliveryDispatcher(WebhookDeliveryService service, ITaskQueue taskQueue)
{
    public async Task<WebhookDeliveryRecord> EnqueueAsync(
        string tenantId,
        string eventType,
        string targetUrl,
        string secret,
        string payload,
        CancellationToken cancellationToken)
    {
        var record = await service.EnqueueAsync(tenantId, eventType, targetUrl, secret, payload, cancellationToken);
        await taskQueue.EnqueueAsync("webhook-deliveries", record.Id, cancellationToken);
        return record;
    }
}

public static class WebhookEndpoints
{
    public static void MapWebhookDeliveryAttempt(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/internal/webhook-deliveries/{id}/attempt", async (
            string id,
            HttpContext httpContext,
            WebhookPushAuthOptions pushAuth,
            WebhookDeliveryService service,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            // In the deployed configuration this endpoint is the Cloud Tasks push target and sits
            // OUTSIDE the /api/v1 gate, so it enforces its own OIDC check: only Cloud Tasks acting
            // as the configured service account may drive an attempt. When CloudTasks is unset
            // (local/CI/E2E) the queue runs InProcessQueue and this HTTP path is exercised only by
            // in-process tests, so the check is disabled.
            if (pushAuth.RequireOidc
                && !await WebhookPushAuthenticator.IsAuthorizedAsync(httpContext, pushAuth, cancellationToken))
            {
                loggerFactory.CreateLogger("WebhookPush")
                    .LogWarning("Rejected unauthenticated webhook-delivery push for {DeliveryId}.", id);
                return Results.Json(new { status = "unauthorized" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var delivery = await service.AttemptAsync(id, cancellationToken);
            return delivery is null
                ? Results.NotFound(new { status = "not_found" })
                : Results.Ok(new { id = delivery.Id, status = delivery.Status, attempts = delivery.Attempts });
        });
    }
}

/// <summary>
/// Guards the Cloud Tasks push target. <see cref="RequireOidc"/> is true only when CloudTasks is
/// configured (Program.cs); then the incoming <c>Authorization: Bearer</c> token must be a Google
/// OIDC token signed for <see cref="ServiceAccountEmail"/> with audience <see cref="Audience"/>.
/// </summary>
public sealed record WebhookPushAuthOptions(bool RequireOidc, string ServiceAccountEmail, string Audience)
{
    public static readonly WebhookPushAuthOptions Disabled = new(false, string.Empty, string.Empty);
}

public static class WebhookPushAuthenticator
{
    private const string BearerPrefix = "Bearer ";

    public static async Task<bool> IsAuthorizedAsync(
        HttpContext httpContext,
        WebhookPushAuthOptions options,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return false;
        }

        var raw = authHeader.ToString();
        if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var token = raw[BearerPrefix.Length..].Trim();
        if (token.Length == 0)
        {
            return false;
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [options.Audience],
            });

            cancellationToken.ThrowIfCancellationRequested();
            return payload.EmailVerified
                && string.Equals(payload.Email, options.ServiceAccountEmail, StringComparison.OrdinalIgnoreCase);
        }
        catch (InvalidJwtException)
        {
            return false;
        }
    }
}
