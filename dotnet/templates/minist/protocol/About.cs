namespace Template;

public partial class Uris {
    public const string About = "about";
}

public partial interface IClient {
    Task<About> GetAbout();
}

public partial class Client(HttpClient http, ILogger<Client> logger) : IClient {
    public async Task<About> GetAbout() => await http.GetAsync(Uris.About).Read<About>(logger);
}

public record About(string Description, string Version, string Environment);

public partial class Errors {
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
}