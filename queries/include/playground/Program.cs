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
    IQueryable<TransactionRecord> dbQuery = db.Transactions;

    int? total = null;
    Dictionary<string, object>? groups = new();

    if (query.Includes("total"))
    {
        total = await dbQuery.CountAsync();
    }

    var groupPathes = query.Include?.GetSubpathes("groups") ?? [];
    var categoryPathes = groupPathes.GetSubpathes("category");

    if (categoryPathes.Any())
        groups.Add("category", await dbQuery.GroupBy(x => x.Category).ToTransactionGroup(categoryPathes));

    var expressionPaths = groupPathes.GetExpressionSubpathes();
    var amountExpressionPaths = expressionPaths.Where(x => x.Key.Field == "amount");

    foreach (var x in amountExpressionPaths)
    {
        var amExp = AmountExpression.From(x.Key);
        groups.Add(amExp.ToString(), await dbQuery.GroupBy(amExp.ToExpression()).ToTransactionGroup(x.Value));
    }

    var items = await dbQuery
        .Limit(query.Limit)
        .ToArrayAsync();

    return new TransactionCollection(
        Count: items.Length,
        Items: items,
        Total: total,
        Groups: groups
    );
});

app.Run();

public record IncludeExpression(
    string Field,
    string Operator,
    string Value
)
{
    public static bool TryParse(string includePath, out IncludeExpression expression)
    {
        expression = null!;
        var parts = includePath.Split("_");
        if (parts.Length != 3) return false;

        expression = new IncludeExpression(
            parts[0],
            parts[1],
            parts[2]
        );

        return true;
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

    public static bool TryParse(IncludeExpression expression, out AmountExpression amountExpression)
    {
        amountExpression = null!;
        if (expression.Field != "amount") return false;

        amountExpression = new AmountExpression(
            Operator: expression.Operator,
            Value: decimal.Parse(expression.Value)
        );

        return true;
    }

    public static Expression<Func<TransactionRecord, bool>> Gte(decimal value) => x => x.Amount >= value;

    public Expression<Func<TransactionRecord, bool>> ToExpression()
    {
        return Operator switch
        {
            GteOperator => Gte(Value),
            _ => throw new($"operator '{Operator}' not found")
        };
    }

    public override string ToString()
    {
        return $"{FieldKey}_{Operator}_{Value}";
    }
}

public static class IQueryableExtensions
{
    public static IQueryable<T> Limit<T>(this IQueryable<T> query, int? limit) =>
        limit == null ? query : query.Take(limit.Value);
}

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

public record IncludeQueryParameter(
    Path[] Paths
)
{
    public static IncludeQueryParameter Parse(string value)
    {
        var rawValues = value.Split(",");
        var paths = rawValues.Select(v => Path.Parse(v));

        return new IncludeQueryParameter([.. paths]);
    }

    public static IncludeQueryParameter? Search(IQueryCollection query)
    {
        var raw = query.SearchString("include");
        return raw == null ? null : Parse(raw);
    }

    public bool Contains(string key) => TryGetSubpath(key, out _);
    public bool TryGetSubpath(string key, out Path? subpath)
    {
        var path = Paths.FirstOrDefault(p => p.Root == key);
        subpath = path?.Subpath;
        return path != null;
    }

    public IEnumerable<Path> GetSubpathes(string key) => this.Paths.GetSubpathes(key);
}

public static class PathEnumerable
{
    public static IEnumerable<Path> GetSubpathes(this IEnumerable<Path> pathes, string key)
    {
        return pathes
            .Where(p => p.Root == key && p.Subpath != null)
            .Select(p => p.Subpath!);
    }

    public static Dictionary<IncludeExpression, IEnumerable<Path>> GetExpressionSubpathes(this IEnumerable<Path> pathes)
    {
        var result = new List<KeyValuePair<IncludeExpression, Path>>();

        foreach (var path in pathes)
        {
            if (IncludeExpression.TryParse(path.Root, out var expression))
            {
                if (path.Subpath != null)
                {
                    result.Add(new(expression, path.Subpath!));
                }
            }
        }

        return result
            .GroupBy(x => x.Key)
            .ToDictionary(gr => gr.Key, gr => gr.Select(x => x.Value));
    }

    public static bool Have(this IEnumerable<Path> pathes, string key)
    {
        return pathes.Any(p => p.Root == key);
    }
}

public record Path(
    string Root,
    Path? Subpath
)
{
    public static Path Parse(string value)
    {
        var parts = value.Split(".");
        return Parse(parts);
    }

    public static Path Parse(string[] parts)
    {
        return new Path(
            Root: parts[0],
            Subpath: parts.Length > 1 ? Path.Parse([.. parts.Skip(1)]) : null
        );
    }
}

public record TransactionCollection(
    int Count,
    TransactionRecord[] Items,
    int? Total = null,
    Dictionary<string, object>? Groups = null
);

public static class QueryCollectionExtensions
{
    public static int? SearchInt(this IQueryCollection query, string name)
    {
        var value = query.FirstOrDefault(q => q.Key == name);
        return value.Key == null ? null : int.Parse(value.Value!);
    }

    public static string? SearchString(this IQueryCollection query, string name)
    {
        var value = query.FirstOrDefault(q => q.Key == name);
        return value.Key == null ? null : value.Value.First();
    }

    public static DateTime? SearchDateTime(this IQueryCollection query, string name)
    {
        var value = query.FirstOrDefault(q => q.Key == name);
        return value.Key == null ? null : DateTime.Parse(value.Value!).ToUniversalTime();
    }
}