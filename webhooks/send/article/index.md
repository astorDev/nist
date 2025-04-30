# Webhooks in .NET 9

> Implement a Webhook Sender with .NET and PostgreSQL: A Comprehensive Guide

Webhooks are practically the only option to build an eventually consistent, event-based communication between servers without relying on any shared resouce or service. However, .NET does not provide much tooling or guidance on implementing them. This artilcle is another attempt of mine to fixing this unfrairness. In [the previous article]() I've covered webhook testing, this time we'll build a solution for a server sending webhooks. Let's get going!

> Or jump straight to the [TLDR;](#tldr) in the end of this article

## TLDR;

```csharp
builder.Services.AddPostgres<Db>();

public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookRecord<WebhookRecord> {
    public DbSet<WebhookRecord> WebhookRecords { get; set; }
}
```

```csharp
builder.Services.AddContinuousWebhookSending(sp => sp.GetRequiredService<Db>());
```

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



