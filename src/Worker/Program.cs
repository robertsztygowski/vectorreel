using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;
using MdReel.Infrastructure;
using MdReel.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<YouTubeGalleryRunnerOptions>(builder.Configuration.GetSection("YouTubeGalleryRunner"));
builder.Services.AddSingleton<StageBRunner>();
builder.Services.AddSingleton<YouTubeInternalGalleryRunner>();
builder.Services.AddVertexInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IObjectStorage, LocalFileObjectStorage>();
builder.Services.AddHostedService<PipelineWorker>();

var host = builder.Build();
host.Run();
