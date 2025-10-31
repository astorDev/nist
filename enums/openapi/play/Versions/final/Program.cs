using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Nist;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddEnumSchemaTransformer();
    options.AddStringEnumSchemaTransformer();
});

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => new Pizza{
    Topping = PizzaTopping.Onions,
    Cheese = PizzaCheeseType.Mozzarella,
    Crust = CrustTypes.Thin
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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PizzaCheeseType
{
    Mozzarella,
    Cheddar,
    Parmesan,
    Gouda
}

public class CrustTypes : IStringEnum
{
    public const string Thin = "Thin";
    public const string Thick = "Thick";
    public const string Stuffed = "Stuffed";

    public static string[] All => [Thin, Thick, Stuffed];
}

public class Pizza
{
    [Description("The topping of the pizza")]
    public required PizzaTopping Topping { get; init; }

    [Description("The cheese of the pizza")]
    public required PizzaCheeseType Cheese { get; init; }

    [Description("The type of crust for the pizza")]
    [StringEnum<CrustTypes>]
    public required string Crust { get; init; }
}