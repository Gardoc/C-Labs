using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truant.Simulation;

namespace Truant.Services
{
    internal class KeyboardShutdownService: BackgroundService
    {
        private readonly IHostApplicationLifetime lifetime;
        public KeyboardShutdownService(IHostApplicationLifetime lifetime)
        {
            this.lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppedToken)
        {
            while (!stoppedToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    Console.WriteLine("Остановка программы...\n");
                    lifetime.StopApplication();
                    break;
                }
                await Task.Delay(100, stoppedToken);
            }
        }
    }
}
