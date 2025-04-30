using System.Text.Json;

public static class VArticle
{
    public static async Task<WebApplication> Main(WebApplicationBuilder builder)
    {        
        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
        
        builder.Services.AddPostgres<Db>();
        builder.Services.AddContinuousWebhookSending(sp => sp.GetRequiredService<Db>());
        
        var app = builder.Build();
        
        await app.Services.EnsureRecreated<Db>(async db => {
            db.WebhookRecords.Add(new WebhookRecord() {
                Url = "http://localhost:5195/webhooks/dump/from-record",
                Body = JsonDocument.Parse("{\"example\": \"one\"}")
            });

            await db.SaveChangesAsync();
        });
        
        app.UseRequestBodyStringReader();
        app.MapWebhookDump<Db>();
        
        return app;
    }

    public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookRecord<WebhookRecord>, IDbWithWebhookDump {
        public DbSet<WebhookRecord> WebhookRecords { get; set; }
        public DbSet<WebhookDump> WebhookDumps { get; set; }
    }
}