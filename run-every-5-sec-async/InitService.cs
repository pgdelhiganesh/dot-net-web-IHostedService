// Here's how you can create an InitService that implements IHostedService and runs only once at application startup, then stops.
// If you want a DoWork() method that executes once every 5 seconds, and stops afterward (i.e., just one execution, delayed by 5 seconds).

/*
Behavior:
InitService runs once when the app starts.
You can perform setup logic like loading config, warming caches, seeding databases, etc.
It won't run again unless the app restarts.

Thread Safety + Cleanup:
Uses CancellationToken properly.
Task.Run only once — avoids multiple overlapping tasks.
StopAsync ensures clean shutdown.
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
	private CancellationTokenSource _cts;
    private Task _backgroundTask;
	
    public InitService(ILogger<InitService> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InitService starting in {Environment} environment...", _env.EnvironmentName);

        /// Your init logic that run once
        await InitializeAsync();
		
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundTask = Task.Run(() => RunAsync(_cts.Token));

        _logger.LogInformation("InitService completed.");
		return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Do anything that want to run on service stopping
		
		_logger.LogInformation("InitService stopping...");
        if (_cts != null)
        {
            _cts.Cancel();
            try
            {
                await _backgroundTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation happens
            }
        }
        _logger.LogInformation("InitService stopped.");
		
        return Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        // Simulate some async init work
        await Task.Delay(2000);
        _logger.LogInformation("Initialization logic finished.");
    }
	
	private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await DoWork();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DoWork");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
	
	private async Task DoWork()
    {
        _logger.LogInformation("DoWork started at {Time}", DateTimeOffset.Now);

        // Your logic here, e.g. DB/API call
        await Task.Delay(500); // Simulate I/O

        _logger.LogInformation("DoWork completed at {Time}", DateTimeOffset.Now);
    }
}
