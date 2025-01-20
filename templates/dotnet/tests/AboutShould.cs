namespace Template.Tests;

[TestClass]
public class AboutShould : Test
{
    [TestMethod]
    public async Task ReturnValidMetadata()
    {
        var about = await this.Client.GetAbout();
        about.ShouldBe(new(
            "Template",
            "1.0.0.0",
            "Development"
        ));
    }
}