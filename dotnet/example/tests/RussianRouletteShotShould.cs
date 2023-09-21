namespace Nist.Example.Tests;

[TestClass]
public class RussianRouletteShotShould : Test
{
    [TestMethod]
    public async Task BeIdleAllTheTime() { // except when it isn't
        var shot = await Client.GetRussianRouletteShot();
        shot.Idle.Should().Be(true);
    }
}