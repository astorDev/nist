public class Test
{
    protected WebApplicationFactory Factory { get; } =  new();
    protected Client Client { get;  }

    protected Test()
    {
        Client = new(Factory.CreateClient());
    }
    
    public class WebApplicationFactory : WebApplicationFactory<Program>
    {
    }
}