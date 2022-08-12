namespace Elnik.Tests;

[TestClass]
public class AboutShould : Test
{
    [TestMethod]
    public async Task ReturnValidMetadata()
    {
        var about = await this.Client.GetAbout();
        about.Should().BeEquivalentTo(new About(
            "Elnik webapi",
            "1.0.0.0",
            "Development"
        ));
    }
}