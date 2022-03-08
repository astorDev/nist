namespace Template.Protocol;

public class Client
{
    public HttpClient HttpClient { get; }

    public Client(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public async Task<About> GetAbout()
    {
        var response = await this.HttpClient.GetAsync(Uris.About);
        return await response.Read<About>();
    }
}