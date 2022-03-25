namespace Example.Tests;

[TestClass]
public class GreetingShould : Test
{
    [TestMethod]
    public async Task ReturnValidGreetingAsync()
    {
        var greeting = await this.Client.GetGreeting(new());
        greeting!.Text.Should().Be("Hello");
    }

    [TestMethod]
    public async Task ReturnNullForSilenceLanguage()
    {
        var greeting = await this.Client.GetGreeting(new(Language : Languages.Silence));
        greeting.Should().BeNull();
    }

    [TestMethod]
    public async Task ReturnBadRequestForGovernment()
    {
        var exception = await Assert.ThrowsExceptionAsync<UnsuccessfulResponseException>(
            () => this.Client.PatchGreeting(new (Addressee : "government"))
           );
           
        exception.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        exception.DeserializedBody<Error>()!.Reason.Should().Be(Error.GovernmentNotWelcomed.Reason);
    }
}