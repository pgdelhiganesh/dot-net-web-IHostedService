// Here's how you can create an InitService that implements IHostedService and runs only once at application startup, then stops.
// If you want a DoWork() method that executes once every 5 seconds, and stops afterward (i.e., just one execution, delayed by 5 seconds).
// Inject Scoped Services (e.g., DbContext, HttpClient)
// Stop After X Runs or on Condition

// Since IHostedService is a singleton, you can’t inject scoped services directly into its constructor — but you can resolve them inside DoWorkAsync() using a scope.
/*
Quick Notes:
Concept                        | Explanation
IServiceProvider.CreateScope() | Creates a DI scope so you can resolve scoped services safely.
GetRequiredService<T>()        | Resolves the scoped service instance within that scope.
Dispose()                      | Ensures scoped services are cleaned up after use.
*/

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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class InitService : IHostedService
{
    private readonly ILogger<InitService> _logger;
	private readonly IHostEnvironment _env;
	private readonly IServiceProvider _serviceProvider;
	private CancellationTokenSource _cts;
    private Task _backgroundTask;
	private int _runCount = 0;
    private readonly int _maxRuns = 10; // Change this to however many times you want DoWork to run
	
    public InitService(ILogger<InitService> logger, IHostEnvironment env, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _env = env;
		_serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("InitService starting in {Environment} environment...", _env.EnvironmentName);

        /// Your init logic that run once
        await InitializeAsync();
		
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundTask = Task.Run(() => RunAsync(_cts.Token));

        _logger.LogInformation("InitService completed.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
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
    }

    private async Task InitializeAsync()
    {
        // Simulate some async init work
        await Task.Delay(2000);
        _logger.LogInformation("Initialization logic finished.");
    }
	
    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _runCount < _maxRuns)
        {
            try
            {
                await DoWork();
				_runCount++;
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
		 _logger.LogInformation("DoWork #{Run} started at {Time}", _runCount + 1, DateTimeOffset.Now);

		// Use a scoped service (e.g., DbContext, HttpClientFactory, etc.)
        using var scope = _serviceProvider.CreateScope(); // <== auto-disposed

        // Example: get a service from DI
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        await dbContext.SomeDbOperationAsync();
		
		// Example: get a service from DI
		var myService = scope.ServiceProvider.GetRequiredService<IMyScopedService>();
		await myService.DoScopedStuffAsync();
	
        // Your logic here, e.g. DB/API call
        await Task.Delay(500); // Simulate I/O

		// scope.Dispose() is called automatically here at the end of using block
		
        _logger.LogInformation("DoWork #{Run} completed at {Time}", _runCount + 1, DateTimeOffset.Now);
    }
}
