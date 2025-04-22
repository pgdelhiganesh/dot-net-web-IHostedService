public interface IMyScopedService
{
    Task DoScopedStuffAsync();
}

public class MyScopedService : IMyScopedService
{
    public Task DoScopedStuffAsync()
    {
        Console.WriteLine("Scoped service called.");
        return Task.CompletedTask;
    }
}
