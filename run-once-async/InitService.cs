// Here's how you can create an InitService that implements IHostedService and runs only once at application startup, then stops.

/*
Behavior:
InitService runs once when the app starts.
You can perform setup logic like loading config, warming caches, seeding databases, etc.
It won't run again unless the app restarts.
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class InitService : IHostedService
{
    private readonly ILogger<InitService> _logger;
	private readonly IHostEnvironment _env;

    public InitService(ILogger<InitService> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InitService starting in {Environment} environment...", _env.EnvironmentName);

        // Your init logic that run once
        await InitializeAsync();

        _logger.LogInformation("InitService completed.");
		return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // No need to do anything since it only runs once at startup
        return Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        // Simulate some async init work
        await Task.Delay(2000);
        _logger.LogInformation("Initialization logic finished.");
    }
}
