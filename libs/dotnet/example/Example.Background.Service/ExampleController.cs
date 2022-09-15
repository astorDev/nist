public class ExampleController
{
    readonly IClient client;
    public ExampleController(IClient client) {
        this.client = client;
    }
    
    [RunsEvery("00:00:01")]
    public async Task<string> UsePatch()
    {
        var patched = await client.PatchGreeting(new() { Addressee = Guid.NewGuid().ToString() });
        return patched.Text;
    }
}