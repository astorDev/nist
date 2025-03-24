using Astor.Logging;
using Scalar.AspNetCore;
using Fluenv;
using Nist;
using Versy;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFluentEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddMiniJsonConsole();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddVersionProvider();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(s => s.WithTheme(ScalarTheme.DeepSpace));

app.UseHttpIOLogging(l => l.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);
app.UseProblemForExceptions(ex => ex switch {
    _ => Errors.Unknown
}, showExceptions: true);

app.MapGet($"/{Uris.About}", (IHostEnvironment env, VersionProvider version) => { 
    
    return new About(
        Description: "Template",
        Version: version.Get(),
        Environment: env.EnvironmentName,
        Dependencies: new () {
            [ "foo" ] = "bar"
        }
    );
});

app.Run();

public partial class Program;