using MdReel.Core.Pipeline.StageB;
using MdReel.Core.Providers;
using MdReel.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<YouTubeGalleryRunnerOptions>(builder.Configuration.GetSection("YouTubeGalleryRunner"));
builder.Services.AddSingleton<StageBRunner>();
builder.Services.AddSingleton<YouTubeInternalGalleryRunner>();
builder.Services.AddSingleton<IVideoAnalyzer, UnconfiguredVideoAnalyzer>();
builder.Services.AddSingleton<ITextFuser, UnconfiguredTextFuser>();
builder.Services.AddSingleton<IObjectStorage, LocalFileObjectStorage>();
builder.Services.AddHostedService<PipelineWorker>();

var host = builder.Build();
host.Run();
