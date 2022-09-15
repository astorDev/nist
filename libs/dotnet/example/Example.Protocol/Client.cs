namespace Example;

public interface IClient
{
    Task<About> GetAbout();

    Task<Greeting?> GetGreeting(GreetingQuery query);

    Task<Greeting> PatchGreeting(GreetingChanges changes);
}

public class Client : IClient
{
    public HttpClient HttpClient { get; }

    public Client(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public Task<About> GetAbout() => this.HttpClient.GetAsync(Uris.About).Read<About>();
    public Task<Greeting?> GetGreeting(GreetingQuery query) => this.HttpClient.GetAsync(QueryUri.From(Uris.Greetings, query)).ReadNullable<Greeting>();
    public Task<Greeting> PatchGreeting(GreetingChanges changes) => this.HttpClient.PatchAsJsonAsync(Uris.Greetings, changes).Read<Greeting>();
}