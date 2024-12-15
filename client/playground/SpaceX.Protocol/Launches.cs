using Microsoft.Extensions.Logging;
using Nist.Responses;

namespace SpaceX;

public record Launch(string Name, bool? Success);

public class Uris {
    public const string Launches = "launches";
    public const string Latest = "latest";

    public static string LatestLaunch => $"{Launches}/{Latest}";
}

public class Client(HttpClient http, ILogger<Client> logger) {
    public async Task<Launch> GetLatestLaunch() => await GetAsync<Launch>(Uris.LatestLaunch);

    public Task<T> GetAsync<T>(string uri) => http.GetAsync(uri).Read<T>(logger);
}