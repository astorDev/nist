namespace Nist.Template.Playground;

[TestClass]
public class HelloTests
{
    [TestMethod]
    public void Message()
    {
        var hello = "Hello, Tests!";

        Console.WriteLine(hello);

        hello.ShouldBe("Hello, Tests!");
    }
}