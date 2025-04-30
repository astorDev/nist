# Webhooks in .NET 9

> Implement a Webhook Sender with .NET and PostgreSQL: A Comprehensive Guide

Webhooks are practically the only option to build an eventually consistent, event-based communication between servers without relying on any shared resouce or service. However, .NET does not provide much tooling or guidance on implementing them. This artilcle is another attempt of mine to fixing this unfrairness. In [the previous article]() I've covered webhook testing, this time we'll build a solution for a server sending webhooks. Let's get going!

> Or jump straight to the [TLDR;](#tldr) in the end of this article

## Testing Our Sender Utilizing Webhook Dump

In order to test the package we can use `webhooks/dump` endpoint, that we've discussed in [the previous webhooks article](https://medium.com/@vosarat1995/webhook-testing-in-c-your-own-wiremock-alternative-1439040931c3). After installing the `Nist.Webhooks.Dump` package we should be able to add it with those two lines of code:

```csharp
app.UseRequestBodyStringReader();
app.MapWebhookDump<Db>();
```

```csharp
await app.Services.EnsureRecreated<Db>(async db => {
    db.WebhookRecords.Add(new WebhookRecord() {
        Url = "http://localhost:5195/webhooks/dump/from-record",
        Body = JsonDocument.Parse("{\"example\": \"one\"}")
    });

    await db.SaveChangesAsync();
});
```

`dotnet run` and `GET /webhooks/dump`

```json
[
  {
    "id": 1,
    "path": "/webhooks/dump/from-record",
    "body": {
      "example": "one"
    },
    "time": "2025-04-30T14:11:49.607198Z"
  }
]
```

## TLDR;

In this article, we've built and tested a background service for sending webhooks, based on a PostgreSQL queue. Instead of recreating it from scratch you can use `Nist.Webhooks.Sender` package:

```sh
dotnet add package Nist.Webhooks.Sender
```

With the package in place, we could add continous webhook sending to our app in just a few lines of code:

> For a simple EF Core PostgreSQL setup I use `Persic.EF.Postgres` package. Check out [the dedicated article](https://medium.com/@vosarat1995/integrating-postgresql-with-net-9-using-ef-core-a-step-by-step-guide-a773768777f2) for details!

```csharp
builder.Services.AddPostgres<Db>();
builder.Services.AddContinuousWebhookSending(sp => sp.GetRequiredService<Db>());

// ...

public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookRecord<WebhookRecord> {
    public DbSet<WebhookRecord> WebhookRecords { get; set; }
}
```

You can find the code to check the setup in the previous section. You can also check a playground project [straight on the GitHub](). The playground, the package, and even this article, are parts of the [NIST project](https://github.com/astorDev/nist). The project's purpose in a few words is to be a Non-Toxic REST alternative, so there's many interesting stuff beyond webhooks - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉