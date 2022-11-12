namespace Template;

public class Uris{
    public const string About = "about";
}

public record About(string Description, string Version, string Environment);

public interface IClient {
    Task<About> GetAbout();
}

public class Client : IClient {
    public HttpClient Http { get; }
    public Client(HttpClient http) { this.Http = http; }

    public Task<About> GetAbout() => this.Http.GetAsync(Uris.About).Read<About>();
}

public class Errors {
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
}