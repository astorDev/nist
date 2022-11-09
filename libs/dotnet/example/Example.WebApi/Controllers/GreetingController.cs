[Route(Uris.Greetings)]
public class GreetingController
{
    private readonly Random random = new Random();
    
    [HttpGet]
    public async Task<Greeting?> Get([FromQuery] GreetingQuery query)
    {
        await Task.Delay(this.random.Next(0, 50));
        var baseText = query.Language switch
        {
            Languages.Silence => null,
            Languages.English => "Hello!",
            _ => "Hello"
        };

        var signatures = query.Signatures?.Select(s => $"{s.Key}_{s.Value}").ToArray() ?? Array.Empty<string>();
        var signature = signatures.Any() ? $" from {String.Join(" ", signatures)}" : "";
        
        return baseText == null ? null : new(baseText + signature);
    }

    [HttpGet("{name}")]
    public async Task<Greeting> Get(string name)
    {
        await Task.Delay(this.random.Next(0, 10));
        if (name == "government") throw new GovernmentNotWelcomedException();
        return new($"Hi, {name}!");
    }

    [HttpPatch]
    public async Task<Greeting> Patch([FromBody] GreetingChanges changes)
    {
        await Task.Delay(this.random.Next(0, 100));
        return changes.Addressee switch
        {
            "government" => throw new GovernmentNotWelcomedException(),
            _ => new Greeting($"Hello {changes.Addressee}")
        };
    }

    public class GovernmentNotWelcomedException : Exception
    {
        public override string Message => "Don't tread on me!";
    }
}