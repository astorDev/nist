var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
Astor.Logging.JsonConsoleLoggerProviderExtensions.AddJsonConsole(builder.Logging);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseErrorBody(Error.Interpret);
app.UseRequestsLogging();
app.MapControllers();

app.Run();

public partial class Program {}