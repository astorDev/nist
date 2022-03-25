namespace Example.Protocol;

public record About(string Description, string Version, string Environment);

public record EmptyQuery(bool Empty = true);

public record Greeting(string Text);

public record GreetingQuery(string? Language = null);

public record GreetingChanges(string? Addressee = null);

public class Languages
{
    public const string English = "english";
    public const string Silence = "silence";
}