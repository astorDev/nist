using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Elnik.Protocol;

public class Client
{
    public HttpClient HttpClient { get; }

    public Client(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public Task<About> GetAbout() => this.HttpClient.GetAsync(Uris.About).Read<About>();
    
    public Task<DashboardCollection> GetDashboards() => this.HttpClient.GetAsync(Uris.Dashboards).Read<DashboardCollection>();
    public Task<Dashboard> PutDashboard(string name) => this.HttpClient.PutAsync(Uris.Dashboard(name), null).Read<Dashboard>();
    public Task<IndexCollection> GetIndexes() => this.HttpClient.GetAsync(Uris.Indexes).Read<IndexCollection>();

    public Task<string> PostIndex(string candidate) => this.HttpClient.PostAsJsonAsync(Uris.Indexes, candidate).Read<string>();
}

public static class ClientRegistration
{
    public static void AddElnikClient(this IServiceCollection services, string configurationPath = "ElnikUrl")
    {
        services.AddHttpClient<Client>((sp, cl) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var rawUrl = config[configurationPath];
            cl.BaseAddress = new(rawUrl);
        });
    }
}