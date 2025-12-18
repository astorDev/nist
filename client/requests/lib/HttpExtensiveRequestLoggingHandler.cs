using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nist;

public class HttpExtensiveRequestLoggingHandler<T> : DelegatingHandler
{
    private readonly ILogger _logger;

    public HttpExtensiveRequestLoggingHandler(ILoggerFactory logger)
    {
        var categoryName = "Nist.HttpExtensiveRequestLoggingHandler." + typeof(T).Name;
        _logger = logger.CreateLogger(categoryName);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string content = "";
        if (request.Content != null)
        {
            content = await request.Content.ReadAsStringAsync(CancellationToken.None);
        }

        _logger.LogInformation("Sending request: {Method} {Uri} {Content}", request.Method, request.RequestUri, content);
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder WithExtensiveRequestLoggingHandler<T>(this IHttpClientBuilder builder)
    {
        builder.Services.AddTransient(typeof(HttpExtensiveRequestLoggingHandler<>));
        return builder.AddHttpMessageHandler<HttpExtensiveRequestLoggingHandler<T>>();
    }
}


