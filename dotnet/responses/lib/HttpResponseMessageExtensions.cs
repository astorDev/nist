using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Nist.Responses;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> Read<T>(this Task<HttpResponseMessage> responseTask, ILogger? logger = null)
    {
        var result = await ReadNullable<T>(responseTask, logger);
        return result ?? throw new InvalidOperationException("deserialization resulted in null");
    }

    public static async Task<T?> ReadNullable<T>(this Task<HttpResponseMessage> responseTask, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        
        using var response = await responseTask;
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("{HttpMethod} {Url} > {StatusCode} {ResponseBody}", response.RequestMessage!.Method, response.RequestMessage!.RequestUri!.PathAndQuery, response.StatusCode, body);
            throw new UnsuccessfulResponseException(response.StatusCode, response.Headers, body);
        }

        logger.LogInformation("{HttpMethod} {Url} > {StatusCode} {ResponseBody}", response.RequestMessage!.Method, response.RequestMessage!.RequestUri!.PathAndQuery, response.StatusCode, body);
        var result = JsonConvert.DeserializeObject<T>(body);
        return result;
    }
}