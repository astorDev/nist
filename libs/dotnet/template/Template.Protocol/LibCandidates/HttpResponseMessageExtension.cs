namespace Template.Protocol;

public static class HttpResponseMessageExtension
{
    public static async Task<T> Read<T>(this HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<T>(json);
        return result ?? throw new InvalidOperationException("deserialization resulted in null object");
    }
}