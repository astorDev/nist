using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Test {
    public readonly Client Client = new(
        new WebApplicationFactory().CreateClient(),
        new ServiceCollection()
            .AddLogging(builder => builder.AddSimpleConsole(c => c.SingleLine = true))
            .BuildServiceProvider()
            .GetRequiredService<ILogger<Client>>()
    );
}

public class WebApplicationFactory : WebApplicationFactory<Program> {

}