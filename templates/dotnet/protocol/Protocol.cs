using System.Net.Http.Json;

namespace Template;

public partial class Uris {
}

public partial interface IClient {
}

public partial class Client(HttpClient http, ILogger<Client> logger) : IClient {
    public HttpClient Http => http;

    public async Task<T> Get<T>(string uri) => await http.GetAsync(uri).Read<T>(logger);
    public async Task<T> Post<T>(string uri, object body) => await http.PostAsJsonAsync(uri, body).Read<T>(logger);
    public async Task<T> Put<T>(string uri, object body) => await http.PutAsJsonAsync(uri, body).Read<T>(logger);
    public async Task<T> Patch<T>(string uri, object body) => await http.PatchAsJsonAsync(uri, body).Read<T>(logger);
    public async Task<T> Delete<T>(string uri) => await http.DeleteAsync(uri).Read<T>(logger);
}

public partial class Errors {
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
}