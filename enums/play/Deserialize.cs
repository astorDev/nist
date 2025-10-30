using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nist.Enums.Playground;

[TestClass]
public class Deserialize
{
    private static JsonSerializerOptions options = new ()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [TestMethod]
    public void NormalString()
    {
        var json = """
        {
            "status": "InProgress"
        }
        """;

        var result = JsonSerializer.Deserialize<PlayStrict>(json, options);
        result.ShouldNotBeNull();
        Console.WriteLine(result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void UpperSnakeString()
    {
        var json = """
        {
            "status": "IN_PROGRESS"
        }
        """;

        JsonSerializer.Deserialize<PlayStrict>(json, options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void ForwardString()
    {
        var json = """
        {
            "status": "Failed"
        }
        """;

        JsonSerializer.Deserialize<PlayStrict>(json, options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void ForwardStringFlexible()
    {
        var json = """
        {
            "status": "Failed"
        }
        """;

        JsonSerializer.Deserialize<PlayFlexible>(json, options);
    }

    [TestMethod]
    public void ForwardInt()
    {
        var json = """
        {
            "status": 13
        }
        """;

        var result = JsonSerializer.Deserialize<PlayStrict>(json, options);
        result.ShouldNotBeNull();
        Console.WriteLine(result.Status);
    }
}

public enum PlayStatus
{
    Accepted,
    InProgress,
    Success
}

public class PlayStatuses
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
}

public class PlayStrict
{
    public required PlayStatus Status { get; set; }
}

public class PlayFlexible
{
    public PlayStatus? Status { get; set; }
}