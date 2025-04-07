# Webhook Testing in C#: Your Own WireMock Alternative

> Building a Simple Webhook Dump using .NET to Quickly Mock and Test a Webhook Acceptor.

Webhooks are a nice and flexible way to implement an event-based integration. However, it's relatively tricky to test webhook connections, both for a sender and for the receiver. Wouldn't it be nice to quickly set up a simple webhook dump to see all the incoming and outgoing requests? In this article, we'll build such a dump using .NET, giving you full control over webhook testing in no time - no external tools like Postman or WireMock needed.

> For the quick solution, jump straight to the end of the article, to the [TLDR; section](#tldr)

## Setting Up the Fundamentals: Web Project, EF Core, Postgres

To begin, we need a project to run our experiments. Let's use a minimal API template:

```sh
dotnet new web
```

We'll also need a database for storing our received webhooks.

```sh
dotnet add package Persic.EF.Postgres
```

```csharp
using Microsoft.EntityFrameworkCore;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddPostgres<Db>();

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<Db>();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

app.Run();

public class Db(DbContextOptions<Db> options) : DbContext(options)
{
}
```

Of course, we will also need an instance of PostgreSQL database. Here's a simple `compose.yml` that you can use `docker compose up -d`

```yaml
services:
  postgres:
    image: postgres
    environment:
      POSTGRES_DB: webhooks_playground
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - 5432:5432
```

`ConnectionStrings:Postgres` `launchSettings`

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "local": {
      // ...
      "environmentVariables": {
        // ...      
        "ConnectionStrings:Postgres" : "Host=localhost;Port=5432;Database=webhooks_playground;Username=postgres;Password=postgres"
      }
    }
  }
}
```

## Building The Core Parts: `WebhookDump` Models

To begin, we need to create a minimal API project. This will serve as the foundation for our webhook dump. Use the following command to scaffold the project:

```sh
dotnet new web
```

Next, let's define the `WebhookDump` model. This class will represent each webhook request we receive. We will assume the request body will be in JSON. We will also need to have an interface for the database

```csharp
public class WebhookDump
{
    public int Id { get; set; }
    public string Path { get; set; } = null!;
    public JsonDocument Body { get; set; } = null!;
    public DateTime Time { get; set; }
}

public interface IDbWithWebhookDump
{
    DbSet<WebhookDump> WebhookDumps { get; set; }
}
```

```sh
dotnet add package Nist.Bodies
```

> Since we are getting request body as a string we will need to enable it via `app.UseRequestBodyStringReader();`, but more on that later.

```csharp
public static class WebhookDumpEndpoints
{
    public static void MapWebhookDumpPost<TDb>(
        this IEndpointRouteBuilder app,
        string postPath = "/webhooks/dump"
    ) where TDb : DbContext, IDbWithWebhookDump
    {
        app.MapPost(postPath, async (HttpContext context, TDb db) =>
        {
            var record = new WebhookDump
            {
                Path = context.Request.Path.ToString(),
                Body = JsonDocument.Parse(context.GetRequestBodyString()),
                Time = DateTime.UtcNow
            };

            db.WebhookDumps.Add(record);
            await db.SaveChangesAsync();
            return record;
        });
    }

    public static void MapWebhookDumpGet<TDb>(
        this IEndpointRouteBuilder app,
        string getPath = "/webhooks/dump"
    ) where TDb : DbContext, IDbWithWebhookDump
    {
        app.MapGet(getPath, async (TDb db) =>
        {
            return await db.WebhookDumps.ToArrayAsync();
        });
    }
}
```

## Setting Up Our Database: Deploying and Connecting to Postgres

> You can skip the section 

`compose.yml`

```yaml
services:
  postgres:
    image: postgres
    environment:
      POSTGRES_DB: webhooks_playground
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - 5432:5432
```

`ConnectionStrings:Postgres` `launchSettings`

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "local": {
      // ...
      "environmentVariables": {
        // ...      
        "ConnectionStrings:Postgres" : "Host=localhost;Port=5432;Database=webhooks_playground;Username=postgres;Password=postgres"
      }
    }
  }
}
```

```sh
dotnet add package Persic.EF.Postgres
```

```csharp
builder.Services.AddPostgres<Db>();
```

```csharp
await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<Db>();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
```

## Assembling It All Together

```csharp
app.MapWebhookDumpPost<Db>();
app.MapWebhookDumpGet<Db>();
```

```text
Request body is not available. Make sure you've registered the required middleware with `UseRequestBodyStringReader()`
```

```csharp
app.UseRequestBodyStringReader();

app.MapWebhookDumpPost<Db>();
app.MapWebhookDumpGet<Db>();
```

> I use httpyac for http testing, you can read a dedicated article about it [here](https://medium.com/@vosarat1995/best-postman-alternative-5890e3e9ddc7), but the code should be intuitive. You should also use your own generated port, which you can find in the `Now listening on: http://localhost:5209` logs.

```http
POST http://localhost:5209/webhooks/dump

{
    "example" : "one"
}
```

```json
[
  {
    "id": 1,
    "path": "/webhooks/dump",
    "body": {
      "example": "one"
    },
    "time": "2025-04-02T13:40:30.169178Z"
  }
]
```

## Making It More Fluent

```csharp
public static class WebhookDumpEndpoints
{
    private static readonly HashSet<string> registeredPostPaths = [];
    private static readonly HashSet<string> registeredGetPaths = [];

    public static void MapWebhookDump<TDb>(
        this IEndpointRouteBuilder app,
        string postPath = "/webhooks/dump",
        string getPath = "/webhooks/dump"
    ) where TDb : DbContext, IDbWithWebhookDump
    {
        if (!registeredPostPaths.Contains(postPath))
        {
            app.MapWebhookDumpPost<TDb>(postPath);
            registeredPostPaths.Add(postPath);
        }

        if (!registeredGetPaths.Contains(getPath))
        {
            app.MapWebhookDumpGet<TDb>(getPath);
            registeredGetPaths.Add(getPath);
        }
    }
}
```

```http
POST http://localhost:5209/webhooks/dump2

{
    "example" : "two"
}
```

```json
[
  {
    "id": 1,
    "path": "/webhooks/dump",
    "body": {
      "example": "one"
    },
    "time": "2025-04-02T13:52:36.641974Z"
  },
  {
    "id": 2,
    "path": "/webhooks/dump2",
    "body": {
      "example": "two"
    },
    "time": "2025-04-02T13:52:43.420825Z"
  }
]
```

## TLDR;

We built a simple infrastructure for webhook dumps. Instead of implementing it again, you can just install the `Nist.Webhooks.Dump` package:

```sh
dotnet add package Nist.Webhooks.Dump
```

With the package installed, all that is left to do is to make your `DbContext` implement  `IDbWithWebhookDump` like this:

```csharp
public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookDump
{
    public DbSet<WebhookDump> WebhookDumps { get; set; } = null!;
}
```

And to add endpoints to your application:

```csharp
app.UseRequestBodyStringReader();

app.MapWebhookDump<Db>();
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

The package, as well as this article, is part of the [NIST project](https://github.com/astorDev/nist). The project's purpose in a few words is to be a Non-Toxic REST alternative, so there's many interesting stuff beyond webhooks - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉
