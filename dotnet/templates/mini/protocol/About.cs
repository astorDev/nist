namespace Template;

public partial class Uris{
    public const string About = "about";
}

public partial interface IClient {
    Task<About> GetAbout();
}

public partial class Client : IClient {
    public Task<About> GetAbout() => this.Http.GetAsync(Uris.About).Read<About>();
}

public record About(string Description, string Version, string Environment);