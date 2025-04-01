# Webhook Testing in C#: Your Own WireMock Alternative

> Building a Simple Webhook Dump using .NET to Quickly Mock and Test a Webhook Acceptor.

Webhooks are nice and flexible way to implement an event-based integration. However, it's relatively tricky to test webhook connections, both for a sender and for the receiver. Wouldn't it be nice to quickly set up a simple webhook dump to see all the incoming and outgoing requests? In this article, we'll build such a dump using .NET, giving you full control over webhook testing in no time - no external tools like Postman or WireMock needed.

> For the quick solution jump straight to the end of the article, to the [TLDR; section](#tldr)

## TLDR;

We built a simple infrastructure for webhook dumps. Instead of implementing it again, you can just install the `Nist.Webhooks.Dump` package:

```sh
dotnet add package Nist.Webhooks.Dump
```

With the package installed all that is left to do is to make your `DbContext` implement  `IDbWithWebhookDump` like this:

```csharp
public class Db(DbContextOptions<Db> options) : DbContext(options), IDbWithWebhookDump
{
    public DbSet<WebhookDump> WebhookDumps { get; set; } = null!;
}
```

And to add endpoints to your application:

```csharp
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

The package as well as this article is part of the [NIST project](https://github.com/astorDev/nist). The project purpose in a few words is to be a Non-Toxic REST alternative, so there's many interesting stuff beyond webhooks - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉