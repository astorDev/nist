using System.Text;
using System.Text.Json;
using OneOf;

namespace Nist;

public interface IWebhook
{
    long Id { get; }
    string Url { get; }
    JsonDocument Body { get; }
}

public static class IWebhookMapper
{
    public static HttpRequestMessage ToHttpRequestMessage(this IWebhook webhook)
    {
        return new HttpRequestMessage(HttpMethod.Post, webhook.Url)
        {
            Content = new StringContent(webhook.Body.RootElement.GetRawText(), Encoding.UTF8, "application/json")
        };
    }
}

public class WebhookSender(HttpClient client, ILogger<WebhookSender> logger)
{
    public async Task<OneOf<HttpResponseMessage, Exception>> Send(IWebhook record)
    {
        try
        {
            logger.LogDebug("Sending webhook {webhookId}", record.Id);

            var request = record.ToHttpRequestMessage();
            var response = await client.SendAsync(request);

            logger.LogInformation("Sent webhook {webhookId}", record.Id);

            return response;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}