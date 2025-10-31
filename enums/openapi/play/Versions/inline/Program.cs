using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Scalar.AspNetCore;
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

    [Description("The topping of the pizza")]
    public required PizzaCheeseType Cheese { get; init; }

    [Description("The type of crust for the pizza")]
    [StringEnum<CrustTypes>]
    public required string Crust { get; init; }
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

public interface IStringEnum
{
    public static abstract string[] All { get; }
}

public class StringEnumAttribute<ConstEnumType> : Attribute where ConstEnumType : IStringEnum
{
    public Type Type => typeof(ConstEnumType);
}

public static class StringEnumCollector
{
    class StubStringEnum : IStringEnum
    {
        public static string[] All => [];
    }

    public static Dictionary<Type, string[]> GetAllLoadedStringEnumTypes()
    {
        var dict = new Dictionary<Type, string[]>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var stringEnumTypes = assemblies.SelectMany(assembly => assembly
            .GetExportedTypes()
            .Where(t => typeof(IStringEnum).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        );

        return stringEnumTypes.ToDictionary(
            type => type,
            type => GetAllFromStringEnumType(type)
        );
    }

    private static string[] GetAllFromStringEnumType(Type type)
    {
        var allProperty = type.GetProperty(nameof(IStringEnum.All), BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException($"Type `{type.FullName}` does not have a static All property.");
        return allProperty.GetValue(null) as string[] ?? throw new InvalidOperationException($"Type `{type.FullName}` All property is not of type string[].");
    }

    public static Type? OptionalPropertyStringEnumType(this OpenApiSchemaTransformerContext context)
    {
        var attr = context.JsonPropertyInfo?.AttributeProvider?.GetCustomAttributes(typeof(StringEnumAttribute<>), true)
            .FirstOrDefault();

        if (attr == null) return null;

        var type = attr!.GetType().GetProperty(nameof(StringEnumAttribute<StubStringEnum>.Type))!.GetValue(attr);
        return (Type?)type;
    }

    public static OpenApiOptions AddStringEnumSchemaTransformer(this OpenApiOptions options)
    {
        var typeValueDictionary = GetAllLoadedStringEnumTypes();

        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            var description = context.OptionalPropertyStringEnumType();
            if (description != null && typeValueDictionary.TryGetValue(description, out var constEnumValues))
            {
                schema.Enum = [.. constEnumValues.Select(v => new OpenApiString(v))];
            }

            return Task.CompletedTask;
        });

        return options;
    }
}