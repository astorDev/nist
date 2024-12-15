using Nist.Responses;

namespace SpaceX.Tests;

[TestClass]
public sealed class LaunchesShould
{
    [TestMethod]
    public async Task BeReadableAsRawString()
    {
        var http = new HttpClient {
            BaseAddress = new("https://api.spacexdata.com/v4/")
        };

        var response = await http.GetAsync("launches");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
    
    [TestMethod]
    public async Task BeReadableAsLaunchObject()
    {
        var http = new HttpClient {
            BaseAddress = new("https://api.spacexdata.com/v4/")
        };

        var launches = await http.GetAsync("launches").Read<LaunchV1[]>();
        Console.WriteLine(launches[0]);
    }
}