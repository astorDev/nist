using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // not from original
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.None).WithEndpointPrefix("none/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Alternate).WithEndpointPrefix("alt/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Default).WithEndpointPrefix("def/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Moon).WithEndpointPrefix("moon/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Purple).WithEndpointPrefix("purp/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Solarized).WithEndpointPrefix("sol/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.BluePlanet).WithEndpointPrefix("blue/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Saturn).WithEndpointPrefix("sat/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Kepler).WithEndpointPrefix("kep/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.Mars).WithEndpointPrefix("mars/{documentName}"));
    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.DeepSpace).WithEndpointPrefix("deep/{documentName}"));

    app.MapScalarApiReference(o => o
            .WithTheme(ScalarTheme.Mars)
            .WithModels(false)
            .WithSidebar(true)
            .WithEndpointPrefix("special/{documentName}")
    );
}

// app.UseHttpsRedirection(); // from original

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
//.WithName("GetWeatherForecast") // from original
;

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
