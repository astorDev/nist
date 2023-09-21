namespace Nist.Example.Tests;

[TestClass]
public class AboutShould : Test
{
    [TestMethod]
    public async Task ReturnValidMetadata()
    {
        var about = await Client.GetAbout();
        about.Should().Be(new About(
            "Example Nist WebApi",
            "1.0.0",
            "Development"
        ));
    }
}