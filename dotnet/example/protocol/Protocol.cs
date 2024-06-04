using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Nist.Example;

public partial class Uris{
}

public partial interface IClient {
}

public partial class Client : IClient
{
    readonly HttpClient _http;
    readonly ILogger<Client> _logger;
    public Client(HttpClient http, ILogger<Client> logger) { _http = http; _logger = logger; }

    public Task<T> Get<T>(string uri) => _http.GetAsync(uri).Read<T>(_logger);
    public Task<T> Post<T>(string uri, object body) => _http.PostAsJsonAsync(uri, body).Read<T>(_logger);
}

public partial class Errors {
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
}