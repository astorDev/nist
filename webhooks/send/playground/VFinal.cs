public static class VFinal
{
    public static async Task<WebApplication> Main(WebApplicationBuilder builder)
    {        
        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
        
        builder.Services.AddPostgres<Db>();
        builder.Services.AddContinuousWebhookSending(sp => sp.GetRequiredService<Db>());
        
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
        
        app.MapGetWebhooks<Db, WebhookRecord>();
        app.MapWebhookDump<Db>();
        
        return app;
    }

    public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookRecord<WebhookRecord>, IDbWithWebhookDump {
        public DbSet<WebhookRecord> WebhookRecords { get; set; }
        public DbSet<WebhookDump> WebhookDumps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}