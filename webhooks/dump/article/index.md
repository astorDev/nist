# Webhook Testing in C#: Your Own WireMock Alternative

> Building a Simple Webhook Dump using .NET to Quickly Mock and Test a Webhook Acceptor.

Webhooks are a nice and flexible way to implement an event-based integration. However, it's relatively tricky to test webhook connections, both for a sender and for the receiver. Wouldn't it be nice to quickly set up a simple webhook dump to see all the incoming and outgoing requests? In this article, we'll build such a dump using .NET, giving you full control over webhook testing in no time - no external tools like Postman or WireMock needed.

> For the quick solution, jump straight to the end of the article, to the [TLDR; section](#tldr)

## Webhook Dump Object: Reading HttpContext

```sh
dotnet new web
```

```csharp
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
```

```sh
dotnet add package Nist.Bodies
```

> We will make an EF-friendly model from the get-go, hence the `Id` and the auto-properties look.

```csharp
public class WebhookDump
{
    public int Id { get; set; }
    public required string Path { get; set; }
    public required JsonDocument Body { get; set; }
    public required DateTime Time { get; set; }

    public static WebhookDump From(HttpContext context) => new()
    {
        Path = context.Request.Path.ToString(),
        Body = JsonDocument.Parse(context.GetRequestBodyString()),
        Time = DateTime.UtcNow
    };
}
```

```csharp
app.MapPost("webhooks/dump", (HttpContext context) => 
    WebhookDump.From(context));
```

```http
POST /webhooks/dump

{
    "experiment" : "alpha"
}
```

```text
System.InvalidOperationException: Request body is not available. Make sure you've registered the required middleware with `UseRequestBodyStringReader()`
```

## Webhook Dump Object: Fixing It with UseRequestBodyStringReader

```csharp
app.UseRequestBodyStringReader();
```

```json
{
  "id": 0,
  "path": "/webhooks/dump",
  "body": {
    "experiment": "alpha"
  },
  "time": "2025-04-15T14:18:54.034791Z"
}
```

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.UseRequestBodyStringReader();

app.MapPost("webhooks/dump", (HttpContext context) => 
            WebhookDump.From(context));

app.Run();
```

## Storing Webhook Dump. Part 1: Introducing an In-Memory Database

```sh
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

```csharp
public class WebhookDumpDb(DbContextOptions<WebhookDumpDb> options) 
    : DbContext(options) 
{
    public DbSet<WebhookDump> WebhookDumps { get; set; }
}
```

```csharp
public static class WebhookDbRegistration
{
    public static IServiceCollection AddInMemoryWebhookDumpDb(this IServiceCollection services)
    {
        var inMemoryDbId = Guid.NewGuid();
        services.AddDbContext<WebhookDumpDb>(o => o.UseInMemoryDatabase(inMemoryDbId.ToString()));
        return services;
    }
}
```

```csharp
// ...

builder.Services.AddInMemoryWebhookDumpDb();

// ...

app.MapPost("webhooks/dump", async (HttpContext context, WebhookDumpDb db) => {
        var record = WebhookDump.From(context);
        db.Add(record);
        await db.SaveChangesAsync();
        return record;
    } 
);
```

```text
System.InvalidOperationException: No suitable constructor was found for entity type 'JsonDocument'. The following constructors had parameters that could not be bound to properties of the entity type: 
```

## Storing Webhook Dump. Part 2: Making In-Memory Database work with JsonDocument

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<WebhookDump>().Property(p => p.Body)
        .HasConversion(
            v => v.RootElement.GetRawText(),
            v => JsonDocument.Parse(v, new())
        );
}
```

```csharp
app.MapGet("webhooks/dump", async (WebhookDumpDb db) => {
        return await db.WebhookDumps.ToArrayAsync();
    }
);
```

```http
POST /webhooks/dump

{
    "name" : "basic POST-GET one"
}

###
POST /webhooks/dump

{
    "name" : "basic POST-GET two"
}

###
GET /webhooks/dump
```

```json
[
  {
    "id": 1,
    "path": "/webhooks/dump",
    "body": {
      "name": "basic POST-GET one"
    },
    "time": "2025-04-15T14:50:09.416792Z"
  },
  {
    "id": 2,
    "path": "/webhooks/dump",
    "body": {
      "name": "basic POST-GET two"
    },
    "time": "2025-04-15T14:50:09.774897Z"
  }
]
```

## TLDR;

In this article, we've build a simple setup for webhook dumps. We could've extracted a reusable components out of our work, however, there is a `Nist.Webhooks.Dump` package that already does the heavy-lifting for us. It also make them slightly more flexible with multiple dumps support, flexible path, and bring-your-own-database design. To use it just install the package:

```sh
dotnet add package Nist.Webhooks.Dump
```

Then, just plug the required services, middleware, and mapper to your application like this:

```csharp
builder.Services.AddInMemoryWebhookDumpDb();

// ...

app.UseRequestBodyStringReader();
app.MapWebhookDump<WebhookDumpDb>();
```


After that, you can point any webhook sender to the `/webhooks/dump` path and see all the received webhooks via `GET webhooks/dump`:

```json
[
  {
    "id": 1,
    "path": "/webhooks/dump",
    "body": {
      "example": "one"
    },
    "time": "2025-04-01T13:47:00.210661Z"
  }
]
```

You can use your own database, as well. All you'd have to do is to make it implement `IDbWithWebhookDump`:

```csharp
public class YourOwnDb(DbContextOptions<YourOwnDb> options) : DbContext(options), IDbWithWebhookDump {
    public DbSet<WebhookDump> WebhookDumps { get; set;}
}
```

And register it to be used for the `webhooks/dump` endpoint:

```csharp
app.MapWebhookDump<YourOwnDb>();
```

The package, as well as this article, is part of the [NIST project](https://github.com/astorDev/nist). The project's purpose in a few words is to be a Non-Toxic REST alternative, so there's many interesting stuff beyond webhooks - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉
