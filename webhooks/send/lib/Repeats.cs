using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Nist;

public class RepeatableWebhookRecord : WebhookRecord
{
    public int? Attempt { get; set; }
    public DateTime? StartAt { get; set; }
    public long? RepeatedFrom { get; set;}
}

public class PostgresRepeatableWebhookStore<TDb>(TDb db, Func<TDb, DbSet<RepeatableWebhookRecord>> webhookRecordSetExtractor) 
    : IWebhookStore<RepeatableWebhookRecord>
    where TDb : DbContext
{
    public async Task RunTransactionWithPendingRecords(Func<RepeatableWebhookRecord[], Task> processing, CancellationToken cancellationToken = default)
    {
        using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var pendingForUpdate = await webhookRecordSetExtractor(db)
            .FromSqlInterpolated($"""
            SELECT * 
            FROM webhook_records 
            WHERE status = {WebhookStatus.Pending}
                AND (start_at IS NULL OR start_at <= NOW())
            LIMIT 100
            FOR UPDATE SKIP LOCKED
            """)
            .ToArrayAsync();

        await processing(pendingForUpdate);

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task HandleResult(RepeatableWebhookRecord record, OneOf<HttpResponseMessage, Exception> result)
    {
        await PostgresWebhookStore<TDb>.HandleResultStatic(record, result);

        if (!result.RequiresRepeat()) return;
        
        var attemptIndex = (record.Attempt ?? 0) + 1;
        var delay = Fibonacci.At(attemptIndex);
        webhookRecordSetExtractor(db).Add(new RepeatableWebhookRecord
        {
            Url = record.Url,
            Body = record.Body,
            Status = WebhookStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Attempt = attemptIndex,
            StartAt = DateTime.UtcNow.Add(TimeSpan.FromMinutes(delay)),
            RepeatedFrom = record.Id
        });
    }
}

public static class RepeatConditionExtensions
{
    public static bool RequiresRepeat(this OneOf<HttpResponseMessage, Exception> result)
    {
        if (result.TryPickT0(out var response, out var _))
        {
            return (int)response.StatusCode >= 500;
        }
        else
        {
            return true;
        }
    }
}

public static class PostgresRepeatableWebhookStoreRegistration
{
    public static IServiceCollection AddPostgresRepeatableWebhookSending<TDb>(this IServiceCollection services, Func<TDb, DbSet<RepeatableWebhookRecord>> webhookRecordSetExtractor) where TDb : DbContext
    {
        return services.AddWebhookSending(x => {
            var db = x.GetRequiredService<TDb>();
            return new PostgresRepeatableWebhookStore<TDb>(db, x => webhookRecordSetExtractor(x));
        });
    }

    public static IServiceCollection AddPostgreRepeatableWebhookSending<TDb>(this IServiceCollection services) where TDb : DbContext, IDbWith<RepeatableWebhookRecord>
    {
        return services.AddPostgresRepeatableWebhookSending<TDb>(x => x.WebhookRecords);
    }
}