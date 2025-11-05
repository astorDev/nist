using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddEnumSchemaTransformer();
});

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => new Pizza{
    Topping = PizzaTopping.Onions,
    Cheese = PizzaCheeseType.Mozzarella
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

public class Pizza
{
    public required PizzaTopping Topping { get; init; }
    public required PizzaCheeseType Cheese { get; init; }
}

public static class OpenApiEnumExtensions
{
    public static OpenApiOptions AddEnumSchemaTransformer(this OpenApiOptions options, bool includeInformationalStrings = true)
    {
        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (context.JsonPropertyInfo?.PropertyType?.IsEnum != true) return Task.CompletedTask;
            if (schema.Enum?.Any() == true) return Task.CompletedTask;

            var values = Enum.GetValues(context.JsonPropertyInfo.PropertyType).Cast<Enum>();

            schema.Enum = [
                .. values.Select(v => new OpenApiInteger((int)(object)v)),
                .. includeInformationalStrings
                    ? values.Select(value => new OpenApiString($"Hint: {(int)(object)value} -> {value}"))
                    : []
            ];

            return Task.CompletedTask;
        });

        return options;
    }
}