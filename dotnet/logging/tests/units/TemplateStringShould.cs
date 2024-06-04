namespace Nist.Logs.Tests;

[TestClass]
public class TemplateStringShould
{
    [TestMethod]
    public void ParseValidTemplateSuccessfully()
    {
        var template = IOLoggingSettings.MessageTemplate.Parse("{Method} {Uri} > {ResponseCode}");
        Assert.AreEqual("{Method} {Uri} > {ResponseCode}", template.Message);
        CollectionAssert.AreEqual(new[] { "Method", "Uri", "ResponseCode" }, template.OrderedKeys);
    }
}