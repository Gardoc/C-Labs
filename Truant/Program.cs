using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Truant.Services;
using Truant.Simulation;
using Truant.Strategies;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddSingleton<Random>();
builder.Services.AddSingleton<StudentStrategy>();
builder.Services.AddSingleton<SemesterSimulator>();

builder.Services.AddHostedService<SemesterHostedService>();
builder.Services.AddHostedService<KeyboardShutdownService>();

var host = builder.Build();
await host.RunAsync();