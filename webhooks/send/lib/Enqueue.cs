using System.Text.Json;
using Confi;
using Microsoft.EntityFrameworkCore;

namespace Nist;

public static class WebhookSendingExtensions
{
    public static string[] GetWebhookAddresses(this IConfiguration configuration, string configurationPath = "WebhookAddresses", string addressSeparator = ";")
    {
        var value = configuration.GetRequiredValue(configurationPath);
        var addresses = value.Split(addressSeparator, StringSplitOptions.RemoveEmptyEntries);
        return addresses;
    }

    public static JsonDocument JsonDocumentFrom(object body)
    {
        return JsonDocument.Parse(JsonSerializer.Serialize(body, JsonSerializerOptions.Web));
    }

    public static RepeatableWebhookRecord[] Enqueue(this DbSet<RepeatableWebhookRecord> queue, object body, string path, params string[] baseAddresses)
    {
        return queue.Enqueue(
            JsonDocumentFrom(body),
            path,
            baseAddresses
        );
    }

    public static RepeatableWebhookRecord[] Enqueue(this DbSet<RepeatableWebhookRecord> queue, JsonDocument body, string path, params string[] baseUrls)
    {
        var result = new List<RepeatableWebhookRecord>();

        foreach (var baseUrl in baseUrls)
        {
            var url = Path.Combine(baseUrl, path.TrimStart('/'));
            var webhookRecord = new RepeatableWebhookRecord
            {
                Url = url,
                Body = body,
                Status = WebhookStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            queue.Add(webhookRecord);

            result.Add(webhookRecord);
        }

        return [.. result];
    }
}