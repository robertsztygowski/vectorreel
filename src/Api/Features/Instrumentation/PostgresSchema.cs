using Npgsql;

namespace MdReel.Api.Features.Instrumentation;

public static class PostgresSchema
{
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static bool _ensured;

    public static async Task EnsureAsync(NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        if (_ensured)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_ensured)
            {
                return;
            }

            await using var command = dataSource.CreateCommand("""
                create table if not exists tenants (
                    id text primary key,
                    email text not null unique,
                    first_utm_source text null,
                    first_utm_medium text null,
                    first_utm_campaign text null,
                    first_utm_term text null,
                    first_referrer text null,
                    ab_arm text null,
                    archive_hours double precision null,
                    monthly_hours double precision null,
                    trial_credit_hours double precision not null default 0,
                    plan text null,
                    created_at timestamptz not null default now()
                );

                create table if not exists users (
                    id text primary key,
                    tenant_id text not null references tenants(id),
                    email text not null unique,
                    created_at timestamptz not null default now()
                );

                create table if not exists events (
                    id text primary key,
                    name text not null,
                    session_id text null,
                    tenant_id text null,
                    user_id text null,
                    occurred_at timestamptz not null,
                    referrer text null,
                    ab_arm text null,
                    payload_json jsonb not null default '{}'::jsonb,
                    created_at timestamptz not null default now()
                );

                create table if not exists usage_ledger (
                    id text primary key,
                    tenant_id text null,
                    job_id text null,
                    kind text not null,
                    step text not null,
                    quantity double precision not null,
                    unit text not null,
                    cents integer null,
                    occurred_at timestamptz not null default now()
                );

                create table if not exists ad_spend (
                    id text primary key,
                    day date not null,
                    channel text not null,
                    campaign text null,
                    keyword text null,
                    cost_cents integer not null,
                    clicks integer null,
                    impressions integer null,
                    created_at timestamptz not null default now()
                );

                create table if not exists payments (
                    id text primary key,
                    tenant_id text not null references tenants(id),
                    session_id text not null,
                    plan text not null,
                    amount_cents integer not null,
                    currency text not null,
                    first_utm_source text null,
                    first_utm_medium text null,
                    first_utm_campaign text null,
                    first_utm_term text null,
                    first_referrer text null,
                    ab_arm text null,
                    created_at timestamptz not null default now()
                );

                create table if not exists subscriptions (
                    id text primary key,
                    tenant_id text not null references tenants(id),
                    stripe_subscription_id text null,
                    stripe_customer_id text null,
                    plan text not null,
                    status text not null,
                    created_at timestamptz not null default now(),
                    updated_at timestamptz not null default now()
                );

                alter table subscriptions add column if not exists stripe_customer_id text null;
                alter table subscriptions add column if not exists updated_at timestamptz not null default now();
                """);
            await command.ExecuteNonQueryAsync(cancellationToken);
            _ensured = true;
        }
        finally
        {
            _gate.Release();
        }
    }
}
