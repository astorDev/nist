using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => new Pizza{
    Topping = PizzaTopping.Onions
});

app.MapPost("/", (Pizza pizza) =>
{
    return pizza;
});

app.Run();

public enum PizzaTopping
{
    Pepperoni,
    Mushrooms,
    Onions,
    Sausage
}

public class Pizza
{
    public required PizzaTopping Topping { get; init; }
}