using System.Text.Json;

public static class VExtensive
{
    public static async Task<WebApplication> Main(WebApplicationBuilder builder)
    {
        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        builder.Services.AddPostgres<Db>();
        builder.Services.AddPostgreRepeatableWebhookSending<Db>();

        var app = builder.Build();

        await app.Services.EnsureRecreated<Db>(async db => {
            db.WebhookRecords.Add(new RepeatableWebhookRecord() {
                Url = "http://localhost:5195/webhooks/dump/from-record",
                Body = JsonDocument.Parse("{\"example\": \"one\"}")
            });

            db.WebhookRecords.Add(new RepeatableWebhookRecord() {
                Url = "http://localhost:5195/failure",
                Body = JsonDocument.Parse("{\"example\": \"fail\"}")
            });

            await db.SaveChangesAsync();
        });

        app.UseRequestBodyStringReader();

        app.MapPost(WebhookUris.Webhooks, async (WebhookCandidate candidate, Db db) => {
            var record = new RepeatableWebhookRecord() {
                Url = candidate.Url,
                Body = candidate.Body
            };

            db.WebhookRecords.Add(record);
            await db.SaveChangesAsync();
            return record;
        });

        app.MapGetWebhooks<Db, RepeatableWebhookRecord>();
        app.MapWebhookDump<Db>();
        app.MapPost("/failure", () => Results.InternalServerError("Simulated failure"));

        return app;
    }

    public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWith<RepeatableWebhookRecord>, IDbWithWebhookDump {
        public DbSet<RepeatableWebhookRecord> WebhookRecords { get; set; }
        public DbSet<WebhookDump> WebhookDumps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}


