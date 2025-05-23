using Microsoft.EntityFrameworkCore;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

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
        Total: query.Includes("total") ? await dbQuery.CountAsync() : null
    );
});

app.Run();

public record TransactionCollection(
    int Count,
    Transaction[] Items,
    int? Total = null
);

public record TransactionsQuery(
    string[]? Include = null,
    int? Limit = null,
    string? Category = null
)
{
    public bool Includes(string value) => Include?.Contains(value) ?? false;
}