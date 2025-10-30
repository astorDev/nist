using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    var typeValueDictionary = ConstEnumCollector.CollectTypeValuesDictionary(Assembly.GetExecutingAssembly());

    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        var description = context.OptionalPropertyConstEnumType();
        if (description != null &&typeValueDictionary.TryGetValue(description, out var constEnumValues))
        {
            schema.Enum = [.. constEnumValues.Select(v => new OpenApiString(v))];
        }

        // if (context.JsonPropertyInfo?.Name.Equals(nameof(Pizza.Crust), StringComparison.OrdinalIgnoreCase) == true)
        // {
        //     schema.Enum = [.. CrustTypes.All.Select(v => new OpenApiString(v))];   
        // }

        if (context.JsonPropertyInfo?.PropertyType?.IsEnum ?? false)
        {
            schema.Enum = OpenApiIntEnum.FromValues(context.JsonPropertyInfo.PropertyType);
        }

        return Task.CompletedTask;
    });
});

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => new Pizza{
    Topping = PizzaTopping.Onions,
    Cheese = PizzeCheese.Mozzarella,
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
public enum PizzeCheese
{
    Mozzarella,
    Cheddar,
    Parmesan,
    Gouda
}

public interface IConstEnum
{
    public static abstract string[] All { get; }
}

public class CrustTypes : IConstEnum
{
    public const string Thin = "Thin";
    public const string Thick = "Thick";
    public const string Stuffed = "Stuffed";

    public static string[] All => [Thin, Thick, Stuffed];
}

public class Pizza
{
    [System.ComponentModel.Description("The topping of the pizza")]
    public required PizzaTopping Topping { get; set; }
    public required PizzeCheese Cheese { get; set; }

    [System.ComponentModel.Description("The type of crust for the pizza")]
    [ConstStringEnum<CrustTypes>]
    public required string Crust { get; set; }
}

public static class EnumExtensions
{
    public static IEnumerable<IOpenApiAny> ToOpenApiAnyPair<TEnum>(this TEnum value) where TEnum : Enum
    {
        yield return new OpenApiInteger((int)(object)value);
        yield return new OpenApiString(value.ToString());
    }
}

public class ConstStringEnumAttribute<ConstEnumType> : Attribute where ConstEnumType : IConstEnum
{
    public Type Type => typeof(ConstEnumType);
}

public static class OpenApiIntEnum
{
    public static IOpenApiAny[] FromValues<TEnum>() where TEnum : struct, Enum
    {
        var allValues = Enum.GetValues<TEnum>();
        IEnumerable<IOpenApiAny> actualIntValues = allValues.Select(v => new OpenApiInteger((int)(object)v));
        var informationalStringValues = allValues.Select(v => new OpenApiString($"Hint: {(int)(object)v} -> {v}"));
        return actualIntValues.Concat(informationalStringValues).ToArray();
    }

    public static IOpenApiAny[] FromValues(Type type)
    {
        var allValues = Enum.GetValues(type).Cast<Enum>();
        IEnumerable<IOpenApiAny> actualIntValues = allValues.Select(v => new OpenApiInteger((int)(object)v));
        var informationalStringValues = allValues.Select(v => new OpenApiString($"Hint: {(int)(object)v} -> {v}"));
        return actualIntValues.Concat(informationalStringValues).ToArray();
    }
}

public static class ConstEnumCollector
{
    public static Dictionary<Type, string[]> CollectTypeValuesDictionary(Assembly assembly)
    {
        var dict = new Dictionary<Type, string[]>();
        var constEnumTypes = assembly
            .GetExportedTypes()
            .Where(t => typeof(IConstEnum).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in constEnumTypes)
        {
            var allProperty = type.GetProperty("All", BindingFlags.Public | BindingFlags.Static);
            if (allProperty != null)
            {
                var values = allProperty.GetValue(null) as string[];
                if (values != null)
                {
                    dict[type] = values;
                }
            }
        }

        return dict;
    }

    public static Type? OptionalPropertyConstEnumType(this OpenApiSchemaTransformerContext context)
    {
        var attr = context.JsonPropertyInfo?.AttributeProvider?.GetCustomAttributes(typeof(ConstStringEnumAttribute<>), true)
            .FirstOrDefault();

        if (attr == null) return null;
        
        var type = attr!.GetType().GetProperty("Type")!.GetValue(attr);
        return (Type?)type;
    }
}