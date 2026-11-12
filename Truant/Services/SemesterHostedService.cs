using Microsoft.Extensions.Hosting;
using Truant.Models;
using Truant.Simulation;

namespace Truant.Services;

public class SemesterHostedService: BackgroundService
{
    private readonly SemesterSimulator semesterSimulator;
    private readonly IHostApplicationLifetime lifetime;
    private readonly SimulationStartupOptions options;

    public SemesterHostedService(SemesterSimulator semesterSimulator, IHostApplicationLifetime lifetime, SimulationStartupOptions options)
    {
        this.semesterSimulator = semesterSimulator;
        this.lifetime = lifetime;
        this.options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        lifetime.ApplicationStarted.Register(() => started.TrySetResult(true));

        await started.Task.WaitAsync(stoppingToken);

        Console.WriteLine("=================================");
        Console.WriteLine("Generic Host запущен");
        Console.WriteLine("=================================");
        Console.WriteLine();

        if (options.ResumeExisting)
        {
            await semesterSimulator.ResumeAsync(options.SemesterId!.Value, stoppingToken);
        }
        else
        {
            await semesterSimulator.InitializeAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            bool isRunning = await semesterSimulator.RunDayAsync(stoppingToken);

            if (!isRunning)
            {
                lifetime.StopApplication();
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
