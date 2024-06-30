using Astor.Logging;
using Nist.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(l => l.SingleLine = true).AddMiniJsonConsole();

var app = builder.Build();

app.UseHttpIOLogging(l => l.Message = HttpIOMessagesRegistry.DefaultWithJsonBodies);

app.MapPost("/beavers", (BeaverCandidate candidate) => {
    if (candidate.Name == "Fever") return Results.BadRequest(new { Error = "Beavers cannot be named Fever" });
    var beaver = new Beaver(candidate.Name, Guid.NewGuid());
    return Results.Created($"/beavers/{beaver.Id}", beaver);
});

app.Run();

record BeaverCandidate(string Name);
record Beaver(string Name, Guid Id);