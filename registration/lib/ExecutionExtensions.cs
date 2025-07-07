using Microsoft.Extensions.DependencyInjection;

namespace Nist;

public static class ServiceExecutionExtensions
{
    public static async Task ExecuteWithScoped<TService>(
        this IServiceProvider services,
        Func<TService, Task> action
    )
        where TService : notnull
    {
        using var scope = services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await action(service);
    }

    public static void ExecuteWithScoped<TService>(
        this IServiceProvider services,
        Action<TService> action
    )
        where TService : notnull
    {
        using var scope = services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        action(service);
    }
}