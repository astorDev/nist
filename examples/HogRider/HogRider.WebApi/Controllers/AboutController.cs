[Route(Uris.About)]
public class AboutController
{
    public IHostEnvironment Environment { get; }

    public AboutController(IHostEnvironment environment)
    {
        Environment = environment;
    }

    [HttpGet]
    public About GetAbout()
    {        
        return new(
            Description : "HogRider webapi",
            Version : this.GetType().Assembly.GetName().Version!.ToString(),
            Environment : this.Environment.EnvironmentName
        );
    }
}