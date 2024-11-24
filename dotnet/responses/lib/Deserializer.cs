using System.Text.Json;

namespace Nist.Responses;

public static class Deserialize
{
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public static T? Json<T>(string json, JsonSerializerOptions? serializerOptions = null)
    {
        serializerOptions ??= DefaultJsonSerializerOptions;
        var result = JsonSerializer.Deserialize<T>(json, serializerOptions);
        return result;
    }
}