global using QueryEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>;
using Microsoft.EntityFrameworkCore;

namespace Nist;

public static class WebhookEndpoints {
    public static IEndpointRouteBuilder MapGetWebhooks<TDb, TRecord>(this IEndpointRouteBuilder app) where TDb : IDbWith<TRecord> where TRecord : WebhookRecord {
        app.MapGet($"/{WebhookUris.Webhooks}", GetWebhooks<TDb, TRecord>);

        return app;
    }

    public static async Task<WebhookCollection<TRecord>> GetWebhooks<TDb, TRecord>(HttpRequest request, TDb db) where TDb : IDbWith<TRecord> where TRecord : WebhookRecord{
        var query = WebhookQuery.Parse(request.Query);

        var counters = await db.WebhookRecords
            .GroupBy(r => r.Status)
            .Select(g => new {
                Status = g.Key,
                Count = g.Count()
            })
            .ToArrayAsync();

        var selected = await db.WebhookRecords
            .OrderByDescending(r => r.Id)
            .Take(query.Limit ?? WebhookQuery.DefaultLimit)
            .ToArrayAsync();

        return new WebhookCollection<TRecord>(
            TotalCounts: counters.ToDictionary(c => c.Status.ToLower(), c => c.Count),
            Count: selected.Length,
            Items: selected
        );
    }
}

public class WebhookUris {
    public const string Webhooks = "webhooks";
}

public record WebhookQuery(
    int? Limit = null
)
{
    public const int DefaultLimit = 100;

    public static WebhookQuery Parse(IQueryCollection source) => new(
        Limit: source.SearchInt(nameof(Limit))
    );
}

public record WebhookCollection<TRecord>(
    Dictionary<string, int> TotalCounts,
    int Count,
    TRecord[] Items
);

public static class QueryCollectionExtensions
{
    public static int? SearchInt(this QueryEnumerable query, string key)
    {
        var value = query.Search(key);
        return value == null ? null : int.Parse(value!);
    }

    public static string? Search(this QueryEnumerable query, string key)
    {
        var pair = query.FirstOrDefault(q => string.Equals(q.Key, key, StringComparison.InvariantCultureIgnoreCase));
        return pair.Value.Where(x => x != null).FirstOrDefault();
    }
}

