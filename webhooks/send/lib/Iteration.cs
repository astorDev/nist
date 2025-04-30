using System.Text.Json;
using Backi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OneOf;

namespace Nist;

public class WebhookRecord : IWebhook
{
    public long Id { get; set; }
    public required string Url { get; set; }
    public required JsonDocument Body { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public int? ResponseStatusCode { get; set; }
    public JsonDocument? Response { get; set; }
}

public class WebhookStatus
{
    public const string Pending = "Pending";
    public const string Success = "Success";
    public const string Error = "Error";
}

public interface IDbWithWebhookRecord<TRecord> where TRecord : WebhookRecord
{
    DbSet<TRecord> WebhookRecords { get; }

    DatabaseFacade Database { get;}
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry Update(object record);
}

public class WebhookSendingIteration<TRecord>(IDbWithWebhookRecord<TRecord> db, WebhookSender sender, ILogger<WebhookSendingIteration<TRecord>> logger) 
    : IContinuousWorkIteration
     where TRecord : WebhookRecord
{
    public async Task Run(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting webhook sending iteration");

        using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        var pendingForUpdate = await GetPendingForUpdate();

        var processingTasks = pendingForUpdate.Select(async record => {
            var result = await sender.Send(record);
            await HandleResult(record, result);
        });

        await Task.WhenAll(processingTasks);
        await db.SaveChangesAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);

        logger.LogInformation("Webhook sending iteration finished");
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }

    public static async Task OnException(Exception ex, ILogger logger)
    {
        logger.LogError(ex, "Error in webhook sending");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    public virtual async Task HandleResult(TRecord record, OneOf<HttpResponseMessage, Exception> result)
    {
        if (result.TryPickT0(out var response, out var ex))
        {
            record.Status = response!.IsSuccessStatusCode ? WebhookStatus.Success : WebhookStatus.Error;
            record.ResponseStatusCode = (int)response.StatusCode;
            record.Response = Json.ParseSafely(await response.Content.ReadAsStreamAsync());
        }
        else
        {
            record.Status = WebhookStatus.Error;
            record.ResponseStatusCode = null;
            record.Response = JsonDocument.Parse($"{{\"error\": \"{ex!.Message}\"}}");
        }
    }

    public virtual async Task<IEnumerable<TRecord>> GetPendingForUpdate(int limit = 100)
    {
        var records = await db.WebhookRecords
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

public static class Registration
{
    public static IServiceCollection AddContinuousWebhookSending<TRecord, TIteration>(this IServiceCollection services, Func<IServiceProvider, IDbWithWebhookRecord<TRecord>> dbFactory)
        where TRecord : WebhookRecord
        where TIteration : WebhookSendingIteration<TRecord>
    {
        services.AddHttpClient();
        services.AddScoped<WebhookSender>();
        services.AddScoped(sp => dbFactory(sp));
        services.AddContinuousBackgroundService<TIteration>();

        return services;
    }

    public static IServiceCollection AddContinuousWebhookSending(this IServiceCollection services, Func<IServiceProvider, IDbWithWebhookRecord<WebhookRecord>> dbFactory)
    {
        return services.AddContinuousWebhookSending<WebhookRecord, WebhookSendingIteration<WebhookRecord>>(dbFactory);
    }
}