namespace Nist.Requests.Playground;

[TestClass]
public class HelloTests
{
    [TestMethod]
    public void MessageSync() => Message().GetAwaiter().GetResult();

    public async Task Message()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l
            .AddSimpleConsole(c => c.SingleLine = true)
            .SetMinimumLevel(LogLevel.Information)
        );
        
        services.AddHttpClient<HttpBinClient>(cl => cl.BaseAddress = new Uri("https://httpbin.org"))
            //.RemoveAllLoggers() // Removes Microsoft's Loggers
            .WithExtensiveRequestLoggingHandler<HttpBinClient>();

        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<HttpBinClient>().Http;
        await client.PostAsJsonAsync("post", new { example = "data" });
    }
}

public record HttpBinClient(HttpClient Http)
{
}