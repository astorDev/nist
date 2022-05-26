[Route(Uris.Greetings)]
public class GreetingController
{
    private readonly Random random = new Random();
    
    [HttpGet]
    public async Task<Greeting?> Get([FromQuery] GreetingQuery query)
    {
        await Task.Delay(this.random.Next(0, 50));
        return query.Language switch
        {
            Languages.Silence => null,
            Languages.English => new Greeting("Hello!"),
            _ => new Greeting("Hello")
        };
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