using System.Net;
using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseProblemForExceptions(ex =>
{
    return ex.SearchAspNetCoreErrors() 
        ?? new Error(HttpStatusCode.InternalServerError, "Unknown");
}, showExceptions: false);

app.MapPost("/people", (Person person) =>
{
    return Results.Ok(person);
});

app.Run();

public record Person(
    string Name
);