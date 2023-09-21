var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddSimpleConsole(c => c.SingleLine = true).AddStateJsonConsole();

var app = builder.Build();

app.UseHttpIOLogging();

app.MapGet(Uris.About, (IHostEnvironment env) => new About(
    "Example Nist WebApi",
    "1.0.0",
    env.EnvironmentName
));

app.MapGet(Uris.RussianRouletteShot, () => {
    var shot = new Shot(Idle: new Random(DateTime.Now.Millisecond).Next(0, 6) != 1);
    return shot.Idle ? Results.Ok(shot) : Results.BadRequest(Errors.Killed);
});

app.Run();

public partial class Program {}