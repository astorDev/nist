namespace Nist;

public record Thing
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public required DateTime CreationTime { get; init; }
    public required DateTime LastModificationTime { get; init; }
}

public record ThingCandidate
{
    public string? Id { get; set; }
    public required string Status { get; init; }
    public DateTime? Time { get; init; }
}

public record ThingChanges
{
    public string? Status { get; init; }
    public DateTime? ModificationTime { get; init; }
}

public record ThingQuery : IQueryWithInclude
{
    public string? Status { get; init; }
    public DateTime? CreationTimeSince { get; init; }
    public DateTime? CreationTimeUntil { get; init; }
    public DateTime? LastModificationTimeSince { get; init; }
    public DateTime? LastModificationTimeUntil { get; init; }


    public int? Limit { get; init; }
    public int? Skip { get; init; }

    public IncludeQueryParameter? Include { get; init; }
}

public record ThingCollection
{
    public int? TotalCount { get; init; }
    public required int Count { get; init; }
    public required Thing[] Items { get; init; }

    public const string TotalCountKey = "totalCount";
}

public partial record ThingRecord
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required DateTime CreationTime { get; init; }
    public required DateTime LastModificationTime { get; init; }

    public Thing ToResponse() => new()
    {
        Id = Id,
        Status = Name,
        CreationTime = CreationTime,
        LastModificationTime = LastModificationTime
    };

    public static ThingRecord From(ThingCandidate candidate, DateTime? creationTime = null, DateTime? lastModificationTime = null) => new()
    {
        Id = candidate.Id ?? Guid.NewGuid().ToString(),
        Name = candidate.Status,
        CreationTime = creationTime ?? DateTime.UtcNow,
        LastModificationTime = lastModificationTime ?? DateTime.UtcNow
    };
}

public partial class Db
{
    public static Dictionary<string, ThingRecord> Things { get; } = [];
}

public partial class Uris
{
    public const string Things = "things";
    public static string Thing(string id) => $"{Things}/{id}";
}

public static class SomethingEndpoints
{
    public static IEndpointRouteBuilder MapThingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(Uris.Things, PostThing);
        app.MapGet(Uris.Thing("{id}"), GetThing);
        app.MapGet(Uris.Things, GetThings);
        app.MapPatch(Uris.Thing("{id}"), PatchThing);
        app.MapPut(Uris.Thing("{id}"), PutThing);
        app.MapDelete(Uris.Thing("{id}"), DeleteThing);

        return app;
    }

    public static Thing PostThing(ThingCandidate candidate)
    {
        if (Db.Things.ContainsKey(candidate.Id ?? "")) throw new BadHttpRequestException(Errors.ThingAlreadyExists.Reason, (int)Errors.ThingAlreadyExists.Code);
        var record = ThingRecord.From(candidate);
        Db.Things[record.Id] = record;
        return record.ToResponse();
    }

    public static Thing GetThing(string id)
    {
        var record = Db.Things.GetValueOrDefault(id) ?? throw new BadHttpRequestException(Errors.ThingNotFound.Reason, (int)Errors.ThingNotFound.Code);
        return record.ToResponse();
    }

    public static ThingCollection GetThings([AsParameters] ThingQuery query)
    {
        var filtered = Db.Things.Values
            .WhereIf(query.Status is not null, x => x.Name == query.Status)
            .WhereIf(query.CreationTimeSince is not null, x => x.CreationTime >= query.CreationTimeSince)
            .WhereIf(query.CreationTimeUntil is not null, x => x.CreationTime <= query.CreationTimeUntil)
            .WhereIf(query.LastModificationTimeSince is not null, x => x.LastModificationTime >= query.LastModificationTimeSince)
            .WhereIf(query.LastModificationTimeUntil is not null, x => x.LastModificationTime < query.LastModificationTimeUntil);

        var extracted = filtered
            .OrderByDescending(x => x.LastModificationTime)
            .Skip(query.Skip ?? 0)
            .Take(query.Limit ?? int.MaxValue)
            .ToArray();

        return new ThingCollection
        {
            TotalCount = query.Includes(ThingCollection.TotalCountKey) ? filtered.Count() : null,
            Count = extracted.Length,
            Items = [.. extracted.Select(x => x.ToResponse())]
        };
    }

    public static Thing PatchThing(string id, ThingChanges changes)
    {
        var record = Db.Things.GetValueOrDefault(id) ?? throw new BadHttpRequestException(Errors.ThingNotFound.Reason, (int)Errors.ThingNotFound.Code);

        var updated = record with
        {
            Name = changes.Status ?? record.Name,
            LastModificationTime = changes.ModificationTime ?? DateTime.UtcNow
        };

        Db.Things[id] = updated;

        return updated.ToResponse();
    }

    public static Thing PutThing(string id, ThingCandidate candidate)
    {
        var existing = Db.Things.GetValueOrDefault(id);
        candidate.Id = id;
        var record = ThingRecord.From(candidate, creationTime: existing?.CreationTime, lastModificationTime: candidate.Time);
        Db.Things[record.Id] = record;
        return record.ToResponse();
    }

    public static Thing DeleteThing(string id)
    {
        if (!Db.Things.TryGetValue(id, out var record)) throw new BadHttpRequestException(Errors.ThingNotFound.Reason, (int)Errors.ThingNotFound.Code);
        if (!Db.Things.Remove(id)) throw new BadHttpRequestException(Errors.ThingNotFound.Reason, (int)Errors.ThingNotFound.Code);

        return record.ToResponse();
    }
}

public partial class Errors
{
    public static Error ThingNotFound = new(HttpStatusCode.BadRequest, "ThingNotFound");
    public static Error ThingAlreadyExists = new(HttpStatusCode.BadRequest, "ThingAlreadyExists");
}

public static class Extension
{
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
    {
        if (condition)
        {
            return source.Where(predicate);
        }
        return source;
    }
}