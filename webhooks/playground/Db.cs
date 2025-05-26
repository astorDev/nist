using Microsoft.EntityFrameworkCore;
using Nist;
using Persic;

public class Db : DbContext, IDbWith<RepeatableWebhookRecord>, IDbWithWebhookDump
{
    public DbSet<RepeatableWebhookRecord> WebhookRecords { get; set; }
    public DbSet<WebhookDump> WebhookDumps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UsePostgres<Db>("Host=localhost:8951;Database=webhooks_playground;Username=webhooks_playground;Password=webhooks_playground");
    }
}