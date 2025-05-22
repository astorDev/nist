using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nist;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddInMemory<Db>();

var app = builder.Build();

await app.Services.EnsureRecreated<Db>(async db =>
{
    db.Transactions.Add(new() { Category = "salary", Amount = 100 });
    db.Transactions.Add(new() { Category = "salary", Amount = 200 });
    db.Transactions.Add(new() { Category = "stocks", Amount = 300 });
    db.Transactions.Add(new() { Category = "food", Amount = -100 });
    db.Transactions.Add(new() { Category = "stocks", Amount = -100 });

    await db.SaveChangesAsync();
});

app.MapGet("/transactions", async (Db db, [AsParameters] TransactionsQuery query) =>
{
    //var query = TransactionsQuery.From(rawQuery);
    IQueryable<Transaction> dbQuery = db.Transactions;

    var groups = await dbQuery.GetGroups(query.Include);
    var items = await dbQuery
        .Take(query.Limit ?? int.MaxValue)
        .ToArrayAsync();

    return new TransactionCollection(
        Count: items.Length,
        Items: items,
        Total: query.Includes("total") ? await dbQuery.CountAsync() : null,
        Groups: groups
    );
});

app.Run();

public record TransactionCollection(
    int Count,
    Transaction[] Items,
    int? Total = null,
    TransactionGroupCollection? Groups = null
);

public record TransactionGroupCollection(
    Dictionary<string, TransactionGroup>? Category = null,
    Dictionary<string, Dictionary<string, TransactionGroup>>? Amount = null
);

public record TransactionGroup(
    decimal? TotalSum,
    int? Total
);

public class Transaction
{
    public long Id { get; set; }
    public required string Category { get; set; }
    public required decimal Amount { get; set; }
}

public class Db(DbContextOptions<Db> options) : DbContext(options)  {
    public required DbSet<Transaction> Transactions { get; set; }
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

public record TransactionsQuery(
    IncludeQueryParameter? Include = null,
    int? Limit = null
)
{
    public bool Includes(string value) => Include?.Contains(value) ?? false;
}

public static class TransactionCollectionAssembler
{
    public static async Task<TransactionGroupCollection?> GetGroups(this IQueryable<Transaction> query, IncludeQueryParameter? include)
    {
        if (include == null) return null;
        var subpathes = include.GetChildren("groups").ToArray();
        if (!subpathes.Any()) return null;

        var categoryGroup = await query.SearchCategoryGroup(subpathes);
        var amountGroup = await query.SearchAmountGroup(subpathes);

        if (categoryGroup == null && amountGroup == null) return null;
        return new TransactionGroupCollection(
            Category: categoryGroup,
            Amount: amountGroup
        );
    }

    public static async Task<Dictionary<string, TransactionGroup>?> SearchCategoryGroup(this IQueryable<Transaction> query, ObjectPath[] groupPathes)
    {
        var categoryPathes = groupPathes.GetChildren("category").ToArray();
        if (categoryPathes.Length == 0) return null;
        return await query.GroupBy(x => x.Category).ToTransactionGroup(categoryPathes);
    }
}