namespace HogRider.Tests;

[TestClass]
public class AboutShould : Test
{
    [TestMethod]
    public async Task ReturnValidMetadata()
    {
        var about = await this.Client.GetAbout();
        about.Should().BeEquivalentTo(new About(
            "HogRider webapi",
            "1.0.0.0",
            "Development"
        ));
    }
}