namespace Template;

public partial class Uris{
}

public partial interface IClient {
}

public partial class Client : IClient {
    public HttpClient Http { get; }
    public Client(HttpClient http) { this.Http = http; }
}

public partial class Errors {
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
}