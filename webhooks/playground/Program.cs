using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nist;
using Nist.Bodies;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddPostgres<Db>();
builder.Services.AddContinuousWebhooksSending(sp => sp.GetRequiredService<Db>());

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<Db>();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

app.UseRequestBodyStringReader();
app.MapWebhookDump<Db>();
app.MapWebhookDump<Db>("/webhooks/dump2");

app.MapPost("/webhooks", async (WebhookCandidate candidate, Db db) => {
    var record = new WebhookRecord {
        Url = candidate.Url,
        Body = candidate.Body,
        Status = WebhookStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    db.WebhookRecords.Add(record);
    await db.SaveChangesAsync();
    return record;
});

app.MapGet("/webhooks", async (Db db) => {
    return await db.WebhookRecords.ToArrayAsync();
});

app.Run();

public record WebhookCandidate(
    string Url,
    JsonDocument Body
);

public class Db(DbContextOptions<Db> options) : 
    DbContext(options), 
    IDbWithWebhookDump,
    IDbWithWebhookRecord
{
    public DbSet<WebhookDump> WebhookDumps { get; set; } = null!;
    public DbSet<WebhookRecord> WebhookRecords { get; set; } = null!;
}