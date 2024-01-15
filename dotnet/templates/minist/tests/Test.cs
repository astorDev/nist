using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Test
{
    protected WebApplicationFactory Factory { get; } =  new();
    protected Client Client { get;  }

    protected Test()
    {
        var services = new ServiceCollection();
        services.AddLogging(l => l.AddSimpleConsole(c => c.SingleLine = true));
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Template.Client>>();
        
        Client = new(Factory.CreateClient(), logger);
    }
    
    public class WebApplicationFactory : WebApplicationFactory<Program>
    {
    }
}