namespace IceWizard.Protocol;

public class Client
{
    public HttpClient HttpClient { get; }

    public Client(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public Task<About> GetAbout() => this.HttpClient.GetAsync(Uris.About).Read<About>();
}