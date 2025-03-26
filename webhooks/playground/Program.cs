using Microsoft.EntityFrameworkCore;
using Nist;
using Nist.Bodies;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddPostgres<Db>();

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<Db>();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

app.UseRequestBodyStringReader();
app.MapWebhookDump<Db>();
app.MapWebhookDump<Db>("/webhooks/dump2");

app.Run();

public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookDump
{
    public DbSet<WebhookDump> WebhookDumps { get; set; } = null!;
}