[Route(Uris.About)]
public class AboutController
{
    public IHostEnvironment Environment { get; }
    
    public AboutController(IHostEnvironment environment) {
        this.Environment = environment;
    }

    [HttpGet]
    public About GetAbout()
    {        
        return new(
            Description : "Template webapi",
            Version : this.GetType().Assembly.GetName().Version!.ToString(),
            Environment : this.Environment.EnvironmentName
        );
    }
}