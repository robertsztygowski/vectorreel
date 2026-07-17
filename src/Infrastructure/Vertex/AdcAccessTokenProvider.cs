using Google.Apis.Auth.OAuth2;

namespace MdReel.Infrastructure.Vertex;

/// <summary>Supplies an OAuth bearer token for Vertex calls.</summary>
public interface IAccessTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Application Default Credentials — ADC locally (gcloud user creds), the Cloud Run service
/// account in production. <b>No JSON key files, ever</b> (CLAUDE.md rule 1). The underlying
/// <see cref="GoogleCredential"/> caches and refreshes the token itself.
/// </summary>
public sealed class AdcAccessTokenProvider : IAccessTokenProvider
{
    private const string CloudPlatformScope = "https://www.googleapis.com/auth/cloud-platform";

    private readonly Lazy<Task<ITokenAccess>> _credential = new(CreateCredentialAsync);

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var credential = await _credential.Value;
        return await credential.GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
    }

    private static async Task<ITokenAccess> CreateCredentialAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        if (credential.IsCreateScopedRequired)
        {
            credential = credential.CreateScoped(CloudPlatformScope);
        }

        return credential;
    }
}
