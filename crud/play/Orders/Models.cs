namespace Nist;

public class Order
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public Dictionary<string, string> Labels { get; init; } = [];
}

public class OrderCandidate
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public Dictionary<string, string> Labels { get; init; } = [];
}

public partial class OrderChanges
{
    public string? Status { get; set; }
    public Dictionary<string, string>? Labels { get; set; }
}

public partial class OrderRecord
{
    public required string Id { get; init; }
    public required string Status { get; set; }
    public Dictionary<string, string> Labels { get; set; } = [];

    public Order ToProtocol() => new()
    {
        Id = Id,
        Status = Status,
        Labels = Labels
    };

    public static OrderRecord From(OrderCandidate candidate)
    {
        return new OrderRecord
        {
            Id = candidate.Id ?? Guid.NewGuid().ToString(),
            Status = candidate.Status ?? "New",
            Labels = candidate.Labels
        };
    }

    public void Apply(OrderChanges changes)
    {
        Status = changes.Status ?? Status;
        Labels = changes.Labels ?? Labels;
    }
}

public partial record OrderQuery : IQueryWithInclude
{
    public CommaSeparatedStringsParameter? Ids { get; init; }
    public CommaSeparatedStringsParameter? Statuses { get; init; }
    public int? Limit { get; init; }
    public IncludeQueryParameter? Include { get; init; }
    public DictionaryQueryParameters? Labels { get; init; }
}

public record OrderCollection
{
    public int? TotalCount { get; init; }
    public required int Count { get; init; }
    public required Order[] Items { get; init; }

    public const string TotalCountKey = "totalCount";
}

public static class QueryExtensions
{
    public static IEnumerable<OrderRecord> FilterBy(this IEnumerable<OrderRecord> source, OrderQuery query) => source
        .WhereIf(query.Ids?.Any() == true, x => query.Ids!.Inner.Contains(x.Id))
        .WhereIf(query.Statuses?.Any() == true, x => query.Statuses!.Inner.Contains(x.Status))
        .WhereIf(query.Labels?.Any() == true, x => query.Labels!.All(l => x.Labels.TryGetValue(l.Key, out var v) && v == l.Value));
}