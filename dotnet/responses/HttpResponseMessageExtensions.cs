using Newtonsoft.Json;

namespace Nist.Responses;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> Read<T>(this Task<HttpResponseMessage> responseTask)
    {
        var result = await ReadNullable<T>(responseTask);
        return result ?? throw new InvalidOperationException("deserialization resulted in null");
    }

    public static async Task<T?> ReadNullable<T>(this Task<HttpResponseMessage> responseTask)
    {
        using var response = await responseTask;
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UnsuccessfulResponseException(response.StatusCode, response.Headers, body);
        }

        var result = JsonConvert.DeserializeObject<T>(body);
        return result;
    }
}