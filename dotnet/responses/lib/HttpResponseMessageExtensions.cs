using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nist.Responses;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> Read<T>(this Task<HttpResponseMessage> responseTask, ILogger? logger = null, JsonSerializerOptions? serializerOptions = null)
    {
        var result = await ReadNullable<T>(responseTask, logger, serializerOptions);
        return result ?? throw new InvalidOperationException("deserialization resulted in null");
    }

    public static async Task<T> Read<T>(this HttpResponseMessage response, ILogger? logger = null, JsonSerializerOptions? serializerOptions = null)
    {
        var result = await ReadNullable<T>(response, logger, serializerOptions);
        return result ?? throw new InvalidOperationException("deserialization resulted in null");
    }
    
    public static async Task<T?> ReadNullable<T>(this Task<HttpResponseMessage> responseTask, ILogger? logger = null, JsonSerializerOptions? serializerOptions = null)
    {
        using var response = await responseTask;
        return await response.ReadNullable<T>(logger, serializerOptions);
    }
    
    public static async Task<T?> ReadNullable<T>(this HttpResponseMessage response, ILogger? logger = null, JsonSerializerOptions? serializerOptions = null)
    {
        logger ??= NullLogger.Instance;
        
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("{HttpMethod} {Url} > {StatusCode} {ResponseBody}", response.RequestMessage!.Method, response.RequestMessage!.RequestUri!.PathAndQuery, response.StatusCode, body);
            throw new UnsuccessfulResponseException(response.StatusCode, response.Headers, body);
        }

        logger.LogInformation("{HttpMethod} {Url} > {StatusCode} {ResponseBody}", response.RequestMessage!.Method, response.RequestMessage!.RequestUri!.PathAndQuery, response.StatusCode, body);
        
        var result = Deserialize.Json<T>(body, serializerOptions);
        return result;
    }
}

