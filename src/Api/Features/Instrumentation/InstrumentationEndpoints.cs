using System.Text.Json;

namespace MdReel.Api.Features.Instrumentation;

public static class InstrumentationEndpoints
{
    public static void MapEvents(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/events", async (
            HttpRequest request,
            IEventStore events,
            ITenantStore tenants,
            CancellationToken cancellationToken) =>
        {
            using var body = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
            var root = body.RootElement;
            var name = GetString(root, "name");
            if (string.IsNullOrWhiteSpace(name) || !EventNames.Stable.Contains(name))
            {
                return Results.Problem(title: "Invalid event name", detail: "Event name is blank or unknown.", statusCode: StatusCodes.Status400BadRequest, type: "about:blank");
            }

            if (name == "signup")
            {
                var email = GetString(root, "email");
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Results.Problem(title: "Invalid signup", detail: "Signup events require email.", statusCode: StatusCodes.Status400BadRequest, type: "about:blank");
                }

                var signup = await tenants.UpsertSignupAsync(new SignupDraft(
                    email,
                    GetDouble(root, "archive_hours"),
                    GetDouble(root, "monthly_hours"),
                    GetString(root, "utm_source"),
                    GetString(root, "utm_medium"),
                    GetString(root, "utm_campaign"),
                    GetString(root, "utm_term"),
                    GetString(root, "first_referrer"),
                    GetString(root, "ab_arm")), cancellationToken);

                var stored = await events.RecordAsync(BuildEventDraft(root, name, signup.Tenant.Id, signup.User.Id), cancellationToken);
                return Results.Json(new
                {
                    eventId = stored.Id,
                    tenant_id = signup.Tenant.Id,
                    user_id = signup.User.Id,
                    trial_credit_hours = signup.Tenant.TrialCreditHours,
                }, statusCode: StatusCodes.Status202Accepted);
            }

            var ev = await events.RecordAsync(BuildEventDraft(root, name, GetString(root, "tenant_id"), GetString(root, "user_id")), cancellationToken);
            return Results.Json(new { eventId = ev.Id }, statusCode: StatusCodes.Status202Accepted);
        });

        routes.MapGet("/admin/cohorts/hour-decay", async (ICohortAnalytics cohorts, CancellationToken cancellationToken) =>
            Results.Json(new { cohorts = await cohorts.GetTenantHoursByWeekAsync(cancellationToken) }));
    }

    public static EventDraft BuildEventDraft(JsonElement root, string name, string? tenantId, string? userId)
    {
        var occurredAt = GetDate(root, "occurred_at") ?? DateTimeOffset.UtcNow;
        return new EventDraft(
            name,
            GetString(root, "session_id"),
            tenantId,
            userId,
            occurredAt,
            GetString(root, "referrer"),
            GetString(root, "ab_arm"),
            BuildPayloadJson(root));
    }

    public static string? GetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

    private static string BuildPayloadJson(JsonElement root)
    {
        var reserved = new HashSet<string>(StringComparer.Ordinal)
        {
            "name",
            "session_id",
            "tenant_id",
            "user_id",
            "occurred_at",
            "referrer",
            "ab_arm",
        };
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var property in root.EnumerateObject())
            {
                if (!reserved.Contains(property.Name))
                {
                    property.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        return JsonSerializer.Deserialize<JsonElement>(stream.ToArray()).GetRawText();
    }

    private static double? GetDouble(JsonElement root, string name) =>
        root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

    private static DateTimeOffset? GetDate(JsonElement root, string name) =>
        root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(property.GetString(), out var value) ? value.ToUniversalTime() : null;
}
