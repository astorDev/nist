namespace Template;

public partial class Uris {
    public const string About = "about";
}

public partial interface IClient {
    Task<About> GetAbout();
}

public partial class Client {
    public async Task<About> GetAbout() => await Get<About>(Uris.About);
}

public record About(
    string Description, 
    string Version, 
    string Environment,
    Dictionary<string, object> Dependencies
);