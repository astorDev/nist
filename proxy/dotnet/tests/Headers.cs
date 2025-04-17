using Nist;

namespace Tests;

[TestClass]
public sealed class Headers
{
    [TestMethod]
    public async Task InContentAndResponse()
    {
        var client = new HttpClient() { BaseAddress = new Uri("https://api.spacexdata.com/v4") };
        var response = await client.GetAsync("launches/latest");

        Console.WriteLine("\n\n------Headers directly in response:-----\n\n");
        foreach (var header in response.Headers)
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");

        Console.WriteLine("\n\n------Headers in content:----------\n\n");
        foreach (var header in response.Content.Headers)
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
    }
}
