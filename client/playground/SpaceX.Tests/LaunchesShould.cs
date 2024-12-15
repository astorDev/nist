using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nist.Responses;

namespace SpaceX.Tests;

[TestClass]
public sealed class LaunchesShould
{
    readonly ILogger logger;
    readonly Client client;
    
    public LaunchesShould()
    {
        var services = new ServiceCollection()
            .AddLogging(l => l.AddSimpleConsole(c => c.SingleLine = true))
            .AddHttpClient<Client>(cl => cl.BaseAddress = new("https://api.spacexdata.com/v4/")).Services
            .BuildServiceProvider();

        logger = services.GetRequiredService<ILogger<LaunchesShould>>();
        client = services.GetRequiredService<Client>();
    }
    
    [TestMethod]
    public async Task ReturnLatestAsRawString()
    {
        var http = new HttpClient {
            BaseAddress = new("https://api.spacexdata.com/v4/")
        };

        var response = await http.GetAsync("launches/latest");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
    
    [TestMethod]
    public async Task ReturnLatestAsLaunchObject()
    {
        var http = new HttpClient {
            BaseAddress = new("https://api.spacexdata.com/v4/")
        };

        var response = await http.GetAsync(Uris.LatestLaunch);
        var launch = await response.Read<Launch>();
        Console.WriteLine(launch);
    }
    
    [TestMethod]
    public async Task ReturnLatestWithLogging()
    {
        var http = new HttpClient {
            BaseAddress = new("https://api.spacexdata.com/v4/")
        };

        var response = await http.GetAsync(Uris.LatestLaunch);
        var launch = await response.Read<Launch>(logger);
        Console.WriteLine(launch);
    }
    
    [TestMethod]
    public async Task ReturnLatestFromTypedClient()
    {
        var launch = await client.GetLatestLaunch();
        Console.WriteLine(launch);
    }
}