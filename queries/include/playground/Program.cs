using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Nist;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddInMemory<Db>();
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

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

app.MapGet("/transactions", async (Db db, HttpRequest request) =>
{
    var query = TransactionsQuery.Parse(request.Query);
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

public record TransactionsQuery(
    IncludeQueryParameter? Include = null,
    int? Limit = null
)
{
    public static TransactionsQuery Parse(IQueryCollection query)
    {
        return new TransactionsQuery(
            Include: IncludeQueryParameter.Search(query),
            Limit: query.SearchInt("limit")
        );
    }

    public bool Includes(string value) => Include?.Contains(value) ?? false;
}

public static class TransactionCollectionAssembler
{
    public static async Task<TransactionGroupCollection?> GetGroups(this IQueryable<Transaction> query, IncludeQueryParameter? include) 
    {
        if (include == null) return null;
        var subpathes = include.GetSubpathes("groups").ToArray();
        if (!subpathes.Any()) return null;

        var categoryGroup = await query.SearchCategoryGroup(subpathes);
        var amountGroup = await query.SearchAmountGroup(subpathes);
        
        if (categoryGroup == null && amountGroup == null) return null;
        return new TransactionGroupCollection(
            Category: categoryGroup,
            Amount: amountGroup
        );
    }

    public static async Task<Dictionary<string, TransactionGroup>?> SearchCategoryGroup(this IQueryable<Transaction> query, Nist.IncludePath[] groupPathes)
    {
        var categoryPathes = groupPathes.GetSubpathes("category").ToArray();
        if (categoryPathes.Length == 0) return null;
        return await query.GroupBy(x => x.Category).ToTransactionGroup(categoryPathes);
    }

    public static async Task<Dictionary<string, Dictionary<string, TransactionGroup>>?> SearchAmountGroup(this IQueryable<Transaction> query, Nist.IncludePath[] groupPathes)
    {
        var amountPathes = groupPathes.GetSubpathes("amount")
            .GetExpressionSubpathes()
            .NewKeys(AmountExpression.From)
            .GroupToDictionary();

        if (amountPathes.Count == 0) return null;

        var result = new Dictionary<string, Dictionary<string, TransactionGroup>>();
        
        foreach (var pair in amountPathes)
        {
            var groups = await query.GroupBy(pair.Key.ToExpression()).ToTransactionGroup(pair.Value);
            result.Add(pair.Key.ToString(), groups);
        }

        return result;
    }
}

public record AmountExpression(string Operator, decimal Value)
{
    public const string FieldKey = "amount";
    public const string GteOperator = "gte";

    public static AmountExpression From(IncludeExpression source)
    {
        return new AmountExpression(
            Operator: source.Operator,
            Value: decimal.Parse(source.Value)
        );
    }

    public static Expression<Func<Transaction, bool>> Gte(decimal value) => x => x.Amount >= value;

    public Expression<Func<Transaction, bool>> ToExpression()
    {
        return Operator switch
        {
            GteOperator => Gte(Value),
            _ => throw new($"operator '{Operator}' not found")
        };
    }

    public override string ToString()
    {
        return $"{Operator}_{Value}";
    }
}