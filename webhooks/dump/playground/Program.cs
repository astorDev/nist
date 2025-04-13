using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInMemoryWebhookDumpDb();

var app = builder.Build();

app.UseRequestBodyStringReader();
app.MapWebhookDump<WebhookDumpDb>();

app.Run();