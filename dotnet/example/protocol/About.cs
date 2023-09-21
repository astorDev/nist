namespace Nist.Example;

public partial class Uris {
    public const string About = "about";
}

public record About(string Description, string Version, string Environment);

public partial class Client {
    public Task<About> GetAbout() => Get<About>(Uris.About);
}