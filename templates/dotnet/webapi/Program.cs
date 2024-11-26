using Astor.Logging;
using Scalar.AspNetCore;
using Fluenv;
using Nist.Logs;
using Nist.Errors;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFluentEnvironmentVariables();

builder.Services.AddOpenApi();

builder.Logging.ClearProviders();
builder.Logging.AddMiniJsonConsole();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(s => s.WithTheme(ScalarTheme.DeepSpace));

app.UseHttpIOLogging(l => l.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);
app.UseErrorBody(ex => ex switch {
    _ => Errors.Unknown
});

app.MapGet($"/{Uris.About}", (IHostEnvironment env) => new About(
    Description: "Template",
    Version: typeof(Program).Assembly!.GetName().Version!.ToString(),
    Environment: env.EnvironmentName
));

app.Run();

public partial class Program;