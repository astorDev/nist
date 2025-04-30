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

public class WebhookSender(IHttpClientFactory httpClientFactory, ILogger<WebhookSender> logger)
{
    public async Task<OneOf<HttpResponseMessage, Exception>> Send(IWebhook record)
    {
        try
        {
            logger.LogDebug("Sending webhook {webhookId}", record.Id);

            var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, record.Url) {
                Content = new StringContent(record.Body.RootElement.GetRawText(), Encoding.UTF8, "application/json")
            };

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