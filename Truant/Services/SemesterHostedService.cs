using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truant.Simulation;

namespace Truant.Services
{
    internal class SemesterHostedService: BackgroundService
    {
        private readonly SemesterSimulator semesterSimulator;
        private readonly IHostApplicationLifetime lifetime;

        public SemesterHostedService(SemesterSimulator semesterSimulator, IHostApplicationLifetime lifetime)
        {
            this.semesterSimulator = semesterSimulator;
            this.lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppedToken)
        {
            while (!stoppedToken.IsCancellationRequested) {

                bool isRunning = semesterSimulator.RunDay();

                if (!isRunning) {
                    lifetime.StopApplication();
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppedToken);
            }

        }
    }
}
