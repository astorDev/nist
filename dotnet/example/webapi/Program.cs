DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddFluentEnvironmentVariables();

builder.Logging.ClearProviders().AddSimpleConsole(c => c.SingleLine = true).AddStateJsonConsole();
builder.Services.Configure<Shooter>(builder.Configuration.GetSection("Shooter"));

var app = builder.Build();

app.UseRequestBodyStringReader();
app.UseHttpIOLogging();
app.UseResponseBodyStringReader();

app.MapGet(Uris.About, (IHostEnvironment env) => new About(
    "Example Nist WebApi",
    "1.0.0",
    env.EnvironmentName
));

app.MapGet(Uris.RussianRouletteShot, () => {
    var gun = new Gun(Size: 6, BulletIndex: 1);
    var shot = Shoot(gun, app.Logger);
    return shot.Idle ? Results.Ok(shot) : Results.BadRequest(Errors.Killed);
});

app.MapPost(Uris.RussianRouletteShot, (Gun gun, IOptions<Shooter> shooterOptions) => {
    var shot = Shoot(gun, app.Logger, shooterOptions);
    return shot.Idle ? Results.Ok(shot) : Results.BadRequest(Errors.Killed);
});

app.Run();

static Shot Shoot(Gun gun, ILogger logger, IOptions<Shooter>? shooterOptions = null) {
    logger.LogWarning("{shooter} shooting gun with {size} bullets", shooterOptions?.Value.Name ?? Shooter.DefaultName, gun.Size);
    return new Shot(Idle: new Random(DateTime.Now.Millisecond).Next(0, gun.Size) != gun.BulletIndex);
}

public class Shooter
{
    public const string DefaultName = "Unknown shooter";
    public string Name { get; init; } = DefaultName;
}

public partial class Program;
