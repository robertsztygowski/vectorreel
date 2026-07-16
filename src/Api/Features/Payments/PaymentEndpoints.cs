using System.Text.Json;
using MdReel.Api.Features.Instrumentation;

namespace MdReel.Api.Features.Payments;

public static class PaymentEndpoints
{
    public static void MapCheckout(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/checkout", async (
            CheckoutRequest request,
            PaymentOptions options,
            IPaymentGateway gateway,
            IEventStore events,
            ITenantStore tenants,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Plan) || !IsKnownPlan(request.Plan))
            {
                return Results.Problem(title: "Invalid plan", detail: "Plan must be pro, business, or starter.", statusCode: StatusCodes.Status400BadRequest, type: "about:blank");
            }

            if (request.Plan == "starter" && !options.ShowStarterPlan)
            {
                return Results.Problem(title: "Plan unavailable", detail: "Starter is not available.", statusCode: StatusCodes.Status404NotFound, type: "about:blank");
            }

            if (string.IsNullOrWhiteSpace(request.TenantId))
            {
                return Results.Problem(title: "Missing tenant", detail: "tenant_id is required.", statusCode: StatusCodes.Status400BadRequest, type: "about:blank");
            }

            var tenant = await tenants.GetAsync(request.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Results.Problem(title: "Tenant not found", detail: "Unknown tenant_id.", statusCode: StatusCodes.Status404NotFound, type: "about:blank");
            }

            var checkout = await gateway.CreateCheckoutSessionAsync(request.Plan, request.TenantId, cancellationToken);
            var payload = JsonSerializer.SerializeToElement(new { plan = request.Plan, sessionId = checkout.SessionId });
            await events.RecordAsync(new EventDraft("checkout_clicked", null, request.TenantId, null, DateTimeOffset.UtcNow, null, null, payload.GetRawText()), cancellationToken);
            return Results.Json(new { url = checkout.Url, sessionId = checkout.SessionId }, statusCode: StatusCodes.Status201Created);
        });
    }

    public static void MapStripeWebhooks(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/webhooks/stripe", async (
            HttpRequest request,
            IPaymentGateway gateway,
            IPaymentStore payments,
            ITenantStore tenants,
            IEventStore events,
            CancellationToken cancellationToken) =>
        {
            using var reader = new StreamReader(request.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);
            PaymentWebhookEvent webhook;
            try
            {
                webhook = gateway.ConstructWebhookEvent(payload, request.Headers["Stripe-Signature"].ToString());
            }
            catch (PaymentWebhookSignatureException ex)
            {
                return Results.Problem(title: "Invalid Stripe signature", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, type: "about:blank");
            }
            catch (Stripe.StripeException ex)
            {
                return Results.Problem(title: "Invalid Stripe signature", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, type: "about:blank");
            }

            if (webhook.Type != "checkout.session.completed" && webhook.Type != "payment_intent.succeeded")
            {
                return Results.Ok(new { received = true });
            }

            var tenant = await tenants.GetAsync(webhook.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Results.Problem(title: "Tenant not found", detail: "Webhook referenced an unknown tenant.", statusCode: StatusCodes.Status404NotFound, type: "about:blank");
            }

            await payments.RecordPaymentAsync(new PaymentRecord(
                string.Empty,
                tenant.Id,
                webhook.SessionId,
                webhook.Plan,
                webhook.AmountCents,
                webhook.Currency,
                tenant.FirstUtmSource,
                tenant.FirstUtmMedium,
                tenant.FirstUtmCampaign,
                tenant.FirstUtmTerm,
                tenant.FirstReferrer,
                tenant.AbArm), cancellationToken);

            var eventPayload = JsonSerializer.SerializeToElement(new
            {
                amount_cents = webhook.AmountCents,
                plan = webhook.Plan,
                sessionId = webhook.SessionId,
            });
            await events.RecordAsync(new EventDraft("payment_succeeded", null, tenant.Id, null, DateTimeOffset.UtcNow, null, tenant.AbArm, eventPayload.GetRawText()), cancellationToken);
            await tenants.SetPlanAsync(tenant.Id, webhook.Plan, cancellationToken);
            return Results.Ok(new { received = true });
        });
    }

    private static bool IsKnownPlan(string plan) => plan is "pro" or "business" or "starter";
}
