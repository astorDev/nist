using System.Text.Json;

using Nist;

public static class Json
{
    /// <summary>
    /// Because .NET team decided not to implement proper TryParse: https://github.com/dotnet/runtime/issues/82605
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static JsonDocument? ParseSafely(this Stream stream)
    {
        try {
            return JsonDocument.Parse(stream);
        }
        catch {
            return null;
        }
    }
}