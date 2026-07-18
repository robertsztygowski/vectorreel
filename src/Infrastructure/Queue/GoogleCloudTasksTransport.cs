using Google.Cloud.Tasks.V2;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using GoogleHttpMethod = Google.Cloud.Tasks.V2.HttpMethod;
using GoogleHttpRequest = Google.Cloud.Tasks.V2.HttpRequest;
using GoogleTask = Google.Cloud.Tasks.V2.Task;

namespace MdReel.Infrastructure.Queue;

/// <summary>
/// Real <see cref="ICloudTasksTransport"/>: maps a <see cref="CloudTaskRequest"/> to a Cloud Tasks
/// HTTP-target task carrying an OIDC push token (so the receiving Cloud Run endpoint can verify the
/// caller is Cloud Tasks acting as <see cref="CloudTasksOptions.ServiceAccountEmail"/>). Only
/// constructed when CloudTasks is configured — never instantiated locally/in tests, so ADC is not
/// required there (Program.cs keeps <see cref="InProcessQueue"/> as the default).
/// </summary>
public sealed class GoogleCloudTasksTransport : ICloudTasksTransport
{
    private readonly CloudTasksClient _client;
    private readonly CloudTasksOptions _options;

    public GoogleCloudTasksTransport(IOptions<CloudTasksOptions> options)
        : this(CloudTasksClient.Create(), options)
    {
    }

    // Test/DI seam: inject a client (or an emulator-backed one) explicitly.
    public GoogleCloudTasksTransport(CloudTasksClient client, IOptions<CloudTasksOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async System.Threading.Tasks.Task CreateTaskAsync(CloudTaskRequest request, CancellationToken cancellationToken)
    {
        var httpRequest = new GoogleHttpRequest
        {
            Url = request.TargetUrl.ToString(),
            HttpMethod = System.Enum.Parse<GoogleHttpMethod>(request.HttpMethod, ignoreCase: true),
            Body = ByteString.CopyFromUtf8(request.Body),
        };

        foreach (var header in request.Headers)
        {
            httpRequest.Headers[header.Key] = header.Value;
        }

        if (!string.IsNullOrWhiteSpace(_options.ServiceAccountEmail))
        {
            httpRequest.OidcToken = new OidcToken
            {
                ServiceAccountEmail = _options.ServiceAccountEmail,
                Audience = _options.Audience,
            };
        }

        var task = new GoogleTask
        {
            HttpRequest = httpRequest,
            DispatchDeadline = Duration.FromTimeSpan(request.DispatchDeadline),
        };

        await _client.CreateTaskAsync(
            new CreateTaskRequest { Parent = request.QueuePath, Task = task },
            cancellationToken);
    }
}
