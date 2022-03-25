[Route(Uris.Greetings)]
public class GreetingController
{
    [HttpGet]
    public Greeting? Get([FromQuery] GreetingQuery query)
    {
        return query.Language switch
        {
            Languages.Silence => null,
            Languages.English => new Greeting("Hello!"),
            _ => new Greeting("Hello")
        };
    }

    [HttpPatch]
    public Greeting Patch([FromBody] GreetingChanges changes)
    {
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