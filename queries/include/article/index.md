# REST API Solution to Over-Fetching: .NET 9 Example

> Ditch GraphQL, Use This Instead!

![All we need is ...](thumb.png)

GraphQL appeared in 2012 as a new API architecture, allowing you to "give clients the power to ask for exactly what they need and nothing more". It was solving the problem, commonly known as over-fetching, which is considered a weak point of REST architecture.

However, there is a solution to this problem in REST, which is much more elegant, in my view. In this article, we are going to investigate the solution and make a practical example in C#. So, without further ado, let's get started!

> Or jump straight to the [TLDR](#tldr) at the end of this article for a minimized example.

## The Solution: Include Query Parameter

Let's say we have a transactions API. So, as you might expect from a REST API, it is extremely easy to get started and see the list of all the transactions. You just call the `GET /transactions` method. We'll assume we have just 5 records in the system and this is what the endpoint will return: 

```json
{
  "count": 5,
  "items": [
    {
      "id": 1,
      "category": "salary",
      "amount": 100
    },
    {
      "id": 2,
      "category": "salary",
      "amount": 200
    },
    {
      "id": 3,
      "category": "stocks",
      "amount": 300
    },
    {
      "id": 4,
      "category": "food",
      "amount": -100
    },
    {
      "id": 5,
      "category": "stocks",
      "amount": -100
    }
  ]
}
```

Of course, REST is well-known to be good at simple queries. But what if we want to introduce pagination? It is likely we'll need to get a `total` number of rows to show how many pages we have. 

The easiest solution would be to just add the parameter to the response. However, it will mean that we will need to do an additional query on **every** `GET /transactions` request. Gladly, there is a solution that is also pretty simple, but is much more flexible: 

> Allow inclusion of additional information via an `include` parameter.

Here's what an example pagination query might look like:

```http
GET /transactions?limit=2&include=total
```

And here's what the response will be:

```json
{
  "count": 2,
  "items": [
    {
      "id": 1,
      "category": "salary",
      "amount": 100
    },
    {
      "id": 2,
      "category": "salary",
      "amount": 200
    }
  ],
  "total": 5
}
```

So far so good. But using `include` for the `total` count doesn't fully demonstrate the power of the parameter. The true power is demonstrated best by aggregated queries.

For example, let's say we want to see just the total sum and number of transactions by category, not the transactions themselves. Here's how our request will look:

```http
GET /transactions?limit=0&include=categories.total,categories.totalSum
```

```json
{
  "count": 0,
  "items": [],
  "categories": {
    "salary": {
      "totalSum": 300,
      "total": 2
    },
    "stocks": {
      "totalSum": 200,
      "total": 2
    },
    "food": {
      "totalSum": -100,
      "total": 1
    }
  }
}
```

As you might see in this example, the `include` parameter enables versatile customization of a `GET` endpoint for various use cases. Moreover, it gives control over that customization to the client — something GraphQL brags about.

I hope you find the `include` parameter as powerful as I do. You might be wondering how hard it is to implement the parameter on the server side. Let me show you an example implementation in the next sections.

## Setting Up An Example API

To start, let's initiate a new project. We'll use the most minimalistic template:

```sh
dotnet new web
```

Let's also add those two common cosmetic customizations to our app:

```csharp
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});
```

Finally, we'll need two EntityFramework NuGet packages: one for an in-memory database and the second for a simplified database setup:

```sh
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Persic.EF
```

Now, we are ready to go. Let's start by defining our database model:

```csharp
using Microsoft.EntityFrameworkCore;

public class Transaction
{
    public long Id { get; set; }
    public required string Category { get; set; }
    public required decimal Amount { get; set; }
}

public class Db(DbContextOptions<Db> options) : DbContext(options)  {
    public required DbSet<Transaction> Transactions { get; set; }
}

public static class InMemoryDbRegistration
{
    public static IServiceCollection AddInMemory<TDb>(this IServiceCollection services) where TDb : DbContext
    {
        var inMemoryDbId = Guid.NewGuid();
        services.AddDbContext<TDb>(o => o.UseInMemoryDatabase(inMemoryDbId.ToString()));
        return services;
    }
}
```

Then, our API models:

```csharp
public record TransactionCollection(
    int Count,
    Transaction[] Items
);

public record TransactionsQuery(
    int? Limit = null
);
```

Finally, let's assemble those together to create a very simple API, prefilled with the transactions we've seen before:

```csharp
using Microsoft.EntityFrameworkCore;
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
    IQueryable<Transaction> dbQuery = db.Transactions;

    var items = await dbQuery
        .Take(query.Limit ?? int.MaxValue)
        .ToArrayAsync();

    return new TransactionCollection(
        Count: items.Length,
        Items: items
    );
});

app.Run();
```

For now, we don't have the `include` parameter, just a single endpoint returning our transactions and letting us `limit` the number of transactions returned like this:

```http
GET /transactions?limit=2
```

Here's what we should get in response:

```json
{
  "count": 2,
  "items": [
    {
      "id": 1,
      "category": "salary",
      "amount": 100
    },
    {
      "id": 2,
      "category": "salary",
      "amount": 200
    }
  ]
}
```

The setup is done, let's move to the interesting part!

## Enabling Pagination: include=total

As you might remember from the first section, the first problem we are going to solve is getting the total number of items for pagination. We'll need an optional `Total` response parameter and an optional `Include` parameter. Let's even increase the complexity and let a client to filter transactions by a category. Here's what our API model will look like:

```csharp
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
```

And the logic update is very simple, as well. Let me just show you the code:

```csharp
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
```

Let's check how this works. Let's say we want only the total count, we can use this query:

```http
GET /transactions?limit=0&include=total
```

Here's the response we'll get:

```json
{
  "count": 0,
  "items": [],
  "total": 5
}
```

Let's also try to combine our `include` with a `category` filter:

```http
GET http://localhost:5058/transactions?limit=1&include=total&category=salary
```

We will get the first record with the `salary` category and the `total` number of items in the category:

```json
{
  "count": 1,
  "items": [
    {
      "id": 1,
      "category": "salary",
      "amount": 100
    }
  ],
  "total": 2
}
```

Enabling pagination is great, but let's move to the advanced parts with category groups.

## Building Aggregates: include=categories.totalSum

In this section, we'll allow a client to `include` category groups in the response. Here's the updated response model we'll have:

```csharp
public record TransactionGroup(
    decimal? TotalSum,
    int? Total
);

public record TransactionCollection(
    int Count,
    Transaction[] Items,
    int? Total = null,
    Dictionary<string, TransactionGroup>? Categories = null
);
```

As you might remember from the examples, we will need a slightly more complex `include` parameter to achieve that: with comma-separated nested paths.

In the [previous article about queries in .NET](https://medium.com/@vosarat1995/net-minimal-api-broke-fromquery-1326e0aa50b4), we've talked about creating custom query parameters. In short, the class should have `public static bool TryParse(string source, out TQueryParameter queryParameter)` method. Let's use the technique for `include` parameter!

In the article, I've also introduced a helper package, called `Nist.Queries`, that will be handy now as well. Let's install it first:

```sh
dotnet add reference Nist.Queries;
```

Then, using the `ObjectPath` and `CommaSeparatedParameters<T>` objects from this package, we should be able to create our custom `IncludeQueryParameter`:

```csharp
using Nist;

public record IncludeQueryParameter(CommaSeparatedParameters<ObjectPath> Inner) : CommaSeparatedParameters<ObjectPath>(Inner)
{
    public static bool TryParse(string source, out IncludeQueryParameter includeQueryParameter)
    {
        includeQueryParameter = Parse(source);
        return true;
    }

    public static IncludeQueryParameter Parse(string source) => new(
        Parse(source, x => ObjectPath.Parse(x))
    );

    public override string ToString() => base.ToString();
}
```

Of course, we'll need to update `TransactionQuery` accordingly:

```csharp
public record TransactionsQuery(
    IncludeQueryParameter? Include = null,
    int? Limit = null,
    string? Category = null
)
{
    public bool Includes(string value) => Include?.Have(value) ?? false;
}
```

Now, to the actual logic implementation!

First, we'll need a database model for our group query result:

```csharp
public class GroupAggregateDbResult
{
    public required string Key { get; set; }
    public decimal? TotalSum { get; set; }
    public int? Total { get; set; }
}
```

Then, we'll build an extension method on `IQueryable` returning the data and a small helper method turning the data into our response object:

```csharp
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
```

Here's how we will use the method:

> `IEnumerable<ObjectPath>.GetChildren(string rootKey)` is an extension method inside the `Nist.Queries` package.

```csharp
query.Includes("categories")
  ? await dbQuery.GroupBy(x => x.Category).ToTransactionGroup(query.Include!.GetChildren("categories"))
  : null
```

And here's how we'll need to update our endpoint code:

```csharp
return new TransactionCollection(
    Count: items.Length,
    Items: items,
    Total: query.Includes("total") ? await dbQuery.CountAsync() : null,
    Categories: query.Includes("categories")
        ? await dbQuery.GroupBy(x => x.Category).ToTransactionGroup(query.Include!.GetChildren("categories"))
        : null
);
```

With this in place, we will be able to request only the total sum and the number of items by categories with the query like this:

```http
GET /transactions?limit=0&include=categories.total,categories.totalSum
```

With our data it should give us the following response:

```json
{
  "count": 0,
  "items": [],
  "categories": {
    "salary": {
      "totalSum": 300,
      "total": 2
    },
    "stocks": {
      "totalSum": 200,
      "total": 2
    },
    "food": {
      "totalSum": -100,
      "total": 1
    }
  }
}
```

And this is the last piece of code we are going to write in this article. Let me do a quick recap of the things we've done and give you a few useful links in the last section.

## TLDR;

In this article, I've proposed a REST solution to over-fetching: The `include` query parameter. With this parameter a client can opt-in for additional pieces of data from the server. For example, if a client needs `total` number of items for pagination and total sum by categories for an overview look, the client can send a request like this:

```http
GET /transactions?limit=2&include=total,categories.total,categories.totalSum
```

A server can then return those additional fields along with the default `count` and `items` fields:

```json
{
  "count": 2,
  "items": [
    {
      "id": 1,
      "category": "salary",
      "amount": 100
    },
    {
      "id": 2,
      "category": "salary",
      "amount": 200
    }
  ],
  "total": 5,
  "categories": {
    "salary": {
      "totalSum": 300,
      "total": 2
    },
    "stocks": {
      "totalSum": 200,
      "total": 2
    },
    "food": {
      "totalSum": -100,
      "total": 1
    }
  }
}
```

Implementing the behaviour on the server side is relatively easy, here's the code of the endpoint:

```csharp
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
```

You can see the rest of the implementation code in the article above or check it out [here on GitHub](https://github.com/astorDev/nist/blob/main/queries/include/playground/Program.cs). To streamline working with the `include` parameter we have implemented `IncludeQueryParameter` in the article. If you don't want to recreate it, you can just use the `Nist.Queries.Include` NuGet package from the same project.

This example, package, and even this article are part of the [NIST project](https://github.com/astorDev/nist). The project contains many HTTP-related tools beyond queries — check it out and don't hesitate to give the repository a star! ⭐

Claps for this article are also highly appreciated! 😉