using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nist;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddPostgres<Db>();
builder.Services.AddContinuousWebhooksSending(sp => sp.GetRequiredService<Db>());

var app = builder.Build();

await app.Services.EnsureRecreated<Db>();

app.UseRequestBodyStringReader();

app.MapPost(WebhookUris.Webhooks, async (WebhookCandidate candidate, Db db) => {
    var record = new WebhookRecord() {
        Url = candidate.Url,
        Body = candidate.Body,
        CreatedAt = DateTime.UtcNow,
        Status = WebhookStatus.Pending
    };

    db.Add(record);
    await db.SaveChangesAsync();
    return record;
});

app.MapGetWebhooks<Db>();
app.MapWebhookDump<Db>();

app.Run();

public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookRecord, IDbWithWebhookDump {
    public DbSet<WebhookRecord> WebhookRecords { get; set; }
    public DbSet<WebhookDump> WebhookDumps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebhookRecord>().Property(p => p.Body).HasStringConversion();
        modelBuilder.Entity<WebhookRecord>().Property(p => p.Response!).HasStringConversion();
        modelBuilder.Entity<WebhookDump>().Property(p => p.Body).HasStringConversion();
    }
}

public static class JsonDocumentPropertyBuilderExtensions
{
    public static PropertyBuilder<JsonDocument> HasStringConversion(this PropertyBuilder<JsonDocument> builder) => 
        builder.HasConversion(
            v => v.RootElement.GetRawText(),
            v => JsonDocument.Parse(v, new())
        );
}

public record WebhookCandidate(
    JsonDocument Body,
    string Url
);