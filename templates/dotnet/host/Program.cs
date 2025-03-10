using Astor.Logging;
using Scalar.AspNetCore;
using Fluenv;
using Nist;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFluentEnvironmentVariables();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Logging.ClearProviders();
builder.Logging.AddMiniJsonConsole();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(s => s.WithTheme(ScalarTheme.DeepSpace));

app.UseHttpIOLogging(l => l.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);
app.UseProblemForExceptions(ex => ex switch {
    _ => Errors.Unknown
}, showExceptions: true);

app.MapGet($"/{Uris.About}", (IHostEnvironment env, IConfiguration configuration) => { 
    var versionFromConfig = configuration["Version"];
    
    return new About(
        Description: "Template",
        Version: !String.IsNullOrEmpty(versionFromConfig) ? versionFromConfig 
            : typeof(Program).Assembly!.GetName().Version!.ToString(),
        Environment: env.EnvironmentName
    );
});

app.Run();

public partial class Program;