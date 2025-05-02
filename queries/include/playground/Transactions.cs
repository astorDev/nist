using Microsoft.EntityFrameworkCore;

namespace Nist;

public class TransactionRecord
{
    public long Id { get; set; }
    public required string Category { get; set; }
    public required decimal Amount { get; set; }
}

public class Db(DbContextOptions<Db> options) : DbContext(options)  {
    public required DbSet<TransactionRecord> Transactions { get; set; }
}

public static class WebhookDbRegistration
{
    public static IServiceCollection AddInMemory<TDb>(this IServiceCollection services) where TDb : DbContext
    {
        var inMemoryDbId = Guid.NewGuid();
        services.AddDbContext<TDb>(o => o.UseInMemoryDatabase(inMemoryDbId.ToString()));
        return services;
    }
}