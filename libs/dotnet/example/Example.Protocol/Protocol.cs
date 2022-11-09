namespace Example;

public class Uris {
    public const string About = "about";
    public const string Greetings = "greetings";
    public const string Farewells = "farewells";
    public const string Companies = "companies";
    public const string Office = "office";
}

public record About(string Description, string Version, string Environment);
public record Greeting(string Text);
public record GreetingQuery(string? Language = null, Dictionary<string, string>? Signatures = null);
public record GreetingChanges(string? Addressee = null);

public class Languages {
    public const string English = "english";
    public const string Silence = "silence";
}

public interface IClient {
    Task<About> GetAbout();
    Task<Greeting?> GetGreeting(GreetingQuery query);
    Task<Greeting> PatchGreeting(GreetingChanges changes);
}

public class Client : IClient {
    readonly HttpClient http;
    public Client(HttpClient http) => this.http = http;

    public Task<About> GetAbout() => this.http.GetAsync(Uris.About).Read<About>();
    public Task<Greeting?> GetGreeting(GreetingQuery query) => this.http.GetAsync(QueryUri.From(Uris.Greetings, query)).ReadNullable<Greeting>();
    public Task<Greeting> PatchGreeting(GreetingChanges changes) => this.http.PatchAsJsonAsync(Uris.Greetings, changes).Read<Greeting>();
}

public class Errors {
    public static Error GovernmentNotWelcomed => new(HttpStatusCode.BadRequest, "GovernmentNotWelcomed");
    public static Error Unknown => new(HttpStatusCode.InternalServerError, "Unknown");
    public static Error GivingUpIsNotAllowed => new(HttpStatusCode.BadRequest, "GivingUpIsNotAllowed");
}