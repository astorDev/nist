using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nist;

public class WebhookDumpDb(DbContextOptions<WebhookDumpDb> options) : DbContext(options), IDbWithWebhookDump {
    public DbSet<WebhookDump> WebhookDumps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebhookDump>().Property(p => p.Body)
            .HasConversion<string>(
                v => v.RootElement.GetRawText(),
                v => JsonDocument.Parse(v, new())
            );
    }
}

public static class WebhookDbRegistration
{
    public static IServiceCollection AddInMemoryWebhookDumpDb(this IServiceCollection services)
    {
        var inMemoryDbId = Guid.NewGuid();
        services.AddDbContext<WebhookDumpDb>(o => o.UseInMemoryDatabase(inMemoryDbId.ToString()));
        return services;
    }
}