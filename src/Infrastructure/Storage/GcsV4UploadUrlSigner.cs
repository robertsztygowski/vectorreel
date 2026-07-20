using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using MdReel.Core.Providers;

namespace MdReel.Infrastructure.Storage;

public sealed class DisabledUploadUrlSigner : IUploadUrlSigner
{
    public bool DirectUploadsEnabled => false;

    public Task<string> SignPutUrlAsync(
        string bucket,
        string objectName,
        string contentType,
        TimeSpan expiresAfter,
        CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Direct upload signing is not enabled in this environment.");
}

public sealed class GcsV4UploadUrlSigner : IUploadUrlSigner
{
    private readonly Lazy<Task<UrlSigner>> _signer = new(CreateSignerAsync);

    public bool DirectUploadsEnabled => true;

    public async Task<string> SignPutUrlAsync(
        string bucket,
        string objectName,
        string contentType,
        TimeSpan expiresAfter,
        CancellationToken cancellationToken)
    {
        var template = UrlSigner.RequestTemplate
            .FromBucket(bucket)
            .WithObjectName(objectName)
            .WithHttpMethod(HttpMethod.Put)
            .WithContentHeaders([
                new KeyValuePair<string, IEnumerable<string>>("Content-Type", [contentType]),
            ]);
        var options = UrlSigner.Options.FromDuration(expiresAfter).WithSigningVersion(SigningVersion.V4);
        var signer = await _signer.Value.WaitAsync(cancellationToken);

        return await signer.SignAsync(template, options, cancellationToken);
    }

    private static async Task<UrlSigner> CreateSignerAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        return UrlSigner.FromCredential(credential);
    }
}
