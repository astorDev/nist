namespace Nist;

public partial class Db
{
    public static Dictionary<string, OrderRecord> OrderRecords { get; } = [];
}

public partial class Uris
{
    public const string Orders = "orders";
    public static string Order(string id) => $"{Orders}/{id}";
}

public static partial class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrders(this IEndpointRouteBuilder app)
    {
        app.MapPost(Uris.Orders, OrderEndpoints.PostOrder);
        app.MapPut(Uris.Orders, OrderEndpoints.PutOrder);
        app.MapGet(Uris.Order("{id}"), OrderEndpoints.GetOrder);
        app.MapGet(Uris.Orders, OrderEndpoints.GetOrders);
        app.MapPatch(Uris.Order("{id}"), OrderEndpoints.PatchOrder);
        app.MapDelete(Uris.Order("{id}"), OrderEndpoints.DeleteOrder);
        return app;
    }

    public static Order PostOrder(OrderCandidate candidate)
    {
        var record = OrderRecord.From(candidate);
        if (!Db.OrderRecords.TryAdd(record.Id, record)) throw new OrderAlreadyExistsException(record.Id);
        return record.ToProtocol();
    }

    public static Order PutOrder(OrderCandidate candidate)
    {
        var record = OrderRecord.From(candidate);
        Db.OrderRecords[record.Id] = record;
        return record.ToProtocol();
    }

    public static Order PatchOrder(string id, OrderChanges changes)
    {
        if (!Db.OrderRecords.TryGetValue(id, out var record)) throw new OrderNotFoundException(id);
        record.Apply(changes);
        return record.ToProtocol();
    }

    public static Order DeleteOrder(string id)
    {
        if (!Db.OrderRecords.TryGetValue(id, out var record)) throw new OrderNotFoundException(id);
        Db.OrderRecords.Remove(id);
        return record.ToProtocol();
    }
    
    public static Order GetOrder(string id)
    {
        if (!Db.OrderRecords.TryGetValue(id, out var record)) throw new OrderNotFoundException(id);
        return record.ToProtocol();
    }

    public static OrderCollection GetOrders([AsParameters] OrderQuery query)
    {
        var filtered = Db.OrderRecords.Values.FilterBy(query);
        var items = filtered.Take(query.Limit ?? int.MaxValue).ToArray();
        return new OrderCollection
        {
            TotalCount = query.Includes(OrderCollection.TotalCountKey) ? filtered.Count() : null,
            Count = items.Length,
            Items = items.Map(x => x.ToProtocol())
        };
    }
}

public partial class Errors
{
    public readonly static Error OrderAlreadyExists = new(HttpStatusCode.BadRequest, "OrderAlreadyExists");
}

public class OrderAlreadyExistsException(string id) : NistException(Errors.OrderAlreadyExists, new () { { "id", id } });

public partial class Errors
{
    public readonly static Error OrderNotFound = new(HttpStatusCode.NotFound, "OrderNotFound");
}

public class OrderNotFoundException(string id) : NistException(Errors.OrderNotFound, new () { { "id", id } });