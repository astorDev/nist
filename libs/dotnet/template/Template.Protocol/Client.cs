namespace Template;

public class Client
{
    public HttpClient Http { get; }
    public Client(HttpClient http) { this.Http = http; }

    public Task<About> GetAbout() => this.Http.GetAsync(Uris.About).Read<About>();
}