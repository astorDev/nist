using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nist;

VFinal.Run(args);

public class V1 {
    public static void Run(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        var app = builder.Build();

        app.MapPost("webhooks/dump", (HttpContext context) => 
            WebhookDump.From(context));

        app.Run();
    }
}

public class V2 {
    public static void Run(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        var app = builder.Build();

        app.UseRequestBodyStringReader();

        app.MapPost("webhooks/dump", (HttpContext context) => 
            WebhookDump.From(context));

        app.Run();
    }
}

public class V3 {
    public static void Run(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        builder.Services.AddInMemoryWebhookDumpDb();

        var app = builder.Build();

        app.UseRequestBodyStringReader();

        app.MapPost("webhooks/dump", async (HttpContext context, WebhookDumpDb db) => {
                var record = WebhookDump.From(context);
                db.Add(record);
                await db.SaveChangesAsync();
                return record;
            } 
        );

        app.MapGet("webhooks/dump", async (WebhookDumpDb db) => {
                return await db.WebhookDumps.ToArrayAsync();
            }
        );

        app.Run();
    }

    public class WebhookDumpDb(DbContextOptions<WebhookDumpDb> options) 
        : DbContext(options) 
    {
        public DbSet<WebhookDump> WebhookDumps { get; set; }
    }
}

public class V4 {
    public static void Run(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        builder.Services.AddInMemoryWebhookDumpDbV4();

        var app = builder.Build();

        app.UseRequestBodyStringReader();

        app.MapPost("webhooks/dump", async (HttpContext context, WebhookDumpDb db) => {
                var record = WebhookDump.From(context);
                db.Add(record);
                await db.SaveChangesAsync();
                return record;
            } 
        );

        app.MapGet("webhooks/dump", async (WebhookDumpDb db) => {
                return await db.WebhookDumps.ToArrayAsync();
            }
        );

        app.Run();
    }

    public class WebhookDumpDb(DbContextOptions<WebhookDumpDb> options) 
        : DbContext(options) 
    {
        public DbSet<WebhookDump> WebhookDumps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebhookDump>().Property(p => p.Body)
                .HasConversion(
                    v => v.RootElement.GetRawText(),
                    v => JsonDocument.Parse(v, new())
                );
        }
    }
}

public class VFinal {
    public static void Run(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        builder.Services.AddInMemoryWebhookDumpDb();

        var app = builder.Build();

        app.UseRequestBodyStringReader();
        app.MapWebhookDump<WebhookDumpDb>();

        app.Run();
    }
}

public class VFinal {
    public static void Run(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        builder.Services.AddInMemoryWebhookDumpDb();

        var app = builder.Build();

        app.UseRequestBodyStringReader();
        app.MapWebhookDump<YourOwnDb>();

        app.Run();
    }

    public class YourOwnDb(DbContextOptions<YourOwnDb> options) : DbContext(options), IDbWithWebhookDump {
        public DbSet<WebhookDump> WebhookDumps { get; set;}
    }
}

public static class WebhookDbRegistration
{
    public static IServiceCollection AddInMemoryWebhookDumpDbV3(this IServiceCollection services)
    {
        var inMemoryDbId = Guid.NewGuid();
        services.AddDbContext<V3.WebhookDumpDb>(o => o.UseInMemoryDatabase(inMemoryDbId.ToString()));
        return services;
    }

    public static IServiceCollection AddInMemoryWebhookDumpDbV4(this IServiceCollection services)
    {
        var inMemoryDbId = Guid.NewGuid();
        services.AddDbContext<V4.WebhookDumpDb>(o => o.UseInMemoryDatabase(inMemoryDbId.ToString()));
        return services;
    }
}