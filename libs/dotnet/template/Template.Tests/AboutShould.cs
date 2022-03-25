using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Template.Tests;

[TestClass]
public class AboutShould : Test
{
    [TestMethod]
    public async Task ReturnValidMetadata()
    {
        var about = await this.Client.GetAbout();
        about.Should().BeEquivalentTo(new About(
            "Template - my webapi",
            "1.0.0.0",
            "Development"
        ));
    }
}