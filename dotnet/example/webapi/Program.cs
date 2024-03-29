var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddSimpleConsole(c => c.SingleLine = true).AddStateJsonConsole();

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

app.MapPost(Uris.RussianRouletteShot, (Gun gun) => {
    var shot = Shoot(gun, app.Logger);
    return shot.Idle ? Results.Ok(shot) : Results.BadRequest(Errors.Killed);
});

app.Run();

static Shot Shoot(Gun gun, ILogger logger) {
    logger.LogWarning("Shooting gun with {size} bullets", gun.Size);
    return new Shot(Idle: new Random(DateTime.Now.Millisecond).Next(0, gun.Size) != gun.BulletIndex);
}

public partial class Program {}
