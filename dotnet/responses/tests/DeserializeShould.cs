using FluentAssertions;

namespace Nist.Responses.Tests;

[TestClass]
public class DeserializeShould
{
    public record ExampleModel(
        string Description
    );
    
    [TestMethod]
    public void DeserializeCamelCaseJson()
    {
        var json = """
                   {
                    "description" : "lol"
                   }
                   """;

        var result = Deserialize.Json<ExampleModel>(json);
        
        result!.Description.Should().Be("lol");
    }
    
    [TestMethod]
    public void DeserializeKebabCaseJson()
    {
        var json = """
                   {
                    "Description" : "lol"
                   }
                   """;

        var result = Deserialize.Json<ExampleModel>(json);
        
        result!.Description.Should().Be("lol");
    }
}


