using Confi;
using Nist;
using Persic;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddFluentEnvironmentVariables();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddDbContext<Db>();
builder.Services.AddPostgresRepeatableWebhookSending<Db>(x => x.WebhookRecords);

var app = builder.Build();

await app.Services.EnsureRecreated<Db>();

app.MapPost("/enqueue/{from}", async (string from, Db db, IConfiguration configuration) =>
{
    var enqueued = db.WebhookRecords.Enqueue(
        new { message = $"Hello from {from}!" },
        "/messages",
        configuration.GetWebhookAddresses()
    );

    await db.SaveChangesAsync();
    return enqueued;
});

app.MapGetWebhooks<Db, RepeatableWebhookRecord>();
app.MapWebhookDump<Db>();

app.Run();