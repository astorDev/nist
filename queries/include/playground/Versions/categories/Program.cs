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
    IQueryable<Transaction> dbQuery = db.Transactions
        .Where(x => query.Category == null || x.Category == query.Category);

    var items = await dbQuery
        .Take(query.Limit ?? int.MaxValue)
        .ToArrayAsync();

    return new TransactionCollection(
        Count: items.Length,
        Items: items,
        Total: query.Includes("total") ? await dbQuery.CountAsync() : null,
        Categories: query.Includes("categories")
            ? await dbQuery.GroupBy(x => x.Category).ToTransactionGroup(query.Include!.GetChildren("categories"))
            : null
    );
});

app.Run();

public record TransactionCollection(
    int Count,
    Transaction[] Items,
    int? Total = null,
    Dictionary<string, TransactionGroup>? Categories = null
);

public record TransactionGroup(
    decimal? TotalSum,
    int? Total,
    Transaction[]? Items = null
);

public record TransactionsQuery(
    IncludeQueryParameter? Include = null,
    string? Category = null,
    int? Limit = null
)
{
    public bool Includes(string value) => Include?.Have(value) ?? false;
}

public class GroupAggregateDbResult
{
    public required string Key { get; set; }
    public decimal? TotalSum { get; set; }
    public int? Total { get; set; }
}

public static class GroupAggregateExtensions
{
    public static async Task<Dictionary<string, TransactionGroup>> ToTransactionGroup<TKey>(this IQueryable<IGrouping<TKey, Transaction>> query, IEnumerable<ObjectPath> includes)
    {
        var aggregate = await query.Select(cgr => new GroupAggregateDbResult
        {
            Key = cgr.Key!.ToString()!,
            TotalSum = includes.Have("totalSum") ? cgr.Sum(x => x.Amount) : null,
            Total = includes.Have("total") ? cgr.Count() : null
        }).ToArrayAsync();

        return aggregate.ToResponseDictionary();
    }
    
    public static Dictionary<string, TransactionGroup> ToResponseDictionary(this IEnumerable<GroupAggregateDbResult> dbResults)
    {
        return dbResults.ToDictionary(row => row.Key, row => new TransactionGroup (
            row.TotalSum,
            row.Total
        ));
    }
}