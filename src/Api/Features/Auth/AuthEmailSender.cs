using Microsoft.AspNetCore.Identity;

namespace MdReel.Api.Features.Auth;

public sealed class BrevoOptions
{
    public string? ApiKey { get; init; }

    public string SenderEmail { get; init; } = "no-reply@mdreel.com";

    public string SenderName { get; init; } = "mdreel";

    public bool Configured => !string.IsNullOrWhiteSpace(ApiKey);
}

/// <summary>
/// Transactional email via Brevo's HTTP API (free tier). Ships as a logging no-op until
/// <c>BREVO_API_KEY</c> exists in the environment — registration and password reset still work
/// because <c>RequireConfirmedEmail = false</c>, so the confirmation link is optional. Raw
/// <see cref="HttpClient"/>, no SDK; EU data path (Brevo is an EU provider).
/// </summary>
public sealed class BrevoEmailSender(HttpClient httpClient, BrevoOptions options, ILogger<BrevoEmailSender> logger)
    : IEmailSender<AppUser>
{
    public Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your mdreel account", $"Confirm your email by visiting <a href=\"{confirmationLink}\">this link</a>.");

    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your mdreel password", $"Reset your password by visiting <a href=\"{resetLink}\">this link</a>.");

    public Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode) =>
        SendAsync(email, "Your mdreel password reset code", $"Your password reset code is: {resetCode}");

    private async Task SendAsync(string toEmail, string subject, string htmlContent)
    {
        if (!options.Configured)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Email suppressed (no BREVO_API_KEY): to={ToEmail} subject={Subject}", toEmail, subject);
            }

            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
        {
            Content = JsonContent.Create(new
            {
                sender = new { email = options.SenderEmail, name = options.SenderName },
                to = new[] { new { email = toEmail } },
                subject,
                htmlContent,
            }),
        };
        request.Headers.Add("api-key", options.ApiKey);

        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Brevo email send failed with status {Status} for {ToEmail}", (int)response.StatusCode, toEmail);
        }
    }
}
