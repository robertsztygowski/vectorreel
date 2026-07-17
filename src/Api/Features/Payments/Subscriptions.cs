using MdReel.Api.Features.Instrumentation;
using Npgsql;

namespace MdReel.Api.Features.Payments;

public sealed record SubscriptionRecord(
    string Id,
    string TenantId,
    string? StripeSubscriptionId,
    string? StripeCustomerId,
    string Plan,
    string Status);

public interface ISubscriptionStore
{
    Task UpsertAsync(SubscriptionRecord subscription, CancellationToken cancellationToken);

    Task<SubscriptionRecord?> GetByTenantAsync(string tenantId, CancellationToken cancellationToken);
}

public sealed class InMemorySubscriptionStore : ISubscriptionStore
{
    private readonly Dictionary<string, SubscriptionRecord> _byTenant = [];
    private readonly object _gate = new();

    public Task UpsertAsync(SubscriptionRecord subscription, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _byTenant[subscription.TenantId] = subscription;
        }

        return Task.CompletedTask;
    }

    public Task<SubscriptionRecord?> GetByTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            return Task.FromResult(_byTenant.TryGetValue(tenantId, out var record) ? record : null);
        }
    }
}

public sealed class PostgresSubscriptionStore(NpgsqlDataSource dataSource) : ISubscriptionStore
{
    public async Task UpsertAsync(SubscriptionRecord subscription, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            insert into subscriptions (id, tenant_id, stripe_subscription_id, stripe_customer_id, plan, status, updated_at)
            values (@id, @tenant_id, @sub_id, @cus_id, @plan, @status, now())
            on conflict (id) do update set
                stripe_subscription_id = excluded.stripe_subscription_id,
                stripe_customer_id = excluded.stripe_customer_id,
                plan = excluded.plan,
                status = excluded.status,
                updated_at = now()
            """);
        command.Parameters.AddWithValue("id", subscription.Id);
        command.Parameters.AddWithValue("tenant_id", subscription.TenantId);
        command.Parameters.AddWithValue("sub_id", (object?)subscription.StripeSubscriptionId ?? DBNull.Value);
        command.Parameters.AddWithValue("cus_id", (object?)subscription.StripeCustomerId ?? DBNull.Value);
        command.Parameters.AddWithValue("plan", subscription.Plan);
        command.Parameters.AddWithValue("status", subscription.Status);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<SubscriptionRecord?> GetByTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        await PostgresSchema.EnsureAsync(dataSource, cancellationToken);
        await using var command = dataSource.CreateCommand("""
            select id, tenant_id, stripe_subscription_id, stripe_customer_id, plan, status
            from subscriptions
            where tenant_id = @tenant_id
            order by updated_at desc
            limit 1
            """);
        command.Parameters.AddWithValue("tenant_id", tenantId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new SubscriptionRecord(
            reader.GetString(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5));
    }
}
