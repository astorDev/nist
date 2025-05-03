using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Nist;

public class WebhookRecord : IWebhook
{
    public long Id { get; set; }
    public required string Url { get; set; }
    public required JsonDocument Body { get; set; }
    public string Status { get; set; } = WebhookStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? ResponseStatusCode { get; set; }
    public JsonDocument? Response { get; set; }
}

public class WebhookStatus
{
    public const string Pending = "Pending";
    public const string Success = "Success";
    public const string Error = "Error";
}

public class PostgresWebhookStore<TDb>(TDb db, Func<TDb, DbSet<WebhookRecord>> webhookRecordSetExtractor) 
    : IWebhookStore<WebhookRecord>
    where TDb : DbContext
{
    public async Task RunTransactionWithPendingRecords(Func<WebhookRecord[], Task> processing, CancellationToken cancellationToken = default)
    {
        using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var pendingForUpdate = await webhookRecordSetExtractor(db)
            .FromSqlInterpolated($"""
            SELECT * 
            FROM webhook_records 
            WHERE status = {WebhookStatus.Pending}
            LIMIT 100
            FOR UPDATE SKIP LOCKED
            """)
            .ToArrayAsync();

        await processing(pendingForUpdate);

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task HandleResult(WebhookRecord record, OneOf<HttpResponseMessage, Exception> result)
    {
        await HandleResultStatic(record, result);
    }

    public static async Task HandleResultStatic(WebhookRecord record, OneOf<HttpResponseMessage, Exception> result)
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
}

public interface IDbWith<T> where T : class {
    public DbSet<T> WebhookRecords { get; }
}

public static class PostgresWebhookStoreRegistration
{
    public static IServiceCollection AddPostgresWebhookSending<TDb>(this IServiceCollection services, Func<TDb, DbSet<WebhookRecord>> webhookRecordSetExtractor) where TDb : DbContext
    {
        return services.AddWebhookSending(x => {
            var db = x.GetRequiredService<TDb>();
            return new PostgresWebhookStore<TDb>(db, x => webhookRecordSetExtractor(x));
        });
    }

    public static IServiceCollection AddPostgresWebhookSending<TDb>(this IServiceCollection services) where TDb : DbContext, IDbWith<WebhookRecord>
    {
        return services.AddPostgresWebhookSending<TDb>(x => x.WebhookRecords);
    }
}