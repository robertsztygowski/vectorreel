using System.Text.Json.Serialization;

namespace MdReel.Api.Features.Admin;

public sealed record AdminOverviewResponse(
    IReadOnlyList<AdminFunnelWindow> Funnel,
    AdminUsageOverview Usage,
    AdminRetentionOverview Retention,
    IReadOnlyList<AdminSourceOverview> Sources);

public sealed record AdminFunnelWindow(
    string Window,
    int PageView,
    int SignupView,
    int Signup,
    int UploadStarted,
    int JobCompleted,
    int CheckoutClicked,
    int PaymentSucceeded);

public sealed record AdminUsageOverview(
    AdminUsageWindow Today,
    AdminUsageWindow SevenDays);

public sealed record AdminUsageWindow(
    int VideosProcessed,
    double VideoMinutes);

public sealed record AdminRetentionOverview(
    int NewLast7d,
    int NewLast30d,
    int ReturningThisWeek,
    int Inactive30d,
    IReadOnlyList<AdminSignupWeek> SignupWeeks);

public sealed record AdminSignupWeek(
    DateOnly Week,
    int Tenants);

public sealed record AdminSourceOverview(
    string? FirstUtmSource,
    string? FirstUtmMedium,
    string? FirstUtmCampaign,
    int TenantCount,
    int PayingTenantCount,
    int RevenueCents,
    int AdSpendCents,
    double? CacCents);

public sealed record AdminAdSpendRequest(
    string? Source,
    string? Campaign,
    [property: JsonPropertyName("amount_cents")] int AmountCents,
    string? Currency,
    [property: JsonPropertyName("spent_on")] DateOnly SpentOn);

public interface IAdminOverviewStore
{
    Task<AdminOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken);
}
