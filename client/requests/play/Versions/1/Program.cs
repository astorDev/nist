var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapGet("/", () => new {
    Message = "Hello World!"
});

app.Run();