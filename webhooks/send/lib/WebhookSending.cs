using System.Text;
using System.Text.Json;
using Backi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nist;

public class WebhookRecord
{
    public long Id { get; set; }
    public required string Url { get; set; }
    public required JsonDocument Body { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Status { get; set; }
    public int? ResponseStatusCode { get; set; }
    public JsonDocument? Response { get; set; }
}

public interface IDbWithWebhookRecord
{
    DbSet<WebhookRecord> WebhookRecords { get; }

    DatabaseFacade Database { get;}
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry Update(object record);
}

public static class WebhookQueryableExtensions
{
    public static async Task<WebhookRecord[]> GetPendingForUpdate(this DbSet<WebhookRecord> query, int limit = 100)
    {
        var records = await query
            .FromSqlInterpolated($"""
            SELECT * 
            FROM webhook_records 
            WHERE status = {WebhookStatus.Pending}
            LIMIT {limit}
            FOR UPDATE SKIP LOCKED
            """)
            .ToArrayAsync();

        return records;
    }
}

public class WebhookStatus
{
    public const string Pending = "Pending";
    public const string Success = "Success";
    public const string Error = "Error";
}

public class WebhooksSendingIteration(IDbWithWebhookRecord db, IHttpClientFactory httpClientFactory, ILogger<WebhooksSendingIteration> logger) : IContinuousWorkIteration
{
    public static async Task OnException(Exception ex, ILogger logger)
    {
        logger.LogError(ex, "Error in WebhooksSendingIteration");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    public async Task Run(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting WebhooksSendingIteration");

        using var transaction = await db.Database.BeginTransactionAsync();
        var pendingForUpdate = await db.WebhookRecords.GetPendingForUpdate();

        logger.LogInformation("Extracted {recordsCount} pending records", pendingForUpdate.Length);

        var sendTasks = pendingForUpdate.Select(wh => Task.Run(async () => {
            db.Update(wh);
            await SendPendingWebhook(wh);
        }));

        await Task.WhenAll(sendTasks);

        logger.LogInformation("Sent {recordsCount} pending records", pendingForUpdate.Length);

        await db.SaveChangesAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);

        logger.LogInformation("Saved {recordsCount} pending records, Sleeping for 1 sec", pendingForUpdate.Length);

        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }

    private async Task SendPendingWebhook(WebhookRecord record)
    {
        var client = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, record.Url) {
            Content = new StringContent(record.Body.RootElement.GetRawText(), Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response;

        try
        {
            response = await client.SendAsync(request);
        }
        catch(Exception ex)
        {
            record.Status = WebhookStatus.Error;
            record.ResponseStatusCode = null;
            record.Response = JsonDocument.Parse($"{{\"error\": \"{ex.Message}\"}}");
            return;
        }

        record.Status = response.IsSuccessStatusCode ? WebhookStatus.Success : WebhookStatus.Error;
        record.ResponseStatusCode = (int)response.StatusCode;
        record.Response = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }
}

public static class WebhookSendingRegistration
{
    public static void AddContinuousWebhooksSending(this IServiceCollection services, Func<IServiceProvider, IDbWithWebhookRecord> dbFactory)
    {
        services.AddHttpClient();
        services.AddScoped(provider => dbFactory(provider));
        services.AddContinuousBackgroundService<WebhooksSendingIteration>();
    }
}