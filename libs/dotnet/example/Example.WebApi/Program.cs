var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration["Logging:StateJsonConsole:LogLevel:Default"] = "None";
builder.Configuration["Logging:StateJsonConsole:LogLevel:Nist.Logs"] = "Information";
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
builder.Logging.AddStateJsonConsole();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpIOLogging();
app.UseErrorBody(ex => ex switch
{
    GreetingController.GovernmentNotWelcomedException _ => Errors.GovernmentNotWelcomed,
    GivingUpIsNotAllowedException => Errors.GivingUpIsNotAllowed,
    _ => Errors.Unknown
});

app.MapControllers();
app.MapGet($"/{Uris.Farewells}", (string name) => {
    if (name == "dream") throw new GivingUpIsNotAllowedException();
    
    return new {
        phrase = $"Farewell, {name}"
    };
});
app.MapPost($"/{Uris.Companies}/{{name}}/{Uris.Office}", (string name) => 
    new { 
        fullname = $"{name} HQ" 
    }
);

app.Run();

public partial class Program {}

public class GivingUpIsNotAllowedException : Exception {};