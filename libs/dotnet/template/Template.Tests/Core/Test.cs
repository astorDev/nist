class Test
{
    protected readonly WebApplicationFactory Factory = new();
    protected readonly Client Client;

    protected Test()
    {
        Client = new(Factory.CreateClient());
    }
}