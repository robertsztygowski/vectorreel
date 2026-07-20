using System.Text;
using System.Text.Json;
using MdReel.Api.Features.Auth;
using MdReel.Api.Features.Instrumentation;
using MdReel.Api.Features.Payments;
using MdReel.Api.Features.PrivateProcessing;
using MdReel.Api.Features.Webhooks;
using MdReel.Core.Domain;
using MdReel.Core.Media;
using MdReel.Core.Pipeline.StageA;
using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;
using MdReel.Infrastructure;
using MdReel.Infrastructure.Queue;
using MdReel.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MdReel.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services, builder.Configuration, builder.Environment);
        ConfigureTelemetry(builder);

        var app = builder.Build();
        ConfigurePipeline(app);
        app.Run();
    }

    private static void ConfigureTelemetry(WebApplicationBuilder builder)
    {
        builder.Services.AddMdreelOpenTelemetry(
            builder.Configuration,
            "mdreel-api",
            static tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddNpgsql(),
            static metrics => metrics.AddAspNetCoreInstrumentation());
    }

    private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        services.AddProblemDetails();

        // Browser origin of the web panel (e.g. http://localhost:3000 in the compose e2e profile).
        // Unset ⇒ no CORS headers, cross-origin browser calls stay blocked.
        var corsOrigins = (configuration["CORS_ALLOWED_ORIGINS"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (corsOrigins.Length > 0)
        {
            services.AddCors(options => options.AddDefaultPolicy(policy => policy
                .WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials())); // sendBeacon (first-party events) always sends credentials mode "include"
        }

        services.AddSingleton(new MediaToolOptions
        {
            FfmpegPath = configuration["MEDIA_FFMPEG_PATH"] ?? "ffmpeg",
            FfprobePath = configuration["MEDIA_FFPROBE_PATH"] ?? "ffprobe",
        });
        services.AddSingleton<IMediaProbe, FfprobeMediaProbe>();
        services.AddSingleton<IMediaScanner, FfmpegMediaScanner>();
        services.AddSingleton(CreatePaymentOptions(configuration));
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<ITenantStore, InMemoryTenantStore>();
        services.AddSingleton<IPaymentStore, InMemoryPaymentStore>();
        services.AddSingleton<ISubscriptionStore, InMemorySubscriptionStore>();
        services.AddSingleton<IWebhookDeliveryStore, InMemoryWebhookDeliveryStore>();
        services.AddSingleton<IUploadStore, InMemoryUploadStore>();
        services.AddSingleton<IAdSpendLedger, InMemoryAdSpendLedger>();
        services.AddSingleton<ICohortAnalytics, InMemoryCohortAnalytics>();
        services.AddSingleton<IPaymentGateway>(sp =>
        {
            var options = sp.GetRequiredService<PaymentOptions>();
            if (!string.IsNullOrWhiteSpace(options.StripeSecretKey))
            {
                return new StripePaymentGateway(options);
            }

            return string.Equals(options.Mode, "disabled", StringComparison.OrdinalIgnoreCase)
                ? new DisabledPaymentGateway()
                : new FakePaymentGateway();
        });
        services.AddSingleton<ICostLedger, InMemoryCostLedger>();

        var postgresConnection = configuration["POSTGRES_CONNECTION"]
            ?? configuration.GetConnectionString("Postgres")
            ?? configuration["POSTGRES_CONNECTION_STRING"];
        if (!string.IsNullOrWhiteSpace(postgresConnection))
        {
            services.AddSingleton(NpgsqlDataSource.Create(postgresConnection));
            services.AddSingleton<ICostLedger, PostgresCostLedger>();
            services.AddSingleton<IEventStore, PostgresEventStore>();
            services.AddSingleton<ITenantStore, PostgresTenantStore>();
            services.AddSingleton<IPaymentStore, PostgresPaymentStore>();
            services.AddSingleton<ISubscriptionStore, PostgresSubscriptionStore>();
            services.AddSingleton<IWebhookDeliveryStore, PostgresWebhookDeliveryStore>();
            services.AddSingleton<IUploadStore, PostgresUploadStore>();
            services.AddSingleton<IAdSpendLedger, PostgresAdSpendLedger>();
            services.AddSingleton<ICohortAnalytics, PostgresCohortAnalytics>();
        }

        services.AddHttpClient<HttpWebhookSender>();
        services.AddSingleton<IWebhookSender>(sp => sp.GetRequiredService<HttpWebhookSender>());
        services.AddSingleton<WebhookDeliveryService>();
        services.AddSingleton<WebhookDeliveryDispatcher>();

        // Queue flip: when CloudTasks is fully configured (deployed) route webhook deliveries
        // through Google Cloud Tasks — durable retries + OIDC push back to the /internal endpoint.
        // Unset (local/CI/E2E) keeps the in-process handler so tests stay hermetic (no ADC, no cloud).
        var cloudTasksOptions = configuration.GetSection(CloudTasksOptions.SectionName).Get<CloudTasksOptions>()
            ?? new CloudTasksOptions();
        var cloudTasksEnabled = !string.IsNullOrWhiteSpace(cloudTasksOptions.ProjectId)
            && !string.IsNullOrWhiteSpace(cloudTasksOptions.QueueName)
            && !string.IsNullOrWhiteSpace(cloudTasksOptions.TargetBaseUrl);

        services.AddSingleton(cloudTasksEnabled && !string.IsNullOrWhiteSpace(cloudTasksOptions.ServiceAccountEmail)
            ? new WebhookPushAuthOptions(true, cloudTasksOptions.ServiceAccountEmail, cloudTasksOptions.Audience)
            : WebhookPushAuthOptions.Disabled);

        if (cloudTasksEnabled)
        {
            services.AddSingleton<ICloudTasksTransport, GoogleCloudTasksTransport>();
            services.AddSingleton<ITaskQueue, CloudTasksQueue>();
        }
        else
        {
            services.AddSingleton<ITaskQueue>(sp => new InProcessQueue(async (queue, payload, cancellationToken) =>
            {
                if (!string.Equals(queue, "webhook-deliveries", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"No in-process handler is registered for queue '{queue}'.");
                }

                await sp.GetRequiredService<WebhookDeliveryService>().AttemptAsync(payload, cancellationToken);
            }));
        }

        services.AddSingleton<StageARunner>();
        services.AddSingleton<StageBRunner>();
        services.AddPipelineInfrastructure(
            configuration,
            Path.Combine(environment.ContentRootPath, ".local-state", "object-storage"));
        services.AddSingleton<PrivatePipelineService>();

        ConfigureAuth(services, configuration, postgresConnection);
    }

    /// <summary>
    /// ASP.NET Core Identity (email + password, cookie mode). EF Core is scoped to the identity
    /// tables only — Npgsql against the shared product database when configured, otherwise an
    /// isolated EF InMemory store (no-Postgres dev/test). Registration and the tenant hook flow
    /// through <c>AuthEndpoints.MapAuth</c>; the built-in PBKDF2 hasher is used.
    /// </summary>
    private static void ConfigureAuth(IServiceCollection services, ConfigurationManager configuration, string? postgresConnection)
    {
        var identityInMemoryName = "mdreel-identity-" + Guid.NewGuid().ToString("N");
        services.AddDbContext<AppIdentityDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(postgresConnection))
            {
                options.UseNpgsql(postgresConnection);
            }
            else
            {
                options.UseInMemoryDatabase(identityInMemoryName);
            }
        });

        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.AddAuthorization();

        // This is an API, not an MVC app: an unauthenticated/forbidden request must get 401/403,
        // never a 302 redirect to a (nonexistent) login page.
        services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddApiEndpoints()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

        services.AddSingleton(new BrevoOptions
        {
            ApiKey = configuration["BREVO_API_KEY"],
            SenderEmail = configuration["BREVO_SENDER_EMAIL"] ?? "no-reply@mdreel.com",
            SenderName = configuration["BREVO_SENDER_NAME"] ?? "mdreel",
        });
        services.AddHttpClient<IEmailSender<AppUser>, BrevoEmailSender>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter(AuthEndpoints.RateLimitPolicy, limiter =>
            {
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.PermitLimit = 20;
                limiter.QueueLimit = 0;
            });
        });
    }

    private static PaymentOptions CreatePaymentOptions(ConfigurationManager configuration) => new()
    {
        StripeSecretKey = configuration["STRIPE_SECRET_KEY"],
        StripeWebhookSecret = configuration["STRIPE_WEBHOOK_SECRET"],
        ProPriceId = configuration["STRIPE_PRICE_PRO"],
        BusinessPriceId = configuration["STRIPE_PRICE_BUSINESS"],
        StarterPriceId = configuration["STRIPE_PRICE_STARTER"],
        AppBaseUrl = configuration["APP_BASE_URL"] ?? "http://localhost",
        ShowStarterPlan = string.Equals(configuration["SHOW_STARTER_PLAN"], "true", StringComparison.OrdinalIgnoreCase),
        Mode = configuration["PAYMENTS_MODE"] ?? "fake",
    };

    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            if (response.StatusCode is < StatusCodes.Status400BadRequest or > 599)
            {
                return;
            }

            if (!string.IsNullOrEmpty(response.ContentType) || response.ContentLength is > 0)
            {
                return;
            }

            var result = Results.Problem(
                title: ReasonPhrases.GetReasonPhrase(response.StatusCode),
                statusCode: response.StatusCode,
                instance: context.HttpContext.Request.Path);

            await result.ExecuteAsync(context.HttpContext);
        });

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        if (app.Services.GetService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsService>() is not null)
        {
            app.UseCors();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments("/api/v1"))
            {
                await next(context);
                return;
            }

            // Auth endpoints (login/signup/logout/refresh/manage/password) run their own
            // authorization; the coarse gate must not block anonymous login/signup.
            if (context.Request.Path.StartsWithSegments("/api/v1/auth"))
            {
                await next(context);
                return;
            }

            var isSignedUploadPut =
                HttpMethods.IsPut(context.Request.Method)
                && context.Request.Path.Value?.StartsWith("/api/v1/uploads/", StringComparison.Ordinal) == true
                && context.Request.Path.Value?.EndsWith("/content", StringComparison.Ordinal) == true;
            var isUnauthedInstrumentationOrWebhook =
                HttpMethods.IsPost(context.Request.Method)
                && (string.Equals(context.Request.Path.Value, "/api/v1/events", StringComparison.Ordinal)
                    || string.Equals(context.Request.Path.Value, "/api/v1/webhooks/stripe", StringComparison.Ordinal));

            if (isSignedUploadPut || isUnauthedInstrumentationOrWebhook)
            {
                await next(context);
                return;
            }

            // A signed-in cookie principal satisfies the gate; the legacy bearer header remains
            // accepted so the frozen API contract and its tests are unaffected.
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
            {
                await context.WriteProblemAsync(401, "Unauthorized", "Missing Authorization header.");
                return;
            }

            await next(context);
        });

        app.MapAuth();
        app.MapWebhookDeliveryAttempt();

        var api = app.MapGroup("/api/v1");

        api.MapPost("/uploads", async (
            HttpContext httpContext,
            PrivatePipelineService pipeline,
            CancellationToken cancellationToken) =>
        {
            CreateUploadRequest? request = null;
            if (httpContext.Request.ContentLength is > 0)
            {
                try
                {
                    request = await httpContext.Request.ReadFromJsonAsync<CreateUploadRequest>(cancellationToken);
                }
                catch (JsonException)
                {
                    return Results.Problem(
                        title: "Invalid request body",
                        detail: "Expected a JSON object.",
                        statusCode: StatusCodes.Status400BadRequest,
                        type: "about:blank");
                }
            }

            var created = await pipeline.CreateUploadAsync(
                httpContext.Request,
                request?.ContentType,
                cancellationToken);
            return Results.Json(created, statusCode: StatusCodes.Status201Created);
        });

        api.MapPut("/uploads/{uploadId}/content", async (
            string uploadId,
            HttpContext httpContext,
            PrivatePipelineService pipeline,
            CancellationToken cancellationToken) =>
        {
            var sig = httpContext.Request.Query["sig"].ToString();
            var stored = await pipeline.StoreUploadAsync(uploadId, sig, httpContext.Request, cancellationToken);
            if (!stored)
            {
                return Results.Problem(
                    title: "Upload not found",
                    detail: "Unknown uploadId or invalid signature.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.NoContent();
        });

        api.MapPost("/jobs", async (
            HttpContext httpContext,
            PrivatePipelineService pipeline,
            CancellationToken cancellationToken) =>
        {
            var request = await httpContext.Request.ReadFromJsonAsync<CreateJobRequest>(cancellationToken: cancellationToken);
            if (request is null)
            {
                return Results.Problem(
                    title: "Invalid request body",
                    detail: "Expected a JSON object.",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "about:blank");
            }

            if (string.IsNullOrWhiteSpace(request.UploadId))
            {
                return Results.Problem(
                    title: "Missing job source",
                    detail: "Provide an uploadId from POST /uploads.",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "about:blank");
            }

            var tenantId = httpContext.User.FindFirst(AppUserClaimsPrincipalFactory.TenantClaimType)?.Value;
            var created = await pipeline.CreateJobAsync(request.UploadId, request.Options, tenantId, cancellationToken);
            if (created is null)
            {
                return Results.Problem(
                    title: "Upload not found",
                    detail: "Unknown uploadId.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.Json(new { jobId = created.JobId }, statusCode: StatusCodes.Status202Accepted);
        });

        MapFrozenJobSubset(api);
        api.MapEvents();
        api.MapCheckout();
        api.MapBillingPortal();
        api.MapStripeWebhooks();

        api.MapGet("/jobs/{id}", (string id, PrivatePipelineService pipeline) =>
        {
            var status = pipeline.GetJobStatus(id);
            if (status is null)
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.Json(status);
        });

        api.MapGet("/jobs/{id}/output.json", (string id, PrivatePipelineService pipeline) =>
        {
            if (!pipeline.HasJob(id))
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            var output = pipeline.GetOutput(id);
            if (output is null)
            {
                return Results.Problem(
                    title: "Job not finished",
                    detail: "Output is available only for finished jobs.",
                    statusCode: StatusCodes.Status409Conflict,
                    type: "about:blank");
            }

            return Results.Json(output.Document);
        });

        api.MapGet("/jobs/{id}/output.md", (string id, PrivatePipelineService pipeline) =>
        {
            if (!pipeline.HasJob(id))
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            var output = pipeline.GetOutput(id);
            if (output is null)
            {
                return Results.Problem(
                    title: "Job not finished",
                    detail: "Output is available only for finished jobs.",
                    statusCode: StatusCodes.Status409Conflict,
                    type: "about:blank");
            }

            return Results.Content(output.Markdown, "text/markdown; charset=utf-8", Encoding.UTF8);
        });

    }

    private static void MapFrozenJobSubset(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/jobs", (PrivatePipelineService pipeline) =>
        {
            var jobs = pipeline.ListJobs();
            return Results.Json(new { jobs });
        });

        routes.MapDelete("/jobs/{id}", async (
            string id,
            PrivatePipelineService pipeline,
            CancellationToken cancellationToken) =>
        {
            var deleted = await pipeline.DeleteJobAsync(id, cancellationToken);
            if (!deleted)
            {
                return Results.Problem(
                    title: "Job not found",
                    detail: "Unknown job id.",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "about:blank");
            }

            return Results.NoContent();
        });
    }
}
