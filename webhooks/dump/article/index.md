# Webhook Testing in C#: Your Own WireMock Alternative

> Building a Simple Webhook Dump using .NET to Quickly Mock and Test a Webhook Acceptor.

Webhooks are a nice and flexible way to implement an event-based integration. However, it's relatively tricky to test webhook connections, both for a sender and for the receiver. Wouldn't it be nice to quickly set up a simple webhook dump to see all the incoming and outgoing requests? In this article, we'll build such a dump using .NET, giving you full control over webhook testing in no time - no external tools like Postman or WireMock needed.

> For the quick solution, jump straight to the end of the article, to the [TLDR; section](#tldr)

## Webhook Dump Object: Reading HttpContext

To start, let's create our project. The minimal API template will work perfectly for us:

> Normally, I also add this line `builder.Logging.AddSimpleConsole(c => c.SingleLine = true);` just to make logs look nicer.

```sh
dotnet new web
```

Now, let's create our model and make a helper method to fill the model from the `HttpContext`. We'll need a `Path`, a `Body`, and the `Time` when the request was received.

While filling `Path` and `Time` is extremely easy, reading request and response bodies from `HttpContext` as a string is surprisingly complicated. Gladly, there's a package that does all the heavy lifting. Let's install it first:

```sh
dotnet add package Nist.Bodies
```

We'll also assume the request body will be a `JsonDocument`. Despite not being flexible, this will allow us to work with JSON more fluently, while covering most of the use cases anyway. Here's the code:

> We will make an EF-friendly model from the get-go, hence the `Id` and the auto-properties look.

```csharp
using Nist;

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

Now, we can map our very basic version of the endpoint receiving webhook:

```csharp
app.MapPost("webhooks/dump", (HttpContext context) => 
    WebhookDump.From(context));
```

Let's test it with a request below:

> This article uses `httpyac` syntax for requests. I've written a [dedicated article](https://medium.com/@vosarat1995/best-postman-alternative-5890e3e9ddc7) about the tool, but you should be able to understand without any problems anyway.

```http
POST /webhooks/dump

{
    "experiment" : "alpha"
}
```

Sadly, there's still a thing we need to fix. Here's an error message we should receive:

```text
System.InvalidOperationException: Request body is not available. Make sure you've registered the required middleware with `UseRequestBodyStringReader()`
```

Let's fix it in the next section.

## Webhook Dump Object: Fixing It with UseRequestBodyStringReader

`Nist.Bodies` package contains a middlewares that save the request and response as a string in the `HttpContext`. However, those middlewares need to be registered in the pipeline. Since we are only interested in request, let's add just that:

```csharp
app.UseRequestBodyStringReader();
```

With that change in place, we should be able to get the following response from our earlier defined `POST /webhooks/dump` request:

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

Here's the full `Program.cs` just for reference:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.UseRequestBodyStringReader();

app.MapPost("webhooks/dump", (HttpContext context) => 
            WebhookDump.From(context));

app.Run();
```

We were able to extract a `WebhookDump` from the example webhook request we've received. But there's not much use of it yet, since we haven't introduced any way to see the requests we've received. Let's go ahead and complete our system in the next section.

## Storing Webhook Dump. Part 1: Introducing an In-Memory Database

The primary goal of the webhook dump is to test webhook integration and just show that everything was going as expected. Therefore, most of the time, we don't need real persistence. And since an in-memory database is easier to set up than a real one, we'll go with it. Let's start by installing the appropriate package:

> We can easily use a real database with our setup as well. More on that at the end of the article.

```sh
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

Now, we can create our database model:

```csharp
public class WebhookDumpDb(DbContextOptions<WebhookDumpDb> options) 
    : DbContext(options) 
{
    public DbSet<WebhookDump> WebhookDumps { get; set; }
}
```

Then, we'll generate an in-memory database ID, generated once per our application startup. We'll use the ID to have a single in-memory database in our app. Here's the code:

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

With the method in place, we can save our received webhooks in memory. Here's how we should update our `Program.cs`:

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

Unfortunately, if we repeat the test we've made earlier with the current setup, we will get an exception.

```text
System.InvalidOperationException: No suitable constructor was found for entity type 'JsonDocument'. The following constructors had parameters that could not be bound to properties of the entity type: 
```

Let's fix it and finish our setup in the next section.

## Storing Webhook Dump. Part 2: Making an In-Memory Database work with JsonDocument

Unfortunately, the in-memory database doesn't support `JsonDocument` natively. Unlike PostgreSQL, by the way. Gladly, we can always add an arbitrary type support using EF converters. Let's add one to our `WebhookDumpDb`:

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

Let's add a simple endpoint to read our dump, as well:

```csharp
app.MapGet("webhooks/dump", async (WebhookDumpDb db) => {
        return await db.WebhookDumps.ToArrayAsync();
    }
);
```

With those two changes in place, let's expand our test requests a little bit:

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

And here's the last response we should've received from running those tests:

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

This wraps up our little experiment. Of couse, there are quite a few improvements we can make, but we'll do something even better in the last section. Let's get straight to it!

## TLDR;

In this article, we've built a simple setup for webhook dumps. We could've extracted reusable components out of our work, however, there is a `Nist.Webhooks.Dump` package that already does the heavy-lifting for us. It also makes them slightly more flexible with multiple dumps support, flexible path, and bring-your-own-database design. To use it, just install the package:

```sh
dotnet add package Nist.Webhooks.Dump
```

Then, just plug the required services, middleware, and mapper into your application like this:

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
