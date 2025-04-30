using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Nist;

public class RepeatableWebhookRecord : WebhookRecord
{
    public int? Attempt { get; set; }
    public DateTime? StartAt { get; set; }
    public long? RepeatedFrom { get; set;}
}

public class RepeatableWebhookSendingIteration(
    IDbWithWebhookRecord<RepeatableWebhookRecord> db, 
    WebhookSender sender, 
    ILogger<WebhookSendingIteration<RepeatableWebhookRecord>> logger) 
    : WebhookSendingIteration<RepeatableWebhookRecord>(db, sender, logger)
{
    public override async Task HandleResult(RepeatableWebhookRecord record, OneOf<HttpResponseMessage, Exception> result)
    {
        await base.HandleResult(record, result);

        if (!result.RequiresRepeat()) return;
        
        var attemptIndex = (record.Attempt ?? 0) + 1;
        var delay = Fibonacci.At(attemptIndex);
        db.WebhookRecords.Add(new RepeatableWebhookRecord
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

    public override async Task<IEnumerable<RepeatableWebhookRecord>> GetPendingForUpdate(int limit = 100)
    {
        var records = await db.WebhookRecords
            .FromSqlInterpolated($"""
            SELECT * 
            FROM webhook_records 
            WHERE status = {WebhookStatus.Pending} 
                AND (start_at IS NULL OR start_at <= NOW())
            LIMIT {limit}
            FOR UPDATE SKIP LOCKED
            """)
            .ToArrayAsync();

        return records;
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

public static class RepeatableRegistration
{
    public static IServiceCollection AddContinuousRepeatableWebhookSending(this IServiceCollection services, Func<IServiceProvider, IDbWithWebhookRecord<RepeatableWebhookRecord>> dbFactory)
    {
        return services.AddContinuousWebhookSending<RepeatableWebhookRecord, RepeatableWebhookSendingIteration>(dbFactory);
    }
}