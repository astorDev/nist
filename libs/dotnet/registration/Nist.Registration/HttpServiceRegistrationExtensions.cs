using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpServiceRegistrationExtensions
{
    public static void AddHttpService<TService>(this IServiceCollection services, string urlConfigurationPath, Action<HttpClient>? configuration = null) where TService : class
    {
        services.AddHttpClient<TService>((sp, cl) => ConfigureClient(sp, cl, urlConfigurationPath, configuration));
    }

    public static void AddHttpService<TInterface, TService>(this IServiceCollection services, string urlConfigurationPath, Action<HttpClient>? configuration = null) where TService : class, TInterface where TInterface : class
    {
        services.AddHttpClient<TInterface, TService>((sp, cl) => ConfigureClient(sp, cl, urlConfigurationPath, configuration));
    }
    
    private static void ConfigureClient(IServiceProvider serviceProvider, HttpClient client, string urlConfigurationPath, Action<HttpClient>? adjustment = null)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var url = configuration[urlConfigurationPath];
        var uri = new Uri(url);
        client.BaseAddress = uri;
        adjustment?.Invoke(client);
    }
}