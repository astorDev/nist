namespace Template;

public partial class Uris {
    public const string About = "about";
}

public record About(string Description, string Version, string Environment);

public partial class Client {
    public HttpClient Http { get; }
    public Client(HttpClient http) { this.Http = http; }

    public Task<About> GetAbout() => this.Http.GetAsync(Uris.About).Read<About>();
}

public partial class Errors {
    public static readonly Error Unknown = new(HttpStatusCode.InternalServerError, "Unknown");
}